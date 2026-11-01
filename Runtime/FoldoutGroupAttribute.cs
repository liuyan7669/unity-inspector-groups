using System;

namespace Cowart.InspectorGroups
{
    /// <summary>从当前成员开始创建折叠分组，直到同一区域的下一个分组或成员列表结束。</summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class FoldoutGroupAttribute : Attribute
    {
        /// <summary>分组标题。</summary>
        public string Title { get; }

        /// <summary>首次显示时是否展开。</summary>
        public bool Expanded { get; }

        /// <summary>当前分组结束后到下一个分组的垂直间距，单位为 Inspector GUI 点。</summary>
        public float Spacing { get; }

        /// <summary>创建分组，可指定初始展开状态和到下一个分组的间距。</summary>
        public FoldoutGroupAttribute(string title, bool expanded = true, float spacing = 2f)
        {
            Title = title;
            Expanded = expanded;
            Spacing = Math.Max(0f, spacing);
        }
    }
}
