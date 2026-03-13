# AI Start Here

如果你是一个没有任何上下文的新 AI，从这个文件开始。

## 1. 你的唯一目标

继续推进“一览（Overview）”项目，直到达到验收标准。

你不能依赖记忆，不能猜测当前状态，不能跳过文档读取步骤。

## 2. 必须遵守的读取顺序

按以下顺序读取文件，不能跳过：

1. [`AI-MASTER-PROMPT.md`](./AI-MASTER-PROMPT.md)
2. [`“一览”用户要求.md`](./“一览”用户要求.md)
3. [`PROJECT-STATUS.md`](./PROJECT-STATUS.md)
4. [`PROJECT-HANDOFF.md`](./PROJECT-HANDOFF.md)
5. [`PROJECT-TODO.md`](./PROJECT-TODO.md)
6. [`PROJECT-ROADMAP.md`](./PROJECT-ROADMAP.md)
7. [`PROJECT-DECISIONS.md`](./PROJECT-DECISIONS.md)
8. [`PROJECT-ACCEPTANCE.md`](./PROJECT-ACCEPTANCE.md)
9. [`PROJECT-FILE-MAP.md`](./PROJECT-FILE-MAP.md)
10. [`一览-开发设计文档.md`](./一览-开发设计文档.md)
11. [`一览-开发任务拆解.md`](./一览-开发任务拆解.md)

其中第 2 步是强制要求。任何 AI 在开始工作前都必须先重新阅读用户原始需求文档，避免偏离开发方向。

## 3. 你必须如何选择任务

- 只允许执行 `PROJECT-TODO.md` 中依赖已满足的最小未完成任务。
- 优先执行 `PROJECT-STATUS.md` 里标记的“下一个唯一优先任务 ID”。
- 如果发现那个任务实际无法执行，必须先在 `PROJECT-HANDOFF.md` 和 `PROJECT-STATUS.md` 中写明原因，再选择下一个依赖已满足的任务。
- 不能擅自改变里程碑顺序。
- 不能跳过 MVVM 顺序直接堆页面；必须先完成 Shell、Domain、Infrastructure、Application，再进入主要 Presentation 页面。

## 4. 你必须如何更新状态

每次完成工作后，按固定顺序更新：

1. `PROJECT-STATUS.md`
2. `PROJECT-TODO.md`
3. `PROJECT-DECISIONS.md`（仅当新增确定性结论时）
4. `PROJECT-HANDOFF.md`
5. `PROJECT-CHANGELOG.md`
6. `PROJECT-FILE-MAP.md`（仅当新增代码结构或文件时）

更新完状态后，如果当前没有真实阻塞，必须继续执行下一个依赖已满足的最小未完成任务，不能停下来等待用户再次发指令。

## 5. 明确禁止事项

- 不得跳过状态文件直接修改代码
- 不得只改代码不改文档状态
- 不得在未阅读原始需求文档的情况下开始本轮工作
- 不得擅自扩展产品范围
- 不得自行引入多人协作、附件、服务端 AI 代理、推送中心
- 不得跳过测试或验证要求就宣布完成
- 不得修改固定开发路线，除非先更新 `PROJECT-DECISIONS.md`

## 6. 流程结束要求

- 当项目达到最终验收标准后，必须执行最终收尾流程。
- 若本地已经是 git 仓库且远端已配置，必须将最终结果提交并推送到远端仓库。
- 若 git 仓库尚未创建或远端未配置，必须在 `PROJECT-HANDOFF.md` 和 `PROJECT-STATUS.md` 中明确记录该阻塞，等待用户完成仓库初始化后再执行上传。

## 7. 工具链与环境规则

- 如果完成当前任务所需工具链缺失，默认动作不是阻塞，而是由 AI 自行安装或补齐环境后继续。
- 只有在尝试安装或补齐环境后仍无法继续时，才允许把任务标记为 `blocked`。
- 若安装了新的关键工具链，必须在 `PROJECT-HANDOFF.md` 和 `PROJECT-CHANGELOG.md` 中记录。

## 8. 当前项目状态摘要

当前仅完成文档准备阶段，尚未开始项目骨架和代码实现。

你接手时，默认应该从 `BOOT-100` 系列任务开始，除非状态文件已经说明有更新。
