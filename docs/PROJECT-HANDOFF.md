# Project Handoff

## 本轮目标

- 完成 `SHELL-110`，建立客户端 MVVM 分层目录和基础注册点

## 本轮完成

- 在客户端项目中创建 `Presentation`、`Application`、`Domain`、`Infrastructure` 目录
- 将默认 `MainPage` 迁移至 `Presentation/Pages/MainPage.xaml`
- 新增 `Presentation/ViewModels/MainViewModel.cs`
- 新增基础注册点 `Application/DependencyInjection/ClientServiceRegistry.cs`
- 新增 `DomainAssemblyMarker.cs` 与 `InfrastructureAssemblyMarker.cs` 作为分层落点
- 更新 `App.xaml.cs`，使应用启动时初始化轻量注册中心并解析页面 ViewModel
- 验证 `dotnet build Overview.Client/Overview.Client/Overview.Client.csproj -f net10.0-desktop` 通过，0 warning / 0 error

## 本轮未完成

- 服务端分层目录和基础配置样例
- 五页应用壳层、底部导航、默认主页启动
- 任何业务代码实现

## 当前阻塞

- 无

## 已更新文件

- `Overview.Client/Overview.Client/App.xaml.cs`
- `Overview.Client/Overview.Client/Application/DependencyInjection/ClientServiceRegistry.cs`
- `Overview.Client/Overview.Client/Presentation/Pages/MainPage.xaml`
- `Overview.Client/Overview.Client/Presentation/Pages/MainPage.xaml.cs`
- `Overview.Client/Overview.Client/Presentation/ViewModels/MainViewModel.cs`
- `Overview.Client/Overview.Client/Domain/DomainAssemblyMarker.cs`
- `Overview.Client/Overview.Client/Infrastructure/InfrastructureAssemblyMarker.cs`
- `docs/PROJECT-STATUS.md`
- `docs/PROJECT-TODO.md`
- `docs/PROJECT-HANDOFF.md`
- `docs/PROJECT-CHANGELOG.md`
- `docs/PROJECT-FILE-MAP.md`

## 下一步唯一推荐动作

- 执行 `SHELL-120`：在服务端建立 `Api`、`Application`、`Domain`、`Infrastructure` 目录和基础配置样例；不要提前实现完整业务逻辑

## 接手 AI 注意事项

- 开始任何实现前，先重新阅读 `“一览”用户要求.md`
- 开始任何实现前，先检查 git 仓库状态
- 根解决方案入口是 `Overview.Uno.slnx`，不是 `.sln`
- 若从仓库根目录执行 `dotnet restore/build`，必须保留根级 `global.json`，否则 `Uno.Sdk` 无法解析
- Uno 模板还生成了 `Overview.Client/Overview.Client.sln`，当前以仓库根解决方案为主
- 当前客户端已经有轻量注册中心，但这只是骨架，不应在 `SHELL-120` 前开始实现业务服务
- 不要跳过 Shell/Domain/Application 直接做页面细节
- 如果发现本地环境缺少构建所需工具链，先尝试自行安装或补齐环境，再继续任务
- 只有在尝试安装后仍无法继续时，才记录阻塞
- 每完成一个最小任务项后，要先更新状态文件，再立即创建一次包含任务 ID 的 git commit

## 若中断时优先检查的文件

1. `PROJECT-STATUS.md`
2. `PROJECT-TODO.md`
3. `PROJECT-HANDOFF.md`
