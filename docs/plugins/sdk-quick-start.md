# Plugin SDK v2 快速实现

Plugin SDK v2 的最小插件包由 `manifest.json` 和插件入口文件组成。`2.0.0-preview.3` 提供 C#、Python、Node.js 和 Go 预览能力。

## Python stdio 插件

目录结构：

```text
top.example.echo/
  manifest.json
  plugin.py
```

manifest 中声明 stdio 启动方式：

```json
{
  "pluginId": "top.example.echo",
  "name": "Echo",
  "version": "0.1.0",
  "author": "Example",
  "protocolVersion": 2,
  "sdkVersion": "2.0.0-preview.3",
  "transport": {
    "type": "stdio-jsonrpc",
    "stdio": {
      "command": "python",
      "args": ["plugin.py", "--iskypro-stdio"],
      "workingDirectory": "."
    }
  },
  "supportedPlatforms": [{ "platform": "windows" }],
  "eventSubscriptions": [{ "eventType": "message.created" }],
  "permissions": ["messages.reply"],
  "commands": [{ "name": "echo", "prefixes": ["/"], "priority": 10 }]
}
```

关键约定：

- `--iskypro-stdio` 表示进入协议模式。
- 没有该参数时，插件应向 stderr 打印帮助并退出。
- 进入协议模式后，不要向 stdout 写普通日志。
- 插件必须等待 `iskypro.initialize`，不要自行假定已经授权。

仓库内可直接参考：

- `samples/stdio-python-plugin`
- `samples/stdio-node-plugin`
- `samples/stdio-go-plugin`
- `samples/ISkyPro.SamplePlugin/EchoPluginV2.cs`

## 安装到 WebUI

1. 将插件目录打成 zip。
2. zip 根目录或唯一顶层目录下必须有 `manifest.json`。
3. 打开 WebUI 插件页，切到“新插件”。
4. 上传 zip。
5. 如需覆盖旧版本，勾选覆盖已安装插件。
6. 需要立即运行时，勾选安装后启动。

安装阶段不会执行插件，只读取 zip 和 manifest。运行中的插件更新前需要先停止。
