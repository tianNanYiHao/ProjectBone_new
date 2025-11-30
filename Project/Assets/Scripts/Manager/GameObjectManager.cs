using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Newtonsoft.Json;

public class GameObjectManager:SingletonManager<GameObjectManager>, IGeneric
{
        public GameObject Body;
        public bool dragenable = false;
        public bool rotateenable = false;
        public List<SkeletonInfo> skeletonInfos;
        private bool bodyVisible = false;
        private Color NormalColor = new Color(0.7353569f, 0.7353569f, 0.7353569f, 1f);
        private Color SelectColor = Color.cyan;
        private int boneShowType =(int) BoneShowType.All;
        private int selectBoneType = (int)EnumPos.All;
        //初始坐标位置
        private Vector3 initpos = new Vector3(0, 0, 0);
        //初始大小
        private Vector3 initscale = new Vector3(1, 1, 1);
        //初始角度
        private Vector3 initangle = new Vector3(0, 0, 0);
        
        public bool BodyVisible
        {
                get
                {
                        return bodyVisible;
                }
                set
                {
                        if (!Body)
                        {
                                bodyVisible = false;
                                return;
                        }
                        
                        bodyVisible = value;
                        if (value)
                        {
                                ChangeBodyLayer(UnityLayer.Layer_Body);        
                        }
                        else
                        {
                                ChangeBodyLayer(UnityLayer.Layer_Default);
                        }
                        Body.SetActive(value);
                }
        }

        public int ShowType
        {
                get { return boneShowType; }
                set
                {
                        boneShowType = value;
                        ShowBoneByType(boneShowType);
                }
        }

        public int SelectBoneType
        {
                get { return selectBoneType; }
                set
                {
                        selectBoneType = value;
                        SelectBoneByPos(selectBoneType);
                }
        }

        public override void Initialize()
        {
                base.Initialize();
                skeletonInfos = new List<SkeletonInfo>();
            
        }
        
        public override void AllManagerInitialize()
        {
                base.AllManagerInitialize();
                LoadBody();
                BodyVisible = false;
        }
        
        void ChangeBodyLayer(int layer)
        {
                if (Body != null)
                {
                        
                        for (int i = 0; i < Body.transform.childCount; i++)
                        {
                                Body.transform.GetChild(i).gameObject.layer = layer;
                        }
                }
        }
        
        void LoadBody()
        {
                GameObject obj = ResManager.Instance.LoadRes<GameObject>("Model/jirou_nan");
                obj.transform.position = new Vector3(0, 0, 0);
                for (int i = 0; i < obj.transform.childCount; i++)
                {
                        GameObject boneobj = obj.transform.GetChild(i).gameObject;
                        string name = boneobj.name;
                        if (int.TryParse(name,out int id))
                        {
                                SkeletonInfo skeletonInfo = new SkeletonInfo();
                                Bone  bone = new Bone();
                                bone.Id = id;
                                skeletonInfo.boneId = id;
                                skeletonInfo.bone = bone;
                                skeletonInfo.boneGameObject = boneobj;
                                skeletonInfo.meshRenderer = boneobj.GetComponent<MeshRenderer>();
                                if (BoneMod.Instance.boneDic.ContainsKey(id))
                                {
                                        BoneMod.Instance.boneDic[id] = bone;
                                }
                                else
                                {
                                        BoneMod.Instance.boneDic.Add(id,bone);
                                }
                                skeletonInfos.Add(skeletonInfo);
                        }
                        obj.transform.GetChild(i).gameObject.layer = UnityLayer.Layer_Body;
                        if ( obj.transform.GetChild(i).gameObject.GetComponent<MeshCollider>() == null)
                        {
                                obj.transform.GetChild(i).gameObject.AddComponent<MeshCollider>();
                        }
                      
                }
                obj.transform.position = new Vector3(0, 0, 0.5f);
                Body = obj;
                Body.transform.localScale = new Vector3(1, 1, 1);
                initpos = Body.transform.position;
                initscale = Body.transform.localScale;
                initangle = Body.transform.eulerAngles;
        }
        
        public void SelectBone(int boneid)
        {
                for (int i = 0; i < skeletonInfos.Count; i++)
                {
                        SkeletonInfo skeletonInfo = skeletonInfos[i];
                        if (skeletonInfo.boneId == boneid)
                        {
                                skeletonInfo.meshRenderer.material.color = SelectColor;
                        }
                        else
                        {
                                skeletonInfo.meshRenderer.material.color = NormalColor;
                        }
                }
        }
        public void ResetBoneColor()
        {
                for (int i = 0; i < skeletonInfos.Count; i++)
                {
                        SkeletonInfo skeletonInfo = skeletonInfos[i];
                        skeletonInfo.meshRenderer.material.color = NormalColor;
                }
        }

        /// <summary>
        /// 透明其他骨骼（选中的骨骼保持不透明，其他骨骼变透明）
        /// </summary>
        public void TransparentOtherBone()
        {
                var selectedBoneIds = BoneMod.Instance.selectedBoneIds;
                if (selectedBoneIds.Count == 0)
                {
                        Debug.LogWarning("没有选中的骨骼");
                        return;
                }
                
                int currentBoneId = selectedBoneIds[selectedBoneIds.Count - 1];
                
                for (int i = 0; i < skeletonInfos.Count; i++)
                {
                        SkeletonInfo skeletonInfo = skeletonInfos[i];
                        Material material = skeletonInfo.meshRenderer.material;
                        
                        // 设置材质为透明模式
                        material.SetFloat("_Mode", 3); // Transparent mode
                        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        material.SetInt("_ZWrite", 0);
                        material.DisableKeyword("_ALPHATEST_ON");
                        material.EnableKeyword("_ALPHABLEND_ON");
                        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        material.renderQueue = 3000;
                        
                        if (skeletonInfo.boneId == currentBoneId)
                        {
                                // 选中的骨骼：不透明，高亮颜色
                                Color color = SelectColor;
                                color.a = 1f;
                                material.color = color;
                        }
                        else
                        {
                                // 其他骨骼：半透明
                                Color color = NormalColor;
                                color.a = 0.3f;
                                material.color = color;
                        }
                }
        }

        /// <summary>
        /// 重置所有骨骼透明度（恢复为不透明状态）
        /// </summary>
        public void ResetBoneTransparency()
        {
                for (int i = 0; i < skeletonInfos.Count; i++)
                {
                        SkeletonInfo skeletonInfo = skeletonInfos[i];
                        Material material = skeletonInfo.meshRenderer.material;
                        
                        // 设置材质为不透明模式
                        material.SetFloat("_Mode", 0); // Opaque mode
                        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                        material.SetInt("_ZWrite", 1);
                        material.DisableKeyword("_ALPHATEST_ON");
                        material.DisableKeyword("_ALPHABLEND_ON");
                        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        material.renderQueue = -1;
                        
                        Color color = NormalColor;
                        color.a = 1f;
                        material.color = color;
                }
        }
        private void OnDrag(Vector2 lastpos, Vector2 curpos)
        {
               //拖拽模型位置
               if (Body != null && dragenable)
               {
                       Vector2 diff = curpos - lastpos;
                       Vector3 pos = Body.transform.position;  
                       pos.x += diff.x * 0.001f;
                       pos.y += diff.y * 0.001f;
                       Body.transform.position = pos;
               }
                
                
                
        }

        private void OnZoom(float obj)
        {
               //修改相机视角
                 Camera.main.fieldOfView -= obj;
                
        }

        private void OnRotate(float angle)
        {
                Debug.Log("旋转角度："+angle);
                if (Body != null && rotateenable)
                {
                        //当前角度+旋转角度
                        float obj = Body.transform.rotation.eulerAngles.y - angle;
                        Body.transform.rotation = Quaternion.Euler(0, obj, 0);
                       
                }       
        }


        public override void Update(float time)
        {
                base.Update(time);
        }

        public override void OnApplicationQuit()
        {
                base.OnApplicationQuit();
        }

        public override void Dispose()
        {
                base.Dispose();
        }

        public override void OnApplicationFocus(bool hasFocus)
        {
                base.OnApplicationFocus(hasFocus);
        }

        public override void OnApplicationPause(bool pauseStatus)
        {
                base.OnApplicationPause(pauseStatus);
        }
        
        /// <summary>
        /// 隐藏选中的骨骼
        /// </summary>
        public void HideBone()
        {
                var selectedBoneIds = BoneMod.Instance.selectedBoneIds;
                for (int i = 0; i < skeletonInfos.Count; i++)
                {
                        SkeletonInfo skeletonInfo = skeletonInfos[i];
                        skeletonInfo.boneGameObject.SetActive(true);
                        for (int j = 0; j < selectedBoneIds.Count; j++)
                        {
                                if (skeletonInfo.boneId == selectedBoneIds[j])
                                {
                                        skeletonInfo.boneGameObject.SetActive(false);
                                }
                        }
                }
        }

        /// <summary>
        /// 显示选中的骨骼（恢复被隐藏的选中骨骼）
        /// </summary>
        public void ShowBone()
        {
                var selectedBoneIds = BoneMod.Instance.selectedBoneIds;
                for (int i = 0; i < skeletonInfos.Count; i++)
                {
                        SkeletonInfo skeletonInfo = skeletonInfos[i];
                        for (int j = 0; j < selectedBoneIds.Count; j++)
                        {
                                if (skeletonInfo.boneId == selectedBoneIds[j])
                                {
                                        skeletonInfo.boneGameObject.SetActive(true);
                                }
                        }
                }
        }

        /// <summary>
        /// 隐藏其他骨骼（只显示选中的）
        /// </summary>
        public void HideOtherBone()
        {
                var boneId = BoneMod.Instance.CurrentBoneId;
                for (int i = 0; i < skeletonInfos.Count; i++)
                {
                        SkeletonInfo skeletonInfo = skeletonInfos[i];
                        skeletonInfo.boneGameObject.SetActive(skeletonInfo.boneId == boneId);
                }
        }

        /// <summary>
        /// 显示其他骨骼（恢复被隐藏的其他骨骼）
        /// </summary>
        public void ShowOtherBone()
        {
                for (int i = 0; i < skeletonInfos.Count; i++)
                {
                        SkeletonInfo skeletonInfo = skeletonInfos[i];
                        skeletonInfo.boneGameObject.SetActive(true);
                }
        }

        /// <summary>
        /// 透明选中的骨骼（选中的骨骼变透明）
        /// </summary>
        public void TransparentBone()
        {
                var selectedBoneIds = BoneMod.Instance.selectedBoneIds;
                if (selectedBoneIds.Count == 0)
                {
                        Debug.LogWarning("没有选中的骨骼");
                        return;
                }

                for (int i = 0; i < skeletonInfos.Count; i++)
                {
                        SkeletonInfo skeletonInfo = skeletonInfos[i];
                        bool isSelected = false;
                        for (int j = 0; j < selectedBoneIds.Count; j++)
                        {
                                if (skeletonInfo.boneId == selectedBoneIds[j])
                                {
                                        isSelected = true;
                                        break;
                                }
                        }

                        if (isSelected)
                        {
                                SetMaterialTransparent(skeletonInfo.meshRenderer.material, SelectColor, 0.3f);
                        }
                }
        }

        /// <summary>
        /// 实体选中的骨骼（选中的骨骼恢复不透明）
        /// </summary>
        public void SolidBone()
        {
                var selectedBoneIds = BoneMod.Instance.selectedBoneIds;
                if (selectedBoneIds.Count == 0)
                {
                        Debug.LogWarning("没有选中的骨骼");
                        return;
                }

                for (int i = 0; i < skeletonInfos.Count; i++)
                {
                        SkeletonInfo skeletonInfo = skeletonInfos[i];
                        bool isSelected = false;
                        for (int j = 0; j < selectedBoneIds.Count; j++)
                        {
                                if (skeletonInfo.boneId == selectedBoneIds[j])
                                {
                                        isSelected = true;
                                        break;
                                }
                        }

                        if (isSelected)
                        {
                                SetMaterialOpaque(skeletonInfo.meshRenderer.material, SelectColor);
                        }
                }
        }

        /// <summary>
        /// 实体其他骨骼（其他骨骼恢复不透明，选中的保持当前状态）
        /// </summary>
        public void SolidOtherBone()
        {
                var selectedBoneIds = BoneMod.Instance.selectedBoneIds;

                for (int i = 0; i < skeletonInfos.Count; i++)
                {
                        SkeletonInfo skeletonInfo = skeletonInfos[i];
                        bool isSelected = false;
                        for (int j = 0; j < selectedBoneIds.Count; j++)
                        {
                                if (skeletonInfo.boneId == selectedBoneIds[j])
                                {
                                        isSelected = true;
                                        break;
                                }
                        }

                        if (!isSelected)
                        {
                                SetMaterialOpaque(skeletonInfo.meshRenderer.material, NormalColor);
                        }
                }
        }

        /// <summary>
        /// 设置材质为透明模式
        /// </summary>
        private void SetMaterialTransparent(Material material, Color baseColor, float alpha)
        {
                material.SetFloat("_Mode", 3); // Transparent mode
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 3000;

                Color color = baseColor;
                color.a = alpha;
                material.color = color;
        }

        /// <summary>
        /// 设置材质为不透明模式
        /// </summary>
        private void SetMaterialOpaque(Material material, Color baseColor)
        {
                material.SetFloat("_Mode", 0); // Opaque mode
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt("_ZWrite", 1);
                material.DisableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = -1;

                Color color = baseColor;
                color.a = 1f;
                material.color = color;
        }

        /// <summary>
        /// 复位模型变换（位置、角度、大小恢复初始值）
        /// </summary>
        public void ResetTransform()
        {
                if (!Body)
                {
                        return;
                }
                Body.transform.position = initpos;
                Body.transform.localScale = initscale;
                Body.transform.eulerAngles = initangle;
        }

        /// <summary>
        /// 完全重置（包括模型变换和选中状态）
        /// </summary>
        public void ResetAll()
        {
                // 重置模型变换
                ResetTransform();
                // 重置骨骼透明度
                ResetBoneTransparency();
                // 显示所有骨骼
                ShowBoneByType((int)BoneShowType.All);
                // 清除选中状态
                BoneMod.Instance.ClearSelection();
        }

      public void ShowBoneByType(int type)
      {
              for (int i = 0; i < skeletonInfos.Count; i++)
              {
                      SkeletonInfo skeletonInfo = skeletonInfos[i];
                      skeletonInfo.boneGameObject.SetActive(false);
                              
              }
             
              if (UtilHelper.IsContains(type,(int)BoneShowType.All) )
              {
                      for (int i = 0; i < skeletonInfos.Count; i++)
                      {
                              SkeletonInfo skeletonInfo = skeletonInfos[i];
                              skeletonInfo.boneGameObject.SetActive(true);
                              
                      }
              }
              else
              {
                      if (UtilHelper.IsContains(type,(int)BoneShowType.Bone) )
                      {
                              for (int i = 0; i < skeletonInfos.Count; i++)
                              {
                                      SkeletonInfo skeletonInfo = skeletonInfos[i];
                                      if (skeletonInfo.bone.Boneenum == EnumBone.Bone)
                                      {
                                              skeletonInfo.boneGameObject.SetActive(true);
                                      }
                                    
                                      
                              }
                      }
                      if (UtilHelper.IsContains(type,(int)BoneShowType.Muscle))
                      {
                              for (int i = 0; i < skeletonInfos.Count; i++)
                              {
                                      SkeletonInfo skeletonInfo = skeletonInfos[i];
                                      if (skeletonInfo.bone.Boneenum == EnumBone.Muscle)
                                      {
                                              skeletonInfo.boneGameObject.SetActive(true);
                                      }
                                    
                                      
                              }
                      }
                      
                      if (UtilHelper.IsContains(type,(int)BoneShowType.Fascia))
                      {
                              for (int i = 0; i < skeletonInfos.Count; i++)
                              {
                                      SkeletonInfo skeletonInfo = skeletonInfos[i];
                                      if (skeletonInfo.bone.Boneenum == EnumBone.Fascia)
                                      {
                                              skeletonInfo.boneGameObject.SetActive(true);
                                      }
                                    
                                      
                              }
                      }
              }
             
           
      }
      

      public void SelectBoneByPos(int pos)
      {
              for (int i = 0; i < skeletonInfos.Count; i++)
              {
                      SkeletonInfo skeletonInfo = skeletonInfos[i];
                      if (UtilHelper.IsContains(pos,skeletonInfo.bone.Pos)  )
                      {
                              skeletonInfo.boneGameObject.SetActive(true);
                              Debug.Log("骨骼所属位置:"+skeletonInfo.bone.Pos);
                      }
                      else
                      {
                              skeletonInfo.boneGameObject.SetActive(false);
                      }
              }
            
      }
      
      public SkeletonInfo GetSkeletonInfo(int boneid)
      {
              for (int i = 0; i < skeletonInfos.Count; i++)
              {
                      SkeletonInfo skeletonInfo = skeletonInfos[i];
                      if (skeletonInfo.boneId == boneid)
                      {
                              return skeletonInfo;
                      }
              }
              return null;
      }

      /// <summary>
      /// 重置位置
      /// </summary>
      public void ReSetPos()
      {
              if (!Body)
              {
                      return;
              }
              Body.transform.position = initpos;
      }
      
      /// <summary>
      /// 重置缩放
      /// </summary>
      public void ReSetScale()
      {
              if (!Body)
              {
                      return;
              }
              Body.transform.localScale = initscale;
      }
      
      /// <summary>
      /// 重置角度
      /// </summary>
      public void ResetAngle()
      {
              if (!Body)
              {
                      return;
              }
              Body.transform.eulerAngles = initangle;
      }
      
      /// <summary>
      /// 完全重置模型（位置、缩放、角度）
      /// </summary>
      public void ReSet()
      {
              ReSetPos();
              ReSetScale();
              ResetAngle();
      }

     /// <summary>
     /// 导出当前所有骨骼配置数据
     /// </summary>
     public List<BoneData> ExportBoneConfig()
     {
             List<BoneData> boneDataList = new List<BoneData>();
             
             for (int i = 0; i < skeletonInfos.Count; i++)
             {
                     SkeletonInfo skeletonInfo = skeletonInfos[i];
                     if (skeletonInfo.bone != null)
                     {
                             BoneData data = new BoneData
                             {
                                     id = skeletonInfo.bone.Id,
                                     type = (int)skeletonInfo.bone.Boneenum,
                                     position = skeletonInfo.bone.Pos,
                                     direction = skeletonInfo.bone.Direction
                             };
                             boneDataList.Add(data);
                     }
             }
             
             Debug.Log($"导出骨骼配置: 共{boneDataList.Count}个骨骼");
             return boneDataList;
     }
      
     /// <summary>
     /// 应用骨骼配置数据
     /// </summary>
     public void ApplyBoneConfig(List<BoneData> boneDataList)
     {
             if (boneDataList == null || boneDataList.Count == 0)
             {
                     Debug.LogWarning("骨骼配置数据为空");
                     return;
             }
             
             int appliedCount = 0;
             for (int i = 0; i < boneDataList.Count; i++)
             {
                     BoneData data = boneDataList[i];
                     if (BoneMod.Instance.boneDic.ContainsKey(data.id))
                     {
                             Bone bone = BoneMod.Instance.boneDic[data.id];
                             bone.Boneenum = (EnumBone)data.type;
                             bone.Pos = data.position;
                             bone.Direction = data.direction;
                             appliedCount++;
                     }
                     else
                     {
                             Debug.LogWarning($"未找到ID为{data.id}的骨骼");
                     }
             }
             
             Debug.Log($"应用骨骼配置: 成功应用{appliedCount}/{boneDataList.Count}个骨骼");
             
             // 刷新显示
             ShowBoneByType(boneShowType);
             SelectBoneByPos(selectBoneType);
     }
      
      /// <summary>
      /// 从JSON字符串加载骨骼配置
      /// </summary>
      public void LoadBoneConfigFromJson(string jsonString)
      {
              if (string.IsNullOrEmpty(jsonString))
              {
                      Debug.LogWarning("JSON字符串为空");
                      return;
              }
              
              List<BoneData> boneDataList = JsonConvert.DeserializeObject<List<BoneData>>(jsonString);
              ApplyBoneConfig(boneDataList);
      }
      
      /// <summary>
      /// 将骨骼配置导出为JSON字符串
      /// </summary>
      public string ExportBoneConfigToJson()
      {
              List<BoneData> boneDataList = ExportBoneConfig();
              return JsonConvert.SerializeObject(boneDataList, Formatting.Indented);
      }
}
