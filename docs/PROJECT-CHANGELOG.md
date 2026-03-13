# Project Changelog

## 2026-03-13

### Round 45

- 完成 `SYNC-900`
- 新增 `ISyncLifecycleCoordinator` 与 `SyncLifecycleCoordinator`，集中管理自动同步的页面生命周期接线
- 将自动同步接入 `ShellPage` 加载/卸载与应用窗口激活/关闭事件
- 当前进入壳层后会启动自动同步并立即首轮同步；窗口重新激活时会补做一次前台同步
- 新增同步生命周期测试并验证客户端桌面构建通过、客户端测试 45/45 通过

### Round 44

- 完成 `AI-830`
- 为 `AiStructuredResponse` 与 `AiOrchestrationService` 补齐 `itemIds`、`color`、`expectedDurationMinutes`、`targetDate` 等结构化字段解析与校验
- 将 `AiChatService` 升级为“解析 -> 校验 -> 意图分发 -> 安全执行 -> 追问保护”闭环，当前支持 `create_item`、`delete_item`、`query_items`、`answer_question`、`clarify`
- 写操作仅在高置信度且关键字段完整时执行；删除必须依赖 AI 返回的明确 `itemIds`
- 扩展 AI 服务与解析测试，并验证客户端桌面构建通过、客户端测试 41/41 通过

### Round 43

- 完成 `AI-820`
- 为 AI 聊天应用层新增 `AiChatPeriodSnapshot` 和按任意日 / 周 / 月范围读取消息的入口
- 将 `AiPage` 升级为支持日、周、月模式切换，并接入时间选择组件确认范围
- AI 页发送消息后会按当前选中范围重新加载聊天记录，而不是固定回到当日线程
- 扩展 AI 服务与 ViewModel 测试，并验证客户端桌面构建通过、客户端测试 35/35 通过

### Round 42

- 完成 `AI-810`
- 将 `AiPage` 从占位页切换为真实聊天页，当前固定展示当日聊天线程
- 新增 `IAiChatService`、`AiChatService` 和 `AiChatDaySnapshot`，负责当日消息加载、AI 请求发送和本地按日存储
- 新增 `IAiRemoteClient` 与 `AiRemoteClient`，直连用户配置的 OpenAI 兼容 `chat/completions` 端点
- 在客户端注册中心接入 AI 聊天仓储、AI 远程客户端和聊天应用服务
- 新增 AI 页 ViewModel / 服务测试并验证客户端桌面构建通过、客户端测试 33/33 通过

### Round 41

- 完成 `AI-800`
- 将设置页 `AI` 分区从只读摘要升级为可编辑配置表单
- 新增 AI Base URL、API Key、Model 输入与保存按钮
- AI 配置保存已复用 `UserSettingsService` 和同步变更链路
- 新增 `AiSettingsFormModel` 与设置页 AI 配置测试
- 验证客户端桌面构建通过，客户端测试 27/27 通过

### Round 40

- 完成 `LIST-750`
- 为列表页事项行新增左滑删除与右滑编辑入口
- 为列表页新增右下角浮动添加按钮
- 列表页浮动添加已按当前标签预填事项类型、重要标记和当天日期
- 扩展 `AddItemNavigationRequest` 与添加页预填逻辑，复用列表页和主页导航入口
- 新增列表页删除与添加预设测试，并验证客户端桌面构建与测试全部通过

### Round 39

- 完成 `LIST-740`
- 为列表页工具栏新增“More Settings”入口
- 列表页现可直接导航到设置页的 `list` 分页
- 设置页初始化已支持按导航参数直接展开指定二级分区
- 新增设置页分页联动测试并验证客户端桌面构建与测试全部通过

### Round 38

- 完成 `LIST-730`
- 为列表应用层新增主题持久化入口 `SetThemeAsync`
- 为列表页新增主题选择下拉框和四种主题预设：`default`、`sunrise`、`forest`、`slate`
- 列表页主题切换已覆盖页面背景、工具栏、分组卡片、事项行和状态提示
- 新增列表页主题状态模型 `ListPageThemeOptionViewModel`
- 新增列表页主题切换测试并验证客户端桌面构建与测试全部通过

### Round 1

- 创建 `一览-开发设计文档.md`
- 创建 `一览-开发任务拆解.md`
- 补充设计文档中的项目结构、同步契约、AI JSON 契约、验收基线

### Round 2

- 创建无记忆 AI 接力开发文档体系
- 新增 `AI-START-HERE.md`
- 新增 `AI-MASTER-PROMPT.md`
- 新增 `PROJECT-ROADMAP.md`
- 新增 `PROJECT-TODO.md`
- 新增 `PROJECT-STATUS.md`
- 新增 `PROJECT-DECISIONS.md`
- 新增 `PROJECT-HANDOFF.md`
- 新增 `PROJECT-ACCEPTANCE.md`
- 新增 `PROJECT-FILE-MAP.md`
- 新增 `PROJECT-CHANGELOG.md`
- 初始化当前项目状态为“文档准备完成，代码未开始”

### Round 3

- 强化接力规则：开始每轮工作前必须重新阅读 `“一览”用户要求.md`
- 新增最终 git 收尾要求：项目完成时需提交并推送到远端仓库；若仓库尚未就绪则记录阻塞

### Round 4

- 按 MVVM 分层重写开发路线
- 重写 `PROJECT-ROADMAP.md`
- 重写 `PROJECT-TODO.md`
- 重写 `PROJECT-ACCEPTANCE.md`
- 更新当前状态、交接说明和任务拆解，使其对齐 MVVM 顺序

### Round 5

- 调整接力规则：工具链缺失时默认由 AI 自行安装或补齐环境后继续
- 将全部 Markdown 文档迁移到 `docs/` 目录
- 修正文档内部引用路径，使其对齐 `docs/` 结构

### Round 6

- 强化执行规则：单轮对话内完成一个任务后不得停下等待用户，而应继续推进下一个可执行任务

### Round 7

- 撤回对话结束时机相关规则，不再在文档中约束 AI 何时结束输出

### Round 8

- 新增提交规则：每完成一个最小任务项后，必须更新状态文档并创建一次包含任务 ID 的 git commit

### Round 9

- 新增开始前规则：每轮工作开始前必须先检查 git 仓库状态

### Round 10

- 完成 `BOOT-100`
- 创建根解决方案 `Overview.Uno.slnx`
- 创建 Uno Platform 客户端项目骨架 `Overview.Client/`
- 创建 ASP.NET Core 服务端项目骨架 `Overview.Server/`
- 新增根级 `global.json`，固定 `Uno.Sdk` 版本并支持从仓库根目录恢复
- 验证客户端桌面目标和服务端项目可成功构建
- 修正文档中关于 git 初始化状态的过期描述

### Round 11

- 完成 `SHELL-110`
- 在客户端项目中建立 `Presentation`、`Application`、`Domain`、`Infrastructure` 分层目录
- 将默认页面迁移到 `Presentation/Pages`
- 新增 `Presentation/ViewModels/MainViewModel.cs`
- 新增轻量注册点 `Application/DependencyInjection/ClientServiceRegistry.cs`
- 验证客户端桌面目标在分层调整后仍可无警告构建

### Round 12

- 完成 `SHELL-120`
- 在服务端项目中建立 `Api`、`Application`、`Domain`、`Infrastructure` 分层目录
- 删除模板天气示例，改为最小健康检查控制器
- 新增服务端基础配置样例 `appsettings.Sample.json`
- 将应用层和基础设施层注册扩展接入服务端启动入口
- 验证服务端项目可无警告构建

### Round 13

- 完成 `SHELL-130`
- 新增 `ShellPage` 作为客户端应用壳层和底部导航入口
- 新增主页、列表页、AI 页、添加页、设置页五个占位页面
- 将客户端默认启动页切换为主页壳层
- 新增壳层相关 ViewModel 并接入轻量注册中心
- 验证客户端桌面目标在五页壳层接入后仍可无警告构建

### Round 14

- 完成 `DOMAIN-200`
- 在客户端和服务端分别新增 `Domain/Entities`、`Domain/Enums`、`Domain/ValueObjects`
- 定义统一核心领域模型：`Item`、`UserSettings`、`AiChatMessage`、`SyncChange`
- 为事项、设置、AI 聊天和同步变更补充配套枚举和值对象
- 验证客户端桌面目标和服务端项目在新领域模型接入后仍可无警告构建

### Round 15

- 完成 `DOMAIN-210`
- 在客户端和服务端分别新增 `Domain/Rules`
- 定义 `ITimeRuleService`、`TimeRuleService`、`CalendarPeriod`、`TimeBlockDefinition`
- 实现时间块生成、日周月范围计算、前后周期切换和基础标题格式化
- 验证客户端桌面目标和服务端项目在时间规则接入后仍可无警告构建

### Round 16

- 完成 `DOMAIN-220`
- 在客户端和服务端分别新增 `IReminderRuleService`、`ReminderRuleService`
- 在客户端和服务端分别新增 `ItemOccurrence`、`ScheduledReminder`
- 实现提醒触发器归一化、重复展开和提醒调度领域规则
- 支持日、周、月、年频率重复及 `Interval`、`Count`、`UntilAt` 约束
- 验证客户端桌面目标和服务端项目在提醒与重复规则接入后仍可无警告构建

### Round 17

- 完成 `DOMAIN-230`
- 在客户端和服务端分别新增 `IHomeInteractionRuleService`、`HomeInteractionRuleService`
- 在客户端和服务端分别新增 `TimelineItem`、`TimelineItemOverlap`
- 实现主页重叠透明度计算和点击命中领域规则
- 命中算法按独占区间和完全包裹规则裁决，不依赖绘制顺序
- 验证客户端桌面目标和服务端项目在主页交互规则接入后仍可无警告构建

### Round 18

- 完成 `INFRA-300`
- 为客户端项目引入 `sqlite-net-pcl`
- 新增 SQLite 连接工厂、数据库选项、表记录和 JSON 序列化辅助
- 新增事项、设置、聊天记录、同步变更四类本地仓储接口与实现
- 采用“索引列 + JSON 载荷”方式保存完整聚合
- 验证客户端桌面目标可构建；当前存在 1 条 `NETSDK1206` 警告，已记录待后续多平台复核

### Round 19

- 完成 `INFRA-310`
- 为服务端项目引入 `Npgsql.EntityFrameworkCore.PostgreSQL` 与 `Microsoft.EntityFrameworkCore.Design`
- 新增 `OverviewDbContext`、设计时工厂、JSONB 转换器和五张核心表映射
- 新增 `AuthUser` 持久化实体，为后续认证任务提供用户表基础
- 生成首个 EF Core 迁移 `20260313092718_InitialPostgreSqlInfrastructure`
- 新增仓库级 `dotnet-tools.json`，固定 `dotnet-ef` 10.0.0
- 验证服务端项目可构建、迁移可生成且可输出 idempotent 脚本
- 修正文档中“阶段编号/路线状态/SQLite 验收项”与真实进度不一致的问题

### Round 20

- 完成 `INFRA-320`
- 新增认证契约 DTO：注册、登录、发送验证码、刷新令牌
- 新增 `AuthController`，落地 `register`、`login`、`send-verification-code`、`refresh` 四个端点
- 为服务端引入 JWT Bearer 认证配置和访问令牌签发
- 新增 PBKDF2 密码哈希服务、验证码服务、刷新令牌哈希服务
- 新增 `AuthRefreshToken`、`AuthVerificationCode` 持久化实体与 EF 映射
- 生成认证补充迁移 `20260313093808_AddAuthInfrastructure`
- 更新 `appsettings.Sample.json` 认证配置样例
- 验证服务端项目可无警告构建，认证迁移可生成且可输出 idempotent 脚本

### Round 21

- 完成 `INFRA-330`
- 新增服务端同步契约 DTO：`pull` / `push` / `conflict`
- 新增 `SyncController`，落地 `GET /api/sync/pull` 与 `POST /api/sync/push`
- 为服务端新增 `SyncOptions` 并接入基础设施配置绑定
- 在同步接口中补入 JWT 用户隔离、批次限制、幂等去重和 `LastModifiedAt` 冲突返回
- 新增客户端远程同步访问目录 `Infrastructure/Api/Sync`
- 新增 `ISyncRemoteClient` 与 `SyncRemoteClient`
- 在客户端轻量注册中心注册 `HttpClient` 与同步远程访问服务
- 验证服务端与客户端项目均可无警告构建

### Round 22

- 完成 `INFRA-340`
- 在客户端新增日志抽象：`IOverviewLogger`、`IOverviewLoggerFactory`
- 在客户端新增通知抽象：`INotificationScheduler`、`NotificationScheduleRequest`
- 在客户端新增默认通知空实现：`NoOpNotificationScheduler`
- 在客户端新增小组件快照抽象：`IWidgetSnapshotStore`、`WidgetSnapshot`
- 在客户端新增默认小组件内存实现：`InMemoryWidgetSnapshotStore`
- 在客户端轻量注册中心注册日志工厂、通知调度和小组件快照服务
- 在服务端新增统一日志抽象：`IOverviewLogger`、`IOverviewLoggerFactory`
- 在服务端新增 `MicrosoftOverviewLoggerFactory` 以桥接 `Microsoft.Extensions.Logging`
- 将 `VerificationCodeService` 切换到统一日志抽象
- 验证服务端与客户端项目均可无警告构建

### Round 23

- 完成 `APP-400`
- 在客户端新增认证应用层目录 `Application/Auth`
- 新增 `AuthSession`、`VerificationCodeDispatchResult`
- 新增 `IAuthenticationService` 与 `AuthenticationService`
- 认证应用层覆盖发送验证码、注册、登录、登录态恢复、令牌刷新和登出
- 在客户端新增认证远程访问目录 `Infrastructure/Api/Auth`
- 新增 `IAuthRemoteClient`、`AuthRemoteClient` 及认证请求/响应 DTO
- 在客户端新增登录态持久化目录 `Infrastructure/Settings`
- 新增 `IAuthSessionStore` 与基于 JSON 文件的 `FileAuthSessionStore`
- 在客户端轻量注册中心接入认证远程访问、登录态存储和认证应用服务
- 验证客户端桌面目标可无警告构建

### Round 24

- 完成 `APP-410`
- 在客户端新增事项应用层目录 `Application/Items`
- 新增 `IItemService`、`ItemService`、`ItemUpsertRequest`、`ItemQueryOptions`
- 在客户端新增设置应用层目录 `Application/Settings`
- 新增 `IUserSettingsService`、`UserSettingsService`、`UserSettingsUpdateRequest`
- 在客户端新增本地设备标识存储 `IDeviceIdStore` 与 `FileDeviceIdStore`
- 事项与设置写入统一接入 `ISyncChangeRepository`，登记本地待同步变更
- 在客户端轻量注册中心接入 SQLite 仓储、设备标识存储以及事项/设置应用服务
- 验证客户端桌面目标可无警告构建

### Round 25

- 完成 `APP-420`
- 在客户端新增主页应用层目录 `Application/Home`
- 新增 `IHomeLayoutService`、`HomeLayoutService`
- 新增 `ITimeSelectionService`、`TimeSelectionService`
- 新增主页/时间选择输出模型：`HomeLayoutSnapshot`、`HomeDateColumn`、`HomeLayoutItem`、`TimeSelectionSnapshot`、`TimeSelectionWeekRow`、`TimeSelectionDateCell`
- 主页布局应用层支持周/月时间段解析、时间块快照、跨格分段、可见区裁剪和重叠透明度映射
- 时间选择应用层支持月份网格构建、日/周/月映射和前后周期导航解析
- 在客户端轻量注册中心接入 `ITimeRuleService`、`IHomeInteractionRuleService`、`IHomeLayoutService`、`ITimeSelectionService`
- 验证客户端桌面目标可无警告构建

### Round 26

- 完成 `APP-430`
- 在客户端新增列表应用层目录 `Application/Lists`
- 新增 `IListPageService`、`ListPageService`
- 新增 `ListPageQuery`、`ListPageSnapshot`、`ListPageItem`
- 列表应用层支持标签筛选、未完成/已完成分组、排序依据切换和手动重排顺序持久化
- 为补齐手动重排的持久化承载，在客户端与服务端新增 `ListManualOrderPreferences`，并将其接入 `UserSettings`
- 在服务端新增 EF Core 迁移 `20260313103139_AddListManualOrderPreferences`
- 在客户端轻量注册中心接入 `IListPageService`
- 验证客户端与服务端项目可无警告构建，且新迁移可生成并输出 idempotent 脚本

### Round 27

- 完成 `UI-500`
- 新增 `Presentation/Pages/LoginPage.xaml` 与 `LoginPage.xaml.cs`
- 新增 `Presentation/ViewModels/LoginPageViewModel.cs`
- 将客户端启动入口切换为 `LoginPage`
- 接入登录态恢复、登录/注册切换、验证码发送与登录成功后进入壳层
- 验证客户端桌面目标可无警告构建

### Round 28

- 完成 `UI-510`
- 将 `Presentation/Pages/AddItemPage.xaml` 从占位页改为真实事项表单
- 更新 `Presentation/Pages/AddItemPage.xaml.cs`，接入加载、表单同步、编辑态切换和保存动作
- 重写 `Presentation/ViewModels/AddItemPageViewModel.cs`
- 新增 `Presentation/ViewModels/AddItemFormModel.cs`
- 新增 `Presentation/ViewModels/AddItemListEntry.cs`
- 在 `ClientServiceRegistry` 为添加/编辑事项页接入认证、事项和设置服务
- 添加/编辑事项页覆盖日程、任务、备忘三类事项的新增与编辑基础字段
- 同页提供已有事项列表，点击可进入编辑模式
- 验证客户端桌面目标可无警告构建

### Round 29

- 完成 `LIST-710`
- 更新 `Presentation/Pages/ListPage.xaml` 与 `ListPage.xaml.cs`，接入排序入口、行点击完成切换和星标重要切换
- 更新 `Presentation/ViewModels/ListPageViewModel.cs`
- 新增 `Presentation/ViewModels/ListPageSortOptionViewModel.cs`
- 更新 `Presentation/ViewModels/ListPageItemEntryViewModel.cs`
- 在 `Application/Items` 补齐 `SetImportantAsync` 入口
- 在 `ClientServiceRegistry` 为列表页 ViewModel 接入事项应用服务
- 新增列表页排序与状态切换测试 `ListPageViewModelTests.cs`
- 扩展 `ListPageServiceTests.cs` 覆盖排序逻辑
- 验证客户端桌面目标可无警告构建，测试 15/15 通过

### Round 29

- 完成 `HOME-600`
- 新增独立时间选择组件 `TimeSelectionPicker`
- 新增时间选择确认事件参数 `TimeSelectionConfirmedEventArgs`
- 新增时间选择 Presentation ViewModel 与日期 / 周行展示模型
- 在 `HomePage` 接入时间选择组件宿主页，支持日 / 周 / 月模式切换与确认结果展示
- 在 `ClientServiceRegistry` 接入 `TimeSelectionViewModel`
- 时间选择组件当前支持月份前后切换、左右滑动切换、日期到周 / 月映射和确认返回
- 验证客户端桌面目标可无警告构建

### Round 29

- 完成 `UI-520`
- 新增统一事项详情组件 `Presentation/Components/ItemDetailCard.xaml`
- 新增 `Presentation/ViewModels/ItemDetailViewModel.cs`
- 更新 `AddItemPage` 以接入事项详情查看与编辑联动
- 验证客户端桌面目标可无警告构建

### Round 30

- 完成 `UI-530`
- 将 `Presentation/Pages/SettingsPage.xaml` 从占位页改为真实设置结构
- 更新 `Presentation/Pages/SettingsPage.xaml.cs`，接入设置主页、二级页切换、返回导航和摘要刷新
- 新增 `Presentation/ViewModels/SettingsPageViewModel.cs`
- 新增 `Presentation/ViewModels/SettingsSectionEntry.cs`
- 新增 `Presentation/ViewModels/SettingsSectionField.cs`
- 在 `ClientServiceRegistry` 为设置页接入认证和用户设置服务
- 修正文档与任务拆解不一致：新增 `DEC-019`，明确删除事项入口属于阶段 7 列表页
- 验证客户端桌面目标可无警告构建

### Round 29

- 完成 `UI-520`
- 新增统一事项详情组件 `Presentation/Components/ItemDetailCard`
- 新增 `Presentation/ViewModels/ItemDetailViewModel.cs`
- 更新 `AddItemPage` 与 `AddItemPageViewModel`，提供已有事项的查看入口与详情区编辑跳转
- 事项详情展示已覆盖标题、类型、时间、地点、详情、提醒、重复、状态和编辑入口
- 验证客户端桌面目标可无警告构建

### Round 29

- 完成 `APP-440`
- 在客户端新增 AI 应用层目录 `Application/Ai`
- 新增 `IAiOrchestrationService`、`AiOrchestrationService`
- 新增 `AiRequestPackage`、`AiChatCompletionRequest`、`AiChatCompletionMessage`
- 新增 `AiItemSummary`、`AiParseResult`、`AiStructuredResponse`
- 新增 `AiReminderInstruction`、`AiRepeatRuleInstruction`
- AI 应用层支持请求类型识别、相关事项摘要检索、OpenAI 兼容请求体组装和结构化响应解析入口
- 在客户端轻量注册中心接入 `IAiOrchestrationService`
- 验证客户端桌面目标可无警告构建

### Round 30

- 完成 `APP-450`
- 在客户端新增同步应用层目录 `Application/Sync`
- 新增 `ISyncOrchestrationService`、`SyncOrchestrationService`
- 新增 `SyncStatusSnapshot`、`SyncCheckpoint`、`SyncExecutionTrigger`、`SyncLifecycleState`
- 新增同步游标持久化抽象 `ISyncStateStore` 与文件实现 `FileSyncStateStore`
- 扩展 `ISyncChangeRepository`，支持待同步变更已同步标记和冲突/过期记录删除
- 同步应用层支持自动后台轮询、手动同步、状态事件、访问令牌刷新重试和 `LastModifiedAt` 冲突收敛
- 在客户端轻量注册中心接入 `ISyncOrchestrationService`、`ISyncStateStore` 与 `TimeProvider`
- 验证客户端桌面目标可无警告构建

### Round 31

- 完成 `UI-500`
- 在客户端新增登录页 `Presentation/Pages/LoginPage.xaml`
- 新增登录页代码后置和 `LoginPageViewModel`
- 应用启动入口改为 `LoginPage`
- 登录页支持登录态恢复、登录/注册切换、验证码发送和成功后导航到 `ShellPage`
- 在客户端轻量注册中心接入 `LoginPageViewModel`
- 验证客户端桌面目标可无警告构建

### Round 32

- 完成 `HOME-610`
- 新增主页时间块网格组件 `Presentation/Components/HomeTimelineGrid.xaml`
- 新增主页网格代码后置与左右滑动导航事件参数
- 重写 `Presentation/ViewModels/HomePageViewModel.cs`，接入认证态、主页布局快照和宽度驱动的周 / 月视图切换
- 重写 `Presentation/Pages/HomePage.xaml` 与 `Presentation/Pages/HomePage.xaml.cs`
- 在 `ClientServiceRegistry` 为主页接入 `IAuthenticationService` 与 `IHomeLayoutService`
- 主页当前支持顶栏周期切换、标题展开时间选择组件、窄屏周视图、宽屏月视图、时间块网格和左右滑动换页
- 验证客户端桌面目标可无警告构建

### Round 33

- 完成 `HOME-620`
- 更新 `Presentation/Components/HomeTimelineGrid.xaml`，为主页网格预留事项覆盖层宿主
- 更新 `Presentation/Components/HomeTimelineGrid.xaml.cs`，在周 / 月时间块网格上按 `TopRatio`、`HeightRatio` 与 `Opacity` 渲染事项块
- 更新 `Presentation/ViewModels/HomePageViewModel.cs`，同步主页状态文案为已完成事项覆盖层渲染
- 主页当前支持任务与日程按真实时间比例跨格显示、可见区裁剪和重叠透明度展示
- 验证客户端桌面目标可无警告构建

### Round 34

- 完成 `HOME-630`
- 新增主页交互解析服务 `Application/Home/IHomeTimelineInteractionService.cs`
- 新增主页交互解析实现 `Application/Home/HomeTimelineInteractionService.cs`
- 新增主页交互解析结果模型 `Application/Home/HomeTimelineInteractionResult.cs`
- 更新 `Presentation/Components/HomeTimelineGrid.xaml` 与 `Presentation/Components/HomeTimelineGrid.xaml.cs`，接入统一点击/长按命中层
- 新增 `Presentation/Components/HomeTimelineInteractionRequestedEventArgs.cs`
- 更新 `Presentation/Pages/HomePage.xaml` 与 `Presentation/Pages/HomePage.xaml.cs`，接入详情弹层、长按创建和长按编辑导航
- 更新 `Presentation/ViewModels/HomePageViewModel.cs`，接入详情加载与主页交互解析服务
- 新增 `Presentation/Pages/AddItemNavigationRequest.cs`
- 更新 `Presentation/Pages/AddItemPage.xaml.cs` 与 `Presentation/ViewModels/AddItemPageViewModel.cs`，支持编辑态和预填起始时间导航参数
- 新增主页交互测试工程 `tests/Overview.Client.Tests/Overview.Client.Tests.csproj`
- 新增主页交互测试 `tests/Overview.Client.Tests/HomeTimelineInteractionServiceTests.cs`
- 验证客户端桌面目标可无警告构建，且主页交互测试 4/4 通过

### Round 35

- 完成 `LIST-700`
- 将 `Presentation/Pages/ListPage.xaml` 从占位页改为真实列表筛选页
- 更新 `Presentation/Pages/ListPage.xaml.cs`，接入初始化、标签切换和空态刷新
- 重写 `Presentation/ViewModels/ListPageViewModel.cs`
- 新增 `Presentation/ViewModels/ListPageTabEntryViewModel.cs`
- 新增 `Presentation/ViewModels/ListPageItemEntryViewModel.cs`
- 在 `ClientServiceRegistry` 为列表页接入认证与列表应用服务
- 列表页当前支持我的一天、全部、任务、日程、备忘、重要事项六个标签和未完成 / 已完成分组展示
- 新增列表筛选测试 `tests/Overview.Client.Tests/ListPageServiceTests.cs`
- 验证客户端桌面目标可无警告构建，且测试项目 10/10 通过

### Round 36

- 完成 `LIST-720`
- 更新 `Application/Lists/ListPageService.cs`，使手动顺序在列表快照中真实生效，并继续保留未完成 / 已完成分组
- 更新 `Presentation/ViewModels/ListPageViewModel.cs`，新增重排模式开关、分组内上下移动和保存后刷新逻辑
- 更新 `Presentation/ViewModels/ListPageItemEntryViewModel.cs`，新增行级上下移动可用状态
- 更新 `Presentation/Pages/ListPage.xaml` 与 `Presentation/Pages/ListPage.xaml.cs`，接入顶部重排模式入口和行内上下移动按钮
- 扩展 `tests/Overview.Client.Tests/ListPageServiceTests.cs`，覆盖手动顺序持久化后的快照顺序
- 扩展 `tests/Overview.Client.Tests/ListPageViewModelTests.cs`，覆盖重排模式下的保存与刷新链路
- 验证客户端桌面目标可无警告构建，且测试项目 17/17 通过
