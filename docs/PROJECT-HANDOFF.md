# Project Handoff

## 本轮目标

- 完成 `AI-810`，实现 AI 聊天页与按日存储

## 本轮完成

- 新增 AI 聊天应用层：
  - `Application/Ai/IAiChatService.cs`
  - `Application/Ai/AiChatDaySnapshot.cs`
  - `Application/Ai/AiChatService.cs`
  - 负责加载当日消息、调用 AI 接口、保存用户/助手消息到本地 SQLite
- 新增 AI 远程访问层：
  - `Infrastructure/Api/Ai/IAiRemoteClient.cs`
  - `Infrastructure/Api/Ai/AiRemoteClient.cs`
  - 直接请求用户配置的 OpenAI 兼容 `chat/completions` 端点
- 更新客户端 AI 页为真实聊天页：
  - `Presentation/Pages/AiPage.xaml`
  - `Presentation/Pages/AiPage.xaml.cs`
  - `Presentation/ViewModels/AiPageViewModel.cs`
  - `Presentation/ViewModels/AiChatMessageEntryViewModel.cs`
  - 当前固定展示“当日”聊天线程，并保留后续扩展到日/周/月聚合的接口边界
- 更新客户端轻量注册中心，接入 AI 聊天仓储、AI 远程客户端和聊天应用服务：
  - `Application/DependencyInjection/ClientServiceRegistry.cs`
- 新增 AI 聊天测试：
  - `tests/Overview.Client.Tests/AiChatServiceTests.cs`
  - `tests/Overview.Client.Tests/AiPageViewModelTests.cs`
- 验证结果：
  - `dotnet build Overview.Client/Overview.Client/Overview.Client.csproj -f net10.0-desktop` 通过，0 warning / 0 error
  - `dotnet test tests/Overview.Client.Tests/Overview.Client.Tests.csproj` 通过，33/33 用例通过

## 本轮未完成

- `AI-820` 及后续 AI 页时间范围聚合与自然语言事项操作任务
- 真实邮件发送提供程序接入
- 通知平台映射
- 小组件平台映射

## 当前阻塞

- 无

## 已更新文件

- `Overview.Client/Overview.Client/Application/Ai/IAiChatService.cs`
- `Overview.Client/Overview.Client/Application/Ai/AiChatDaySnapshot.cs`
- `Overview.Client/Overview.Client/Application/Ai/AiChatService.cs`
- `Overview.Client/Overview.Client/Application/DependencyInjection/ClientServiceRegistry.cs`
- `Overview.Client/Overview.Client/Infrastructure/Api/Ai/IAiRemoteClient.cs`
- `Overview.Client/Overview.Client/Infrastructure/Api/Ai/AiRemoteClient.cs`
- `Overview.Client/Overview.Client/Presentation/Pages/AiPage.xaml`
- `Overview.Client/Overview.Client/Presentation/Pages/AiPage.xaml.cs`
- `Overview.Client/Overview.Client/Presentation/ViewModels/AiPageViewModel.cs`
- `Overview.Client/Overview.Client/Presentation/ViewModels/AiChatMessageEntryViewModel.cs`
- `tests/Overview.Client.Tests/AiChatServiceTests.cs`
- `tests/Overview.Client.Tests/AiPageViewModelTests.cs`
- `docs/PROJECT-STATUS.md`
- `docs/PROJECT-TODO.md`
- `docs/PROJECT-HANDOFF.md`
- `docs/PROJECT-CHANGELOG.md`
- `docs/PROJECT-FILE-MAP.md`
- `docs/PROJECT-ACCEPTANCE.md`

## 下一步唯一推荐动作

- 执行 `AI-820`：实现 AI 按日/周/月聚合展示

## 接手 AI 注意事项

- 开始任何实现前，先重新阅读 `“一览”用户要求.md`
- 开始任何实现前，先检查 git 仓库状态
- 根解决方案入口是 `Overview.Uno.slnx`，不是 `.sln`
- 若从仓库根目录执行 `dotnet restore/build`，必须保留根级 `global.json`，否则 `Uno.Sdk` 无法解析
- 当前列表页已完成标签筛选、排序、完成切换、重要切换、手动重排、主题切换、“更多设置”联动、滑动编辑删除和浮动添加；本轮已切换到 AI 任务
- 当前 AI 页已具备真实聊天 UI，但只显示“当日”线程；下一轮应在 `AI-820` 复用时间选择规则，把仓储读取扩展到日/周/月范围展示
- 新增的主页命中测试项目当前通过直接引用桌面构建产物 `Overview.Client.dll` 运行；执行 `dotnet test tests/Overview.Client.Tests/Overview.Client.Tests.csproj` 前，先确保客户端桌面目标已经构建过
- 当前列表页采用“ViewModel 聚合状态 + 页面手动 Apply”模式；后续列表页任务应延续该模式，避免额外引入状态库
- 当前设置页 AI 分区也沿用“ViewModel 聚合状态 + 页面手动 Apply”模式；后续 AI 页若需要新增表单或状态，优先复用这一模式
- 当前 AI 页同样沿用“ViewModel 聚合状态 + 页面手动 Apply”模式；`AiPage.xaml.cs` 负责把 ViewModel 状态回填到页面，并在发送后滚动到最后一条消息
- 当前 `AiChatService` 只按用户时区解析“今天”，并且每次请求只发送系统提示、当前用户消息和相关事项摘要，不发送历史聊天
- 当前 `AiRemoteClient` 直接拼接 `chat/completions` 相对路径；若后续接入更广泛兼容提供商，需要在这里做响应兼容扩展，而不是把协议细节散到页面层
- 当前列表页标签选中态通过 ViewModel 文案前缀表现，避免依赖 Uno 未实现的 `ItemContainerGenerator`
- 当前列表页重排入口采用顶部按钮 + 行内上下移动按钮；手动顺序已提升为快照主顺序，后续“更多设置”联动不要破坏该模式
- 设置页现在支持通过导航参数直接打开指定分区；若后续新增从其他页面深链设置页，优先复用该入口
- 当前添加页导航参数已支持 `SuggestedType`、`SuggestedIsImportant`、`SuggestedStartDate`、`SuggestedStartTime`；列表页浮动按钮和主页长按都复用这个入口
- 当前登录页仍负责会话恢复；列表页 ViewModel 默认直接读取 `IAuthenticationService.CurrentSession`
- AI 设置当前保存到 `UserSettings.AiBaseUrl`、`UserSettings.AiApiKey`、`UserSettings.AiModel`，并通过 `UserSettingsService` 生成 `SyncChange`
- 运行 EF CLI 前先执行 `dotnet tool restore`
- 本地没有可连接的 PostgreSQL 实例；如果下轮需要验证 `database update` 或真实读写，请先启动数据库或调整连接串
- 每完成一个最小任务项后，要先更新状态文件，再立即创建一次包含任务 ID 的 git commit

## 若中断时优先检查的文件

1. `PROJECT-STATUS.md`
2. `PROJECT-TODO.md`
3. `PROJECT-HANDOFF.md`
