# Project Handoff

## 本轮目标

- 完成 `DOMAIN-230`，实现主页重叠透明度与命中领域规则

## 本轮完成

- 在客户端新增 `Domain/Rules/IHomeInteractionRuleService.cs` 和 `Domain/Rules/HomeInteractionRuleService.cs`
- 在服务端新增 `Domain/Rules/IHomeInteractionRuleService.cs` 和 `Domain/Rules/HomeInteractionRuleService.cs`
- 在客户端和服务端新增 `TimelineItem`、`TimelineItemOverlap`
- 实现以下领域规则：
  - 根据事项所经历的最大重叠次数计算透明度
  - 基于点击时间点筛出同一重叠组候选事项
  - 按“独占区间/完全包裹/起始时间最早”三条规则裁决命中事项
  - 命中计算不依赖绘制顺序，保留为可单测的纯领域逻辑
- 验证 `dotnet build Overview.Client/Overview.Client/Overview.Client.csproj -f net10.0-desktop` 通过，0 warning / 0 error
- 验证 `dotnet build Overview.Server/Overview.Server.csproj` 通过，0 warning / 0 error

## 本轮未完成

- 客户端 SQLite 数据层
- 服务端 PostgreSQL 数据层

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

- 执行 `INFRA-300`：建立客户端 SQLite 数据层，覆盖事项、设置、聊天记录、同步变更

## 接手 AI 注意事项

- 开始任何实现前，先重新阅读 `“一览”用户要求.md`
- 开始任何实现前，先检查 git 仓库状态
- 根解决方案入口是 `Overview.Uno.slnx`，不是 `.sln`
- 若从仓库根目录执行 `dotnet restore/build`，必须保留根级 `global.json`，否则 `Uno.Sdk` 无法解析
- Uno 模板还生成了 `Overview.Client/Overview.Client.sln`，当前以仓库根解决方案为主
- `DOMAIN-200`、`DOMAIN-210`、`DOMAIN-220`、`DOMAIN-230` 已完成，客户端与服务端领域模型和规则目前保持镜像结构；后续基础设施实现应继续复用既有规则，而不是重写算法
- 当前时间标题只做了基础格式化规则，真正的多语言资源化应放在后续 Presentation/i18n 阶段，不要在 Domain 层引入资源依赖
- 当前提醒规则服务已提供 `NormalizeReminderConfig`、`ExpandOccurrences`、`BuildReminderSchedule` 三个入口；后续应用层和基础设施层应复用，而不是各自再写一套时间展开
- 当前主页交互规则服务已提供 `CalculateOverlapStates`、`ResolveHit` 两个入口；后续 Presentation 主页应只负责把坐标映射成时间点和可见事项集合
- 当前备忘重复以 `TargetDate` 零点为锚点，这是为了让无开始时间的备忘也能参与首版规则；如果后续要改成其他锚点，必须先更新决策或状态文档
- 当前已进入 Infrastructure 阶段，不应跳去做 Application 或 Presentation 细节，除非先完成 `INFRA-300`
- 不要跳过 Shell/Domain/Application 直接做页面细节
- 如果发现本地环境缺少构建所需工具链，先尝试自行安装或补齐环境，再继续任务
- 只有在尝试安装后仍无法继续时，才记录阻塞
- 每完成一个最小任务项后，要先更新状态文件，再立即创建一次包含任务 ID 的 git commit

## 若中断时优先检查的文件

1. `PROJECT-STATUS.md`
2. `PROJECT-TODO.md`
3. `PROJECT-HANDOFF.md`
