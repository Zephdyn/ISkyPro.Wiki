# QQBot Event Setup

Whether ISkyPro receives messages depends on Bot login status, connection mode, QQ platform event subscriptions, group permissions, and plugin handling results. First confirm that the WebUI dashboard shows the Bot as online, then check event configuration.

## WebSocket

WebSocket mode lets ISkyPro actively connect to the QQ gateway. It is suitable for local use or deployments without a public callback URL.

Recommended setup:

- Select WebSocket on the WebUI Bot login page.
- Start with automatic private/public event subscription selection. If group messages are missing, choose manually based on the bot type.
- If QQ Open Platform already has a Webhook callback URL configured, confirm whether you are actually using WebSocket or Webhook instead of mixing both debugging paths.

## Webhook

Webhook mode lets the QQ platform send events to your public HTTPS address.

All of the following must be true:

- Select Webhook on the WebUI Bot login page.
- ISkyPro or a reverse proxy is reachable through public HTTPS.
- The QQ Open Platform callback URL points to the actual public URL.
- Required events are selected in QQ Open Platform event settings.
- Group messages usually require `GROUP_MESSAGE_CREATE`.

## Group Messages

Group messages often also need a user-side switch:

- Open the bot settings for the target group in mobile QQ.
- Enable the option that allows the bot to receive all group messages.
- Confirm the bot is still in the group and is not muted or removed.

If only mentioned messages are received, first check the group-level full-message switch and QQ platform event subscription.

## Common Checks

- Bot not logged in: verify Bot ID / Secret again in the WebUI.
- Mode mismatch: WebUI uses WebSocket while the QQ platform setup is being debugged as Webhook, or the other way around.
- Webhook unreachable: public domain, certificate, reverse proxy, or port forwarding is unavailable.
- Webhook signature failure: confirm the callback secret matches the platform configuration.
- Events not subscribed: select group, C2C, guild, or direct-message events on the QQ platform when using Webhook.
- Group full-message access disabled: enable it in the mobile QQ group bot settings.
- Plugin did not respond: check WebUI logs to see whether the event entered the framework, was intercepted by a legacy plugin, or was filtered by a modern plugin.
