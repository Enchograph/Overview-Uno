# Project Status

## 当前阶段

- 阶段编号：3
- 阶段名称：Infrastructure 层
- 阶段状态：`active`

## 当前里程碑

- 里程碑：A
- 名称：MVP
- 里程碑状态：`active`

## 已完成任务 ID

- `DOC-000`
- `BOOT-100`
- `SHELL-110`
- `SHELL-120`
- `SHELL-130`
- `DOMAIN-200`
- `DOMAIN-210`
- `DOMAIN-220`
- `DOMAIN-230`
- `INFRA-300`
- `INFRA-310`
- `INFRA-320`
- `INFRA-330`

## 正在进行任务 ID

- 无

## 下一个唯一优先任务 ID

- `INFRA-340`

## 当前阻塞

- 无

## 最近已验证结果

- `dotnet build Overview.Server/Overview.Server.csproj` 通过，0 warning / 0 error
- `dotnet build Overview.Client/Overview.Client/Overview.Client.csproj -f net10.0-desktop` 通过，0 warning / 0 error
- `dotnet dotnet-ef migrations add InitialPostgreSqlInfrastructure --project Overview.Server/Overview.Server.csproj --startup-project Overview.Server/Overview.Server.csproj --output-dir Migrations` 通过
- `dotnet dotnet-ef migrations script --project Overview.Server/Overview.Server.csproj --startup-project Overview.Server/Overview.Server.csproj --idempotent` 通过
- `dotnet dotnet-ef migrations list --project Overview.Server/Overview.Server.csproj --startup-project Overview.Server/Overview.Server.csproj` 可列出 `20260313092718_InitialPostgreSqlInfrastructure`
- 已确认服务端新增 PostgreSQL 基础设施目录：
  - `Infrastructure/Persistence/Configurations`
  - `Infrastructure/Persistence/Converters`
  - `Infrastructure/Persistence/Entities`
- 已确认服务端新增数据库上下文与设计时工厂：
  - `OverviewDbContext`
  - `OverviewDbContextDesignTimeFactory`
- 已确认服务端 PostgreSQL 映射覆盖：
  - `users`
  - `items`
  - `user_settings`
  - `sync_changes`
  - `ai_chat_messages`
- 已确认客户端与服务端均新增统一领域模型目录：
  - `Domain/Entities`
  - `Domain/Enums`
  - `Domain/ValueObjects`
- 已确认客户端与服务端均新增时间规则服务目录：
  - `Domain/Rules`
- 已确认双方均定义以下核心模型：
  - `Item`
  - `UserSettings`
  - `AiChatMessage`
  - `SyncChange`
- 已确认双方均定义时间块和时间范围规则：
  - `ITimeRuleService`
  - `TimeRuleService`
  - `CalendarPeriod`
  - `TimeBlockDefinition`
- 已确认双方均定义提醒与重复规则对象：
  - `IReminderRuleService`
  - `ReminderRuleService`
  - `ItemOccurrence`
  - `ScheduledReminder`
- 已确认提醒规则支持：
  - 提醒触发器去重、排序和基础校验
  - 基于事项展开提醒调度时间
- 已确认重复规则支持：
  - 日、周、月、年频率展开
  - `Interval`、`Count`、`UntilAt` 边界控制
  - 按事项时区计算重复起点
- 已确认双方均定义主页命中与重叠规则对象：
  - `IHomeInteractionRuleService`
  - `HomeInteractionRuleService`
  - `TimelineItem`
  - `TimelineItemOverlap`
- 已确认主页领域规则支持：
  - 根据最大重叠次数计算透明度
  - 按独占区间与包裹关系实现点击命中
  - 命中计算不依赖绘制顺序
- `dotnet build Overview.Client/Overview.Client/Overview.Client.csproj -f net10.0-desktop` 通过，1 warning / 0 error
- 已确认客户端新增 SQLite 基础设施目录：
  - `Infrastructure/Persistence/Options`
  - `Infrastructure/Persistence/Records`
  - `Infrastructure/Persistence/Repositories`
  - `Infrastructure/Persistence/Services`
- 已确认客户端本地数据层已覆盖：
  - `Item`
  - `UserSettings`
  - `AiChatMessage`
  - `SyncChange`
- `dotnet tool restore` 通过，可恢复本地 `dotnet-ef` 10.0.0
- `dotnet build Overview.Server/Overview.Server.csproj` 通过，0 warning / 0 error
- `dotnet dotnet-ef migrations add AddAuthInfrastructure --project Overview.Server/Overview.Server.csproj --startup-project Overview.Server/Overview.Server.csproj --output-dir Migrations` 通过
- `dotnet dotnet-ef migrations script --project Overview.Server/Overview.Server.csproj --startup-project Overview.Server/Overview.Server.csproj --idempotent` 通过
- `dotnet dotnet-ef migrations list --project Overview.Server/Overview.Server.csproj --startup-project Overview.Server/Overview.Server.csproj` 可列出：
  - `20260313092718_InitialPostgreSqlInfrastructure`
  - `20260313093808_AddAuthInfrastructure`
- 已确认服务端新增认证 API 契约目录：
  - `Api/Contracts/Auth`
- 已确认服务端新增认证控制器：
  - `AuthController`
- 已确认服务端新增认证基础设施目录：
  - `Infrastructure/Identity`
- 已确认服务端新增认证持久化表映射：
  - `auth_refresh_tokens`
  - `auth_verification_codes`
- 已确认认证接口已覆盖：
  - `POST /api/auth/send-verification-code`
  - `POST /api/auth/register`
  - `POST /api/auth/login`
  - `POST /api/auth/refresh`
- 已确认认证基础设施已支持：
  - PBKDF2 密码哈希
  - JWT 访问令牌生成
  - 刷新令牌持久化与轮换
  - 邮箱验证码持久化、过期校验与消费
- `dotnet build Overview.Server/Overview.Server.csproj` 通过，0 warning / 0 error
- `dotnet build Overview.Client/Overview.Client/Overview.Client.csproj -f net10.0-desktop` 通过，0 warning / 0 error
- 已确认服务端新增同步 API 契约目录：
  - `Api/Contracts/Sync`
- 已确认服务端新增同步控制器：
  - `SyncController`
- 已确认服务端同步接口已覆盖：
  - `GET /api/sync/pull?since=...`
  - `POST /api/sync/push`
- 已确认服务端同步基础设施已支持：
  - 基于 JWT 当前用户的数据隔离
  - `push` 批次大小限制
  - 基于 `SyncChange.Id` 的幂等去重
  - 按 `LastModifiedAt` 返回服务端冲突快照
- 已确认客户端新增远程同步访问目录：
  - `Infrastructure/Api/Sync`
- 已确认客户端同步远程访问层已提供：
  - `ISyncRemoteClient`
  - `SyncRemoteClient`
  - `pull/push` 契约 DTO

## 当前真实状态摘要

- 已创建根解决方案 `Overview.Uno.slnx`
- 已创建 Uno Platform 客户端最小项目骨架 `Overview.Client/`
- 已创建 ASP.NET Core 服务端最小项目骨架 `Overview.Server/`
- 已完成客户端 MVVM 分层目录与基础注册点
- 已完成服务端分层目录与基础配置样例
- 已完成五页应用壳层、底部导航和默认主页启动
- 已完成客户端与服务端统一核心领域模型定义，覆盖事项、用户设置、AI 聊天记录和同步变更
- 已完成时间块生成、日周月范围计算、前后周期切换和周期标题格式化规则
- 已完成提醒配置归一化、重复展开和提醒调度领域规则
- 已完成主页重叠透明度与点击命中领域规则
- 已完成客户端 SQLite 数据层、数据库初始化与四类本地仓储骨架
- 已完成服务端 PostgreSQL 数据层、EF Core 映射与初始迁移基线
- 已完成服务端认证 API 契约、认证控制器和验证码/刷新令牌持久化支持
- 已完成服务端同步 API 契约、同步控制器和基础冲突检测逻辑
- 已完成客户端远程同步访问封装与轻量注册中心接入
- 已确认 git 仓库已初始化，当前分支为 `main`，且已配置 `origin`
- 已修正文档与仓库真实 git 状态不一致的问题
- 已修正文档内部“阶段编号仍为 2 / 路线仍显示阶段 3 未开始 / SQLite 验收未勾选”的状态偏差
- 下一步应进入通知、小组件、日志基础设施抽象

## 风险与偏差

- 已解决此前“无代码骨架”风险
- 当前五页仍是占位壳层，尚未承载真实领域规则和数据流
- 服务端当前已具备认证与同步基础设施，但事项 CRUD 与设置独立读写控制器仍未实现
- 当前时间标题格式化只提供基础中英文格式，真正的 UI 本地化资源接入仍在后续 Presentation 阶段
- 当前重复展开对备忘采用“目标日期零点”为锚点，真正的编辑页输入约束和展示语义仍需后续页面与应用层配合收敛
- 当前主页命中规则已沉到 Domain，但尚未接入页面坐标映射和单元测试工程
- 当前客户端 SQLite 方案使用 `sqlite-net-pcl`；桌面构建存在 1 条 `NETSDK1206` RID 兼容性警告，后续若影响多平台发布需在平台集成前复核
- 本地未运行 PostgreSQL 实例，因此本轮只验证了“可生成迁移、可输出脚本”，尚未执行 `database update`
- 当前 `send-verification-code` 仅把验证码写入数据库并记录到服务端日志，尚未接入真实 SMTP/邮件供应商发送链路
- 当前同步控制器直接操作 `DbContext` 完成基础设施闭环；真正的自动同步编排、状态机和本地收敛仍在后续 Application/Sync 阶段
- 如果跳过应用壳层，后续即使页面分别实现，也不能保证应用直接可用
- 如果不按 MVVM 顺序推进，页面逻辑会提前侵入数据和同步细节
- 如果后续实现未同步更新状态文件，接力链路会很快失效

## 接手 AI 执行准则

- 优先执行 `INFRA-340`
- 除非发现环境或工具限制，否则不要跳到 Domain、同步、主页等后续任务
- 在现有认证与同步基础设施之上继续补通知、小组件、日志抽象
- 当前客户端根解决方案依赖仓库根目录 `global.json` 提供 `Uno.Sdk` 版本钉住；不要删除
- 迁移工具通过仓库根目录 `dotnet-tools.json` 固定；后续执行 EF CLI 前先运行 `dotnet tool restore`
- 只有在尝试补齐环境后仍无法继续时，才允许在 `PROJECT-HANDOFF.md` 标记阻塞
