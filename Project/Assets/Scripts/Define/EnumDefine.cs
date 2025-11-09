
using System;public enum EnumGender
{
    Male = 0,
    Female = 1,
}



[Flags]
public enum EnumBone
{
    //骨骼
    Bone = 1<<0,
    //肌肉
    Muscle = 1<<1,
    //筋膜
    Fascia = 1<<2,
    //所有
    All = Bone|Muscle|Fascia,
}

[Flags]
public enum EnumPos
{

    //开始定义枚举
    //无
    None = 0,
    //上肢
    UpperLimbs = 1 << 1,
    //肩背
    ShoulderBack = 1<<2,
    //下肢
    LowerLimbs = 1 << 3,
    //盆骨
    Pelvis = 1 << 4,
    //头颈
    HeadAndNeck = 1 << 5,
    //胸腹
    ChestAndAbdomen = 1 << 6,
    //脊柱
    Spine = 1 << 7,
    //肩胛带
    Scapula = 1 << 8,
    //ALL
    All = None| UpperLimbs | ShoulderBack | LowerLimbs | Pelvis | HeadAndNeck | ChestAndAbdomen | Spine | Scapula,
    
    
}
[Flags]
public enum BoneShowType
{ 
    //不显示
    None = 0,
    //显示骨骼
    Bone = 1<< 0,
    //显示肌肉
    Muscle = 1<< 1,     
    //筋膜
    Fascia = 1<< 2,
   //显示所有
    All =Bone|Muscle|Fascia,
   
  
}

/// <summary>
/// 骨骼方向枚举
/// </summary>
[Flags]
public enum EnumDirection
{
    //无
    None = 0,
    //左侧
    Left = 1 << 0,
    //右侧
    Right = 1 << 1,
    //其他
    Other = 1 << 2,
}



