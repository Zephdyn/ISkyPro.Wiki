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

## 在 ISkyPro 中配置

在 WebUI 设置页确认：

- Webhook 监听地址。
- 是否启用独立 Webhook 端口。
- Webhook 路径。
- 反向代理生成器中的公网域名。

保存监听地址、端口或路径后，需要重启 ISkyPro 才会让当前进程使用新网络入口。

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

服务模式不会自动弹浏览器。服务启动后请从服务日志、控制台输出或配置文件中确认 WebUI 地址，并使用有效访问 token 登录。
