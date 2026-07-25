# C# stdio Plugin Sample

Publish the plugin with the official SDK target:

```powershell
dotnet publish .\ISkyPro.SamplePlugin.csproj -c Release
```

The publish automatically creates an installable package at:

```text
artifacts/ISkyPro.SamplePlugin.zip
```

The ZIP contains `manifest.json`, the application DLL, runtime metadata, and
all required managed dependencies. Upload it from WebUI **Plugins > ISkyPro v2+ >
Install plugin package**.

Projects that reference the `ISkyPro.PluginSdk` NuGet package receive the
packaging target automatically. Set `ISkyProPackagePluginOnPublish=false` to
disable it, or set `ISkyProPluginPackagePath` to override the ZIP path.
