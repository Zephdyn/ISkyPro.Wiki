# 2.1.0-preview.1

Release date: 2026-08-25

The first 2.1.0 preview adds an OneBot platform gateway and an optional platform-selectable login UI; the SDK simultaneously gains image sending and other capabilities (see the SDK changelog below).

## ✨ Features

- onebot: Added an OneBot platform gateway that runs alongside QQBot, supporting OneBot v11 / v12 message protocols, forward and reverse WebSocket, CQ-code rich media (images, mentions, replies, voice), and outbound message throttling.
- gateway: The gateway page can now report configuration, online state, and receive/send counts per platform for QQBot and OneBot. OneBot and QQBot are independent and can be enabled separately.
- webui: The login page lets you choose the QQBot or OneBot configuration per platform, and the frontend now uses a modular platform architecture.
- webui: The plugin page is now a unified card list with a separate Install plugins tab. v1 and v2+ plugins share one list with search, type/status filters, and a detail panel; v2+ zip installs, v1 DLL uploads, and HTTP plugin registration are separate entries to avoid mistakes.

## ⚙️ Configuration

- onebot: Enable OneBot forward/reverse WebSocket from the WebUI or a config file, and configure reverse connection `access_token` validation and reconnect backoff limits.

## ♻️ Compatibility

- onebot: The new platform gateway does not affect the existing QQBot gateway; `IBotGateway` (simulation/state interface) remains QQBot as the default implementation.

## 📦 SDK Changelog

Plugin SDK v2 changes in this version (image sending, message recall, platform-neutral messaging, group-management catalog) are detailed in the [SDK changelog](/en/changelog/sdk/v2-1-0-preview-1.md).
