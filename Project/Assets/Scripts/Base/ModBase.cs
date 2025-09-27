using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.Experimental.GlobalIllumination;

namespace GameUI
{
    public abstract class ModBase 
{
    bool _isInitialized = false;

    public bool IsInitialized => _isInitialized;

    public void Use()
    {
    }

    public virtual void Initialize()
    {
        _isInitialized = true;
        RegisterMessageHandler();
    }

    public virtual void Update()
    {
    }

    public virtual void Dispose()
    {
        UnregisterMessageHandler();
    }

    public abstract void RegisterMessageHandler();
    public abstract void UnregisterMessageHandler();

  
   
}
}

