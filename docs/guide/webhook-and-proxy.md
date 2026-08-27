# Webhook 与反向代理

Webhook 适合服务器部署和公网 HTTPS 回调。ISkyPro 可以监听本机 HTTP 地址，再由 Nginx、Caddy、Cloudflare Tunnel 或其他反向代理暴露到公网。

## 推荐结构

```text
QQ 平台 -> 公网 HTTPS 域名 -> 反向代理 -> ISkyPro Webhook 监听地址
```

建议：

- WebUI 管理入口保持仅本机或内网访问。
- Webhook 使用独立路径，例如 `/qqbot/webhook`。
- 公网入口必须使用 HTTPS。
- 反向代理只转发 Webhook 路径，不要把 WebUI token 登录页暴露到公网。

## 反向代理配置示例

启用独立 Webhook 端口时（默认 `0.0.0.0:5433`），Nginx 配置示例：

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

Caddy 配置示例（自动申请和维护 HTTPS 证书）：

```caddy
bot.example.com {
    reverse_proxy 127.0.0.1:5433
}
```

如果未启用独立 Webhook 端口，Webhook 监听在 WebUI 端口（默认 `5432`）上，
把上面示例中的目标地址改为 `127.0.0.1:5432`。实际监听地址以 WebUI 设置页为准。

## 在 ISkyPro 中配置

在 WebUI 设置页确认：

- Webhook 监听地址。独立 Webhook 端口默认 `http://0.0.0.0:5433`。
- 是否启用独立 Webhook 端口。
- Webhook 路径，默认 `/qqbot/webhook`。
- 反向代理生成器中的公网域名。

保存监听地址、端口或路径后，需要重启 ISkyPro 才会让当前进程使用新网络入口。

## 回调签名验证

QQ 平台回调请求携带 `X-Signature-Ed25519` 和 `X-Signature-Timestamp` 请求头。
ISkyPro 使用 Bot 登录时填写的 Secret 验证签名，签名无效的请求会被拒绝并在日志中
记录警告。排查签名失败时，确认 Bot 登录填写的 Secret 与 QQ 平台回调配置使用的
密钥一致。

## 在 QQ 平台中配置

在 QQ 机器人管理后台中填写公网回调地址，例如：

```text
https://bot.example.com/qqbot/webhook
```

然后勾选需要接收的事件。群消息通常需要 `GROUP_MESSAGE_CREATE`。如果没有勾选事件，Webhook 地址可达也不会收到对应消息。

## 排查顺序

1. 访问公网 URL，确认反向代理能到达 ISkyPro。
2. 检查 HTTPS 证书有效。
3. 检查 QQ 平台回调地址是否与 WebUI 中显示的路径一致。
4. 检查签名或密钥配置。
5. 检查 QQ 平台事件勾选。
6. 检查 WebUI 框架日志和 Bot 会话日志。
