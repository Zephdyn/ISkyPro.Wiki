from __future__ import annotations

import json
import sys
import threading
from concurrent.futures import ThreadPoolExecutor
from dataclasses import dataclass
from typing import Any, Callable, Dict, Optional

from .generated_methods import GeneratedSdkMethodsMixin, SDK_METHODS, SDK_VERSION


JSON_RPC_VERSION = "2.0"
PROTOCOL_VERSION = 2
DEFAULT_REQUEST_TIMEOUT_SECONDS = 30.0
MAX_HEADER_LENGTH = 8 * 1024
MAX_PAYLOAD_LENGTH = 4 * 1024 * 1024

JsonObject = Dict[str, Any]
EventHandler = Callable[[JsonObject, "PluginContext"], Optional[JsonObject]]


class JsonRpcError(RuntimeError):
    def __init__(self, code: int, message: str) -> None:
        super().__init__(message)
        self.code = code


def _positive_int(value: Any, fallback: int) -> int:
    try:
        parsed = int(value)
    except (TypeError, ValueError, OverflowError):
        return fallback
    return parsed if parsed > 0 else fallback


def _validate_message(message: Any) -> JsonObject:
    if not isinstance(message, dict):
        raise RuntimeError("JSON-RPC payload must be an object")
    if message.get("jsonrpc") != JSON_RPC_VERSION:
        raise RuntimeError("JSON-RPC payload must declare jsonrpc 2.0")

    has_method = isinstance(message.get("method"), str) and bool(message.get("method", "").strip())
    has_result = "result" in message
    has_error = "error" in message
    if has_method:
        return message
    if has_result == has_error or "id" not in message:
        raise RuntimeError("JSON-RPC response must contain id and exactly one of result or error")
    return message


def _read_exact(input_stream, length: int) -> bytes:
    payload = bytearray()
    while len(payload) < length:
        chunk = input_stream.read(length - len(payload))
        if not chunk:
            raise EOFError("stdio-jsonrpc stream ended in the middle of a frame")
        payload.extend(chunk)
    return bytes(payload)


def read_frame(input_stream=None) -> Optional[JsonObject]:
    input_stream = input_stream or sys.stdin.buffer
    header = bytearray()
    while not header.endswith(b"\r\n\r\n"):
        chunk = input_stream.read(1)
        if not chunk:
            if not header:
                return None
            raise EOFError("stdio-jsonrpc stream ended in the middle of a header")
        header.extend(chunk)
        if len(header) > MAX_HEADER_LENGTH:
            raise RuntimeError(f"stdio-jsonrpc header exceeds {MAX_HEADER_LENGTH} bytes")

    content_length = None
    for line in bytes(header[:-4]).decode("ascii").split("\r\n"):
        if not line:
            continue
        if ":" not in line:
            raise RuntimeError("stdio-jsonrpc header is invalid")
        name, value = line.split(":", 1)
        if name.lower() != "content-length":
            raise RuntimeError("stdio-jsonrpc only supports Content-Length headers")
        if content_length is not None:
            raise RuntimeError("stdio-jsonrpc Content-Length header must appear exactly once")
        content_length = int(value.strip())

    if content_length is None or content_length <= 0:
        raise RuntimeError("stdio-jsonrpc frame is missing Content-Length")
    if content_length > MAX_PAYLOAD_LENGTH:
        raise RuntimeError(f"stdio-jsonrpc payload exceeds {MAX_PAYLOAD_LENGTH} bytes")

    message = json.loads(_read_exact(input_stream, content_length).decode("utf-8"))
    return _validate_message(message)


def write_frame(message: JsonObject, output_stream=None) -> None:
    output_stream = output_stream or sys.stdout.buffer
    _validate_message(message)
    payload = json.dumps(message, ensure_ascii=False, separators=(",", ":")).encode("utf-8")
    if not payload or len(payload) > MAX_PAYLOAD_LENGTH:
        raise RuntimeError(f"invalid stdio-jsonrpc payload length: {len(payload)}")
    output_stream.write(f"Content-Length: {len(payload)}\r\n\r\n".encode("ascii"))
    output_stream.write(payload)
    output_stream.flush()


@dataclass
class _PendingRequest:
    completed: threading.Event
    result: Any = None
    error: Optional[BaseException] = None


class PluginContext(GeneratedSdkMethodsMixin):
    def __init__(
        self,
        plugin_id: str,
        token: str,
        *,
        request_sender: Callable[[str, JsonObject], Any],
    ) -> None:
        if request_sender is None:
            raise ValueError("PluginContext requires a multiplexed request sender")
        self.plugin_id = plugin_id
        self.token = token
        self._request_sender = request_sender

    def invoke(self, method: str, parameters: Optional[JsonObject] = None) -> Any:
        if not method or not method.strip():
            raise ValueError("SDK method is required")
        request_parameters = dict(parameters or {})
        request_parameters["token"] = self.token
        return self._request_sender(method, request_parameters)

    def log_write(self, message: str, level: str = "Information") -> Any:
        return self.invoke("log.write", {"level": level or "Information", "message": message})

    def reply_text(self, message_reference: JsonObject, content: str) -> Any:
        return self.invoke(
            "messages.replyText",
            {
                "messageReference": message_reference,
                "content": content,
            },
        )


class StdioJsonRpcPlugin:
    def __init__(
        self,
        plugin_id: str,
        sdk_name: str = "iskypro-python-sdk-v2",
        sdk_version: str = SDK_VERSION,
        *,
        max_concurrent_events: Optional[int] = None,
        queue_capacity: Optional[int] = None,
        request_timeout_seconds: float = DEFAULT_REQUEST_TIMEOUT_SECONDS,
        input_stream=None,
        output_stream=None,
    ) -> None:
        self.plugin_id = plugin_id
        self.sdk_name = sdk_name
        self.sdk_version = sdk_version
        self.token = ""
        self._max_concurrent_events = max_concurrent_events
        self._queue_capacity = queue_capacity
        self._request_timeout_seconds = max(0.1, min(float(request_timeout_seconds), 300.0))
        self._input = input_stream or sys.stdin.buffer
        self._output = output_stream or sys.stdout.buffer
        self._write_gate = threading.Lock()
        self._pending_gate = threading.Lock()
        self._pending: Dict[str, _PendingRequest] = {}
        self._next_request_id = 1000
        self._executor: Optional[ThreadPoolExecutor] = None
        self._event_slots: Optional[threading.BoundedSemaphore] = None
        self._active_condition = threading.Condition()
        self._active_events = 0
        self._initialized = False
        self._stopping = False
        self._finished = threading.Event()
        self._fatal_error: Optional[BaseException] = None
        self._on_event: Optional[EventHandler] = None

    def run(self, on_event: EventHandler) -> None:
        if on_event is None:
            raise ValueError("event handler is required")
        self._on_event = on_event
        reader = threading.Thread(
            target=self._read_loop,
            name=f"iskypro-{self.plugin_id}-reader",
            daemon=True,
        )
        reader.start()
        self._finished.wait()
        self._fail_pending(RuntimeError("ISkyPro stdio-jsonrpc connection closed"))
        if self._executor is not None:
            self._executor.shutdown(wait=True, cancel_futures=True)
        if self._fatal_error is not None:
            raise self._fatal_error

    def _read_loop(self) -> None:
        try:
            while not self._finished.is_set():
                message = read_frame(self._input)
                if message is None:
                    self._finished.set()
                    return
                if "method" in message:
                    self._handle_request(message)
                else:
                    self._handle_response(message)
        except BaseException as exc:
            if not self._finished.is_set():
                self._fatal_error = exc
                self._finished.set()

    def _handle_request(self, request: JsonObject) -> None:
        method = request.get("method")
        request_id = request.get("id")
        parameters = request.get("params") or {}
        if not isinstance(parameters, dict):
            self._write_error(request_id, -32602, "JSON-RPC params must be an object")
            return

        if method == "iskypro.initialize":
            self._handle_initialize(request_id, parameters)
            return
        if method in ("plugin.stop", "shutdown"):
            self._handle_stop(request_id)
            return
        if method == "events.message":
            self._schedule_event(request_id, parameters)
            return
        if request_id is not None:
            self._write_error(request_id, -32601, f"unknown method: {method}")

    def _handle_initialize(self, request_id: Any, parameters: JsonObject) -> None:
        if request_id is None:
            raise RuntimeError("iskypro.initialize must be a JSON-RPC request")
        if self._initialized:
            self._write_error(request_id, -32000, "Plugin SDK v2 is already initialized")
            return
        supported = parameters.get("supportedProtocolVersions") or []
        if PROTOCOL_VERSION not in supported:
            self._write_error(request_id, -32602, "Main does not support protocol version 2")
            return
        requested_plugin_id = str(parameters.get("pluginId") or "")
        if requested_plugin_id != self.plugin_id:
            self._write_error(request_id, -32602, "initialize pluginId does not match the plugin")
            return
        if str(parameters.get("encoding") or "").lower() != "json":
            self._write_error(request_id, -32602, "Plugin SDK v2 only supports json encoding")
            return
        self.token = str(parameters.get("token") or parameters.get("runtimeToken") or "")
        if not self.token:
            self._write_error(request_id, -32602, "initialize token is required")
            return

        runtime = parameters.get("runtime") or {}
        main_concurrency = _positive_int(runtime.get("maxConcurrentEvents"), 1)
        local_concurrency = _positive_int(self._max_concurrent_events, main_concurrency)
        effective_concurrency = min(main_concurrency, local_concurrency)
        main_queue_capacity = _positive_int(runtime.get("queueCapacity"), 64)
        local_queue_capacity = _positive_int(self._queue_capacity, main_queue_capacity)
        effective_queue_capacity = min(main_queue_capacity, local_queue_capacity)
        timeout_ms = _positive_int(runtime.get("requestTimeoutMilliseconds"), 0)
        if timeout_ms > 0:
            self._request_timeout_seconds = max(0.1, min(timeout_ms / 1000.0, 300.0))
        self._executor = ThreadPoolExecutor(
            max_workers=effective_concurrency,
            thread_name_prefix=f"iskypro-{self.plugin_id}-event",
        )
        self._event_slots = threading.BoundedSemaphore(
            effective_concurrency + effective_queue_capacity
        )
        self._initialized = True
        self._write_success(
            request_id,
            {
                "protocolVersion": PROTOCOL_VERSION,
                "pluginId": self.plugin_id,
                "sdkName": self.sdk_name,
                "sdkVersion": self.sdk_version,
                "capabilities": [
                    "events.message",
                    "log.write",
                    "messages.replyText",
                    "bidirectional-requests",
                    "concurrent-events",
                    "graceful-shutdown",
                ],
                "encoding": "json",
            },
        )

    def _schedule_event(self, request_id: Any, event: JsonObject) -> None:
        if not self._initialized or self._executor is None or self._event_slots is None:
            self._write_error(request_id, -32600, "Plugin must be initialized before events")
            return
        if self._stopping:
            self._write_error(request_id, -32000, "Plugin is stopping")
            return
        if not self._event_slots.acquire(blocking=False):
            if request_id is not None:
                self._write_success(
                    request_id,
                    {
                        "accepted": False,
                        "eventId": str(event.get("eventId") or ""),
                        "error": "Plugin SDK v2 local event queue is full.",
                    },
                )
            return
        with self._active_condition:
            self._active_events += 1
        try:
            self._executor.submit(self._handle_event, request_id, event)
        except BaseException:
            with self._active_condition:
                self._active_events -= 1
                self._active_condition.notify_all()
            self._event_slots.release()
            raise

    def _handle_event(self, request_id: Any, event: JsonObject) -> None:
        try:
            context = PluginContext(
                self.plugin_id,
                self.token,
                request_sender=self._send_request,
            )
            try:
                result = (self._on_event or (lambda _event, _context: {}))(event, context) or {}
                if not isinstance(result, dict):
                    raise TypeError("event handler result must be an object")
                response = {
                    "accepted": bool(result.get("accepted", True)),
                    "eventId": str(event.get("eventId") or ""),
                    "error": result.get("error"),
                }
            except BaseException as exc:
                response = {
                    "accepted": False,
                    "eventId": str(event.get("eventId") or ""),
                    "error": str(exc),
                }
            if request_id is not None:
                self._write_success(request_id, response)
        finally:
            with self._active_condition:
                self._active_events -= 1
                self._active_condition.notify_all()
            if self._event_slots is not None:
                self._event_slots.release()

    def _handle_stop(self, request_id: Any) -> None:
        if self._stopping:
            return
        self._stopping = True

        def complete_stop() -> None:
            with self._active_condition:
                while self._active_events > 0:
                    self._active_condition.wait(timeout=0.1)
            if request_id is not None:
                self._write_success(request_id, {"accepted": True})
            self._finished.set()

        threading.Thread(target=complete_stop, name="iskypro-stop", daemon=True).start()

    def _send_request(self, method: str, parameters: JsonObject) -> Any:
        with self._pending_gate:
            self._next_request_id += 1
            request_id = self._next_request_id
            pending = _PendingRequest(threading.Event())
            self._pending[str(request_id)] = pending
        try:
            self._write(
                {
                    "jsonrpc": JSON_RPC_VERSION,
                    "id": request_id,
                    "method": method,
                    "params": parameters,
                }
            )
            if not pending.completed.wait(self._request_timeout_seconds):
                raise TimeoutError(
                    f"SDK request '{method}' timed out after "
                    f"{self._request_timeout_seconds * 1000:.0f} ms"
                )
            if pending.error is not None:
                raise pending.error
            return pending.result
        finally:
            with self._pending_gate:
                self._pending.pop(str(request_id), None)

    def _handle_response(self, response: JsonObject) -> None:
        request_id = str(response.get("id"))
        with self._pending_gate:
            pending = self._pending.get(request_id)
        if pending is None:
            return
        if "error" in response:
            error = response["error"]
            code = int(error.get("code", -32000)) if isinstance(error, dict) else -32000
            message = error.get("message") if isinstance(error, dict) else str(error)
            pending.error = JsonRpcError(code, str(message))
        else:
            pending.result = response.get("result")
        pending.completed.set()

    def _write_success(self, request_id: Any, result: Any) -> None:
        self._write({"jsonrpc": JSON_RPC_VERSION, "id": request_id, "result": result})

    def _write_error(self, request_id: Any, code: int, message: str) -> None:
        if request_id is None:
            return
        self._write(
            {
                "jsonrpc": JSON_RPC_VERSION,
                "id": request_id,
                "error": {"code": code, "message": message},
            }
        )

    def _write(self, message: JsonObject) -> None:
        with self._write_gate:
            write_frame(message, self._output)

    def _fail_pending(self, error: BaseException) -> None:
        with self._pending_gate:
            pending_requests = list(self._pending.values())
        for pending in pending_requests:
            pending.error = error
            pending.completed.set()


__all__ = [
    "DEFAULT_REQUEST_TIMEOUT_SECONDS",
    "GeneratedSdkMethodsMixin",
    "JSON_RPC_VERSION",
    "JsonRpcError",
    "MAX_HEADER_LENGTH",
    "MAX_PAYLOAD_LENGTH",
    "PluginContext",
    "SDK_METHODS",
    "SDK_VERSION",
    "StdioJsonRpcPlugin",
    "read_frame",
    "write_frame",
]
