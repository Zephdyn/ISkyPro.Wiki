# Plugin SDK v2 快速实现

稳定版 `2.0.0` 正式支持 C#、Python、Node.js 和 Go。开始前先选择部署方式：

| 方式 | 适合场景 | 如何接入 ISkyPro |
| --- | --- | --- |
| `stdio-jsonrpc` | 本地插件、需要完整 Plugin SDK v2 API、希望由 ISkyPro 管理进程生命周期 | 发布为包含 `manifest.json` 的 ZIP，在 WebUI 上传 |
| HTTP | 已有 Web 服务、容器或远程服务，希望自己管理进程和扩缩容 | 部署 HTTP 服务，在 WebUI 注册 Base URL，不上传 ZIP |

两种方式不是同一个发布包的两种启动参数。stdio 插件是本地受管进程；HTTP 插件是独立运行的服务。

## Python stdio 插件

目录结构：

```text
top.example.echo/
  manifest.json
  plugin.py
```

manifest 中声明 stdio 启动方式：

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

关键约定：

- `--iskypro-stdio` 表示进入协议模式。
- 没有该参数时，插件应向 stderr 打印帮助并退出。
- 进入协议模式后，不要向 stdout 写普通日志。
- 插件必须等待 `iskypro.initialize`，不要自行假定已经授权。

仓库内可直接参考：

- `samples/stdio-python-plugin`
- `samples/stdio-node-plugin`
- `samples/stdio-go-plugin`
- `samples/ISkyPro.SamplePlugin/EchoPluginV2.cs`
- `samples/QQBotMarkdownRepeatPlugin`

SDK 唯一源码位于本公开仓库的 `sdk/`。更新 API catalog 后运行
`python tools/plugin-sdk-stub-generator/generate.py` 即可同时替换四语言生成方法。

## Markdown 群消息

结构化消息可选择 Markdown 格式，同时继续使用类型化 @，不需要插件拼接平台标签：

```csharp
await message.ReplyMarkdownAsync(
    At.User(message.Sender),
    " **你好**");
```

Python、Node.js 和 Go 对应使用 `reply_markdown`、`replyMarkdown` 和
`ReplyMarkdown`。目前仅支持 QQ 群目标；Main 会按官方群消息协议发送
`msg_type = 2` 和 `markdown.content`。普通文本片段中的 `<` / `>` 仍会转义，
只有类型化 mention 会生成 `<qqbot-at-user ... />`。

::: warning 实测兼容性记录（2026-07-25）
截至 2026-07-25 的 QQ 群实机测试，普通文本 `msg_type = 0` 中的
`<qqbot-at-user ... />`、`<@id>` 和 `<@!id>` 都会显示为普通文本；使用 Markdown
的 `msg_type = 2` 与 `markdown.content` 后，`<qqbot-at-user ... />` 可以正常触发
@。这是当日服务端和客户端行为的实测记录，不是永久兼容保证；腾讯后续可能调整解析
规则，请同时核对[最新官方群消息接口文档](https://bot.q.qq.com/wiki/develop/api-v2/autogen/api/v2_groups_group_openid_messages.post.html#schema-messagemarkdown)
与目标客户端表现。
:::

可安装的完整 @ 复读示例位于 `samples/QQBotMarkdownRepeatPlugin`，命令为
`复读 你好`。

## 指定目标平台

`messages.send` 的主动发送目标支持可选 `platform` 参数，缺省为 `"qqbot"`。
当 ISkyPro 已接入 OneBot 时，可显式把目标平台设为 `"onebot"`：

```csharp
await context.Messages.Group("123456", "onebot").SendAsync("你好");
```

```python
context.messages.group("123456", "onebot").send("你好")
```

```js
context.messages.group("123456", "onebot").send("你好");
```

```go
sdk.Messages.Group("123456", "onebot").Send(ctx, "你好")
```

`message.reply()` 不需要传平台：Main 会根据事件里的 `source` / `messageReference.platform`
自动路由；旧版 C# 插件即使剥离了 `platform` 字段，也会使用 Main 最近一次向该插件
分发事件时记录的平台。

## 发送本地图片

结构化消息支持 `image` part，可发送运行 ISkyPro 的机器上的本地图片（群聊、私聊均可）：

```csharp
await message.ReplyAsync(
    Image.FromFile(@"C:\news\daily_news_latest.png"),
    " 每日新闻");
```

Python、Node.js 和 Go 对应使用 `image(...)` 和 `Image(filePath)`。Main 会把图片上传到
QQ 服务器（私聊 `v2/users/{openid}/files`、群聊 `v2/groups/{openid}/files`），再以富媒体
`msg_type = 7` 发送。一条消息最多包含一个 `image` part，且不能与 Markdown 格式组合；
发送多张图片请拆成多条消息。`image` part 不使用低层 `media.uploadC2CFile` 授权 stub。

## 打包 stdio 插件

不要直接压缩源码目录。正式 ZIP 必须包含插件入口、运行依赖和根目录 `manifest.json`。仓库样例提供了可直接运行的打包入口：

### C#

引用 `ISkyPro.PluginSdk` NuGet 包并在项目目录放置 `manifest.json` 后，`dotnet publish` 会自动生成 ZIP：

```powershell
dotnet publish .\MyPlugin.csproj -c Release
```

默认输出：

```text
artifacts/<AssemblyName>.zip
```

完整样例：`samples/ISkyPro.SamplePlugin` 和
`samples/QQBotMarkdownRepeatPlugin`。发布目标会把 DLL、`.deps.json`、
`.runtimeconfig.json`、SDK 依赖和 manifest 一起打包。

### Python

```powershell
Set-Location samples\stdio-python-plugin
python package.py
```

脚本使用 Python 标准库生成 ZIP，并把 `iskypro_sdk_v2` 一起放进插件包。目标机器仍需提供兼容的 `python` 命令。

### Node.js

```powershell
Set-Location samples\stdio-node-plugin
npm run package:plugin
```

也可以直接运行 `node package-plugin.mjs`。脚本只使用 Node.js 内置模块，并把 `@iskypro/plugin-sdk-v2` 放入包内 `node_modules`。目标机器仍需提供兼容的 `node` 命令。

### Go

```powershell
Set-Location samples\stdio-go-plugin
go run ./tools/package-plugin
```

交叉编译示例：

```powershell
go run ./tools/package-plugin -goos linux -goarch amd64
go run ./tools/package-plugin -goos windows -goarch arm64
```

Go 包内是已经编译好的本机程序，目标机器不需要安装 Go。每个平台和架构需要分别生成 ZIP。

## 安装到 WebUI

1. 使用上面的语言打包命令生成可安装 ZIP；自定义流程也必须包含入口、依赖和根目录 `manifest.json`。
2. zip 根目录或唯一顶层目录下必须有 `manifest.json`。
3. 打开 WebUI 插件页，进入「安装插件」选项卡，选择「安装 v2+ 插件包」。
4. 上传 zip。
5. 如检测到同 ID 插件，确认框会显示新旧版本和插件信息；确认后才会覆盖。
6. 需要立即运行时，勾选安装后启动。

安装阶段不会执行新包，只读取 zip 和 manifest。确认更新运行中的插件后，框架会自动停止旧版本，
完成替换后恢复运行。

## 部署 HTTP 插件

HTTP 插件不需要、也不能通过本地插件 ZIP 上传。先把它作为普通 Web 服务部署，再向 ISkyPro 注册服务地址。

服务必须提供：

```text
GET  /iskypro/plugin/manifest
POST /iskypro/plugin/events/message
```

最小 manifest 响应：

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

`9` 表示 `ReceiveMessages | HttpTransport`。消息接口接收 `ModernPluginMessageEvent`，并返回：

```json
{
  "accepted": true,
  "intercepted": false,
  "outboundMessages": [],
  "error": null
}
```

部署完成后，在 WebUI“插件 → v2 插件 → 注册 HTTP 插件”中填写 Base URL，例如
`http://127.0.0.1:5080`。ISkyPro 会先读取 manifest；校验通过后才注册。

HTTP 服务的进程、依赖、TLS、鉴权、日志、更新和高可用由插件开发者负责。当前 HTTP transport 使用 HTTP modern-plugin request/response contract，不使用 stdio runtime token，也不通过本地 ZIP 安装器管理生命周期。
