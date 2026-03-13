# Project Status

## 当前阶段

- 阶段编号：2
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

## 正在进行任务 ID

- 无

## 下一个唯一优先任务 ID

- `INFRA-310`

## 当前阻塞

- 无

## 最近已验证结果

- `dotnet build Overview.Server/Overview.Server.csproj` 通过，0 warning / 0 error
- `dotnet build Overview.Client/Overview.Client/Overview.Client.csproj -f net10.0-desktop` 通过，0 warning / 0 error
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
- 已确认 git 仓库已初始化，当前分支为 `main`，且已配置 `origin`
- 已修正文档与仓库真实 git 状态不一致的问题
- 阶段 3 已启动，下一步应开始服务端 PostgreSQL 数据层

## 风险与偏差

- 已解决此前“无代码骨架”风险
- 当前五页仍是占位壳层，尚未承载真实领域规则和数据流
- 服务端当前只有健康检查和配置样例，尚未进入数据库与认证实现
- 当前时间标题格式化只提供基础中英文格式，真正的 UI 本地化资源接入仍在后续 Presentation 阶段
- 当前重复展开对备忘采用“目标日期零点”为锚点，真正的编辑页输入约束和展示语义仍需后续页面与应用层配合收敛
- 当前主页命中规则已沉到 Domain，但尚未接入页面坐标映射和单元测试工程
- 当前客户端 SQLite 方案使用 `sqlite-net-pcl`；桌面构建存在 1 条 `NETSDK1206` RID 兼容性警告，后续若影响多平台发布需在平台集成前复核
- 如果跳过应用壳层，后续即使页面分别实现，也不能保证应用直接可用
- 如果不按 MVVM 顺序推进，页面逻辑会提前侵入数据和同步细节
- 如果后续实现未同步更新状态文件，接力链路会很快失效

## 接手 AI 执行准则

- 优先执行 `INFRA-310`
- 除非发现环境或工具限制，否则不要跳到 Domain、同步、主页等后续任务
- 在现有客户端本地数据层经验基础上实现服务端 PostgreSQL 数据层、实体映射和迁移基础
- 当前客户端根解决方案依赖仓库根目录 `global.json` 提供 `Uno.Sdk` 版本钉住；不要删除
- 只有在尝试补齐环境后仍无法继续时，才允许在 `PROJECT-HANDOFF.md` 标记阻塞
