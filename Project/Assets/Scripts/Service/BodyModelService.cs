using UnityEngine;

/// <summary>
/// 模型加载与生命周期服务。负责人体模型预制体加载、MeshCollider 初始化、
/// 骨骼注册、Layer 设置、变换重置和可见性控制。
/// </summary>
public class BodyModelService
{
    private readonly SkeletonRegistry _registry;
    private GameObject _body;
    private Vector3 _initPos;
    private Vector3 _initScale;
    private Vector3 _initAngle;

    /// <summary>
    /// 当前加载的人体模型 GameObject
    /// </summary>
    public GameObject Body => _body;

    public BodyModelService(SkeletonRegistry registry)
    {
        _registry = registry;
    }

    /// <summary>
    /// 完整的模型加载流程：加载预制体 → 初始化碰撞体 → 注册骨骼 → 设置Layer → 记录初始变换
    /// </summary>
    public void LoadBody()
    {
        Debug.Log("[BodyModel] 开始加载模型...");
        GameObject obj = LoadModelPrefab();
        if (obj == null)
        {
            Debug.LogError("[BodyModel] 模型预制体加载失败! Resources.Load(\"Model/jirou_nan\") 返回 null");
            return;
        }

        Debug.Log($"[BodyModel] 模型加载成功, childCount: {obj.transform.childCount}");

        obj.transform.position = new Vector3(0, 0, 0);

        InitializeBoneColliders(obj);
        RegisterBones(obj);
        SetBodyLayer(obj, UnityLayer.Layer_Body);

        obj.transform.position = new Vector3(0, 0, 0.5f);
        _body = obj;
        _body.transform.localScale = new Vector3(10, 10, 10);

        _initPos = _body.transform.position;
        _initScale = _body.transform.localScale;
        _initAngle = _body.transform.eulerAngles;
        
        Debug.Log($"[BodyModel] 模型初始化完成, 注册骨骼数: {_registry.Count}, Body active: {_body.activeSelf}");
    }

    /// <summary>
    /// 从 Resources 加载模型预制体并实例化
    /// </summary>
    public GameObject LoadModelPrefab()
    {
        return ResManager.Instance.LoadRes<GameObject>("Model/jirou_nan");
    }

    /// <summary>
    /// 为每个子对象添加 MeshCollider（如果不存在）
    /// </summary>
    public void InitializeBoneColliders(GameObject root)
    {
        for (int i = 0; i < root.transform.childCount; i++)
        {
            GameObject child = root.transform.GetChild(i).gameObject;
            if (child.GetComponent<MeshCollider>() == null)
            {
                child.AddComponent<MeshCollider>();
            }
        }
    }

    /// <summary>
    /// 解析子对象名称，创建 SkeletonInfo 并注册到 SkeletonRegistry 和 BoneMod.Instance.boneDic。
    /// 先统计有效骨骼数量，再初始化 SkeletonRegistry 容量，最后逐个注册。
    /// </summary>
    public void RegisterBones(GameObject root)
    {
        // 第一遍：统计有效骨骼数量（名称可解析为 int 的子对象）
        int validCount = 0;
        for (int i = 0; i < root.transform.childCount; i++)
        {
            if (int.TryParse(root.transform.GetChild(i).gameObject.name, out _))
            {
                validCount++;
            }
        }

        _registry.Initialize(validCount);

        // 第二遍：注册骨骼
        for (int i = 0; i < root.transform.childCount; i++)
        {
            GameObject boneObj = root.transform.GetChild(i).gameObject;
            string name = boneObj.name;

            if (int.TryParse(name, out int id))
            {
                Bone bone = new Bone();
                bone.Id = id;

                MeshRenderer meshRenderer = boneObj.GetComponent<MeshRenderer>();
                _registry.Register(id, bone, boneObj, meshRenderer);

                if (BoneMod.Instance.boneDic.ContainsKey(id))
                {
                    BoneMod.Instance.boneDic[id] = bone;
                }
                else
                {
                    BoneMod.Instance.boneDic.Add(id, bone);
                }
            }
        }
    }

    /// <summary>
    /// 设置所有子对象的 Layer
    /// </summary>
    public void SetBodyLayer(GameObject root, int layer)
    {
        for (int i = 0; i < root.transform.childCount; i++)
        {
            root.transform.GetChild(i).gameObject.layer = layer;
        }
    }

    /// <summary>
    /// 重置模型位置到初始值
    /// </summary>
    public void ResetPosition()
    {
        if (_body == null) return;
        _body.transform.position = _initPos;
    }

    /// <summary>
    /// 重置模型缩放到初始值
    /// </summary>
    public void ResetScale()
    {
        if (_body == null) return;
        _body.transform.localScale = _initScale;
    }

    /// <summary>
    /// 重置模型旋转到初始值
    /// </summary>
    public void ResetRotation()
    {
        if (_body == null) return;
        _body.transform.eulerAngles = _initAngle;
    }

    /// <summary>
    /// 完全重置变换（位置 + 缩放 + 旋转）
    /// </summary>
    public void ResetTransform()
    {
        ResetPosition();
        ResetScale();
        ResetRotation();
    }

    /// <summary>
    /// 设置 Body 的可见性。
    /// visible=true 时设置 Layer 为 Layer_Body 并激活；
    /// visible=false 时设置 Layer 为 Layer_Default 并停用。
    /// </summary>
    public void SetBodyVisible(bool visible)
    {
        if (_body == null) return;

        if (visible)
        {
            SetBodyLayer(_body, UnityLayer.Layer_Body);
        }
        else
        {
            SetBodyLayer(_body, UnityLayer.Layer_Default);
        }

        _body.SetActive(visible);
    }
}
