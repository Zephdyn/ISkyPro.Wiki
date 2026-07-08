# ISkyPro Wiki

[![Status](https://img.shields.io/badge/status-preview-orange)](https://github.com/Zephdyn/ISkyPro.Wiki)
[![Version](https://img.shields.io/badge/version-2.0.0--preview.3-blue)](https://github.com/Zephdyn/ISkyPro.Wiki)
[![Docs Build](https://img.shields.io/github/actions/workflow/status/Zephdyn/ISkyPro.Wiki/deploy.yml?branch=main&label=docs%20build&logo=githubactions)](https://github.com/Zephdyn/ISkyPro.Wiki/actions/workflows/deploy.yml)
[![VitePress](https://img.shields.io/badge/VitePress-1.6-646CFF?logo=vite)](https://vitepress.dev/)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20Linux%20preview-0078D6)](https://github.com/Zephdyn/ISkyPro.Wiki)
[![Languages](https://img.shields.io/badge/docs-%E4%B8%AD%E6%96%87%20%7C%20English-2ea44f)](README.en.md)

English: [README.en.md](README.en.md)

ISkyPro Wiki 是 ISkyPro 的公开文档仓库，面向用户、插件作者和部署者提供项目介绍、快速开始、发布说明和插件文档。

ISkyPro 是面向 QQBot 与旧 ISky 插件生态的 .NET 10 重构版 Bot 框架。它把主程序、Web 管理界面、QQBot 网关、旧 DLL 插件隔离宿主、`message.dll` 兼容层和 Plugin SDK v2 统一到新的运行与扩展模型中，在保留旧插件兼容性的同时提供现代化的部署和管理体验。

当前文档面向 `2.0.0-preview.3` 预览版。预览版优先保持旧插件 ABI、Windows x86 插件宿主发布策略和主要 WebUI 操作稳定；新的 Plugin SDK v2 仍可能在正式版前调整。

## 在线文档

- 中文文档：[https://zephdyn.github.io/ISkyPro.Wiki/](https://zephdyn.github.io/ISkyPro.Wiki/)
- English docs: [https://zephdyn.github.io/ISkyPro.Wiki/en/](https://zephdyn.github.io/ISkyPro.Wiki/en/)
- 快速开始：[docs/guide/getting-started.md](docs/guide/getting-started.md)
- 更新日志：[docs/changelog/index.md](docs/changelog/index.md)
- 插件文档：[docs/plugins/index.md](docs/plugins/index.md)
- Webhook 与反向代理：[docs/guide/webhook-and-proxy.md](docs/guide/webhook-and-proxy.md)

## 核心能力

| 领域 | 说明 |
| --- | --- |
| QQBot 接入 | 支持 WebSocket / Webhook 接入、事件分发、签名校验和消息处理流程。 |
| WebUI 管理 | 提供 Bot 登录、运行状态、插件管理、日志查看、系统设置和中英文界面。 |
| 旧插件兼容 | Windows 包内通过独立 x86 插件宿主运行旧 DLL 插件，并提供 `message.dll` 兼容层。 |
| 现代插件 | Plugin SDK v2 面向新的 C# 插件开发模式，适合新插件和逐步迁移。 |
| 部署运行 | 支持 Windows x64 / Windows ARM64；Linux x64 preview 支持主程序、WebUI、QQBot 网关和现代插件。 |
| 用户文档 | 覆盖安装启动、QQBot 事件配置、Webhook 反代、插件发布、下载和常见问题。 |

## 平台说明

- Windows 发布包包含主程序、WebUI、旧插件兼容宿主、`message.dll` 和 Windows Service 脚本。
- Linux x64 preview 包面向 glibc 发行版，包含主程序、WebUI、QQBot 网关和 Plugin SDK v2 现代插件运行能力。
- Linux 包不包含旧 DLL 插件宿主、`message.dll` 或 Windows Service 脚本。
- 使用发布包运行 ISkyPro 时不需要安装 .NET SDK、Node.js 或编译工具链。

## 本地预览

本仓库使用 VitePress 构建文档站点。

```powershell
pnpm install
pnpm docs:dev
pnpm docs:build
pnpm docs:preview
```

在 Windows 路径包含 `#` 时，`pnpm docs:dev` 会通过临时的无 `#` 工作目录启动，避免 Vite dev server 把路径片段误解析为 URL fragment。编辑文档后请重启 dev server，以刷新临时副本。

GitHub Pages 仓库站点构建时需要设置 `VITEPRESS_BASE`，例如：

```powershell
$env:VITEPRESS_BASE = "/ISkyPro.Wiki/"
pnpm docs:build
```

## 内容边界

本仓库仅收录公开发布文档、用户指南、插件文档和文档站点资源。运行时密钥、访问令牌、诊断数据、未发布内容和源码仓库专用说明不应进入本仓库。
