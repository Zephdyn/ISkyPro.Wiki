# 2.0.0-preview.3

Release date: 2026-07-08

## ✨ Features

- about: Update downloads now show visible progress with status, file name, downloaded bytes, total bytes, and percent; unknown total size uses an indeterminate progress state.
- about: Automatic installation now completes the visible WebUI download before calling the install endpoint; failed or cancelled downloads no longer continue into installation.
- about: The install confirmation dialog now closes after confirmation instead of covering the progress bar, and the progress card includes a cancel button for an in-progress update download.
- about: Update cache cleanup now removes cancelled `.download` partial files and clears consumed or leftover packages, one-time installer scripts, and staging directories after successful installs and on application startup, preventing `data/updates` from accumulating one package per version.
- about: Automatic installation supports Linux `.tar.gz` / `.tgz` main application packages; the Linux update script preserves user directories, replaces WebUI assets, and restarts `ISkyPro`.
- about: The About page now links to the GitHub repository while keeping the docs, Release, and changelog links.
- webui: The sidebar version badge and About current-version field now read the backend runtime version, so temporary test versions and post-update restarts no longer depend on the WebUI-bundled changelog version.
- webui: Automatically opened WebUI URLs now include the current application version, and `index.html` plus update API responses use no-cache/no-store headers to avoid showing stale frontend assets after an update restart.
- package: Added the Linux x64 preview application package `ISkyPro-<version>-linux-x64.tar.gz`; after extraction, users can run `ISkyPro` directly. Linux packages enable invariant globalization, do not require an additional ICU install, and exclude the Windows legacy plugin host, `message.dll`, and Windows Service scripts.
- plugins: Added Legacy / Modern plugin tabs with count badges and remembered selection; the first load chooses the larger plugin set.
- plugins: The Modern plugins tab shows Plugin SDK v2 protocol version, transport, permissions, command count, bot binding, queue metrics, recent errors, HTTP registration, and log entry points.
- gateway: WebSocket business-event receive is decoupled from plugin dispatch and QQBot HTTP sends, so the gateway read loop no longer waits on slow plugins or slow sends.
- gateway: Added business-event queue and outbound-send queue metrics for queued, in-flight, completed, dropped, failed, rate-limited, and last-error states.
- runtime: Non-Windows runtimes now protect WebUI tokens, Bot Secrets, and other runtime credentials with a local AES-GCM key stored at `data/iskypro.runtime.key`, which is preserved with user data.

## 🧩 Plugin Development

- sdk: Added the Plugin SDK v2 protocol draft covering static manifests, `stdio-jsonrpc` transport, JSON-RPC 2.0 length framing, initialize handshake, typed + raw events, and `messageReference`.
- sdk: Added `ISkyPro.PluginSdk.V2` C# prototype interfaces and `EchoPluginV2`, covering typed fields, `rawPayload`, delayed replies, a non-message SDK method, and plugin logging.
- sdk: Added Python / Node.js Plugin SDK v2 minimal packages and `stdio-jsonrpc` cross-language samples that wrap `Content-Length` framing, initialize, event ACK, `invoke`, `log.write`, and passive text replies.
- sdk: Added a Go Plugin SDK v2 minimal module and `samples/stdio-go-plugin`, wrapping `stdio-jsonrpc` framing, initialize, event ACK, `context.Context`, `Invoke`, `LogWrite`, and `ReplyText`.
- sdk: Added a QQBot API catalog stub generator that emits C#, Python, Node.js, and Go SDK method stubs from `qqbot-api.catalog.json`, with tests that verify generated output stays in sync with the catalog.
- qqbot-api: Added a QQBot API v2 method catalog and normalized OpenAPI client prototype covering core message, media, user, group, channel, member, and permission endpoints.
- modern-plugin: Modern plugin dispatch now supports observer / command / filter modes, fast ACK, per-plugin bounded queues, command routing, filter timeout, and dispatch metrics.
- modern-plugin: Main now statically discovers `plugins-v2/*/manifest.json`; valid Plugin SDK v2 packages appear in API snapshots as `stopped`, invalid manifests appear as `invalidManifest` with validation errors, and discovery does not execute unknown processes.
- modern-plugin: Modern plugin snapshots now include stdio runtime status fields such as state, processId, startedAt, lastExitAt, exitCode, crashCount, nextRestartAt, permissions, settingsAvailability, and packageDirectory.
- modern-plugin: Added a `stdio-jsonrpc` JSON-RPC client and managed process start path; user-triggered start now launches the manifest command, sends `iskypro.initialize`, validates pluginId/protocol/encoding/capabilities, and binds the successful process as a running client.
- modern-plugin: stdout is now treated as a strict protocol channel, so ordinary stdout logs are rejected as protocol pollution; stderr is collected line by line into framework plugin logs.
- modern-plugin: Added real stop/restart/shutdown lifecycle for `stdio-jsonrpc` plugins; stop sends `plugin.stop`, kills the process tree after a short timeout, and user stop/disable/shutdown exits do not trigger automatic restart.
- modern-plugin: Unexpected `stdio-jsonrpc` exits now enter `restartPending` and reuse the legacy plugin crash window, delay, and consecutive-crash disable thresholds; a due-restart background service starts the plugin and reruns initialize.
- modern-plugin: Added a Plugin SDK v2 runtime token service; tokens are bound to pluginId, instanceId, and process lifetime, delivered through initialize, and invalidated on stop, crash, and restart before issuing a new token.
- modern-plugin: Added an SDK API dispatcher that validates token, pluginId ownership, manifest permissions, and catalog defaultEnabled state before `log.write`, `users.getCurrentBot`, and catalog methods; `unsafe.rawOpenApi` remains rejected by the global risk switch by default.
- modern-plugin: `stdio-jsonrpc` plugins now receive `events.message` JSON-RPC events; Main maps `ModernPluginMessageEvent` to a Plugin SDK v2 event envelope and maps `PluginSdkV2EventAck` back into dispatch results.
- modern-plugin: The `stdio-jsonrpc` runtime now handles plugin-originated SDK API JSON-RPC requests / notifications, reusing runtime tokens, manifest permissions, and the API catalog dispatcher.
- modern-plugin: Event ACK timeouts, pending requests during process exit, and plugin-rejected ACKs now update timeout/failure metrics; stderr and `log.write` plugin logs are truncated to 4096 characters.
- modern-plugin: Added Plugin SDK v2 settings schema APIs and a WebUI auto form for string, number, boolean, select, path, and secret fields; config is saved in the plugin-isolated config directory, secret values are not echoed, and blank secret saves preserve the previous value.
- modern-plugin: The Modern plugins tab now shows real stdio runtime state, PID, exit code, timeout metrics, and settings entry points; `settings.pageUrl` remains limited to loopback HTTP URLs.
- modern-plugin: The Modern plugins tab now includes local zip upload installation for `stdio-jsonrpc` plugins, with overwrite, start-after-install, post-install scan refresh, and backend rejection for path traversal, invalid manifests, non-stdio transports, and updates while running.
- modern-plugin: `unsafe.rawOpenApi` remains available only as a disabled-by-default experimental entry that requires explicit permission.

## ♻️ Compatibility

- legacy-plugin: Legacy DLL plugin ordered dispatch, return-value interception, x86 PluginHost isolation, and drag sorting are unaffected by the new plugin tabs.
- linux: Linux preview packages support the main process, WebUI, QQBot gateway, and Plugin SDK v2 modern plugins; legacy DLL plugins, `isky.exe`, and the `message.dll` compatibility layer remain Windows-package only.
- modern-plugin: HTTP modern plugin controls currently provide registry-level enable, disable, remove, and state refresh behavior; `stdio-jsonrpc` static discovery, zip install, start, stop, restart, crash recovery, token auth, SDK API permission checks, event delivery, log observability, settings schema, Python / Node.js / Go minimal SDKs, and generators are wired, while Java SDKs and stable API naming remain future work.

## 📝 Docs

- docs: Updated `docs/reference/update-check.md` with download task progress endpoints, status fields, and WebUI download/install flow.
- docs: Updated Wiki quick start, FAQ, and SDK download pages with Linux preview package startup, systemd long-running deployment, Linux auto-update behavior, and the Windows-only boundary for legacy plugins.
- docs: Updated `docs/reference/new-plugin-sdk.md` with the Plugin SDK v2 protocol, event model, permission model, SDK method naming, and language coverage status.
- docs: Updated `docs/designs/pluginhost-message-dispatch.md` with the legacy/modern dispatch model, gateway business queue, and modern plugin queue differences.
- docs: The Wiki now includes quick start, QQBot event setup, Webhook and reverse proxy, Plugin SDK, FAQ, SDK downloads, and preview.3 changelog pages, with Chinese/English i18n support; the homepage now links to GitHub.
- docs: Compressed the Wiki homepage `/assets/yuki.png` below 256 KB while keeping the same path.
- release: Added a reusable version-release prompt covering Windows / Linux application packages, SDK packages, Wiki Release upload, and post-release verification.
