using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 零GC骨骼注册表。使用 Dictionary + 预分配数组实现 O(1) 按 boneId 查找和零GC顺序遍历。
/// 初始化完成后不再产生任何新的堆内存分配。
/// </summary>
public class SkeletonRegistry
{
    /// <summary>
    /// boneId → 数组索引的映射，提供 O(1) 查找
    /// </summary>
    private Dictionary<int, int> _boneIdToIndex;

    /// <summary>
    /// 预分配固定大小数组，存储所有骨骼信息
    /// </summary>
    private SkeletonInfo[] _items;

    /// <summary>
    /// 当前有效元素数量
    /// </summary>
    private int _count;

    /// <summary>
    /// 有效骨骼数量
    /// </summary>
    public int Count => _count;

    /// <summary>
    /// 初始化注册表，预分配指定容量。
    /// 所有容器在此阶段一次性分配，运行时不触发扩容。
    /// </summary>
    public void Initialize(int capacity)
    {
        _boneIdToIndex = new Dictionary<int, int>(capacity);
        _items = new SkeletonInfo[capacity];
        _count = 0;
    }

    /// <summary>
    /// 注册一个骨骼信息（仅在初始化阶段调用）。
    /// </summary>
    public void Register(int boneId, Bone bone, GameObject boneGameObject, MeshRenderer meshRenderer)
    {
        if (_items == null)
        {
            Debug.LogError("SkeletonRegistry: 未初始化，请先调用 Initialize");
            return;
        }

        if (_count >= _items.Length)
        {
            Debug.LogError("SkeletonRegistry: 容量已满，无法注册更多骨骼");
            return;
        }

        int index = _count;
        _items[index].boneId = boneId;
        _items[index].bone = bone;
        _items[index].boneGameObject = boneGameObject;
        _items[index].meshRenderer = meshRenderer;

        _boneIdToIndex[boneId] = index;
        _count++;
    }

    /// <summary>
    /// 按 boneId 查找，O(1) 复杂度。找不到返回 false。
    /// </summary>
    public bool TryGet(int boneId, out SkeletonInfo info)
    {
        if (_boneIdToIndex != null && _boneIdToIndex.TryGetValue(boneId, out int index))
        {
            info = _items[index];
            return true;
        }

        info = default;
        return false;
    }

    /// <summary>
    /// 获取预分配数组和有效元素数量，用于零GC遍历。
    /// 调用方使用 for(int i = 0; i &lt; count; i++) 遍历。
    /// </summary>
    public SkeletonInfo[] GetItems(out int count)
    {
        count = _count;
        return _items;
    }

    /// <summary>
    /// 清空注册表，重置计数和字典，但不释放已分配的数组内存。
    /// </summary>
    public void Clear()
    {
        if (_boneIdToIndex != null)
        {
            _boneIdToIndex.Clear();
        }

        // 清空数组中的引用，帮助 GC 回收引用的对象
        for (int i = 0; i < _count; i++)
        {
            _items[i] = default;
        }

        _count = 0;
    }
}
