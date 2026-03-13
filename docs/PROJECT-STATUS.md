# Project Status

## 当前阶段

- 阶段编号：2
- 阶段名称：Domain 层建模
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

## 正在进行任务 ID

- 无

## 下一个唯一优先任务 ID

- `DOMAIN-220`

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

## 当前真实状态摘要

- 已创建根解决方案 `Overview.Uno.slnx`
- 已创建 Uno Platform 客户端最小项目骨架 `Overview.Client/`
- 已创建 ASP.NET Core 服务端最小项目骨架 `Overview.Server/`
- 已完成客户端 MVVM 分层目录与基础注册点
- 已完成服务端分层目录与基础配置样例
- 已完成五页应用壳层、底部导航和默认主页启动
- 已完成客户端与服务端统一核心领域模型定义，覆盖事项、用户设置、AI 聊天记录和同步变更
- 已完成时间块生成、日周月范围计算、前后周期切换和周期标题格式化规则
- 尚未开始提醒与重复规则实现
- 已确认 git 仓库已初始化，当前分支为 `main`，且已配置 `origin`
- 已修正文档与仓库真实 git 状态不一致的问题
- 阶段 1 退出条件已满足，阶段 2 的首个建模任务已完成，下一步应开始时间块和时间范围规则

## 风险与偏差

- 已解决此前“无代码骨架”风险
- 当前五页仍是占位壳层，尚未承载真实领域规则和数据流
- 服务端当前只有健康检查和配置样例，尚未进入数据库与认证实现
- 当前时间标题格式化只提供基础中英文格式，真正的 UI 本地化资源接入仍在后续 Presentation 阶段
- 如果跳过应用壳层，后续即使页面分别实现，也不能保证应用直接可用
- 如果不按 MVVM 顺序推进，页面逻辑会提前侵入数据和同步细节
- 如果后续实现未同步更新状态文件，接力链路会很快失效

## 接手 AI 执行准则

- 优先执行 `DOMAIN-220`
- 除非发现环境或工具限制，否则不要跳到 Domain、同步、主页等后续任务
- 在现有模型基础上实现提醒与重复规则对象和展开规则
- 当前客户端根解决方案依赖仓库根目录 `global.json` 提供 `Uno.Sdk` 版本钉住；不要删除
- 只有在尝试补齐环境后仍无法继续时，才允许在 `PROJECT-HANDOFF.md` 标记阻塞
