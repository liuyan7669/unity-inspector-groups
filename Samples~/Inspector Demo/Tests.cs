using System;
using System.Collections.Generic;
using Cowart.InspectorGroups;
using UnityEngine;

/// <summary>
/// 软件唯一测试脚本
/// </summary>
[ExecuteAlways]
public class Tests : MonoBehaviour
{

    /// <summary>原有测试索引。</summary>
    [FoldoutGroup("测试")]
    public int index;
    /// <summary>原有名称测试。</summary>
    [Header("名字")][Tooltip("这是名字")] public string name;
    /// <summary>原有数值列表。</summary>
    public List<int> list = new List<int>();
    /// <summary>原有对象列表。</summary>
    public List<GameObject> listGo = new List<GameObject>();

    [FoldoutGroup("01 · ReadOnly 只读", spacing: 8f)]
    [ReadOnly, Tooltip("不能手动修改；点击底部的增加进度按钮会更新此值。")]
    [SerializeField] private int buttonClickCount;

    [FoldoutGroup("02 · ShowIf 条件显示", spacing: 8f)]
    [Tooltip("切换开关，观察下面两段文字交替出现。")]
    [SerializeField] private bool showDetails = true;

    [ShowIf(nameof(showDetails)), Tooltip("ShowIf 的默认条件为 true。")]
    [SerializeField] private string visibleWhenTrue = "开关开启时显示";

    [ShowIf(nameof(showDetails), false)]
    [SerializeField] private string visibleWhenFalse = "开关关闭时显示";

    [FoldoutGroup("03 · EnableIf 条件启用", spacing: 8f)]
    [Tooltip("关闭后数值仍然可见，但不能编辑。也控制底部的条件按钮。")]
    [SerializeField] private bool canEdit = true;

    [EnableIf(nameof(canEdit)), Range(0, 100)]
    [SerializeField] private int editableValue = 50;

    [FoldoutGroup("04 · Required 必填提示", spacing: 8f)]
    [Required("填写文字后，此提示会消失。")]
    [SerializeField] private string requiredText;

    [Required("拖入任意场景对象，观察空引用提示消失。")]
    [SerializeField] private GameObject requiredObject;

    [FoldoutGroup("05 · InlineEditor 内联编辑", spacing: 8f)]
    [InlineEditor, Required, Tooltip("自动生成的临时配置，可直接修改其字段并点击内部按钮。")]
    [SerializeField] private InlineSampleData inlineSample;

    [ReadOnly, InlineEditor, Tooltip("引用同一份配置。这里的内容与上方同步，但不能编辑。")]
    [SerializeField] private InlineSampleData readOnlyInlineSample;

    [FoldoutGroup("06 · AssetPreview 资源预览", spacing: 8f)]
    [AssetPreview(96f), Tooltip("自动生成的测试纹理，也可以拖入自己的 Texture2D 查看。")]
    [SerializeField] private Texture2D previewTexture;

    [FoldoutGroup("07 · Dropdown 下拉来源", spacing: 8f)]
    [Tooltip("可编辑选项集合，下面三个下拉框分别通过字段、属性和方法读取它。")]
    [SerializeField] private string[] moduleOptions = { "模块一", "模块二", "模块三" };

    [Dropdown(nameof(moduleOptions)), Tooltip("选项来自字段。")]
    [SerializeField] private string fieldOption = "模块一";

    [Dropdown(nameof(ModuleOptions)), Tooltip("选项来自属性。")]
    [SerializeField] private string propertyOption = "模块二";

    [Dropdown(nameof(GetModuleOptions)), Tooltip("选项来自无参方法。")]
    [SerializeField] private string methodOption = "模块三";

    [FoldoutGroup("08 · ProgressBar 进度条", spacing: 8f)]
    [Range(0f, 100f), ProgressBar(0f, 100f, 22f)]
    [Tooltip("拖动原生 Range 滑条，观察下方进度条；也可使用底部的增加进度按钮。")]
    [SerializeField] private float progress = 35f;

    [FoldoutGroup("09 · ShowAsString 文本展示", spacing: 8f)]
    [ShowAsString("只读文本"), Tooltip("此字段使用文本展示，不能直接编辑。")]
    [SerializeField] private string textSample = "ShowAsString 与普通输入框的区别";

    [TextArea(2, 4), ShowAsString("文本摘要"), Tooltip("原生文本框与只读展示同时生效。")]
    [SerializeField] private string editableNote = "修改上方文本框，观察只读摘要。";

    [FoldoutGroup("10 · 嵌套与组合", spacing: 8f)]
    [Tooltip("展开后可测试嵌套分组、局部条件、多种展示和嵌套按钮。")]
    [SerializeField] private NestedSample nestedSample = new NestedSample();

    [Tooltip("展开元素，内部仍支持全部适用特性。")]
    [SerializeField] private List<NestedSample> nestedSamples = new List<NestedSample> { new NestedSample() };

    [Dropdown(nameof(GetScores)), ProgressBar(0, 100), ShowAsString, Range(0, 100), Min(10)]
    [Tooltip("列表的每个元素同时显示原生滑条、下拉框、进度条与文本。")]
    [SerializeField] private List<int> combinedScores = new List<int> { 25, 75 };

    [ReadOnly, Tooltip("父级 ReadOnly 会同时禁用内部字段和按钮。")]
    [SerializeField] private NestedSample readOnlyNestedSample = new NestedSample();

    [FoldoutGroup("12 · 字段外观与选择器", spacing: 8f, color: "#4FC3F7", border: true)]
    [Title("字段外观")]
    [InfoBox("拖动彩色进度条或双端滑块，修改下方数值。")]
    [Divider("新消息")]
    [Label("生命值"), ProgressBar(100f, "#E53935")]
    [SerializeField] private float health = 80f;

    [ColorField("#FFD54F", true), Line(LinePosition.Both)]
    [SerializeField] private string characterName = "Hero";

    [MinMaxSlider(0f, 100f)]
    [SerializeField] private Vector2 damageRange = new Vector2(10f, 30f);

    [ReadOnly, Label("唯一 ID")]
    [SerializeField] private int characterId = 1001;

    [ColorField(1f, 0.5f, 0.5f)]
    [SerializeField] private float importantValue = 1f;

    [ProgressBar(50f, "#42A5F5")]
    [SerializeField] private int mana = 30;

    [Title("项目选择器", "#AB47BC")]
    [Divider("这里是一段更长的分隔文字", textColor: "#FFCA80", lineColor: "#4488CC", thickness: 2f, fontSize: 15)]
    [Tag]
    [SerializeField] private string targetTag = "Untagged";

    [Layer]
    [SerializeField] private int targetLayer;

    [SceneName]
    [SerializeField] private string nextScene;

    private readonly string[] difficulties = { "Easy", "Normal", "Hard" };
    private readonly float[] speedOptions = { 0.5f, 1f, 2f, 4f };

    [Dropdown(nameof(difficulties))]
    [SerializeField] private string difficulty = "Normal";

    [Dropdown(nameof(speedOptions))]
    [SerializeField] private float speed = 1f;

    [Flags]
    private enum Element { None = 0, Fire = 1, Water = 2, Wind = 4, Earth = 8 }

    [EnumFlags]
    [SerializeField] private Element elements = Element.Fire | Element.Wind;

    private enum AttackType { Melee, Ranged, Magic }

    [SerializeField] private AttackType attackType;

    [ShowIf(nameof(attackType), AttackType.Ranged, AttackType.Magic)]
    [SerializeField] private float attackRange = 10f;

    [ShowIf(nameof(attackType), AttackType.Magic)]
    [SerializeField] private int manaCost = 5;

    [HideIf(nameof(attackType), AttackType.Melee)]
    [SerializeField] private float projectileSpeed = 5f;

    [Title("分割线位置", "#26A69A")]
    [Line]
    [SerializeField] private int lineAbove = 1;

    [Line("#FF7043", 2f, LinePosition.Below)]
    [SerializeField] private int lineBelow = 2;

    [Line("#29B6F6", 2f, LinePosition.Both)]
    [SerializeField] private int lineBoth = 3;

    [FoldoutGroup("12a · 移动与音效", color: "#4FC3F7")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private bool canDoubleJump;

    [ShowIf(nameof(canDoubleJump))]
    [SerializeField] private float doubleJumpHeight = 1f;

    [SerializeField] private AudioClip hitSound;

    [Range(0f, 1f)]
    [SerializeField] private float volume = 1f;

    [FoldoutGroup("12b · 战斗与外观", expanded: false, color: "#EF5350")]
    [SerializeField] private int attack = 10;

    [ProgressBar(100f, "#FFA726")]
    [SerializeField] private int defense = 40;

    [ReadOnly]
    [SerializeField] private float critRate = 0.15f;

    [SerializeField] private bool hasShield;

    [ShowIf(nameof(hasShield))]
    [SerializeField] private float shieldValue = 50f;

    [HideIf(nameof(hasShield)), InfoBox("没有护盾时显示。", InfoBoxType.Warning)]
    [SerializeField] private float dodgeRate = 0.1f;

    [Required, AssetPreview(64f)]
    [SerializeField] private GameObject weaponPrefab;

    [FoldoutGroup("12c · 资源预览与调试", color: "#42A5F5")]
    [AssetPreview(64f)]
    [SerializeField] private Sprite icon;

    [AssetPreview(48f)]
    [SerializeField] private Sprite avatar;

    [SerializeField] private Color tint = Color.white;

    [SerializeField] private bool debugMode;

    [ShowIf(nameof(debugMode))]
    [SerializeField] private float tickRate = 0.02f;

    [EndFoldout]
    [InfoBox("此字段位于折叠分组之外。")]
    [SerializeField] private string ungroupedNote = "收起上方分组后，这一项仍可见。";

    [FoldoutGroup("ShowAsString · 未序列化数据", spacing: 8f)]
    [ShowAsString("字典示例")]
    private readonly Dictionary<string, int> sampleDictionary = new Dictionary<string, int>
    {
        { "示例一", 10 },
        { "示例二", 20 }
    };

    /// <summary>实时显示按钮次数和当前进度。</summary>
    [ShowAsString("实时摘要")]
    public string Summary => $"按钮次数：{buttonClickCount}；进度：{progress:0}%";

    private string[] ModuleOptions => moduleOptions;
    private InlineSampleData generatedInlineSample;
    private Texture2D generatedTexture;

    private string[] GetModuleOptions()
    {
        return moduleOptions;
    }

    private int[] GetScores()
    {
        return new[] { 0, 10, 25, 50, 75, 100, 150 };
    }

    private void OnEnable()
    {
        // 演示对象仅驻留内存，不创建资源文件；禁用组件时只清理由本组件生成的对象。
        if (inlineSample == null)
        {
            generatedInlineSample = ScriptableObject.CreateInstance<InlineSampleData>();
            generatedInlineSample.name = "临时内联配置";
            generatedInlineSample.hideFlags = HideFlags.HideAndDontSave;
            inlineSample = generatedInlineSample;
        }
        if (readOnlyInlineSample == null)
        {
            readOnlyInlineSample = inlineSample;
        }
        if (previewTexture == null)
        {
            generatedTexture = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            generatedTexture.name = "临时预览纹理";
            generatedTexture.hideFlags = HideFlags.HideAndDontSave;
            Color[] pixels = new Color[64 * 64];
            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    pixels[y * 64 + x] = new Color(x / 63f, y / 63f, (x / 8 + y / 8) % 2 == 0 ? 0.25f : 0.75f);
                }
            }
            generatedTexture.SetPixels(pixels);
            generatedTexture.Apply();
            previewTexture = generatedTexture;
        }
    }

    private void OnDisable()
    {
        if (inlineSample == generatedInlineSample) inlineSample = null;
        if (readOnlyInlineSample == generatedInlineSample) readOnlyInlineSample = null;
        if (previewTexture == generatedTexture) previewTexture = null;
        foreach (UnityEngine.Object generated in new UnityEngine.Object[] { generatedInlineSample, generatedTexture })
        {
            if (generated == null) continue;
            if (Application.isPlaying)
            {
                Destroy(generated);
            }
            else
            {
                DestroyImmediate(generated);
            }
        }
        generatedInlineSample = null;
        generatedTexture = null;
    }

    [FoldoutGroup("11 · Button 操作按钮", spacing: 8f)]
    [Button("进度 +10，并更新只读计数", height: 28f)]
    private void AdvanceProgress()
    {
        progress = progress >= 100f ? 0f : Mathf.Min(100f, progress + 10f);
        buttonClickCount++;
    }

    [Button("恢复进度与条件开关")]
    private void ResetDemo()
    {
        progress = 35f;
        buttonClickCount = 0;
        showDetails = true;
        canEdit = true;
    }

    [Button("EnableIf 条件按钮"), EnableIf(nameof(canEdit))]
    private void EnabledButton()
    {
        buttonClickCount++;
    }

    [Button("ShowIf 条件按钮"), ShowIf(nameof(showDetails))]
    private void VisibleButton()
    {
        buttonClickCount++;
    }

    [Button("回满生命和法力", height: 28f)]
    private void Heal()
    {
        health = 100f;
        mana = 50;
    }

    [Button("打印角色状态"), ShowIf(nameof(debugMode))]
    private void PrintCharacterState()
    {
        Debug.Log($"{characterName} HP={health} MP={mana}");
    }

    [Button("仅 Play Mode 可点击", playModeOnly: true)]
    private void PlayModeButton()
    {
        buttonClickCount++;
        Debug.Log("Button 的 Play Mode 限制测试通过。", this);
    }

    /// <summary>可在普通字段或列表元素中展开的组合示例。</summary>
    [Serializable]
    public class NestedSample
    {
        [FoldoutGroup("嵌套 · 条件与组合")]
        [SerializeField] private bool showScore = true;
        [SerializeField] private bool allowEdit = true;

        [ShowIf(nameof(showScore)), EnableIf(nameof(allowEdit))]
        [Header("原生装饰与四种数值展示"), Space(4f), Tooltip("条件来源为当前嵌套对象，而不是外层 Tests。")]
        [Range(0, 100), Min(10), Dropdown(nameof(GetScores)), ProgressBar(0, 100), ShowAsString]
        [SerializeField] private int score = 50;

        [Required("输入内容后，嵌套字段的必填提示会消失。"), TextArea(2, 4)]
        [SerializeField] private string note;

        private int[] GetScores()
        {
            return new[] { 0, 10, 25, 50, 75, 100, 150 };
        }

        [FoldoutGroup("嵌套 · Button"), Button("重置嵌套分数"), EnableIf(nameof(allowEdit))]
        private void ResetScore()
        {
            score = 50;
        }
    }

    /// <summary>仅供当前组件临时创建和内联展示的配置对象。</summary>
    public class InlineSampleData : ScriptableObject
    {
        [FoldoutGroup("内联 · 嵌套特性")]
        [Required, Tooltip("可直接编辑这份临时配置。")]
        [SerializeField] private string displayName = "内联配置示例";

        [Range(0f, 100f), ProgressBar(0f, 100f)]
        [SerializeField] private float value = 40f;

        [ReadOnly]
        [SerializeField] private int revision = 1;

        /// <summary>内联对象的实时文本展示。</summary>
        [ShowAsString("内联摘要")]
        public string Summary => $"{displayName}，数值 {value:0}，修订 {revision}";

        [FoldoutGroup("内联 · Button"), Button("修订号 +1")]
        private void IncreaseRevision()
        {
            revision++;
        }
    }


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
