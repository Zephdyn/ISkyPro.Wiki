# Plugin SDK v2 Quick Start

Stable release `2.0.0` officially supports C#, Python, Node.js, and Go. Choose a deployment model first:

| Mode | Best for | ISkyPro integration |
| --- | --- | --- |
| `stdio-jsonrpc` | Local plugins, full Plugin SDK v2 APIs, and lifecycle management by ISkyPro | Publish an installable ZIP containing `manifest.json`, then upload it from WebUI |
| HTTP | Existing web applications, containers, or remote services with independently managed lifecycle | Deploy the service and register its base URL; no ZIP upload |

These are not two command-line modes of one package. A stdio plugin is a local managed process; an HTTP plugin is an independently running service.

## Python stdio Plugin

Directory layout:

```text
top.example.echo/
  manifest.json
  plugin.py
```

The manifest declares the stdio startup command:

```json
{
  "pluginId": "top.example.echo",
  "name": "Echo",
  "version": "0.1.0",
  "author": "Example",
  "protocolVersion": 2,
  "sdkVersion": "2.0.0",
  "transport": {
    "type": "stdio-jsonrpc",
    "stdio": {
      "command": "python",
      "args": ["plugin.py", "--iskypro-stdio"],
      "workingDirectory": "."
    }
  },
  "supportedPlatforms": [{ "platform": "windows" }],
  "eventSubscriptions": [{ "eventType": "message.created" }],
  "permissions": ["messages.reply"],
  "commands": [{ "name": "echo", "prefixes": ["/"], "priority": 10 }]
}
```

Key conventions:

- `--iskypro-stdio` enters protocol mode.
- Without that argument, the plugin should print help to stderr and exit.
- After entering protocol mode, do not write ordinary logs to stdout.
- The plugin must wait for `iskypro.initialize` and must not assume it is already authorized.

Repository samples:

- `samples/stdio-python-plugin`
- `samples/stdio-node-plugin`
- `samples/stdio-go-plugin`
- `samples/ISkyPro.SamplePlugin/EchoPluginV2.cs`

The canonical SDK source is under `sdk/` in this public repository. After changing
the API catalog, run `python tools/plugin-sdk-stub-generator/generate.py` to replace
the generated method surfaces for all four languages.

## Package stdio plugins

Do not simply zip the source directory. A release ZIP must contain the entry
point, runtime dependencies, and `manifest.json` at its root. Repository samples
provide runnable packaging commands.

### C#

After referencing the `ISkyPro.PluginSdk` NuGet package and placing
`manifest.json` in the project directory, a normal publish creates the ZIP:

```powershell
dotnet publish .\MyPlugin.csproj -c Release
```

Default output:

```text
artifacts/<AssemblyName>.zip
```

See `samples/ISkyPro.SamplePlugin`. The publish target includes DLLs,
`.deps.json`, `.runtimeconfig.json`, SDK dependencies, and the manifest.

### Python

```powershell
Set-Location samples\stdio-python-plugin
python package.py
```

The standard-library-only script creates the ZIP and vendors
`iskypro_sdk_v2`. The target machine still needs a compatible `python` command.

### Node.js

```powershell
Set-Location samples\stdio-node-plugin
npm run package:plugin
```

`node package-plugin.mjs` is equivalent. The script uses only Node.js built-ins
and vendors `@iskypro/plugin-sdk-v2` under `node_modules`. The target machine
still needs a compatible `node` command.

### Go

```powershell
Set-Location samples\stdio-go-plugin
go run ./tools/package-plugin
```

Cross-compilation examples:

```powershell
go run ./tools/package-plugin -goos linux -goarch amd64
go run ./tools/package-plugin -goos windows -goarch arm64
```

Go packages contain a compiled native executable, so target machines do not
need Go. Produce a separate ZIP for every target OS and architecture.

## Install from WebUI

1. Use the language packaging command above to create an installable ZIP. A custom pipeline must also include the entry point, dependencies, and root `manifest.json`.
2. The zip root or its single top-level directory must contain `manifest.json`.
3. Open the WebUI Plugins page and switch to Modern.
4. Upload the zip.
5. To replace an installed version, enable overwrite.
6. To run immediately, enable start after install.

Installation does not execute the plugin. It only reads the zip and manifest. Stop a running plugin before updating it.

## Deploy HTTP plugins

HTTP plugins are not uploaded through the local ZIP installer. Deploy the plugin
as a normal web service first, then register its address with ISkyPro.

The service must expose:

```text
GET  /iskypro/plugin/manifest
POST /iskypro/plugin/events/message
```

Minimal manifest response:

```json
{
  "pluginId": "top.example.http",
  "name": "HTTP Example",
  "version": "0.1.0",
  "author": "Example",
  "protocolVersion": 2,
  "capabilities": 9
}
```

`9` means `ReceiveMessages | HttpTransport`. The message endpoint receives a
`ModernPluginMessageEvent` and returns:

```json
{
  "accepted": true,
  "intercepted": false,
  "outboundMessages": [],
  "error": null
}
```

After deployment, enter a base URL such as `http://127.0.0.1:5080` under
**Plugins > Modern > Register HTTP plugin**. ISkyPro fetches and validates the
manifest before registration.

The plugin developer owns the HTTP service process, dependencies, TLS,
authentication, logs, updates, and availability. The current HTTP transport uses
the HTTP modern-plugin request/response contract; it does not use the stdio
runtime token or the local ZIP lifecycle manager.
