# Project Handoff

## 本轮目标

- 完成 `INFRA-330`，实现同步 API 契约与远程访问层

## 本轮完成

- 在服务端新增同步契约目录 `Api/Contracts/Sync`
- 新增同步控制器 `Api/Controllers/SyncController.cs`
- 新增同步配置对象 `Infrastructure/Configuration/SyncOptions.cs`
- 在服务端基础设施注册中接入 `Sync` 配置绑定
- 为服务端同步接口补入以下基础能力：
  - `GET /api/sync/pull?since=...`
  - `POST /api/sync/push`
  - 基于 JWT 当前用户的数据隔离
  - 基于 `SyncChange.Id` 的幂等去重
  - 按 `LastModifiedAt` 返回服务端冲突快照
  - `push` 批次大小限制
- 在客户端新增远程同步访问目录 `Infrastructure/Api/Sync`
- 新增客户端同步远程访问接口 `ISyncRemoteClient`
- 新增客户端同步远程访问实现 `SyncRemoteClient`
- 新增客户端 `pull/push` 同步契约 DTO
- 在 `ClientServiceRegistry` 注册 `HttpClient` 与 `ISyncRemoteClient`
- 验证结果：
  - `dotnet build Overview.Server/Overview.Server.csproj` 通过，0 warning / 0 error
  - `dotnet build Overview.Client/Overview.Client/Overview.Client.csproj -f net10.0-desktop` 通过，0 warning / 0 error

## 本轮未完成

- 通知、小组件、日志基础设施抽象
- 真实邮件发送提供程序接入

## 当前阻塞

- 无

## 已更新文件

- `Overview.Server/Api/Contracts/Sync/`
- `Overview.Server/Api/Controllers/SyncController.cs`
- `Overview.Server/Infrastructure/Configuration/SyncOptions.cs`
- `Overview.Server/Infrastructure/DependencyInjection/InfrastructureServiceCollectionExtensions.cs`
- `Overview.Client/Overview.Client/Application/DependencyInjection/ClientServiceRegistry.cs`
- `Overview.Client/Overview.Client/Infrastructure/Api/Sync/`
- `docs/PROJECT-STATUS.md`
- `docs/PROJECT-TODO.md`
- `docs/PROJECT-HANDOFF.md`
- `docs/PROJECT-CHANGELOG.md`
- `docs/PROJECT-FILE-MAP.md`

## 下一步唯一推荐动作

- 执行 `INFRA-340`：建立通知、小组件、日志基础设施抽象

## 接手 AI 注意事项

- 开始任何实现前，先重新阅读 `“一览”用户要求.md`
- 开始任何实现前，先检查 git 仓库状态
- 根解决方案入口是 `Overview.Uno.slnx`，不是 `.sln`
- 若从仓库根目录执行 `dotnet restore/build`，必须保留根级 `global.json`，否则 `Uno.Sdk` 无法解析
- Uno 模板还生成了 `Overview.Client/Overview.Client.sln`，当前以仓库根解决方案为主
- `DOMAIN-200`、`DOMAIN-210`、`DOMAIN-220`、`DOMAIN-230`、`INFRA-300`、`INFRA-310` 已完成；后续基础设施实现应继续复用既有领域规则，而不是重写算法
- `INFRA-320`、`INFRA-330` 已完成；当前服务端已具备认证与同步 API 基础设施，客户端已具备远程同步访问封装
- 当前时间标题只做了基础格式化规则，真正的多语言资源化应放在后续 Presentation/i18n 阶段，不要在 Domain 层引入资源依赖
- 当前提醒规则服务已提供 `NormalizeReminderConfig`、`ExpandOccurrences`、`BuildReminderSchedule` 三个入口；后续应用层和基础设施层应复用，而不是各自再写一套时间展开
- 当前主页交互规则服务已提供 `CalculateOverlapStates`、`ResolveHit` 两个入口；后续 Presentation 主页应只负责把坐标映射成时间点和可见事项集合
- 当前备忘重复以 `TargetDate` 零点为锚点，这是为了让无开始时间的备忘也能参与首版规则；如果后续要改成其他锚点，必须先更新决策或状态文档
- 当前客户端本地仓储已采用 JSON 载荷存储完整聚合，同时保留用户、时间、类型等索引列，后续如果服务端或应用层需要更细查询，再增量调整
- 服务端当前使用 EF Core 10 + Npgsql，值对象和快照字段以 `jsonb` 存储；后续若要把某些字段拆成列，先确认是否属于当前最小任务边界
- 当前 `SyncController` 直接使用 `OverviewDbContext` 完成基础设施级 pull/push；自动后台同步、手动同步入口、同步状态机和本地冲突收敛还没有开始
- 当前 `send-verification-code` 端点会生成 6 位验证码、持久化哈希，并把明文验证码写入服务端日志；这是为了完成当前最小基础设施任务，真实邮件发送链路仍需后续补齐
- 当前密码哈希采用 PBKDF2-SHA256，自定义格式为 `PBKDF2.{saltHex}.{hashHex}`
- 当前刷新令牌按单条记录持久化，`refresh` 端点会吊销旧令牌并写入替换后的哈希
- 当前已进入 Infrastructure 阶段，不应跳去做 Application 或 Presentation 细节，除非先完成当前剩余基础设施任务
- 下一步不要回头扩写登录页、同步编排或页面状态，先完成 `INFRA-340`
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
