# FAQ

## Where can I find the WebUI token?

When `ISkyPro.exe` or the Linux package's `ISkyPro` starts for the first time, the terminal prints the WebUI address and access token. Service mode does not open a browser automatically, so check service logs or startup output.

## Should I use WebSocket or Webhook?

Use WebSocket for local use or when you do not have a public HTTPS callback address. Use Webhook when you have configured a public callback URL on the QQ platform or the deployment is intended to receive public HTTPS callbacks. See [QQBot Event Setup](/en/guide/qqbot-events) and [Webhook and Reverse Proxy](/en/guide/webhook-and-proxy) for detailed setup and troubleshooting.

## What does "Unable to connect to the remote server" mean for OneBot?

Check, in order:

1. **Ports swapped**: in NapCat `3001` is the forward WebSocket and `3000` is the HTTP API. The ISkyPro "WebSocket URL" should be `ws://127.0.0.1:3001`.
2. **Forward WebSocket not enabled**: if only the HTTP server is enabled in NapCat, the WebSocket connection is refused.
3. **Access Token mismatch**: when the sidecar sets a token, the exact same value must be entered in ISkyPro, otherwise the handshake returns 401/403.
4. **Sidecar not running**: "connection refused" usually means nothing is listening on that port.

"Test connection" on the login or settings page reports the concrete reason, and "Auto-detect (local)" scans the ports and fills in a recommended configuration. See [Getting Started](/en/guide/getting-started#connecting-the-onebot-platform).

## Why did login succeed but group messages are missing?

Common causes are a disabled group full-message switch, missing `GROUP_MESSAGE_CREATE` selection for Webhook, a mismatch between connection mode and QQ platform configuration, or a plugin that does not handle the event.

## Where do I get Bot ID / AppID and Secret?

Get them from the QQ Open Platform bot console. Do not copy Secrets from third-party plugins, documentation screenshots, or configuration snippets sent by others.

## What should the Webhook callback URL be?

Use a public HTTPS address whose path matches the ISkyPro Webhook setting, for example `https://bot.example.com/qqbot/webhook`.

## What is the difference between ISky v1 and ISkyPro v2+ plugins?

ISky v1 plugins are *EPL* / x86 DLL plugins compatible with the original ISky framework v1 ABI. They are placed in `plugin/` and run by the x86 compatibility host. ISkyPro v2+ plugins use Plugin SDK v2 and connect through static manifests plus stdio-jsonrpc or HTTP transport; local stdio plugins are installed under `plugins-v2/`.

## Does Linux support ISky v1 plugins?

No. Linux x64 packages support the main process, WebUI, QQBot gateway, and ISkyPro v2+ plugins. ISky v1 plugins depend on the Windows/x86 compatibility host and `message.dll`, so they still require a Windows package.

## Why do ISky v1 plugins need a Windows/x86 compatibility host?

ISky v1 plugins depend on the original ISky framework DLL ABI and a 32-bit runtime environment. ISkyPro isolates them in an independent x86 host instead of loading them directly into the main process.

## Why did a newly uploaded ISkyPro v2+ plugin not start?

Successful installation does not always start the plugin. Confirm that start after install was selected during upload, or start it manually from the plugin list page. When updating a running plugin, ISkyPro stops the old version and resumes it after replacement; no manual stop is needed.

## Does update-check failure affect runtime?

No. Update-check failure only affects newer-version prompts and download entries in the WebUI About page. Bot and plugin runtime do not depend on it.

## Why does service mode not open a browser automatically?

Windows Services and Linux systemd services run in a background session and cannot open the browser like a desktop process. Open the WebUI address manually and enter the access token.

## How do I access the WebUI from another machine?

By default the WebUI listens on the loopback address only. Remote access requires both:

1. Set `ISkyPro:WebUI:Url` to a routable listen address, for example `http://0.0.0.0:5432`.
2. Enable remote access: set `ISkyPro:WebUI:AllowRemote` to `true`, or enable remote access in the WebUI settings.

Restart ISkyPro, open `http://<server-address>:5432`, and enter the access token printed at startup. Always put remote access behind an HTTPS reverse proxy; do not expose the WebUI login page directly to the public internet.

## How do I upgrade ISkyPro?

Older versions can detect and download the new release from the update entry on the WebUI About page. You can also download the new release package manually and replace the old files while keeping the `config`, `data`, and plugin directories. Review the compatibility notes in the [changelog](/en/changelog/) before upgrading. SDKs are downloaded separately as `SDK-V2-<Language>-<version>.zip`; SDK archives are not used to install or update the main application.
