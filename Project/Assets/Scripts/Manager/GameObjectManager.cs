using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameObjectManager : SingletonManager<GameObjectManager>, IGeneric
{
    private SkeletonRegistry _registry;
    private BodyModelService _bodyModelService;
    private BoneVisibilityService _visibilityService;
    private BoneSelectionService _selectionService;
    private BoneMaterialService _materialService;
    private BoneConfigService _configService;

    private bool _bodyVisible;
    private int _boneShowType = (int)BoneShowType.All;
    private int _selectBoneType = (int)EnumPos.All;

    public GameObject Body => _bodyModelService.Body;

    public bool BodyVisible
    {
        get { return _bodyVisible; }
        set
        {
            if (_bodyModelService.Body == null)
            {
                _bodyVisible = false;
                return;
            }
            _bodyVisible = value;
            _bodyModelService.SetBodyVisible(value);
        }
    }

    public int ShowType
    {
        get { return _boneShowType; }
        set
        {
            _boneShowType = value;
            _visibilityService.ShowBoneByType(_boneShowType);
        }
    }

    public int SelectBoneType
    {
        get { return _selectBoneType; }
        set
        {
            _selectBoneType = value;
            _visibilityService.SelectBoneByPos(_selectBoneType);
        }
    }

    public override void Initialize()
    {
        base.Initialize();
        _registry = new SkeletonRegistry();
        _materialService = new BoneMaterialService(_registry);
        _bodyModelService = new BodyModelService(_registry);
        _visibilityService = new BoneVisibilityService(_registry);
        _selectionService = new BoneSelectionService(_registry, _materialService);
        _configService = new BoneConfigService(_registry);
    }

    public override void AllManagerInitialize()
    {
        base.AllManagerInitialize();
        _bodyModelService.LoadBody();
        _configService.InitializeBuffer(_registry.Count);
        BodyVisible = false;
    }

    public void SelectBone(int boneid)
    {
        _selectionService.SelectBone(boneid);
    }

    public void ResetBoneColor()
    {
        _selectionService.ResetBoneColor();
    }

    public void TransparentOtherBone()
    {
        _materialService.TransparentOtherBone(BoneMod.Instance.selectedBoneIds);
    }

    public void ResetBoneTransparency()
    {
        _materialService.ResetBoneTransparency();
    }

    public void HideBone()
    {
        _visibilityService.HideBones(BoneMod.Instance.selectedBoneIds);
    }

    public void ShowBone()
    {
        _visibilityService.ShowBones(BoneMod.Instance.selectedBoneIds);
    }

    public void HideOtherBone()
    {
        _visibilityService.ShowOnlyBone(BoneMod.Instance.CurrentBoneId);
    }

    public void ShowOtherBone()
    {
        _visibilityService.ShowAllBones();
    }

    public void TransparentBone()
    {
        _materialService.TransparentBones(BoneMod.Instance.selectedBoneIds, 0.3f);
    }

    public void SolidBone()
    {
        _materialService.SolidBones(BoneMod.Instance.selectedBoneIds);
    }

    public void SolidOtherBone()
    {
        _materialService.SolidOtherBones(BoneMod.Instance.selectedBoneIds);
    }

    public void ShowBoneByType(int type)
    {
        _visibilityService.ShowBoneByType(type);
    }

    public void SelectBoneByPos(int pos)
    {
        _visibilityService.SelectBoneByPos(pos);
    }

    public SkeletonInfo? GetSkeletonInfo(int boneid)
    {
        if (_registry.TryGet(boneid, out SkeletonInfo info))
            return info;
        return null;
    }

    public void ResetTransform()
    {
        _bodyModelService.ResetTransform();
    }

    public void ResetAll()
    {
        _bodyModelService.ResetTransform();
        _materialService.ResetBoneTransparency();
        _visibilityService.ShowBoneByType((int)BoneShowType.All);
        BoneMod.Instance.ClearSelection();
    }

    public void ReSetPos()
    {
        _bodyModelService.ResetPosition();
    }

    public void ReSetScale()
    {
        _bodyModelService.ResetScale();
    }

    public void ResetAngle()
    {
        _bodyModelService.ResetRotation();
    }

    /// <summary>
    /// 完全重置模型（位置、缩放、角度）— 向后兼容
    /// </summary>
    public void ReSet()
    {
        _bodyModelService.ResetTransform();
    }

    public List<BoneData> ExportBoneConfig()
    {
        return _configService.ExportBoneConfigAsList();
    }

    public void ApplyBoneConfig(List<BoneData> boneDataList)
    {
        _configService.ApplyBoneConfigFromList(boneDataList);
        ShowBoneByType(_boneShowType);
        SelectBoneByPos(_selectBoneType);
    }

    public void LoadBoneConfigFromJson(string jsonString)
    {
        _configService.LoadBoneConfigFromJson(jsonString);
        ShowBoneByType(_boneShowType);
        SelectBoneByPos(_selectBoneType);
    }

    public string ExportBoneConfigToJson()
    {
        return _configService.ExportBoneConfigToJson();
    }
}
