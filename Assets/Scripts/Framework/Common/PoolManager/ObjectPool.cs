using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ObjectPool : MonoBehaviour
{
    private string prefabPath;

    [SerializeField]
    private List<GameObject> unusedList = new List<GameObject>();
    [SerializeField]
    private List<GameObject> usedList = new List<GameObject>();

    [SerializeField]
    private int preloadAmount;
    [SerializeField]
    private int maxAmount;
    [SerializeField]
    private bool autoSetActive;

    private Timer checkTimer;
    private Timer destroyTimer;

    public string PoolName
    {
        get
        {
            return prefabPath;
        }
    }

    public int CurrentAmount
    {
        get
        {
            return unusedList.Count + usedList.Count;
        }
    }

    public void InitPool(string _prefabPath, int _maxAmount, bool _autoSetActive = true)
    {
        SetPool(_prefabPath, 0, _maxAmount, _autoSetActive);

        //ResManager.Instance.LoadAsync(_prefabPath, (go) => gameObject.name = (go as GameObject).name + " Pool", (progress) => Debug.Log(progress));

        GameObject go = ResManager.Instance.Load(_prefabPath) as GameObject;

        gameObject.name = go.name + " Pool";

    }

    public void InitPoolAsync(string _prefabPath, int _preloadAmount, int _maxAmount, bool _autoSetActive = true, Action<float> _progress = null)
    {
        if (_preloadAmount > _maxAmount)
        {
            Debug.LogError("PreloadAmount is greater than maxAmount");
        }

        SetPool(_prefabPath, _preloadAmount, _maxAmount, _autoSetActive);

        ResManager.Instance.LoadAsync(_prefabPath, (go) => gameObject.name = (go as GameObject).name + " Pool", _progress);
        //GameObject go = ResManager.Instance.Load(_prefabPath) as GameObject;

        //gameObject.name = go.name + " Pool";

        PreLoad(_progress);
    }

    private void SetPool(string _prefabPath, int _preloadAmount, int _maxAmount, bool _autoSetActive = true)
    {
        this.prefabPath = _prefabPath;
        this.preloadAmount = _preloadAmount;
        this.maxAmount = _maxAmount;
        this.autoSetActive = _autoSetActive;
    }

    public void EnableAutoDestroy(float _startDestroyTime, float _destroyPeriod, int _destroyAmountPerTime)
    {
        if (checkTimer == null)
        {
            checkTimer = new Timer(_startDestroyTime,
                () =>
                {
                    if (destroyTimer == null)
                        destroyTimer = TimerManager.Instance.Register(_destroyPeriod, () => DestroyUnusedObj(_destroyAmountPerTime), null, true,
                            true, () =>
                            {
                                return CurrentAmount <= preloadAmount || unusedList.Count <= 0;
                            }, null, false);
                    else
                        destroyTimer.Reset();
                }
            , null, true, true);

            TimerManager.Instance.Register(checkTimer);
        }
        else
        {
            checkTimer.Reset();
        }
    }

    public void DisableAutoDestroy()
    {
        if (checkTimer != null) checkTimer.Destroy();
        if (destroyTimer != null) destroyTimer.Destroy();

        checkTimer = null;
        destroyTimer = null;
    }

    private void GenerateNewObj()
    {
        GameObject go = ResManager.Instance.LoadInstance(prefabPath) as GameObject;
        OnGenerateNewObj(go);
    }

    private void GenerateNewObjAsync(Action<float> _progress)
    {
        ResManager.Instance.LoadAsyncInstance(prefabPath, OnGenerateNewObj, _progress);
    }

    private void OnGenerateNewObj(UnityEngine.Object _object)
    {
        GameObject go = _object as GameObject;
        go.GetComponent<IPoolObject>().Init();
        go.transform.SetParent(transform, false);
        if (autoSetActive) go.SetActive(false);
        unusedList.Add(go);
    }

    private void DestroyUnusedObj(int amount)
    {
        for (int i = 0; i < amount && CurrentAmount > preloadAmount && unusedList.Count > 0; i++)
        {
            GameObject.Destroy(unusedList[0]);
            unusedList.RemoveAt(0);
        }
    }

    private void PreLoad(Action<float> _progress)
    {
        unusedList = new List<GameObject>();
        usedList = new List<GameObject>();

        for (int i = 0; i < preloadAmount; i++)
        {
            GenerateNewObjAsync(_progress);
        }
    }

    public GameObject Spawn()
    {
        if (unusedList.Count <= 0)
        {
            if (CurrentAmount >= maxAmount)
            {
                Debug.LogError("Run out of object in pool :" + PoolName);
                return null;
            }
            else
            {
                GenerateNewObj();
            }
        }

        GameObject go = unusedList[unusedList.Count - 1];
        unusedList.RemoveAt(unusedList.Count - 1);
        usedList.Add(go);

        if (autoSetActive) go.SetActive(true);
        go.GetComponent<IPoolObject>().OnSpawn();

        if (checkTimer != null) checkTimer.Reset();
        if (destroyTimer != null) destroyTimer.ResetAndStandBy();

        return go;
    }

    public void Despawn(GameObject go)
    {
        if (Contains(go))
        {
            unusedList.Add(go);
            usedList.Remove(go);

            if (autoSetActive) go.SetActive(false);
            go.transform.SetParent(transform, false);
            go.GetComponent<IPoolObject>().OnDespawn();
        }
    }

    public void DespawnAll()
    {
        List<GameObject> list = new List<GameObject>(usedList);
        for (int i = 0; i < list.Count; i++)
        {
            Despawn(list[i]);
        }

    }

    public void DestroyPool()
    {
        foreach (GameObject item in unusedList)
        {
            GameObject.Destroy(item);
        }

        foreach (GameObject item in usedList)
        {
            GameObject.Destroy(item);
        }

        unusedList.Clear();
        usedList.Clear();

        DisableAutoDestroy();
    }

    public bool Contains(GameObject go)
    {
        return usedList.Contains(go);
    }

}
