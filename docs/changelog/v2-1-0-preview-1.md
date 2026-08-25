# 2.1.0-preview.1

发布日期：2026-08-25

2.1.0 首个预览版，主要新增 OneBot 平台网关、可选的平台化登录界面，以及 Plugin SDK v2 的本地图片发送能力。

## ✨ Features

- onebot: 新增 OneBot 平台网关，可与 QQBot 并行运行；支持 OneBot v11 / v12 消息协议、正向/反向 WebSocket、CQ 码富媒体（图片、@、回复、语音）和出站消息限速。
- gateway: 网关页可按平台查看 QQBot / OneBot 的配置、在线状态与收发计数；OneBot 与 QQBot 相互独立，可单独启用。
- webui: 登录页支持按平台选择 QQBot 或 OneBot 配置，前端改为模块化平台架构。

## 🧩 Plugin Development

- sdk: 结构化消息新增图片发送：四语言 SDK 分别提供 `Image.FromFile`、`image()`、`image(filePath)`、`Image(filePath)`，传入 ISkyPro 所在机器上的本地图片路径即可发送群聊或私聊图片（富媒体 `msg_type = 7`）。一条消息最多包含一个图片，且不能与 Markdown 组合；多张图片请拆成多条消息。
- sdk: Plugin SDK v2 消息链路升级为平台无关：`messageReference` 与 `messages.send` 的 `target` 支持可选 `platform`；Main 新增 OneBot v2 事件映射和 CQ 码发送，已编译旧 C# 插件在 OneBot 事件内回复也会通过最近事件平台自动路由到 OneBot。
- sdk: 低层 catalog 方法已从占位改为真实 QQBot OpenAPI 转发；`unsafe.rawOpenApi` 仍默认关闭，开启全局允许开关并声明权限后可调用任意 QQBot OpenAPI。

## ⚙️ Configuration

- onebot: 可通过 WebUI 或配置文件启用 OneBot 正向/反向 WebSocket，配置反向连接 `access_token` 校验与重连退避上限。

## ♻️ Compatibility

- onebot: 新增平台网关不影响现有 QQBot 网关；`IBotGateway`（模拟/状态接口）仍以 QQBot 为默认实现。
- sdk: `media.uploadC2CFile` 等低层目录方法仍为授权 stub；发图请使用高层 `messages.send` / `messages.reply` 的 `image` part。
