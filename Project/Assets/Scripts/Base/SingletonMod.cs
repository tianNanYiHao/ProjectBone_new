using System;
using UnityEngine;


public abstract class SingletonMod<T>  where T : class, IMod, new()
    {
        private static T instance;

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new T();
                }
                return instance;
            }
        }
        bool initialized = false;
        public abstract void RegisterMessageHandler();
        public abstract void UnregisterMessageHandler();
        public bool Initialized => initialized;

        public SingletonMod()
        {
            if (instance != null)
            {
                throw new InvalidOperationException("Cannot create instances of a singleton class.");
            }
        }
      

       
       
        public virtual void Initialize()
        {
            // Default implementation, can be overridden in derived classes
            ModManager.Instance.RegisterMod(Instance);
            RegisterMessageHandler();
            initialized = true;
            Debug.Log(Instance.ToString()+" Initialize");
        }

        public virtual void Update(float time)
        {
            // Default implementation, can be overridden in derived classes
            //Debug.Log(Instance.ToString()+" Update"+time);
        }

        public virtual void OnApplicationFocus(bool hasFocus)
        {
            // Default implementation, can be overridden in derived classes
            Debug.Log(Instance.ToString()+" OnApplicationFocus"+hasFocus);
        }

        public virtual void OnApplicationPause(bool pauseStatus)
        {
            // Default implementation, can be overridden in derived classes
            Debug.Log(Instance.ToString()+"OnApplicationPause"+pauseStatus);
        }

        public virtual void OnApplicationQuit()
        {
            // Default implementation, can be overridden in derived classes
            Debug.Log(Instance.ToString()+" OnApplicationQuit");
        }

        public virtual void AllModInitialize()
        {
            // Default implementation, can be overridden in derived classes
            Debug.Log(Instance.ToString()+" AllModInitialize");
        }
        public virtual void Dispose()
        {
            // Default implementation, can be overridden in derived classes
            Debug.Log(Instance.ToString()+" Destroy");
            UnregisterMessageHandler();
        }
    }
