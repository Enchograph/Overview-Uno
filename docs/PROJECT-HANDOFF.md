# Project Handoff

## 本轮目标

- 完成 `AI-830`，实现 AI JSON 响应解析和自然语言增删查事项

## 本轮完成

- 扩展 AI 结构化响应模型与解析：
  - `Application/Ai/AiStructuredResponse.cs`
  - `Application/Ai/AiOrchestrationService.cs`
  - 当前已支持 `itemIds`、`color`、`expectedDurationMinutes`、`targetDate` 等字段解析与校验
- 更新客户端 AI 聊天执行链路：
  - `Application/Ai/AiChatService.cs`
  - `Application/DependencyInjection/ClientServiceRegistry.cs`
  - 当前已支持 `create_item` 创建事项、`delete_item` 删除事项、`query_items` / `answer_question` / `clarify` 自然语言答复
  - 当前对低置信度或缺关键字段写操作会降级为追问，不直接改数据
- 扩展 AI 测试覆盖：
  - `tests/Overview.Client.Tests/AiChatServiceTests.cs`
  - `tests/Overview.Client.Tests/AiOrchestrationServiceTests.cs`
- 验证结果：
  - `dotnet build Overview.Client/Overview.Client/Overview.Client.csproj -f net10.0-desktop` 通过，0 warning / 0 error
  - `dotnet test tests/Overview.Client.Tests/Overview.Client.Tests.csproj` 通过，41/41 用例通过

## 本轮未完成

- `SYNC-900` 及后续实时同步接入任务
- 真实邮件发送提供程序接入
- 通知平台映射
- 小组件平台映射

## 当前阻塞

- 无

## 已更新文件

- `Overview.Client/Overview.Client/Application/Ai/AiChatService.cs`
- `Overview.Client/Overview.Client/Application/Ai/AiOrchestrationService.cs`
- `Overview.Client/Overview.Client/Application/Ai/AiStructuredResponse.cs`
- `Overview.Client/Overview.Client/Application/DependencyInjection/ClientServiceRegistry.cs`
- `tests/Overview.Client.Tests/AiChatServiceTests.cs`
- `tests/Overview.Client.Tests/AiOrchestrationServiceTests.cs`
- `docs/PROJECT-STATUS.md`
- `docs/PROJECT-TODO.md`
- `docs/PROJECT-HANDOFF.md`
- `docs/PROJECT-CHANGELOG.md`
- `docs/PROJECT-FILE-MAP.md`
- `docs/PROJECT-ACCEPTANCE.md`

## 下一步唯一推荐动作

- 执行 `SYNC-900`：把自动后台同步接入页面生命周期与后台触发点

## 接手 AI 注意事项

- 开始任何实现前，先重新阅读 `“一览”用户要求.md`
- 开始任何实现前，先检查 git 仓库状态
- 根解决方案入口是 `Overview.Uno.slnx`，不是 `.sln`
- 若从仓库根目录执行 `dotnet restore/build`，必须保留根级 `global.json`，否则 `Uno.Sdk` 无法解析
- 当前列表页已完成标签筛选、排序、完成切换、重要切换、手动重排、主题切换、“更多设置”联动、滑动编辑删除和浮动添加；当前已推进到 AI 任务
- 当前 AI 页已支持按日 / 周 / 月查看聊天记录，并且 `AI-830` 已完成；下一轮应切换到 `SYNC-900`，不要再继续扩展 AI 范围外能力
- 新增的客户端测试项目当前通过直接引用桌面构建产物 `Overview.Client.dll` 运行；执行 `dotnet test tests/Overview.Client.Tests/Overview.Client.Tests.csproj` 前，先确保客户端桌面目标已经构建过
- 当前列表页采用“ViewModel 聚合状态 + 页面手动 Apply”模式；后续列表页任务应延续该模式，避免额外引入状态库
- 当前设置页 AI 分区也沿用“ViewModel 聚合状态 + 页面手动 Apply”模式；后续 AI 页若需要新增表单或状态，优先复用这一模式
- 当前 AI 页同样沿用“ViewModel 聚合状态 + 页面手动 Apply”模式；`AiPage.xaml.cs` 负责把 ViewModel 状态回填到页面、同步时间选择器模式，并在发送后滚动到最后一条消息
- 当前 `AiChatService` 已在发送链路内完成“解析 -> 校验 -> 执行 -> 追问保护 -> 写入聊天记录”；后续不要把 AI 意图执行散到页面层
- 当前删除操作只接受 AI 返回的明确 `itemIds`，这是避免误删的保护边界；若后续需要更宽松匹配，必须先更新任务或决策文档
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
