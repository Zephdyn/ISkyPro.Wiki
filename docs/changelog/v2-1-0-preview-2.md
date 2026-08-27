# 2.1.0-preview.2

状态：开发中，暂未发布

本预览版聚焦多平台体验与扩展架构：仪表盘支持任意已登录平台展示；当前版本限制单一平台启用（QQBot / OneBot 二选一），账号模型、日志字段与插件协议已按多平台多账号设计；stdio-jsonrpc 内部优化对插件与用户无感。

## ✨ 新功能

- dashboard: 仪表盘改为多平台账号视图：任何已登录平台（QQBot 或 OneBot）都会显示账号名称、头像（QQBot 使用开放平台头像，OneBot 无头像时显示首字母占位）、平台标识、在线状态与收发消息计数；指标卡随当前启用平台变化。
- accounts: 新增 `GET /api/bot/accounts` 统一账号视图；机器人消息日志新增 `platform` 字段，`transport` 字段语义统一（QQBot=连接来源 websocket/webhook/simulated，OneBot=连接模式 reverse-ws/forward-ws/http），旧数据库自动迁移并回填。
- platform: 当前版本仅允许单一平台启用。登录 QQBot 前需先退出 OneBot（反之亦然）；WebUI 切换平台时弹出确认并自动先退出当前平台（聊天记录保留），后端亦拒绝双平台并存配置。
- settings: 设置页随启用平台切换：QQBot 激活时显示 QQBot 专属设置（机器人配置、Mention 过滤、Webhook 回调与反代生成器）；OneBot 激活时显示 OneBot 专属设置面板（协议版本、连接模式、WS/HTTP 地址、Access Token、重连参数等），保存立即热生效，无需先退出平台，并可直接在设置页退出平台。
- guide: Webhook 接入指引上线：登录页选择 Webhook 模式后（设置页 Webhook 区域同步展示）会给出分步指引——准备公网地址、确认监听地址、在 QQ 开放平台填写回调地址、订阅事件、验证回调；OneBot 接入也提供四步指引。
- proxy: 「反代生成器」新增 IIS（URL Rewrite + ARR）模板，Windows Server + IIS 用户可直接复制 web.config 重写规则。
- plugins: v1 插件上传遇到同名 DLL 时不再直接报错，改为弹出覆盖确认框；确认后覆盖更新并重新扫描启动，与 v2 插件的冲突处理体验一致。
- ux: 后端错误增加稳定错误码，WebUI 按界面语言（中文/英文）本地化显示错误提示。

## ⚡ 性能

- stdio: Main 侧 stdio-jsonrpc 帧读取改为缓冲扫描（替代逐字节读取），插件 SDK 调用由有界并发派发处理，不再阻塞同一连接上的事件 ACK 与其它 SDK 响应；线上协议字节不变，插件与用户无感。实测帧解析吞吐提升约 1.4~4.9 倍（视帧大小与运行环境而定）。

## ♻️ 兼容性

- 日志库自动迁移：`bot_messages` 新增 `platform` 列并按账号前缀回填，无需手动处理。
- 双平台同时启用将被拒绝（400），当前仅支持单平台单账号；放开多平台的能力已按设计预留（见开发计划）。

## 📦 SDK 更新日志

本版本的 Plugin SDK v2 变更（消息目标可选 `botAccountId`、稳定错误码等）详见 [SDK 更新日志](/changelog/sdk/v2-1-0-preview-2.md)。