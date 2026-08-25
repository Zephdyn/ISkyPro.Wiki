# 事件详解

Plugin SDK v2 事件同时提供规范字段和完整 `rawPayload`。规范字段用于常规开发，`rawPayload` 用于读取 QQ 平台新增或暂未建模的字段。

常见字段：

- `eventId`：框架侧事件 ID。
- `eventType`：事件类型。
- `bot`：机器人账号上下文。
- `conversation`：会话类型和目标 ID。
- `sender`：发送者 ID、成员 ID、展示名等。
- `message`：消息 ID、内容、附件和 mentions。
- `messageReference`：用于延迟被动回复的引用。
- `rawPayload`：完整 QQBot payload。

## 群消息

常见 QQBot 事件：`GROUP_AT_MESSAGE_CREATE`。ISkyPro 收到群聊消息事件后映射为
消息事件，`conversation.type` 为 `group`，`messageReference.targetType` 为
`group`。回复时使用事件绑定的 `message.ReplyAsync(...)`（底层为
`messages.reply`），不需要手工传递 `messageReference`。

## 媒体与提及

QQ 媒体消息（图片/视频/语音/文件）的 `message.attachments` 数组已自动填充，每项包含
`url`、`content_type`、`size` 等字段；频道消息中被 @ 的用户会填充到 `message.mentions`
（`id`/`username`）。纯文本消息两者的数组为空；`rawPayload` 仍保留完整原始字段
（如附件尺寸、文件名）。

## C2C / 单聊消息

单聊消息通常映射为 `conversation.type = "c2c"`。回复仍使用事件中的 `messageReference`，主动消息需要插件 manifest 声明发送权限，并受平台权限限制。

## 频道消息

频道消息通常包含 guild 和 channel 上下文。插件需要按实际事件中的 `conversation.guildId`、`conversation.channelId` 和 `rawPayload` 判断来源。

## 私信消息

频道私信和 C2C 不是同一类目标。开发时不要只按文本内容判断目标，应该读取 `conversation.type` 和 `messageReference.targetType`。

## 开发建议

- 常规回复优先使用 `messageReference`。
- 需要 QQ 平台新增字段时读取 `rawPayload`。
- 插件收到事件后尽快 ACK，耗时业务放到插件内部异步处理。
- command 插件用 manifest 中的命令和前缀路由，避免多个插件重复响应同一命令。
