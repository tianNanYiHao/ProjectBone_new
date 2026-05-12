using UnityEngine;

/// <summary>
/// 骨骼信息结构体。从 class 改为 struct 以消除每个骨骼实例的独立堆分配。
/// 存储在 SkeletonRegistry 的预分配数组中，实现连续内存布局。
/// </summary>
/// <remarks>
/// struct 中的引用类型字段（bone、boneGameObject、meshRenderer）不会导致额外堆分配，
/// 它们只是存储指向已有堆对象的引用指针。
/// struct 本身存储在数组中（连续内存），避免了每个 SkeletonInfo 的独立堆分配和 GC 压力。
/// </remarks>
public struct SkeletonInfo
{
    public int boneId;
    public Bone bone;
    public GameObject boneGameObject;
    public MeshRenderer meshRenderer;
}
