# Project Handoff

## 本轮目标

- 完成 `SYNC-920`，验证事项与设置在不手动触发的情况下也能自动收敛

## 本轮完成

- 新增同步自动收敛验证测试：
  - `tests/Overview.Client.Tests/SyncOrchestrationServiceTests.cs`
- 当前验证方式：
  - 使用共享内存远端和两套独立客户端仓储模拟双设备
  - 两端只调用 `StartAutoSyncAsync`，不调用手动同步入口
  - 分别验证事项自动收敛与设置自动收敛
- 已确认阶段 9 验收闭环：
  - 自动后台同步已接入
  - 手动同步入口和同步状态展示已存在
  - 自动收敛测试已通过
- 验证结果：
  - `dotnet build Overview.Client/Overview.Client/Overview.Client.csproj -f net10.0-desktop` 通过，0 warning / 0 error
  - `dotnet test tests/Overview.Client.Tests/Overview.Client.Tests.csproj` 通过，49/49 用例通过

## 本轮未完成

- 真实邮件发送提供程序接入
- 通知平台映射
- 小组件平台映射

## 当前阻塞

- 无

## 已更新文件

- `tests/Overview.Client.Tests/SyncOrchestrationServiceTests.cs`
- `docs/PROJECT-STATUS.md`
- `docs/PROJECT-TODO.md`
- `docs/PROJECT-ROADMAP.md`
- `docs/PROJECT-ACCEPTANCE.md`
- `docs/PROJECT-HANDOFF.md`
- `docs/PROJECT-CHANGELOG.md`
- `docs/PROJECT-FILE-MAP.md`

## 下一步唯一推荐动作

- 执行 `PLATFORM-1000`：按平台实现本地通知能力映射

## 接手 AI 注意事项

- 开始任何实现前，先重新阅读 `“一览”用户要求.md`
- 开始任何实现前，先检查 git 仓库状态
- 根解决方案入口是 `Overview.Uno.slnx`，不是 `.sln`
- 若从仓库根目录执行 `dotnet restore/build`，必须保留根级 `global.json`，否则 `Uno.Sdk` 无法解析
- 当前列表页已完成标签筛选、排序、完成切换、重要切换、手动重排、主题切换、“更多设置”联动、滑动编辑删除和浮动添加；当前已推进到 AI 任务
- 当前阶段 9 已完成；下一轮应切换到阶段 10 的 `PLATFORM-1000`
- 新增的客户端测试项目当前通过直接引用桌面构建产物 `Overview.Client.dll` 运行；执行 `dotnet test tests/Overview.Client.Tests/Overview.Client.Tests.csproj` 前，先确保客户端桌面目标已经构建过
- 当前列表页采用“ViewModel 聚合状态 + 页面手动 Apply”模式；后续列表页任务应延续该模式，避免额外引入状态库
- 当前设置页 AI 分区也沿用“ViewModel 聚合状态 + 页面手动 Apply”模式；后续 AI 页若需要新增表单或状态，优先复用这一模式
- 当前 AI 页同样沿用“ViewModel 聚合状态 + 页面手动 Apply”模式；`AiPage.xaml.cs` 负责把 ViewModel 状态回填到页面、同步时间选择器模式，并在发送后滚动到最后一条消息
- 当前自动同步触发点只集中在 `SyncLifecycleCoordinator`、`App.xaml.cs` 与 `ShellPage.xaml.cs`；后续不要把同步启动/停止逻辑散到 `HomePage`、`ListPage`、`AiPage` 等业务页面
- 当前应用窗口激活会执行一次前台同步；若后续需要更细粒度退避、网络感知或平台后台任务，优先在协调层扩展，不要修改各业务 ViewModel
- 当前手动同步入口只在设置页 `sync` 分区；状态展示依赖 `ISyncOrchestrationService.StatusChanged` 驱动刷新，不要在页面里直接拼接同步逻辑
- 当前 `SyncOrchestrationServiceTests` 使用共享内存远端验证双设备自动收敛；后续若修改同步协议，应先保持这些验证通过
- 运行 EF CLI 前先执行 `dotnet tool restore`
- 本地没有可连接的 PostgreSQL 实例；如果下轮需要验证 `database update` 或真实读写，请先启动数据库或调整连接串
- 每完成一个最小任务项后，要先更新状态文件，再立即创建一次包含任务 ID 的 git commit

## 若中断时优先检查的文件

1. `PROJECT-STATUS.md`
2. `PROJECT-TODO.md`
3. `PROJECT-HANDOFF.md`
