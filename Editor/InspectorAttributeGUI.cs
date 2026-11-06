using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Cowart.InspectorGroups.Editor
{
    /// <summary>在统一 Inspector 中组合绘制字段特性、文本成员和方法按钮。</summary>
    internal sealed class InspectorAttributeGUI : IDisposable
    {
        private readonly Dictionary<Object, UnityEditor.Editor> inlineEditors = new Dictionary<Object, UnityEditor.Editor>();
        private readonly HashSet<Object> inlinePath = new HashSet<Object>();
        private readonly List<object> objectPath = new List<object>();

        /// <summary>保存各文本控件上次绘制的实际宽度，供下一次布局精确计算换行。</summary>
        private sealed class TextLayoutState
        {
            internal float Width;

            /// <summary>创建文本布局状态。</summary>
            public TextLayoutState()
            {
            }
        }

        internal void DrawInspector(UnityEditor.Editor inspector, bool fullWidth = true)
        {
            if (!inlinePath.Add(inspector.target))
            {
                EditorGUILayout.HelpBox("循环引用：停止在此处展开内联编辑器。", MessageType.Info);
                return;
            }
            try
            {
                inspector.serializedObject.Update();
                DrawProperties(inspector.serializedObject, null, inspector, fullWidth);
                inspector.serializedObject.ApplyModifiedProperties();
            }
            finally { inlinePath.Remove(inspector.target); }
        }

        private void DrawProperties(SerializedObject serialized, SerializedProperty parent,
            UnityEditor.Editor inspector, bool fullWidth)
        {
            SerializedProperty iterator = parent == null ? serialized.GetIterator() : parent.Copy();
            int depth = parent == null ? 0 : parent.depth + 1;
            bool next = iterator.NextVisible(true);
            using (var groups = new InspectorGroupGUI())
            {
                while (next && iterator.depth >= depth)
                {
                    if (iterator.propertyPath == "m_Script")
                    {
                        using (new EditorGUI.DisabledScope(true)) EditorGUILayout.PropertyField(iterator);
                    }
                    else
                    {
                        InspectorPropertyContext context = InspectorPropertyContext.Resolve(iterator);
                        FoldoutGroupAttribute group = context.IsElement ? null : context.Field?.GetCustomAttribute<FoldoutGroupAttribute>();
                        if (!context.IsElement && context.Field?.IsDefined(typeof(EndFoldoutAttribute), true) == true)
                            groups.Dispose();
                        if (group != null)
                            groups.Begin(group, GroupKey(inspector, iterator.propertyPath), fullWidth);
                        if (groups.Expanded)
                        {
                            using (new EditorGUI.IndentLevelScope(groups.ContentIndent))
                                DrawField(iterator, context, inspector);
                        }
                    }
                    next = iterator.NextVisible(false);
                }
                // 序列化字段和未序列化成员属于同一成员列表，共用分组状态，保证分组延续和组间距一致。
                DrawMembers(inspector, parent?.propertyPath ?? "", fullWidth, groups);
            }
        }

        private static string GroupKey(UnityEditor.Editor inspector, string path)
        {
            return $"FoldoutGroup:{inspector.target.GetType().AssemblyQualifiedName}:{path}";
        }

        internal static bool Evaluate(MemberInfo member, object[] owners, out bool enabled)
        {
            enabled = !member.IsDefined(typeof(ReadOnlyAttribute), true);
            ShowIfAttribute show = member.GetCustomAttribute<ShowIfAttribute>();
            if (show != null)
            {
                bool visible = false;
                foreach (object owner in owners)
                {
                    bool matches = owner != null && InspectorMemberUtility.Condition(owner, show);
                    visible |= matches;
                    enabled &= matches;
                }
                if (!visible) return false;
            }
            EnableIfAttribute enable = member.GetCustomAttribute<EnableIfAttribute>();
            if (enable != null)
                foreach (object owner in owners)
                    enabled &= owner != null && InspectorMemberUtility.Condition(owner, enable.MemberName, enable.Expected);
            return true;
        }

        internal void DrawField(SerializedProperty property, InspectorPropertyContext context, UnityEditor.Editor inspector)
        {
            FieldInfo field = context.Field;
            bool enabled = true;
            try
            {
                if (field != null && !Evaluate(field, context.Owners, out enabled)) return;
            }
            catch (Exception exception)
            {
                EditorGUILayout.HelpBox(InspectorMemberUtility.Error(exception), MessageType.Error);
                enabled = false;
            }

            using (new EditorGUI.DisabledScope(!enabled))
            using (new FieldColorScope(field?.GetCustomAttribute<ColorFieldAttribute>()))
            {
                ShowAsStringAttribute text = field?.GetCustomAttribute<ShowAsStringAttribute>();
                DropdownAttribute dropdown = field?.GetCustomAttribute<DropdownAttribute>();
                ProgressBarAttribute progress = field?.GetCustomAttribute<ProgressBarAttribute>();
                bool presentation = text != null || dropdown != null || progress != null;
                bool container = IsContainer(property);
                bool array = property.isArray && property.propertyType != SerializedPropertyType.String;
                bool native = !array && (HasNativeRenderer(field) || (!presentation && !container));
                if (native)
                    EditorGUILayout.PropertyField(property, Label(property, field), true);
                else if (!context.IsElement) DrawDecorators(field);

                if (!array)
                {
                    Type valueType = context.IsElement ? ElementType(field.FieldType) : field?.FieldType;
                    if (dropdown != null) DrawDropdown(property, field, dropdown, context.Owners, valueType);
                    if (progress != null) DrawProgress(property, field, progress, valueType);
                    if (text != null)
                    {
                        if (context.IsElement)
                        {
                            object[] elements = new object[inspector.targets.Length];
                            for (int i = 0; i < elements.Length; i++)
                                elements[i] = InspectorPropertyContext.Value(inspector.targets[i], property.propertyPath);
                            DrawTextValues(elements, text.Label ?? property.displayName, Label(property, field).tooltip);
                        }
                        else DrawText(context.Owners, field.Name, text.Label, Label(property, field).tooltip);
                    }
                }
                if (container && !native) DrawContainer(property, field, inspector);

                RequiredAttribute required = field?.GetCustomAttribute<RequiredAttribute>();
                if (required != null && !context.IsElement) DrawRequired(property, required, inspector.targets);

                if (!array && field != null && field.IsDefined(typeof(AssetPreviewAttribute), true))
                    DrawPreview(property, field.GetCustomAttribute<AssetPreviewAttribute>(), inspector);
                if (!array && field != null && field.IsDefined(typeof(InlineEditorAttribute), true))
                    DrawInline(property, inspector);

                if (!context.IsElement && field != null)
                    foreach (LineAttribute line in field.GetCustomAttributes<LineAttribute>())
                        if (line.Position != LinePosition.Above)
                            LineDecoratorDrawer.Draw(EditorGUI.IndentedRect(EditorGUILayout.GetControlRect(false, line.Thickness + 8f)), line);
            }
        }

        internal static Type ElementType(Type type)
        {
            return type.IsArray ? type.GetElementType() : type.IsGenericType ? type.GetGenericArguments()[0] : type;
        }

        internal static bool HasNativeRenderer(FieldInfo field)
        {
            if (field == null) return false;
            foreach (PropertyAttribute attribute in field.GetCustomAttributes<PropertyAttribute>())
                if (!(attribute is HeaderAttribute) && !(attribute is SpaceAttribute) && !(attribute is TooltipAttribute) &&
                    !(attribute is TitleAttribute) && !(attribute is LineAttribute) && !(attribute is InfoBoxAttribute) && !(attribute is DividerAttribute))
                    return true;
            return false;
        }

        private static GUIContent Label(SerializedProperty property, FieldInfo field)
        {
            return new GUIContent(field?.GetCustomAttribute<LabelAttribute>()?.Text ?? property.displayName,
                field?.GetCustomAttribute<TooltipAttribute>()?.tooltip);
        }

        private static void DrawDecorators(FieldInfo field)
        {
            if (field == null) return;
            var attributes = new List<PropertyAttribute>(field.GetCustomAttributes<PropertyAttribute>());
            attributes.Sort((a, b) => a.order.CompareTo(b.order));
            foreach (PropertyAttribute attribute in attributes)
            {
                if (attribute is SpaceAttribute space) EditorGUILayout.Space(space.height);
                else if (attribute is TitleAttribute title)
                    TitleDecoratorDrawer.Draw(EditorGUI.IndentedRect(EditorGUILayout.GetControlRect(false, TitleDecoratorDrawer.Height)), title);
                else if (attribute is DividerAttribute divider)
                    DividerDecoratorDrawer.Draw(EditorGUI.IndentedRect(EditorGUILayout.GetControlRect(false, DividerDecoratorDrawer.Height(divider))), divider);
                else if (attribute is InfoBoxAttribute info)
                    InfoBoxDecoratorDrawer.Draw(EditorGUI.IndentedRect(EditorGUILayout.GetControlRect(false, InfoBoxDecoratorDrawer.Height(info))), info);
                else if (attribute is LineAttribute line && line.Position != LinePosition.Below)
                    LineDecoratorDrawer.Draw(EditorGUI.IndentedRect(EditorGUILayout.GetControlRect(false, line.Thickness + 8f)), line);
                else if (attribute is HeaderAttribute header)
                {
                    EditorGUILayout.Space(8f);
                    EditorGUILayout.LabelField(header.header, EditorStyles.boldLabel);
                }
            }
        }

        private readonly struct FieldColorScope : IDisposable
        {
            private readonly Color background;
            private readonly Color content;
            private readonly Color label;

            internal FieldColorScope(ColorFieldAttribute attribute)
            {
                background = GUI.backgroundColor;
                content = GUI.contentColor;
                label = EditorStyles.label.normal.textColor;
                if (attribute == null) return;
                GUI.backgroundColor = attribute.Color;
                if (!attribute.TintText) return;
                GUI.contentColor = attribute.Color;
                EditorStyles.label.normal.textColor = attribute.Color;
            }

            public void Dispose()
            {
                GUI.backgroundColor = background;
                GUI.contentColor = content;
                EditorStyles.label.normal.textColor = label;
            }
        }

        private static bool IsContainer(SerializedProperty property)
        {
            return property.isArray && property.propertyType != SerializedPropertyType.String ||
                property.propertyType == SerializedPropertyType.Generic || property.propertyType == SerializedPropertyType.ManagedReference;
        }

        private void DrawContainer(SerializedProperty property, FieldInfo field, UnityEditor.Editor inspector)
        {
            Rect rect = EditorGUILayout.GetControlRect();
            EditorGUI.BeginProperty(rect, Label(property, field), property);
            property.isExpanded = EditorGUI.Foldout(rect, property.isExpanded, Label(property, field), true);
            EditorGUI.EndProperty();
            if (!property.isExpanded) return;
            using (new EditorGUI.IndentLevelScope())
            {
                if (property.isArray)
                {
                    EditorGUILayout.PropertyField(property.FindPropertyRelative("Array.size"));
                    for (int i = 0; i < property.arraySize; i++)
                    {
                        SerializedProperty element = property.GetArrayElementAtIndex(i);
                        DrawField(element, InspectorPropertyContext.Resolve(element), inspector);
                    }
                    return;
                }
                object value = InspectorPropertyContext.Value(inspector.target, property.propertyPath);
                if (value == null) return;
                if (objectPath.Exists(item => ReferenceEquals(item, value)))
                {
                    EditorGUILayout.HelpBox("循环引用：停止在此处展开嵌套对象。", MessageType.Info);
                    return;
                }
                objectPath.Add(value);
                try { DrawProperties(property.serializedObject, property, inspector, false); }
                finally { objectPath.RemoveAt(objectPath.Count - 1); }
            }
        }

        private static void DrawRequired(SerializedProperty property, RequiredAttribute attribute, Object[] owners)
        {
            bool missing = false;
            foreach (Object owner in owners)
            {
                // 每个对象分别校验，不能让多选中第一个有效值遮住其他对象的空引用。
                var serialized = new SerializedObject(owner);
                try
                {
                    SerializedProperty value = serialized.FindProperty(property.propertyPath);
                    if (value == null) continue;
                    try { missing |= RequiredMissing(value); }
                    catch (InvalidOperationException exception)
                    {
                        EditorGUILayout.HelpBox(exception.Message, MessageType.Warning);
                        return;
                    }
                }
                finally { serialized.Dispose(); }
            }
            // 绘制时当前编辑尚未 Apply，先使用当前合并属性让单选提示即时刷新。
            if (owners.Length == 1)
            {
                missing = RequiredMissing(property);
            }
            if (missing)
                EditorGUILayout.HelpBox(string.IsNullOrEmpty(attribute.Message)
                    ? $"{property.displayName} 必须填写（至少一个选中对象为空）。" : attribute.Message, MessageType.Warning);
        }

        internal static bool RequiredMissing(SerializedProperty property)
        {
            if (property.propertyType == SerializedPropertyType.ObjectReference) return property.objectReferenceValue == null;
            if (property.propertyType == SerializedPropertyType.String) return string.IsNullOrWhiteSpace(property.stringValue);
            if (property.isArray) return property.arraySize == 0;
            throw new InvalidOperationException("Required 仅支持对象引用、字符串和数组/列表。");
        }

        private static void DrawDropdown(SerializedProperty property, FieldInfo field, DropdownAttribute attribute,
            object[] owners, Type valueType)
        {
            try
            {
                Type type = valueType;
                if (type != typeof(string) && type != typeof(int) && type != typeof(float) &&
                    !type.IsEnum && !typeof(Object).IsAssignableFrom(type))
                    throw new InvalidOperationException("Dropdown 支持 string、int、float、枚举和 Unity 对象引用。");
                List<object> options = DropdownOptions(owners[0], field, attribute.MemberName, type);
                for (int i = 1; i < owners.Length; i++)
                {
                    List<object> other = DropdownOptions(owners[i], field, attribute.MemberName, type);
                    if (other.Count != options.Count) throw new InvalidOperationException("多选对象的下拉选项不同，请单独编辑。");
                    for (int j = 0; j < options.Count; j++)
                        if (!Equals(options[j], other[j])) throw new InvalidOperationException("多选对象的下拉选项不同，请单独编辑。");
                }

                object current;
                if (type == typeof(string)) current = property.stringValue;
                else if (type == typeof(int)) current = property.intValue;
                else if (type == typeof(float)) current = property.floatValue;
                else if (type.IsEnum) current = Enum.ToObject(type, property.intValue);
                else current = property.objectReferenceValue;

                int index = options.FindIndex(option => Equals(option, current));
                var labels = new string[options.Count + 1];
                labels[0] = property.hasMultipleDifferentValues ? "—（多个值）" : $"（当前值不在选项中：{current ?? "null"}）";
                for (int i = 0; i < options.Count; i++) labels[i + 1] = options[i]?.ToString() ?? "null";
                Rect rect = EditorGUILayout.GetControlRect();
                GUIContent label = Label(property, field);
                EditorGUI.BeginProperty(rect, label, property);
                try
                {
                    EditorGUI.BeginChangeCheck();
                    var contents = Array.ConvertAll(labels, item => new GUIContent(item));
                    int selected = EditorGUI.Popup(rect, label,
                        property.hasMultipleDifferentValues ? 0 : index + 1, contents);
                    if (EditorGUI.EndChangeCheck() && selected > 0)
                    {
                        SetDropdownValue(property, type, options[selected - 1]);
                    }
                }
                finally { EditorGUI.EndProperty(); }
            }
            catch (Exception exception)
            {
                EditorGUILayout.PropertyField(property, true);
                EditorGUILayout.HelpBox(InspectorMemberUtility.Error(exception), MessageType.Error);
            }
        }

        internal static void SetDropdownValue(SerializedProperty property, Type type, object value)
        {
            if (type == typeof(string)) property.stringValue = (string)value;
            else if (type == typeof(int)) property.intValue = (int)value;
            else if (type == typeof(float)) property.floatValue = (float)value;
            else if (type.IsEnum) property.intValue = Convert.ToInt32(value);
            else property.objectReferenceValue = (Object)value;
        }

        internal static List<object> DropdownOptions(object owner, FieldInfo field, string memberName, Type valueType = null)
        {
            Type type = valueType ?? field.FieldType;
            List<object> options = InspectorMemberUtility.Options(owner, memberName, type);
            RangeAttribute range = field.GetCustomAttribute<RangeAttribute>();
            MinAttribute min = field.GetCustomAttribute<MinAttribute>();
            if ((type == typeof(int) || type == typeof(float)) && (range != null || min != null))
            {
                options.RemoveAll(option =>
                {
                    float value = Convert.ToSingle(option);
                    return float.IsNaN(value) || float.IsInfinity(value) ||
                        range != null && (value < range.min || value > range.max) || min != null && value < min.min;
                });
                if (options.Count == 0) throw new InvalidOperationException("没有满足 Range/Min 范围的下拉选项。");
            }
            return options;
        }

        private static void DrawProgress(SerializedProperty property, FieldInfo field, ProgressBarAttribute attribute, Type valueType)
        {
            if ((valueType != typeof(int) && valueType != typeof(float)) ||
                float.IsNaN(attribute.Min) || float.IsNaN(attribute.Max) ||
                float.IsInfinity(attribute.Min) || float.IsInfinity(attribute.Max) || attribute.Max <= attribute.Min)
            {
                EditorGUILayout.PropertyField(property, true);
                EditorGUILayout.HelpBox("ProgressBar 需要 int/float 字段及有限的 min < max 范围。", MessageType.Error);
                return;
            }
            float value = valueType == typeof(int) ? property.intValue : property.floatValue;
            Rect rect = EditorGUILayout.GetControlRect(false, attribute.Height);
            EditorGUI.BeginProperty(rect, Label(property, field), property);
            rect = EditorGUI.PrefixLabel(rect, Label(property, field));
            bool finite = !float.IsNaN(value) && !float.IsInfinity(value);
            float progress = finite ? Mathf.InverseLerp(attribute.Min, attribute.Max, value) : 0f;
            bool mixed = property.hasMultipleDifferentValues;
            string text = mixed ? "—（多个值）" : $"{value:0.##} / {attribute.Max:0.##}";
            if (attribute.FillColor is Color color)
            {
                EditorGUI.DrawRect(rect, new Color(0.15f, 0.15f, 0.15f));
                if (!mixed)
                    EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width * progress, rect.height), color);
                GUI.Label(rect, text, EditorStyles.centeredGreyMiniLabel);
                Event current = Event.current;
                int id = GUIUtility.GetControlID(FocusType.Passive, rect);
                if (current.GetTypeForControl(id) == EventType.MouseUp && GUIUtility.hotControl == id)
                {
                    GUIUtility.hotControl = 0;
                    current.Use();
                }
                else if (GUI.enabled && rect.width > 0f &&
                    (current.GetTypeForControl(id) == EventType.MouseDown && current.button == 0 && rect.Contains(current.mousePosition) ||
                     current.GetTypeForControl(id) == EventType.MouseDrag && GUIUtility.hotControl == id))
                {
                    GUIUtility.hotControl = id;
                    float next = Mathf.Lerp(attribute.Min, attribute.Max, (current.mousePosition.x - rect.x) / rect.width);
                    if (valueType == typeof(int)) property.intValue = Mathf.RoundToInt(next);
                    else property.floatValue = next;
                    GUI.changed = true;
                    current.Use();
                }
            }
            else EditorGUI.ProgressBar(rect, mixed ? 0f : progress, text);
            EditorGUI.EndProperty();
        }

        private static void DrawPreview(SerializedProperty property, AssetPreviewAttribute attribute, UnityEditor.Editor inspector)
        {
            if (property.propertyType != SerializedPropertyType.ObjectReference)
            {
                EditorGUILayout.HelpBox("AssetPreview 需要 Unity 对象引用字段。", MessageType.Warning);
                return;
            }
            if (property.hasMultipleDifferentValues || property.objectReferenceValue == null) return;
            Object asset = property.objectReferenceValue;
            Texture preview = UnityEditor.AssetPreview.GetAssetPreview(asset);
            if (preview == null)
            {
                preview = UnityEditor.AssetPreview.GetMiniThumbnail(asset);
                if (UnityEditor.AssetPreview.IsLoadingAssetPreview(asset.GetInstanceID())) inspector.Repaint();
            }
            if (preview == null) return;
            Rect rect = EditorGUILayout.GetControlRect(false, attribute.Height);
            GUI.DrawTexture(EditorGUI.IndentedRect(rect), preview, ScaleMode.ScaleToFit, true);
        }

        private void DrawInline(SerializedProperty property, UnityEditor.Editor inspector)
        {
            if (property.propertyType != SerializedPropertyType.ObjectReference)
            {
                EditorGUILayout.HelpBox("InlineEditor 需要 ScriptableObject 引用字段。", MessageType.Warning);
                return;
            }
            if (property.hasMultipleDifferentValues || property.objectReferenceValue == null) return;
            if (!(property.objectReferenceValue is ScriptableObject asset))
            {
                EditorGUILayout.HelpBox("InlineEditor 仅支持 ScriptableObject。", MessageType.Warning);
                return;
            }
            if (inspector.targets.Length > 1)
            {
                EditorGUILayout.HelpBox("内联编辑请单选对象；引用字段仍可批量修改。", MessageType.Info);
                return;
            }
            if (!inlineEditors.TryGetValue(asset, out UnityEditor.Editor editor) || editor == null)
            {
                editor = UnityEditor.Editor.CreateEditor(asset);
                inlineEditors[asset] = editor;
            }
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                DrawInspector(editor, false);
            }
        }

        private static object[] Owners(UnityEditor.Editor inspector, string path)
        {
            var owners = new object[inspector.targets.Length];
            for (int i = 0; i < owners.Length; i++)
                owners[i] = string.IsNullOrEmpty(path) ? inspector.targets[i] : InspectorPropertyContext.Value(inspector.targets[i], path);
            return owners;
        }

        private void DrawMembers(UnityEditor.Editor inspector, string path, bool fullWidth, InspectorGroupGUI groups)
        {
            object[] owners = Owners(inspector, path);
            if (Array.Exists(owners, item => item == null)) return;
            foreach (MemberInfo member in InspectorMemberUtility.Members(owners[0].GetType()))
            {
                if (member.IsDefined(typeof(HideInInspector), true)) continue;
                ShowAsStringAttribute text = member.GetCustomAttribute<ShowAsStringAttribute>();
                MethodInfo method = member as MethodInfo;
                ButtonAttribute button = method?.GetCustomAttribute<ButtonAttribute>();
                string memberPath = string.IsNullOrEmpty(path) ? member.Name : path + "." + member.Name;
                bool showText = text != null && (member is FieldInfo || member is PropertyInfo) &&
                    inspector.serializedObject.FindProperty(memberPath) == null;
                if (!showText && button == null) continue;
                FoldoutGroupAttribute group = member.GetCustomAttribute<FoldoutGroupAttribute>();
                if (group != null) groups.Begin(group, GroupKey(inspector, memberPath), fullWidth);
                if (!groups.Expanded) continue;
                bool enabled;
                try { if (!Evaluate(member, owners, out enabled)) continue; }
                catch (Exception exception)
                {
                    EditorGUILayout.HelpBox(InspectorMemberUtility.Error(exception), MessageType.Error);
                    continue;
                }
                using (new EditorGUI.IndentLevelScope(groups.ContentIndent))
                using (new EditorGUI.DisabledScope(!enabled))
                {
                    if (showText) DrawText(owners, member.Name, text.Label);
                    if (button == null) continue;
                    if (!InspectorMemberUtility.IsButtonMethod(method))
                    {
                        EditorGUILayout.HelpBox($"{method.Name}：Button 仅支持非异步、无参数、返回 void 的实例方法。", MessageType.Warning);
                        continue;
                    }
                    using (new EditorGUI.DisabledScope(button.PlayModeOnly && !Application.isPlaying))
                    {
                        var content = new GUIContent(string.IsNullOrEmpty(button.Label)
                            ? ObjectNames.NicifyVariableName(method.Name) : button.Label);
                        // GUILayout.Button 不响应缩进，按当前层级缩进后与同组字段对齐。
                        Rect rect = EditorGUI.IndentedRect(button.Height > 0f
                            ? GUILayoutUtility.GetRect(content, GUI.skin.button, GUILayout.ExpandWidth(true), GUILayout.Height(button.Height))
                            : GUILayoutUtility.GetRect(content, GUI.skin.button, GUILayout.ExpandWidth(true)));
                        if (!GUI.Button(rect, content)) continue;
                        InvokeButton(inspector, method, path);
                        inspector.Repaint();
                        GUIUtility.ExitGUI();
                    }
                }
            }
        }

        internal static void InvokeButton(UnityEditor.Editor inspector, MethodInfo method, string path = "")
        {
            // 先落下字段编辑，再记录按钮调用对选中对象的改动。
            inspector.serializedObject.ApplyModifiedProperties();
            Undo.RecordObjects(inspector.targets, method.Name);
            foreach (Object root in inspector.targets)
            {
                try
                {
                    object owner = string.IsNullOrEmpty(path) ? root : InspectorPropertyContext.Value(root, path);
                    method.Invoke(owner, null);
                    if (!string.IsNullOrEmpty(path) && owner.GetType().IsValueType)
                        InspectorPropertyContext.Write(root, path, owner);
                }
                catch (Exception exception) { Debug.LogException(exception is TargetInvocationException invocation
                    ? invocation.InnerException ?? exception : exception, root); }
                finally
                {
                    if (root != null)
                    {
                        EditorUtility.SetDirty(root);
                        if (PrefabUtility.IsPartOfPrefabInstance(root))
                            PrefabUtility.RecordPrefabInstancePropertyModifications(root);
                    }
                }
            }
            inspector.serializedObject.Update();
        }

        private static void DrawText(object[] owners, string memberName, string label, string tooltip = null)
        {
            try
            {
                object[] values = new object[owners.Length];
                for (int i = 0; i < values.Length; i++) values[i] = InspectorMemberUtility.Read(owners[i], memberName);
                DrawTextValues(values, string.IsNullOrEmpty(label) ? ObjectNames.NicifyVariableName(memberName) : label, tooltip);
            }
            catch (Exception exception)
            {
                EditorGUILayout.HelpBox($"{memberName}：{InspectorMemberUtility.Error(exception)}", MessageType.Error);
            }
        }

        private static void DrawTextValues(object[] values, string label, string tooltip)
        {
            string value = InspectorMemberUtility.Text(values[0]);
            for (int i = 1; i < values.Length; i++)
                if (InspectorMemberUtility.Text(values[i]) != value) { value = "—（多个值）"; break; }
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PrefixLabel(new GUIContent(label, tooltip));
                int id = GUIUtility.GetControlID("Cowart.ShowAsString.Layout".GetHashCode(), FocusType.Passive);
                var state = (TextLayoutState)GUIUtility.GetStateObject(typeof(TextLayoutState), id);
                float width = state.Width > 0f ? state.Width : Mathf.Max(20f,
                    EditorGUIUtility.currentViewWidth - EditorGUIUtility.labelWidth - 40f);
                float height = Mathf.Clamp(EditorStyles.wordWrappedLabel.CalcHeight(new GUIContent(value), width),
                    EditorGUIUtility.singleLineHeight, 160f);
                Rect rect = GUILayoutUtility.GetRect(0f, height, GUILayout.ExpandWidth(true), GUILayout.Height(height));
                if (Event.current.type == EventType.Repaint && !Mathf.Approximately(state.Width, rect.width))
                {
                    state.Width = rect.width;
                    // 宽度在实际绘制后才确定；下一次布局按精确宽度更新高度，适配面板缩放与嵌套缩进。
                    UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                }
                // 前缀标签已处理缩进；SelectableLabel 会再次缩进，嵌套越深值列越偏离其他控件。
                using (new EditorGUI.IndentLevelScope(-EditorGUI.indentLevel))
                    EditorGUI.SelectableLabel(rect, value, EditorStyles.wordWrappedLabel);
            }
        }

        /// <summary>释放缓存的内联编辑器。</summary>
        public void Dispose()
        {
            foreach (UnityEditor.Editor editor in inlineEditors.Values)
                if (editor != null) Object.DestroyImmediate(editor);
            inlineEditors.Clear();
        }
    }
}
