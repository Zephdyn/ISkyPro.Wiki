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

## Reverse Proxy Examples

With a separate Webhook port enabled (default `0.0.0.0:5433`), an Nginx example:

```nginx
server {
    listen 443 ssl;
    server_name bot.example.com;

    ssl_certificate     /path/to/fullchain.pem;
    ssl_certificate_key /path/to/privkey.pem;

    location /qqbot/webhook {
        proxy_pass http://127.0.0.1:5433;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

A Caddy example (automatically obtains and renews the HTTPS certificate):

```caddy
bot.example.com {
    reverse_proxy 127.0.0.1:5433
}
```

If the separate Webhook port is not enabled, the Webhook listens on the WebUI
port (default `5432`); change the target address in the examples to
`127.0.0.1:5432`. Confirm the actual listen address in the WebUI settings page.

## Configure ISkyPro

In the WebUI settings page, confirm:

- Webhook listen address. The separate Webhook port defaults to `http://0.0.0.0:5433`.
- Whether a separate Webhook port is enabled.
- Webhook path, which defaults to `/qqbot/webhook`.
- Public domain in the reverse-proxy generator.

After saving a listen address, port, or path change, restart ISkyPro so the current process uses the new network entry point.

## Callback Signature Verification

QQ platform callbacks carry the `X-Signature-Ed25519` and `X-Signature-Timestamp`
request headers. ISkyPro verifies the signature with the Secret entered at Bot
login; requests with invalid signatures are rejected and logged as warnings. When
debugging signature failures, confirm that the Secret used at Bot login matches
the key configured for the callback on the QQ platform.

## Configure QQ Platform

Enter the public callback address in the QQ bot console, for example:

```text
https://bot.example.com/qqbot/webhook
```

Then select the events you need to receive. Group messages require at least `GROUP_MESSAGE_CREATE`. If events are not selected, a reachable Webhook address still will not receive those messages.

## Troubleshooting Order

1. Visit the public URL and confirm the reverse proxy can reach ISkyPro.
2. Check that the HTTPS certificate is valid.
3. Check whether the QQ platform callback URL matches the path shown in the WebUI.
4. Check signature or secret configuration.
5. Check QQ platform event selections.
6. Check WebUI framework logs and Bot conversation logs.
