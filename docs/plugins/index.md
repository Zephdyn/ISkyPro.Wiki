# 插件 SDK

ISkyPro 同时支持旧 DLL 插件和新的 Plugin SDK v2 插件。

旧插件面向既有 ISky / 易语言生态，继续放在 `plugin/` 目录，由独立 x86 `isky.exe` 宿主隔离运行。新插件面向跨语言和跨平台路线，使用 `plugins-v2/` 静态 manifest、`stdio-jsonrpc` 或 HTTP transport，并由 Main 管理生命周期。

Linux preview 包只支持新插件路线。旧 DLL 插件依赖 Windows/x86 兼容宿主和 `message.dll`，需要使用 Windows 包。

## 读什么

- 已有 DLL 插件用户：先看 [旧插件与新插件](/plugins/legacy-vs-modern)。
- 想写新插件：从 [快速实现](/plugins/sdk-quick-start) 开始。
- 想发布 stdio ZIP 或部署 HTTP 插件服务：看 [发布插件](/plugins/publishing)。
- 启动失败或收不到事件：看 [故障排查](/plugins/troubleshooting)。
- 需要 SDK 包：看 [SDK 下载](/plugins/downloads)。

Plugin SDK v2 从 `2.0.0` 起进入稳定支持。公共 SDK 源码位于本仓库 `sdk/`，样例位于 `samples/`。
