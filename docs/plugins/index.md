# 插件 SDK

ISkyPro 同时支持 **ISky v1 插件（*EPL* / x86）** 和 **ISkyPro v2+ 插件**。

v1 插件兼容原 ISky 框架 v1 ABI，继续放在 `plugin/` 目录，由独立 x86 `isky.exe` 宿主隔离运行。v2 插件面向跨语言和跨平台路线，使用 `plugins-v2/` 静态 manifest、`stdio-jsonrpc` 或 HTTP transport，并由 Main 管理生命周期。

Linux x64 包只支持 v2 插件路线。ISky v1 插件依赖 Windows/x86 兼容宿主和 `message.dll`，需要使用 Windows 包。

## 阅读指引

- 已有 ISky / *EPL* DLL 插件用户：先看 [ISky v1 与 ISkyPro v2+ 插件](/plugins/v1-vs-v2)。
- 开发 v2 插件：从 [快速实现](/plugins/sdk-quick-start) 开始。
- 发布 stdio ZIP 或部署 HTTP 插件服务：看 [发布插件](/plugins/publishing)。
- 启动失败或收不到事件：看 [故障排查](/plugins/troubleshooting)。
- 获取 SDK 包：看 [SDK 下载](/plugins/downloads)。

Plugin SDK v2 从 `2.0.0` 起进入稳定支持。公共 SDK 源码位于 [ISkyPro.Wiki 仓库](https://github.com/Zephdyn/ISkyPro.Wiki)的 `sdk/`，可运行样例位于 `samples/`。
