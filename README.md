# Inspector Groups

适用于 Unity Inspector 的 11 个自定义特性：折叠分组、只读、条件显示、条件启用、必填提示、方法按钮、内联编辑、资源预览、下拉选项、进度条和文本展示。

支持在同一成员上组合不同特性，也支持在可序列化的类、结构体、数组、列表及内联 ScriptableObject 中继续使用特性。可以混用 Unity 的 Header、Space、Tooltip、Range、Min、TextArea 等特性。

## 包信息

- Package Manager 显示名：`Inspector Groups`
- 包名：`com.cowart.inspector-groups`
- 当前版本：`1.0.0`
- Runtime 程序集与命名空间：`Cowart.InspectorGroups`
- Editor 程序集与命名空间：`Cowart.InspectorGroups.Editor`
- 许可：MIT
- 最低目标版本：Unity 2021.3 LTS；跨版本的 Inspector 外观应在目标工程中验收。

## 安装

### 从本地磁盘安装

打开 **Window > Package Manager**，点击 **+ > Add package from disk...**，选择本包根目录的 `package.json`。

### 作为 Embedded Package 安装

把完整包目录放入 `Packages/com.cowart.inspector-groups`，在项目 `Packages/manifest.json` 的 `dependencies` 中添加：

```json
"com.cowart.inspector-groups": "file:com.cowart.inspector-groups"
```

### 通过 Git URL 安装

在 Package Manager 中选择 **+ > Add package from git URL...**，输入固定版本地址：

```text
https://github.com/liuyan7669/unity-inspector-groups.git#1.0.0
```

开发期间可使用 `https://github.com/liuyan7669/unity-inspector-groups.git#main`。

本仓库已公开，可以通过 Git URL 安装。

### 通过 OpenUPM 安装

OpenUPM 收录申请正在提交；在注册表确认版本可用前，请使用上面的 Git URL。收录完成后，在 Unity 的 **Edit > Project Settings > Package Manager** 添加 Scoped Registry：

```text
Name: package.openupm.com
URL: https://package.openupm.com
Scope: com.cowart.inspector-groups
```

然后通过 **Add package by name** 安装 `com.cowart.inspector-groups`。

## 目录结构

```text
com.cowart.inspector-groups/
├── Runtime/
│   ├── Cowart.InspectorGroups.asmdef
│   ├── FoldoutGroupAttribute.cs
│   └── InspectorAttributes.cs
├── Editor/
│   ├── Cowart.InspectorGroups.Editor.asmdef
│   └── Inspector 绘制与成员解析代码
├── Samples~/
│   └── Inspector Demo/
│       ├── Cowart.InspectorGroups.Samples.asmdef
│       ├── Tests.cs
│       └── README.md
├── package.json
├── README.md
├── CHANGELOG.md
└── LICENSE.md
```

`Tests.cs` 是交互演示组件，放在可选的 `Samples~` 中，不作为自动化测试或 Runtime 的组成部分。安装包不会自动编译演示代码。

## 示例 Demo

在 Package Manager 选中 **Inspector Groups**，从 Samples 导入 **Inspector Demo**。在场景创建空 GameObject 并添加 `Tests` 组件，即可检查全部特性、按钮、嵌套类、列表和内联 ScriptableObject。示例的临时配置与纹理只驻留内存。

当前开发工程已保留一份导入的 demo，路径为 `Assets/Samples/Inspector Groups/1.0.0/Inspector Demo`；脚本 GUID 与原始 `Tests.cs` 一致，以保留现有场景引用。完整步骤见示例 README。

## 接入

业务脚本引入：

```csharp
using Cowart.InspectorGroups;
```

业务代码若使用 `.asmdef`，在其 references 中添加 `Cowart.InspectorGroups`；自定义编辑器继承包内 `FoldoutGroupEditor` 时，在 Editor 程序集中同时引用 `Cowart.InspectorGroups` 与 `Cowart.InspectorGroups.Editor`，并设置 `includePlatforms` 为 `Editor`。

Runtime 只包含特性声明，不引用 UnityEditor；绘制、反射解析和内联编辑逻辑均位于 Editor 程序集。无需依赖其他 Cowart 包。

自动绘制适用于没有专用 `CustomEditor` 的 MonoBehaviour 和 ScriptableObject。已有专用编辑器的接入方法见文末。不得同时保留原来的 Assets 源码目录与本包，否则会重复定义类型。

## 目录

- [组合与嵌套规则](#组合与嵌套规则)
- [FoldoutGroup](#foldoutgroup)
- [ReadOnly](#readonly)
- [ShowIf](#showif)
- [EnableIf](#enableif)
- [Required](#required)
- [Button](#button)
- [InlineEditor](#inlineeditor)
- [AssetPreview](#assetpreview)
- [Dropdown](#dropdown)
- [ProgressBar](#progressbar)
- [ShowAsString](#showasstring)
- [完整组合示例](#完整组合示例)
- [自定义编辑器接入](#自定义编辑器接入)

## 组合与嵌套规则

| 规则 | 实际行为 |
| --- | --- |
| 特性顺序 | 自定义特性的书写顺序不决定谁覆盖谁。显示条件、禁用状态、字段控件、附加展示分别处理。 |
| 同一字段叠加 | 例如 `ReadOnly + ShowIf + Dropdown + ProgressBar + ShowAsString`：先判断是否显示，再限制编辑，同时绘制下拉、进度与文本。 |
| 多种展示叠加 | Dropdown、ProgressBar、ShowAsString 同时标记时，依次显示各自视图，不再只选择一种。ProgressBar、ShowAsString 本身只读，但不把其他编辑控件强制置灰。 |
| Unity 装饰特性 | Header、Space 保留标题和间距；Tooltip 保留悬停提示。HideInInspector 隐藏字段，不被这些自定义特性强制显示。 |
| Unity 字段控件 | 与 Range、Min、TextArea、Multiline 等组合时，保留原生字段控件，再添加自定义展示。Dropdown 的数值选项会过滤掉不满足 Range、Min 的项。 |
| 嵌套对象 | 子对象展开后，内部字段继续识别特性。子对象的条件、下拉来源，相对于子对象自身解析，不相对于根组件。 |
| 数组和列表 | ReadOnly、ShowIf、EnableIf 控制整个集合；Dropdown、ProgressBar、ShowAsString、InlineEditor、AssetPreview 应用于集合元素。FoldoutGroup 在集合字段处开始一组，不为每个元素重复创建同一组。 |
| 父子状态 | 父对象被隐藏或折叠时，不绘制子内容；父对象只读或禁用时，子字段和内联编辑也不能修改。 |
| 内联对象 | InlineEditor 中继续使用相同的绘制规则，内部的分组、条件、按钮及其他特性仍生效。循环对象引用停止继续展开，并显示提示。 |
| 方法和非序列化成员 | Button 方法、未序列化的 ShowAsString 字段/属性显示在当前对象的序列化字段之后，可独立分组，也可组合 ReadOnly、ShowIf、EnableIf。 |
| 类型要求 | 组合不会改变类型契约。例如 ProgressBar 用于 int/float，InlineEditor 用于 ScriptableObject，不能把对象引用当成数值进度。类型或成员名错误会显示提示。 |
| 同种特性重复 | 同一声明不允许重复标记同种特性；不同对象层级可以各自声明。 |

以下写法等价：

```csharp
[ReadOnly, ShowIf(nameof(ready)), Tooltip("当前状态")]
[SerializeField] private int current;
```

```csharp
[Tooltip("当前状态")]
[ShowIf(nameof(ready))]
[ReadOnly]
[SerializeField] private int current;
```

所有特性名都可以省略 `Attribute` 后缀，例如 `[ReadOnly]` 和 `[ReadOnlyAttribute]`。参数表中的小写名称是构造函数参数，命名传参写 `spacing: 8f`，不写 `Spacing = 8f`；对应公开属性只读。

## FoldoutGroup

**功能：**从被标记的成员开始创建折叠分组，持续到同一对象层级的下一个分组标记或成员列表结束。标题栏和内容区使用固定灰色背景，不绘制边框或分隔线。

**适用位置：**序列化字段，以及 Button 方法、ShowAsString 非序列化字段或属性。序列化字段区与后置成员区分别分组，不跨区合并。

```csharp
FoldoutGroupAttribute(string title, bool expanded = true, float spacing = 2f)
```

| 参数 | 类型 | 默认值 | 说明 |
| --- | --- | --- | --- |
| `title` | string | 必填 | 标题文字。 |
| `expanded` | bool | true | 没有会话记录时是否展开。 |
| `spacing` | float | 2f | 本组结束后到下一组的空隙，单位为 GUI 点。负数按 0 处理，最后一组不添加此空隙。 |

| 调用方式 | 效果 |
| --- | --- |
| `[FoldoutGroup("配置")]` | 默认展开，间距 2。 |
| `[FoldoutGroup(title: "配置")]` | 与仅标题相同。 |
| `[FoldoutGroup("配置", false)]` | 默认收起，间距 2。 |
| `[FoldoutGroup("配置", expanded: false)]` | 同上，使用命名参数。 |
| `[FoldoutGroup("配置", spacing: 8f)]` | 默认展开，间距 8。 |
| `[FoldoutGroup("配置", false, 8f)]` | 默认收起，间距 8。 |
| `[FoldoutGroup("配置", false, spacing: 8f)]` | 位置参数和命名参数混用。 |
| `[FoldoutGroup("配置", expanded: false, spacing: 8f)]` | 显式指定两个可选参数。 |
| `[FoldoutGroup(title: "配置", spacing: 8f, expanded: false)]` | 全部命名，可调整命名参数顺序。 |

```csharp
[FoldoutGroup("配置", spacing: 8f)]
[SerializeField] private string configPath;
[SerializeField] private int retryCount;

[FoldoutGroup("状态", expanded: false)]
[SerializeField] private bool ready;
```

上例的 8 点空隙在“配置”结束后、“状态”开始前；不改变标题栏高度。折叠状态按对象类型和成员路径保存在当前 Unity 会话中，同类型对象共享相应状态。手动点击后，保存状态优先于 `expanded`；重启 Unity 清除会话记录。

## ReadOnly

**功能：**保持可见，禁止修改字段或点击按钮。标记集合、嵌套对象或 InlineEditor 引用时，禁用状态向内部传递。

**适用位置：**序列化字段、ShowAsString 字段/属性、Button 方法。

```csharp
ReadOnlyAttribute()
```

| 参数 | 说明 |
| --- | --- |
| 无 | 不需要参数。只限制 Inspector 交互，不影响脚本赋值。 |

| 调用方式 | 效果 |
| --- | --- |
| `[ReadOnly]` | 只读显示或禁用按钮。 |
| `[ReadOnly()]` | 与无括号写法相同。 |

```csharp
[FoldoutGroup("运行状态"), ReadOnly, Tooltip("运行时由程序更新")]
[SerializeField] private int currentModule;
```

## ShowIf

**功能：**条件满足时显示字段、文本成员或按钮，不满足时隐藏。

**适用位置：**字段、ShowAsString 属性、Button 方法。来源为**同一所属对象**的实例 bool 字段、可读无索引属性或无参方法，支持私有和继承成员。

```csharp
ShowIfAttribute(string memberName, bool expected = true)
```

| 参数 | 类型 | 默认值 | 说明 |
| --- | --- | --- | --- |
| `memberName` | string | 必填 | 返回 bool 的条件成员名，推荐使用 nameof。 |
| `expected` | bool | true | 条件成员的值等于此值时显示。 |

| 调用方式 | 效果 |
| --- | --- |
| `[ShowIf(nameof(useConfig))]` | useConfig 为 true 时显示。 |
| `[ShowIf(nameof(useConfig), false)]` | useConfig 为 false 时显示。 |
| `[ShowIf(nameof(useConfig), expected: false)]` | 同上，显式命名。 |
| `[ShowIf(memberName: nameof(useConfig), expected: true)]` | 全部命名。 |

```csharp
[SerializeField] private bool useConfig;

[ShowIf(nameof(useConfig)), Required, Tooltip("启用自定义配置后显示")]
[SerializeField] private ScriptableObject config;
```

多选时全部不满足才隐藏；部分满足时显示但禁止编辑，避免修改条件不满足的对象。条件成员应只读取数据，不执行有副作用的操作；名称错误或返回类型错误时显示提示。

## EnableIf

**功能：**条件满足时允许交互，否则保持可见并置灰。嵌套对象和内联内容继承此禁用状态。

**适用位置：**字段、ShowAsString 属性、Button 方法。条件来源与 ShowIf 相同。

```csharp
EnableIfAttribute(string memberName, bool expected = true)
```

| 参数 | 类型 | 默认值 | 说明 |
| --- | --- | --- | --- |
| `memberName` | string | 必填 | 返回 bool 的字段、属性或无参方法名。 |
| `expected` | bool | true | 返回值等于此值时允许交互。 |

| 调用方式 | 效果 |
| --- | --- |
| `[EnableIf(nameof(canEdit))]` | canEdit 为 true 时可编辑。 |
| `[EnableIf(nameof(canEdit), false)]` | canEdit 为 false 时可编辑。 |
| `[EnableIf(nameof(canEdit), expected: false)]` | 同上，显式命名。 |
| `[EnableIf(memberName: nameof(canEdit), expected: true)]` | 全部命名。 |

```csharp
[SerializeField] private bool canEdit;

[EnableIf(nameof(canEdit)), Range(0, 100), Tooltip("可编辑时调整")]
[SerializeField] private int score;
```

多选要求所有对象都满足条件。与 ReadOnly 组合时仍只读，EnableIf 不会覆盖父级禁用状态。

## Required

**功能：**对象引用、字符串、数组或列表为空时，在字段下方显示警告。不阻止保存或运行，不自动赋值。

**适用位置：**Unity 对象引用、string、数组和列表字段。字符串仅含空白也视为空；集合只检查元素数量，不逐项检查空引用。

```csharp
RequiredAttribute(string message = null)
```

| 参数 | 类型 | 默认值 | 说明 |
| --- | --- | --- | --- |
| `message` | string | null | 自定义警告；null 或空字符串使用自动提示。 |

| 调用方式 | 效果 |
| --- | --- |
| `[Required]` | 使用默认提示。 |
| `[Required()]` | 与无括号写法相同。 |
| `[Required("请绑定图标")]` | 使用指定提示。 |
| `[Required(message: "请绑定图标")]` | 使用命名参数。 |

```csharp
[Header("显示资源"), Required("请绑定图标"), AssetPreview(64f)]
[SerializeField] private Sprite icon;
```

多选对象中任意一个为空时显示警告。字段被 ShowIf 隐藏或所在分组收起时，不显示该字段的警告。

## Button

**功能：**为方法生成 Inspector 按钮；点击后调用该方法，多选时依次调用每个选中对象的方法。

**适用位置：**非异步、无参数、返回 void 的实例方法，支持 public、private、继承和重写方法。不支持 static、泛型、协程、Task 或 async void 方法。

```csharp
ButtonAttribute(string label = null, bool playModeOnly = false)
```

| 参数 | 类型 | 默认值 | 说明 |
| --- | --- | --- | --- |
| `label` | string | null | 按钮文字；null 或空字符串使用整理后的方法名。 |
| `playModeOnly` | bool | false | true 时只允许在 Play Mode 点击，编辑模式仍显示但禁用。 |

| 调用方式 | 效果 |
| --- | --- |
| `[Button]` | 使用方法名，编辑和运行模式均可点击。 |
| `[Button()]` | 与无括号写法相同。 |
| `[Button("刷新")]` | 自定义文字。 |
| `[Button(label: "刷新")]` | 显式命名文字参数。 |
| `[Button(playModeOnly: true)]` | 默认文字，仅运行时可点击。 |
| `[Button("刷新", true)]` | 自定义文字，仅运行时可点击。 |
| `[Button("刷新", playModeOnly: true)]` | 混合传参。 |
| `[Button(label: "刷新", playModeOnly: true)]` | 全部命名，可交换命名参数顺序。 |

```csharp
/// <summary>重新加载配置。</summary>
[FoldoutGroup("调试"), Button("重新加载", playModeOnly: true)]
[ShowIf(nameof(ready)), EnableIf(nameof(canReload))]
private void Reload()
{
    Debug.Log("重新加载配置");
}
```

按钮显示在当前对象的序列化字段之后；嵌套类和结构体的方法也可以生成按钮。自动记录选中根对象的 Undo 和预制体覆盖，结构体的修改会逐层写回。方法若修改其他对象、创建或删除对象，需在方法内部调用相应 Undo API；文件写入、网络请求等外部操作不能通过 Unity Undo 撤销。异常会输出到 Console。

## InlineEditor

**功能：**在 ScriptableObject 引用字段下方直接编辑资源，内部继续支持本库特性和 Unity 字段特性。

**适用位置：**ScriptableObject 引用或其数组/列表。修改的是被引用资源本身，其他引用该资源的对象也会看到修改。

```csharp
InlineEditorAttribute()
```

| 参数 | 说明 |
| --- | --- |
| 无 | 引用不为空时绘制内联内容。 |

| 调用方式 | 效果 |
| --- | --- |
| `[InlineEditor]` | 显示可编辑的内联资源内容。 |
| `[InlineEditor()]` | 与无括号写法相同。 |

```csharp
[Required, InlineEditor, Tooltip("直接编辑共享配置资源")]
[SerializeField] private ScriptableObject config;

[ReadOnly, InlineEditor]
[SerializeField] private ScriptableObject configPreview;
```

第二个字段只能查看，内部字段、按钮也被禁用。内联对象使用本库的统一绘制，不调用资源原有 CustomEditor；循环引用到已经在展开路径上的资源时停止递归。当前内联内容仅在单选时编辑；多选仍允许修改引用字段，引用不一致时不显示内联内容。

## AssetPreview

**功能：**在资源引用下方显示预览，适合贴图、Sprite、模型、Prefab 等。没有完整预览时尝试显示 Unity 缩略图。

**适用位置：**UnityEngine.Object 引用字段或其数组/列表；没有可用预览时不绘制图片。

```csharp
AssetPreviewAttribute(float height = 80f)
```

| 参数 | 类型 | 默认值 | 说明 |
| --- | --- | --- | --- |
| `height` | float | 80f | 预览区域高度，单位为 GUI 点；小于 16 时按 16 处理。 |

| 调用方式 | 效果 |
| --- | --- |
| `[AssetPreview]` | 使用 80 点高的预览区。 |
| `[AssetPreview()]` | 与无括号写法相同。 |
| `[AssetPreview(120f)]` | 指定预览高度。 |
| `[AssetPreview(height: 120f)]` | 使用命名参数。 |

```csharp
[Required, AssetPreview(96f), Tooltip("图标资源")]
[SerializeField] private Sprite icon;
```

预览等比例缩放，不修改资源尺寸。与 InlineEditor 组合时先显示预览，再显示编辑区。多选引用不一致时不显示统一预览。如果脚本同时引入 `UnityEditor`，可写 `[Cowart.InspectorGroups.AssetPreview]` 避免与 Unity 的 AssetPreview 类型名称混淆。

## Dropdown

**功能：**把字段的可编辑值改为下拉选项，选项由所属对象的字段、属性或无参方法提供。

**适用位置：**string、int、float、枚举、Unity 对象引用字段，以及这些类型的数组/列表。来源必须返回 IEnumerable 集合，元素类型与目标字段或集合元素类型一致。

```csharp
DropdownAttribute(string memberName)
```

| 参数 | 类型 | 默认值 | 说明 |
| --- | --- | --- | --- |
| `memberName` | string | 必填 | 提供选项集合的实例成员名，支持私有和继承成员。 |

| 调用方式 | 效果 |
| --- | --- |
| `[Dropdown(nameof(moduleOptions))]` | 从字段读取选项。 |
| `[Dropdown(nameof(ModuleOptions))]` | 从可读属性读取选项。 |
| `[Dropdown(nameof(GetModuleOptions))]` | 从无参方法读取选项。 |
| `[Dropdown(memberName: nameof(GetModuleOptions))]` | 使用命名参数。 |

```csharp
[Dropdown(nameof(GetModuleOptions)), Required]
[SerializeField] private string module;

private string[] GetModuleOptions()
{
    return new[] { "模块一", "模块二", "模块三" };
}
```

空集合、类型不一致或超过 4096 项时显示错误提示。当前值不在选项中时显示占位项，**不会自动改成第一项**。与 Range、Min 组合时过滤不满足范围的数值选项；过滤后为空同样提示。多选对象提供的选项及顺序必须一致，否则保留原字段并提示分别编辑。来源成员应只读取数据。

## ProgressBar

**功能：**把 int 或 float 数值显示为只读进度条，不修改真实数值；超出范围只影响条的填充比例。

**适用位置：**int、float 字段及其数组/列表。与 Dropdown 或原生 Range 控件组合时，编辑控件仍可修改数值，进度条展示修改后的状态。

```csharp
ProgressBarAttribute(float min = 0f, float max = 1f, float height = 18f)
```

| 参数 | 类型 | 默认值 | 说明 |
| --- | --- | --- | --- |
| `min` | float | 0f | 填充比例的起点。 |
| `max` | float | 1f | 填充比例的终点，必须大于 min。 |
| `height` | float | 18f | 条的高度，最小 16。 |

| 调用方式 | 效果 |
| --- | --- |
| `[ProgressBar]` / `[ProgressBar()]` | 范围 0～1，高度 18。 |
| `[ProgressBar(-1f)]` | 仅指定 min，范围 -1～1，高度 18。 |
| `[ProgressBar(0f, 100f)]` | 范围 0～100，高度 18。 |
| `[ProgressBar(0f, 100f, 24f)]` | 范围 0～100，高度 24。 |
| `[ProgressBar(min: -1f)]` | 范围 -1～1。 |
| `[ProgressBar(max: 100f)]` | 范围 0～100。 |
| `[ProgressBar(height: 24f)]` | 默认范围，高度 24。 |
| `[ProgressBar(min: 10f, max: 100f)]` | 指定范围。 |
| `[ProgressBar(min: -1f, height: 24f)]` | 范围 -1～1，高度 24。 |
| `[ProgressBar(max: 100f, height: 24f)]` | 范围 0～100，高度 24。 |
| `[ProgressBar(0f, max: 100f, height: 24f)]` | 位置参数与命名参数混用。 |
| `[ProgressBar(min: 0f, max: 100f, height: 24f)]` | 全部命名，可调整命名参数顺序。 |

```csharp
[Range(0f, 100f), ProgressBar(0f, 100f), Tooltip("完成率")]
[SerializeField] private float completion;
```

上例同时显示可编辑的 Range 滑条和只读进度条。min、max 必须为有限值且 min < max。多选数值不一致时显示“多个值”，不把第一个对象的值当成全部对象的值。

## ShowAsString

**功能：**以可复制的只读文本显示数据，可用于未序列化字段、字典和计算属性。

**适用位置：**实例字段和可读无索引属性。序列化字段在原位置展示；未序列化字段和属性在当前对象的序列化字段之后展示。数组/列表标记时在各元素处展示；若要显示整个未序列化集合摘要，可直接标记该集合。

```csharp
ShowAsStringAttribute(string label = null)
```

| 参数 | 类型 | 默认值 | 说明 |
| --- | --- | --- | --- |
| `label` | string | null | 显示名称，null 或空字符串使用整理后的成员名。 |

| 调用方式 | 效果 |
| --- | --- |
| `[ShowAsString]` | 使用成员名。 |
| `[ShowAsString()]` | 与无括号写法相同。 |
| `[ShowAsString("当前状态")]` | 自定义显示名称。 |
| `[ShowAsString(label: "当前状态")]` | 使用命名参数。 |

```csharp
[ShowAsString("统计")]
private Dictionary<string, int> counts = new Dictionary<string, int>();

/// <summary>当前统计摘要。</summary>
[FoldoutGroup("摘要"), ShowAsString("状态"), ShowIf(nameof(ready))]
public string Summary => ready ? "已就绪" : "未就绪";
```

字典按“键: 值”展示，集合最多显示前 20 项；其他对象使用 ToString，null 显示为 null。文本区域按实际内容和可用宽度自适应高度，最少一行，最高 160 GUI 点。多个选中对象的文本不同则显示“多个值”。属性 Getter 应只读取数据；抛异常时显示错误提示。组合 ShowAsString 不会阻止其他展示特性或嵌套字段继续绘制。

## 完整组合示例

将以下组件保存为 `InspectorAttributesExample.cs`。引用类型 Profile 可另存为对应脚本并创建资源。

```csharp
using System;
using System.Collections.Generic;
using Cowart.InspectorGroups;
using UnityEngine;

/// <summary>Inspector 组合与嵌套示例。</summary>
public class InspectorAttributesExample : MonoBehaviour
{
    [FoldoutGroup("开关", spacing: 8f)]
    [SerializeField] private bool enabledSettings = true;

    [FoldoutGroup("嵌套配置"), ShowIf(nameof(enabledSettings))]
    [SerializeField] private Settings settings = new Settings();

    [FoldoutGroup("共享资源"), Required, InlineEditor, AssetPreview(64f)]
    [SerializeField] private Profile profile;

    /// <summary>示例嵌套数据。</summary>
    [Serializable]
    public class Settings
    {
        [SerializeField] private bool editable = true;

        [FoldoutGroup("数值"), EnableIf(nameof(editable))]
        [Header("分数"), Space(4f), Tooltip("下拉和滑条均可编辑，另有进度和文本展示")]
        [Range(0, 100), Min(10), Dropdown(nameof(GetScores)), ProgressBar(0, 100), ShowAsString]
        [SerializeField] private int score = 50;

        [ReadOnly, Tooltip("整个列表及内部字段只读")]
        [SerializeField] private List<Entry> entries = new List<Entry>();

        private int[] GetScores() => new[] { 10, 25, 50, 75, 100 };

        /// <summary>重置嵌套对象的分数。</summary>
        [FoldoutGroup("操作"), Button("重置"), EnableIf(nameof(editable))]
        private void ResetScore()
        {
            score = 50;
        }
    }

    /// <summary>列表元素。</summary>
    [Serializable]
    public class Entry
    {
        [Required, Tooltip("元素名称")]
        [SerializeField] private string entryName;
    }
}

/// <summary>可内联编辑的资源。</summary>
[CreateAssetMenu(menuName = "Examples/Inspector Profile")]
public class Profile : ScriptableObject
{
    [FoldoutGroup("显示"), Required, AssetPreview]
    [SerializeField] private Sprite icon;

    [TextArea, Tooltip("原生文本框"), ShowAsString("说明摘要")]
    [SerializeField] private string description;
}
```

## 自定义编辑器接入

本库使用备用 Inspector，不覆盖已有专用 CustomEditor。需要接入时继承 `FoldoutGroupEditor`，在原编辑器中调用基类绘制，避免再重复绘制同一份字段：

```csharp
using Cowart.InspectorGroups.Editor;
using UnityEditor;

/// <summary>接入组合特性的自定义编辑器示例。</summary>
[CustomEditor(typeof(InspectorAttributesExample)), CanEditMultipleObjects]
public class InspectorAttributesExampleEditor : FoldoutGroupEditor
{
    /// <summary>绘制特性界面及额外内容。</summary>
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.HelpBox("额外的编辑器内容", MessageType.Info);
    }
}
```

此脚本放在 `Editor/` 目录。已有第三方 CustomEditor、第三方 PropertyDrawer 可能有自己的绘制和生命周期约定，需要单独验证，不能仅凭特性名称声明通用兼容。

## 多选、序列化与目录

| 项目 | 约定 |
| --- | --- |
| 字段可见性 | 普通字段仍需满足 Unity 序列化条件，例如 public 或 SerializeField；只有 ShowAsString 可以主动显示未序列化实例字段及可读属性。 |
| 多选条件 | ShowIf 部分满足时显示但禁用，EnableIf 全部满足才启用，ReadOnly 始终禁用。 |
| 多选资源 | 引用不同则不显示共用预览；内联编辑限单选，避免误改共享资源。 |
| 多选下拉 | 选项内容和顺序须一致；选中项不一致时显示“多个值”。 |
| Undo | 序列化字段沿用 Unity 序列化编辑流程；按钮记录选中根对象的修改。 |
| 集合编辑 | 为逐元素识别特性，数组和列表使用折叠、Size 与逐项字段绘制，不提供原生 ReorderableList 的拖动排序界面。 |
| 循环引用 | 内联资源和嵌套对象检测当前递归路径，避免无限展开；不会删除或改写引用。 |
| Runtime | 特性定义位于 Runtime，不引用 UnityEditor。 |
| Editor | 绘制实现位于 Editor，不进入玩家构建。 |

```text
InspectorGroups/
├── Runtime/
│   ├── FoldoutGroupAttribute.cs
│   └── InspectorAttributes.cs
├── Editor/
│   ├── FoldoutGroupEditor.cs
│   ├── InspectorAttributeGUI.cs
│   ├── InspectorGroupGUI.cs
│   ├── InspectorMemberUtility.cs
│   └── InspectorPropertyContext.cs
└── README.md
```
