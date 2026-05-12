using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 材质模式切换服务。负责材质透明/不透明模式切换。
/// 通过缓存 sharedMaterial 引用，确保重置时能完全恢复到原始状态。
/// 选中高亮由 HighlightPlus 插件处理，本服务不负责选中效果。
/// </summary>
public class BoneMaterialService
{
    private readonly SkeletonRegistry _registry;

    // 预缓存 Shader 属性ID（静态，程序生命周期内只计算一次）
    private static readonly int PropMode = Shader.PropertyToID("_Mode");
    private static readonly int PropSrcBlend = Shader.PropertyToID("_SrcBlend");
    private static readonly int PropDstBlend = Shader.PropertyToID("_DstBlend");
    private static readonly int PropZWrite = Shader.PropertyToID("_ZWrite");
    private static readonly int PropColor = Shader.PropertyToID("_Color");
    private static readonly int PropBaseColor = Shader.PropertyToID("_BaseColor");
    // URP Lit shader 属性
    private static readonly int PropSurface = Shader.PropertyToID("_Surface");
    private static readonly int PropBlend = Shader.PropertyToID("_Blend");
    private static readonly int PropSrcBlendAlpha = Shader.PropertyToID("_SrcBlendAlpha");
    private static readonly int PropDstBlendAlpha = Shader.PropertyToID("_DstBlendAlpha");
    private static readonly int PropAlphaToMask = Shader.PropertyToID("_AlphaToMask");

    /// <summary>
    /// 缓存每个骨骼的原始 sharedMaterial 引用（boneId → 原始共享材质）。
    /// 重置时直接赋回 sharedMaterial，确保与加载时完全一致。
    /// </summary>
    private Dictionary<int, Material> _originalMaterials;

    public BoneMaterialService(SkeletonRegistry registry)
    {
        _registry = registry;
        _originalMaterials = new Dictionary<int, Material>();
    }

    /// <summary>
    /// 缓存骨骼的原始 sharedMaterial（首次修改前调用）。
    /// </summary>
    public void CacheOriginalMaterial(int boneId, MeshRenderer meshRenderer)
    {
        if (!_originalMaterials.ContainsKey(boneId))
        {
            _originalMaterials[boneId] = meshRenderer.sharedMaterial;
        }
    }

    /// <summary>
    /// 恢复骨骼的原始材质（直接赋回 sharedMaterial，销毁实例材质）。
    /// </summary>
    public void RestoreOriginalMaterial(int boneId, MeshRenderer meshRenderer)
    {
        if (_originalMaterials.TryGetValue(boneId, out Material originalMat))
        {
            meshRenderer.material = originalMat;
        }
    }

    /// <summary>
    /// 将材质切换到透明渲染模式（兼容 Standard 和 URP Lit shader）
    /// </summary>
    public void SetTransparentMode(Material material)
    {
        // Standard shader 属性
        material.SetFloat(PropMode, 3f);
        material.SetInt(PropSrcBlend, (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt(PropDstBlend, (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt(PropZWrite, 0);

        // URP Lit shader 属性
        material.SetFloat(PropSurface, 1f); // 1 = Transparent
        material.SetFloat(PropBlend, 0f);   // 0 = Alpha blend
        material.SetInt(PropSrcBlendAlpha, (int)UnityEngine.Rendering.BlendMode.One);
        material.SetInt(PropDstBlendAlpha, (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetFloat(PropAlphaToMask, 0f);

        // Keywords
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");

        material.renderQueue = 3000;
    }

    /// <summary>
    /// 将材质切换到不透明渲染模式（兼容 Standard 和 URP Lit shader）
    /// </summary>
    public void SetOpaqueMode(Material material)
    {
        // Standard shader 属性
        material.SetFloat(PropMode, 0f);
        material.SetInt(PropSrcBlend, (int)UnityEngine.Rendering.BlendMode.One);
        material.SetInt(PropDstBlend, (int)UnityEngine.Rendering.BlendMode.Zero);
        material.SetInt(PropZWrite, 1);

        // URP Lit shader 属性
        material.SetFloat(PropSurface, 0f); // 0 = Opaque
        material.SetFloat(PropBlend, 0f);
        material.SetInt(PropSrcBlendAlpha, (int)UnityEngine.Rendering.BlendMode.One);
        material.SetInt(PropDstBlendAlpha, (int)UnityEngine.Rendering.BlendMode.Zero);
        material.SetFloat(PropAlphaToMask, 0f);

        // Keywords
        material.DisableKeyword("_ALPHATEST_ON");
        material.DisableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");

        material.renderQueue = -1;
    }

    /// <summary>
    /// 设置材质颜色（同时设置 _Color 和 _BaseColor 以兼容 Standard 和 URP Lit shader）
    /// </summary>
    public void SetMaterialColor(Material material, Color color)
    {
        material.SetColor(PropColor, color);
        material.SetColor(PropBaseColor, color);
    }

    /// <summary>
    /// 透明其他骨骼：选中骨骼保持原始材质，其他骨骼半透明。
    /// 选中高亮由 HighlightPlus 处理。
    /// </summary>
    public void TransparentOtherBone(List<int> selectedBoneIds)
    {
        if (selectedBoneIds == null || selectedBoneIds.Count == 0)
        {
            Debug.LogWarning("没有选中的骨骼");
            return;
        }

        var items = _registry.GetItems(out int count);

        for (int i = 0; i < count; i++)
        {
            MeshRenderer meshRenderer = items[i].meshRenderer;
            if (meshRenderer == null) continue;

            int boneId = items[i].boneId;

            // 缓存原始 sharedMaterial（首次修改前）
            CacheOriginalMaterial(boneId, meshRenderer);

            bool isSelected = false;
            for (int j = 0; j < selectedBoneIds.Count; j++)
            {
                if (boneId == selectedBoneIds[j])
                {
                    isSelected = true;
                    break;
                }
            }

            if (isSelected)
            {
                // 选中的骨骼：保持原始材质不变（高亮由 HighlightPlus 处理）
            }
            else
            {
                // 其他骨骼：半透明（使用原始颜色降低 alpha）
                Material material = meshRenderer.material;
                SetTransparentMode(material);
                Color color = _originalMaterials.TryGetValue(boneId, out Material origMat)
                    ? origMat.GetColor(PropColor)
                    : material.GetColor(PropColor);
                color.a = 0.3f;
                SetMaterialColor(material, color);
            }
        }
    }

    /// <summary>
    /// 重置所有骨骼：恢复原始 sharedMaterial，确保与加载时完全一致。
    /// </summary>
    public void ResetBoneTransparency()
    {
        var items = _registry.GetItems(out int count);

        for (int i = 0; i < count; i++)
        {
            MeshRenderer meshRenderer = items[i].meshRenderer;
            if (meshRenderer == null) continue;

            int boneId = items[i].boneId;
            RestoreOriginalMaterial(boneId, meshRenderer);
        }
    }

    /// <summary>
    /// 将指定骨骼设为透明（使用原始颜色降低 alpha）
    /// </summary>
    public void TransparentBones(List<int> boneIds, float alpha)
    {
        if (boneIds == null || boneIds.Count == 0)
        {
            Debug.LogWarning("没有指定的骨骼");
            return;
        }

        for (int i = 0; i < boneIds.Count; i++)
        {
            if (_registry.TryGet(boneIds[i], out SkeletonInfo info))
            {
                if (info.meshRenderer == null) continue;

                // 缓存原始 sharedMaterial
                CacheOriginalMaterial(boneIds[i], info.meshRenderer);

                Material material = info.meshRenderer.material;
                SetTransparentMode(material);

                // 使用原始颜色，只降低 alpha
                Color color = _originalMaterials.TryGetValue(boneIds[i], out Material origMat)
                    ? origMat.GetColor(PropColor)
                    : material.GetColor(PropColor);
                color.a = alpha;
                SetMaterialColor(material, color);
            }
        }
    }

    /// <summary>
    /// 将指定骨骼恢复为不透明（恢复原始材质）
    /// </summary>
    public void SolidBones(List<int> boneIds)
    {
        if (boneIds == null || boneIds.Count == 0)
        {
            Debug.LogWarning("没有指定的骨骼");
            return;
        }

        for (int i = 0; i < boneIds.Count; i++)
        {
            if (_registry.TryGet(boneIds[i], out SkeletonInfo info))
            {
                if (info.meshRenderer == null) continue;

                RestoreOriginalMaterial(boneIds[i], info.meshRenderer);
            }
        }
    }

    /// <summary>
    /// 将非选中骨骼恢复为不透明（恢复原始材质）
    /// </summary>
    public void SolidOtherBones(List<int> selectedBoneIds)
    {
        var items = _registry.GetItems(out int count);

        for (int i = 0; i < count; i++)
        {
            MeshRenderer meshRenderer = items[i].meshRenderer;
            if (meshRenderer == null) continue;

            bool isSelected = false;
            int boneId = items[i].boneId;
            for (int j = 0; j < selectedBoneIds.Count; j++)
            {
                if (boneId == selectedBoneIds[j])
                {
                    isSelected = true;
                    break;
                }
            }

            if (!isSelected)
            {
                RestoreOriginalMaterial(boneId, meshRenderer);
            }
        }
    }
}
