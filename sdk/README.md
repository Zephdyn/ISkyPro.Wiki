# ISkyPro Plugin SDK v2

This directory is the canonical public source for the ISkyPro Plugin SDK v2.
The private application repository consumes these files through the Wiki Git
submodule; it does not keep a second SDK copy.

## Layout

- `catalog/qqbot-api.catalog.json`: single source for generated SDK methods and permissions.
- `csharp/ISkyPro.Contracts`: public protocol and plugin DTOs.
- `csharp/ISkyPro.PluginSdk`: C# SDK and stdio runtime.
- `python`: Python package.
- `node/iskypro-sdk-v2`: Node.js package and TypeScript declarations.
- `go/iskypro`: Go package. Its v2 module path ends in `/v2` as required by Go Modules.

Runnable examples are under [`../samples`](../samples). `Directory.Build.props`
at the repository root is the SDK version source. After changing the catalog or
version, run:

```text
python tools/plugin-sdk-stub-generator/generate.py
python tools/plugin-sdk-stub-generator/generate.py --check
dotnet build SDK.sln
```

Generated files should be replaced by the generator, not edited method by method.
