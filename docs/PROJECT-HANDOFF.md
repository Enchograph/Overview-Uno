# Project Handoff

## 本轮目标

- 完成 `INFRA-310`，建立服务端 PostgreSQL 数据层

## 本轮完成

- 在服务端新增 `Infrastructure/Persistence/Configurations`、`Converters`、`Entities`
- 引入 `Npgsql.EntityFrameworkCore.PostgreSQL` 和 `Microsoft.EntityFrameworkCore.Design`
- 新增 `OverviewDbContext` 与 `OverviewDbContextDesignTimeFactory`
- 新增 PostgreSQL 映射配置：
  - `AuthUserConfiguration`
  - `ItemConfiguration`
  - `UserSettingsConfiguration`
  - `SyncChangeConfiguration`
  - `AiChatMessageConfiguration`
- 新增 JSONB 转换基础：
  - `ServerJsonSerializer`
  - `JsonValueConverterExtensions`
- 新增持久化实体：
  - `AuthUser`
- 生成首个迁移：
  - `20260313092718_InitialPostgreSqlInfrastructure`
- 新增仓库级 EF 工具清单：
  - `dotnet-tools.json`
- 验证结果：
  - `dotnet build Overview.Server/Overview.Server.csproj` 通过，0 warning / 0 error
  - `dotnet dotnet-ef migrations add InitialPostgreSqlInfrastructure --project Overview.Server/Overview.Server.csproj --startup-project Overview.Server/Overview.Server.csproj --output-dir Migrations` 通过
  - `dotnet dotnet-ef migrations script --project Overview.Server/Overview.Server.csproj --startup-project Overview.Server/Overview.Server.csproj --idempotent` 通过

## 本轮未完成

- 认证 API 契约与持久化支持
- 同步 API 契约与远程访问层

## 当前阻塞

- 无

## 已更新文件

- `Overview.Client/Overview.Client/Domain/Entities/`
- `Overview.Client/Overview.Client/Domain/Enums/`
- `Overview.Client/Overview.Client/Domain/Rules/`
- `Overview.Client/Overview.Client/Domain/ValueObjects/`
- `Overview.Server/Infrastructure/Persistence/`
- `Overview.Server/Migrations/`
- `dotnet-tools.json`
- `Overview.Server/Domain/Entities/`
- `Overview.Server/Domain/Enums/`
- `Overview.Server/Domain/Rules/`
- `Overview.Server/Domain/ValueObjects/`
- `docs/PROJECT-STATUS.md`
- `docs/PROJECT-TODO.md`
- `docs/PROJECT-HANDOFF.md`
- `docs/PROJECT-CHANGELOG.md`
- `docs/PROJECT-FILE-MAP.md`

## 下一步唯一推荐动作

- 执行 `INFRA-320`：实现认证 API 契约和持久化支持

## 接手 AI 注意事项

- 开始任何实现前，先重新阅读 `“一览”用户要求.md`
- 开始任何实现前，先检查 git 仓库状态
- 根解决方案入口是 `Overview.Uno.slnx`，不是 `.sln`
- 若从仓库根目录执行 `dotnet restore/build`，必须保留根级 `global.json`，否则 `Uno.Sdk` 无法解析
- Uno 模板还生成了 `Overview.Client/Overview.Client.sln`，当前以仓库根解决方案为主
- `DOMAIN-200`、`DOMAIN-210`、`DOMAIN-220`、`DOMAIN-230`、`INFRA-300`、`INFRA-310` 已完成；后续基础设施实现应继续复用既有领域规则，而不是重写算法
- 当前时间标题只做了基础格式化规则，真正的多语言资源化应放在后续 Presentation/i18n 阶段，不要在 Domain 层引入资源依赖
- 当前提醒规则服务已提供 `NormalizeReminderConfig`、`ExpandOccurrences`、`BuildReminderSchedule` 三个入口；后续应用层和基础设施层应复用，而不是各自再写一套时间展开
- 当前主页交互规则服务已提供 `CalculateOverlapStates`、`ResolveHit` 两个入口；后续 Presentation 主页应只负责把坐标映射成时间点和可见事项集合
- 当前备忘重复以 `TargetDate` 零点为锚点，这是为了让无开始时间的备忘也能参与首版规则；如果后续要改成其他锚点，必须先更新决策或状态文档
- 当前客户端本地仓储已采用 JSON 载荷存储完整聚合，同时保留用户、时间、类型等索引列，后续如果服务端或应用层需要更细查询，再增量调整
- 服务端当前使用 EF Core 10 + Npgsql，值对象和快照字段以 `jsonb` 存储；后续若要把某些字段拆成列，先确认是否属于当前最小任务边界
- `AuthUser` 目前只提供用户表基础字段，验证码、刷新令牌、密码策略细节应在 `INFRA-320` 再补，不要在本轮基础上擅自扩范围
- 当前已进入 Infrastructure 阶段，不应跳去做 Application 或 Presentation 细节，除非先完成 `INFRA-320`
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
