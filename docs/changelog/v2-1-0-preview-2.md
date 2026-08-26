# 2.1.0-preview.2

发布日期：2026-08-26

本预览版聚焦多平台体验与扩展架构：仪表盘支持任意已登录平台展示；当前版本限制单一平台启用（QQBot / OneBot 二选一），账号模型、日志字段与插件协议已按多平台多账号设计；stdio-jsonrpc 内部优化对插件与用户无感。

## ✨ Features

- dashboard: 仪表盘改为多平台账号视图：任何已登录平台（QQBot 或 OneBot）都会显示账号名称、头像（QQBot 使用开放平台头像，OneBot 无头像时显示首字母占位）、平台标识、在线状态与收发消息计数；指标卡随当前启用平台变化。
- accounts: 新增 `GET /api/bot/accounts` 统一账号视图；机器人消息日志新增 `platform` 字段，`transport` 字段语义统一（QQBot=连接来源 websocket/webhook/simulated，OneBot=连接模式 reverse-ws/forward-ws/http），旧数据库自动迁移并回填。
- platform: 当前版本仅允许单一平台启用。登录 QQBot 前需先退出 OneBot（反之亦然）；WebUI 切换平台时弹出确认并自动先退出当前平台（聊天记录保留），后端亦拒绝双平台并存配置。

## 🧩 Plugin Development

- sdk: 消息目标支持可选 `botAccountId`：`messages.reply` 的事件引用与 `messages.send` / `messages.recall` 的 `target` 可显式指定目标账号（不传则使用平台默认账号，旧插件无需改动、行为不变）；事件引用自动携带事件来源账号。
- sdk: 平台未注册或账号不存在时返回稳定错误码 `message.target.platform_not_supported` / `message.target.account_not_found`。
- sdk: 四语言 SDK（C# / Node.js / Python / Go）消息 API 增加可选账号参数，为后续同时登录多平台多账号预留；新增平台（微信、飞书、钉钉、Telegram、Discord 等）只需在 Main 注册网关与适配器，SDK 与协议无需改动。

## ⚡ Performance

- stdio: Main 侧 stdio-jsonrpc 帧读取改为缓冲扫描（替代逐字节读取），插件 SDK 调用由有界并发派发处理，不再阻塞同一连接上的事件 ACK 与其它 SDK 响应；线上协议字节不变，插件与用户无感。实测帧解析吞吐提升约 1.4~4.9 倍（视帧大小与运行环境而定）。

## ♻️ Compatibility

- 日志库自动迁移：`bot_messages` 新增 `platform` 列并按账号前缀回填，无需手动处理。
- 插件协议向后兼容：新增字段全部可选；已编译旧插件无需重新发布。
- 双平台同时启用将被拒绝（400），当前仅支持单平台单账号；放开多平台的能力已按设计预留（见开发计划）。