# Project File Map

本文档用于帮助无记忆 AI 快速定位项目关键文件。

## 当前实际文件

- `global.json`
  - 仓库根目录 SDK 固定文件，提供 .NET SDK 版本和 `Uno.Sdk` 版本
- `Overview.Uno.slnx`
  - 仓库主解决方案入口，统一挂接客户端与服务端项目
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
- `Overview.Client/Overview.Client/Overview.Client.csproj`
  - Uno Platform 客户端主项目
- `Overview.Client/Overview.Client/Application/DependencyInjection/ClientServiceRegistry.cs`
  - 客户端轻量注册中心，作为后续服务注册的基础落点
- `Overview.Client/Overview.Client/Presentation/Pages/ShellPage.xaml`
  - 客户端应用壳层和底部导航入口
- `Overview.Client/Overview.Client/Presentation/Pages/HomePage.xaml`
  - 当前主页壳层占位页
- `Overview.Client/Overview.Client/Presentation/Pages/ListPage.xaml`
  - 当前列表页壳层占位页
- `Overview.Client/Overview.Client/Presentation/Pages/AiPage.xaml`
  - 当前 AI 页壳层占位页
- `Overview.Client/Overview.Client/Presentation/Pages/AddItemPage.xaml`
  - 当前添加页壳层占位页
- `Overview.Client/Overview.Client/Presentation/Pages/SettingsPage.xaml`
  - 当前设置页壳层占位页
- `Overview.Client/Overview.Client/Presentation/ViewModels/`
  - 当前五页壳层的 ViewModel 集合
- `Overview.Client/Overview.Client/Domain/DomainAssemblyMarker.cs`
  - 客户端 Domain 层目录落点
- `Overview.Client/Overview.Client/Domain/Entities/`
  - 客户端核心领域实体：事项、用户设置、AI 聊天记录、同步变更
- `Overview.Client/Overview.Client/Domain/Enums/`
  - 客户端领域枚举：事项类型、主题、列表排序、AI 请求、同步变更等
- `Overview.Client/Overview.Client/Domain/ValueObjects/`
  - 客户端领域值对象：提醒、重复、重复展开结果、小组件偏好
- `Overview.Client/Overview.Client/Domain/Rules/`
  - 客户端领域规则：时间块生成、时间范围计算、周期标题格式化、提醒调度、重复展开
- `Overview.Client/Overview.Client/Infrastructure/InfrastructureAssemblyMarker.cs`
  - 客户端 Infrastructure 层目录落点
- `Overview.Client/Overview.Client/Platforms/Desktop/Program.cs`
  - 客户端桌面入口
- `Overview.Client/Overview.Client/Platforms/WebAssembly/Program.cs`
  - 客户端 WebAssembly 入口
- `Overview.Server/Overview.Server.csproj`
  - ASP.NET Core 服务端主项目
- `Overview.Server/Program.cs`
  - 服务端入口文件
- `Overview.Server/Api/Controllers/HealthController.cs`
  - 服务端最小健康检查控制器
- `Overview.Server/Api/Contracts/HealthResponse.cs`
  - 服务端最小 API 返回契约
- `Overview.Server/Application/DependencyInjection/ApplicationServiceCollectionExtensions.cs`
  - 服务端应用层注册入口
- `Overview.Server/Domain/Entities/`
  - 服务端核心领域实体：事项、用户设置、AI 聊天记录、同步变更
- `Overview.Server/Domain/Enums/`
  - 服务端领域枚举：事项类型、主题、列表排序、AI 请求、同步变更等
- `Overview.Server/Domain/ValueObjects/`
  - 服务端领域值对象：提醒、重复、重复展开结果、小组件偏好
- `Overview.Server/Domain/Rules/`
  - 服务端领域规则：时间块生成、时间范围计算、周期标题格式化、提醒调度、重复展开
- `Overview.Server/Infrastructure/DependencyInjection/InfrastructureServiceCollectionExtensions.cs`
  - 服务端基础设施层注册入口
- `Overview.Server/Infrastructure/Configuration/PersistenceOptions.cs`
  - 服务端持久化配置样例对象
- `Overview.Server/appsettings.Sample.json`
  - 服务端 PostgreSQL、认证、邮件、同步配置样例

## 当前不存在但未来应出现的关键路径

- `Overview.Server/Api/`
  - 服务端 API 分层目录
- `Overview.Server/Application/`
  - 服务端 Application 分层目录
- `Overview.Server/Domain/`
  - 服务端 Domain 分层目录
- `Overview.Server/Infrastructure/`
  - 服务端 Infrastructure 分层目录
- `tests/` 或等价测试项目目录
  - 自动化测试

## 维护规则

- 当新增关键目录、项目文件、入口文件、测试项目时，必须更新本文件
- 不记录无关的小型辅助文件
- 只记录会影响下一个 AI 快速定位工作入口的结构
