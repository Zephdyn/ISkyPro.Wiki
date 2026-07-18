# Plugin SDK v2 Quick Start

A minimal Plugin SDK v2 package contains `manifest.json` and the plugin entry file. `2.0.0-preview.4` provides preview support for C#, Python, Node.js, and Go.

## Python stdio Plugin

Directory layout:

```text
top.example.echo/
  manifest.json
  plugin.py
```

The manifest declares the stdio startup command:

```json
{
  "pluginId": "top.example.echo",
  "name": "Echo",
  "version": "0.1.0",
  "author": "Example",
  "protocolVersion": 2,
  "sdkVersion": "2.0.0-preview.4",
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

Key conventions:

- `--iskypro-stdio` enters protocol mode.
- Without that argument, the plugin should print help to stderr and exit.
- After entering protocol mode, do not write ordinary logs to stdout.
- The plugin must wait for `iskypro.initialize` and must not assume it is already authorized.

Repository samples:

- `samples/stdio-python-plugin`
- `samples/stdio-node-plugin`
- `samples/stdio-go-plugin`
- `samples/ISkyPro.SamplePlugin/EchoPluginV2.cs`

## Install from WebUI

1. Zip the plugin directory.
2. The zip root or its single top-level directory must contain `manifest.json`.
3. Open the WebUI Plugins page and switch to Modern.
4. Upload the zip.
5. To replace an installed version, enable overwrite.
6. To run immediately, enable start after install.

Installation does not execute the plugin. It only reads the zip and manifest. Stop a running plugin before updating it.
