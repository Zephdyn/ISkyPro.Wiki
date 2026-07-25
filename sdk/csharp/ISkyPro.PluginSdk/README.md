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

Message handlers receive a bound `MessageContext`:

```csharp
await message.ReplyAsync(At.User(message.Sender), " hello");
await message.ReplyMarkdownAsync(At.User(message.Sender), " **hello**");

await context.Messages
    .Group(groupOpenId)
    .SendAsync("proactive message");
```

Implement `IISkyProPluginV2`, keep one static `manifest.json` beside the plugin,
and reserve stdout for protocol frames. `StdioPluginV2Host` loads that manifest
from the working directory or application output directory. Application logs
must use stderr or `IISkyProPluginV2Context.WriteLogAsync`.

`MessageContext.ReplyAsync`, `ReplyMarkdownAsync`, and `context.Messages` use
structured text/mention parts; SDK code never emits QQ markup. Markdown is
currently supported for QQ group targets and is sent as `msg_type = 2` with
`markdown.content`. Generated low-level catalog methods return
`JsonElement` results and expose permission,
stability, request/response model, and default-enabled metadata. The runtime
supports initialize negotiation, bounded concurrent event dispatch,
bidirectional JSON-RPC request multiplexing, request timeouts, runtime-token
injection, event ACKs, and graceful shutdown.

## Publish an installable stdio plugin

The NuGet package includes a transitive MSBuild target. When the plugin project
contains `manifest.json`, a normal publish also creates the WebUI-installable ZIP:

```powershell
dotnet publish .\MyPlugin.csproj -c Release
```

The default archive is `artifacts/<AssemblyName>.zip` under the plugin project.
It contains the publish output and `manifest.json` at the ZIP root.

Optional MSBuild properties:

```xml
<PropertyGroup>
  <ISkyProPackagePluginOnPublish>true</ISkyProPackagePluginOnPublish>
  <ISkyProPluginManifest>path/to/manifest.json</ISkyProPluginManifest>
  <ISkyProPluginPackagePath>path/to/MyPlugin.zip</ISkyProPluginPackagePath>
</PropertyGroup>
```

Set `ISkyProPackagePluginOnPublish` to `false` when publish output should not be
packaged. Source checkouts using a direct project reference can import
`buildTransitive/ISkyPro.PluginSdk.targets`; the C# sample demonstrates this.
