# 2.0.0-preview.1

Release date: 2026-07-05

## Breaking Changes

- runtime: The main application moved to .NET 10 and ASP.NET Core, so startup, publishing, and deployment differ from the legacy ISky host.
- package: The main application is published for win-x64 / win-arm64, while the legacy DLL plugin host is published separately as a win-x86 process.
- plugin-host: Legacy DLL plugins no longer load inside the main process; they run in an isolated host supervised by the main application.
- api: This is a preview release, and `ISkyPro.Contracts` plus `ISkyPro.PluginSdk` may still receive compatibility changes before the stable release.

## Features

- dashboard: Added a runtime overview for gateway status, message counters, plugin count, and recent framework logs.
- bot: Added QQBot credential verification, WebSocket / Webhook mode selection, mention filtering, and bot logout.
- plugins: Added legacy DLL plugin scan, upload, enable, disable, restart, settings, and uninstall actions.
- plugins: Added a bounded FIFO serial request queue for legacy plugin hosts, plus a dispatch queue capacity setting, so concurrent message dispatch waits instead of failing immediately.
- logs: Added framework logs, plugin logs, and Bot conversation logs with search, pagination, and cleanup.
- settings: Added WebUI access controls, Webhook listen address settings, reverse-proxy config generation, log retention, and debug tools.
- about: Added an About entry at the bottom of the sidebar for the current version and changelog.
- about: Update checks now detect release packages for the current system and can download, replace program files, and restart after WebUI confirmation while preserving user directories such as `config`, `data`, and `plugin`.

## Plugin Development

- contracts: Added `ISkyPro.Contracts` for shared IPC, Bot, and plugin-host contract types.
- sdk: Added `ISkyPro.PluginSdk` for modern C# plugin development.
- sample: Added `ISkyPro.SamplePlugin` as a modern plugin example.
- debug: Added a WebUI simulated-message action for validating modern plugin dispatch without sending messages to QQ.
- native: Added the `message.dll` compatibility layer to preserve the legacy plugin ABI entry path.
- native: Refactored the internal `message.dll` implementation into auth, queue, JSON payload, member query, stats, and return-buffer modules, with added concurrency, wake-up, and member-query tests.

## Compatibility

- plugin: Kept legacy DLL plugin start, stop, restart, enable, disable, settings window, and uninstall paths.
- plugin: Kept the legacy plugin call path through `message.dll`.
- plugin: Kept lifecycle and message callbacks serialized for each legacy DLL; concurrent entry points now queue or time out instead of hitting the single pending request failure.
- host: Kept Windows x86 compatibility for the legacy plugin host.
- native: Kept `message.dll` exported functions, calling conventions, and legacy plugin-visible return semantics unchanged.
- state: Plugin enabled state, crash counts, consecutive-crash disabling, and restart windows are managed by the main application.

## Docs

- changelog: Added root `CHANGELOG.md` as the primary release-note source.
- i18n: Added English changelog source `CHANGELOG.en-US.md`; the WebUI displays changelog content for the active language.
- webui: The WebUI build reads changelog files and uses the latest entry as the current version.
- release: Future releases should continue adding new version entries at the top of this file.
