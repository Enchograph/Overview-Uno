# Project Handoff

## 本轮目标

- 完成 `AI-820`，实现 AI 按日 / 周 / 月聚合展示

## 本轮完成

- 新增 AI 聊天范围快照模型与读取入口：
  - `Application/Ai/AiChatPeriodSnapshot.cs`
  - `Application/Ai/IAiChatService.cs`
  - `Application/Ai/AiChatService.cs`
  - 当前支持按任意日 / 周 / 月范围读取本地聊天记录
- 更新客户端 AI 页状态与界面：
  - `Presentation/ViewModels/AiPageViewModel.cs`
  - `Presentation/Pages/AiPage.xaml`
  - `Presentation/Pages/AiPage.xaml.cs`
  - 当前支持日 / 周 / 月模式切换、时间选择器展开确认，以及发送后按当前范围重新刷新消息列表
- 更新客户端注册中心，给 AI 页接入时间选择应用服务：
  - `Application/DependencyInjection/ClientServiceRegistry.cs`
- 扩展 AI 测试覆盖：
  - `tests/Overview.Client.Tests/AiChatServiceTests.cs`
  - `tests/Overview.Client.Tests/AiPageViewModelTests.cs`
- 验证结果：
  - `dotnet build Overview.Client/Overview.Client/Overview.Client.csproj -f net10.0-desktop` 通过，0 warning / 0 error
  - `dotnet test tests/Overview.Client.Tests/Overview.Client.Tests.csproj` 通过，35/35 用例通过

## 本轮未完成

- `AI-830` 及后续 AI 自然语言事项操作任务
- 真实邮件发送提供程序接入
- 通知平台映射
- 小组件平台映射

## 当前阻塞

- 无

## 已更新文件

- `Overview.Client/Overview.Client/Application/Ai/IAiChatService.cs`
- `Overview.Client/Overview.Client/Application/Ai/AiChatDaySnapshot.cs`
- `Overview.Client/Overview.Client/Application/Ai/AiChatPeriodSnapshot.cs`
- `Overview.Client/Overview.Client/Application/Ai/AiChatService.cs`
- `Overview.Client/Overview.Client/Application/DependencyInjection/ClientServiceRegistry.cs`
- `Overview.Client/Overview.Client/Presentation/Pages/AiPage.xaml`
- `Overview.Client/Overview.Client/Presentation/Pages/AiPage.xaml.cs`
- `Overview.Client/Overview.Client/Presentation/ViewModels/AiPageViewModel.cs`
- `tests/Overview.Client.Tests/AiChatServiceTests.cs`
- `tests/Overview.Client.Tests/AiPageViewModelTests.cs`
- `docs/PROJECT-STATUS.md`
- `docs/PROJECT-TODO.md`
- `docs/PROJECT-HANDOFF.md`
- `docs/PROJECT-CHANGELOG.md`
- `docs/PROJECT-FILE-MAP.md`
- `docs/PROJECT-ACCEPTANCE.md`

## 下一步唯一推荐动作

- 执行 `AI-830`：实现 AI JSON 响应解析和自然语言增删查事项

## 接手 AI 注意事项

- 开始任何实现前，先重新阅读 `“一览”用户要求.md`
- 开始任何实现前，先检查 git 仓库状态
- 根解决方案入口是 `Overview.Uno.slnx`，不是 `.sln`
- 若从仓库根目录执行 `dotnet restore/build`，必须保留根级 `global.json`，否则 `Uno.Sdk` 无法解析
- 当前列表页已完成标签筛选、排序、完成切换、重要切换、手动重排、主题切换、“更多设置”联动、滑动编辑删除和浮动添加；当前已推进到 AI 任务
- 当前 AI 页已支持按日 / 周 / 月查看聊天记录；下一轮应在现有范围选择 UI 上继续落地 `AI-830` 的 JSON 意图解析与事项增删查
- 新增的客户端测试项目当前通过直接引用桌面构建产物 `Overview.Client.dll` 运行；执行 `dotnet test tests/Overview.Client.Tests/Overview.Client.Tests.csproj` 前，先确保客户端桌面目标已经构建过
- 当前列表页采用“ViewModel 聚合状态 + 页面手动 Apply”模式；后续列表页任务应延续该模式，避免额外引入状态库
- 当前设置页 AI 分区也沿用“ViewModel 聚合状态 + 页面手动 Apply”模式；后续 AI 页若需要新增表单或状态，优先复用这一模式
- 当前 AI 页同样沿用“ViewModel 聚合状态 + 页面手动 Apply”模式；`AiPage.xaml.cs` 负责把 ViewModel 状态回填到页面、同步时间选择器模式，并在发送后滚动到最后一条消息
- 当前 `AiChatService` 仍按用户时区把消息按日落库，但新增了基于 `CalendarPeriod` 的范围读取；后续不要把范围聚合逻辑散到页面层
- 当前 `AiRemoteClient` 直接拼接 `chat/completions` 相对路径；若后续接入更广泛兼容提供商，需要在这里做响应兼容扩展，而不是把协议细节散到页面层
- 当前添加页导航参数已支持 `SuggestedType`、`SuggestedIsImportant`、`SuggestedStartDate`、`SuggestedStartTime`；后续 AI 若生成事项创建动作，可直接复用这些入口或 `IItemService`
- AI 设置当前保存到 `UserSettings.AiBaseUrl`、`UserSettings.AiApiKey`、`UserSettings.AiModel`，并通过 `UserSettingsService` 生成 `SyncChange`
- 运行 EF CLI 前先执行 `dotnet tool restore`
- 本地没有可连接的 PostgreSQL 实例；如果下轮需要验证 `database update` 或真实读写，请先启动数据库或调整连接串
- 每完成一个最小任务项后，要先更新状态文件，再立即创建一次包含任务 ID 的 git commit

## 若中断时优先检查的文件

1. `PROJECT-STATUS.md`
2. `PROJECT-TODO.md`
3. `PROJECT-HANDOFF.md`
