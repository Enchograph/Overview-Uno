# Project Handoff

## 本轮目标

- 完成 `PLATFORM-1010`，设计并实现四类小组件的平台映射

## 本轮完成

- 新增小组件刷新应用层：
  - `Overview.Client/Overview.Client/Application/Widgets/IWidgetRefreshService.cs`
  - `Overview.Client/Overview.Client/Application/Widgets/WidgetRefreshService.cs`
- 新增小组件导航协议：
  - `Overview.Client/Overview.Client/Application/Navigation/AppNavigationRequest.cs`
- 新增小组件文件快照存储与平台渲染入口：
  - `Overview.Client/Overview.Client/Infrastructure/Widgets/IWidgetSnapshotStore.cs`
  - `Overview.Client/Overview.Client/Infrastructure/Widgets/IWidgetRenderer.cs`
- 已把小组件刷新接入以下链路：
  - `AuthenticationService` 的登录、注册、会话恢复、刷新和登出
  - `ItemService` 的新增、编辑、完成、重要、删除
  - `UserSettingsService` 的保存
  - `SyncOrchestrationService` 的远端拉取/冲突收敛完成后
- 已新增 Android 平台小组件映射：
  - `Platforms/Android/Widgets/AndroidWidgetRenderer.cs`
  - `Platforms/Android/Widgets/OverviewWidgetProviderBase.cs`
  - `Platforms/Android/Widgets/HomeWidgetProvider.cs`
  - `Platforms/Android/Widgets/ListWidgetProvider.cs`
  - `Platforms/Android/Widgets/AiShortcutWidgetProvider.cs`
  - `Platforms/Android/Widgets/QuickAddWidgetProvider.cs`
  - `Platforms/Android/Resources/layout/overview_widget.xml`
  - `Platforms/Android/Resources/xml/*.xml`
- 当前平台行为：
  - Android 已映射主页、列表、AI、新建事项四类小组件
  - 小组件点击通过 `overview://home|list|ai|add?type=task` 深链进入应用
  - `MainActivity` 已支持冷启动与热启动两种外部导航入口
- 新增验证测试：
  - `tests/Overview.Client.Tests/WidgetRefreshServiceTests.cs`
- 验证结果：
  - `dotnet build Overview.Client/Overview.Client/Overview.Client.csproj -f net10.0-desktop -v q` 通过，0 warning / 0 error
  - `dotnet test tests/Overview.Client.Tests/Overview.Client.Tests.csproj` 通过，58/58 用例通过
  - `dotnet build Overview.Client/Overview.Client/Overview.Client.csproj -f net10.0-android -v q` 在当前环境下 120 秒超时前未出现编译错误，且已生成新的 `bin/Debug/net10.0-android/Overview.Client.dll`；构建收尾阶段仍存在环境级不退出现象

## 本轮未完成

- 真实邮件发送提供程序接入
- 平板横竖屏适配
- Windows / Web 主流程适配与通知降级说明收尾

## 当前阻塞

- 无

## 已更新文件

- `Overview.Client/Overview.Client/Application/Navigation/AppNavigationRequest.cs`
- `Overview.Client/Overview.Client/Application/Widgets/IWidgetRefreshService.cs`
- `Overview.Client/Overview.Client/Application/Widgets/WidgetRefreshService.cs`
- `Overview.Client/Overview.Client/Infrastructure/Widgets/IWidgetRenderer.cs`
- `Overview.Client/Overview.Client/Infrastructure/Widgets/IWidgetSnapshotStore.cs`
- `Overview.Client/Overview.Client/Application/DependencyInjection/ClientServiceRegistry.cs`
- `Overview.Client/Overview.Client/Application/Auth/AuthenticationService.cs`
- `Overview.Client/Overview.Client/Application/Items/ItemService.cs`
- `Overview.Client/Overview.Client/Application/Settings/UserSettingsService.cs`
- `Overview.Client/Overview.Client/Application/Sync/SyncOrchestrationService.cs`
- `Overview.Client/Overview.Client/Platforms/Android/MainActivity.Android.cs`
- `Overview.Client/Overview.Client/Presentation/Pages/LoginPage.xaml.cs`
- `Overview.Client/Overview.Client/Presentation/Pages/ShellPage.xaml.cs`
- `Overview.Client/Overview.Client/Platforms/Android/Widgets/AndroidWidgetRenderer.cs`
- `Overview.Client/Overview.Client/Platforms/Android/Widgets/OverviewWidgetProviderBase.cs`
- `Overview.Client/Overview.Client/Platforms/Android/Widgets/HomeWidgetProvider.cs`
- `Overview.Client/Overview.Client/Platforms/Android/Widgets/ListWidgetProvider.cs`
- `Overview.Client/Overview.Client/Platforms/Android/Widgets/AiShortcutWidgetProvider.cs`
- `Overview.Client/Overview.Client/Platforms/Android/Widgets/QuickAddWidgetProvider.cs`
- `Overview.Client/Overview.Client/Platforms/Android/Resources/layout/overview_widget.xml`
- `Overview.Client/Overview.Client/Platforms/Android/Resources/xml/overview_home_widget_info.xml`
- `Overview.Client/Overview.Client/Platforms/Android/Resources/xml/overview_list_widget_info.xml`
- `Overview.Client/Overview.Client/Platforms/Android/Resources/xml/overview_ai_widget_info.xml`
- `Overview.Client/Overview.Client/Platforms/Android/Resources/xml/overview_quick_add_widget_info.xml`
- `tests/Overview.Client.Tests/WidgetRefreshServiceTests.cs`
- `docs/PROJECT-STATUS.md`
- `docs/PROJECT-TODO.md`
- `docs/PROJECT-ACCEPTANCE.md`
- `docs/PROJECT-HANDOFF.md`
- `docs/PROJECT-CHANGELOG.md`
- `docs/PROJECT-FILE-MAP.md`

## 下一步唯一推荐动作

- 执行 `PLATFORM-1020`：完成平板横竖屏适配

## 接手 AI 注意事项

- 开始任何实现前，先重新阅读 `“一览”用户要求.md`
- 开始任何实现前，先检查 git 仓库状态
- 根解决方案入口是 `Overview.Uno.slnx`，不是 `.sln`
- 若从仓库根目录执行 `dotnet restore/build`，必须保留根级 `global.json`，否则 `Uno.Sdk` 无法解析
- 当前列表页已完成标签筛选、排序、完成切换、重要切换、手动重排、主题切换、“更多设置”联动、滑动编辑删除和浮动添加；当前已推进到 AI 任务
- 当前阶段 10 已开始；`PLATFORM-1010` 已完成，下一轮应切换到 `PLATFORM-1020`
- 新增的客户端测试项目当前通过直接引用桌面构建产物 `Overview.Client.dll` 运行；执行 `dotnet test tests/Overview.Client.Tests/Overview.Client.Tests.csproj` 前，先确保客户端桌面目标已经构建过
- 当前列表页采用“ViewModel 聚合状态 + 页面手动 Apply”模式；后续列表页任务应延续该模式，避免额外引入状态库
- 当前设置页 AI 分区也沿用“ViewModel 聚合状态 + 页面手动 Apply”模式；后续 AI 页若需要新增表单或状态，优先复用这一模式
- 当前 AI 页同样沿用“ViewModel 聚合状态 + 页面手动 Apply”模式；`AiPage.xaml.cs` 负责把 ViewModel 状态回填到页面、同步时间选择器模式，并在发送后滚动到最后一条消息
- 当前自动同步触发点只集中在 `SyncLifecycleCoordinator`、`App.xaml.cs` 与 `ShellPage.xaml.cs`；后续不要把同步启动/停止逻辑散到 `HomePage`、`ListPage`、`AiPage` 等业务页面
- 当前应用窗口激活会执行一次前台同步；若后续需要更细粒度退避、网络感知或平台后台任务，优先在协调层扩展，不要修改各业务 ViewModel
- 当前手动同步入口只在设置页 `sync` 分区；状态展示依赖 `ISyncOrchestrationService.StatusChanged` 驱动刷新，不要在页面里直接拼接同步逻辑
- 当前 `SyncOrchestrationServiceTests` 使用共享内存远端验证双设备自动收敛；后续若修改同步协议，应先保持这些验证通过
- 当前本地提醒重建统一收敛在 `NotificationRefreshService`；后续不要把平台通知调度逻辑散到页面或 ViewModel
- 当前 Desktop / Web 的小组件仍未映射真实平台能力；后续做 `PLATFORM-1030` 时要补明确降级说明
- 当前 Android 构建在本环境里会长时间停留在收尾阶段，但已产出 `bin/Debug/net10.0-android/Overview.Client.dll`；若下轮继续做 Android 平台能力，建议优先复查构建脚本或增加显式超时
- 当前外部导航协议统一走 `overview://...`，如需新增平台入口或小组件动作，优先扩展 `AppNavigationRequest`，不要在 `MainActivity` 或页面代码里散落字符串
- 运行 EF CLI 前先执行 `dotnet tool restore`
- 本地没有可连接的 PostgreSQL 实例；如果下轮需要验证 `database update` 或真实读写，请先启动数据库或调整连接串
- 每完成一个最小任务项后，要先更新状态文件，再立即创建一次包含任务 ID 的 git commit

## 若中断时优先检查的文件

1. `PROJECT-STATUS.md`
2. `PROJECT-TODO.md`
3. `PROJECT-HANDOFF.md`
