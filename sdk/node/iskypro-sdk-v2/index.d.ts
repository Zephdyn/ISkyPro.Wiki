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
  readonly data?: unknown;
  constructor(code: number, message: string, data?: unknown);
}

export function readFrame(fd?: number): JsonObject | null;
export function readFrames(stream?: AsyncIterable<Uint8Array>): AsyncGenerator<JsonObject>;
export function writeFrame(message: JsonObject, fd?: number): void;

export type RequestSender = (method: string, parameters: JsonObject) => Promise<unknown> | unknown;

export interface UserRef {
  readonly provider: string;
  readonly mentionId: string;
  readonly userOpenId?: string | null;
  readonly memberOpenId?: string | null;
  readonly unionOpenId?: string | null;
  readonly displayName?: string | null;
}

export interface MessagePart {}

export type QqBotMentionFormat = "current" | "legacy" | "legacy-bang";

export const at: {
  user(user: string | UserRef, qqBotFormat?: QqBotMentionFormat): MessagePart;
  users(
    users: Iterable<string | UserRef>,
    separator?: string,
    qqBotFormat?: QqBotMentionFormat,
  ): MessagePart;
  readonly everyone: MessagePart;
};

export function image(filePath: string): MessagePart;

export class MessageContext {
  readonly eventId: string;
  readonly eventType: string;
  readonly timestamp: unknown;
  readonly source: string;
  readonly bot: JsonObject;
  readonly conversation: JsonObject;
  readonly sender: UserRef;
  readonly id: string;
  readonly text: string;
  readonly attachments: readonly unknown[];
  readonly mentions: readonly UserRef[];
  readonly rawPayload: JsonObject;
  reply(...parts: Array<string | MessagePart>): Promise<unknown>;
  replyMarkdown(...parts: Array<string | MessagePart>): Promise<unknown>;
}

export class MessageTarget {
  send(...parts: Array<string | MessagePart>): Promise<unknown>;
  sendMarkdown(...parts: Array<string | MessagePart>): Promise<unknown>;
}

export class MessageService {
  group(id: string, platform?: string): MessageTarget;
  channel(id: string, platform?: string): MessageTarget;
  user(id: string, platform?: string): MessageTarget;
  directMessage(id: string, platform?: string): MessageTarget;
}

export class PluginContext extends GeneratedSdkMethods {
  readonly pluginId: string;
  readonly token: string;
  readonly messages: MessageService;
  constructor(pluginId: string, token: string, requestSender: RequestSender);
  invoke(method: string, parameters?: JsonObject): Promise<unknown>;
  logWrite(message: string, level?: string): Promise<unknown>;
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
  message: MessageContext,
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
