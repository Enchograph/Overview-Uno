# Project Decisions

本文档记录已经确认、不应由后续 AI 再自行猜测的决策。

## 决策状态枚举

- `confirmed`
- `superseded`

---

## DEC-001

- Status: `confirmed`
- Topic: 客户端技术栈
- Decision: 使用 `Uno Platform`
- Reason: 需要覆盖手机、平板、Windows、Web，且用户已明确指定可采用 Uno Platform

## DEC-002

- Status: `confirmed`
- Topic: 服务端技术栈
- Decision: 使用 `ASP.NET Core + PostgreSQL`
- Reason: 与客户端生态统一，适合用户自建服务器

## DEC-003

- Status: `confirmed`
- Topic: 服务端职责边界
- Decision: 服务端只负责同步账号下事项数据和应用配置
- Reason: 用户明确要求服务器唯一任务就是同步事项信息和应用配置

## DEC-004

- Status: `confirmed`
- Topic: AI 请求链路
- Decision: 客户端直连用户配置的 OpenAI 兼容接口
- Reason: 用户已确认同步服务器和 AI 服务是两套独立配置

## DEC-005

- Status: `confirmed`
- Topic: 同步策略
- Decision: 本地优先，冲突按最后修改时间覆盖
- Reason: 用户已确认首版按该规则执行

## DEC-006

- Status: `confirmed`
- Topic: 登录方式
- Decision: 邮箱验证码/密码
- Reason: 用户已确认首版账号体系采用该方案

## DEC-007

- Status: `confirmed`
- Topic: 首版范围排除项
- Decision: 不做多人协作，不做附件，不做服务端推送中心，不做服务端 AI 代理
- Reason: 用户已明确排除

## DEC-008

- Status: `confirmed`
- Topic: 通知能力
- Decision: 只做本地提醒通知
- Reason: 用户明确要求不做服务端消息推送

## DEC-009

- Status: `confirmed`
- Topic: 周起始日
- Decision: 由用户设置决定
- Reason: 用户明确要求允许用户自己设置一周起始日

## DEC-010

- Status: `confirmed`
- Topic: 平台优先级
- Decision: 手机 > 平板横竖屏 > Windows > Web
- Reason: 用户已明确优先顺序

## DEC-011

- Status: `confirmed`
- Topic: 首版执行模式
- Decision: 无记忆 AI 单 AI 串行接力
- Reason: 当前接力文档体系就是为该模式设计

## DEC-012

- Status: `confirmed`
- Topic: 固定提示词策略
- Decision: 所有后续 AI 使用单一固定主提示词
- Reason: 用户明确希望给没有上下文的 AI 一个固定提示词

## DEC-013

- Status: `confirmed`
- Topic: 当前项目阶段
- Decision: 当前初始化状态为“文档准备完成，代码未开始”
- Reason: 当前仓库只有需求和开发文档，尚无项目代码

## DEC-014

- Status: `confirmed`
- Topic: 接手前阅读顺序
- Decision: 每一轮无记忆 AI 工作开始前，必须先重新阅读用户原始需求文档
- Reason: 用户要求在流程开始时回读最原先的文档，避免偏离方向

## DEC-015

- Status: `confirmed`
- Topic: 最终交付收尾
- Decision: 项目最终验收完成后，若 git 仓库和远端已存在，则必须提交并推送；若尚未具备条件，则记录为最终阻塞
- Reason: 用户要求流程结束时上传 git 仓库，但当前仓库尚未建立

## DEC-016

- Status: `confirmed`
- Topic: 开发流程顺序
- Decision: 接力开发必须按 MVVM 分层推进，顺序为 Shell -> Domain -> Infrastructure -> Application -> Presentation -> Platform -> QA/Release
- Reason: 这样才能避免页面层提前侵入业务与数据逻辑，并保证流程做完后应用可直接用起来

## DEC-017

- Status: `confirmed`
- Topic: 提交粒度
- Decision: 每完成 `PROJECT-TODO.md` 中的一个最小任务项，都必须创建一次 git commit
- Reason: 这样可以让代码变更、状态文档和任务边界保持一致，降低接力开发时的回溯成本
