# Project Handoff

## 本轮目标

- 完成 `INFRA-300`，建立客户端 SQLite 数据层

## 本轮完成

- 在客户端新增 `Infrastructure/Persistence/Options`、`Records`、`Repositories`、`Services`
- 引入 `sqlite-net-pcl`
- 新增 `ClientSqliteOptions`、`ISqliteConnectionFactory`、`SqliteConnectionFactory`
- 新增本地表记录：
  - `ItemRecord`
  - `UserSettingsRecord`
  - `AiChatMessageRecord`
  - `SyncChangeRecord`
- 新增本地仓储接口与实现：
  - `IItemRepository` / `SqliteItemRepository`
  - `IUserSettingsRepository` / `SqliteUserSettingsRepository`
  - `IAiChatMessageRepository` / `SqliteAiChatMessageRepository`
  - `ISyncChangeRepository` / `SqliteSyncChangeRepository`
- 当前客户端 SQLite 数据层采用“索引列 + JSON 载荷”方案：
  - 先保证四类聚合都能持久化
  - 后续若查询模式变复杂，再按应用层需要做拆表细化
- 验证 `dotnet build Overview.Client/Overview.Client/Overview.Client.csproj -f net10.0-desktop` 通过，1 warning / 0 error
- 当前已知警告：
  - `NETSDK1206`，来源于 `sqlite-net-pcl` 间接依赖的 RID 资产；当前不阻塞桌面构建，但后续多平台发布前需要复核

## 本轮未完成

- 服务端 PostgreSQL 数据层
- 认证 API 契约与持久化支持

## 当前阻塞

- 无

## 已更新文件

- `Overview.Client/Overview.Client/Domain/Entities/`
- `Overview.Client/Overview.Client/Domain/Enums/`
- `Overview.Client/Overview.Client/Domain/Rules/`
- `Overview.Client/Overview.Client/Domain/ValueObjects/`
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

- 执行 `INFRA-310`：建立服务端 PostgreSQL 数据层，补实体映射、数据库上下文和迁移基础

## 接手 AI 注意事项

- 开始任何实现前，先重新阅读 `“一览”用户要求.md`
- 开始任何实现前，先检查 git 仓库状态
- 根解决方案入口是 `Overview.Uno.slnx`，不是 `.sln`
- 若从仓库根目录执行 `dotnet restore/build`，必须保留根级 `global.json`，否则 `Uno.Sdk` 无法解析
- Uno 模板还生成了 `Overview.Client/Overview.Client.sln`，当前以仓库根解决方案为主
- `DOMAIN-200`、`DOMAIN-210`、`DOMAIN-220`、`DOMAIN-230`、`INFRA-300` 已完成；后续基础设施实现应继续复用既有领域规则，而不是重写算法
- 当前时间标题只做了基础格式化规则，真正的多语言资源化应放在后续 Presentation/i18n 阶段，不要在 Domain 层引入资源依赖
- 当前提醒规则服务已提供 `NormalizeReminderConfig`、`ExpandOccurrences`、`BuildReminderSchedule` 三个入口；后续应用层和基础设施层应复用，而不是各自再写一套时间展开
- 当前主页交互规则服务已提供 `CalculateOverlapStates`、`ResolveHit` 两个入口；后续 Presentation 主页应只负责把坐标映射成时间点和可见事项集合
- 当前备忘重复以 `TargetDate` 零点为锚点，这是为了让无开始时间的备忘也能参与首版规则；如果后续要改成其他锚点，必须先更新决策或状态文档
- 当前客户端本地仓储已采用 JSON 载荷存储完整聚合，同时保留用户、时间、类型等索引列，后续如果服务端或应用层需要更细查询，再增量调整
- 当前已进入 Infrastructure 阶段，不应跳去做 Application 或 Presentation 细节，除非先完成 `INFRA-310`
- 不要跳过 Shell/Domain/Application 直接做页面细节
- 如果发现本地环境缺少构建所需工具链，先尝试自行安装或补齐环境，再继续任务
- 只有在尝试安装后仍无法继续时，才记录阻塞
- 每完成一个最小任务项后，要先更新状态文件，再立即创建一次包含任务 ID 的 git commit

## 若中断时优先检查的文件

1. `PROJECT-STATUS.md`
2. `PROJECT-TODO.md`
3. `PROJECT-HANDOFF.md`
