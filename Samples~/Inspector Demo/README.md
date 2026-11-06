# Inspector Demo

`Tests.cs` 是手动验收 demo，不是 NUnit 自动化测试。

1. 在 Package Manager 选择 **Inspector Groups**，在 Samples 中导入 **Inspector Demo**。
2. 在场景创建空 GameObject，添加 `Tests` 组件。
3. 在 Edit Mode 展开各个分组，观察只读、条件显示、条件启用、必填、预览、下拉、进度条和文本展示。
4. 展开第 12 组及 12a～12c，检查彩色进度条、双端滑块、字段装饰、选择器、条件显示和资源预览。比较第 12 组的两条 Divider，调整 Inspector 宽度观察线条如何伸缩。收起 12c，确认组外备注仍可见。
5. 点击进度和重置按钮，检查实时摘要；展开嵌套对象、列表和内联配置，检查组合特性与禁用状态。
6. 进入 Play Mode，检查仅运行时可点击的按钮。

组件带有 `ExecuteAlways`，临时 ScriptableObject 与纹理只驻留内存，禁用组件时会清理由自身生成的对象。完整参数和适用限制见包根目录 README。

为了保留现有场景引用，本示例仍使用原来的全局 `Tests` 类名和脚本 GUID；不要在同一工程重复导入多个版本的示例。
