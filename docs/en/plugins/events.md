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

Common QQBot event: `GROUP_AT_MESSAGE_CREATE` (full group message streams are
private-domain only).

In SDK v2, it is usually mapped as a message event with `conversation.type` set to
`group` and `messageReference.targetType` set to `group`. Reply with the
event-bound `message.ReplyAsync(...)` (the underlying method is `messages.reply`);
you never pass `messageReference` manually.

## Media and Mentions

For QQ media messages (image/video/voice/file), `message.attachments` is filled
with one entry per file (`url`, `content_type`, `size`, ...). Users mentioned in
guild messages are filled into `message.mentions` (`id`/`username`). Both arrays
are empty for plain-text messages; `rawPayload` keeps the complete original
fields (for example attachment dimensions and file names).

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
