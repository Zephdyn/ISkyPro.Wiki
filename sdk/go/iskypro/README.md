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
