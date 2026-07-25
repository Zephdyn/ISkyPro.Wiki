# ISky v1 and ISkyPro v2+ Plugins

ISkyPro uses versioned terminology for its two plugin systems:

- **ISky v1 plugins (*EPL* / x86)** are *EPL* DLL plugins compatible with the original ISky framework v1 ABI.
- **ISkyPro v2+ plugins** are available from ISkyPro 2.0 and use Plugin SDK v2, static manifests, and stdio-jsonrpc or HTTP transport.

The shorter names “v1 plugins” and “v2 plugins” are used below.

## ISky v1 Plugins

ISky v1 plugins continue to use the `plugin/` directory:

```text
ISkyPro/
  plugin/
    ExamplePlugin.dll
```

Characteristics:

- Suitable for existing ISky / E-language DLL plugins.
- Run in the independent x86 `isky.exe` host.
- Message callbacks remain serialized for each v1 plugin, preserving v1 return-value interception semantics.
- The WebUI can scan, upload, enable, disable, restart, sort, open settings, and uninstall them.
- The v1 plugin ABI and `message.dll` compatibility layer remain available.

ISky v1 plugins are supported only by Windows packages. Linux x64 packages do not include `isky.exe`, `message.dll`, or the v1 plugin ABI compatibility layer.

## ISkyPro v2+ Plugins

Local stdio v2 plugins use `plugins-v2/`:

```text
ISkyPro/
  plugins-v2/
    top.example.echo/
      manifest.json
      plugin.py
```

Characteristics:

- Discovered through static `manifest.json`.
- Local `stdio-jsonrpc` is the recommended default transport.
- stdout may contain only JSON-RPC protocol frames. Ordinary logs must go to stderr or `log.write`.
- The plugin process is started, stopped, restarted, and monitored by Main.
- Supports typed + raw events, delayed replies through `messageReference`, permission declarations, and settings schema.
- The WebUI v2 plugins tab can install zip packages, show status, start, stop, restart, disable, uninstall, and open settings.

HTTP v2 plugins do not live under `plugins-v2/` and are not uploaded as
ZIP files. They run as independent web services, expose
`GET /iskypro/plugin/manifest` and `POST /iskypro/plugin/events/message`, and are
registered in WebUI by base URL. Their deployment environment owns the service
lifecycle.

## Which One to Use

- Existing ISky / *EPL* x86 DLL plugin: keep using the v1 plugin entry.
- New plugin: prefer the ISkyPro v2+ plugin system.
- Linux, Python / Node.js / Go, or cross-platform plugin: use the v2 plugin path.
- v1 interception-chain semantics: use a v1 plugin, or explicitly design command / filter behavior in a v2 plugin.
