using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using Newtonsoft.Json;

public class NativeAPI
{
#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    public static extern void sendMessageToMobileApp(string message);
#endif
}


public class ButtonBehaviorCustomData // 定义可序列化数据结构
{
    public string msg;
    public int code;
}

/// <summary>
/// 骨骼数据配置类 - 用于序列化和反序列化
/// </summary>
public class BoneData
{
    public int id;              // 骨骼ID
    public int type;            // 骨骼类型 (EnumBone: Bone=1, Muscle=2, Fascia=4)
    public int position;        // 骨骼位置 (EnumPos: 上肢、肩背、下肢等)
}

public class ButtonBehavior : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    public void ButtonPressed(string jsonString)
    {
        Debug.Log("---- 通信脚本触发 ----" + jsonString);
        if (Application.platform == RuntimePlatform.Android)
        {
            using (AndroidJavaClass jc = new AndroidJavaClass("com.azesmwayreactnativeunity.ReactNativeUnityViewManager"))
            {
                jc.CallStatic("sendMessageToMobileApp", jsonString);
            }
            
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
#if UNITY_IOS && !UNITY_EDITOR
            NativeAPI.sendMessageToMobileApp(jsonString);
#endif
        }
    }

    /// <summary>
    /// 显示模型
    /// </summary>
    public void ShowModel()
    {
        Debug.Log("---- 显示模型 ----");
        GameObjectManager.Instance.BodyVisible = true;
    }

    /// <summary>
    /// 隐藏模型
    /// </summary>
    public void HideModel()
    {
        Debug.Log("---- 隐藏模型 ----");
        GameObjectManager.Instance.BodyVisible = false;
    }

    /// <summary>
    /// 序列化单个骨骼数据为JSON字符串
    /// </summary>
    public string SerializeBoneData(BoneData data)
    {
        return JsonConvert.SerializeObject(data);
    }

    /// <summary>
    /// 序列化骨骼数据列表为JSON字符串
    /// </summary>
    public string SerializeBoneDataList(List<BoneData> dataList)
    {
        return JsonConvert.SerializeObject(dataList);
    }

    /// <summary>
    /// 反序列化JSON字符串为单个骨骼数据
    /// </summary>
    public BoneData DeserializeBoneData(string jsonString)
    {
        return JsonConvert.DeserializeObject<BoneData>(jsonString);
    }

    /// <summary>
    /// 反序列化JSON字符串为骨骼数据列表
    /// </summary>
    public List<BoneData> DeserializeBoneDataList(string jsonString)
    {
        return JsonConvert.DeserializeObject<List<BoneData>>(jsonString);
    }

    /// <summary>
    /// 序列化骨骼数据并发送到移动端
    /// </summary>
    public void SendBoneData(BoneData data)
    {
        string jsonString = SerializeBoneData(data);
        ButtonPressed(jsonString);
    }

    /// <summary>
    /// 序列化骨骼数据列表并发送到移动端
    /// </summary>
    public void SendBoneDataList(List<BoneData> dataList)
    {
        string jsonString = SerializeBoneDataList(dataList);
        ButtonPressed(jsonString);
    }

    /// <summary>
    /// 接收并应用骨骼配置数据
    /// </summary>
    public void ReceiveBoneConfig(string jsonString)
    {
        Debug.Log("---- 接收骨骼配置数据 ----" + jsonString);
        List<BoneData> boneDataList = DeserializeBoneDataList(jsonString);
        if (boneDataList != null && boneDataList.Count > 0)
        {
            GameObjectManager.Instance.ApplyBoneConfig(boneDataList);
        }
    }

    /// <summary>
    /// 导出当前所有骨骼配置
    /// </summary>
    public void ExportBoneConfig()
    {
        Debug.Log("---- 导出骨骼配置 ----");
        List<BoneData> boneDataList = GameObjectManager.Instance.ExportBoneConfig();
        string jsonString = SerializeBoneDataList(boneDataList);
        ButtonPressed(jsonString);
    }

    /// <summary>
    /// 根据骨骼类型显示模型
    /// </summary>
    /// <param name="type">骨骼类型 (EnumBone: Bone=1, Muscle=2, Fascia=4)</param>
    public void ShowModelByType(int type)
    {
        Debug.Log($"---- 根据类型显示模型 ---- type: {type}");
        GameObjectManager.Instance.ShowBoneByType(type);
    }

    /// <summary>
    /// 根据骨骼位置显示模型
    /// </summary>
    /// <param name="position">骨骼位置 (EnumPos: 上肢、肩背、下肢等)</param>
    public void ShowModelByPosition(int position)
    {
        Debug.Log($"---- 根据位置显示模型 ---- position: {position}");
        GameObjectManager.Instance.SelectBoneByPos(position);
    }
    
}
