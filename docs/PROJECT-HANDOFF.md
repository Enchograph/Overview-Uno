# Project Handoff

## 本轮目标

- 完成 `SHELL-120`，建立服务端分层目录和基础配置样例

## 本轮完成

- 在服务端项目中创建 `Api`、`Application`、`Domain`、`Infrastructure` 目录
- 删除模板自带的天气示例控制器和模型
- 新增 `Api/Controllers/HealthController.cs` 作为最小 API 落点
- 新增 `Api/Contracts/HealthResponse.cs`
- 新增 `Application/DependencyInjection/ApplicationServiceCollectionExtensions.cs`
- 新增 `Infrastructure/DependencyInjection/InfrastructureServiceCollectionExtensions.cs`
- 新增 `Infrastructure/Configuration/PersistenceOptions.cs`
- 新增 `appsettings.Sample.json` 作为 PostgreSQL、认证、邮件、同步的配置样例
- 更新 `Program.cs`，将应用层和基础设施层注册接入启动入口
- 验证 `dotnet build Overview.Server/Overview.Server.csproj` 通过，0 warning / 0 error

## 本轮未完成

- 五页应用壳层、底部导航、默认主页启动
- 任何业务代码实现

## 当前阻塞

- 无

## 已更新文件

- `Overview.Server/Program.cs`
- `Overview.Server/Api/Contracts/HealthResponse.cs`
- `Overview.Server/Api/Controllers/HealthController.cs`
- `Overview.Server/Application/DependencyInjection/ApplicationServiceCollectionExtensions.cs`
- `Overview.Server/Domain/AssemblyMarker.cs`
- `Overview.Server/Infrastructure/Configuration/PersistenceOptions.cs`
- `Overview.Server/Infrastructure/DependencyInjection/InfrastructureServiceCollectionExtensions.cs`
- `Overview.Server/appsettings.Sample.json`
- `docs/PROJECT-STATUS.md`
- `docs/PROJECT-TODO.md`
- `docs/PROJECT-HANDOFF.md`
- `docs/PROJECT-CHANGELOG.md`
- `docs/PROJECT-FILE-MAP.md`

## 下一步唯一推荐动作

- 执行 `SHELL-130`：实现 5 页应用壳层、底部导航和默认主页启动，保持页面为占位壳层，不提前进入业务逻辑

## 接手 AI 注意事项

- 开始任何实现前，先重新阅读 `“一览”用户要求.md`
- 开始任何实现前，先检查 git 仓库状态
- 根解决方案入口是 `Overview.Uno.slnx`，不是 `.sln`
- 若从仓库根目录执行 `dotnet restore/build`，必须保留根级 `global.json`，否则 `Uno.Sdk` 无法解析
- Uno 模板还生成了 `Overview.Client/Overview.Client.sln`，当前以仓库根解决方案为主
- 当前客户端已经有轻量注册中心，但这只是骨架，不应在 `SHELL-120` 前开始实现业务服务
- 服务端当前只有健康检查与配置落点，不应在 `SHELL-130` 前跳去实现持久化或认证
- 不要跳过 Shell/Domain/Application 直接做页面细节
- 如果发现本地环境缺少构建所需工具链，先尝试自行安装或补齐环境，再继续任务
- 只有在尝试安装后仍无法继续时，才记录阻塞
- 每完成一个最小任务项后，要先更新状态文件，再立即创建一次包含任务 ID 的 git commit

## 若中断时优先检查的文件

1. `PROJECT-STATUS.md`
2. `PROJECT-TODO.md`
3. `PROJECT-HANDOFF.md`
