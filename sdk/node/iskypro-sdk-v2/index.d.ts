import { GeneratedSdkMethods } from "./generated-methods.js";
import type { JsonObject } from "./generated-methods.js";

export {
  GeneratedSdkMethods,
  sdkMethods,
  sdkVersion,
} from "./generated-methods.js";
export type { JsonObject, SdkMethodDescriptor } from "./generated-methods.js";

export const jsonRpcVersion: "2.0";
export const protocolVersion: 2;
export const defaultRequestTimeoutMs: number;
export const maxHeaderLength: number;
export const maxPayloadLength: number;

export class JsonRpcError extends Error {
  readonly code: number;
  constructor(code: number, message: string);
}

export function readFrame(fd?: number): JsonObject | null;
export function readFrames(stream?: AsyncIterable<Uint8Array>): AsyncGenerator<JsonObject>;
export function writeFrame(message: JsonObject, fd?: number): void;

export type RequestSender = (method: string, parameters: JsonObject) => Promise<unknown> | unknown;

export class PluginContext extends GeneratedSdkMethods {
  readonly pluginId: string;
  readonly token: string;
  constructor(pluginId: string, token: string, requestSender: RequestSender);
  invoke(method: string, parameters?: JsonObject): Promise<unknown>;
  logWrite(message: string, level?: string): Promise<unknown>;
  replyText(messageReference: JsonObject, content: string): Promise<unknown>;
}

export interface StdioJsonRpcPluginOptions {
  input?: AsyncIterable<Uint8Array> & { destroy?: () => void };
  outputFd?: number;
  maxConcurrentEvents?: number;
  queueCapacity?: number;
  requestTimeoutMs?: number;
}

export interface EventAck {
  accepted?: boolean;
  error?: string | null;
}

export type EventHandler = (
  event: JsonObject,
  context: PluginContext,
) => EventAck | void | Promise<EventAck | void>;

export class StdioJsonRpcPlugin {
  constructor(
    pluginId: string,
    sdkName?: string,
    sdkVersion?: string,
    options?: StdioJsonRpcPluginOptions,
  );
  run(onEvent: EventHandler): Promise<void>;
}
