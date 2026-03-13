# Project Handoff

## 本轮目标

- 完成 `HOME-630`，实现主页点击命中、长按和详情交互

## 本轮完成

- 新增客户端主页交互解析服务：
  - `Application/Home/IHomeTimelineInteractionService.cs`
  - `Application/Home/HomeTimelineInteractionService.cs`
  - `Application/Home/HomeTimelineInteractionResult.cs`
- 更新客户端主页时间块网格组件与主页页面：
  - `Presentation/Components/HomeTimelineGrid.xaml`
  - `Presentation/Components/HomeTimelineGrid.xaml.cs`
  - `Presentation/Components/HomeTimelineInteractionRequestedEventArgs.cs`
  - `Presentation/Pages/HomePage.xaml`
  - `Presentation/Pages/HomePage.xaml.cs`
  - `Presentation/ViewModels/HomePageViewModel.cs`
- 更新客户端添加页导航预填链路：
  - `Presentation/Pages/AddItemNavigationRequest.cs`
  - `Presentation/Pages/AddItemPage.xaml.cs`
  - `Presentation/ViewModels/AddItemPageViewModel.cs`
- 新增主页命中交互独立测试：
  - `tests/Overview.Client.Tests/Overview.Client.Tests.csproj`
  - `tests/Overview.Client.Tests/HomeTimelineInteractionServiceTests.cs`
- 当前主页已新增覆盖：
  - 点击事项按命中规则打开详情卡片
  - 长按空白单元格跳转添加页并预填起始日期、时间块起始时间
  - 长按事项跳转编辑页
  - 详情卡片可直接跳转编辑态
- 验证结果：
  - `dotnet build Overview.Client/Overview.Client/Overview.Client.csproj -f net10.0-desktop` 通过，0 warning / 0 error
  - `dotnet test tests/Overview.Client.Tests/Overview.Client.Tests.csproj` 通过，4/4 用例通过

## 本轮未完成

- `LIST-700` 及后续列表页 Presentation 层任务
- 真实邮件发送提供程序接入
- 通知平台映射
- 小组件平台映射

## 当前阻塞

- 无

## 已更新文件

- `Overview.Client/Overview.Client/Application/DependencyInjection/ClientServiceRegistry.cs`
- `Overview.Client/Overview.Client/Application/Home/IHomeTimelineInteractionService.cs`
- `Overview.Client/Overview.Client/Application/Home/HomeTimelineInteractionService.cs`
- `Overview.Client/Overview.Client/Application/Home/HomeTimelineInteractionResult.cs`
- `Overview.Client/Overview.Client/Presentation/Components/HomeTimelineGrid.xaml`
- `Overview.Client/Overview.Client/Presentation/Components/HomeTimelineGrid.xaml.cs`
- `Overview.Client/Overview.Client/Presentation/Components/HomeTimelineInteractionRequestedEventArgs.cs`
- `Overview.Client/Overview.Client/Presentation/Pages/AddItemNavigationRequest.cs`
- `Overview.Client/Overview.Client/Presentation/Pages/AddItemPage.xaml.cs`
- `Overview.Client/Overview.Client/Presentation/Pages/HomePage.xaml`
- `Overview.Client/Overview.Client/Presentation/Pages/HomePage.xaml.cs`
- `Overview.Client/Overview.Client/Presentation/ViewModels/AddItemPageViewModel.cs`
- `Overview.Client/Overview.Client/Presentation/ViewModels/HomePageViewModel.cs`
- `tests/Overview.Client.Tests/Overview.Client.Tests.csproj`
- `tests/Overview.Client.Tests/HomeTimelineInteractionServiceTests.cs`
- `docs/PROJECT-STATUS.md`
- `docs/PROJECT-TODO.md`
- `docs/PROJECT-HANDOFF.md`
- `docs/PROJECT-CHANGELOG.md`
- `docs/PROJECT-FILE-MAP.md`
- `docs/PROJECT-ACCEPTANCE.md`
- `docs/PROJECT-ROADMAP.md`

## 下一步唯一推荐动作

- 执行 `LIST-700`：实现列表页标签和数据筛选

## 接手 AI 注意事项

- 开始任何实现前，先重新阅读 `“一览”用户要求.md`
- 开始任何实现前，先检查 git 仓库状态
- 根解决方案入口是 `Overview.Uno.slnx`，不是 `.sln`
- 若从仓库根目录执行 `dotnet restore/build`，必须保留根级 `global.json`，否则 `Uno.Sdk` 无法解析
- 当前主页阶段任务已经完成；下一轮应转入列表页，不要再回到主页补平行增强项
- 新增的主页命中测试项目当前通过直接引用桌面构建产物 `Overview.Client.dll` 运行；执行 `dotnet test tests/Overview.Client.Tests/Overview.Client.Tests.csproj` 前，先确保客户端桌面目标已经构建过
- 当前主页顶栏标题会复用 `TimeSelectionPicker`；AI 页后续如需时间选择，仍应复用同一组件，不要另写一套
- 当前主页左右滑动切换已经由 `HomeTimelineGrid` 发出导航事件；后续若叠加事项块，需保留这一导航链路
- 当前登录页仍负责会话恢复；主页 ViewModel 默认直接读取 `IAuthenticationService.CurrentSession`
- 运行 EF CLI 前先执行 `dotnet tool restore`
- 本地没有可连接的 PostgreSQL 实例；如果下轮需要验证 `database update` 或真实读写，请先启动数据库或调整连接串
- 每完成一个最小任务项后，要先更新状态文件，再立即创建一次包含任务 ID 的 git commit

## 若中断时优先检查的文件

1. `PROJECT-STATUS.md`
2. `PROJECT-TODO.md`
3. `PROJECT-HANDOFF.md`
