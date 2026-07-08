# Publishing Plugins

Plugin SDK v2 local install packages use zip files. Each package must contain a static `manifest.json` and plugin runtime resources.

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

- `pluginId` must be stable. Do not change it casually when updating versions.
- Use `2` for `protocolVersion`.
- The first local package installation path accepts only `stdio-jsonrpc`.
- `transport.stdio.args` should include `--iskypro-stdio`.
- Declare only permissions that are actually needed.
- Declare fields under `settings.configSchema` when a settings form is needed.

## Upload Install

Upload the zip from the WebUI Modern plugins tab. The installer:

- Rejects path traversal entries.
- Reads only the manifest and does not execute the plugin.
- Validates the manifest.
- Rejects overwriting a running plugin.
- Keeps a backup of the previous version during update.

Plugin data/config is not deleted by default. Data deletion should be an explicit user action, not part of a normal update.

## Release Notes

The release page or README should state:

- Supported ISkyPro versions.
- Why each plugin permission is needed.
- Required runtime, such as Python, Node.js, or Go.
- Configuration fields.
- Common errors and log locations.
