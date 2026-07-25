# ISkyPro Plugin SDK v2 for Go

The module path is:

```text
github.com/Zephdyn/ISkyPro.Wiki/sdk/go/iskypro/v2
```

Create a `StdioJsonRpcPlugin`, pass a `context.Context` to `Run`, and reserve
stdout for JSON-RPC frames. Logs must go to stderr or `PluginContext.LogWrite`.

Handlers receive `*MessageContext`. Reply with structured parts:

```go
_, err := message.Reply(
    ctx,
    iskypro.AtUserRef(message.Sender),
    iskypro.Text(" hello"),
)
```

Use `pluginContext.Messages.Group(id).Send(...)` for proactive messages.
Use `message.ReplyMarkdown(...)` or `SendMarkdown(...)` for QQ group Markdown
messages. Mentions remain structured and Main sends `msg_type = 2` with
`markdown.content`.

## Package a stdio plugin

The runnable sample builds a native executable and packages it with a generated
release manifest:

```powershell
Set-Location samples\stdio-go-plugin
go run ./tools/package-plugin
```

Use `-goos` and `-goarch` for cross-compilation. The ZIP is written under
`artifacts/`; target machines do not need Go because the package contains the
compiled binary.
