using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class WebRequestManager : MonoSingleton<WebRequestManager>, IDisposable
{
    private string url = "http://ecmo.froglesson.com/protobuf/index";
    public override void Initialize()
    {
        base.Initialize();
    }
    
    public override void Update()
    {
        base.Update();
    }

    
    public override void Destroy()
    {
        base.Destroy();
    }

    
   public void Dispose()
   {
       
   }
  
   

   
   
   public Dictionary<string, string> ParseResponse(string responseData)
   {
       var data = new Dictionary<string, string>();
       var lines = responseData.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
       foreach (var line in lines)
       {
           var match = Regex.Match(line, @"[""']?(\w+)[""']?\s*=>\s*string\(\d+\)\s*[""'](.+)[""']");
           if (match.Success)
           {
               data[match.Groups[1].Value] = match.Groups[2].Value;
           }
       }
       return data;
   }
}
