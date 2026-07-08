# Plugin Troubleshooting

## Modern Plugin Not Found

- The zip root or its single top-level directory does not contain `manifest.json`.
- The manifest is not valid JSON.
- `pluginId`, `name`, `version`, `author`, or `sdkVersion` is missing.
- `transport.type` is not `stdio-jsonrpc`.
- The plugin was copied to the legacy `plugin/` directory instead of installed through the Modern plugin upload entry.

## Startup Failure

- `transport.stdio.command` does not exist or is not in `PATH`.
- `workingDirectory` points to the wrong location.
- Python / Node.js / Go is not installed.
- The plugin did not enter protocol mode with `--iskypro-stdio`.
- The initialize response has a different `pluginId`, protocol version, or encoding than the manifest.

## stdout Protocol Pollution

stdio plugin stdout may contain only JSON-RPC `Content-Length` frames. Ordinary logs must go to stderr or `log.write`.

Common mistakes:

```text
print("hello")
console.log("hello")
fmt.Println("hello")
```

These all pollute stdout. Write them to stderr instead.

## ACK Timeout

Return an ACK quickly after receiving an event. For slow HTTP calls, database work, or long tasks, ACK first, then continue asynchronously inside the plugin and call SDK methods to reply.

## Missing Permission

SDK API calls are checked against manifest `permissions`. `messages.replyText` requires `messages.reply`; reading the current bot profile requires `users.read`.

## Reply Failure

- The event has no usable `messageReference`.
- The reply is outside the platform's allowed window.
- The Bot has no permission for the target conversation.
- The plugin declared the permission, but the Bot platform side does not have the corresponding capability.
- The send queue or platform API returned an error; check WebUI logs.

## Missing Group Messages

Start with [QQBot Event Setup](/en/guide/qqbot-events). Focus on the group full-message switch, `GROUP_MESSAGE_CREATE` event selection, connection mode, and plugin logs.
