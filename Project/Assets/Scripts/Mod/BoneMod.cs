using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class BoneMod : SingletonMod<BoneMod>,IMod
{
    private LayerMask interactableLayer;
    public Dictionary<int, Bone> boneDic;
    public bool boneLoaded = false;
    private int currentBoneId = 0;
    public List<int> selectedBoneIds;

    public int CurrentBoneId
    {
        get
        {
            if (selectedBoneIds.Count > 0)
            {
                return selectedBoneIds[selectedBoneIds.Count - 1];
            }
            return  0;
        }

        set
        {
            if (value == 0)
            {
                selectedBoneIds.Clear();
            }
            else
            {
                selectedBoneIds.Add(value);
                if (GameObjectManager.Instance.Body != null)
                {
                    GameObjectManager.Instance.SelectBone(selectedBoneIds[selectedBoneIds.Count - 1]);
                    
                    // 通知移动端骨骼被选中
                    ButtonBehavior buttonBehavior = GameObject.FindObjectOfType<ButtonBehavior>();
                    if (buttonBehavior != null)
                    {
                        buttonBehavior.NotifyBoneSelected(selectedBoneIds[selectedBoneIds.Count - 1]);
                    }
                }
            }
        }
        
    }

    public override void Initialize()
    {
        base.Initialize();
        boneLoaded = false;
        interactableLayer = 1<<UnityLayer.Layer_Body;
      //  InputManager.Instance.OnTap += OnTap;
        boneDic = new Dictionary<int, Bone>();
        selectedBoneIds = new List<int>();
        CurrentBoneId = 0;
      
    }

    private void OnTap(Vector2 vec2)
    {
        Ray ray = Camera.main.ScreenPointToRay(vec2);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, interactableLayer))
        {
           Debug.Log(hit.transform.name);
        }
    }

    
    public override void RegisterMessageHandler()
    {
       
    }

   


    public override void UnregisterMessageHandler()
    {
       
    }
    
    public void Test()
    {
       
       
    }
    
    public List<Bone> SearchBone(string name)
    {
        
        List<Bone> bones = new List<Bone>();
        foreach (var bone in boneDic)
        {
            if (IsMathch(name,bone.Value.Name ))
            {
                bones.Add(bone.Value);
            }
        }

        return bones;
    }
    
    public bool  IsMathch(string input, string target)
    {
        if (string.IsNullOrEmpty(target))
        {
            return false;
        }

        if (string.IsNullOrEmpty(input))
        {
            return false;
        }
        // 预处理字符串：替换中文括号为英文括号，并对特殊字符进行转义
        input = Regex.Escape(input.Replace('（', '(').Replace('）', ')'));
        target = target.Replace('（', '(').Replace('）', ')');

        // 构建正则表达式，该表达式按顺序包含input中的每个字符，字符之间可以有其他字符
        var patternBuilder = new System.Text.StringBuilder();
        foreach (var ch in input) {
            patternBuilder.Append(Regex.Escape(ch.ToString()) + ".*?");
        }

        Regex regex = new Regex(patternBuilder.ToString(), RegexOptions.Singleline);
        bool isMatch = regex.IsMatch(target);
        return isMatch;
       
    }
    private string BuildPattern(string input) {
        // 使用字典统计input中每个字符的出现次数
        var charCount = new Dictionary<char, int>();
        foreach (var ch in input) {
            if (charCount.ContainsKey(ch)) {
                charCount[ch]++;
            } else {
                charCount[ch] = 1;
            }
        }

        // 构建正则表达式
        var patternBuilder = new System.Text.StringBuilder();
        foreach (var kvp in charCount) {
            // 对每个字符，构建一个模式，确保它在字符串中至少出现了指定次数
            // 考虑到正则表达式的特殊字符需要转义
            var escapedChar = Regex.Escape(kvp.Key.ToString());
            patternBuilder.Append($"(?=.*{escapedChar}{{{kvp.Value},}})");
        }

        return patternBuilder.ToString();
    }

}
