using UnityEngine;
using HighlightPlus;

/// <summary>
/// 骨骼选中高亮服务。使用 HighlightPlus 插件实现描边/发光选中效果。
/// 不改变材质颜色和透明度，选中效果通过 HighlightEffect 组件实现。
/// </summary>
public class BoneSelectionService
{
    private readonly SkeletonRegistry _registry;
    private readonly BoneMaterialService _materialService;

    // 当前高亮的骨骼 GameObject（用于取消高亮）
    private GameObject _currentHighlightedObj;

    public BoneSelectionService(SkeletonRegistry registry, BoneMaterialService materialService)
    {
        _registry = registry;
        _materialService = materialService;
    }

    /// <summary>
    /// 高亮指定骨骼（使用 HighlightPlus 描边效果），取消之前的高亮。
    /// </summary>
    public void SelectBone(int boneId)
    {
        // 取消之前的高亮
        ClearCurrentHighlight();

        // 查找并高亮新骨骼
        if (_registry.TryGet(boneId, out SkeletonInfo info))
        {
            if (info.boneGameObject == null) return;

            _currentHighlightedObj = info.boneGameObject;

            // 获取或添加 HighlightEffect 组件
            HighlightEffect effect = _currentHighlightedObj.GetComponent<HighlightEffect>();
            if (effect == null)
            {
                effect = _currentHighlightedObj.AddComponent<HighlightEffect>();
                ConfigureHighlightEffect(effect);
            }

            effect.highlighted = true;
        }
    }

    /// <summary>
    /// 重置所有骨骼高亮（关闭所有 HighlightEffect）。
    /// </summary>
    public void ResetBoneColor()
    {
        ClearCurrentHighlight();
    }

    /// <summary>
    /// 取消当前高亮的骨骼
    /// </summary>
    private void ClearCurrentHighlight()
    {
        if (_currentHighlightedObj != null)
        {
            HighlightEffect effect = _currentHighlightedObj.GetComponent<HighlightEffect>();
            if (effect != null)
            {
                effect.highlighted = false;
            }
            _currentHighlightedObj = null;
        }
    }

    /// <summary>
    /// 配置 HighlightEffect 的默认参数（描边 + 外发光）
    /// </summary>
    private void ConfigureHighlightEffect(HighlightEffect effect)
    {
        // 描边效果
        effect.outline = 1f;
        effect.outlineColor = Color.cyan;
        effect.outlineWidth = 0.8f;
        effect.outlineQuality = HighlightPlus.QualityLevel.Medium;

        // 外发光效果
        effect.glow = 0.5f;
        effect.glowWidth = 0.5f;
        effect.glowHQColor = new Color(0f, 1f, 1f, 1f);

        // 叠加色闪烁效果
        effect.overlay = 0.3f;
        effect.overlayColor = new Color(0f, 1f, 1f, 1f);
        effect.overlayAnimationSpeed = 2f;       // 闪烁速度
        effect.overlayMinIntensity = 0.1f;       // 最低亮度（产生明暗交替的闪烁）

        // 不使用透视效果
        effect.seeThrough = SeeThroughMode.Never;

        // 只影响当前对象
        effect.effectGroup = TargetOptions.OnlyThisObject;
    }
}
