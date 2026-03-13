# Project File Map

本文档用于帮助无记忆 AI 快速定位项目关键文件。

## 当前实际文件

- `docs/“一览”用户要求.md`
  - 用户原始需求，最高业务优先级来源
- `docs/一览-开发设计文档.md`
  - 总设计文档
- `docs/一览-开发任务拆解.md`
  - 高层阶段与任务拆解
- `docs/AI-START-HERE.md`
  - 无记忆 AI 唯一入口
- `docs/AI-MASTER-PROMPT.md`
  - 固定主提示词
- `docs/PROJECT-ROADMAP.md`
  - 固定开发路线
- `docs/PROJECT-TODO.md`
  - 任务清单
- `docs/PROJECT-STATUS.md`
  - 当前状态
- `docs/PROJECT-DECISIONS.md`
  - 已确认决策
- `docs/PROJECT-HANDOFF.md`
  - 最近一轮交接说明
- `docs/PROJECT-ACCEPTANCE.md`
  - 最终验收标准
- `docs/PROJECT-FILE-MAP.md`
  - 当前文件地图
- `docs/PROJECT-CHANGELOG.md`
  - 接力变更记录

## 当前不存在但未来应出现的关键路径

- `Overview.Client/`
  - Uno Platform 客户端根目录
- `Overview.Server/`
  - ASP.NET Core 服务端根目录
- `tests/` 或等价测试项目目录
  - 自动化测试

## 维护规则

- 当新增关键目录、项目文件、入口文件、测试项目时，必须更新本文件
- 不记录无关的小型辅助文件
- 只记录会影响下一个 AI 快速定位工作入口的结构
