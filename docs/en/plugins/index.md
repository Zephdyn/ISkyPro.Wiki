# Plugin SDK

ISkyPro supports both **ISky v1 plugins (*EPL* / x86)** and **ISkyPro v2+ plugins**.

ISky v1 plugins are compatible with the original ISky framework v1 ABI. They continue to live in `plugin/` and run in the isolated x86 `isky.exe` host. ISkyPro v2+ plugins provide the cross-language and cross-platform path through static manifests under `plugins-v2/`, `stdio-jsonrpc` or HTTP transport, and lifecycle management from Main.

Linux x64 packages support only the v2 plugin path. ISky v1 plugins depend on the Windows/x86 compatibility host and `message.dll`, so they require a Windows package.

## What to Read

- Existing ISky / *EPL* DLL plugin users: start with [ISky v1 and ISkyPro v2+ Plugins](/en/plugins/v1-vs-v2).
- ISkyPro v2+ plugin authors: start with [Quick Start](/en/plugins/sdk-quick-start).
- stdio ZIP publishers and HTTP service operators: read [Publishing Plugins](/en/plugins/publishing).
- Startup failures or missing events: read [Troubleshooting](/en/plugins/troubleshooting).
- SDK packages: read [SDK Downloads](/en/plugins/downloads).

Plugin SDK v2 is stable starting with `2.0.0`. Public SDK sources are under `sdk/` in the [ISkyPro.Wiki repository](https://github.com/Zephdyn/ISkyPro.Wiki), with runnable examples under `samples/`.
