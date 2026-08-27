# Plugin SDK v2 Quick Start

Stable SDK `2.0.0` officially supports C#, Python, Node.js, and Go; the latest preview is `2.1.0-preview.2`. Choose a deployment model first:

| Mode | Best for | ISkyPro integration |
| --- | --- | --- |
| `stdio-jsonrpc` | Local plugins, full Plugin SDK v2 APIs, and lifecycle management by ISkyPro | Publish an installable ZIP containing `manifest.json`, then upload it from WebUI |
| HTTP | Existing web applications, containers, or remote services with independently managed lifecycle | Deploy the service and register its base URL; no ZIP upload |

`stdio-jsonrpc` and HTTP are two different deployment models, not two launch
modes of one package. A stdio plugin is a local managed process; an HTTP plugin
is an independently running service.

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

The canonical SDK source is under `sdk/` in this public repository. Version and
packaging are described under [SDK Downloads](/en/plugins/downloads), and change
history in the [SDK changelog](/en/changelog/sdk/).

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

The complete example is `samples/QQBotMarkdownRepeatPlugin` (command
`复读 hello`).

## Target Platform and Account

> The following features are available since 2.1.0-preview.2.

The proactive `messages.send` target accepts an optional `platform` parameter
that defaults to `"qqbot"`. When ISkyPro is connected to OneBot, set the target
platform explicitly to `"onebot"`:

```csharp
await context.Messages.Group("123456", "onebot").SendAsync("hello");
```

The target also accepts an optional `botAccountId` parameter (format
`"{platform}:{accountId}"`, such as `"qqbot:10001"` or `"onebot:123456"`,
compatible with the platform-side account ID) that defaults to the platform
default account. The current release supports one account per platform; this
parameter prepares for multiple platforms and accounts. Replies inside events
carry the source account automatically, so no explicit account is needed:

```csharp
// Explicit account (use the platform default or omit until multi-account is enabled)
await context.Messages.Group("123456", "onebot", "onebot:123456").SendAsync("hello");
```

When the account does not exist or the platform is not registered,
`messages.send` / `messages.reply` return the stable error codes
`message.target.account_not_found` / `message.target.platform_not_supported`.

```python
context.messages.group("123456", "onebot").send("hello")
```

```js
context.messages.group("123456", "onebot").send("hello");
```

```go
sdk.Messages.Group("123456", "onebot").Send(ctx, "hello")
```

`message.reply()` does not need a platform: Main routes by the event's
`source` / `messageReference.platform`. Even legacy C# plugins that strip the
`platform` field fall back to the platform recorded when Main last dispatched
an event to that plugin.

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
format; send additional images as separate messages. The `image` part uploads
through Main; plugins do not need to call the low-level `media.uploadC2CFile`.

## Send a remote image

When the image is not available as a local file but has a public URL, use the
`image-url` part. Main performs the official QQ URL upload flow
(`file_type=1` + `url` + `srv_send_msg=false` → `file_info` → rich media
`msg_type = 7`); only group and C2C targets are supported:

```csharp
await message.ReplyAsync(
    Image.FromUrl("https://example.com/news_banner.png"),
    " Daily news");
```

```python
await message.reply(image_url("https://example.com/news_banner.png"), " Daily news")
```

```js
await message.reply(imageUrl("https://example.com/news_banner.png"), " Daily news");
```

```go
err := message.Reply(ctx, iskypro.ImageUrl("https://example.com/news_banner.png"), iskypro.Text(" Daily news"))
```

Channel and direct-message targets do not support remote URL images yet; use a
local file path instead. The local base64 `file_data` upload path is kept as a
compatibility capability and is not migrated.

## Recall a message

A plugin can recall messages **it sent itself** (QQ limits: messages older than
2 minutes cannot be recalled; text-channel/direct-message recall works only for
private-domain bots). The manifest must declare the `messages.recall`
permission:

```csharp
await context.Messages.Group("group-openid").RecallAsync("platform-message-id");
```

```python
context.messages.group("group-openid").recall("platform-message-id")
```

```js
await context.messages.group("group-openid").recall("platform-message-id");
```

```go
err := sdk.Messages.Group("group-openid").Recall(ctx, "platform-message-id")
```

The message id is the `id` returned by the successful send response
(`MessageSendResult.MessageId`).

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
3. Open the WebUI Plugins page, go to the Install plugins tab, and pick Install v2+ plugin package.
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
