# 2.0.0-preview.3

发布日期：2026-07-08

## ✨ Features

- about: 更新下载流程新增可见进度，显示状态、文件名、已下载大小、总大小和百分比；总大小未知时显示不确定进度。
- about: 自动安装现在先完成 WebUI 可见下载，再触发安装接口；下载失败或取消时不会继续进入安装步骤。
- about: 关于页新增 GitHub 仓库入口，保留文档、Release 和更新日志入口。
- plugins: 插件页新增“旧插件 / 新插件”选项卡、数量徽标和选择记忆；首次打开时优先显示插件数量更多的一类。
- plugins: 新插件页展示 Plugin SDK v2 插件的协议版本、transport、权限、命令数量、Bot 绑定、队列指标、最近错误、HTTP 注册和日志入口。
- gateway: WebSocket 业务事件接收与插件分发、QQBot HTTP 发送解耦，网关读取循环不会再等待慢插件或慢发送链路。
- gateway: 新增业务事件队列和 outbound 发送队列指标，包括排队、处理中、完成、丢弃、失败、限流和最近错误。

## 🧩 Plugin Development

- sdk: 新增 Plugin SDK v2 协议草案，覆盖静态 manifest、`stdio-jsonrpc` transport、JSON-RPC 2.0 长度帧、初始化握手、typed + raw 事件和 `messageReference`。
- sdk: 新增 `ISkyPro.PluginSdk.V2` C# 原型接口与 `EchoPluginV2` 示例，覆盖 typed 字段、`rawPayload`、延迟回复、非消息类 SDK 方法和日志。
- sdk: 新增 Python / Node.js Plugin SDK v2 最小包和 `stdio-jsonrpc` 跨语言样例，封装 `Content-Length` framing、initialize、事件 ACK、`invoke`、`log.write` 和被动文本回复。
- sdk: 新增 Go Plugin SDK v2 最小 module 和 `samples/stdio-go-plugin`，封装 `stdio-jsonrpc` framing、initialize、事件 ACK、`context.Context`、`Invoke`、`LogWrite` 和 `ReplyText`。
- sdk: 新增 QQBot API catalog stub 生成器，可从 `qqbot-api.catalog.json` 生成 C#、Python、Node.js 和 Go SDK method stubs，并用测试校验生成输出与 catalog 同步。
- qqbot-api: 新增 QQBot API v2 方法目录和规范 OpenAPI client 原型，核心覆盖消息、媒体、用户、群、频道、成员和权限接口。
- modern-plugin: 现代插件调度新增 observer / command / filter 模式、快速 ACK、插件专属有界队列、命令路由、过滤器超时和分发指标。
- modern-plugin: Main 新增 `plugins-v2/*/manifest.json` 静态发现；有效 Plugin SDK v2 包会以 `stopped` 状态进入 API 快照，无效 manifest 会以 `invalidManifest` 和校验错误暴露，扫描阶段不会执行未知进程。
- modern-plugin: 新插件快照新增 stdio 运行时状态字段，包括 state、processId、startedAt、lastExitAt、exitCode、crashCount、nextRestartAt、permissions、settingsAvailability 和 packageDirectory。
- modern-plugin: 新增 `stdio-jsonrpc` JSON-RPC client 和受管进程启动路径；用户触发 start 时会按 manifest 启动插件、发送 `iskypro.initialize`、校验 pluginId/protocol/encoding/capabilities，并把成功握手的进程绑定为运行中 client。
- modern-plugin: stdout 现在作为严格协议通道处理，普通 stdout 日志会被判定为协议污染并拒绝启动；stderr 已按行采集到框架插件日志。
- modern-plugin: `stdio-jsonrpc` 插件新增真实 stop/restart/shutdown lifecycle；stop 会发送 `plugin.stop`，短超时未退出时 kill 进程树，用户 stop/disable/shutdown 不会触发自动重启。
- modern-plugin: `stdio-jsonrpc` 插件异常退出会进入 `restartPending`，复用旧插件崩溃窗口、延迟和连续崩溃禁用阈值；到期后台服务会自动重新启动并重新执行 initialize。
- modern-plugin: 新增 Plugin SDK v2 runtime token 服务；token 绑定 pluginId、instanceId 和进程生命周期，通过 initialize 传入插件，stop、crash 和 restart 会使旧 token 失效并重新签发。
- modern-plugin: 新增 SDK API dispatcher，调用 `log.write`、`users.getCurrentBot` 和 catalog 方法前会校验 token、pluginId 归属、manifest 权限和 catalog defaultEnabled 状态；`unsafe.rawOpenApi` 默认继续由全局风险开关拒绝。
- modern-plugin: `stdio-jsonrpc` 插件现在可接收 `events.message` JSON-RPC 事件；Main 会将 `ModernPluginMessageEvent` 映射为 Plugin SDK v2 event envelope，并将 `PluginSdkV2EventAck` 映射回分发结果。
- modern-plugin: `stdio-jsonrpc` runtime 现在可处理插件主动发起的 SDK API JSON-RPC request / notification，复用 runtime token、manifest 权限和 API catalog dispatcher。
- modern-plugin: 事件 ACK 超时、进程退出时的 pending request 和插件拒绝 ACK 会更新 timeout/failure 指标；stderr 与 `log.write` 插件日志加入 4096 字符截断，避免异常日志撑爆记录。
- modern-plugin: Plugin SDK v2 新增 settings schema API 和 WebUI 自动表单，支持 string、number、boolean、select、path、secret 字段，配置保存到插件隔离目录，secret 字段不回显且留空保存会保留旧值。
- modern-plugin: 新插件页现在展示真实 stdio runtime 状态、PID、exit code、超时指标和 settings 入口；`settings.pageUrl` 继续只允许 loopback HTTP URL。
- modern-plugin: 新插件页新增本地 zip 上传安装入口，支持覆盖已安装的 `stdio-jsonrpc` 插件、安装后启动、安装后扫描刷新，并在后端拒绝路径穿越、无效 manifest、非 stdio transport 和运行中更新。
- modern-plugin: `unsafe.rawOpenApi` 作为默认关闭的实验入口保留，需要显式权限。

## ♻️ Compatibility

- legacy-plugin: 旧 DLL 插件的顺序分发、返回值拦截、x86 PluginHost 隔离和拖拽排序不受新插件分栏影响。
- modern-plugin: HTTP 现代插件控制端点目前提供注册表级启用、禁用、移除和状态刷新；`stdio-jsonrpc` 静态发现、zip 安装、启动、停止、重启、崩溃恢复、token 鉴权、SDK API 权限校验、事件投递、日志观测、settings schema、Python / Node.js / Go 最小 SDK 和生成器已接入，Java 等 SDK 与稳定 API 命名冻结仍属于后续工作。

## 📝 Docs

- docs: 更新 `docs/reference/update-check.md`，记录下载任务进度接口、状态字段和 WebUI 下载/安装衔接。
- docs: 更新 `docs/reference/new-plugin-sdk.md`，记录 Plugin SDK v2 协议、事件模型、权限模型、SDK 方法命名和语言覆盖状态。
- docs: 更新 `docs/designs/pluginhost-message-dispatch.md`，记录新旧插件分发模型、网关业务队列和现代插件队列差异。
- docs: Wiki 新增快速开始、QQBot 事件配置、Webhook 与反向代理、插件 SDK、FAQ、SDK 下载和 preview.3 changelog 页面，支持中英文 i18n 站点；首页新增 GitHub 仓库入口。
- docs: Wiki 首页 `/assets/yuki.png` 压缩到 256 KB 以下，并保持原路径。
