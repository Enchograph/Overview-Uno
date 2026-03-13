# Project Handoff

## 本轮目标

- 完成 `DOMAIN-220`，实现提醒与重复领域规则

## 本轮完成

- 在客户端新增 `Domain/Rules/IReminderRuleService.cs` 和 `Domain/Rules/ReminderRuleService.cs`
- 在服务端新增 `Domain/Rules/IReminderRuleService.cs` 和 `Domain/Rules/ReminderRuleService.cs`
- 在客户端和服务端新增 `ItemOccurrence`、`ScheduledReminder`
- 实现以下领域规则：
  - 提醒触发器归一化、去重、排序和非法值校验
  - 单次事项与重复事项的统一展开
  - 日、周、月、年频率重复展开
  - `Interval`、`Count`、`UntilAt` 的边界控制
  - 基于事项时区生成提醒调度时间
- 当前备忘重复锚点规则已落地：
  - 若备忘存在 `TargetDate`，则以该日期零点作为展开与提醒锚点
  - 若备忘没有 `TargetDate`，当前不参与重复展开
- 验证 `dotnet build Overview.Client/Overview.Client/Overview.Client.csproj -f net10.0-desktop` 通过，0 warning / 0 error
- 验证 `dotnet build Overview.Server/Overview.Server.csproj` 通过，0 warning / 0 error

## 本轮未完成

- 主页重叠透明度与命中规则

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

- 执行 `DOMAIN-230`：实现主页重叠透明度与命中领域规则，严格对齐原始需求中的三条命中规则

## 接手 AI 注意事项

- 开始任何实现前，先重新阅读 `“一览”用户要求.md`
- 开始任何实现前，先检查 git 仓库状态
- 根解决方案入口是 `Overview.Uno.slnx`，不是 `.sln`
- 若从仓库根目录执行 `dotnet restore/build`，必须保留根级 `global.json`，否则 `Uno.Sdk` 无法解析
- Uno 模板还生成了 `Overview.Client/Overview.Client.sln`，当前以仓库根解决方案为主
- `DOMAIN-200`、`DOMAIN-210`、`DOMAIN-220` 已完成，客户端与服务端领域模型和规则目前保持镜像结构；后续规则实现应继续保持字段和方法语义对齐
- 当前时间标题只做了基础格式化规则，真正的多语言资源化应放在后续 Presentation/i18n 阶段，不要在 Domain 层引入资源依赖
- 当前提醒规则服务已提供 `NormalizeReminderConfig`、`ExpandOccurrences`、`BuildReminderSchedule` 三个入口；后续应用层和基础设施层应复用，而不是各自再写一套时间展开
- 当前备忘重复以 `TargetDate` 零点为锚点，这是为了让无开始时间的备忘也能参与首版规则；如果后续要改成其他锚点，必须先更新决策或状态文档
- 服务端当前只有健康检查、配置落点和领域模型，不应跳去实现持久化或认证，除非先完成 `DOMAIN-230`
- 不要跳过 Shell/Domain/Application 直接做页面细节
- 如果发现本地环境缺少构建所需工具链，先尝试自行安装或补齐环境，再继续任务
- 只有在尝试安装后仍无法继续时，才记录阻塞
- 每完成一个最小任务项后，要先更新状态文件，再立即创建一次包含任务 ID 的 git commit

## 若中断时优先检查的文件

1. `PROJECT-STATUS.md`
2. `PROJECT-TODO.md`
3. `PROJECT-HANDOFF.md`
