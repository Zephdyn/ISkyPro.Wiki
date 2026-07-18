# ISkyPro Wiki

[![Status](https://img.shields.io/badge/status-preview-orange)](https://github.com/Zephdyn/ISkyPro.Wiki)
[![Version](https://img.shields.io/badge/version-2.0.0--preview.4-blue)](https://github.com/Zephdyn/ISkyPro.Wiki)
[![Docs Build](https://img.shields.io/github/actions/workflow/status/Zephdyn/ISkyPro.Wiki/deploy.yml?branch=main&label=docs%20build&logo=githubactions)](https://github.com/Zephdyn/ISkyPro.Wiki/actions/workflows/deploy.yml)
[![VitePress](https://img.shields.io/badge/VitePress-1.6-646CFF?logo=vite)](https://vitepress.dev/)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20Linux%20preview-0078D6)](https://github.com/Zephdyn/ISkyPro.Wiki)
[![Languages](https://img.shields.io/badge/docs-%E4%B8%AD%E6%96%87%20%7C%20English-2ea44f)](README.md)

中文：[README.md](README.md)

ISkyPro Wiki is the public documentation repository for ISkyPro. It provides the project overview, getting started guides, release notes, deployment notes, and plugin documentation for users, plugin authors, and operators.

ISkyPro is a .NET 10 rewrite of the bot framework for QQBot and the legacy ISky plugin ecosystem. It brings the main process, Web management UI, QQBot gateway, isolated legacy DLL plugin host, `message.dll` compatibility layer, and Plugin SDK v2 into a clearer runtime and extension model while preserving legacy plugin compatibility.

The current documentation targets `2.0.0-preview.4`. Preview builds prioritize stability for the legacy plugin ABI, the Windows x86 plugin-host packaging model, and major WebUI workflows. The new Plugin SDK v2 may still change before a stable release.

## Documentation

- Chinese docs: [https://zephdyn.github.io/ISkyPro.Wiki/](https://zephdyn.github.io/ISkyPro.Wiki/)
- English docs: [https://zephdyn.github.io/ISkyPro.Wiki/en/](https://zephdyn.github.io/ISkyPro.Wiki/en/)
- Getting started: [docs/en/guide/getting-started.md](docs/en/guide/getting-started.md)
- Changelog: [docs/en/changelog/index.md](docs/en/changelog/index.md)
- Plugin docs: [docs/en/plugins/index.md](docs/en/plugins/index.md)
- Webhook and reverse proxy: [docs/en/guide/webhook-and-proxy.md](docs/en/guide/webhook-and-proxy.md)

## Highlights

| Area | Description |
| --- | --- |
| QQBot gateway | Supports WebSocket / Webhook connections, event dispatch, signature verification, and message handling flows. |
| WebUI management | Provides Bot login, runtime status, plugin management, log viewing, settings, and Chinese / English UI. |
| Legacy plugin compatibility | Windows packages run legacy DLL plugins through an isolated x86 plugin host and provide the `message.dll` compatibility layer. |
| Modern plugins | Plugin SDK v2 supports the new C# plugin development model for new plugins and gradual migration. |
| Deployment | Supports Windows x64 / Windows ARM64; Linux x64 preview supports the main process, WebUI, QQBot gateway, and modern plugins. |
| User documentation | Covers installation, QQBot event setup, Webhook reverse proxy, plugin publishing, downloads, and FAQ topics. |

## Platform Notes

- Windows packages include the main process, WebUI, legacy plugin compatibility host, `message.dll`, and Windows Service scripts.
- Linux x64 preview packages target glibc distributions and include the main process, WebUI, QQBot gateway, and Plugin SDK v2 modern plugin runtime.
- Linux packages do not include the legacy DLL plugin host, `message.dll`, or Windows Service scripts.
- Running ISkyPro from a release package does not require the .NET SDK, Node.js, or compiler toolchains.

## Local Preview

This repository uses VitePress for the documentation site.

```powershell
pnpm install
pnpm docs:dev
pnpm docs:build
pnpm docs:preview
```

On Windows paths containing `#`, `pnpm docs:dev` starts from a temporary hash-free workspace so the Vite dev server does not interpret the path segment as a URL fragment. Restart the dev server after editing docs so the temporary copy is refreshed.

For GitHub Pages repository sites, set `VITEPRESS_BASE` when building:

```powershell
$env:VITEPRESS_BASE = "/ISkyPro.Wiki/"
pnpm docs:build
```

## Content Boundaries

This repository contains public release documentation, user guides, plugin documentation, and documentation-site assets. Runtime secrets, access tokens, diagnostic data, unpublished material, and source-repository-only notes should stay out of this repository.
