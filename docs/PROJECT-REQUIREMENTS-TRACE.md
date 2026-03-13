# Project Requirements Trace

本文档用于把用户原始需求逐条映射到当前实现、验证依据和已知缺口。它服务于 `QA-1100`，不是新的需求来源；若与 [`“一览”用户要求.md`](./“一览”用户要求.md) 冲突，以原始需求为准。

## 映射状态

- `implemented`：已有实现，且有代码或测试佐证
- `degraded`：该平台或场景按既定决策做了明确降级
- `pending`：需求仍未闭环，后续任务继续完成
- `out_of_scope`：按已记录决策明确不纳入首版

## 核心需求映射

| 需求 | 当前状态 | 实现/说明 | 验证依据 |
| --- | --- | --- | --- |
| 应用有主页、列表页、AI 页、添加页、设置页五个主页面 | `implemented` | 已落地壳层导航和五页主流程 | `Presentation/Pages/ShellPage.xaml`、`PROJECT-ACCEPTANCE.md` 里程碑 A |
| 启动默认进入主页 | `implemented` | 客户端默认导航到主页壳层 | `Presentation/Pages/ShellPage.xaml`、既有 `SHELL-130` 验证记录 |
| 主页窄屏显示周视图，宽屏可切月视图 | `implemented` | 主页按断点切换周/月和双栏布局 | `Presentation/Pages/HomePage.xaml`、`PROJECT-ACCEPTANCE.md` 终局项 |
| 时间选择组件支持日/周/月选择、日期映射到周格或月格 | `implemented` | 时间选择 ViewModel 和组件已完成映射规则 | `Presentation/Components/TimeSelectionPicker.xaml`、相关终局验收已勾选 |
| 主页按真实时间比例跨格绘制事项 | `implemented` | 主页时间轴已按可见起止时间生成覆盖层比例 | `Application/Home/HomeLayoutService.cs`、`Presentation/Components/HomeTimelineGrid.xaml` |
| 主页点击/长按命中遵循包裹与最早起始规则 | `implemented` | 主页交互规则和应用层交互解析已实现 | `Domain/Rules/HomeInteractionRuleService.cs`、`tests/Overview.Client.Tests/HomeTimelineInteractionServiceTests.cs` |
| 事项重叠时按最大重叠数计算透明度 | `implemented` | 领域规则已实现重叠透明度计算 | `Domain/Rules/HomeInteractionRuleService.cs` |
| 列表页支持标签筛选、排序、手动重排、主题切换、更多设置、滑动编辑删除、浮动添加预填 | `implemented` | 列表主流程和设置联动已完成 | `Presentation/Pages/ListPage.xaml`、`tests/Overview.Client.Tests/ListPageServiceTests.cs`、`tests/Overview.Client.Tests/ListPageViewModelTests.cs` |
| AI 聊天记录按日存储，并可按日/周/月查看 | `implemented` | AI 聊天服务按日落库，页面支持范围切换 | `Application/Ai/AiChatService.cs`、`tests/Overview.Client.Tests/AiChatServiceTests.cs`、`tests/Overview.Client.Tests/AiPageViewModelTests.cs` |
| AI 每次只传当前用户消息和系统提示，不依赖历史上下文 | `implemented` | AI 服务发送时不拼接历史线程 | `Application/Ai/AiChatService.cs`、既有 AI 测试覆盖 |
| AI 可自然语言新建事项、删除事项、查询事项，低置信度时不误改数据 | `implemented` | AI 编排已实现 create/delete/query/clarify 闭环 | `Application/Ai/AiOrchestrationService.cs`、`tests/Overview.Client.Tests/AiOrchestrationServiceTests.cs`、`tests/Overview.Client.Tests/AiChatServiceTests.cs` |
| 添加页支持三类事项基础信息、日期时间、提醒、重复 | `implemented` | 添加/编辑页与统一表单模型已落地 | `Presentation/Pages/AddItemPage.xaml`、`Presentation/ViewModels/AddItemFormModel.cs` |
| 设置页包含通用、主页、列表页、AI、关于等分区 | `implemented` | 设置主页与二级页骨架已存在，并联通 AI / Sync / About | `Presentation/Pages/SettingsPage.xaml`、`Presentation/ViewModels/SettingsPageViewModel.cs` |
| 数据全端实时同步到自建服务器 | `implemented` | 已实现自动后台同步、手动同步补充入口与服务端同步 API | `Application/Sync/SyncOrchestrationService.cs`、`Overview.Server/Api/Controllers/SyncController.cs`、`tests/Overview.Client.Tests/SyncOrchestrationServiceTests.cs` |
| 同步冲突按最后修改时间收敛 | `implemented` | 本地/远端同步以 `LastModifiedAt` 更新者覆盖旧版本 | `Application/Sync/SyncOrchestrationService.cs`、`tests/Overview.Client.Tests/SyncOrchestrationServiceTests.cs` |
| 本地通知能力 | `implemented` / `degraded` | Android 已接真实调度；Desktop / Web 明确降级 | `Infrastructure/Notifications/PlatformNotificationScheduler.cs`、Android 通知目录、设置页 About 能力说明 |
| 桌面小组件与平台小组件能力 | `implemented` / `degraded` | Android 已实现四类小组件；Desktop / Web 明确降级说明 | `Application/Widgets/WidgetRefreshService.cs`、Android Widgets 目录、设置页 About 能力说明 |
| 平台覆盖手机、平板、桌面、Web | `implemented` / `degraded` | 手机/平板/Windows/Web 主流程可用；按平台能力边界声明降级 | `Presentation/Layout/AdaptiveLayout.cs`、`Infrastructure/Platform/IPlatformCapabilities.cs` |
| 中英双语主流程 | `pending` | 当前仅确认 `en` 资源文件已存在，中文资源与主流程缺失文案验收尚未闭环 | `Overview.Client/Overview.Client/Strings/en/Resources.resw`、`PROJECT-ACCEPTANCE.md` 终局项未勾选 |
| 多人协作 | `out_of_scope` | 首版明确不做 | `PROJECT-DECISIONS.md` `DEC-007` |
| 附件上传 | `out_of_scope` | 首版明确不做 | `PROJECT-DECISIONS.md` `DEC-007` |
| 服务端 AI 代理 | `out_of_scope` | 首版明确不做，AI 由客户端直连用户配置接口 | `PROJECT-DECISIONS.md` `DEC-004`、`DEC-007` |

## QA-1100 本轮新增验证

- 新增 `tests/Overview.Client.Tests/TimeRuleServiceTests.cs`
  - 直接覆盖时间块切分、周起始日和中英文时间标题规则
- 新增 `tests/Overview.Client.Tests/ReminderRuleServiceTests.cs`
  - 直接覆盖提醒归一化、周重复展开和提醒调度顺序
- 扩展 `tests/Overview.Client.Tests/SyncOrchestrationServiceTests.cs`
  - 直接覆盖同步冲突中“服务器版本更新时，本地较旧待同步变更被收敛”的场景

## 当前仍留到后续任务的终局缺口

- 中文资源文件与中英文主流程无缺失文案的完整验收
- 性能指标验证与文档化
- 最终 git 推送收尾
