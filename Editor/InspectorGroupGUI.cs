using System;
using UnityEditor;
using UnityEngine;

namespace Cowart.InspectorGroups.Editor
{
    /// <summary>在任意对象层级绘制分组，统一管理背景、折叠和组间距。</summary>
    internal sealed class InspectorGroupGUI : IDisposable
    {
        private bool inGroup;
        private float spacing;
        internal bool Expanded = true;

        internal void Begin(FoldoutGroupAttribute group, string key, bool fullWidth)
        {
            if (inGroup)
            {
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(spacing);
            }
            spacing = group.Spacing;
            Rect rect = EditorGUILayout.BeginVertical(new GUIStyle
            {
                padding = new RectOffset(GUI.skin.box.padding.left, GUI.skin.box.padding.right, 0, 0),
                margin = new RectOffset(GUI.skin.box.margin.left, GUI.skin.box.margin.right, 0, 0)
            });
            inGroup = true;
            if (fullWidth) rect.xMin = 0;
            // 嵌套分组的背景从上一缩进层级开始，标题条不越过父级折叠箭头，保留层级关系。
            else rect.xMin += Mathf.Max(0, EditorGUI.indentLevel - 1) * 15f;
            bool pro = EditorGUIUtility.isProSkin;
            if (Event.current.type == EventType.Repaint)
                EditorGUI.DrawRect(rect, pro ? new Color32(56, 56, 56, 255) : new Color32(194, 194, 194, 255));
            bool previous = SessionState.GetBool(key, group.Expanded);
            Rect title = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight + 4f);
            if (Event.current.type == EventType.Repaint)
            {
                Rect background = title;
                background.xMin = rect.xMin;
                background.xMax = rect.xMax;
                EditorGUI.DrawRect(background, pro ? new Color32(71, 71, 71, 255) : new Color32(210, 210, 210, 255));
            }
            Rect foldout = title;
            foldout.y += (title.height - EditorGUIUtility.singleLineHeight) * 0.5f - 1f;
            foldout.height = EditorGUIUtility.singleLineHeight;
            if (group.Border && Event.current.type == EventType.Repaint)
                EditorGUI.DrawRect(new Rect(rect.xMin, title.y, rect.width, 1f), pro ? Color.black : Color.gray);
            GUIStyle style = EditorStyles.foldout;
            if (group.Color is Color color)
            {
                style = new GUIStyle(style);
                style.normal.textColor = style.onNormal.textColor = color;
                style.focused.textColor = style.onFocused.textColor = color;
                style.hover.textColor = style.onHover.textColor = color;
                style.active.textColor = style.onActive.textColor = color;
            }
            Expanded = EditorGUI.Foldout(foldout, previous, group.Title, true, style);
            if (Expanded != previous) SessionState.SetBool(key, Expanded);
        }

        internal int ContentIndent => inGroup ? 1 : 0;

        /// <summary>结束当前分组，不增加末尾组间距。</summary>
        public void Dispose()
        {
            if (inGroup) EditorGUILayout.EndVertical();
            inGroup = false;
            Expanded = true;
        }
    }
}
