# 2.1.0-preview.1

发布日期：2026-08-25

2.1.0 首个预览版，主要新增 OneBot 平台网关和可选的平台化登录界面；SDK 同步新增图片发送等能力（详见文末 SDK 更新日志）。

## ✨ 新功能

- onebot: 新增 OneBot 平台网关，可与 QQBot 并行运行；支持 OneBot v11 / v12 消息协议、正向/反向 WebSocket、CQ 码富媒体（图片、@、回复、语音）和出站消息限速。
- gateway: 网关页可按平台查看 QQBot / OneBot 的配置、在线状态与收发计数；OneBot 与 QQBot 相互独立，可单独启用。
- webui: 登录页支持按平台选择 QQBot 或 OneBot 配置，前端改为模块化平台架构。
- webui: 插件页重构为统一卡片列表和独立「安装插件」选项卡：v1 与 v2+ 插件合并展示，支持搜索、按类型/状态筛选和详情面板；v2+ zip 安装、v1 DLL 上传与 HTTP 插件注册三个入口分开，避免误安装。

## ⚙️ 配置

- onebot: 可通过 WebUI 或配置文件启用 OneBot 正向/反向 WebSocket，配置反向连接 `access_token` 校验与重连退避上限。

## ♻️ 兼容性

- onebot: 新增平台网关不影响现有 QQBot 网关；`IBotGateway`（模拟/状态接口）仍以 QQBot 为默认实现。

## 📦 SDK 更新日志

本版本的 Plugin SDK v2 变更（图片发送、消息撤回、平台无关消息链路、群管理接口等）详见 [SDK 更新日志](/changelog/sdk/v2-1-0-preview-1.md)。
