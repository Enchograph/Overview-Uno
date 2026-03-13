# Project Handoff

## 本轮目标

- 完成 `APP-450`，实现自动同步编排、同步状态机与冲突收敛

## 本轮完成

- 在客户端新增同步应用层目录 `Application/Sync`
- 新增同步编排服务接口 `ISyncOrchestrationService`
- 新增同步编排服务实现 `SyncOrchestrationService`
- 新增同步应用层状态模型：
  - `SyncStatusSnapshot`
  - `SyncCheckpoint`
  - `SyncExecutionTrigger`
  - `SyncLifecycleState`
- 同步应用层已覆盖：
  - 手动同步入口
  - 自动后台轮询同步
  - 同步状态变更事件
  - 本地增量 `push` 与游标化 `pull`
  - 访问令牌过期后的刷新重试
  - 401/403 失效后登录态清理与重新登录要求
  - `LastModifiedAt` 冲突的服务端覆盖回写
  - 本地同步游标 JSON 持久化
- 在客户端同步变更仓储新增：
  - 已同步标记能力
  - 冲突/过期待同步项删除能力
- 在客户端 `Infrastructure/Settings` 新增：
  - `ISyncStateStore`
  - `FileSyncStateStore`
- 在客户端 `ClientServiceRegistry` 注册：
  - `ISyncOrchestrationService`
  - `ISyncStateStore`
  - `TimeProvider`
- 验证结果：
  - `dotnet build Overview.Client/Overview.Client/Overview.Client.csproj -f net10.0-desktop` 通过，0 warning / 0 error

## 本轮未完成

- `UI-500` 及后续 Presentation 层任务
- 登录页与设置页 Presentation 接入
- 真实邮件发送提供程序接入
- 通知平台映射
- 小组件平台映射

## 当前阻塞

- 无

## 已更新文件

- `Overview.Client/Overview.Client/Application/Sync/`
- `Overview.Client/Overview.Client/Infrastructure/Settings/ISyncStateStore.cs`
- `Overview.Client/Overview.Client/Infrastructure/Settings/FileSyncStateStore.cs`
- `Overview.Client/Overview.Client/Infrastructure/Persistence/Repositories/ISyncChangeRepository.cs`
- `Overview.Client/Overview.Client/Infrastructure/Persistence/Repositories/SqliteSyncChangeRepository.cs`
- `Overview.Client/Overview.Client/Application/DependencyInjection/ClientServiceRegistry.cs`
- `docs/PROJECT-STATUS.md`
- `docs/PROJECT-TODO.md`
- `docs/PROJECT-HANDOFF.md`
- `docs/PROJECT-CHANGELOG.md`
- `docs/PROJECT-FILE-MAP.md`

## 下一步唯一推荐动作

- 执行 `UI-500`：实现登录页与登录态恢复

## 接手 AI 注意事项

- 开始任何实现前，先重新阅读 `“一览”用户要求.md`
- 开始任何实现前，先检查 git 仓库状态
- 根解决方案入口是 `Overview.Uno.slnx`，不是 `.sln`
- 若从仓库根目录执行 `dotnet restore/build`，必须保留根级 `global.json`，否则 `Uno.Sdk` 无法解析
- Uno 模板还生成了 `Overview.Client/Overview.Client.sln`，当前以仓库根解决方案为主
- `DOMAIN-200`、`DOMAIN-210`、`DOMAIN-220`、`DOMAIN-230`、`INFRA-300`、`INFRA-310` 已完成；后续基础设施实现应继续复用既有领域规则，而不是重写算法
- `INFRA-320`、`INFRA-330`、`INFRA-340`、`APP-400`、`APP-410` 已完成；当前服务端已具备认证与同步 API 基础设施，客户端已具备认证应用层、事项/设置应用层、远程同步访问封装以及通知/小组件/日志抽象
- `APP-420` 已完成；当前客户端已具备主页布局快照与时间选择映射应用服务，后续主页/时间选择 UI 不应在页面层重写算法
- `APP-430` 已完成；当前客户端已具备列表筛选、排序、分组和手动重排应用服务，后续列表页 UI 不应在页面层重写筛选/排序逻辑
- `APP-440` 已完成；当前客户端已具备 AI 请求编排、事项摘要检索和结构化响应解析入口，后续 AI 页与 AI 远程客户端应直接复用这些模型/服务，而不是在页面层重新拼提示词和 JSON 解析
- `APP-450` 已完成；当前客户端已具备自动/手动同步编排、同步状态模型、增量同步游标持久化以及基于 `LastModifiedAt` 的冲突收敛，后续 Presentation 层只应消费状态与命令，不应在页面层自行拼 `push/pull`
- 当前时间标题只做了基础格式化规则，真正的多语言资源化应放在后续 Presentation/i18n 阶段，不要在 Domain 层引入资源依赖
- 当前提醒规则服务已提供 `NormalizeReminderConfig`、`ExpandOccurrences`、`BuildReminderSchedule` 三个入口；后续应用层和基础设施层应复用，而不是各自再写一套时间展开
- 当前主页交互规则服务已提供 `CalculateOverlapStates`、`ResolveHit` 两个入口；后续 Presentation 主页应只负责把坐标映射成时间点和可见事项集合
- 当前备忘重复以 `TargetDate` 零点为锚点，这是为了让无开始时间的备忘也能参与首版规则；如果后续要改成其他锚点，必须先更新决策或状态文档
- 当前客户端本地仓储已采用 JSON 载荷存储完整聚合，同时保留用户、时间、类型等索引列，后续如果服务端或应用层需要更细查询，再增量调整
- 服务端当前使用 EF Core 10 + Npgsql，值对象和快照字段以 `jsonb` 存储；后续若要把某些字段拆成列，先确认是否属于当前最小任务边界
- 当前 `SyncController` 仍直接使用 `OverviewDbContext` 完成基础设施级 pull/push；客户端应用层已补齐自动后台同步、手动同步入口、同步状态机和本地冲突收敛
- 当前 `send-verification-code` 端点会生成 6 位验证码、持久化哈希，并把明文验证码写入服务端日志；这是为了完成当前最小基础设施任务，真实邮件发送链路仍需后续补齐
- 当前客户端登录态以 `LocalApplicationData/Overview.Client/auth-session.json` 持久化，用于支持首版恢复与刷新；尚未做平台安全存储适配
- 当前同步游标状态以 `LocalApplicationData/Overview.Client/sync-state.json` 持久化，用于支持增量 `pull` 与同步状态展示恢复
- 当前密码哈希采用 PBKDF2-SHA256，自定义格式为 `PBKDF2.{saltHex}.{hashHex}`
- 当前刷新令牌按单条记录持久化，`refresh` 端点会吊销旧令牌并写入替换后的哈希
- 当前 Application 阶段已完成，下一步进入 Presentation 基础页面
- 下一步不要回头扩写平台映射，先完成 `UI-500`
- 当前 `IUserSettingsService.GetAsync` 若本地不存在记录，会返回默认对象但不落库；只有 `SaveAsync` 才会生成同步变更
- 运行 EF CLI 前先执行 `dotnet tool restore`
- 本地没有可连接的 PostgreSQL 实例；如果下轮需要验证 `database update` 或真实读写，请先启动数据库或调整连接串
- 不要跳过 Shell/Domain/Application 直接做页面细节
- 如果发现本地环境缺少构建所需工具链，先尝试自行安装或补齐环境，再继续任务
- 只有在尝试安装后仍无法继续时，才记录阻塞
- 每完成一个最小任务项后，要先更新状态文件，再立即创建一次包含任务 ID 的 git commit

## 若中断时优先检查的文件

1. `PROJECT-STATUS.md`
2. `PROJECT-TODO.md`
3. `PROJECT-HANDOFF.md`
