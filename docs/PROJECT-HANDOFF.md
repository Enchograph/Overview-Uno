# Project Handoff

## 本轮目标

- 建立无记忆 AI 接力开发文档体系

## 本轮完成

- 新增统一入口文件 `AI-START-HERE.md`
- 新增固定主提示词 `AI-MASTER-PROMPT.md`
- 新增固定开发路线 `PROJECT-ROADMAP.md`
- 新增主任务清单 `PROJECT-TODO.md`
- 新增当前状态快照 `PROJECT-STATUS.md`
- 新增决策记录 `PROJECT-DECISIONS.md`
- 新增验收清单 `PROJECT-ACCEPTANCE.md`
- 新增文件地图 `PROJECT-FILE-MAP.md`
- 新增变更日志 `PROJECT-CHANGELOG.md`
- 按 MVVM 重写开发路线、TODO 与验收门禁

## 本轮未完成

- 客户端项目骨架
- 服务端项目骨架
- 任何业务代码实现
- git 仓库初始化与最终上传流程

## 当前阻塞

- 无

## 已更新文件

- `AI-START-HERE.md`
- `AI-MASTER-PROMPT.md`
- `PROJECT-ROADMAP.md`
- `PROJECT-TODO.md`
- `PROJECT-STATUS.md`
- `PROJECT-DECISIONS.md`
- `PROJECT-HANDOFF.md`
- `PROJECT-ACCEPTANCE.md`
- `PROJECT-FILE-MAP.md`
- `PROJECT-CHANGELOG.md`

## 下一步唯一推荐动作

- 执行 `BOOT-100`：创建 Uno Platform 客户端和 ASP.NET Core 服务端最小项目骨架；随后按 `SHELL-110`、`SHELL-120`、`SHELL-130` 建立 MVVM 目录和五页应用壳层

## 接手 AI 注意事项

- 开始任何实现前，先重新阅读 `“一览”用户要求.md`
- 当前所有状态文件都认为项目还没有代码
- 不要跳过 Shell/Domain/Application 直接做页面细节
- 如果发现本地环境缺少构建所需工具链，先尝试自行安装或补齐环境，再继续任务
- 只有在尝试安装后仍无法继续时，才记录阻塞

## 若中断时优先检查的文件

1. `PROJECT-STATUS.md`
2. `PROJECT-TODO.md`
3. `PROJECT-HANDOFF.md`
