# Project Handoff

## 本轮目标

- 完成 `SHELL-130`，实现五页应用壳层、底部导航和默认主页启动

## 本轮完成

- 新增 `Presentation/Pages/ShellPage.xaml` 作为客户端应用壳层
- 将应用默认启动页从 `MainPage` 切换为 `ShellPage`
- 新增五个主页面占位：
  - `HomePage`
  - `ListPage`
  - `AiPage`
  - `AddItemPage`
  - `SettingsPage`
- 新增五个占位页对应的 ViewModel 和共享 `PlaceholderPageViewModel`
- 更新 `ClientServiceRegistry`，注册壳层和五个页面的 ViewModel
- 验证 `dotnet build Overview.Client/Overview.Client/Overview.Client.csproj -f net10.0-desktop` 通过，0 warning / 0 error

## 本轮未完成

- 任何业务代码实现
- 统一领域模型定义

## 当前阻塞

- 无

## 已更新文件

- `Overview.Client/Overview.Client/App.xaml.cs`
- `Overview.Client/Overview.Client/Application/DependencyInjection/ClientServiceRegistry.cs`
- `Overview.Client/Overview.Client/Presentation/Pages/ShellPage.xaml`
- `Overview.Client/Overview.Client/Presentation/Pages/ShellPage.xaml.cs`
- `Overview.Client/Overview.Client/Presentation/Pages/HomePage.xaml`
- `Overview.Client/Overview.Client/Presentation/Pages/ListPage.xaml`
- `Overview.Client/Overview.Client/Presentation/Pages/AiPage.xaml`
- `Overview.Client/Overview.Client/Presentation/Pages/AddItemPage.xaml`
- `Overview.Client/Overview.Client/Presentation/Pages/SettingsPage.xaml`
- `Overview.Client/Overview.Client/Presentation/ViewModels/`
- `docs/PROJECT-STATUS.md`
- `docs/PROJECT-TODO.md`
- `docs/PROJECT-HANDOFF.md`
- `docs/PROJECT-CHANGELOG.md`
- `docs/PROJECT-FILE-MAP.md`

## 下一步唯一推荐动作

- 执行 `DOMAIN-200`：在客户端和服务端定义统一核心模型，覆盖三类事项、用户设置、聊天记录和同步变更模型

## 接手 AI 注意事项

- 开始任何实现前，先重新阅读 `“一览”用户要求.md`
- 开始任何实现前，先检查 git 仓库状态
- 根解决方案入口是 `Overview.Uno.slnx`，不是 `.sln`
- 若从仓库根目录执行 `dotnet restore/build`，必须保留根级 `global.json`，否则 `Uno.Sdk` 无法解析
- Uno 模板还生成了 `Overview.Client/Overview.Client.sln`，当前以仓库根解决方案为主
- 当前客户端已具备五页壳层，但页面仍是占位；下一步应先做统一领域模型，而不是直接填页面细节
- 服务端当前只有健康检查与配置落点，不应跳去实现持久化或认证，除非先完成 `DOMAIN-200`
- 不要跳过 Shell/Domain/Application 直接做页面细节
- 如果发现本地环境缺少构建所需工具链，先尝试自行安装或补齐环境，再继续任务
- 只有在尝试安装后仍无法继续时，才记录阻塞
- 每完成一个最小任务项后，要先更新状态文件，再立即创建一次包含任务 ID 的 git commit

## 若中断时优先检查的文件

1. `PROJECT-STATUS.md`
2. `PROJECT-TODO.md`
3. `PROJECT-HANDOFF.md`
