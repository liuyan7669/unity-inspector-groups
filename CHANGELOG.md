# Changelog

## [1.1.0] - 2026-11-06

### Added

- 添加 Title、Line、InfoBox、Label 和 ColorField，支持字段装饰、自定义标签和着色。
- 添加 MinMaxSlider、Tag、Layer、SceneName 和 EnumFlags 字段控件。
- ShowIf 支持多个候选值和引用条件，并添加 HideIf。
- FoldoutGroup 支持标题颜色和边线，EndFoldout 可结束当前分组。
- 添加可拖动的彩色 ProgressBar 重载，以及 Button 高度设置。

### Changed

- 新旧特性共用包内 Inspector，重复的资源预览和下拉能力分别统一到 AssetPreview 和 Dropdown。
- 扩充 Tests 示例，覆盖新增控件、不同参数和组合用法。

## [1.0.0] - 2026-11-01

### Added

- 将现有 InspectorGroups 整理为独立 UPM 包，保留原有 11 个特性的 API 与绘制行为。
- 分离 Cowart.InspectorGroups 与仅 Editor 平台加载的 Cowart.InspectorGroups.Editor 程序集。
- 提供可通过 Package Manager 导入的 Inspector Demo，保留 Tests.cs 演示脚本。
- 补充包安装、程序集引用、示例使用与自定义编辑器接入说明。
