# Project Handoff

## 本轮目标

- 完成 `BOOT-100`，建立客户端与服务端最小项目骨架

## 本轮完成

- 创建根解决方案 `Overview.Uno.slnx`
- 创建 Uno Platform 客户端项目骨架 `Overview.Client/Overview.Client`
- 创建 ASP.NET Core 服务端项目骨架 `Overview.Server/`
- 创建根级 `global.json`，从仓库根目录固定 `Uno.Sdk` 版本
- 将客户端和服务端项目加入根解决方案
- 验证 `dotnet restore Overview.Uno.slnx` 通过
- 验证 `dotnet build Overview.Server/Overview.Server.csproj` 通过
- 验证 `dotnet build Overview.Client/Overview.Client/Overview.Client.csproj -f net10.0-desktop` 通过
- 修正接力文档中“git 仓库尚未初始化”的过期状态，确认当前仓库位于 `main` 分支并已配置 `origin`

## 本轮未完成

- 客户端 MVVM 分层目录
- 服务端分层目录和基础配置样例
- 五页应用壳层、底部导航、默认主页启动
- 任何业务代码实现

## 当前阻塞

- 无

## 已更新文件

- `global.json`
- `Overview.Uno.slnx`
- `Overview.Client/`
- `Overview.Server/`
- `docs/PROJECT-STATUS.md`
- `docs/PROJECT-TODO.md`
- `docs/PROJECT-HANDOFF.md`
- `docs/PROJECT-CHANGELOG.md`
- `docs/PROJECT-FILE-MAP.md`

## 下一步唯一推荐动作

- 执行 `SHELL-110`：在客户端建立 `Presentation`、`Application`、`Domain`、`Infrastructure` 目录和基础注册点；不要提前实现业务逻辑

## 接手 AI 注意事项

- 开始任何实现前，先重新阅读 `“一览”用户要求.md`
- 开始任何实现前，先检查 git 仓库状态
- 根解决方案入口是 `Overview.Uno.slnx`，不是 `.sln`
- 若从仓库根目录执行 `dotnet restore/build`，必须保留根级 `global.json`，否则 `Uno.Sdk` 无法解析
- Uno 模板还生成了 `Overview.Client/Overview.Client.sln`，当前以仓库根解决方案为主
- 不要跳过 Shell/Domain/Application 直接做页面细节
- 如果发现本地环境缺少构建所需工具链，先尝试自行安装或补齐环境，再继续任务
- 只有在尝试安装后仍无法继续时，才记录阻塞
- 每完成一个最小任务项后，要先更新状态文件，再立即创建一次包含任务 ID 的 git commit

## 若中断时优先检查的文件

1. `PROJECT-STATUS.md`
2. `PROJECT-TODO.md`
3. `PROJECT-HANDOFF.md`
