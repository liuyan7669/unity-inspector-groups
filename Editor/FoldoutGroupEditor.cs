using UnityEditor;
using UnityEngine;

namespace Cowart.InspectorGroups.Editor
{
    /// <summary>为没有专用 Inspector 的组件绘制分组和可组合的嵌套特性。</summary>
    [CustomEditor(typeof(MonoBehaviour), true, isFallback = true)]
    [CanEditMultipleObjects]
    public class FoldoutGroupEditor : UnityEditor.Editor
    {
        private readonly InspectorAttributeGUI attributeGUI = new InspectorAttributeGUI();

        /// <summary>释放内联资源编辑器。</summary>
        protected virtual void OnDisable()
        {
            attributeGUI.Dispose();
        }

        /// <summary>递归绘制字段、分组、内联对象和方法按钮。</summary>
        public override void OnInspectorGUI()
        {
            attributeGUI.DrawInspector(this);
        }
    }

    /// <summary>为没有专用 Inspector 的 ScriptableObject 提供相同的折叠分组。</summary>
    [CustomEditor(typeof(ScriptableObject), true, isFallback = true)]
    [CanEditMultipleObjects]
    public sealed class FoldoutGroupScriptableObjectEditor : FoldoutGroupEditor
    {
    }
}
