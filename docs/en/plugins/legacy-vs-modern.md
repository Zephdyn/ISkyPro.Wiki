# Legacy and Modern Plugins

## Legacy DLL Plugins

Legacy plugins continue to use the `plugin/` directory:

```text
ISkyPro/
  plugin/
    ExamplePlugin.dll
```

Characteristics:

- Suitable for existing ISky / E-language DLL plugins.
- Run in the independent x86 `isky.exe` host.
- Message callbacks remain serialized for each legacy plugin, preserving legacy return-value interception semantics.
- The WebUI can scan, upload, enable, disable, restart, sort, open settings, and uninstall them.
- The legacy plugin ABI and `message.dll` compatibility layer remain available.

## Modern Plugins

Modern plugins use `plugins-v2/`:

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
- The WebUI Modern tab can install zip packages, show status, start, stop, restart, disable, uninstall, and open settings.

## Which One to Use

- Existing DLL plugin: keep using the legacy plugin entry.
- New plugin: prefer Plugin SDK v2.
- Python / Node.js / Go or cross-platform plugin: use the modern plugin path.
- Legacy interception-chain semantics: use a legacy plugin, or explicitly design command / filter behavior in a modern plugin.
