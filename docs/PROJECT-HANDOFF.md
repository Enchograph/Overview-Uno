# Project Handoff

## 本轮目标

- 完成 `SYNC-910`，为设置页补上手动同步入口和同步状态展示

## 本轮完成

- 将设置页 `sync` 分区从占位摘要升级为真实同步面板：
  - `Overview.Client/Overview.Client/Presentation/Pages/SettingsPage.xaml`
  - `Overview.Client/Overview.Client/Presentation/Pages/SettingsPage.xaml.cs`
- 设置页当前已支持：
  - 手动 `Sync Now` 入口
  - 自动同步是否运行、当前状态、最近触发方式、待同步变更数、应用/拉取数量、冲突数、最近尝试/成功时间、最近错误展示
  - 同步状态变化时自动刷新页面展示
- 设置页 ViewModel 当前已接入同步编排服务：
  - `Overview.Client/Overview.Client/Presentation/ViewModels/SettingsPageViewModel.cs`
- 更新客户端注册中心以注入同步编排服务到设置页：
  - `Overview.Client/Overview.Client/Application/DependencyInjection/ClientServiceRegistry.cs`
- 扩展设置页测试：
  - `tests/Overview.Client.Tests/SettingsPageViewModelTests.cs`
- 验证结果：
  - `dotnet build Overview.Client/Overview.Client/Overview.Client.csproj -f net10.0-desktop` 通过，0 warning / 0 error
  - `dotnet test tests/Overview.Client.Tests/Overview.Client.Tests.csproj` 通过，47/47 用例通过

## 本轮未完成

- `SYNC-920` 自动收敛验证
- 真实邮件发送提供程序接入
- 通知平台映射
- 小组件平台映射

## 当前阻塞

- 无

## 已更新文件

- `Overview.Client/Overview.Client/Application/DependencyInjection/ClientServiceRegistry.cs`
- `Overview.Client/Overview.Client/Presentation/Pages/SettingsPage.xaml`
- `Overview.Client/Overview.Client/Presentation/Pages/SettingsPage.xaml.cs`
- `Overview.Client/Overview.Client/Presentation/ViewModels/SettingsPageViewModel.cs`
- `tests/Overview.Client.Tests/SettingsPageViewModelTests.cs`
- `docs/PROJECT-STATUS.md`
- `docs/PROJECT-TODO.md`
- `docs/PROJECT-ACCEPTANCE.md`
- `docs/PROJECT-HANDOFF.md`
- `docs/PROJECT-CHANGELOG.md`
- `docs/PROJECT-FILE-MAP.md`

## 下一步唯一推荐动作

- 执行 `SYNC-920`：验证事项与设置在不手动触发的情况下也能自动收敛

## 接手 AI 注意事项

- 开始任何实现前，先重新阅读 `“一览”用户要求.md`
- 开始任何实现前，先检查 git 仓库状态
- 根解决方案入口是 `Overview.Uno.slnx`，不是 `.sln`
- 若从仓库根目录执行 `dotnet restore/build`，必须保留根级 `global.json`，否则 `Uno.Sdk` 无法解析
- 当前列表页已完成标签筛选、排序、完成切换、重要切换、手动重排、主题切换、“更多设置”联动、滑动编辑删除和浮动添加；当前已推进到 AI 任务
- 当前 `SYNC-900` 和 `SYNC-910` 已完成；下一轮应切换到 `SYNC-920`，不要跳到平台集成
- 新增的客户端测试项目当前通过直接引用桌面构建产物 `Overview.Client.dll` 运行；执行 `dotnet test tests/Overview.Client.Tests/Overview.Client.Tests.csproj` 前，先确保客户端桌面目标已经构建过
- 当前列表页采用“ViewModel 聚合状态 + 页面手动 Apply”模式；后续列表页任务应延续该模式，避免额外引入状态库
- 当前设置页 AI 分区也沿用“ViewModel 聚合状态 + 页面手动 Apply”模式；后续 AI 页若需要新增表单或状态，优先复用这一模式
- 当前 AI 页同样沿用“ViewModel 聚合状态 + 页面手动 Apply”模式；`AiPage.xaml.cs` 负责把 ViewModel 状态回填到页面、同步时间选择器模式，并在发送后滚动到最后一条消息
- 当前自动同步触发点只集中在 `SyncLifecycleCoordinator`、`App.xaml.cs` 与 `ShellPage.xaml.cs`；后续不要把同步启动/停止逻辑散到 `HomePage`、`ListPage`、`AiPage` 等业务页面
- 当前应用窗口激活会执行一次前台同步；若后续需要更细粒度退避、网络感知或平台后台任务，优先在协调层扩展，不要修改各业务 ViewModel
- 当前手动同步入口只在设置页 `sync` 分区；状态展示依赖 `ISyncOrchestrationService.StatusChanged` 驱动刷新，不要在页面里直接拼接同步逻辑
- 运行 EF CLI 前先执行 `dotnet tool restore`
- 本地没有可连接的 PostgreSQL 实例；如果下轮需要验证 `database update` 或真实读写，请先启动数据库或调整连接串
- 每完成一个最小任务项后，要先更新状态文件，再立即创建一次包含任务 ID 的 git commit

## 若中断时优先检查的文件

1. `PROJECT-STATUS.md`
2. `PROJECT-TODO.md`
3. `PROJECT-HANDOFF.md`
