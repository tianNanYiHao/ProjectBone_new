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


public class ButtonBehaviorCustomData // 定义可序列化数据结构 - 通讯协议外层封装
{
    public string msg;      // 实际业务数据的JSON字符串
    public int code;        // 消息类型代码
}

/// <summary>
/// 骨骼数据配置类 - 用于序列化和反序列化
/// </summary>
public class BoneData
{
    public int id;              // 骨骼ID
    public int type;            // 骨骼类型 (EnumBone: Bone=1, Muscle=2, Fascia=4)
    public int position;        // 骨骼位置 (EnumPos: 上肢、肩背、下肢等)
    public int direction;       // 骨骼方向 (EnumDirection: None=0, Left=1, Right=2, Other=4)
}

/// <summary>
/// 消息类型代码定义
/// </summary>
public static class MessageCode
{
    public const int ShowModel = 1;             // 显示模型
    public const int HideModel = 2;             // 隐藏模型
    public const int ReceiveBoneConfig = 3;     // 接收骨骼配置
    public const int ExportBoneConfig = 4;      // 导出骨骼配置
    public const int ShowByType = 5;            // 根据类型显示
    public const int ShowByPosition = 6;        // 根据位置显示
    public const int SendBoneData = 7;          // 发送单个骨骼数据
    public const int SendBoneDataList = 8;      // 发送骨骼数据列表
    public const int BoneSelected = 9;          // 骨骼被选中
    public const int HideBone = 10;             // 隐藏选中的骨骼
    public const int HideOtherBone = 11;        // 隐藏其他骨骼（只显示选中的）
    public const int ShowAllBone = 12;          // 显示所有骨骼
    public const int ResetBoneColor = 13;       // 重置骨骼颜色
    public const int TransparentOtherBone = 14; // 透明其他骨骼（只有选中的不透明）
    public const int ResetBoneTransparency = 15; // 重置骨骼透明度
}

public class ButtonBehavior : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    /// <summary>
    /// 发送消息到移动端（底层通讯方法）
    /// </summary>
    private void SendMessageToMobile(string jsonString)
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
    /// 封装并发送消息（外层包装 ButtonBehaviorCustomData）
    /// </summary>
    private void SendWrappedMessage(int code, string msg)
    {
        ButtonBehaviorCustomData customData = new ButtonBehaviorCustomData
        {
            code = code,
            msg = msg
        };
        string jsonString = JsonConvert.SerializeObject(customData);
        SendMessageToMobile(jsonString);
    }

    /// <summary>
    /// 统一消息接收入口（从移动端接收）
    /// </summary>
    public void ReceiveMessage(string jsonString)
    {
        Debug.Log("---- 接收消息 ----" + jsonString);
        
        try
        {
            // 第一层反序列化：解析 ButtonBehaviorCustomData
            ButtonBehaviorCustomData customData = JsonConvert.DeserializeObject<ButtonBehaviorCustomData>(jsonString);
            
            if (customData == null)
            {
                Debug.LogError("反序列化 ButtonBehaviorCustomData 失败");
                return;
            }

            // 根据 code 分发消息，msg 为具体业务数据
            ProcessMessage(customData.code, customData.msg);
        }
        catch (Exception e)
        {
            Debug.LogError($"处理消息异常: {e.Message}");
        }
    }

    /// <summary>
    /// 处理具体业务消息
    /// </summary>
    private void ProcessMessage(int code, string msg)
    {
        switch (code)
        {
            case MessageCode.ShowModel:
                ShowModel();
                break;
            
            case MessageCode.HideModel:
                HideModel();
                break;
            
            case MessageCode.ReceiveBoneConfig:
                ReceiveBoneConfigInternal(msg);
                break;
            
            case MessageCode.ExportBoneConfig:
                ExportBoneConfig();
                break;
            
            case MessageCode.ShowByType:
                if (int.TryParse(msg, out int type))
                {
                    ShowModelByType(type);
                }
                break;
            
            case MessageCode.ShowByPosition:
                if (int.TryParse(msg, out int position))
                {
                    ShowModelByPosition(position);
                }
                break;
            
            case MessageCode.HideBone:
                HideBone();
                break;
            
            case MessageCode.HideOtherBone:
                HideOtherBone();
                break;
            
            case MessageCode.ShowAllBone:
                ShowAllBone();
                break;
            
            case MessageCode.ResetBoneColor:
                ResetBoneColor();
                break;
            
            case MessageCode.TransparentOtherBone:
                TransparentOtherBone();
                break;
            
            case MessageCode.ResetBoneTransparency:
                ResetBoneTransparency();
                break;
            
            default:
                Debug.LogWarning($"未知的消息类型代码: {code}");
                break;
        }
    }

    /// <summary>
    /// 兼容旧的 ButtonPressed 方法（保持向后兼容）
    /// </summary>
    public void ButtonPressed(string jsonString)
    {
        SendMessageToMobile(jsonString);
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
        string msg = SerializeBoneData(data);
        SendWrappedMessage(MessageCode.SendBoneData, msg);
    }

    /// <summary>
    /// 序列化骨骼数据列表并发送到移动端
    /// </summary>
    public void SendBoneDataList(List<BoneData> dataList)
    {
        string msg = SerializeBoneDataList(dataList);
        SendWrappedMessage(MessageCode.SendBoneDataList, msg);
    }

    /// <summary>
    /// 接收并应用骨骼配置数据（公开接口，兼容旧调用）
    /// </summary>
    public void ReceiveBoneConfig(string jsonString)
    {
        Debug.Log("---- 接收骨骼配置数据（兼容方法） ----" + jsonString);
        ReceiveBoneConfigInternal(jsonString);
    }

    /// <summary>
    /// 内部处理：接收并应用骨骼配置数据
    /// </summary>
    private void ReceiveBoneConfigInternal(string msg)
    {
        Debug.Log("---- 处理骨骼配置数据 ----" + msg);
        try
        {
            // 第二层反序列化：从 msg 中解析具体的骨骼数据
            List<BoneData> boneDataList = DeserializeBoneDataList(msg);
            if (boneDataList != null && boneDataList.Count > 0)
            {
                GameObjectManager.Instance.ApplyBoneConfig(boneDataList);
            }
            else
            {
                Debug.LogWarning("骨骼配置数据为空");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"处理骨骼配置数据异常: {e.Message}");
        }
    }

    /// <summary>
    /// 导出当前所有骨骼配置
    /// </summary>
    public void ExportBoneConfig()
    {
        Debug.Log("---- 导出骨骼配置 ----");
        List<BoneData> boneDataList = GameObjectManager.Instance.ExportBoneConfig();
        string msg = SerializeBoneDataList(boneDataList);
        SendWrappedMessage(MessageCode.ExportBoneConfig, msg);
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

    /// <summary>
    /// 通知移动端骨骼被选中
    /// </summary>
    /// <param name="boneId">被选中的骨骼ID</param>
    public void NotifyBoneSelected(int boneId)
    {
        Debug.Log($"---- 通知移动端骨骼被选中 ---- boneId: {boneId}");
        SendWrappedMessage(MessageCode.BoneSelected, boneId.ToString());
    }

    /// <summary>
    /// 隐藏选中的骨骼
    /// </summary>
    public void HideBone()
    {
        Debug.Log("---- 隐藏选中的骨骼 ----");
        GameObjectManager.Instance.HideBone();
    }

    /// <summary>
    /// 隐藏其他骨骼（只显示选中的）
    /// </summary>
    public void HideOtherBone()
    {
        Debug.Log("---- 隐藏其他骨骼 ----");
        GameObjectManager.Instance.HideOtherBone();
    }

    /// <summary>
    /// 显示所有骨骼
    /// </summary>
    public void ShowAllBone()
    {
        Debug.Log("---- 显示所有骨骼 ----");
        GameObjectManager.Instance.ShowBoneByType((int)BoneShowType.All);
    }

    /// <summary>
    /// 重置骨骼颜色
    /// </summary>
    public void ResetBoneColor()
    {
        Debug.Log("---- 重置骨骼颜色 ----");
        GameObjectManager.Instance.ResetBoneColor();
    }

    /// <summary>
    /// 透明其他骨骼（只有选中的不透明）
    /// </summary>
    public void TransparentOtherBone()
    {
        Debug.Log("---- 透明其他骨骼 ----");
        GameObjectManager.Instance.TransparentOtherBone();
    }

    /// <summary>
    /// 重置骨骼透明度（恢复所有骨骼为不透明）
    /// </summary>
    public void ResetBoneTransparency()
    {
        Debug.Log("---- 重置骨骼透明度 ----");
        GameObjectManager.Instance.ResetBoneTransparency();
    }
    
}
