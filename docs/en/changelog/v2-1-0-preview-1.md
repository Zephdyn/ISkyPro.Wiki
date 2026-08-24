# 2.1.0-preview.1

Release date: 2026-08-25

The first 2.1.0 preview adds an OneBot platform gateway, an optional platform-selectable login UI, and local image sending through the Plugin SDK v2 structured messages.

## ✨ Features

- onebot: Added an OneBot platform gateway that runs alongside QQBot, supporting OneBot v11 / v12 message protocols, forward and reverse WebSocket, CQ-code rich media (images, mentions, replies, voice), and outbound message throttling.
- gateway: The gateway page can now report configuration, online state, and receive/send counts per platform for QQBot and OneBot. OneBot and QQBot are independent and can be enabled separately.
- webui: The login page lets you choose the QQBot or OneBot configuration per platform, and the frontend now uses a modular platform architecture.

## 🧩 Plugin Development

- sdk: Added image sending to structured messages. The C#, Python, Node.js, and Go SDKs provide `Image.FromFile`, `image()`, `image(filePath)`, and `Image(filePath)` respectively; pass a local image path on the machine running ISkyPro to send a group or private-chat image (rich media `msg_type = 7`). A message allows at most one image and cannot be combined with Markdown; send additional images as separate messages.

## ⚙️ Configuration

- onebot: Enable OneBot forward/reverse WebSocket from the WebUI or a config file, and configure reverse connection `access_token` validation and reconnect backoff limits.

## ♻️ Compatibility

- onebot: The new platform gateway does not affect the existing QQBot gateway; `IBotGateway` (simulation/state interface) remains QQBot as the default implementation.
- sdk: Low-level catalog methods such as `media.uploadC2CFile` remain authorization stubs; use the high-level `messages.send` / `messages.reply` `image` part to send images.
