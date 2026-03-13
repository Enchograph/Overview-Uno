# Project Handoff

## 本轮目标

- 完成 `PLATFORM-1030`，完成 Windows 与 Web 主流程适配和能力降级

## 本轮完成

- 新增平台能力描述模型：
  - `Overview.Client/Overview.Client/Infrastructure/Platform/IPlatformCapabilities.cs`
- 已在客户端注册中心增加平台分支：
  - WebAssembly 当前切换到内存事项仓储、设置仓储、AI 聊天仓储、同步变更仓储
  - WebAssembly 当前切换到内存登录态、同步状态和设备 ID 存储
  - Desktop / Android 继续保留原有 SQLite 与文件存储链路
  - `Overview.Client/Overview.Client/Application/DependencyInjection/ClientServiceRegistry.cs`
  - `Overview.Client/Overview.Client/Infrastructure/Persistence/Repositories/InMemoryRepositories.cs`
  - `Overview.Client/Overview.Client/Infrastructure/Settings/InMemoryStores.cs`
- 已将设置页 `About` 分区升级为平台能力说明入口：
  - 展示当前平台、平台家族、主流程状态
  - 展示本地数据能力、通知能力、小组件能力
  - 展示明确降级策略，避免伪造 Windows / Web 不具备的平台能力
  - `Overview.Client/Overview.Client/Presentation/ViewModels/SettingsPageViewModel.cs`
- 已补充平台降级说明测试：
  - `tests/Overview.Client.Tests/SettingsPageViewModelTests.cs`
- 验证结果：
  - `dotnet build Overview.Client/Overview.Client/Overview.Client.csproj -f net10.0-desktop -v q` 通过，0 warning / 0 error
  - `dotnet build Overview.Client/Overview.Client/Overview.Client.csproj -f net10.0-browserwasm -v q` 通过，0 error；仍有既有 Wasm trimming / SQLite provider warning
  - `dotnet test tests/Overview.Client.Tests/Overview.Client.Tests.csproj` 通过，59/59 用例通过

## 本轮未完成

- 真实邮件发送提供程序接入
- 阶段 11 的测试补齐、需求映射验收与性能验证

## 当前阻塞

- 无

## 已更新文件

- `Overview.Client/Overview.Client/Application/DependencyInjection/ClientServiceRegistry.cs`
- `Overview.Client/Overview.Client/Infrastructure/Persistence/Repositories/InMemoryRepositories.cs`
- `Overview.Client/Overview.Client/Infrastructure/Settings/InMemoryStores.cs`
- `Overview.Client/Overview.Client/Infrastructure/Platform/IPlatformCapabilities.cs`
- `Overview.Client/Overview.Client/Presentation/ViewModels/SettingsPageViewModel.cs`
- `tests/Overview.Client.Tests/SettingsPageViewModelTests.cs`
- `docs/PROJECT-STATUS.md`
- `docs/PROJECT-TODO.md`
- `docs/PROJECT-ACCEPTANCE.md`
- `docs/PROJECT-HANDOFF.md`
- `docs/PROJECT-CHANGELOG.md`
- `docs/PROJECT-FILE-MAP.md`

## 下一步唯一推荐动作

- 执行 `QA-1100`：补齐自动化测试与原始需求映射验收

## 接手 AI 注意事项

- 开始任何实现前，先重新阅读 `“一览”用户要求.md`
- 开始任何实现前，先检查 git 仓库状态
- 根解决方案入口是 `Overview.Uno.slnx`，不是 `.sln`
- 若从仓库根目录执行 `dotnet restore/build`，必须保留根级 `global.json`，否则 `Uno.Sdk` 无法解析
- 新增的客户端测试项目当前通过直接引用桌面构建产物 `Overview.Client.dll` 运行；执行 `dotnet test tests/Overview.Client.Tests/Overview.Client.Tests.csproj` 前，先确保客户端桌面目标已经构建过
- 当前列表页、AI 页、设置页仍沿用“ViewModel 聚合状态 + 页面手动 Apply”模式；后续新增状态时优先延续该模式
- 当前自动同步触发点只集中在 `SyncLifecycleCoordinator`、`App.xaml.cs` 与 `ShellPage.xaml.cs`；不要把同步启动/停止逻辑散到业务页面
- 当前本地提醒重建统一收敛在 `NotificationRefreshService`；不要把平台通知调度逻辑散到页面或 ViewModel
- 当前 Desktop / Web 的通知与小组件已经补入明确降级说明；后续 QA 轮次里不要把 no-op 降级误记为“已实现真实平台能力”
- 当前 WebAssembly 已切到内存仓储和会话级状态存储，主流程不再依赖 SQLite / 文件路径；但 `browserwasm build` 仍有既有 SQLite provider 警告，后续若要彻底消除需进一步拆分包引用或裁剪共享文件
- 当前 Android 构建在本环境里会长时间停留在收尾阶段，但已产出 `bin/Debug/net10.0-android/Overview.Client.dll`
- 当前外部导航协议统一走 `overview://...`，如需新增平台入口或小组件动作，优先扩展 `AppNavigationRequest`
- 运行 EF CLI 前先执行 `dotnet tool restore`
- 本地没有可连接的 PostgreSQL 实例；如果下轮需要验证 `database update` 或真实读写，请先启动数据库或调整连接串
- 每完成一个最小任务项后，要先更新状态文件，再立即创建一次包含任务 ID 的 git commit

## 若中断时优先检查的文件

1. `PROJECT-STATUS.md`
2. `PROJECT-TODO.md`
3. `PROJECT-HANDOFF.md`
