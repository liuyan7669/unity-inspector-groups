using System;
using UnityEngine;

namespace Cowart.InspectorGroups
{
    internal static class ColorParser
    {
        /// <summary>解析 "#RRGGBB" / "#RRGGBBAA" 或 Unity 支持的颜色名（red、yellow、cyan 等）。</summary>
        /// <param name="html">颜色字符串。</param>
        /// <param name="fallback">解析失败时返回的颜色。</param>
        public static Color Parse(string html, Color fallback)
        {
            Color c;
            return !string.IsNullOrEmpty(html) && ColorUtility.TryParseHtmlString(html, out c) ? c : fallback;
        }
    }

    // ───────────── 装饰类（DecoratorDrawer，可与其他特性叠加） ─────────────

    /// <summary>带颜色与下划线的分组标题。</summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class TitleAttribute : PropertyAttribute
    {
        public readonly string Text;
        public readonly Color Color;
        public readonly bool Line;

        /// <param name="text">标题文字。</param>
        /// <param name="color">标题与下划线颜色，"#RRGGBB" 或颜色名（如 "red"）。</param>
        /// <param name="line">是否在标题下方绘制下划线。</param>
        public TitleAttribute(string text, string color = "#4FC3F7", bool line = true)
        {
            Text = text; Color = ColorParser.Parse(color, Color.white); Line = line; order = -10;
        }
    }

    /// <summary>在居中文字两侧绘制自动伸缩的分割线。</summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public sealed class DividerAttribute : PropertyAttribute
    {
        public string Text { get; }
        public Color TextColor { get; }
        public Color LineColor { get; }
        public float Thickness { get; }
        public int FontSize { get; }

        /// <summary>设置分隔文字、文字颜色、线条颜色、线宽和字号。</summary>
        public DividerAttribute(string text, string textColor = "#4FC3F7", string lineColor = "#4FC3F7",
            float thickness = 1f, int fontSize = 12)
        {
            Text = text ?? string.Empty;
            TextColor = ColorParser.Parse(textColor, Color.white);
            LineColor = ColorParser.Parse(lineColor, Color.gray);
            Thickness = Mathf.Max(1f, thickness);
            FontSize = Mathf.Max(1, fontSize);
            order = -6;
        }
    }

    /// <summary>分割线位置：上方 / 下方 / 上下都有。</summary>
    public enum LinePosition { Above, Below, Both }

    /// <summary>
    /// 纯分割线，默认在字段上方。
    /// 下方的线由包内统一 Inspector 绘制。
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class LineAttribute : PropertyAttribute
    {
        public readonly Color Color;
        public readonly float Thickness;
        public readonly LinePosition Position;

        /// <param name="color">线条颜色，"#RRGGBB" 或颜色名。</param>
        /// <param name="thickness">线条粗细（像素）。</param>
        /// <param name="position">线的位置：Above 上方（默认）/ Below 下方 / Both 上下都有。</param>
        public LineAttribute(string color = "#808080", float thickness = 1f, LinePosition position = LinePosition.Above)
        {
            Color = ColorParser.Parse(color, Color.gray); Thickness = thickness; Position = position; order = -5;
        }

        /// <param name="position">线的位置：Above 上方 / Below 下方 / Both 上下都有。颜色灰色，粗细 1。</param>
        public LineAttribute(LinePosition position) : this("#808080", 1f, position) { }
    }

    /// <summary>提示框类型：无图标 / 信息 / 警告 / 错误。</summary>
    public enum InfoBoxType { None, Info, Warning, Error }

    /// <summary>字段上方的提示框。</summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class InfoBoxAttribute : PropertyAttribute
    {
        public readonly string Message;
        public readonly InfoBoxType Type;

        /// <param name="message">提示文字，过长会自动换行。</param>
        /// <param name="type">提示框类型，决定图标：None / Info / Warning / Error。</param>
        public InfoBoxAttribute(string message, InfoBoxType type = InfoBoxType.Info)
        {
            Message = message; Type = type; order = -1;
        }
    }

    // ───────────── 值绘制类（PropertyDrawer，每个字段只能用一个） ─────────────

    /// <summary>Vector2 / Vector2Int 双端滑块。</summary>
    public class MinMaxSliderAttribute : PropertyAttribute
    {
        public readonly float Min, Max;

        /// <param name="min">滑块允许的最小值（对应 x）。</param>
        /// <param name="max">滑块允许的最大值（对应 y）。</param>
        public MinMaxSliderAttribute(float min, float max) { Min = min; Max = max; }
    }

    /// <summary>string 字段：Tag 下拉。</summary>
    public class TagAttribute : PropertyAttribute { }

    /// <summary>int 或 string 字段：Layer 下拉。</summary>
    public class LayerAttribute : PropertyAttribute { }

    /// <summary>string 或 int 字段：Build Settings 中的场景下拉。</summary>
    public class SceneNameAttribute : PropertyAttribute { }

    /// <summary>枚举多选掩码（枚举建议标记 [System.Flags]）。</summary>
    public class EnumFlagsAttribute : PropertyAttribute { }

    // ───────────── 元特性（由包内统一 Inspector 处理，可任意叠加） ─────────────

    /// <summary>自定义显示名称。</summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class LabelAttribute : Attribute
    {
        public readonly string Text;

        /// <param name="text">在 Inspector 中显示的字段名。</param>
        public LabelAttribute(string text) { Text = text; }
    }

    /// <summary>字段着色（GUI.backgroundColor 与文字颜色）。</summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ColorFieldAttribute : Attribute
    {
        public readonly Color Color;
        public readonly bool TintText;

        /// <param name="r">红色分量 0~1。</param>
        /// <param name="g">绿色分量 0~1。</param>
        /// <param name="b">蓝色分量 0~1。</param>
        /// <param name="tintText">是否同时给标签和内容文字着色；false 只着色背景。</param>
        public ColorFieldAttribute(float r, float g, float b, bool tintText = false)
        {
            Color = new Color(r, g, b); TintText = tintText;
        }

        /// <param name="color">颜色，"#RRGGBB" 或颜色名。</param>
        /// <param name="tintText">是否同时给标签和内容文字着色；false 只着色背景。</param>
        public ColorFieldAttribute(string color, bool tintText = false)
        {
            Color = ColorParser.Parse(color, Color.white); TintText = tintText;
        }
    }

    /// <summary>结束上一个 [FoldoutGroup]，当前字段不再属于该折叠区。</summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class EndFoldoutAttribute : Attribute { }

}
