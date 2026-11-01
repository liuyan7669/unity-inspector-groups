using System;
using System.Collections;
using System.Reflection;
using UnityEditor;

namespace Cowart.InspectorGroups.Editor
{
    /// <summary>解析嵌套序列化路径，条件和选项来源始终相对于字段所属对象。</summary>
    internal sealed class InspectorPropertyContext
    {
        internal object[] Owners;
        internal FieldInfo Field;
        internal bool IsElement;

        internal static InspectorPropertyContext Resolve(SerializedProperty property)
        {
            var context = new InspectorPropertyContext();
            var targets = property.serializedObject.targetObjects;
            context.Owners = new object[targets.Length];
            string[] parts = Parts(property.propertyPath);
            for (int i = 0; i < targets.Length; i++)
            {
                object current = targets[i];
                object collectionOwner = null;
                FieldInfo collectionField = null;
                for (int j = 0; j < parts.Length; j++)
                {
                    if (current == null) break;
                    string part = parts[j];
                    if (part.StartsWith("[", StringComparison.Ordinal))
                    {
                        int index = int.Parse(part.Substring(1, part.Length - 2));
                        current = current is IList list && index < list.Count ? list[index] : null;
                        if (j == parts.Length - 1)
                        {
                            context.Owners[i] = collectionOwner;
                            context.Field = collectionField;
                            context.IsElement = true;
                        }
                    }
                    else
                    {
                        FieldInfo field = FindField(current.GetType(), part);
                        collectionOwner = current;
                        collectionField = field;
                        if (j == parts.Length - 1)
                        {
                            context.Owners[i] = current;
                            context.Field = field;
                        }
                        current = field?.GetValue(current);
                    }
                }
            }
            return context;
        }

        internal static FieldInfo FindField(Type type, string name)
        {
            for (; type != null; type = type.BaseType)
            {
                FieldInfo field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public |
                    BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                if (field != null) return field;
            }
            return null;
        }

        internal static object Value(object root, string path)
        {
            object current = root;
            foreach (string part in Parts(path))
            {
                if (current == null) return null;
                if (part.StartsWith("[", StringComparison.Ordinal))
                {
                    int index = int.Parse(part.Substring(1, part.Length - 2));
                    current = current is IList list && index < list.Count ? list[index] : null;
                }
                else current = FindField(current.GetType(), part)?.GetValue(current);
            }
            return current;
        }

        internal static void Write(object root, string path, object value)
        {
            string[] parts = Parts(path);
            if (parts.Length > 0) WritePart(root, parts, 0, value);
        }

        private static void WritePart(object current, string[] parts, int index, object value)
        {
            string part = parts[index];
            bool element = part.StartsWith("[", StringComparison.Ordinal);
            int elementIndex = element ? int.Parse(part.Substring(1, part.Length - 2)) : -1;
            FieldInfo field = element ? null : FindField(current.GetType(), part);
            object child = element ? ((IList)current)[elementIndex] : field.GetValue(current);
            if (index < parts.Length - 1)
            {
                WritePart(child, parts, index + 1, value);
                value = child;
            }
            // 逐层写回，保证结构体、结构体列表元素不会只修改临时装箱副本。
            if (element) ((IList)current)[elementIndex] = value;
            else field.SetValue(current, value);
        }

        private static string[] Parts(string path)
        {
            return path.Replace(".Array.data[", ".[").Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
