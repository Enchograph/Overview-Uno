# Project Handoff

## 本轮目标

- 完成 `UI-500`，实现登录页与登录态恢复

## 本轮完成

- 在客户端新增登录页 `Presentation/Pages/LoginPage.xaml`
- 新增登录页代码后置 `Presentation/Pages/LoginPage.xaml.cs`
- 新增登录页 ViewModel `Presentation/ViewModels/LoginPageViewModel.cs`
- 登录页已覆盖：
  - 启动时调用 `RestoreSessionAsync`
  - 已恢复会话自动导航到 `ShellPage`
  - 登录/注册模式切换
  - 注册模式下发送邮箱验证码
  - 登录或注册成功后进入应用壳层
- 在 `App.xaml.cs` 将启动入口切换为 `LoginPage`
- 在 `ClientServiceRegistry` 注册：
  - `LoginPageViewModel`
- 验证结果：
  - `dotnet build Overview.Client/Overview.Client/Overview.Client.csproj -f net10.0-desktop` 通过，0 warning / 0 error

## 本轮未完成

- `UI-510` 及后续 Presentation 层任务
- 添加/编辑事项表单
- 事项详情组件
- 设置页主结构与二级页骨架
- 真实邮件发送提供程序接入
- 通知平台映射
- 小组件平台映射

## 当前阻塞

- 无

## 已更新文件

- `Overview.Client/Overview.Client/Presentation/Pages/LoginPage.xaml`
- `Overview.Client/Overview.Client/Presentation/Pages/LoginPage.xaml.cs`
- `Overview.Client/Overview.Client/Presentation/ViewModels/LoginPageViewModel.cs`
- `Overview.Client/Overview.Client/App.xaml.cs`
- `Overview.Client/Overview.Client/Application/DependencyInjection/ClientServiceRegistry.cs`
- `docs/PROJECT-STATUS.md`
- `docs/PROJECT-TODO.md`
- `docs/PROJECT-HANDOFF.md`
- `docs/PROJECT-CHANGELOG.md`
- `docs/PROJECT-FILE-MAP.md`

## 下一步唯一推荐动作

- 执行 `UI-510`：实现添加/编辑事项页基础表单

## 接手 AI 注意事项

- 开始任何实现前，先重新阅读 `“一览”用户要求.md`
- 开始任何实现前，先检查 git 仓库状态
- 根解决方案入口是 `Overview.Uno.slnx`，不是 `.sln`
- 若从仓库根目录执行 `dotnet restore/build`，必须保留根级 `global.json`，否则 `Uno.Sdk` 无法解析
- Uno 模板还生成了 `Overview.Client/Overview.Client.sln`，当前以仓库根解决方案为主
- `APP-450` 已完成；当前客户端已具备自动/手动同步编排、同步状态模型、增量同步游标持久化以及基于 `LastModifiedAt` 的冲突收敛，后续 Presentation 层只应消费状态与命令，不应在页面层自行拼 `push/pull`
- `UI-500` 已完成；当前应用启动后会先进入 `LoginPage`，页面加载时尝试恢复登录态，恢复成功会直接导航到 `ShellPage`
- 当前登录页同时承担登录和注册入口，但还没有与设置页或壳层做完整会话管理联动
- 当前客户端登录态以 `LocalApplicationData/Overview.Client/auth-session.json` 持久化，用于支持首版恢复与刷新；尚未做平台安全存储适配
- 当前同步游标状态以 `LocalApplicationData/Overview.Client/sync-state.json` 持久化，用于支持增量 `pull` 与同步状态展示恢复
- 当前 `send-verification-code` 端点会生成 6 位验证码、持久化哈希，并把明文验证码写入服务端日志；这是为了完成当前最小基础设施任务，真实邮件发送链路仍需后续补齐
- 下一步不要跳去做主页/列表页细节，先完成 `UI-510`
- 当前 `AddItemPage` 仍是占位页，新增/编辑事项表单还没开始
- 运行 EF CLI 前先执行 `dotnet tool restore`
- 本地没有可连接的 PostgreSQL 实例；如果下轮需要验证 `database update` 或真实读写，请先启动数据库或调整连接串
- 每完成一个最小任务项后，要先更新状态文件，再立即创建一次包含任务 ID 的 git commit

## 若中断时优先检查的文件

1. `PROJECT-STATUS.md`
2. `PROJECT-TODO.md`
3. `PROJECT-HANDOFF.md`
