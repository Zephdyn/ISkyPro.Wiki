from __future__ import annotations

import json
import sys
import threading
from concurrent.futures import ThreadPoolExecutor
from dataclasses import dataclass
from typing import Any, Callable, Dict, Iterable, Mapping, Optional, Tuple, Union

from .generated_methods import GeneratedSdkMethodsMixin, SDK_METHODS, SDK_VERSION


JSON_RPC_VERSION = "2.0"
PROTOCOL_VERSION = 2
DEFAULT_REQUEST_TIMEOUT_SECONDS = 30.0
MAX_HEADER_LENGTH = 8 * 1024
MAX_PAYLOAD_LENGTH = 4 * 1024 * 1024

JsonObject = Dict[str, Any]
EventHandler = Callable[["MessageContext", "PluginContext"], Optional[JsonObject]]


class JsonRpcError(RuntimeError):
    def __init__(self, code: int, message: str, data: Any = None) -> None:
        super().__init__(message)
        self.code = code
        self.data = data


@dataclass(frozen=True)
class UserRef:
    provider: str
    mention_id: str
    user_open_id: Optional[str] = None
    member_open_id: Optional[str] = None
    union_open_id: Optional[str] = None
    display_name: Optional[str] = None

    @classmethod
    def from_dict(cls, value: Mapping[str, Any]) -> "UserRef":
        return cls(
            provider=str(value.get("provider") or ""),
            mention_id=str(value.get("mentionId") or ""),
            user_open_id=_optional_string(value.get("userOpenId")),
            member_open_id=_optional_string(value.get("memberOpenId")),
            union_open_id=_optional_string(value.get("unionOpenId")),
            display_name=_optional_string(value.get("displayName")),
        )


@dataclass(frozen=True)
class _MessagePart:
    payload: JsonObject


@dataclass(frozen=True)
class _CompositePart:
    parts: Tuple[Any, ...]


class _At:
    everyone = _MessagePart({"type": "mention", "target": "everyone"})

    @staticmethod
    def user(
        user: Union[str, UserRef, Mapping[str, Any]],
        qqbot_format: Optional[str] = None,
    ) -> _MessagePart:
        if isinstance(user, UserRef):
            mention_id = user.mention_id
        elif isinstance(user, Mapping):
            mention_id = str(user.get("mentionId") or "")
        else:
            mention_id = str(user or "")
        _validate_id(mention_id, "mention id")
        _validate_qqbot_mention_format(mention_id, qqbot_format)
        payload = {"type": "mention", "target": "user", "id": mention_id}
        if qqbot_format is not None:
            payload["qqBotFormat"] = qqbot_format
        return _MessagePart(payload)

    @classmethod
    def users(
        cls,
        users: Iterable[Union[str, UserRef, Mapping[str, Any]]],
        separator: str = " ",
        qqbot_format: Optional[str] = None,
    ) -> _CompositePart:
        values = list(users)
        if not values:
            raise ValueError("at.users requires at least one user")
        if len(values) > 20:
            raise ValueError("at.users supports at most 20 users")
        parts = []
        for index, user in enumerate(values):
            if index > 0 and separator:
                parts.append(separator)
            parts.append(cls.user(user, qqbot_format))
        return _CompositePart(tuple(parts))


at = _At()


def image(file_path: str) -> _MessagePart:
    """Create a local image part (uploaded and sent as rich media by Main).

    ``file_path`` is the absolute path of the image file on the machine that
    runs ISkyPro. The message can contain at most one image part and cannot be
    combined with markdown format.
    """
    _validate_id(file_path, "image file path")
    return _MessagePart({"type": "image", "filePath": file_path})


def _optional_string(value: Any) -> Optional[str]:
    return str(value) if value is not None else None


def _validate_id(value: str, label: str) -> None:
    if not value.strip() or any(ord(character) < 32 or ord(character) == 127 for character in value):
        raise ValueError(f"{label} must not be empty or contain control characters")


def _validate_qqbot_mention_format(mention_id: str, value: Optional[str]) -> None:
    if value not in (None, "current", "legacy", "legacy-bang"):
        raise ValueError("qqbot mention format must be current, legacy, or legacy-bang")
    if value in ("legacy", "legacy-bang") and any(character in mention_id for character in "<>"):
        raise ValueError("legacy QQBot mention ids must not contain '<' or '>'")


def _normalize_parts(parts: Iterable[Any], message_format: str = "text") -> JsonObject:
    if message_format not in ("text", "markdown"):
        raise ValueError("message format must be text or markdown")

    flattened = []

    def append(part: Any) -> None:
        if isinstance(part, str):
            flattened.append({"type": "text", "text": part})
            return
        if isinstance(part, _MessagePart):
            flattened.append(dict(part.payload))
            return
        if isinstance(part, _CompositePart):
            for child in part.parts:
                append(child)
            return
        raise TypeError("message parts must be strings or SDK message parts")

    for item in parts:
        append(item)

    normalized = []
    for part in flattened:
        if part.get("type") == "text" and part.get("text") == "":
            continue
        if (
            part.get("type") == "text"
            and normalized
            and normalized[-1].get("type") == "text"
        ):
            normalized[-1]["text"] += str(part.get("text") or "")
        else:
            normalized.append(part)
    if not normalized:
        raise ValueError("message must contain at least one non-empty part")
    message = {"parts": normalized}
    if message_format != "text":
        message["format"] = message_format
    return message


class MessageContext:
    def __init__(self, event: JsonObject, context: "PluginContext") -> None:
        self.event_id = str(event.get("eventId") or "")
        self.event_type = str(event.get("eventType") or "")
        self.timestamp = event.get("timestamp")
        self.source = str(event.get("source") or "")
        self.bot = dict(event.get("bot") or {})
        self.conversation = dict(event.get("conversation") or {})
        sender = event.get("sender") or {}
        self.sender = UserRef.from_dict(sender if isinstance(sender, Mapping) else {})
        message = event.get("message") or {}
        self.id = str(message.get("id") or "")
        self.text = str(message.get("content") or "")
        self.attachments = tuple(message.get("attachments") or ())
        self.mentions = tuple(
            UserRef.from_dict(item)
            for item in (message.get("mentions") or ())
            if isinstance(item, Mapping)
        )
        self.raw_payload = event.get("rawPayload") or {}
        self._reference = dict(event.get("messageReference") or {})
        self._context = context

    def reply(self, *parts: Any) -> Any:
        return self._context.invoke(
            "messages.reply",
            {"reference": self._reference, "message": _normalize_parts(parts)},
        )

    def reply_markdown(self, *parts: Any) -> Any:
        return self._context.invoke(
            "messages.reply",
            {
                "reference": self._reference,
                "message": _normalize_parts(parts, "markdown"),
            },
        )


class MessageTarget:
    def __init__(self, context: "PluginContext", target_type: str, target_id: str) -> None:
        _validate_id(target_id, "message target id")
        self._context = context
        self._target = {"type": target_type, "id": target_id}

    def send(self, *parts: Any) -> Any:
        return self._context.invoke(
            "messages.send",
            {"target": self._target, "message": _normalize_parts(parts)},
        )

    def send_markdown(self, *parts: Any) -> Any:
        return self._context.invoke(
            "messages.send",
            {
                "target": self._target,
                "message": _normalize_parts(parts, "markdown"),
            },
        )


class MessageService:
    def __init__(self, context: "PluginContext") -> None:
        self._context = context

    def group(self, group_open_id: str) -> MessageTarget:
        return MessageTarget(self._context, "group", group_open_id)

    def channel(self, channel_id: str) -> MessageTarget:
        return MessageTarget(self._context, "channel", channel_id)

    def user(self, user_open_id: str) -> MessageTarget:
        return MessageTarget(self._context, "user", user_open_id)

    def direct_message(self, guild_id: str) -> MessageTarget:
        return MessageTarget(self._context, "direct", guild_id)


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
        self.messages = MessageService(self)

    def invoke(self, method: str, parameters: Optional[JsonObject] = None) -> Any:
        if not method or not method.strip():
            raise ValueError("SDK method is required")
        request_parameters = dict(parameters or {})
        request_parameters["token"] = self.token
        return self._request_sender(method, request_parameters)

    def log_write(self, message: str, level: str = "Information") -> Any:
        return self.invoke("log.write", {"level": level or "Information", "message": message})

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
                    "messages.reply",
                    "messages.send",
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
            message = MessageContext(event, context)
            try:
                result = (self._on_event or (lambda _message, _context: {}))(message, context) or {}
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
            data = error.get("data") if isinstance(error, dict) else None
            pending.error = JsonRpcError(code, str(message), data)
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
    "MessageContext",
    "MessageService",
    "MessageTarget",
    "PluginContext",
    "SDK_METHODS",
    "SDK_VERSION",
    "StdioJsonRpcPlugin",
    "UserRef",
    "at",
    "image",
    "read_frame",
    "write_frame",
]
