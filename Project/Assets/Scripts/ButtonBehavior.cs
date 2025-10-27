using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

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

    
}
