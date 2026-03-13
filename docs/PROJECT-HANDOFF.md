# Project Handoff

## 本轮目标

- 完成 `LIST-700`，实现列表页标签和数据筛选

## 本轮完成

- 重写客户端列表页与状态 ViewModel：
  - `Presentation/Pages/ListPage.xaml`
  - `Presentation/Pages/ListPage.xaml.cs`
  - `Presentation/ViewModels/ListPageViewModel.cs`
  - `Presentation/ViewModels/ListPageTabEntryViewModel.cs`
  - `Presentation/ViewModels/ListPageItemEntryViewModel.cs`
- 更新客户端轻量注册中心：
  - `Application/DependencyInjection/ClientServiceRegistry.cs`
- 当前列表页已新增覆盖：
  - 我的一天、全部、任务、日程、备忘、重要事项六个标签
  - 标签切换驱动的真实筛选结果
  - 未完成 / 已完成分组展示
  - 未登录空态和筛选空态
- 新增列表筛选独立测试：
  - `tests/Overview.Client.Tests/ListPageServiceTests.cs`
- 验证结果：
  - `dotnet build Overview.Client/Overview.Client/Overview.Client.csproj -f net10.0-desktop` 通过，0 warning / 0 error
  - `dotnet test tests/Overview.Client.Tests/Overview.Client.Tests.csproj` 通过，10/10 用例通过

## 本轮未完成

- `LIST-710` 及后续列表页 Presentation 层任务
- 真实邮件发送提供程序接入
- 通知平台映射
- 小组件平台映射

## 当前阻塞

- 无

## 已更新文件

- `Overview.Client/Overview.Client/Application/DependencyInjection/ClientServiceRegistry.cs`
- `Overview.Client/Overview.Client/Presentation/Pages/ListPage.xaml`
- `Overview.Client/Overview.Client/Presentation/Pages/ListPage.xaml.cs`
- `Overview.Client/Overview.Client/Presentation/ViewModels/ListPageViewModel.cs`
- `Overview.Client/Overview.Client/Presentation/ViewModels/ListPageTabEntryViewModel.cs`
- `Overview.Client/Overview.Client/Presentation/ViewModels/ListPageItemEntryViewModel.cs`
- `tests/Overview.Client.Tests/ListPageServiceTests.cs`
- `docs/PROJECT-STATUS.md`
- `docs/PROJECT-TODO.md`
- `docs/PROJECT-HANDOFF.md`
- `docs/PROJECT-CHANGELOG.md`
- `docs/PROJECT-FILE-MAP.md`

## 下一步唯一推荐动作

- 执行 `LIST-710`：实现列表页排序、完成、重要切换

## 接手 AI 注意事项

- 开始任何实现前，先重新阅读 `“一览”用户要求.md`
- 开始任何实现前，先检查 git 仓库状态
- 根解决方案入口是 `Overview.Uno.slnx`，不是 `.sln`
- 若从仓库根目录执行 `dotnet restore/build`，必须保留根级 `global.json`，否则 `Uno.Sdk` 无法解析
- 当前列表页已完成标签和筛选；下一轮应继续 `LIST-710`，不要跳去主题或更多设置
- 新增的主页命中测试项目当前通过直接引用桌面构建产物 `Overview.Client.dll` 运行；执行 `dotnet test tests/Overview.Client.Tests/Overview.Client.Tests.csproj` 前，先确保客户端桌面目标已经构建过
- 当前列表页采用“ViewModel 聚合状态 + 页面手动 Apply”模式；后续列表页任务应延续该模式，避免额外引入状态库
- 当前列表页标签选中态通过 ViewModel 文案前缀表现，避免依赖 Uno 未实现的 `ItemContainerGenerator`
- 当前登录页仍负责会话恢复；列表页 ViewModel 默认直接读取 `IAuthenticationService.CurrentSession`
- 运行 EF CLI 前先执行 `dotnet tool restore`
- 本地没有可连接的 PostgreSQL 实例；如果下轮需要验证 `database update` 或真实读写，请先启动数据库或调整连接串
- 每完成一个最小任务项后，要先更新状态文件，再立即创建一次包含任务 ID 的 git commit

## 若中断时优先检查的文件

1. `PROJECT-STATUS.md`
2. `PROJECT-TODO.md`
3. `PROJECT-HANDOFF.md`
