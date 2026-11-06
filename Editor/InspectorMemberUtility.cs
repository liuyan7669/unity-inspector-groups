using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Cowart.InspectorGroups.Editor
{
    /// <summary>解析 Inspector 特性引用的成员和显示值。</summary>
    internal static class InspectorMemberUtility
    {
        /// <summary>按类型层级返回实例成员；同一类型内按 Button、ShowAsString 记录的源码行号保持声明顺序。</summary>
        internal static IEnumerable<MemberInfo> Members(Type type)
        {
            var seen = new HashSet<string>();
            for (; type != null && type != typeof(UnityEngine.MonoBehaviour) &&
                type != typeof(UnityEngine.ScriptableObject); type = type.BaseType)
            {
                var members = new List<MemberInfo>();
                foreach (MemberInfo member in type.GetMembers(BindingFlags.Instance |
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    // 优先保留最派生的虚方法声明，避免遗漏在重写方法上新增的特性。
                    string key = member is MethodInfo method && method.IsVirtual
                        ? method.GetBaseDefinition().ToString() : null;
                    if (key == null || seen.Add(key))
                    {
                        members.Add(member);
                    }
                }
                // GetMembers 按方法、属性、字段等种类分批返回，跨种类时不能体现源码顺序，分组归属会因此错位。
                foreach (MemberInfo member in members.OrderBy(item =>
                    item.GetCustomAttribute<ShowAsStringAttribute>()?.Line ?? item.GetCustomAttribute<ButtonAttribute>()?.Line ?? 0))
                {
                    yield return member;
                }
            }
        }

        internal static object Read(object owner, string memberName)
        {
            for (Type type = owner.GetType(); type != null; type = type.BaseType)
            {
                const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public |
                    BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
                FieldInfo field = type.GetField(memberName, flags);
                if (field != null) return field.GetValue(owner);
                PropertyInfo property = type.GetProperty(memberName, flags);
                if (property != null && property.GetIndexParameters().Length == 0 && property.GetGetMethod(true) != null)
                    return property.GetValue(owner);
                MethodInfo method = type.GetMethod(memberName, flags, null, Type.EmptyTypes, null);
                if (method != null && !method.ContainsGenericParameters) return method.Invoke(owner, null);
            }
            throw new InvalidOperationException($"找不到可读取的成员：{memberName}");
        }

        internal static bool Condition(object owner, string memberName, bool expected)
        {
            object value = Read(owner, memberName);
            if (!(value is bool result))
                throw new InvalidOperationException($"条件成员 {memberName} 必须返回 bool。");
            return result == expected;
        }

        internal static bool Condition(object owner, ShowIfAttribute attribute)
        {
            if (attribute.Values == null) return Condition(owner, attribute.MemberName, attribute.Expected);

            object value = Read(owner, attribute.MemberName);
            bool matches = attribute.Values.Length == 0
                ? (value is bool boolean ? boolean : value is UnityEngine.Object reference ? reference != null : value != null)
                : Array.Exists(attribute.Values, expected => Equals(value, expected));
            return attribute is HideIfAttribute ? !matches : matches;
        }

        internal static List<object> Options(object owner, string memberName, Type valueType)
        {
            object value = Read(owner, memberName);
            if (!(value is IEnumerable values) || value is string)
                throw new InvalidOperationException($"选项成员 {memberName} 必须返回集合。");
            var options = new List<object>();
            foreach (object option in values)
            {
                if (option == null ? valueType.IsValueType : !valueType.IsInstanceOfType(option))
                    throw new InvalidOperationException($"{memberName} 中的选项必须与字段类型 {valueType.Name} 一致。");
                options.Add(option);
                if (options.Count > 4096)
                    throw new InvalidOperationException("下拉选项不能超过 4096 项。");
            }
            if (options.Count == 0) throw new InvalidOperationException($"选项成员 {memberName} 返回了空集合。");
            return options;
        }

        internal static bool IsButtonMethod(MethodInfo method)
        {
            return !method.IsStatic && !method.ContainsGenericParameters && method.GetParameters().Length == 0 &&
                method.ReturnType == typeof(void) && method.GetCustomAttribute<AsyncStateMachineAttribute>() == null;
        }

        internal static string Text(object value)
        {
            if (value == null) return "null";
            if (value is string text) return text;
            if (value is IDictionary dictionary)
            {
                var lines = new List<string>();
                foreach (object key in dictionary.Keys)
                {
                    if (lines.Count >= 20) { lines.Add("…（仅显示前 20 项）"); break; }
                    lines.Add($"{key}: {dictionary[key]}");
                }
                return lines.Count == 0 ? "（空集合）" : string.Join("\n", lines);
            }
            if (value is IEnumerable values)
            {
                var result = new StringBuilder();
                int count = 0;
                foreach (object item in values)
                {
                    if (count++ >= 20) { result.Append("\n…（仅显示前 20 项）"); break; }
                    if (result.Length > 0) result.AppendLine();
                    result.Append(item is DictionaryEntry entry
                        ? $"{entry.Key}: {entry.Value}" : item?.ToString() ?? "null");
                }
                return result.Length == 0 ? "（空集合）" : result.ToString();
            }
            return value.ToString();
        }

        internal static string Error(Exception exception)
        {
            return (exception is TargetInvocationException invocation && invocation.InnerException != null
                ? invocation.InnerException : exception).Message;
        }
    }
}
