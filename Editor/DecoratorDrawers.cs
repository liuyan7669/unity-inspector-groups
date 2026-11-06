using UnityEditor;
using UnityEngine;

namespace Cowart.InspectorGroups.Editor
{
    [CustomPropertyDrawer(typeof(TitleAttribute))]
    public class TitleDecoratorDrawer : DecoratorDrawer
    {
        private const float Top = 8f, LabelH = 18f, Bottom = 4f;

        internal const float Height = Top + LabelH + Bottom;
        public override float GetHeight() => Height;

        public override void OnGUI(Rect position) => Draw(position, (TitleAttribute)attribute);

        internal static void Draw(Rect position, TitleAttribute a)
        {
            var style = new GUIStyle(EditorStyles.boldLabel) { fontSize = 12 };
            style.normal.textColor = a.Color;

            var labelRect = new Rect(position.x, position.y + Top, position.width, LabelH);
            GUI.Label(labelRect, a.Text, style);
            if (a.Line)
                EditorGUI.DrawRect(new Rect(position.x, labelRect.yMax, position.width, 1f), a.Color * new Color(1, 1, 1, 0.6f));
        }
    }

    [CustomPropertyDrawer(typeof(DividerAttribute))]
    public sealed class DividerDecoratorDrawer : DecoratorDrawer
    {
        private const float Gap = 10f;

        public override float GetHeight() => Height((DividerAttribute)attribute);
        public override void OnGUI(Rect position) => Draw(position, (DividerAttribute)attribute);

        internal static float Height(DividerAttribute divider)
        {
            float textHeight = Style(divider).CalcSize(new GUIContent(divider.Text)).y;
            return Mathf.Max(EditorGUIUtility.singleLineHeight, textHeight, divider.Thickness) + 8f;
        }

        internal static void Draw(Rect position, DividerAttribute divider)
        {
            GUIStyle style = Style(divider);
            var content = new GUIContent(divider.Text);
            float textWidth = Mathf.Min(position.width, style.CalcSize(content).x);
            float lineWidth = Mathf.Max(0f, (position.width - textWidth) * 0.5f - Gap);
            float lineY = position.center.y - divider.Thickness * 0.5f;

            if (lineWidth > 0f)
            {
                EditorGUI.DrawRect(new Rect(position.x, lineY, lineWidth, divider.Thickness), divider.LineColor);
                EditorGUI.DrawRect(new Rect(position.xMax - lineWidth, lineY, lineWidth, divider.Thickness), divider.LineColor);
            }

            GUI.Label(new Rect(position.center.x - textWidth * 0.5f, position.y, textWidth, position.height), content, style);
        }

        private static GUIStyle Style(DividerAttribute divider)
        {
            var style = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleCenter,
                clipping = TextClipping.Clip,
                fontSize = divider.FontSize
            };
            style.normal.textColor = divider.TextColor;
            return style;
        }
    }

    [CustomPropertyDrawer(typeof(LineAttribute))]
    public class LineDecoratorDrawer : DecoratorDrawer
    {
        public override float GetHeight()
        {
            var a = (LineAttribute)attribute;
            return a.Position != LinePosition.Below ? a.Thickness + 8f : 0f;
        }

        public override void OnGUI(Rect position)
        {
            var a = (LineAttribute)attribute;
            if (a.Position == LinePosition.Below) return;
            Draw(position, a);
        }

        internal static void Draw(Rect position, LineAttribute a)
        {
            EditorGUI.DrawRect(new Rect(position.x, position.y + 4f, position.width, a.Thickness), a.Color);
        }
    }

    [CustomPropertyDrawer(typeof(InfoBoxAttribute))]
    public class InfoBoxDecoratorDrawer : DecoratorDrawer
    {
        public override float GetHeight() => Height((InfoBoxAttribute)attribute);

        internal static float Height(InfoBoxAttribute a)
        {
            // 预留图标宽度，避免多行文字被裁切
            float width = EditorGUIUtility.currentViewWidth - 40f - (a.Type == InfoBoxType.None ? 0f : 40f);
            float h = EditorStyles.helpBox.CalcHeight(new GUIContent(a.Message), Mathf.Max(1f, width));
            return Mathf.Max(h, a.Type == InfoBoxType.None ? 20f : 38f) + 4f;
        }

        public override void OnGUI(Rect position) => Draw(position, (InfoBoxAttribute)attribute);

        internal static void Draw(Rect position, InfoBoxAttribute a)
        {
            position.height -= 4f;
            EditorGUI.HelpBox(position, a.Message, (MessageType)a.Type);
        }
    }
}
