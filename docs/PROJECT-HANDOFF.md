# Project Handoff

## 本轮目标

- 完成 `HOME-600`，实现时间选择组件

## 本轮完成

- 在客户端新增可复用时间选择组件：
  - `Presentation/Components/TimeSelectionPicker.xaml`
  - `Presentation/Components/TimeSelectionPicker.xaml.cs`
  - `Presentation/Components/TimeSelectionConfirmedEventArgs.cs`
- 在客户端新增时间选择 Presentation ViewModel：
  - `Presentation/ViewModels/TimeSelectionViewModel.cs`
  - `Presentation/ViewModels/TimeSelectionWeekRowViewModel.cs`
  - `Presentation/ViewModels/TimeSelectionDateCellViewModel.cs`
- 在客户端重写 `Presentation/ViewModels/HomePageViewModel.cs`，为时间选择宿主页补入模式切换与确认结果状态
- 在客户端重写 `Presentation/Pages/HomePage.xaml` 与 `Presentation/Pages/HomePage.xaml.cs`，提供时间选择组件的最小宿主页入口
- 在客户端轻量注册中心接入 `TimeSelectionViewModel`
- 当前时间选择组件已覆盖：
  - 日 / 周 / 月选择模式
  - 上一月 / 下一月切换
  - 左右滑动切换前后月份
  - 日期点击自动映射到周格或月格
  - 月格、周格、日期格确认返回
- 验证结果：
  - `dotnet build Overview.Client/Overview.Client/Overview.Client.csproj -f net10.0-desktop` 通过，0 warning / 0 error

## 本轮未完成

- `HOME-610` 及后续 Presentation 层任务
- 主页时间块网格与顶栏切换
- 真实邮件发送提供程序接入
- 通知平台映射
- 小组件平台映射

## 当前阻塞

- 无

## 已更新文件

- `Overview.Client/Overview.Client/Presentation/Pages/HomePage.xaml`
- `Overview.Client/Overview.Client/Presentation/Pages/HomePage.xaml.cs`
- `Overview.Client/Overview.Client/Presentation/Components/TimeSelectionPicker.xaml`
- `Overview.Client/Overview.Client/Presentation/Components/TimeSelectionPicker.xaml.cs`
- `Overview.Client/Overview.Client/Presentation/Components/TimeSelectionConfirmedEventArgs.cs`
- `Overview.Client/Overview.Client/Presentation/ViewModels/HomePageViewModel.cs`
- `Overview.Client/Overview.Client/Presentation/ViewModels/TimeSelectionViewModel.cs`
- `Overview.Client/Overview.Client/Presentation/ViewModels/TimeSelectionWeekRowViewModel.cs`
- `Overview.Client/Overview.Client/Presentation/ViewModels/TimeSelectionDateCellViewModel.cs`
- `Overview.Client/Overview.Client/Application/DependencyInjection/ClientServiceRegistry.cs`
- `docs/PROJECT-STATUS.md`
- `docs/PROJECT-TODO.md`
- `docs/PROJECT-HANDOFF.md`
- `docs/PROJECT-CHANGELOG.md`
- `docs/PROJECT-FILE-MAP.md`
- `docs/PROJECT-ACCEPTANCE.md`

## 下一步唯一推荐动作

- 执行 `HOME-610`：实现主页时间块网格与顶栏切换

## 接手 AI 注意事项

- 开始任何实现前，先重新阅读 `“一览”用户要求.md`
- 开始任何实现前，先检查 git 仓库状态
- 根解决方案入口是 `Overview.Uno.slnx`，不是 `.sln`
- 若从仓库根目录执行 `dotnet restore/build`，必须保留根级 `global.json`，否则 `Uno.Sdk` 无法解析
- Uno 模板还生成了 `Overview.Client/Overview.Client.sln`，当前以仓库根解决方案为主
- `APP-450` 已完成；当前客户端已具备自动/手动同步编排、同步状态模型、增量同步游标持久化以及基于 `LastModifiedAt` 的冲突收敛，后续 Presentation 层只应消费状态与命令，不应在页面层自行拼 `push/pull`
- `UI-500` 已完成；当前应用启动后会先进入 `LoginPage`，页面加载时尝试恢复登录态，恢复成功会直接导航到 `ShellPage`
- `UI-510` 已完成；当前 `AddItemPage` 已具备真实新增/编辑表单，并会在同页列出已有事项供重新载入编辑
- `UI-520` 已完成；当前统一详情卡片组件已落地，并已先接入 `AddItemPage` 作为现阶段查看事项入口，后续主页/列表页可直接复用
- `HOME-600` 已完成；当前时间选择组件已经独立落地，后续主页与 AI 页应直接复用，不要再各自重写一套选择逻辑
- `UI-530` 已完成；当前 `SettingsPage` 已具备主页与二级页骨架，但尚未接入逐项可编辑控件
- 当前登录页同时承担登录和注册入口，但还没有与设置页或壳层做完整会话管理联动
- 当前客户端登录态以 `LocalApplicationData/Overview.Client/auth-session.json` 持久化，用于支持首版恢复与刷新；尚未做平台安全存储适配
- 当前同步游标状态以 `LocalApplicationData/Overview.Client/sync-state.json` 持久化，用于支持增量 `pull` 与同步状态展示恢复
- 当前 `send-verification-code` 端点会生成 6 位验证码、持久化哈希，并把明文验证码写入服务端日志；这是为了完成当前最小基础设施任务，真实邮件发送链路仍需后续补齐
- 下一步进入 `HOME-610`，在现有 `HomePage` 中补真实顶栏、周/月视图切换、列头和时间块网格，但不要提前展开 `HOME-620` 的事项跨格渲染
- 当前 `AddItemPage` 的编辑入口来自页面内“Existing Items”列表，主页/列表页对详情和编辑的联动仍待后续 Presentation 任务补齐
- 运行 EF CLI 前先执行 `dotnet tool restore`
- 本地没有可连接的 PostgreSQL 实例；如果下轮需要验证 `database update` 或真实读写，请先启动数据库或调整连接串
- 每完成一个最小任务项后，要先更新状态文件，再立即创建一次包含任务 ID 的 git commit

## 若中断时优先检查的文件

1. `PROJECT-STATUS.md`
2. `PROJECT-TODO.md`
3. `PROJECT-HANDOFF.md`
