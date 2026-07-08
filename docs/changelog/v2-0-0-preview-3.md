# 2.0.0-preview.3

发布日期：2026-07-08

## Features

- about: 更新下载流程新增可见进度，显示状态、文件名、已下载大小、总大小和百分比。
- plugins: 插件页新增旧插件 / 新插件分栏、数量徽标和选择记忆。
- plugins: 新插件页支持上传 zip 安装 `stdio-jsonrpc` 插件包，并展示真实 runtime 状态、PID、退出码、队列指标和 settings 入口。
- gateway: WebSocket 业务事件接收与插件分发、QQBot HTTP 发送解耦，慢插件或慢发送不会阻塞网关读取循环。

## Plugin Development

- sdk: 新增 Plugin SDK v2 草案，覆盖静态 manifest、`stdio-jsonrpc`、JSON-RPC 2.0 长度帧、initialize、typed + raw 事件和 `messageReference`。
- sdk: 新增 C# SDK v2 原型和 `EchoPluginV2` 示例。
- sdk: 新增 Python、Node.js 和 Go 最小 SDK 与 stdio 样例。
- sdk: SDK 方法 stub 可从 QQBot API catalog 生成 C#、Python、Node.js 和 Go 输出。
- modern-plugin: Main 支持扫描 `plugins-v2/*/manifest.json`、托管 stdio 插件进程、token 鉴权、权限校验、事件投递、stderr / `log.write` 日志和崩溃恢复。
- modern-plugin: 新插件支持 settings schema 自动表单，secret 字段不会回显真实值。

## Compatibility

- legacy-plugin: 旧 DLL 插件顺序分发、返回值拦截、x86 PluginHost 隔离和拖拽排序不受新插件分栏影响。
- sdk: Plugin SDK v2 仍是 preview API。Java 等更多语言 SDK、稳定 API 命名冻结和更完整的真实插件兼容测试仍属于后续工作。

## Docs

- Wiki 新增快速开始、QQBot 事件配置、Webhook 与反向代理、插件 SDK、FAQ 和 SDK 下载页面。
- WebUI 关于页新增 GitHub 仓库入口。
- Wiki 首页图片压缩，继续使用 `/assets/yuki.png` 路径。
