# FAQ

## Where can I find the WebUI token?

When `ISkyPro.exe` or the Linux package's `ISkyPro` starts for the first time, the terminal prints the WebUI address and access token. Service mode does not open a browser automatically, so check service logs or startup output.

## Should I use WebSocket or Webhook?

Use WebSocket for local use or when you do not have a public HTTPS callback address. Use Webhook when you have configured a public callback URL on the QQ platform or the deployment is intended to receive public HTTPS callbacks.

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

Successful installation does not always start the plugin. Confirm that start after install was selected during upload, or start it manually from the plugin list page. Stop a running plugin before updating it.

## Does update-check failure affect runtime?

No. Update-check failure only affects newer-version prompts and download entries in the WebUI About page. Bot and plugin runtime do not depend on it.

## Why does service mode not open a browser automatically?

Windows Services and Linux systemd services run in a background session and cannot open the browser like a desktop process. Open the WebUI address manually and enter the access token.
