# ISkyPro Plugin SDK v2 for Go

The module path is:

```text
github.com/Zephdyn/ISkyPro.Wiki/sdk/go/iskypro/v2
```

Create a `StdioJsonRpcPlugin`, pass a `context.Context` to `Run`, and reserve
stdout for JSON-RPC frames. Logs must go to stderr or `PluginContext.LogWrite`.
