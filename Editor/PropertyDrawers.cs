using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Cowart.InspectorGroups.Editor
{
    internal static class DrawerHelp
    {
        public static void TypeError(Rect position, GUIContent label, string message)
        {
            var r = EditorGUI.PrefixLabel(position, label);
            EditorGUI.HelpBox(r, message, MessageType.Error);
        }
    }

    // ───────────── MinMaxSlider ─────────────
    [CustomPropertyDrawer(typeof(MinMaxSliderAttribute))]
    public class MinMaxSliderDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var a = (MinMaxSliderAttribute)attribute;
            bool isInt = property.propertyType == SerializedPropertyType.Vector2Int;
            if (property.propertyType != SerializedPropertyType.Vector2 && !isInt)
            {
                DrawerHelp.TypeError(position, label, "MinMaxSlider 仅支持 Vector2 / Vector2Int");
                return;
            }

            EditorGUI.BeginProperty(position, label, property);
            var r = EditorGUI.PrefixLabel(position, label);
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            const float fieldW = 50f, gap = 4f;
            var minRect = new Rect(r.x, r.y, fieldW, r.height);
            var sliderRect = new Rect(minRect.xMax + gap, r.y, r.width - (fieldW + gap) * 2, r.height);
            var maxRect = new Rect(sliderRect.xMax + gap, r.y, fieldW, r.height);

            float min = isInt ? property.vector2IntValue.x : property.vector2Value.x;
            float max = isInt ? property.vector2IntValue.y : property.vector2Value.y;

            EditorGUI.BeginChangeCheck();
            min = isInt ? EditorGUI.IntField(minRect, (int)min) : EditorGUI.FloatField(minRect, min);
            EditorGUI.MinMaxSlider(sliderRect, ref min, ref max, a.Min, a.Max);
            max = isInt ? EditorGUI.IntField(maxRect, (int)max) : EditorGUI.FloatField(maxRect, max);
            if (EditorGUI.EndChangeCheck())
            {
                min = Mathf.Clamp(min, a.Min, a.Max);
                max = Mathf.Clamp(max, min, a.Max);
                if (isInt) property.vector2IntValue = new Vector2Int(Mathf.RoundToInt(min), Mathf.RoundToInt(max));
                else property.vector2Value = new Vector2(min, max);
            }

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }
    }

    // ───────────── Tag ─────────────
    [CustomPropertyDrawer(typeof(TagAttribute))]
    public class TagDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                DrawerHelp.TypeError(position, label, "Tag 仅支持 string");
                return;
            }
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();
            string v = EditorGUI.TagField(position, label, string.IsNullOrEmpty(property.stringValue) ? "Untagged" : property.stringValue);
            if (EditorGUI.EndChangeCheck()) property.stringValue = v;
            EditorGUI.EndProperty();
        }
    }

    // ───────────── Layer ─────────────
    [CustomPropertyDrawer(typeof(LayerAttribute))]
    public class LayerDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                {
                    int v = EditorGUI.LayerField(position, label, property.intValue);
                    if (EditorGUI.EndChangeCheck()) property.intValue = v;
                    break;
                }
                case SerializedPropertyType.String:
                {
                    int layer = Mathf.Max(0, LayerMask.NameToLayer(property.stringValue));
                    int v = EditorGUI.LayerField(position, label, layer);
                    if (EditorGUI.EndChangeCheck()) property.stringValue = LayerMask.LayerToName(v);
                    break;
                }
                default:
                    EditorGUI.EndChangeCheck();
                    DrawerHelp.TypeError(position, label, "Layer 仅支持 int / string");
                    break;
            }
            EditorGUI.EndProperty();
        }
    }

    // ───────────── SceneName ─────────────
    [CustomPropertyDrawer(typeof(SceneNameAttribute))]
    public class SceneNameDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var scenes = EditorBuildSettings.scenes.Where(s => s.enabled)
                .Select(s => System.IO.Path.GetFileNameWithoutExtension(s.path)).ToArray();
            if (scenes.Length == 0)
            {
                DrawerHelp.TypeError(position, label, "Build Settings 中没有启用的场景");
                return;
            }

            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();
            if (property.propertyType == SerializedPropertyType.String)
            {
                int idx = Array.IndexOf(scenes, property.stringValue);
                int sel = EditorGUI.Popup(position, label.text, idx, scenes);
                if (EditorGUI.EndChangeCheck() && sel >= 0) property.stringValue = scenes[sel];
            }
            else if (property.propertyType == SerializedPropertyType.Integer)
            {
                int sel = EditorGUI.Popup(position, label.text, property.intValue, scenes);
                if (EditorGUI.EndChangeCheck()) property.intValue = sel;
            }
            else
            {
                EditorGUI.EndChangeCheck();
                DrawerHelp.TypeError(position, label, "SceneName 仅支持 string / int");
            }
            EditorGUI.EndProperty();
        }
    }

    // ───────────── EnumFlags ─────────────
    [CustomPropertyDrawer(typeof(EnumFlagsAttribute))]
    public class EnumFlagsDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var enumType = InspectorAttributeGUI.ElementType(fieldInfo.FieldType);
            if (property.propertyType != SerializedPropertyType.Enum || !enumType.IsEnum)
            {
                DrawerHelp.TypeError(position, label, "EnumFlags 仅支持枚举");
                return;
            }

            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();
            var current = (Enum)Enum.ToObject(enumType, property.intValue);
            var result = EditorGUI.EnumFlagsField(position, label, current);
            if (EditorGUI.EndChangeCheck()) property.intValue = Convert.ToInt32(result);
            EditorGUI.EndProperty();
        }
    }
}
