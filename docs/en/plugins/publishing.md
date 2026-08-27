# Publishing Plugins

The transport determines the release model:

| Transport | Release artifact | User installation |
| --- | --- | --- |
| `stdio-jsonrpc` | A runnable plugin ZIP | Upload from WebUI; ISkyPro manages start, stop, and restart |
| HTTP | A deployed and continuously running HTTP service | Register its base URL; no ZIP upload |

## Publish stdio-jsonrpc plugins

Local install packages use ZIP files. Each package must contain a static
`manifest.json`, the plugin entry point, and all runtime dependencies.

Supported layouts:

```text
manifest.json
plugin.py
README.md
```

or:

```text
top.example.echo/
  manifest.json
  plugin.py
  README.md
```

After installation, the package is normalized to:

```text
plugins-v2/
  top.example.echo/
    manifest.json
    plugin.py
```

## Manifest Notes

- `pluginId` must be stable. Do not change it when updating versions.
- Use `2` for `protocolVersion`.
- Local ZIP installation currently accepts only `stdio-jsonrpc`; HTTP plugins are not installed as ZIPs.
- `transport.stdio.args` should include `--iskypro-stdio`.
- Declare only permissions that are actually needed.
- Declare fields under `settings.configSchema` when a settings form is needed.

## Recommended commands by language

### C#

The official NuGet package includes a publish target. When `manifest.json`
exists in the project directory:

```powershell
dotnet publish .\MyPlugin.csproj -c Release
```

The default archive is `artifacts/<AssemblyName>.zip`. Override it when needed:

```xml
<PropertyGroup>
  <ISkyProPackagePluginOnPublish>true</ISkyProPackagePluginOnPublish>
  <ISkyProPluginManifest>manifest.json</ISkyProPluginManifest>
  <ISkyProPluginPackagePath>artifacts/MyPlugin.zip</ISkyProPluginPackagePath>
</PropertyGroup>
```

### Python

See `samples/stdio-python-plugin/package.py`. The package must include
`iskypro_sdk_v2` and any third-party dependencies; it must not depend on paths in
the SDK source checkout.

### Node.js

See `samples/stdio-node-plugin/package-plugin.mjs` and run
`npm run package:plugin`. A production package should contain the required
`node_modules`; do not assume a globally installed SDK package.

### Go

See `samples/stdio-go-plugin/tools/package-plugin` and run
`go run ./tools/package-plugin`. Build every target OS/architecture separately.
The packaged manifest must launch the compiled executable, not `go run`.

## Upload Install

Upload the ZIP from the WebUI “v2 plugins” tab. The installer:

- Rejects path traversal entries.
- Reads only the manifest and does not execute the plugin.
- Validates the manifest.
- Reports a conflict when the same plugin ID is installed again; the user confirms before the update is applied.
- Stops a running plugin before updating it and resumes it after replacement.
- Keeps a backup of the previous version during update.

Plugin data/config is not deleted by default. Data deletion should be an explicit user action, not part of a normal update.

## Publish HTTP plugins

HTTP plugins do not produce an ISkyPro installation ZIP. Deploy the service to a
server, container platform, or local service manager, and ensure ISkyPro can
reach its base URL.

Required endpoints:

- `GET /iskypro/plugin/manifest`: returns `ModernPluginManifest` with the
  `ReceiveMessages` and `HttpTransport` capabilities.
- `POST /iskypro/plugin/events/message`: receives message events and returns
  `ModernPluginMessageResponse`.

Users register the base URL from the WebUI “v2 plugins” page. New releases are
rolled out or restarted by the service deployment system, not through ISkyPro's
ZIP replacement flow.

HTTP release documentation should also describe:

- Service URL and network reachability.
- HTTPS, reverse proxy, or additional authentication requirements.
- Timeout, concurrency, and retry behavior.
- Logs and health-check endpoints.
- Update, rollback, and availability strategy.

## Release Notes

The release page or README should state:

- Supported ISkyPro versions.
- Why each plugin permission is needed.
- Required runtime, such as Python, Node.js, or Go.
- Configuration fields.
- Common errors and log locations (see [Troubleshooting](/en/plugins/troubleshooting)).
- The transport type and whether users should upload a ZIP or register a base URL.
