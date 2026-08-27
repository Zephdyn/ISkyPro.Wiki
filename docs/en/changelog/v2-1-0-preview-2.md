# 2.1.0-preview.2

Release date: 2026-08-27

This preview focuses on multi-platform experience and a forward-looking architecture: the dashboard shows whichever platform is signed in; the current version limits you to a single active platform (QQBot or OneBot), while the account model, log fields, and plugin protocol are already designed for multiple platforms and accounts; stdio-jsonrpc internals were optimized without touching plugins or users.

## ✨ Features

- dashboard: The dashboard now renders a multi-platform account view. Any signed-in platform (QQBot or OneBot) shows its account name, avatar (QQBot uses the official platform avatar; OneBot falls back to the initial letter), platform label, online state, and receive/send counts; the metric cards follow the active platform.
- accounts: New `GET /api/bot/accounts` unified account view; bot message logs gain a `platform` field and the `transport` field semantics are unified (QQBot=connection source websocket/webhook/simulated, OneBot=connection mode reverse-ws/forward-ws/http). Existing databases migrate and backfill automatically.
- platform: Only a single platform can be active in this version. You must log out OneBot before signing in QQBot and vice versa; the WebUI confirms and logs out the current platform first (chat history is kept) and the backend rejects dual-platform configurations.
- settings: The settings page follows the active platform. While QQBot is active it shows the QQBot sections (bot config, mention filtering, Webhook callbacks and the proxy generator); while OneBot is active it shows a dedicated OneBot settings panel (protocol version, connection mode, WS/HTTP URLs, Access Token, reconnect parameters, and more) that saves and applies immediately without logging out — the panel also allows logging out of the platform.
- guide: A Webhook setup guide is now shown step by step when webhook mode is selected on the login page (also reused in the Webhook area of the settings page): prepare a public URL, confirm the listen address, enter the callback URL in the QQ Open Platform, subscribe to events, and verify the callback. OneBot gets a four-step setup guide as well.
- proxy: The reverse-proxy generator gained an IIS (URL Rewrite + ARR) template — copy the `web.config` rewrite rules for Windows Server + IIS deployments.
- plugins: Uploading a v1 plugin whose DLL name already exists no longer fails with a bare error; a replace-confirmation dialog appears, and confirming overwrites and rescans/starts the plugin — matching the v2 conflict experience.
- ux: Backend errors now carry stable error codes, and the WebUI shows localized messages in the interface language (Chinese/English).

## ⚡ Performance

- stdio: Main-side stdio-jsonrpc frame reading now scans headers inside a buffer (replacing byte-at-a-time reads), and plugin SDK calls are dispatched on a bounded pipeline so slow calls no longer block event ACKs or other SDK responses on the same connection. Wire bytes are unchanged; plugins and users see no difference. Measured frame throughput improves roughly 1.4-4.9x depending on frame size and environment.

## ♻️ Compatibility

- Log store migrates automatically: `bot_messages` gains a `platform` column backfilled from the account id prefix.
- Enabling two platforms at once is rejected (400); the current release supports one platform with one account, and multi-platform capability is designed ahead (see the development plan).

## 📦 SDK Changelog

Plugin SDK v2 changes in this version (optional `botAccountId` on message targets, stable error codes) are detailed in the [SDK changelog](/en/changelog/sdk/v2-1-0-preview-2.md).