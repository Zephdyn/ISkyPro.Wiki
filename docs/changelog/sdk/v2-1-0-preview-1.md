# 2.1.0-preview.1 SDK

发布日期：2026-08-25

本版本为 Plugin SDK v2 引入富媒体与群管理能力：本地/远程图片发送、消息撤回，并把低层 catalog 从占位改为真实 QQBot OpenAPI 转发。

## ✨ 新功能

- messages: 结构化消息新增图片发送：四语言 SDK 分别提供 `Image.FromFile`、`image()`、`image(filePath)`、`Image(filePath)`，传入 ISkyPro 所在机器上的本地图片路径即可发送群聊或私聊图片（富媒体 `msg_type = 7`）。一条消息最多包含一个图片，且不能与 Markdown 组合；多张图片请拆成多条消息。
- messages: 新增远程图片 `image-url` part（`Image.FromUrl`、`image_url`、`imageUrl`、`ImageUrl`）：图片有公网 URL 时由 Main 走官方 URL 直传（`file_type=1` + `url` + `srv_send_msg=false` → `file_info` → 富媒体），仅支持群聊与单聊目标；本地 base64 `file_data` 上传路径保留为兼容能力，未做迁移。
- messages: 新增撤回消息能力：四语言 SDK 提供 `RecallAsync` / `recall()` / `recall(messageId)` / `Recall()`，可撤回机器人自己发送的消息（发送超过 2 分钟不可撤回，文字子频道/频道私信仅私域可用），需要 manifest 声明 `messages.recall` 权限。
- protocol: 消息链路升级为平台无关：`messageReference` 与 `messages.send` 的 `target` 支持可选 `platform`；已编译旧 C# 插件在 OneBot 事件内回复也会通过最近事件平台自动路由到 OneBot。
- catalog: 低层 catalog 方法已从占位改为真实 QQBot OpenAPI 转发；`unsafe.rawOpenApi` 仍默认关闭，开启全局允许开关并声明权限后可调用任意 QQBot OpenAPI。
- errors: QQ 平台错误结构化：发送/调用失败时 JSON-RPC error 的 `data` 提供 `errorCode`（`qq.api.<errCode>` 或 `qq.api.http.<status>`）、`statusCode` 与 `platformErrorCode`，22009 消息超频等错误可直接按错误码分支处理。
- events: 事件模型的附件与提及已自动填充：QQ 媒体消息（图片/视频/语音/文件）进入 `message.attachments`（`url`/`content_type`/`size`），频道消息被 @ 的用户进入 `message.mentions`；OneBot 的 voice/video/file 消息段同样映射。
- catalog: 低层目录新增群管理接口：群机器人状态（`groups.getBotState`）、群禁言查询/设置、入群申请列表与审批、入群自动审批策略（列表/创建/修改/删除/执行/白名单）；`groups.getGroupInfo` 路径修正为官方接口（原路径请求失败）。

## ♻️ 兼容性

- 发图优先使用高层 `messages.send` / `messages.reply` 的 `image`（本地文件）或 `image-url`（远程 URL）part；低层目录方法（`media.uploadC2CFile` 等）虽已真实转发，仍不推荐直接调用。
- 主动消息按 QQ 官方现行规则有效，企业认证、个人认证和未认证机器人均可发送，仅 qps/qpm 与每日上限不同；用户可在客户端关闭「允许主动发送」，关闭后发送失败（插件会收到 `qq.api.22009` 等结构化错误）。