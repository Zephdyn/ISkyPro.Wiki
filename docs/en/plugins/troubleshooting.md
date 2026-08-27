# Plugin Troubleshooting

## ISkyPro v2+ Plugin Not Found

- The zip root or its single top-level directory does not contain `manifest.json`.
- The manifest is not valid JSON.
- `pluginId`, `name`, `version`, `author`, or `sdkVersion` is missing.
- `transport.type` is not `stdio-jsonrpc`.
- The plugin was copied to the ISky v1 `plugin/` directory instead of installed through the v2 plugin upload entry.

## Startup Failure

- `transport.stdio.command` does not exist or is not in `PATH`.
- `workingDirectory` points to the wrong location.
- The target machine lacks the Python or Node.js runtime; production Go packages should contain a compiled executable instead of depending on `go run`.
- The ZIP contains only source code and omits the Python SDK, Node.js `node_modules`, C# publish dependencies, or the Go executable.
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

All of the above pollute stdout. Plugin logs should be written to stderr, or output through the SDK's `log.write`.

## ACK Timeout

Return an ACK quickly after receiving an event. For time-consuming HTTP calls,
database work, or long tasks, ACK first, then continue asynchronously inside the
plugin and call SDK methods to reply.

## Missing Permission

SDK API calls are checked against the manifest `permissions`: replying requires
`messages.reply`, active sends require `messages.send`, recalls require
`messages.recall`, and reading the current bot profile requires `users.read`. QQ
platform errors (for example 22009 message rate limit) arrive with a JSON-RPC
error `data.errorCode` (`qq.api.<errCode>`), so plugins can branch on stable
error codes. Error codes evolve with the SDK; see the
[SDK changelog](/en/changelog/sdk/).

## HTTP Plugin Registration Failure

- The HTTP service was incorrectly uploaded as a ZIP; register its base URL instead.
- The base URL is unreachable from the ISkyPro host.
- `GET /iskypro/plugin/manifest` returns an empty response, invalid JSON, or no `HttpTransport` capability.
- `POST /iskypro/plugin/events/message` is not implemented.
- A reverse proxy, TLS policy, or additional authentication blocks ISkyPro requests.

## Reply Failure

- The event has no usable `messageReference`.
- The reply is outside the platform's allowed window.
- The Bot has no permission for the target conversation.
- The plugin declared the permission, but the Bot platform side does not have the corresponding capability.
- The send queue or platform API returned an error; check WebUI logs.

## Missing Group Messages

Start with [QQBot Event Setup](/en/guide/qqbot-events). Focus on the group full-message switch, `GROUP_MESSAGE_CREATE` event selection, connection mode, and plugin logs.
