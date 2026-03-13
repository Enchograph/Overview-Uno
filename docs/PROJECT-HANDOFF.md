# Project Handoff

## 本轮目标

- 完成 `APP-430`，实现列表筛选、排序、手动重排应用服务

## 本轮完成

- 在客户端新增列表应用层目录 `Application/Lists`
- 新增列表应用服务接口 `IListPageService`
- 新增列表应用服务实现 `ListPageService`
- 新增列表应用层输入输出模型：
  - `ListPageQuery`
  - `ListPageSnapshot`
  - `ListPageItem`
- 列表应用层已覆盖：
  - 列表标签页筛选
  - 未完成/已完成分组输出
  - 排序依据切换
  - Microsoft TODO 风格的手动重排顺序持久化
  - 基于用户时区的“我的一天”与“今天执行者”日期判断
- 为修复“手动重排无持久化承载”的代码/文档缺口，在客户端与服务端同步新增：
  - `ListManualOrderPreferences`
  - `UserSettings.ListManualOrder`
- 在服务端新增迁移：
  - `20260313103139_AddListManualOrderPreferences`
- 在客户端 `ClientServiceRegistry` 注册：
  - `IListPageService`
- 验证结果：
  - `dotnet tool restore` 通过
  - `dotnet build Overview.Server/Overview.Server.csproj` 通过，0 warning / 0 error
  - `dotnet build Overview.Client/Overview.Client/Overview.Client.csproj -f net10.0-desktop` 通过，0 warning / 0 error
  - `dotnet dotnet-ef migrations add AddListManualOrderPreferences --project Overview.Server/Overview.Server.csproj --startup-project Overview.Server/Overview.Server.csproj --output-dir Migrations` 通过
  - `dotnet dotnet-ef migrations script --project Overview.Server/Overview.Server.csproj --startup-project Overview.Server/Overview.Server.csproj --idempotent` 通过
  - `dotnet dotnet-ef migrations list --project Overview.Server/Overview.Server.csproj --startup-project Overview.Server/Overview.Server.csproj` 可列出新迁移；由于本地未启动 PostgreSQL，命令会提示无法连接 `127.0.0.1:5432`，但迁移名仍可输出

## 本轮未完成

- `APP-440` 及后续 Application 层用例
- 登录页与设置页 Presentation 接入
- 真实邮件发送提供程序接入
- 通知平台映射
- 小组件平台映射

## 当前阻塞

- 无

## 已更新文件

- `Overview.Client/Overview.Client/Application/DependencyInjection/ClientServiceRegistry.cs`
- `Overview.Client/Overview.Client/Application/Lists/`
- `Overview.Client/Overview.Client/Application/Settings/`
- `Overview.Client/Overview.Client/Domain/Entities/UserSettings.cs`
- `Overview.Client/Overview.Client/Domain/ValueObjects/ListManualOrderPreferences.cs`
- `Overview.Server/Api/Controllers/SyncController.cs`
- `Overview.Server/Domain/Entities/UserSettings.cs`
- `Overview.Server/Domain/ValueObjects/ListManualOrderPreferences.cs`
- `Overview.Server/Infrastructure/Persistence/Configurations/UserSettingsConfiguration.cs`
- `Overview.Server/Migrations/`
- `docs/PROJECT-STATUS.md`
- `docs/PROJECT-TODO.md`
- `docs/PROJECT-HANDOFF.md`
- `docs/PROJECT-CHANGELOG.md`
- `docs/PROJECT-FILE-MAP.md`

## 下一步唯一推荐动作

- 执行 `APP-440`：实现 AI 请求编排与事项摘要检索用例

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
- 本轮发现代码与需求存在显式差异：原模型没有可同步的手动重排承载，无法满足 `APP-430` 的“重排”要求；已通过 `UserSettings.ListManualOrder` 和服务端迁移补齐
- 当前时间标题只做了基础格式化规则，真正的多语言资源化应放在后续 Presentation/i18n 阶段，不要在 Domain 层引入资源依赖
- 当前提醒规则服务已提供 `NormalizeReminderConfig`、`ExpandOccurrences`、`BuildReminderSchedule` 三个入口；后续应用层和基础设施层应复用，而不是各自再写一套时间展开
- 当前主页交互规则服务已提供 `CalculateOverlapStates`、`ResolveHit` 两个入口；后续 Presentation 主页应只负责把坐标映射成时间点和可见事项集合
- 当前备忘重复以 `TargetDate` 零点为锚点，这是为了让无开始时间的备忘也能参与首版规则；如果后续要改成其他锚点，必须先更新决策或状态文档
- 当前客户端本地仓储已采用 JSON 载荷存储完整聚合，同时保留用户、时间、类型等索引列，后续如果服务端或应用层需要更细查询，再增量调整
- 服务端当前使用 EF Core 10 + Npgsql，值对象和快照字段以 `jsonb` 存储；后续若要把某些字段拆成列，先确认是否属于当前最小任务边界
- 当前 `SyncController` 直接使用 `OverviewDbContext` 完成基础设施级 pull/push；自动后台同步、手动同步入口、同步状态机和本地冲突收敛还没有开始
- 当前 `send-verification-code` 端点会生成 6 位验证码、持久化哈希，并把明文验证码写入服务端日志；这是为了完成当前最小基础设施任务，真实邮件发送链路仍需后续补齐
- 当前客户端登录态以 `LocalApplicationData/Overview.Client/auth-session.json` 持久化，用于支持首版恢复与刷新；尚未做平台安全存储适配
- 当前密码哈希采用 PBKDF2-SHA256，自定义格式为 `PBKDF2.{saltHex}.{hashHex}`
- 当前刷新令牌按单条记录持久化，`refresh` 端点会吊销旧令牌并写入替换后的哈希
- 当前已进入 Application 阶段，不应跳去做 Presentation 或 Platform 细节
- 下一步不要回头扩写列表 UI 或平台映射，先完成 `APP-440`
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
