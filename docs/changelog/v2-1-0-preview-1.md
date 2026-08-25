# 2.1.0-preview.1

发布日期：2026-08-25

2.1.0 首个预览版，主要新增 OneBot 平台网关、可选的平台化登录界面，以及 Plugin SDK v2 的本地图片发送能力。

## ✨ Features

- onebot: 新增 OneBot 平台网关，可与 QQBot 并行运行；支持 OneBot v11 / v12 消息协议、正向/反向 WebSocket、CQ 码富媒体（图片、@、回复、语音）和出站消息限速。
- gateway: 网关页可按平台查看 QQBot / OneBot 的配置、在线状态与收发计数；OneBot 与 QQBot 相互独立，可单独启用。
- webui: 登录页支持按平台选择 QQBot 或 OneBot 配置，前端改为模块化平台架构。
- webui: 插件页重构为统一卡片列表和独立「安装插件」选项卡：v1 与 v2+ 插件合并展示，支持搜索、按类型/状态筛选和详情面板；v2+ zip 安装、v1 DLL 上传与 HTTP 插件注册三个入口分开，避免误安装。

## 🧩 Plugin Development

- sdk: 结构化消息新增图片发送：四语言 SDK 分别提供 `Image.FromFile`、`image()`、`image(filePath)`、`Image(filePath)`，传入 ISkyPro 所在机器上的本地图片路径即可发送群聊或私聊图片（富媒体 `msg_type = 7`）。一条消息最多包含一个图片，且不能与 Markdown 组合；多张图片请拆成多条消息。
- sdk: 新增远程图片 `image-url` part（`Image.FromUrl`、`image_url`、`imageUrl`、`ImageUrl`）：图片有公网 URL 时由 Main 走官方 URL 直传（`file_type=1` + `url` + `srv_send_msg=false` → `file_info` → 富媒体），仅支持群聊与单聊目标；本地 base64 `file_data` 上传路径保留为兼容能力，未做迁移。
- sdk: 新增撤回消息能力：四语言 SDK 提供 `RecallAsync` / `recall()` / `recall(messageId)` / `Recall()`，可撤回机器人自己发送的消息（发送超过 2 分钟不可撤回，文字子频道/频道私信仅私域可用），需要 manifest 声明 `messages.recall` 权限。
- sdk: Plugin SDK v2 消息链路升级为平台无关：`messageReference` 与 `messages.send` 的 `target` 支持可选 `platform`；Main 新增 OneBot v2 事件映射和 CQ 码发送，已编译旧 C# 插件在 OneBot 事件内回复也会通过最近事件平台自动路由到 OneBot。
- sdk: 低层 catalog 方法已从占位改为真实 QQBot OpenAPI 转发；`unsafe.rawOpenApi` 仍默认关闭，开启全局允许开关并声明权限后可调用任意 QQBot OpenAPI。
- sdk: QQ 平台错误结构化：发送/调用失败时 JSON-RPC error 的 `data` 提供 `errorCode`（`qq.api.<errCode>` 或 `qq.api.http.<status>`）、`statusCode` 与 `platformErrorCode`，22009 消息超频等错误可直接按错误码分支处理。
- sdk: 事件模型的附件与提及已自动填充：QQ 媒体消息（图片/视频/语音/文件）进入 `message.attachments`（`url`/`content_type`/`size`），频道消息被 @ 的用户进入 `message.mentions`；OneBot 的 voice/video/file 消息段同样映射。
- sdk: 低层目录新增群管理接口：群机器人状态（`groups.getBotState`）、群禁言查询/设置、入群申请列表与审批、入群自动审批策略（列表/创建/修改/删除/执行/白名单）；`groups.getGroupInfo` 路径修正为官方接口（原路径请求失败）。

## ⚙️ Configuration

- onebot: 可通过 WebUI 或配置文件启用 OneBot 正向/反向 WebSocket，配置反向连接 `access_token` 校验与重连退避上限。

## ♻️ Compatibility

- onebot: 新增平台网关不影响现有 QQBot 网关；`IBotGateway`（模拟/状态接口）仍以 QQBot 为默认实现。
- sdk: 低层目录方法（`media.uploadC2CFile` 等）已通过 Main 真实转发到 QQBot OpenAPI；发图仍推荐使用高层 `messages.send` / `messages.reply` 的 `image`（本地文件）或 `image-url`（远程 URL）part。
- sdk: 主动消息按 QQ 官方现行规则有效，企业认证、个人认证和未认证机器人均可发送，仅 qps/qpm 与每日上限不同；用户可在客户端关闭「允许主动发送」，关闭后发送失败（插件会收到 `qq.api.22009` 等结构化错误）。
