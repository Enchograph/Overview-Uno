# Project TODO

状态枚举：

- `todo`
- `in_progress`
- `blocked`
- `done`

## 当前任务选择规则

- 只允许执行依赖已满足的最小未完成任务
- 必须遵循 MVVM 顺序：`Shell -> Domain -> Infrastructure -> Application -> Presentation -> Platform -> QA`
- 如 `PROJECT-STATUS.md` 已指定“下一个唯一优先任务 ID”，优先执行它

---

## DOC 系列

### Task ID: DOC-000

- Title: 文档与规则体系初始化
- Phase: 阶段 0
- Depends On: 无
- Status: `done`
- Owner: `next-ai`
- Goal: 建立用户需求、设计、任务拆解和接力文档体系
- Implementation Notes: 当前仓库已具备需求、设计、任务拆解和接力核心文件
- Done When: 接力所需文档全部存在且可读
- Verification: 检查根目录关键文档存在
- Files Expected: 文档文件集合

---

## SHELL 系列

### Task ID: BOOT-100

- Title: 创建客户端与服务端项目根骨架
- Phase: 阶段 1
- Depends On: DOC-000
- Status: `done`
- Owner: `next-ai`
- Goal: 创建 Uno Platform 客户端和 ASP.NET Core 服务端的最小项目结构
- Implementation Notes: 已建立根解决方案、Uno 客户端项目、ASP.NET Core 服务端项目，并补充根级 `global.json` 以保证从仓库根目录可恢复 Uno SDK
- Done When:
  - 存在客户端项目骨架
  - 存在服务端项目骨架
  - 存在统一的基础目录结构
- Verification:
  - `dotnet restore Overview.Uno.slnx`
  - `dotnet build Overview.Server/Overview.Server.csproj`
  - `dotnet build Overview.Client/Overview.Client/Overview.Client.csproj -f net10.0-desktop`
  - 能定位客户端入口文件
  - 能定位服务端入口文件
- Files Expected:
  - 客户端工程文件
  - 服务端工程文件

### Task ID: SHELL-110

- Title: 建立客户端 MVVM 分层目录
- Phase: 阶段 1
- Depends On: BOOT-100
- Status: `done`
- Owner: `next-ai`
- Goal: 建立 Presentation、Application、Domain、Infrastructure 目录和基础注册点
- Implementation Notes: 已将默认页面迁入 `Presentation/Pages`，新增 `Presentation/ViewModels`，并创建轻量注册中心 `Application/DependencyInjection/ClientServiceRegistry.cs`；未引入业务逻辑
- Done When: 客户端分层目录存在
- Verification:
  - `dotnet build Overview.Client/Overview.Client/Overview.Client.csproj -f net10.0-desktop`
  - 可定位 MVVM 分层目录
  - 可定位基础注册点
- Files Expected:
  - 客户端分层目录
  - 基础注册点

### Task ID: SHELL-120

- Title: 建立服务端分层目录和基础配置样例
- Phase: 阶段 1
- Depends On: BOOT-100
- Status: `done`
- Owner: `next-ai`
- Goal: 建立 API、Application、Domain、Infrastructure 目录与基础配置样例
- Implementation Notes: 已移除模板天气示例，新增 `Api`、`Application`、`Domain`、`Infrastructure` 目录落点、健康检查控制器以及 `appsettings.Sample.json`；暂不实现完整业务逻辑
- Done When: 服务端分层目录存在
- Verification:
  - `dotnet build Overview.Server/Overview.Server.csproj`
  - 可定位服务端分层目录
  - 可定位基础配置样例
- Files Expected:
  - 服务端分层目录
  - 基础配置样例

### Task ID: SHELL-130

- Title: 实现 5 页应用壳层、底部导航和默认主页启动
- Phase: 阶段 1
- Depends On: SHELL-110
- Status: `done`
- Owner: `next-ai`
- Goal: 建立主页、列表页、AI 页、添加页、设置页的壳层与导航
- Implementation Notes: 已新增 `ShellPage` 作为客户端入口页，使用底部导航切换五个占位页面；默认进入 `HomePage`
- Done When:
  - 应用启动默认进入主页
  - 5 页可通过导航切换
- Verification:
  - `dotnet build Overview.Client/Overview.Client/Overview.Client.csproj -f net10.0-desktop`
  - 可定位导航壳层和默认路由
- Files Expected:
  - Shell 页面
  - 导航配置

---

## DOMAIN 系列

### Task ID: DOMAIN-200

- Title: 定义事项与用户设置领域模型
- Phase: 阶段 2
- Depends On: SHELL-110, SHELL-120
- Status: `done`
- Owner: `next-ai`
- Goal: 在客户端和服务端定义统一核心模型
- Implementation Notes: 已在客户端和服务端分别建立同构的 `Entities`、`Enums`、`ValueObjects` 目录，覆盖三类事项、全部同步设置字段、聊天记录和同步变更模型
- Done When: 核心模型完成
- Verification:
  - `dotnet build Overview.Server/Overview.Server.csproj`
  - `dotnet build Overview.Client/Overview.Client/Overview.Client.csproj -f net10.0-desktop`
  - 人工核对模型字段与设计文档一致
- Files Expected: 领域实体、枚举、值对象

### Task ID: DOMAIN-210

- Title: 实现时间块和时间范围领域规则
- Phase: 阶段 2
- Depends On: DOMAIN-200
- Status: `done`
- Owner: `next-ai`
- Goal: 定义主页时间块、周/月标题、日周月范围计算规则
- Implementation Notes: 必须支持用户可配置的一周起始日
- Done When: 时间相关规则可复用
- Verification:
  - `dotnet build Overview.Server/Overview.Server.csproj`
  - `dotnet build Overview.Client/Overview.Client/Overview.Client.csproj -f net10.0-desktop`
  - 可定位 `ITimeRuleService` 和 `TimeRuleService`
- Files Expected: 时间规则服务

### Task ID: DOMAIN-220

- Title: 实现提醒与重复领域规则
- Phase: 阶段 2
- Depends On: DOMAIN-200
- Status: `done`
- Owner: `next-ai`
- Goal: 定义提醒配置、重复规则和展开规则
- Implementation Notes: 已在客户端和服务端分别新增提醒规则服务、展开结果和值对象；当前支持提醒触发器归一化、日/周/月/年重复展开、按时区生成提醒调度
- Done When: 提醒与重复规则完成，并可被通知与列表/主页复用
- Verification:
  - `dotnet build Overview.Server/Overview.Server.csproj`
  - `dotnet build Overview.Client/Overview.Client/Overview.Client.csproj -f net10.0-desktop`
  - 可定位 `IReminderRuleService` 与 `ReminderRuleService`
  - 可定位 `ItemOccurrence` 与 `ScheduledReminder`
- Files Expected: 提醒/重复规则

### Task ID: DOMAIN-230

- Title: 实现主页重叠透明度与命中领域规则
- Phase: 阶段 2
- Depends On: DOMAIN-200, DOMAIN-210
- Status: `done`
- Owner: `next-ai`
- Goal: 实现重叠透明度和点击命中规则
- Implementation Notes: 已在客户端和服务端分别新增主页交互规则服务和值对象；当前支持透明度计算和命中裁决，且命中逻辑不依赖绘制顺序
- Done When: 命中规则和透明度规则完成
- Verification:
  - `dotnet build Overview.Server/Overview.Server.csproj`
  - `dotnet build Overview.Client/Overview.Client/Overview.Client.csproj -f net10.0-desktop`
  - 可定位 `IHomeInteractionRuleService` 与 `HomeInteractionRuleService`
  - 可定位 `TimelineItem` 与 `TimelineItemOverlap`
- Files Expected: 命中规则服务

---

## INFRA 系列

### Task ID: INFRA-300

- Title: 建立客户端 SQLite 数据层
- Phase: 阶段 3
- Depends On: DOMAIN-200
- Status: `done`
- Owner: `next-ai`
- Goal: 建立本地数据库、仓储接口和基础持久化
- Implementation Notes: 已新增 SQLite 连接工厂、初始化器、表记录和四类仓储接口/实现；当前采用 JSON 载荷 + 索引列的轻量持久化方式，覆盖事项、设置、聊天记录、同步变更
- Done When: 本地数据库结构和仓储存在
- Verification:
  - `dotnet build Overview.Client/Overview.Client/Overview.Client.csproj -f net10.0-desktop`
  - 可定位 SQLite 连接工厂与初始化逻辑
  - 可定位事项、设置、聊天记录、同步变更仓储
- Files Expected: 数据库与仓储文件

### Task ID: INFRA-310

- Title: 建立服务端 PostgreSQL 数据层
- Phase: 阶段 3
- Depends On: DOMAIN-200, SHELL-120
- Status: `done`
- Owner: `next-ai`
- Goal: 建立服务端实体映射、数据库上下文和迁移基础
- Implementation Notes: 已新增 `OverviewDbContext`、设计时工厂、EF Core/Npgsql 注册、JSONB 值转换器、五张核心表映射和初始迁移；当前额外补入 `AuthUser` 持久化实体，为后续认证任务提供用户表基础
- Done When: 服务端数据层存在
- Verification:
  - `dotnet build Overview.Server/Overview.Server.csproj`
  - `dotnet dotnet-ef migrations add InitialPostgreSqlInfrastructure --project Overview.Server/Overview.Server.csproj --startup-project Overview.Server/Overview.Server.csproj --output-dir Migrations`
  - `dotnet dotnet-ef migrations script --project Overview.Server/Overview.Server.csproj --startup-project Overview.Server/Overview.Server.csproj --idempotent`
  - 可定位数据库上下文和实体映射
- Files Expected: 服务端持久化文件

### Task ID: INFRA-320

- Title: 实现认证 API 契约和持久化支持
- Phase: 阶段 3
- Depends On: INFRA-310
- Status: `done`
- Owner: `next-ai`
- Goal: 支持邮箱验证码/密码登录所需接口和存储
- Implementation Notes: 契约应对齐开发设计文档
- Done When: 认证接口和持久化支持完成
- Verification:
  - `dotnet tool restore`
  - `dotnet build Overview.Server/Overview.Server.csproj`
  - `dotnet dotnet-ef migrations add AddAuthInfrastructure --project Overview.Server/Overview.Server.csproj --startup-project Overview.Server/Overview.Server.csproj --output-dir Migrations`
  - `dotnet dotnet-ef migrations script --project Overview.Server/Overview.Server.csproj --startup-project Overview.Server/Overview.Server.csproj --idempotent`
  - `dotnet dotnet-ef migrations list --project Overview.Server/Overview.Server.csproj --startup-project Overview.Server/Overview.Server.csproj`
- Files Expected: 认证控制器、DTO、持久化支持

### Task ID: INFRA-330

- Title: 实现同步 API 契约与远程访问层
- Phase: 阶段 3
- Depends On: INFRA-310, DOMAIN-200
- Status: `done`
- Owner: `next-ai`
- Goal: 实现 pull/push 契约和客户端远程访问封装
- Implementation Notes: 只做基础设施，不做同步编排
- Done When: 同步契约和访问层完成
- Verification:
  - `dotnet build Overview.Server/Overview.Server.csproj`
  - `dotnet build Overview.Client/Overview.Client/Overview.Client.csproj -f net10.0-desktop`
  - 可定位 `Overview.Server/Api/Controllers/SyncController.cs`
  - 可定位 `Overview.Client/Overview.Client/Infrastructure/Api/Sync/ISyncRemoteClient.cs`
  - 可定位 `Overview.Client/Overview.Client/Infrastructure/Api/Sync/SyncRemoteClient.cs`
- Files Expected: 同步接口和远程客户端

### Task ID: INFRA-340

- Title: 建立通知、小组件、日志基础设施抽象
- Phase: 阶段 3
- Depends On: DOMAIN-220
- Status: `done`
- Owner: `next-ai`
- Goal: 提供统一通知接口、小组件快照接口和日志接口
- Implementation Notes: 已在客户端新增通知调度、小组件快照和日志工厂抽象，默认采用空实现/内存实现；已在服务端新增统一日志工厂抽象并接入验证码服务
- Done When: 抽象接口存在
- Verification:
  - `dotnet build Overview.Server/Overview.Server.csproj`
  - `dotnet build Overview.Client/Overview.Client/Overview.Client.csproj -f net10.0-desktop`
  - 可定位通知、小组件、日志抽象接口与基础实现
- Files Expected: 接口与基础实现

---

## APP 系列

### Task ID: APP-400

- Title: 实现认证用例与登录态管理
- Phase: 阶段 4
- Depends On: INFRA-320
- Status: `done`
- Owner: `next-ai`
- Goal: 封装登录、登出、登录态恢复
- Implementation Notes: 供登录页调用
- Done When: 认证用例完成
- Verification:
  - `dotnet build Overview.Client/Overview.Client/Overview.Client.csproj -f net10.0-desktop`
  - 可定位 `Overview.Client/Overview.Client/Application/Auth/IAuthenticationService.cs`
  - 可定位 `Overview.Client/Overview.Client/Application/Auth/AuthenticationService.cs`
- Files Expected: 认证服务

### Task ID: APP-410

- Title: 实现事项 CRUD 与设置读写用例
- Phase: 阶段 4
- Depends On: INFRA-300
- Status: `done`
- Owner: `next-ai`
- Goal: 封装事项和设置的核心业务操作
- Implementation Notes: 所有页面必须通过用例层访问数据
- Done When: CRUD 与设置用例完成
- Verification: 可定位应用服务
- Files Expected: 事项/设置用例

### Task ID: APP-420

- Title: 实现主页布局计算与时间选择应用服务
- Phase: 阶段 4
- Depends On: DOMAIN-210, DOMAIN-230, APP-410
- Status: `done`
- Owner: `next-ai`
- Goal: 封装主页跨格布局、重叠透明度、时间选择映射
- Implementation Notes: 不在 UI 层直接写算法
- Done When: 主页相关应用服务完成
- Verification: 可定位服务和输入输出模型
- Files Expected: 主页应用服务

### Task ID: APP-430

- Title: 实现列表筛选、排序、手动重排应用服务
- Phase: 阶段 4
- Depends On: APP-410
- Status: `done`
- Owner: `next-ai`
- Goal: 支持列表标签筛选、排序依据切换和手动重排
- Implementation Notes: 包含 Microsoft TODO 风格的任务重排能力
- Done When: 列表应用服务完成
- Verification: 可定位筛选排序重排逻辑
- Files Expected: 列表应用服务

### Task ID: APP-440

- Title: 实现 AI 请求编排与事项摘要检索用例
- Phase: 阶段 4
- Depends On: APP-410, INFRA-300
- Status: `done`
- Owner: `next-ai`
- Goal: 生成 AI 所需的事项摘要、请求体和响应解析入口
- Implementation Notes: 不传历史聊天上下文
- Done When: AI 编排用例完成
- Verification: 可定位 AI 编排服务
- Files Expected: AI 应用服务

### Task ID: APP-450

- Title: 实现自动同步编排、同步状态机与冲突收敛
- Phase: 阶段 4
- Depends On: INFRA-330, INFRA-300, APP-410
- Status: `done`
- Owner: `next-ai`
- Goal: 支持自动后台同步、手动同步、状态展示和冲突收敛
- Implementation Notes: 必须满足“实时同步”目标，不能只支持手动同步
- Done When: 自动同步和同步状态机完成
- Verification:
  - 可定位同步编排和状态模型
  - `dotnet build Overview.Client/Overview.Client/Overview.Client.csproj -f net10.0-desktop`
- Files Expected: 同步应用服务

---

## UI 基础系列

### Task ID: UI-500

- Title: 实现登录页与登录态恢复
- Phase: 阶段 5
- Depends On: APP-400, SHELL-130
- Status: `done`
- Owner: `next-ai`
- Goal: 提供可用登录页和启动时会话恢复
- Implementation Notes: 与应用壳层协同工作
- Done When: 登录页完成
- Verification:
  - 可定位登录页和 ViewModel
  - `dotnet build Overview.Client/Overview.Client/Overview.Client.csproj -f net10.0-desktop`
- Files Expected: 登录页、ViewModel

### Task ID: UI-510

- Title: 实现添加/编辑事项页基础表单
- Phase: 阶段 5
- Depends On: APP-410, SHELL-130
- Status: `done`
- Owner: `next-ai`
- Goal: 支持日程、任务、备忘三类事项新增与编辑
- Implementation Notes: 已在 `AddItemPage` 接入真实表单，支持日程/任务/备忘切换、已有事项载入编辑、颜色/地点/提醒/重复/重要/完成字段编辑，并通过 `IItemService` 保存
- Done When: 三类事项表单可用
- Verification:
  - 可从 UI 进入添加/编辑页并看到对应字段
  - `dotnet build Overview.Client/Overview.Client/Overview.Client.csproj -f net10.0-desktop`
- Files Expected:
  - 页面
  - ViewModel
  - 表单模型

### Task ID: UI-520

- Title: 实现事项详情组件
- Phase: 阶段 5
- Depends On: UI-510
- Status: `done`
- Owner: `next-ai`
- Goal: 提供统一详情展示组件
- Implementation Notes: 已新增统一详情卡片组件，并在 `AddItemPage` 中提供查看与编辑联动入口，后续主页点击事项时可直接复用
- Done When: 详情组件存在
- Verification:
  - 可定位组件和展示字段
  - `dotnet build Overview.Client/Overview.Client/Overview.Client.csproj -f net10.0-desktop`
- Files Expected:
  - 详情组件
  - 详情 ViewModel

### Task ID: UI-530

- Title: 建立设置页主结构与二级页骨架
- Phase: 阶段 5
- Depends On: APP-410, SHELL-130
- Status: `done`
- Owner: `next-ai`
- Goal: 支持通用、主页、列表页、AI、账号与同步等设置入口
- Implementation Notes: 已将 `SettingsPage` 从占位页切换为真实设置结构，当前覆盖设置主页分区入口、二级页返回骨架与摘要展示；具体可编辑控件留待后续分任务补齐
- Done When: 设置主页和二级页入口存在
- Verification:
  - 可进入设置主结构
  - `dotnet build Overview.Client/Overview.Client/Overview.Client.csproj -f net10.0-desktop`
- Files Expected: 设置页与子页骨架

---

## HOME 系列

### Task ID: HOME-600

- Title: 实现时间选择组件
- Phase: 阶段 6
- Depends On: APP-420, UI-530
- Status: `done`
- Owner: `next-ai`
- Goal: 支持日/周/月选择与映射规则
- Implementation Notes: 已新增独立 `TimeSelectionPicker` 组件与 `TimeSelectionViewModel`，当前覆盖日/周/月选择、月份切换、左右滑动切换、日期到周/月映射与确认返回；`HomePage` 现作为最小宿主页承载调用入口
- Done When: 时间选择组件完成
- Verification:
  - `dotnet build Overview.Client/Overview.Client/Overview.Client.csproj -f net10.0-desktop`
  - 可定位 `Presentation/Components/TimeSelectionPicker.xaml`
  - 可定位 `Presentation/ViewModels/TimeSelectionViewModel.cs`
- Files Expected: 时间选择组件

### Task ID: HOME-610

- Title: 实现主页时间块网格与顶栏切换
- Phase: 阶段 6
- Depends On: HOME-600, UI-510
- Status: `done`
- Owner: `next-ai`
- Goal: 实现周/月视图、列日、行时间块和滑动切换
- Implementation Notes: 已将 `HomePage` 从时间选择宿主页推进为真实主页骨架；当前接入 `HomePageViewModel`、`HomeTimelineGrid` 与 `HomeLayoutService`，覆盖顶栏切换、周/月模式、时间块网格、点击标题展开时间选择组件，以及表格区域左右滑动切换周期；事项跨格渲染仍留给 `HOME-620`
- Done When: 主页网格可显示
- Verification:
  - `dotnet build Overview.Client/Overview.Client/Overview.Client.csproj -f net10.0-desktop`
  - 可定位 `Presentation/Pages/HomePage.xaml`
  - 可定位 `Presentation/Components/HomeTimelineGrid.xaml`
  - 可定位 `Presentation/ViewModels/HomePageViewModel.cs`
- Files Expected: 主页页面、网格组件

### Task ID: HOME-620

- Title: 实现事项跨格布局和重叠透明度
- Phase: 阶段 6
- Depends On: HOME-610, APP-420
- Status: `done`
- Owner: `next-ai`
- Goal: 按真实时间比例渲染任务与日程
- Implementation Notes: 已在主页时间块网格中接入事项覆盖层渲染，复用 `HomeLayoutService` 输出的 `TopRatio`、`HeightRatio` 与 `Opacity`；透明度规则与设计文档保持一致
- Done When: 跨格渲染和透明度逻辑完成
- Verification:
  - `dotnet build Overview.Client/Overview.Client/Overview.Client.csproj -f net10.0-desktop`
  - 可定位 `Overview.Client/Overview.Client/Presentation/Components/HomeTimelineGrid.xaml`
  - 可定位 `Overview.Client/Overview.Client/Presentation/Components/HomeTimelineGrid.xaml.cs`
- Files Expected: 布局计算与渲染代码

### Task ID: HOME-630

- Title: 实现主页点击命中、长按和详情交互
- Phase: 阶段 6
- Depends On: HOME-620, UI-520
- Status: `done`
- Owner: `next-ai`
- Goal: 支持命中规则、长按空白创建、长按事项编辑、点击事项详情
- Implementation Notes: 已新增独立 `HomeTimelineInteractionService` 解析主页列坐标与时间坐标，复用领域层 `HomeInteractionRuleService` 完成命中裁决；主页当前已接入点击事项详情、长按空白创建、长按事项编辑，以及添加页导航参数预填；命中规则已有独立单元测试覆盖
- Done When: 主页交互完成
- Verification:
  - `dotnet build Overview.Client/Overview.Client/Overview.Client.csproj -f net10.0-desktop`
  - `dotnet test tests/Overview.Client.Tests/Overview.Client.Tests.csproj`
  - 可定位 `Overview.Client/Overview.Client/Application/Home/HomeTimelineInteractionService.cs`
  - 可定位 `Overview.Client/Overview.Client/Presentation/Components/HomeTimelineGrid.xaml.cs`
  - 可定位 `tests/Overview.Client.Tests/HomeTimelineInteractionServiceTests.cs`
- Files Expected: 命中逻辑、测试、交互代码

---

## LIST 系列

### Task ID: LIST-700

- Title: 实现列表页标签和数据筛选
- Phase: 阶段 7
- Depends On: APP-430, UI-510
- Status: `done`
- Owner: `next-ai`
- Goal: 实现我的一天、全部、任务、日程、备忘、重要事项标签
- Implementation Notes: 先保证筛选正确
- Done When: 标签切换和数据筛选完成
- Verification: 可定位标签和筛选逻辑
- Files Expected: 列表页、筛选逻辑

### Task ID: LIST-710

- Title: 实现列表页排序、完成、重要切换
- Phase: 阶段 7
- Depends On: LIST-700
- Status: `done`
- Owner: `next-ai`
- Goal: 实现排序和状态切换
- Implementation Notes: 已对齐原始需求中的五种排序项，并在列表页接入完成/重要切换入口
- Done When: 排序与状态切换完成
- Verification:
  - `dotnet build Overview.Client/Overview.Client/Overview.Client.csproj -f net10.0-desktop`
  - `dotnet test tests/Overview.Client.Tests/Overview.Client.Tests.csproj`
  - 可定位排序逻辑和操作入口
- Files Expected: 列表交互逻辑

### Task ID: LIST-720

- Title: 实现列表页手动重新排序
- Phase: 阶段 7
- Depends On: APP-430, LIST-710
- Status: `done`
- Owner: `next-ai`
- Goal: 支持手动更改事项顺序
- Implementation Notes: 已在列表页接入独立手动重排模式，支持在未完成 / 已完成分组内通过上下移动按钮调整顺序，并将顺序持久化到 `ListManualOrderPreferences`；当前列表快照会优先应用手动顺序，再应用各排序规则的补充排序
- Done When: 手动重排完成
- Verification:
  - `dotnet build Overview.Client/Overview.Client/Overview.Client.csproj -f net10.0-desktop`
  - `dotnet test tests/Overview.Client.Tests/Overview.Client.Tests.csproj`
  - 可定位重排行为
- Files Expected: 重排逻辑

### Task ID: LIST-730

- Title: 实现列表页主题切换
- Phase: 阶段 7
- Depends On: UI-530
- Status: `done`
- Owner: `next-ai`
- Goal: 支持列表页主题选择和展示切换
- Implementation Notes: 已在列表页顶部工具栏新增主题选择下拉框，支持 `default`、`sunrise`、`forest`、`slate` 四种主题；主题写入 `UserSettings.ListPageTheme` 并即时刷新页面配色
- Done When: 主题切换完成
- Verification:
  - `dotnet build Overview.Client/Overview.Client/Overview.Client.csproj -f net10.0-desktop`
  - `dotnet test tests/Overview.Client.Tests/Overview.Client.Tests.csproj`
  - 可定位主题选择组件和应用效果
- Files Expected:
  - 主题相关页面和状态
  - 主题持久化逻辑

### Task ID: LIST-740

- Title: 实现列表页更多设置入口与列表页设置分页联动
- Phase: 阶段 7
- Depends On: UI-530
- Status: `done`
- Owner: `next-ai`
- Goal: 支持更多设置跳转到设置页列表页设置分页
- Implementation Notes: 必须与设置页联通
- Done When: 更多设置跳转完成
- Verification:
  - `dotnet build Overview.Client/Overview.Client/Overview.Client.csproj -f net10.0-desktop`
  - `dotnet test tests/Overview.Client.Tests/Overview.Client.Tests.csproj`
  - 可定位列表页更多设置入口与设置页列表分页联动
- Files Expected: 跳转与设置页联动代码

### Task ID: LIST-750

- Title: 实现列表页滑动编辑删除和浮动添加按钮
- Phase: 阶段 7
- Depends On: LIST-710, UI-510
- Status: `done`
- Owner: `next-ai`
- Goal: 完成左右滑动编辑删除与不同标签的默认填充
- Implementation Notes: 浮动按钮默认值必须随标签变化
- Done When: 列表页主流程完整
- Verification:
  - `dotnet build Overview.Client/Overview.Client/Overview.Client.csproj -f net10.0-desktop`
  - `dotnet test tests/Overview.Client.Tests/Overview.Client.Tests.csproj`
  - 可定位编辑删除和浮动按钮逻辑
- Files Expected: 列表页交互代码

---

## AI 系列

### Task ID: AI-800

- Title: 实现 AI 设置页和配置同步接入
- Phase: 阶段 8
- Depends On: UI-530, APP-450
- Status: `done`
- Owner: `next-ai`
- Goal: 支持 Base URL、API Key、Model 配置及同步
- Implementation Notes: 已在设置页 AI 分区新增 Base URL、API Key、Model 编辑表单；保存沿用 `UserSettingsService`，同步服务器地址仍保留在 Sync 分区单独维护
- Done When: AI 设置存在且接入设置存储
- Verification:
  - `dotnet build Overview.Client/Overview.Client/Overview.Client.csproj -f net10.0-desktop`
  - `dotnet test tests/Overview.Client.Tests/Overview.Client.Tests.csproj`
  - 可定位 `Overview.Client/Overview.Client/Presentation/Pages/SettingsPage.xaml`
  - 可定位 `Overview.Client/Overview.Client/Presentation/ViewModels/SettingsPageViewModel.cs`
- Files Expected: AI 设置页、配置模型

### Task ID: AI-810

- Title: 实现 AI 聊天页与按日存储
- Phase: 阶段 8
- Depends On: AI-800, INFRA-300
- Status: `done`
- Owner: `next-ai`
- Goal: 提供基础聊天 UI 和按日存储能力
- Implementation Notes: 不向模型传历史上下文
- Done When: 聊天页和聊天记录存储完成
- Verification:
  - `dotnet build Overview.Client/Overview.Client/Overview.Client.csproj -f net10.0-desktop`
  - `dotnet test tests/Overview.Client.Tests/Overview.Client.Tests.csproj`
  - 可定位 `Overview.Client/Overview.Client/Presentation/Pages/AiPage.xaml`
  - 可定位 `Overview.Client/Overview.Client/Application/Ai/AiChatService.cs`
  - 可定位 `Overview.Client/Overview.Client/Infrastructure/Persistence/Repositories/SqliteAiChatMessageRepository.cs`
- Files Expected: AI 页面、聊天记录数据层

### Task ID: AI-820

- Title: 实现 AI 按日周月聚合展示
- Phase: 阶段 8
- Depends On: AI-810, HOME-600
- Status: `done`
- Owner: `next-ai`
- Goal: 支持 AI 页按日/周/月展示聊天记录
- Implementation Notes: 复用时间选择组件或同规则逻辑
- Done When: AI 时间范围展示完成
- Verification:
  - `dotnet build Overview.Client/Overview.Client/Overview.Client.csproj -f net10.0-desktop`
  - `dotnet test tests/Overview.Client.Tests/Overview.Client.Tests.csproj`
  - 可定位 `Overview.Client/Overview.Client/Presentation/Pages/AiPage.xaml`
  - 可定位 `Overview.Client/Overview.Client/Presentation/ViewModels/AiPageViewModel.cs`
  - 可定位 `Overview.Client/Overview.Client/Application/Ai/AiChatPeriodSnapshot.cs`
- Files Expected: AI 页展示逻辑

### Task ID: AI-830

- Title: 实现 AI JSON 响应解析和自然语言增删查事项
- Phase: 阶段 8
- Depends On: AI-810, APP-440, UI-510
- Status: `done`
- Owner: `next-ai`
- Goal: 支持 create_item、delete_item、query_items、answer_question、clarify
- Implementation Notes: 已在 `AiOrchestrationService` 补齐 JSON 结构化解析字段与校验，并在 `AiChatService` 接入意图分发；当前对 create/delete 仅在高置信度且关键字段完整时执行，删除必须依赖 AI 返回的明确 `itemIds`
- Done When: AI 首版意图闭环完成
- Verification:
  - `dotnet build Overview.Client/Overview.Client/Overview.Client.csproj -f net10.0-desktop`
  - `dotnet test tests/Overview.Client.Tests/Overview.Client.Tests.csproj`
  - 可定位 `Overview.Client/Overview.Client/Application/Ai/AiOrchestrationService.cs`
  - 可定位 `Overview.Client/Overview.Client/Application/Ai/AiChatService.cs`
  - 可定位 `tests/Overview.Client.Tests/AiOrchestrationServiceTests.cs`
  - 可定位 `tests/Overview.Client.Tests/AiChatServiceTests.cs`
- Files Expected: AI 服务、解析器、用例

---

## SYNC 系列

### Task ID: SYNC-900

- Title: 接入自动后台同步
- Phase: 阶段 9
- Depends On: APP-450, UI-530
- Status: `done`
- Owner: `next-ai`
- Goal: 在不手动触发的情况下自动同步事项与设置
- Implementation Notes: 满足“全端实时同步”目标
- Done When: 自动同步接入页面生命周期和后台调度
- Verification:
  - `dotnet build Overview.Client/Overview.Client/Overview.Client.csproj -f net10.0-desktop`
  - `dotnet test tests/Overview.Client.Tests/Overview.Client.Tests.csproj`
  - 可定位 `Overview.Client/Overview.Client/Application/Sync/ISyncLifecycleCoordinator.cs`
  - 可定位 `Overview.Client/Overview.Client/Application/Sync/SyncLifecycleCoordinator.cs`
  - 可定位 `Overview.Client/Overview.Client/App.xaml.cs`
  - 可定位 `Overview.Client/Overview.Client/Presentation/Pages/ShellPage.xaml.cs`
- Files Expected: 自动同步接入代码

### Task ID: SYNC-910

- Title: 实现手动同步入口和同步状态展示
- Phase: 阶段 9
- Depends On: APP-450, UI-530
- Status: `done`
- Owner: `next-ai`
- Goal: 用户可主动触发同步并看到当前同步状态
- Implementation Notes: 手动同步是补充，不可替代自动同步
- Done When: 存在手动同步入口和状态显示
- Verification: UI 中可定位同步入口
- Files Expected: 设置页入口或状态组件

### Task ID: SYNC-920

- Title: 验证事项与设置的自动收敛
- Phase: 阶段 9
- Depends On: SYNC-900, SYNC-910
- Status: `done`
- Owner: `next-ai`
- Goal: 验证多设备在不手动触发时也能同步事项和设置
- Implementation Notes: 这是“实时同步”的强制门禁
- Done When: 自动收敛验证通过
- Verification: 可提供验证记录或测试
- Files Expected: 测试或验证记录

---

## PLATFORM 系列

### Task ID: PLATFORM-1000

- Title: 按平台实现本地通知能力映射
- Phase: 阶段 10
- Depends On: SYNC-920, INFRA-340, UI-510
- Status: `done`
- Owner: `next-ai`
- Goal: 让提醒在目标平台按平台能力触发和取消
- Implementation Notes: 已新增通知刷新服务，把事项/设置/同步后的提醒重建统一收敛到应用层；Android 已接入 `AlarmManager + BroadcastReceiver + NotificationCompat` 本地提醒映射，Desktop / Web 当前保留 `NoOp` 降级并待 `PLATFORM-1030` 补明确能力说明
- Done When: 通知能力接入完成
- Verification:
  - `dotnet build Overview.Client/Overview.Client/Overview.Client.csproj -f net10.0-desktop`
  - `dotnet test tests/Overview.Client.Tests/Overview.Client.Tests.csproj`
  - `dotnet build Overview.Client/Overview.Client/Overview.Client.csproj -f net10.0-android`
  - 可定位平台通知实现
- Files Expected: 平台通知实现

### Task ID: PLATFORM-1010

- Title: 设计并实现四类小组件的平台映射
- Phase: 阶段 10
- Depends On: HOME-630, LIST-750, AI-830, INFRA-340
- Status: `done`
- Owner: `next-ai`
- Goal: 完成主页、列表、AI、新建事项四类小组件
- Implementation Notes: 已新增统一小组件快照 / 渲染协议、文件快照存储、Android `AppWidgetProvider + RemoteViews` 平台实现，以及 `overview://...` 深链跳转；当前 Desktop / Web 的小组件降级说明继续留待 `PLATFORM-1030`
- Done When: 四类小组件可用或降级规则明确
- Verification:
  - `dotnet build Overview.Client/Overview.Client/Overview.Client.csproj -f net10.0-desktop -v q`
  - `dotnet test tests/Overview.Client.Tests/Overview.Client.Tests.csproj`
  - `dotnet build Overview.Client/Overview.Client/Overview.Client.csproj -f net10.0-android -v q`
  - 可定位快照模型和 Android 平台实现
- Files Expected: 小组件实现文件

### Task ID: PLATFORM-1020

- Title: 完成平板横竖屏适配
- Phase: 阶段 10
- Depends On: HOME-630, LIST-750, AI-830
- Status: `done`
- Owner: `next-ai`
- Goal: 完成平板主流程适配
- Implementation Notes: 保持逻辑不变，只调整布局和交互尺寸
- Done When: 平板横竖屏主流程可用
- Verification: 存在平板适配布局
- Files Expected: 自适应布局代码

### Task ID: PLATFORM-1030

- Title: 完成 Windows 与 Web 主流程适配和能力降级
- Phase: 阶段 10
- Depends On: PLATFORM-1020, PLATFORM-1000, PLATFORM-1010
- Status: `done`
- Owner: `next-ai`
- Goal: 完成 Windows 和 Web 主流程适配与降级处理
- Implementation Notes: 不得伪造不可用平台能力
- Done When: Windows 和 Web 主流程可用
- Verification: 存在平台适配逻辑和降级说明
- Files Expected: 平台分支与适配代码

---

## QA 系列

### Task ID: QA-1100

- Title: 补齐自动化测试与原始需求映射验收
- Phase: 阶段 11
- Depends On: PLATFORM-1030
- Status: `todo`
- Owner: `next-ai`
- Goal: 为核心领域规则、同步、AI、主页交互补齐测试，并按原始需求逐条验收
- Implementation Notes: 至少覆盖验收清单中的关键场景
- Done When: 核心测试和原始需求映射验收存在
- Verification: 可定位测试文件和验收映射
- Files Expected: 测试项目与测试文件

### Task ID: QA-1110

- Title: 完成性能、文档一致性和最终收尾
- Phase: 阶段 11
- Depends On: QA-1100
- Status: `todo`
- Owner: `next-ai`
- Goal: 完成最终验收和文档收尾
- Implementation Notes: 必须对齐 PROJECT-ACCEPTANCE
- Done When: 验收清单全部通过
- Verification: PROJECT-ACCEPTANCE 全部勾选完成
- Files Expected: 最终收尾更新

### Task ID: RELEASE-1120

- Title: 完成 git 仓库收尾提交与上传
- Phase: 阶段 11
- Depends On: QA-1110
- Status: `todo`
- Owner: `next-ai`
- Goal: 在项目达到最终验收后，把结果提交并上传到 git 远端仓库
- Implementation Notes: 仅在本地已初始化 git 且远端可用时执行；否则记录阻塞并等待用户完成仓库初始化
- Done When:
  - 最终代码与文档已提交
  - 已推送到远端仓库
  - 若条件未满足，阻塞已被明确记录
- Verification:
  - 可获取提交记录
  - 可确认推送完成，或确认阻塞已记录
- Files Expected:
  - 最终状态更新
  - 最终交接记录
