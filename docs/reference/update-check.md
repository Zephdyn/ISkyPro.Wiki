# 更新检测约定

ISkyPro 后续的更新检测建议以 Wiki public 仓库的 GitHub Releases 作为唯一外部数据源。这样 WebUI 不需要抓取 HTML 页面，只需要读取 GitHub Releases API。

## Release 规则

- 仓库：`ISkyPro.Wiki` public 仓库
- tag：使用 `v` 前缀，例如 `v2.0.0-preview.1`
- title：使用产品名和版本号，例如 `ISkyPro 2.0.0-preview.1`
- body：写入适合 WebUI 预览的更新摘要
- changelog：body 中链接到 Wiki 的完整 changelog 页面

## 推荐检查流程

1. WebUI 或后端读取当前应用版本，例如 `2.0.0-preview.1`。
2. 请求 GitHub Releases latest 接口。
3. 去掉 tag 前缀 `v` 后，与当前版本做语义化版本比较。
4. 如果远端版本更新，则显示最新版本、release body 摘要和 changelog 链接。

GitHub Releases latest 接口格式：

```text
https://api.github.com/repos/<owner>/ISkyPro.Wiki/releases/latest
```

## 可选静态元数据

如果后续希望减少 GitHub API 依赖，也可以在 Wiki public 目录发布静态 JSON：

```text
https://<owner>.github.io/ISkyPro.Wiki/update/latest.json
```

当前仓库已经预留 `docs/public/update/latest.json`。它会随 VitePress 构建输出到 `/update/latest.json`。
