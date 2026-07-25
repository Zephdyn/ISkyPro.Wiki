# ISkyPro.PluginSdk

Official C# Plugin SDK v2 contracts and `stdio-jsonrpc` runtime for ISkyPro.

```csharp
if (!args.Contains("--iskypro-stdio", StringComparer.Ordinal))
{
    return 2;
}

await StdioPluginV2Host.RunAsync(new MyPlugin());
return 0;
```

Implement `IISkyProPluginV2`, keep one static `manifest.json` beside the plugin,
and reserve stdout for protocol frames. `StdioPluginV2Host` loads that manifest
from the working directory or application output directory. Application logs
must use stderr or `IISkyProPluginV2Context.WriteLogAsync`.

Generated catalog methods return `JsonElement` results and expose permission,
stability, request/response model, and default-enabled metadata. The runtime
supports initialize negotiation, bounded concurrent event dispatch,
bidirectional JSON-RPC request multiplexing, request timeouts, runtime-token
injection, event ACKs, and graceful shutdown.
