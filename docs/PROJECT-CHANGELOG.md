# Project Changelog

## 2026-03-13

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
