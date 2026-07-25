# ISky v1 与 ISkyPro v2+ 插件

ISkyPro 使用版本化术语区分两套插件体系：

- **ISky v1 插件（*EPL* / x86）**：兼容原 ISky 框架 v1 ABI 的 *EPL* DLL 插件。
- **ISkyPro v2+ 插件**：从 ISkyPro 2.0 起提供，使用 Plugin SDK v2、manifest 和 stdio-jsonrpc / HTTP transport。

下文分别简称“v1 插件”和“v2 插件”。

## ISky v1 插件

v1 插件继续使用 `plugin/` 目录：

```text
ISkyPro/
  plugin/
    ExamplePlugin.dll
```

特点：

- 适合已有 ISky 框架 v1 / 易语言 DLL 插件。
- 由独立 `isky.exe` x86 宿主运行。
- 同一 v1 插件的消息回调保持串行，保留 v1 返回值拦截语义。
- WebUI 可扫描、上传、启用、禁用、重启、调整排序、打开设置和卸载。
- v1 插件 ABI 和 `message.dll` 兼容层继续保留。

ISky v1 插件仅支持 Windows 包。Linux x64 包不包含 `isky.exe`、`message.dll` 或 v1 插件 ABI 兼容层。

## ISkyPro v2+ 插件

本地 stdio v2 插件使用 `plugins-v2/`：

```text
ISkyPro/
  plugins-v2/
    top.example.echo/
      manifest.json
      plugin.py
```

特点：

- 使用静态 `manifest.json` 发现插件。
- 默认推荐本机 `stdio-jsonrpc`。
- stdout 只允许 JSON-RPC 协议帧，普通日志写 stderr 或 `log.write`。
- 插件进程由 Main 启动、停止、重启和监控。
- 支持 typed + raw 事件、`messageReference` 延迟回复、权限声明和 settings schema。
- WebUI v2 插件页可安装 zip、查看状态、启动、停止、重启、禁用、卸载和打开设置。

HTTP v2 插件不放入 `plugins-v2/`，也不上传 ZIP。它作为独立 Web 服务运行，通过
`GET /iskypro/plugin/manifest` 和 `POST /iskypro/plugin/events/message` 接入，用户只在
WebUI 中注册 Base URL。HTTP 服务生命周期由其自身部署环境管理。

## 什么时候选哪种

- 已有 ISky / *EPL* x86 DLL 插件：继续使用 v1 插件入口。
- 新开发插件：优先使用 ISkyPro v2+ 插件体系。
- 需要 Linux、跨平台或 Python / Node.js / Go：使用 v2 插件。
- 需要 v1 插件拦截链语义：使用 v1 插件，或在 v2 插件中显式设计 command / filter。
