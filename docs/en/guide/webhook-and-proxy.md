# Webhook and Reverse Proxy

Webhook is suitable for server deployments and public HTTPS callbacks. ISkyPro can listen on a local HTTP address and be exposed through Nginx, Caddy, Cloudflare Tunnel, or another reverse proxy.

## Recommended Shape

```text
QQ platform -> public HTTPS domain -> reverse proxy -> ISkyPro Webhook listen address
```

Recommendations:

- Keep the WebUI management entry local or intranet-only.
- Use a dedicated Webhook path, such as `/qqbot/webhook`.
- The public entry must use HTTPS.
- Forward only the Webhook path through the reverse proxy. Do not expose the WebUI token login page publicly.

## Configure ISkyPro

In the WebUI settings page, confirm:

- Webhook listen address.
- Whether a separate Webhook port is enabled.
- Webhook path.
- Public domain in the reverse-proxy generator.

After saving a listen address, port, or path change, restart ISkyPro so the current process uses the new network entry point.

## Configure QQ Platform

Enter the public callback address in the QQ bot console, for example:

```text
https://bot.example.com/qqbot/webhook
```

Then select the events you need to receive. Group messages usually require `GROUP_MESSAGE_CREATE`. If events are not selected, a reachable Webhook address still will not receive those messages.

## Troubleshooting Order

1. Visit the public URL and confirm the reverse proxy can reach ISkyPro.
2. Check that the HTTPS certificate is valid.
3. Check whether the QQ platform callback URL matches the path shown in the WebUI.
4. Check signature or secret configuration.
5. Check QQ platform event selections.
6. Check WebUI framework logs and Bot conversation logs.

Service mode does not open a browser automatically. After the service starts, confirm the WebUI address from service logs, console output, or configuration files, then log in with a valid access token.
