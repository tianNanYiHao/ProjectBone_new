using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// 骨骼配置序列化服务。负责骨骼配置数据的导出、导入和 JSON 序列化/反序列化。
/// 使用预分配 BoneData 数组减少热路径上的 GC 分配。
/// </summary>
public class BoneConfigService
{
    private readonly SkeletonRegistry _registry;

    /// <summary>
    /// 预分配导出缓冲区，避免每次导出创建新数组
    /// </summary>
    private BoneData[] _exportBuffer;

    public BoneConfigService(SkeletonRegistry registry)
    {
        _registry = registry;
    }

    /// <summary>
    /// 初始化预分配缓冲区（在骨骼注册完成后调用）
    /// </summary>
    public void InitializeBuffer(int capacity)
    {
        _exportBuffer = new BoneData[capacity];
    }

    /// <summary>
    /// 导出当前所有骨骼配置数据（使用预分配数组，零GC）。
    /// 返回有效数据的数量，通过 out 参数返回缓冲区引用。
    /// </summary>
    public int ExportBoneConfig(out BoneData[] buffer)
    {
        var items = _registry.GetItems(out int count);
        int validCount = 0;

        // 确保缓冲区容量足够
        if (_exportBuffer == null || _exportBuffer.Length < count)
        {
            _exportBuffer = new BoneData[count];
        }

        for (int i = 0; i < count; i++)
        {
            Bone bone = items[i].bone;
            if (bone != null)
            {
                _exportBuffer[validCount] = new BoneData
                {
                    id = bone.Id,
                    type = (int)bone.Boneenum,
                    position = bone.Pos,
                    direction = bone.Direction
                };
                validCount++;
            }
        }

        buffer = _exportBuffer;
        return validCount;
    }

    /// <summary>
    /// 应用骨骼配置数据到 BoneMod.Instance.boneDic。
    /// </summary>
    public void ApplyBoneConfig(BoneData[] boneDataArray, int count)
    {
        if (boneDataArray == null)
        {
            Debug.LogWarning("BoneConfigService: boneDataArray 为 null");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            BoneData data = boneDataArray[i];
            if (BoneMod.Instance.boneDic.ContainsKey(data.id))
            {
                Bone bone = BoneMod.Instance.boneDic[data.id];
                bone.Boneenum = (EnumBone)data.type;
                bone.Pos = data.position;
                bone.Direction = data.direction;
            }
        }
    }

    /// <summary>
    /// 从 JSON 字符串加载骨骼配置并应用。
    /// 注意：此方法为非热路径，JSON 反序列化会产生 GC 分配。
    /// </summary>
    public void LoadBoneConfigFromJson(string jsonString)
    {
        if (string.IsNullOrEmpty(jsonString))
        {
            Debug.LogWarning("BoneConfigService: JSON 字符串为空");
            return;
        }

        try
        {
            BoneData[] boneDataArray = JsonConvert.DeserializeObject<BoneData[]>(jsonString);
            if (boneDataArray != null)
            {
                ApplyBoneConfig(boneDataArray, boneDataArray.Length);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"BoneConfigService: JSON 反序列化失败 — {ex.Message}");
        }
    }

    /// <summary>
    /// 将骨骼配置导出为 JSON 字符串。
    /// 注意：此方法为非热路径，JSON 序列化会产生 GC 分配。
    /// </summary>
    public string ExportBoneConfigToJson()
    {
        int count = ExportBoneConfig(out BoneData[] buffer);

        // 创建精确大小的数组用于 JSON 序列化
        BoneData[] result = new BoneData[count];
        System.Array.Copy(buffer, result, count);

        return JsonConvert.SerializeObject(result);
    }

    // --- 向后兼容包装方法 ---

    /// <summary>
    /// 导出骨骼配置为 List（向后兼容，供期望 List&lt;BoneData&gt; 的调用方使用）。
    /// 注意：此方法会产生 GC 分配（创建 List），非热路径使用。
    /// </summary>
    public List<BoneData> ExportBoneConfigAsList()
    {
        int count = ExportBoneConfig(out BoneData[] buffer);
        List<BoneData> list = new List<BoneData>(count);
        for (int i = 0; i < count; i++)
        {
            list.Add(buffer[i]);
        }
        return list;
    }

    /// <summary>
    /// 从 List 应用骨骼配置（向后兼容，供传入 List&lt;BoneData&gt; 的调用方使用）。
    /// </summary>
    public void ApplyBoneConfigFromList(List<BoneData> boneDataList)
    {
        if (boneDataList == null)
        {
            Debug.LogWarning("BoneConfigService: boneDataList 为 null");
            return;
        }

        for (int i = 0; i < boneDataList.Count; i++)
        {
            BoneData data = boneDataList[i];
            if (BoneMod.Instance.boneDic.ContainsKey(data.id))
            {
                Bone bone = BoneMod.Instance.boneDic[data.id];
                bone.Boneenum = (EnumBone)data.type;
                bone.Pos = data.position;
                bone.Direction = data.direction;
            }
        }
    }
}
