using System;
using System.Runtime.CompilerServices;

namespace Cowart.InspectorGroups
{
    /// <summary>在 Inspector 中只读显示字段或属性，标记方法时禁用按钮。</summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
    public sealed class ReadOnlyAttribute : Attribute
    {
    }

    /// <summary>根据所属对象的布尔字段、属性或无参方法决定是否显示成员。</summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
    public class ShowIfAttribute : Attribute
    {
        /// <summary>条件成员名。</summary>
        public string MemberName { get; }
        /// <summary>条件成员需要满足的值。</summary>
        public bool Expected { get; }

        /// <summary>任一值匹配即满足条件。空集合表示按布尔值或对象引用判断。</summary>
        public object[] Values { get; }
        /// <summary>指定条件成员及期望值。</summary>
        public ShowIfAttribute(string memberName, bool expected)
        {
            MemberName = memberName;
            Expected = expected;
        }

        /// <summary>指定条件成员及允许的值。</summary>
        public ShowIfAttribute(string memberName, params object[] values)
        {
            MemberName = memberName;
            Expected = true;
            Values = values ?? new object[] { null };
        }
    }

    /// <summary>条件成立时隐藏字段、属性或按钮。</summary>
    public sealed class HideIfAttribute : ShowIfAttribute
    {
        public HideIfAttribute(string memberName, params object[] values) : base(memberName, values) { }
    }

    /// <summary>根据所属对象的布尔字段、属性或无参方法决定是否允许成员交互。</summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
    public sealed class EnableIfAttribute : Attribute
    {
        /// <summary>条件成员名。</summary>
        public string MemberName { get; }
        /// <summary>条件成员需要满足的值。</summary>
        public bool Expected { get; }
        /// <summary>指定条件成员及期望值。</summary>
        public EnableIfAttribute(string memberName, bool expected = true)
        {
            MemberName = memberName;
            Expected = expected;
        }
    }

    /// <summary>引用、字符串或集合为空时显示提示。</summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class RequiredAttribute : Attribute
    {
        /// <summary>自定义提示，为空时使用默认提示。</summary>
        public string Message { get; }
        /// <summary>指定可选的必填提示。</summary>
        public RequiredAttribute(string message = null)
        {
            Message = message;
        }
    }

    /// <summary>为返回 void 的实例无参方法生成 Inspector 按钮。</summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ButtonAttribute : Attribute
    {
        /// <summary>按钮文字，为空时使用方法名。</summary>
        public string Label { get; }
        /// <summary>是否仅允许在运行时点击。</summary>
        public bool PlayModeOnly { get; }
        /// <summary>特性所在源码行号，由编译器自动填写，用于按声明顺序绘制。</summary>
        public int Line { get; }
        /// <summary>自定义按钮高度，0 表示使用默认高度。</summary>
        public float Height { get; }
        /// <summary>指定按钮文字和运行时限制；line 由编译器自动填写，无需手动传入。</summary>
        public ButtonAttribute(string label = null, bool playModeOnly = false, [CallerLineNumber] int line = 0, float height = 0f)
        {
            Label = label;
            PlayModeOnly = playModeOnly;
            Height = height;
            Line = line;
        }
    }

    /// <summary>在引用字段下方直接编辑 ScriptableObject 的序列化内容。</summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class InlineEditorAttribute : Attribute
    {
    }

    /// <summary>在资源引用字段下方显示预览。</summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class AssetPreviewAttribute : Attribute
    {
        /// <summary>预览区域高度。</summary>
        public float Height { get; }
        /// <summary>指定预览区域高度。</summary>
        public AssetPreviewAttribute(float height = 80f)
        {
            Height = Math.Max(16f, height);
        }
    }

    /// <summary>使用同一对象成员提供的集合绘制字段下拉选项。</summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class DropdownAttribute : Attribute
    {
        /// <summary>返回选项集合的字段、属性或无参方法名。</summary>
        public string MemberName { get; }
        /// <summary>指定选项来源成员。</summary>
        public DropdownAttribute(string memberName)
        {
            MemberName = memberName;
        }
    }

    /// <summary>将 int 或 float 字段显示为只读进度条。</summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ProgressBarAttribute : Attribute
    {
        /// <summary>进度起点。</summary>
        public float Min { get; }
        /// <summary>进度终点。</summary>
        public float Max { get; }
        /// <summary>进度条高度。</summary>
        public float Height { get; }
        /// <summary>可拖动进度条的填充色。空值表示只读。</summary>
        public UnityEngine.Color? FillColor { get; }
        /// <summary>指定进度范围及显示高度。</summary>
        public ProgressBarAttribute(float min = 0f, float max = 1f, float height = 18f)
        {
            Min = min;
            Max = max;
            Height = Math.Max(16f, height);
        }

        /// <summary>创建从零开始、可拖动修改的彩色进度条。</summary>
        public ProgressBarAttribute(float max, string color) : this(0f, max)
        {
            FillColor = ColorParser.Parse(color, UnityEngine.Color.green);
        }
    }

    /// <summary>以只读文本显示字段或无索引属性，包含未序列化的数据。</summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class ShowAsStringAttribute : Attribute
    {
        /// <summary>显示名称，为空时使用成员名。</summary>
        public string Label { get; }
        /// <summary>特性所在源码行号，由编译器自动填写，用于按声明顺序绘制。</summary>
        public int Line { get; }
        /// <summary>指定可选的显示名称；line 由编译器自动填写，无需手动传入。</summary>
        public ShowAsStringAttribute(string label = null, [CallerLineNumber] int line = 0)
        {
            Label = label;
            Line = line;
        }
    }
}
