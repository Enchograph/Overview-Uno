# Project Handoff

## 本轮目标

- 完成 `LIST-750`，实现列表页滑动编辑删除和浮动添加按钮

## 本轮完成

- 更新客户端列表页行级滑动交互与浮动添加入口：
  - `Presentation/Pages/ListPage.xaml`
  - `Presentation/Pages/ListPage.xaml.cs`
  - 左滑显示删除入口
  - 右滑显示编辑入口
  - 右下角新增浮动添加按钮
- 更新客户端列表页状态 ViewModel：
  - `Presentation/ViewModels/ListPageViewModel.cs`
  - 新增删除事项入口
  - 新增按当前标签生成添加页默认值逻辑
- 更新客户端添加页导航预填：
  - `Presentation/Pages/AddItemNavigationRequest.cs`
  - `Presentation/ViewModels/AddItemPageViewModel.cs`
  - 当前已支持按列表标签预填事项类型、重要标记和日期
- 新增列表页删除与默认填充测试：
  - `tests/Overview.Client.Tests/ListPageViewModelTests.cs`
  - `tests/Overview.Client.Tests/AddItemPageViewModelTests.cs`
- 验证结果：
  - `dotnet build Overview.Client/Overview.Client/Overview.Client.csproj -f net10.0-desktop` 通过，0 warning / 0 error
  - `dotnet test tests/Overview.Client.Tests/Overview.Client.Tests.csproj` 通过，25/25 用例通过

## 本轮未完成

- `AI-800` 及后续 AI 页 Presentation 层任务
- 真实邮件发送提供程序接入
- 通知平台映射
- 小组件平台映射

## 当前阻塞

- 无

## 已更新文件

- `Overview.Client/Overview.Client/Presentation/Pages/AddItemNavigationRequest.cs`
- `Overview.Client/Overview.Client/Presentation/Pages/ListPage.xaml`
- `Overview.Client/Overview.Client/Presentation/Pages/ListPage.xaml.cs`
- `Overview.Client/Overview.Client/Presentation/ViewModels/AddItemPageViewModel.cs`
- `Overview.Client/Overview.Client/Presentation/ViewModels/ListPageViewModel.cs`
- `tests/Overview.Client.Tests/ListPageViewModelTests.cs`
- `tests/Overview.Client.Tests/AddItemPageViewModelTests.cs`
- `docs/PROJECT-STATUS.md`
- `docs/PROJECT-TODO.md`
- `docs/PROJECT-HANDOFF.md`
- `docs/PROJECT-CHANGELOG.md`
- `docs/PROJECT-FILE-MAP.md`
- `docs/PROJECT-ACCEPTANCE.md`

## 下一步唯一推荐动作

- 执行 `AI-800`：实现 AI 设置页和配置同步接入

## 接手 AI 注意事项

- 开始任何实现前，先重新阅读 `“一览”用户要求.md`
- 开始任何实现前，先检查 git 仓库状态
- 根解决方案入口是 `Overview.Uno.slnx`，不是 `.sln`
- 若从仓库根目录执行 `dotnet restore/build`，必须保留根级 `global.json`，否则 `Uno.Sdk` 无法解析
- 当前列表页已完成标签筛选、排序、完成切换、重要切换、手动重排、主题切换、“更多设置”联动、滑动编辑删除和浮动添加；下一轮应切换到 `AI-800`
- 新增的主页命中测试项目当前通过直接引用桌面构建产物 `Overview.Client.dll` 运行；执行 `dotnet test tests/Overview.Client.Tests/Overview.Client.Tests.csproj` 前，先确保客户端桌面目标已经构建过
- 当前列表页采用“ViewModel 聚合状态 + 页面手动 Apply”模式；后续列表页任务应延续该模式，避免额外引入状态库
- 当前列表页标签选中态通过 ViewModel 文案前缀表现，避免依赖 Uno 未实现的 `ItemContainerGenerator`
- 当前列表页重排入口采用顶部按钮 + 行内上下移动按钮；手动顺序已提升为快照主顺序，后续“更多设置”联动不要破坏该模式
- 设置页现在支持通过导航参数直接打开指定分区；若后续新增从其他页面深链设置页，优先复用该入口
- 当前添加页导航参数已支持 `SuggestedType`、`SuggestedIsImportant`、`SuggestedStartDate`、`SuggestedStartTime`；列表页浮动按钮和主页长按都复用这个入口
- 当前登录页仍负责会话恢复；列表页 ViewModel 默认直接读取 `IAuthenticationService.CurrentSession`
- 运行 EF CLI 前先执行 `dotnet tool restore`
- 本地没有可连接的 PostgreSQL 实例；如果下轮需要验证 `database update` 或真实读写，请先启动数据库或调整连接串
- 每完成一个最小任务项后，要先更新状态文件，再立即创建一次包含任务 ID 的 git commit

## 若中断时优先检查的文件

1. `PROJECT-STATUS.md`
2. `PROJECT-TODO.md`
3. `PROJECT-HANDOFF.md`
