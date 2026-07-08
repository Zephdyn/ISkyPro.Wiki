# Plugin SDK

ISkyPro supports both legacy DLL plugins and new Plugin SDK v2 plugins.

Legacy plugins target the existing ISky / E-language ecosystem. They continue to live in `plugin/` and run in the isolated x86 `isky.exe` host. New plugins target a cross-language and cross-platform path. They use static manifests under `plugins-v2/`, `stdio-jsonrpc` or HTTP transport, and lifecycle management from Main.

Linux preview packages support only the modern plugin path. Legacy DLL plugins depend on the Windows/x86 compatibility host and `message.dll`, so they require a Windows package.

## What to Read

- Existing DLL plugin users: start with [Legacy and Modern Plugins](/en/plugins/legacy-vs-modern).
- New plugin authors: start with [Quick Start](/en/plugins/sdk-quick-start).
- Plugin zip publishers: read [Publishing Plugins](/en/plugins/publishing).
- Startup failures or missing events: read [Troubleshooting](/en/plugins/troubleshooting).
- SDK packages: read [SDK Downloads](/en/plugins/downloads).

Plugin SDK v2 is still a preview API in `2.0.0-preview.3`. It is suitable for early validation of stdio plugins and multilingual samples, but the final stable interface is not frozen.
