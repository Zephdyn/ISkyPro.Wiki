# Events

Plugin SDK v2 events provide both normalized fields and the full `rawPayload`. Normalized fields are for routine development. `rawPayload` is available for QQ platform fields that are new or not yet modeled.

Common fields:

- `eventId`: framework event ID.
- `eventType`: event type.
- `bot`: bot account context.
- `conversation`: conversation type and target ID.
- `sender`: sender ID, member ID, display name, and related fields.
- `message`: message ID, content, attachments, and mentions.
- `messageReference`: reference used for delayed passive replies.
- `rawPayload`: complete QQBot payload.

## Group Messages

Common QQBot event: `GROUP_MESSAGE_CREATE`.

In SDK v2, it is usually mapped as a message event with `conversation.type` set to `group` and `messageReference.targetType` set to `group`. Prefer `messages.replyText` with the event `messageReference` when replying.

## C2C / Direct Messages

C2C messages are usually mapped as `conversation.type = "c2c"`. Replies still use the `messageReference` from the event. Active messages require send permissions in the plugin manifest and remain subject to platform permissions.

## Guild Messages

Guild messages usually include guild and channel context. Plugins should inspect `conversation.guildId`, `conversation.channelId`, and `rawPayload` from the actual event.

## Direct Messages

Guild direct messages and C2C messages are different target types. Do not infer the target only from text content; read `conversation.type` and `messageReference.targetType`.

## Development Advice

- Prefer `messageReference` for normal replies.
- Read `rawPayload` when you need newly added QQ platform fields.
- ACK events quickly, then run slow work asynchronously inside the plugin.
- Command plugins should use command names and prefixes from the manifest to avoid duplicated responses from multiple plugins.
