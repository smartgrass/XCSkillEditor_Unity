using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
{

    protected static T _instance = null;

    public static T Instance
    {
        get
        {
            if (!Application.isPlaying)
                return null;

            if (_instance == null)
            {
                GameObject DDOLGO = GameObject.Find("DontDestroyOnLoadGO");
                if (DDOLGO == null)
                {
                    DDOLGO = new GameObject("DontDestroyOnLoadGO");
                    DontDestroyOnLoad(DDOLGO);
                }

                GameObject go = new GameObject(typeof(T).ToString());
                go.transform.SetParent(DDOLGO.transform, false);
                _instance= go.AddComponent<T>();
                _instance.Init();
            }
            return _instance;
        }
    }

   public virtual void Init()
    {

    }

}
