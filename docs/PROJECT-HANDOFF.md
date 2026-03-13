# Project Handoff

## 本轮目标

- 完成 `UI-530`，建立设置页主结构与二级页骨架

## 本轮完成

- 在客户端重写 `Presentation/Pages/SettingsPage.xaml`，建立真实设置主页与二级页骨架
- 在客户端更新 `Presentation/Pages/SettingsPage.xaml.cs`，接入初始化、分区切换、返回导航和摘要刷新
- 在客户端新增设置页状态 ViewModel 与分区模型：
  - `Presentation/ViewModels/SettingsPageViewModel.cs`
  - `Presentation/ViewModels/SettingsSectionEntry.cs`
  - `Presentation/ViewModels/SettingsSectionField.cs`
- 在客户端轻量注册中心为设置页接入认证与用户设置服务
- 当前设置页已覆盖：
  - 设置主页分区入口
  - 通用、主页、列表、AI、账号、同步、关于二级页骨架
  - 二级页返回按钮与标题结构
  - 当前登录态与用户设置摘要展示
- 修正文档不一致：
  - 记录 `DEC-019`，明确“删除事项”入口属于阶段 7 列表页，不属于阶段 5 退出条件
- 验证结果：
  - `dotnet build Overview.Client/Overview.Client/Overview.Client.csproj -f net10.0-desktop` 通过，0 warning / 0 error

## 本轮未完成

- `HOME-600` 及后续 Presentation 层任务
- 时间选择组件
- 真实邮件发送提供程序接入
- 通知平台映射
- 小组件平台映射

## 当前阻塞

- 无

## 已更新文件

- `Overview.Client/Overview.Client/Presentation/Pages/SettingsPage.xaml`
- `Overview.Client/Overview.Client/Presentation/Pages/SettingsPage.xaml.cs`
- `Overview.Client/Overview.Client/Presentation/ViewModels/SettingsPageViewModel.cs`
- `Overview.Client/Overview.Client/Presentation/ViewModels/SettingsSectionEntry.cs`
- `Overview.Client/Overview.Client/Presentation/ViewModels/SettingsSectionField.cs`
- `Overview.Client/Overview.Client/Application/DependencyInjection/ClientServiceRegistry.cs`
- `docs/PROJECT-STATUS.md`
- `docs/PROJECT-TODO.md`
- `docs/PROJECT-DECISIONS.md`
- `docs/PROJECT-HANDOFF.md`
- `docs/PROJECT-CHANGELOG.md`
- `docs/PROJECT-FILE-MAP.md`
- `docs/PROJECT-ROADMAP.md`
- `docs/PROJECT-ACCEPTANCE.md`

## 下一步唯一推荐动作

- 执行 `HOME-600`：实现时间选择组件

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
- `UI-530` 已完成；当前 `SettingsPage` 已具备主页与二级页骨架，但尚未接入逐项可编辑控件
- 当前登录页同时承担登录和注册入口，但还没有与设置页或壳层做完整会话管理联动
- 当前客户端登录态以 `LocalApplicationData/Overview.Client/auth-session.json` 持久化，用于支持首版恢复与刷新；尚未做平台安全存储适配
- 当前同步游标状态以 `LocalApplicationData/Overview.Client/sync-state.json` 持久化，用于支持增量 `pull` 与同步状态展示恢复
- 当前 `send-verification-code` 端点会生成 6 位验证码、持久化哈希，并把明文验证码写入服务端日志；这是为了完成当前最小基础设施任务，真实邮件发送链路仍需后续补齐
- 下一步进入 `HOME-600`，优先把时间选择组件做成可复用组件，不要提前把主页整页表格一并展开
- 当前 `AddItemPage` 的编辑入口来自页面内“Existing Items”列表，主页/列表页对详情和编辑的联动仍待后续 Presentation 任务补齐
- 运行 EF CLI 前先执行 `dotnet tool restore`
- 本地没有可连接的 PostgreSQL 实例；如果下轮需要验证 `database update` 或真实读写，请先启动数据库或调整连接串
- 每完成一个最小任务项后，要先更新状态文件，再立即创建一次包含任务 ID 的 git commit

## 若中断时优先检查的文件

1. `PROJECT-STATUS.md`
2. `PROJECT-TODO.md`
3. `PROJECT-HANDOFF.md`
