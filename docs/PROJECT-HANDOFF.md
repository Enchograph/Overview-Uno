# Project Handoff

## 本轮目标

- 完成 `DOMAIN-200`，在客户端和服务端定义统一核心领域模型

## 本轮完成

- 在客户端新增 `Domain/Entities`、`Domain/Enums`、`Domain/ValueObjects`
- 在服务端新增 `Domain/Entities`、`Domain/Enums`、`Domain/ValueObjects`
- 在客户端和服务端分别定义统一核心模型：
  - `Item`
  - `UserSettings`
  - `AiChatMessage`
  - `SyncChange`
- 为上述模型补充配套枚举和值对象，包括提醒、重复、主题、列表页、AI 请求和同步变更类型
- 将设置模型覆盖到设计文档要求的同步字段，包括语言、主题、周起始日、主页视图、时间块配置、列表页配置、AI 配置、同步服务地址、通知开关和小组件偏好
- 验证 `dotnet build Overview.Client/Overview.Client/Overview.Client.csproj -f net10.0-desktop` 通过，0 warning / 0 error
- 验证 `dotnet build Overview.Server/Overview.Server.csproj` 通过，0 warning / 0 error

## 本轮未完成

- 时间块和时间范围领域规则
- 提醒与重复规则行为实现
- 主页重叠透明度与命中规则

## 当前阻塞

- 无

## 已更新文件

- `Overview.Client/Overview.Client/Domain/Entities/`
- `Overview.Client/Overview.Client/Domain/Enums/`
- `Overview.Client/Overview.Client/Domain/ValueObjects/`
- `Overview.Server/Domain/Entities/`
- `Overview.Server/Domain/Enums/`
- `Overview.Server/Domain/ValueObjects/`
- `docs/PROJECT-STATUS.md`
- `docs/PROJECT-TODO.md`
- `docs/PROJECT-HANDOFF.md`
- `docs/PROJECT-CHANGELOG.md`
- `docs/PROJECT-FILE-MAP.md`

## 下一步唯一推荐动作

- 执行 `DOMAIN-210`：实现时间块和时间范围领域规则，覆盖周/月标题、日周月范围计算，并支持用户设置的一周起始日

## 接手 AI 注意事项

- 开始任何实现前，先重新阅读 `“一览”用户要求.md`
- 开始任何实现前，先检查 git 仓库状态
- 根解决方案入口是 `Overview.Uno.slnx`，不是 `.sln`
- 若从仓库根目录执行 `dotnet restore/build`，必须保留根级 `global.json`，否则 `Uno.Sdk` 无法解析
- Uno 模板还生成了 `Overview.Client/Overview.Client.sln`，当前以仓库根解决方案为主
- `DOMAIN-200` 已完成，客户端与服务端核心模型目前为镜像结构；后续规则实现应尽量复用现有枚举和值对象，不要重新发明另一套字段
- 服务端当前只有健康检查、配置落点和领域模型，不应跳去实现持久化或认证，除非先完成 `DOMAIN-210`
- 不要跳过 Shell/Domain/Application 直接做页面细节
- 如果发现本地环境缺少构建所需工具链，先尝试自行安装或补齐环境，再继续任务
- 只有在尝试安装后仍无法继续时，才记录阻塞
- 每完成一个最小任务项后，要先更新状态文件，再立即创建一次包含任务 ID 的 git commit

## 若中断时优先检查的文件

1. `PROJECT-STATUS.md`
2. `PROJECT-TODO.md`
3. `PROJECT-HANDOFF.md`
