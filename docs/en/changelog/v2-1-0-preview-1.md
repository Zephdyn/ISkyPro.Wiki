# 2.1.0-preview.1

Release date: 2026-08-25

The first 2.1.0 preview adds an OneBot platform gateway, an optional platform-selectable login UI, and local image sending through the Plugin SDK v2 structured messages.

## ✨ Features

- onebot: Added an OneBot platform gateway that runs alongside QQBot, supporting OneBot v11 / v12 message protocols, forward and reverse WebSocket, CQ-code rich media (images, mentions, replies, voice), and outbound message throttling.
- gateway: The gateway page can now report configuration, online state, and receive/send counts per platform for QQBot and OneBot. OneBot and QQBot are independent and can be enabled separately.
- webui: The login page lets you choose the QQBot or OneBot configuration per platform, and the frontend now uses a modular platform architecture.
- webui: The plugin page is now a unified card list with a separate Install plugins tab. v1 and v2+ plugins share one list with search, type/status filters, and a detail panel; v2+ zip installs, v1 DLL uploads, and HTTP plugin registration are separate entries to avoid mistakes.

## 🧩 Plugin Development

- sdk: Added image sending to structured messages. The C#, Python, Node.js, and Go SDKs provide `Image.FromFile`, `image()`, `image(filePath)`, and `Image(filePath)` respectively; pass a local image path on the machine running ISkyPro to send a group or private-chat image (rich media `msg_type = 7`). A message allows at most one image and cannot be combined with Markdown; send additional images as separate messages.
- sdk: Added a remote-image `image-url` part (`Image.FromUrl`, `image_url`, `imageUrl`, `ImageUrl`). When the image has a public URL, Main uses the official URL upload flow (`file_type=1` + `url` + `srv_send_msg=false` → `file_info` → rich media), supported for group and C2C targets only. The local base64 `file_data` upload path stays as a compatibility capability and is not migrated.
- sdk: Added message recall. The four SDKs expose `RecallAsync` / `recall()` / `recall(messageId)` / `Recall()` to recall messages the bot sent itself (QQ: messages older than 2 minutes cannot be recalled; text-channel/direct-message recall is private-domain only). The manifest must declare the `messages.recall` permission.
- sdk: Made the Plugin SDK v2 messaging path platform-neutral. `messageReference` and the `messages.send` target now support an optional `platform` field; Main adds OneBot v2 event mapping and CQ-code sending, and already-compiled C# plugins replying inside OneBot events are routed through the plugin's last event platform automatically.
- sdk: Low-level catalog methods now forward to the real QQBot OpenAPI instead of returning placeholders. `unsafe.rawOpenApi` remains disabled by default, but can invoke arbitrary QQBot OpenAPI endpoints when the global allow switch is enabled and the plugin declares the permission.
- sdk: Structured QQ platform errors. Failures now return JSON-RPC error `data` with `errorCode` (`qq.api.<errCode>` or `qq.api.http.<status>`), `statusCode`, and `platformErrorCode`, so errors such as the 22009 message rate limit can be handled by code.
- sdk: Event attachments and mentions are now populated automatically. QQ media messages (image/video/voice/file) appear in `message.attachments` (`url`/`content_type`/`size`), and mentioned users in guild messages appear in `message.mentions`; OneBot voice/video/file segments are mapped the same way.
- sdk: Added group-management methods to the low-level catalog: bot state (`groups.getBotState`), group mute query/setup, join-request list and approval, and join-auto-approval strategies (list/create/update/delete/execute/whitelist). `groups.getGroupInfo` now uses the official path (the previous path failed).

## ⚙️ Configuration

- onebot: Enable OneBot forward/reverse WebSocket from the WebUI or a config file, and configure reverse connection `access_token` validation and reconnect backoff limits.

## ♻️ Compatibility

- onebot: The new platform gateway does not affect the existing QQBot gateway; `IBotGateway` (simulation/state interface) remains QQBot as the default implementation.
- sdk: Low-level catalog methods (`media.uploadC2CFile` and others) are now forwarded by Main to the real QQBot OpenAPI; for images, prefer the high-level `messages.send` / `messages.reply` `image` (local file) or `image-url` (remote URL) parts.
- sdk: Proactive messaging follows the current QQ official rules: enterprise-, personal-, and unverified-tier bots can all send proactive messages, with different qps/qpm and daily quotas; users can disable "allow proactive messages" in the QQ client, after which sends fail (plugins receive structured errors such as `qq.api.22009`).
