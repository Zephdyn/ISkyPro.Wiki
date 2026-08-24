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
- `samples/QQBotMarkdownRepeatPlugin`

The canonical SDK source is under `sdk/` in this public repository. After changing
the API catalog, run `python tools/plugin-sdk-stub-generator/generate.py` to replace
the generated method surfaces for all four languages.

## Markdown group messages

Structured messages can select Markdown format while keeping mentions typed, so
plugins do not need to concatenate provider markup:

```csharp
await message.ReplyMarkdownAsync(
    At.User(message.Sender),
    " **hello**");
```

Python, Node.js, and Go use `reply_markdown`, `replyMarkdown`, and
`ReplyMarkdown`, respectively. Markdown currently supports QQ group targets.
Main sends `msg_type = 2` with `markdown.content`. `<` and `>` in ordinary text
parts remain escaped; only typed mentions generate `<qqbot-at-user ... />`.

::: warning Compatibility observation (2026-07-25)
In QQ group testing on 2026-07-25, `<qqbot-at-user ... />`, `<@id>`, and `<@!id>`
were all displayed literally in ordinary `msg_type = 0` text. The current
`<qqbot-at-user ... />` form triggered an @ when sent with Markdown
`msg_type = 2` and `markdown.content`. This records server and client behavior on
that date, not a permanent compatibility guarantee. Tencent may change parsing,
so verify the [latest official group-message documentation](https://bot.q.qq.com/wiki/develop/api-v2/autogen/api/v2_groups_group_openid_messages.post.html#schema-messagemarkdown)
and the target client.
:::

The installable `samples/QQBotMarkdownRepeatPlugin` demonstrates this flow with
the `复读 hello` command.

## Send a local image

Structured messages support an `image` part for local image files on the machine
running ISkyPro (both group and private chats):

```csharp
await message.ReplyAsync(
    Image.FromFile(@"C:\news\daily_news_latest.png"),
    " Daily news");
```

Python, Node.js, and Go use `image(...)` and `Image(filePath)` respectively. Main
uploads the image to the QQ server (`v2/users/{openid}/files` for private chats,
`v2/groups/{openid}/files` for groups) and sends it as rich media `msg_type = 7`.
A message allows at most one `image` part and cannot combine it with Markdown
format; send additional images as separate messages. The `image` part does not
use the low-level `media.uploadC2CFile` authorization stub.

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

See `samples/ISkyPro.SamplePlugin` and `samples/QQBotMarkdownRepeatPlugin`. The
publish target includes DLLs, `.deps.json`, `.runtimeconfig.json`, SDK
dependencies, and the manifest.

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
3. Open the WebUI Plugins page and switch to ISkyPro v2+ Plugins.
4. Upload the zip.
5. When the same plugin ID is detected, review the old and new plugin details in the confirmation dialog before replacing it.
6. To run immediately, enable start after install.

Installation does not execute code from the new package; it only reads the zip and manifest. When an update is confirmed for a running plugin, ISkyPro stops the old version and resumes it after replacement.

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
**Plugins > ISkyPro v2+ Plugins > Register HTTP plugin**. ISkyPro fetches and validates the
manifest before registration.

The plugin developer owns the HTTP service process, dependencies, TLS,
authentication, logs, updates, and availability. The current HTTP transport uses
the HTTP modern-plugin request/response contract; it does not use the stdio
runtime token or the local ZIP lifecycle manager.
