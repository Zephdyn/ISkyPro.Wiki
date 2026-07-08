# 发布插件

Plugin SDK v2 本地安装包使用 zip。包内必须包含静态 `manifest.json` 和插件运行资源。

支持两种结构：

```text
manifest.json
plugin.py
README.md
```

或：

```text
top.example.echo/
  manifest.json
  plugin.py
  README.md
```

安装后会规范化为：

```text
plugins-v2/
  top.example.echo/
    manifest.json
    plugin.py
```

## manifest 要点

- `pluginId` 必须稳定，更新版本时不要随意更改。
- `protocolVersion` 使用 `2`。
- 本地包安装第一阶段只接受 `stdio-jsonrpc`。
- `transport.stdio.args` 建议包含 `--iskypro-stdio`。
- `permissions` 只声明实际需要的权限。
- 需要设置表单时，在 `settings.configSchema` 中声明字段。

## 上传安装

在 WebUI 新插件页上传 zip。安装器会：

- 拒绝路径穿越条目。
- 只读取 manifest，不执行插件。
- 校验 manifest。
- 拒绝覆盖运行中的插件。
- 更新时保留旧版本备份。

默认不删除插件的 data/config。删除数据应由用户明确执行，不应作为普通更新的一部分。

## 发布说明

发布页或 README 中建议写清：

- 支持的 ISkyPro 版本。
- 插件权限用途。
- 需要的运行时，例如 Python、Node.js 或 Go。
- 配置项说明。
- 常见错误和日志位置。
