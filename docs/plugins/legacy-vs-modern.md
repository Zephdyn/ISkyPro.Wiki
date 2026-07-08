# 旧插件与新插件

## 旧 DLL 插件

旧插件继续使用 `plugin/` 目录：

```text
ISkyPro/
  plugin/
    ExamplePlugin.dll
```

特点：

- 适合已有 ISky / 易语言 DLL 插件。
- 由独立 `isky.exe` x86 宿主运行。
- 同一旧插件消息回调保持串行，保留旧返回值拦截语义。
- WebUI 可扫描、上传、启用、禁用、重启、调整排序、打开设置和卸载。
- 旧插件 ABI 和 `message.dll` 兼容层继续保留。

## 新插件

新插件使用 `plugins-v2/`：

```text
ISkyPro/
  plugins-v2/
    top.example.echo/
      manifest.json
      plugin.py
```

特点：

- 使用静态 `manifest.json` 发现插件。
- 默认推荐本机 `stdio-jsonrpc`。
- stdout 只允许 JSON-RPC 协议帧，普通日志写 stderr 或 `log.write`。
- 插件进程由 Main 启动、停止、重启和监控。
- 支持 typed + raw 事件、`messageReference` 延迟回复、权限声明和 settings schema。
- WebUI 新插件页可安装 zip、查看状态、启动、停止、重启、禁用、卸载和打开设置。

## 什么时候选哪种

- 已有 DLL 插件：继续用旧插件入口。
- 新写插件：优先用 Plugin SDK v2。
- 需要跨平台或 Python / Node.js / Go：使用新插件。
- 需要旧插件拦截链语义：使用旧插件，或在新插件中显式设计 command / filter。
