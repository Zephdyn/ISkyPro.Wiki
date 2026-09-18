# 2.1.0-preview.3

Release date: 2026-09-19

This preview focuses on the OneBot onboarding experience: it fixes connections to sidecars that
require an access token, adds one-click local detection and connection testing, and lets the
settings page switch platforms and log out of the right one.

## 🐛 Fixes

- OneBot connection: forward WebSocket no longer fails when the sidecar (NapCat / LLOneBot and
  friends) has an `access_token` configured - the handshake now sends the standard
  `Authorization: Bearer` header and also appends the `access_token` query parameter for
  implementations that only read the query string.
- OneBot diagnostics: failures no longer report only `Unable to connect to the remote server`.
  The reason is now actionable: authentication failed, endpoint not found (wrong port), connection
  refused, host not resolvable, timeout, and so on.
- OneBot addresses: values such as `127.0.0.1:3001` without a `ws://` prefix, or with a trailing
  slash, are normalized automatically. An invalid address for a given configuration is reported once
  and retried with backoff instead of logging every few seconds.
- OneBot sending: HTTP API calls now send the standard auth header, and OneBot v12 string ids
  (`"10001"`) are accepted.
- OneBot reverse WebSocket: saving the reverse mode no longer leaves you stuck on the login page.
  The sidecar dials in to ISkyPro, so the bot QQ account is optional - it is detected from the first
  event and stored, after which the dashboard and logs use that account. The form shows the
  paste-ready endpoint.
- OneBot connection mode: the pure HTTP mode is no longer offered. It can only call APIs and never
  receives message events, which produced a confusing "enabled but no messages" state; saving it is
  now rejected with a hint to choose forward or reverse WebSocket.

## ✨ Features

- Connection test: the OneBot login and settings pages gain "Test connection". It performs a real
  handshake with the current values and calls `get_login_info`; on success the bot QQ (self_id),
  nickname and protocol version are filled in automatically, and on failure the troubleshooting
  hints are listed inline.
- Local auto-detection: "Auto-detect (local)" scans the common sidecar ports (NapCat 3001 / 3000,
  go-cqhttp 6700 and more) and applies the recommended configuration with one click. Only local
  loopback addresses are probed.
- Quick setup: the "NapCat preset" fills in `ws://127.0.0.1:3001` and `http://127.0.0.1:3000`.
  Advanced options (reconnect delay, backoff cap, reverse path, outbound interval) are collapsed by
  default, so a typical setup only needs an address and the token.
- Settings platform switching: when more than one platform is active at the same time, the settings
  page shows a platform switcher (with a single platform the panel title already says which one it
  is, so the block is hidden). Select a platform to view its settings and log out of it, and the
  OneBot panel shows online state, connection mode and message counters in a status card.

- Login layout: the card now fills the viewport height and stays fixed while the form area scrolls
  internally, so switching between QQBot and OneBot keeps the card, the artwork and the bottom
  controls in place and never adds a page scrollbar. Platform forms use two columns on wide screens,
  letting the OneBot protocol selector and address fields sit side by side with noticeably more
  content visible at once.
- OneBot form styling is unified with the rest of the UI: the protocol selector reuses the global
  control styles, the quick actions sit as buttons next to the "Connection" heading, and the test and
  detection results appear as a single status line with a state-coloured dot (dark theme colours
  fixed as well).

## ♻️ Compatibility

- Existing OneBot configuration keeps working: `ws://` / `http://` prefixes are added and redundant
  slashes removed automatically on save.
- Only a single active platform (QQBot or OneBot) is still supported; that limit is unchanged.
- Behaviour is unchanged when the sidecar has no `access_token`; if it does, enter exactly the same
  value in ISkyPro.
