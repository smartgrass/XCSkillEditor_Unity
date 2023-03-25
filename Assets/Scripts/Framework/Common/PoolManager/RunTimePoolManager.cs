using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XiaoCao;
using Object = UnityEngine.Object;

public class RunTimePoolManager : MonoSingleton<RunTimePoolManager>
{

    public Dictionary<string , RunTimePool> ResPool = new Dictionary<string, RunTimePool>();

    public Dictionary<string, GameObject> IdObjDic = new Dictionary<string, GameObject>();

    public Dictionary<string, Object> IDTypePool = new Dictionary<string, Object>() ;

    public override void Init()
    {
        base.Init();
        HitEffectParent = new GameObject("HitEffectParent").transform;
    }

    //ResPool
    public GameObject LoadResPoolObj(string resPath,float AutoRecycel = 0)
    {
        if (!ResPool.ContainsKey(resPath))
        {
            var Prefab = Resources.Load(resPath) as GameObject;
            if (Prefab == null)
            {
                Debug.LogError($"yns path null {resPath}");
                string newPath = "SkillEffet/Using/Sword_1";
                Prefab = Resources.Load(newPath) as GameObject;
            }

            GameObject pool = new GameObject(resPath);
            pool.transform.SetParent(transform);
            ResPool.Add(resPath, new RunTimePool(resPath, pool.transform));
        }
        var obj = ResPool[resPath].GetObj();

        if (AutoRecycel >0)
        {
            var Timer = TimerManager.Instance.Register(AutoRecycel, () => {
                ReCycle(resPath, obj);
            });
            ResPool[resPath].TimerDic.Add(obj.GetHashCode(), Timer);
        }
        return obj;
    }

    public void ReCycle(string resPath,GameObject game)
    {
        if (ResPool.ContainsKey(resPath))
        {
            var pool = ResPool[resPath];
            pool.Recycle(game);
        }
    }

    #region IDPool
    public GameObject FindIDObj(string ID)
    {
        GameObject game = null;
        IdObjDic.TryGetValue(ID, out game);
        return game;
    }

    public GameObject FindOrCreatIDObj(string ID,string path = null)
    {
        GameObject game;
        if (!IdObjDic.ContainsKey(ID))
        {
            if(path==null)
                game= new GameObject("Empty_" + ID);
            else
                game= Resources.Load(path) as GameObject;
            RegisterID(ID, game);
        }
        else
        {
            game = IdObjDic[ID];
        }
        return game;

    }

    public GameObject LoadAndRegister(string ID, string resPath)
    {
        var Prefab = Resources.Load(resPath) as GameObject;
        if (Prefab == null)
        {
            Debug.Log("yns path null");
            return null;
        }
        var game = GameObject.Instantiate(Prefab);
        RegisterID(ID, game);
        return game;
    }


    #endregion


    public bool RegisterID(string ID, GameObject game)
    {
        if (IdObjDic.ContainsKey(ID))
        {
            Debug.LogError("ID " + ID + "is Registered!");
            return false;
        }
        IdObjDic.Add(ID, game);
        return true;
    }

    public void UnRegisterID(string ID)
    {
        IdObjDic.Remove(ID);
    }

    public void RegisterTypeIDPool<T>(string ID, string resPath) where T : Object
    {
        T newOne = ResFinder.GetResObject<T>(resPath);
        IDTypePool.Add(ID, newOne);
    }

    public T GetObjectByID<T>(string ID,string resPath) where T:Object
    {
        if (IDTypePool.ContainsKey(ID))
        {
            return IDTypePool[ID] as T;
        }
        else
        {
            T  newOne = ResFinder.GetResObject<T>(resPath);
            IDTypePool.Add(ID, newOne);
            return newOne;
        }
    }

    public AudioClip GetAudioClip(string ID,bool isHit)
    {
        string path = isHit? PrefabPath.GetHitMp3Path(ID) : PrefabPath.GetMp3Path(ID);

        AudioClip res = Resources.Load<AudioClip>(path);
        if (res == null)
        {
            Debug.LogError("yns no " + path);
        }
        return res;
    }


    public Transform HitEffectParent;
    public Dictionary<string, TypeLoopPool<GameObject>> LoopPoolDic = new Dictionary<string, TypeLoopPool<GameObject>>();


    public GameObject GetHitEffect(string ID)
    {
        if (ID.IsEmpty())
        {
            ID = "0";
        }
        if (!LoopPoolDic.ContainsKey(ID))
        {
            string path = string.Format("{0}Hit_{1}", PrefabPath.HitEffectPath, ID);
            var Prefab = Resources.Load<GameObject>(path);
            if (Prefab == null)
            {
                Debug.LogError($"yns path null {path}");
                Prefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Prefab.name = "ErrorCube";
            }
            TypeLoopPool<GameObject> newPool = new TypeLoopPool<GameObject>(Prefab, 13);
            LoopPoolDic.Add(ID, newPool);
        }
        GameObject one = LoopPoolDic[ID].GetOne();
        one.transform.SetParent(HitEffectParent,true);
        return one; //HitEffect
    }



}


public class RunTimePool
{
    public RunTimePool(string path,Transform poolTF)
    {
        ResPath = path;
        Prefab = Resources.Load(ResPath) as GameObject;
        if(Prefab == null)
        {
            Debug.LogError("no res " + ResPath);
        }
        PoolTransform = poolTF;

    }
    public Dictionary<int, Timer> TimerDic = new Dictionary<int, Timer>();


    public string ResPath;
    private GameObject Prefab;
    private Transform PoolTransform;


    public List<GameObject> UsingObj = new List<GameObject>();
    public List<GameObject> PoolObj = new List<GameObject>();


    public GameObject GetObj()
    {
        if (Prefab == null)
            return null;

        GameObject obj = null;
        if(PoolObj.Count <= 0)
        {
            obj = GameObject.Instantiate(Prefab);  
            //Debug.Log("yns  instan ");
            UsingObj.Add(obj);
            return obj;
        }
        else
        {
            obj = PoolObj[0];
            //obj.transform.SetParent(null);
            PoolObj.Remove(obj);
            UsingObj.Add(obj);
            return obj;
        }
    }

    //放回池
    public void Recycle(GameObject obj)
    {
        PoolObj.Add(obj);
        UsingObj.Remove(obj);
        obj.transform.parent = PoolTransform;
        obj.SetActive(false);

        int hash = obj.GetHashCode();
        if (TimerDic.ContainsKey(hash))
        {
            TimerManager.Instance.UnRegister(TimerDic[hash]);
            TimerDic.Remove(hash);
        }
    }

}


public class TypePool<T> where T: Behaviour
{
    public T prefab;

    public TypePool(T prefab, Action<T> recyleAction)
    {
        RecyleAction = recyleAction;
        this.prefab = prefab;
    }
    
    public Action<T> RecyleAction;

    Queue<T> unUseQue = new Queue<T>();

    public T GetOne()
    {
        if (unUseQue.Count > 0)
        {
            return unUseQue.Dequeue();
        }
        else
        {
           return GameObject.Instantiate(prefab);
        }
    }

    public void Recyle(T rec)
    {
        unUseQue.Enqueue(rec);
        RecyleAction?.Invoke(rec);
    }

}


public class TypeLoopPool<T> where T : Object
{
    public T prefab;

    public int Max =5; //大于0

    public int curIndex;

    public int genCount;

    List<T> totalList = new List<T>();

    public TypeLoopPool(T prefab,int Max)
    {
        this.prefab = prefab;
        this.Max = Max;
    }

    public T GetOne()
    {
        if (genCount < Max)
        {
            T newOne = GameObject.Instantiate(prefab);
            totalList.Add(newOne);
            genCount++;
            curIndex++;
            return newOne;
        }
       
        if (curIndex >= Max)
        {
            curIndex = curIndex % Max;    
        }
        return totalList[curIndex++];
    }

}