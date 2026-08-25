import fs from "node:fs";
import process from "node:process";
import {
  GeneratedSdkMethods,
  sdkMethods,
  sdkVersion as currentSdkVersion,
} from "./generated-methods.js";

export { currentSdkVersion as sdkVersion, sdkMethods };

export const jsonRpcVersion = "2.0";
export const protocolVersion = 2;
export const defaultRequestTimeoutMs = 30_000;
export const maxHeaderLength = 8 * 1024;
export const maxPayloadLength = 4 * 1024 * 1024;

function positiveInteger(value, fallback) {
  const parsed = Number(value);
  return Number.isSafeInteger(parsed) && parsed > 0 ? parsed : fallback;
}

function normalizeRequestTimeout(value, fallback = defaultRequestTimeoutMs) {
  const parsed = Number(value);
  return Number.isFinite(parsed) && parsed > 0
    ? Math.max(100, Math.min(parsed, 300_000))
    : fallback;
}

export class JsonRpcError extends Error {
  constructor(code, message, data = undefined) {
    super(message);
    this.name = "JsonRpcError";
    this.code = code;
    this.data = data;
  }
}

function validateMessage(message) {
  if (message === null || typeof message !== "object" || Array.isArray(message)) {
    throw new Error("JSON-RPC payload must be an object");
  }
  if (message.jsonrpc !== jsonRpcVersion) {
    throw new Error("JSON-RPC payload must declare jsonrpc 2.0");
  }

  const hasMethod = typeof message.method === "string" && message.method.trim().length > 0;
  const hasResult = Object.hasOwn(message, "result");
  const hasError = Object.hasOwn(message, "error");
  if (!hasMethod && (hasResult === hasError || !Object.hasOwn(message, "id"))) {
    throw new Error("JSON-RPC response must contain id and exactly one of result or error");
  }
  return message;
}

function parseContentLength(header) {
  const headerText = header.toString("ascii");
  let contentLength = null;
  for (const line of headerText.split("\r\n")) {
    if (line.length === 0) {
      continue;
    }

    const separator = line.indexOf(":");
    const name = separator >= 0 ? line.slice(0, separator).toLowerCase() : "";
    if (name !== "content-length") {
      throw new Error("stdio-jsonrpc only supports Content-Length headers");
    }
    if (contentLength !== null) {
      throw new Error("stdio-jsonrpc Content-Length header must appear exactly once");
    }

    contentLength = Number.parseInt(line.slice(separator + 1).trim(), 10);
  }

  if (!Number.isInteger(contentLength) || contentLength <= 0) {
    throw new Error("stdio-jsonrpc frame is missing Content-Length");
  }
  if (contentLength > maxPayloadLength) {
    throw new Error(`stdio-jsonrpc payload exceeds ${maxPayloadLength} bytes`);
  }
  return contentLength;
}

export function readFrame(fd = 0) {
  const header = [];
  const byte = Buffer.alloc(1);
  while (true) {
    const read = fs.readSync(fd, byte, 0, 1, null);
    if (read === 0) {
      if (header.length === 0) {
        return null;
      }
      throw new Error("stdio-jsonrpc stream ended in the middle of a header");
    }

    header.push(byte[0]);
    if (header.length > maxHeaderLength) {
      throw new Error(`stdio-jsonrpc header exceeds ${maxHeaderLength} bytes`);
    }
    const length = header.length;
    if (
      length >= 4 &&
      header[length - 4] === 13 &&
      header[length - 3] === 10 &&
      header[length - 2] === 13 &&
      header[length - 1] === 10
    ) {
      break;
    }
  }

  const contentLength = parseContentLength(Buffer.from(header.slice(0, -4)));
  const payload = Buffer.alloc(contentLength);
  let offset = 0;
  while (offset < contentLength) {
    const read = fs.readSync(fd, payload, offset, contentLength - offset, null);
    if (read === 0) {
      throw new Error("stdio-jsonrpc stream ended in the middle of a frame");
    }
    offset += read;
  }

  return validateMessage(JSON.parse(payload.toString("utf8")));
}

export async function* readFrames(stream = process.stdin) {
  let buffer = Buffer.alloc(0);
  for await (const chunk of stream) {
    buffer = Buffer.concat([buffer, Buffer.isBuffer(chunk) ? chunk : Buffer.from(chunk)]);
    while (buffer.length > 0) {
      const headerEnd = buffer.indexOf("\r\n\r\n");
      if (headerEnd < 0) {
        if (buffer.length > maxHeaderLength) {
          throw new Error(`stdio-jsonrpc header exceeds ${maxHeaderLength} bytes`);
        }
        break;
      }
      if (headerEnd > maxHeaderLength - 4) {
        throw new Error(`stdio-jsonrpc header exceeds ${maxHeaderLength} bytes`);
      }

      const contentLength = parseContentLength(buffer.subarray(0, headerEnd));
      const frameLength = headerEnd + 4 + contentLength;
      if (buffer.length < frameLength) {
        break;
      }

      const payload = buffer.subarray(headerEnd + 4, frameLength);
      buffer = buffer.subarray(frameLength);
      yield validateMessage(JSON.parse(payload.toString("utf8")));
    }
  }

  if (buffer.length !== 0) {
    throw new Error("stdio-jsonrpc stream ended in the middle of a frame");
  }
}

export function writeFrame(message, fd = 1) {
  validateMessage(message);
  const payload = Buffer.from(JSON.stringify(message), "utf8");
  if (payload.length <= 0 || payload.length > maxPayloadLength) {
    throw new Error(`invalid stdio-jsonrpc payload length: ${payload.length}`);
  }
  fs.writeSync(fd, Buffer.from(`Content-Length: ${payload.length}\r\n\r\n`, "ascii"));
  fs.writeSync(fd, payload);
}

function validateId(value, label) {
  if (
    typeof value !== "string" ||
    value.trim().length === 0 ||
    [...value].some((character) => character.charCodeAt(0) < 32 || character.charCodeAt(0) === 127)
  ) {
    throw new TypeError(`${label} must not be empty or contain control characters`);
  }
}

function messagePart(payload) {
  return Object.freeze({ __iskyproMessagePart: true, payload: Object.freeze(payload) });
}

function compositePart(parts) {
  return Object.freeze({ __iskyproCompositePart: true, parts: Object.freeze(parts) });
}

export const at = Object.freeze({
  user(user, qqBotFormat = undefined) {
    const id = typeof user === "string" ? user : user?.mentionId ?? "";
    validateId(id, "mention id");
    if (![undefined, "current", "legacy", "legacy-bang"].includes(qqBotFormat)) {
      throw new TypeError("QQBot mention format must be current, legacy, or legacy-bang");
    }
    if (["legacy", "legacy-bang"].includes(qqBotFormat) && /[<>]/u.test(id)) {
      throw new TypeError("legacy QQBot mention ids must not contain '<' or '>'");
    }
    return messagePart({
      type: "mention",
      target: "user",
      id,
      ...(qqBotFormat === undefined ? {} : { qqBotFormat }),
    });
  },
  users(users, separator = " ", qqBotFormat = undefined) {
    const values = [...users];
    if (values.length === 0) {
      throw new TypeError("at.users requires at least one user");
    }
    if (values.length > 20) {
      throw new TypeError("at.users supports at most 20 users");
    }
    const parts = [];
    for (const [index, user] of values.entries()) {
      if (index > 0 && separator.length > 0) {
        parts.push(separator);
      }
      parts.push(at.user(user, qqBotFormat));
    }
    return compositePart(parts);
  },
  everyone: messagePart({ type: "mention", target: "everyone" }),
});

export function image(filePath) {
  validateId(filePath, "image file path");
  return messagePart({ type: "image", filePath });
}

function normalizeParts(parts, messageFormat = "text") {
  if (!["text", "markdown"].includes(messageFormat)) {
    throw new TypeError("message format must be text or markdown");
  }

  const flattened = [];
  function append(part) {
    if (typeof part === "string") {
      flattened.push({ type: "text", text: part });
      return;
    }
    if (part?.__iskyproMessagePart === true) {
      flattened.push({ ...part.payload });
      return;
    }
    if (part?.__iskyproCompositePart === true) {
      for (const child of part.parts) {
        append(child);
      }
      return;
    }
    throw new TypeError("message parts must be strings or SDK message parts");
  }

  for (const part of parts) {
    append(part);
  }
  const normalized = [];
  for (const part of flattened) {
    if (part.type === "text" && part.text.length === 0) {
      continue;
    }
    if (part.type === "text" && normalized.at(-1)?.type === "text") {
      normalized.at(-1).text += part.text;
    } else {
      normalized.push(part);
    }
  }
  if (normalized.length === 0) {
    throw new TypeError("message must contain at least one non-empty part");
  }
  return {
    parts: normalized,
    ...(messageFormat === "text" ? {} : { format: messageFormat }),
  };
}

export class MessageContext {
  constructor(event, context) {
    this.eventId = event.eventId ?? "";
    this.eventType = event.eventType ?? "";
    this.timestamp = event.timestamp;
    this.source = event.source ?? "";
    this.bot = Object.freeze({ ...(event.bot ?? {}) });
    this.conversation = Object.freeze({ ...(event.conversation ?? {}) });
    this.sender = Object.freeze({ ...(event.sender ?? {}) });
    this.id = event.message?.id ?? "";
    this.text = event.message?.content ?? "";
    this.attachments = Object.freeze([...(event.message?.attachments ?? [])]);
    this.mentions = Object.freeze([...(event.message?.mentions ?? [])]);
    this.rawPayload = event.rawPayload ?? {};
    this.reference = Object.freeze({ ...(event.messageReference ?? {}) });
    this.context = context;
  }

  reply(...parts) {
    return this.context.invoke("messages.reply", {
      reference: this.reference,
      message: normalizeParts(parts),
    });
  }

  replyMarkdown(...parts) {
    return this.context.invoke("messages.reply", {
      reference: this.reference,
      message: normalizeParts(parts, "markdown"),
    });
  }
}

export class MessageTarget {
  constructor(context, type, id, platform = undefined) {
    validateId(id, "message target id");
    this.context = context;
    this.target = Object.freeze(platform === undefined ? { type, id } : { type, id, platform });
  }

  send(...parts) {
    return this.context.invoke("messages.send", {
      target: this.target,
      message: normalizeParts(parts),
    });
  }

  sendMarkdown(...parts) {
    return this.context.invoke("messages.send", {
      target: this.target,
      message: normalizeParts(parts, "markdown"),
    });
  }
}

export class MessageService {
  constructor(context) {
    this.context = context;
  }

  group(id, platform) { return new MessageTarget(this.context, "group", id, platform); }
  channel(id, platform) { return new MessageTarget(this.context, "channel", id, platform); }
  user(id, platform) { return new MessageTarget(this.context, "user", id, platform); }
  directMessage(id, platform) { return new MessageTarget(this.context, "direct", id, platform); }
}

export class PluginContext extends GeneratedSdkMethods {
  constructor(pluginId, token, requestSender) {
    super();
    if (typeof requestSender !== "function") {
      throw new TypeError("PluginContext requires a multiplexed request sender");
    }
    this.pluginId = pluginId;
    this.token = token;
    this.requestSender = requestSender;
    this.messages = new MessageService(this);
  }

  invoke(method, parameters = {}) {
    if (typeof method !== "string" || method.trim().length === 0) {
      throw new Error("SDK method is required");
    }
    const requestParameters = { ...parameters, token: this.token };
    return Promise.resolve(this.requestSender(method, requestParameters));
  }

  logWrite(message, level = "Information") {
    return this.invoke("log.write", { level: level || "Information", message });
  }

}

export class StdioJsonRpcPlugin {
  constructor(
    pluginId,
    sdkName = "iskypro-node-sdk-v2",
    sdkVersion = currentSdkVersion,
    options = {},
  ) {
    this.pluginId = pluginId;
    this.sdkName = sdkName;
    this.sdkVersion = sdkVersion;
    this.token = "";
    this.input = options.input ?? process.stdin;
    this.outputFd = options.outputFd ?? 1;
    this.localMaxConcurrentEvents = options.maxConcurrentEvents ?? null;
    this.localQueueCapacity = options.queueCapacity ?? null;
    this.requestTimeoutMs = normalizeRequestTimeout(options.requestTimeoutMs);
    this.nextRequestId = 1000;
    this.pending = new Map();
    this.eventQueue = [];
    this.activeEvents = 0;
    this.maxConcurrentEvents = 1;
    this.queueCapacity = 64;
    this.initialized = false;
    this.stopping = false;
    this.stopCompleted = false;
    this.stopRequestId = null;
    this.onEvent = null;
    this.resolveStop = null;
    this.stopPromise = new Promise((resolve) => {
      this.resolveStop = resolve;
    });
  }

  async run(onEvent) {
    if (typeof onEvent !== "function") {
      throw new TypeError("event handler is required");
    }
    this.onEvent = onEvent;
    const iterator = readFrames(this.input)[Symbol.asyncIterator]();
    try {
      while (!this.stopCompleted) {
        const outcome = await Promise.race([
          iterator.next().then((value) => ({ type: "frame", value })),
          this.stopPromise.then(() => ({ type: "stop" })),
        ]);
        if (outcome.type === "stop") {
          break;
        }
        if (outcome.value.done) {
          break;
        }
        this.handleMessage(outcome.value.value);
      }
    } finally {
      if (typeof iterator.return === "function") {
        await iterator.return();
      }
      this.failPending(new Error("ISkyPro stdio-jsonrpc connection closed"));
    }
  }

  handleMessage(message) {
    if (Object.hasOwn(message, "method")) {
      this.handleRequest(message);
    } else {
      this.handleResponse(message);
    }
  }

  handleRequest(request) {
    const parameters = request.params ?? {};
    if (parameters === null || typeof parameters !== "object" || Array.isArray(parameters)) {
      this.writeError(request.id, -32602, "JSON-RPC params must be an object");
      return;
    }
    if (request.method === "iskypro.initialize") {
      this.handleInitialize(request.id, parameters);
      return;
    }
    if (request.method === "plugin.stop" || request.method === "shutdown") {
      this.handleStop(request.id);
      return;
    }
    if (request.method === "events.message") {
      this.scheduleEvent(request.id, parameters);
      return;
    }
    if (Object.hasOwn(request, "id")) {
      this.writeError(request.id, -32601, `unknown method: ${request.method}`);
    }
  }

  handleInitialize(id, parameters) {
    if (id === undefined || id === null) {
      throw new Error("iskypro.initialize must be a JSON-RPC request");
    }
    if (this.initialized) {
      this.writeError(id, -32000, "Plugin SDK v2 is already initialized");
      return;
    }
    if (!(parameters.supportedProtocolVersions ?? []).includes(protocolVersion)) {
      this.writeError(id, -32602, "Main does not support protocol version 2");
      return;
    }
    if (String(parameters.pluginId ?? "") !== this.pluginId) {
      this.writeError(id, -32602, "initialize pluginId does not match the plugin");
      return;
    }
    if (String(parameters.encoding ?? "").toLowerCase() !== "json") {
      this.writeError(id, -32602, "Plugin SDK v2 only supports json encoding");
      return;
    }
    this.token = String(parameters.token ?? parameters.runtimeToken ?? "");
    if (this.token.length === 0) {
      this.writeError(id, -32602, "initialize token is required");
      return;
    }

    const runtime = parameters.runtime ?? {};
    const mainConcurrency = positiveInteger(runtime.maxConcurrentEvents, 1);
    const localConcurrency = positiveInteger(this.localMaxConcurrentEvents, mainConcurrency);
    this.maxConcurrentEvents = Math.min(mainConcurrency, localConcurrency);
    const mainQueueCapacity = positiveInteger(runtime.queueCapacity, 64);
    const localQueueCapacity = positiveInteger(this.localQueueCapacity, mainQueueCapacity);
    this.queueCapacity = Math.min(mainQueueCapacity, localQueueCapacity);
    this.requestTimeoutMs = normalizeRequestTimeout(
      runtime.requestTimeoutMilliseconds,
      this.requestTimeoutMs,
    );
    this.initialized = true;
    this.writeSuccess(id, {
      protocolVersion,
      pluginId: this.pluginId,
      sdkName: this.sdkName,
      sdkVersion: this.sdkVersion,
      capabilities: [
        "events.message",
        "log.write",
        "messages.reply",
        "messages.send",
        "bidirectional-requests",
        "concurrent-events",
        "graceful-shutdown",
      ],
      encoding: "json",
    });
  }

  scheduleEvent(id, event) {
    if (!this.initialized) {
      this.writeError(id, -32600, "Plugin must be initialized before events");
      return;
    }
    if (this.stopping) {
      this.writeError(id, -32000, "Plugin is stopping");
      return;
    }
    if (
      this.activeEvents >= this.maxConcurrentEvents &&
      this.eventQueue.length >= this.queueCapacity
    ) {
      if (id !== undefined && id !== null) {
        this.writeSuccess(id, {
          accepted: false,
          eventId: event.eventId ?? "",
          error: "Plugin SDK v2 local event queue is full.",
        });
      }
      return;
    }
    this.eventQueue.push({ id, event });
    this.drainEvents();
  }

  drainEvents() {
    while (
      !this.stopping &&
      this.activeEvents < this.maxConcurrentEvents &&
      this.eventQueue.length > 0
    ) {
      const job = this.eventQueue.shift();
      this.activeEvents += 1;
      void this.runEvent(job).finally(() => {
        this.activeEvents -= 1;
        this.drainEvents();
        this.completeStopIfReady();
      });
    }
  }

  async runEvent({ id, event }) {
    const context = new PluginContext(
      this.pluginId,
      this.token,
      (method, parameters) => this.sendRequest(method, parameters),
    );
    const message = new MessageContext(event, context);
    let response;
    try {
      const result = (await this.onEvent(message, context)) ?? {};
      if (result === null || typeof result !== "object" || Array.isArray(result)) {
        throw new TypeError("event handler result must be an object");
      }
      response = {
        accepted: result.accepted ?? true,
        eventId: event.eventId ?? "",
        error: result.error ?? null,
      };
    } catch (error) {
      response = {
        accepted: false,
        eventId: event.eventId ?? "",
        error: error instanceof Error ? error.message : String(error),
      };
    }
    if (id !== undefined && id !== null) {
      this.writeSuccess(id, response);
    }
  }

  handleStop(id) {
    if (this.stopping) {
      return;
    }
    this.stopping = true;
    this.stopRequestId = id;
    while (this.eventQueue.length > 0) {
      const job = this.eventQueue.shift();
      if (job.id !== undefined && job.id !== null) {
        this.writeError(job.id, -32000, "Plugin is stopping");
      }
    }
    this.completeStopIfReady();
  }

  completeStopIfReady() {
    if (!this.stopping || this.stopCompleted || this.activeEvents !== 0) {
      return;
    }
    if (this.stopRequestId !== undefined && this.stopRequestId !== null) {
      this.writeSuccess(this.stopRequestId, { accepted: true });
    }
    this.stopCompleted = true;
    this.resolveStop();
    if (typeof this.input.destroy === "function") {
      this.input.destroy();
    }
  }

  sendRequest(method, parameters) {
    this.nextRequestId += 1;
    const id = this.nextRequestId;
    return new Promise((resolve, reject) => {
      const timer = setTimeout(() => {
        this.pending.delete(String(id));
        reject(new Error(`SDK request '${method}' timed out after ${this.requestTimeoutMs} ms`));
      }, this.requestTimeoutMs);
      this.pending.set(String(id), { resolve, reject, timer });
      try {
        writeFrame(
          {
            jsonrpc: jsonRpcVersion,
            id,
            method,
            params: parameters,
          },
          this.outputFd,
        );
      } catch (error) {
        clearTimeout(timer);
        this.pending.delete(String(id));
        reject(error);
      }
    });
  }

  handleResponse(response) {
    const key = String(response.id);
    const pending = this.pending.get(key);
    if (!pending) {
      return;
    }
    clearTimeout(pending.timer);
    this.pending.delete(key);
    if (response.error) {
      pending.reject(
        new JsonRpcError(
          response.error.code ?? -32000,
          response.error.message ?? String(response.error),
          response.error.data,
        ),
      );
    } else {
      pending.resolve(response.result);
    }
  }

  writeSuccess(id, result) {
    writeFrame({ jsonrpc: jsonRpcVersion, id, result }, this.outputFd);
  }

  writeError(id, code, message) {
    if (id === undefined || id === null) {
      return;
    }
    writeFrame({ jsonrpc: jsonRpcVersion, id, error: { code, message } }, this.outputFd);
  }

  failPending(error) {
    for (const pending of this.pending.values()) {
      clearTimeout(pending.timer);
      pending.reject(error);
    }
    this.pending.clear();
  }
}
