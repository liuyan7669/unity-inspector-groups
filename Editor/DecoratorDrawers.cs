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
