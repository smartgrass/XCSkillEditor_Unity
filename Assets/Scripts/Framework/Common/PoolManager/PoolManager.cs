using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PoolManager : MonoSingleton<PoolManager>
{

    private Dictionary<string, ObjectPool> dicPools = new Dictionary<string, ObjectPool>();

    private Vector3 objPos = Vector3.zero;

    private bool autoRegister = false;

    private const int defaultMaxAmount = 30;

    private const bool defaultAutoSetActive = true;

    public GameObject Spawn(string objPath)
    {
        if (Contains(objPath))
        {

            return dicPools[objPath].Spawn();
        }
        else if (autoRegister)
        {
            RegisterPool(objPath, defaultMaxAmount, defaultAutoSetActive);
            return dicPools[objPath].Spawn();
        }
        else
        {
            Debug.LogError("The objec pool" + " '" + objPath + "' " + "has not registered");
            return null;
        }
    }

    public void Despawn(GameObject go)
    {
        foreach (ObjectPool pool in dicPools.Values)
        {
            if (pool.Contains(go))
            {
                pool.Despawn(go);
                return;
            }
        }

        Debug.LogError("This object doesn't belong to any pool:" + go.name);
    }

    public void DespawnAll()
    {
        foreach (ObjectPool pool in dicPools.Values)
            pool.DespawnAll();
    }

    public void DestroyPool(string _name)
    {
        dicPools[_name].DestroyPool();
    }

    public void DestroyAllPool()
    {
        foreach (ObjectPool pool in dicPools.Values)
            pool.DestroyPool();
    }

    public bool Contains(string _name)
    {
        return dicPools.ContainsKey(_name);
    }

    public ObjectPool Get(string _name)
    {
        if (Contains(_name))
            return dicPools[_name];
        else
            return null;
    }

    public ObjectPool RegisterPool(string _name, int _maxAmount = defaultMaxAmount, bool _autoSetActive = defaultAutoSetActive)
    {
        if (dicPools.ContainsKey(_name))
        {
            //Debug.Log("Pool has already exist");
            return null; ;
        }

        GameObject go = new GameObject();
        go.transform.SetParent(transform, false);
        go.transform.position = objPos;
        ObjectPool pool = go.AddComponent<ObjectPool>();
        pool.InitPool(_name, _maxAmount, _autoSetActive);
        dicPools.Add(_name, pool);

        return pool;
    }

    public ObjectPool RegisterPoolAsync(string _name, int _preloadAmount, int _maxAmount, bool _autoSetActive = true, Action<float> _progress = null)
    {
        if (dicPools.ContainsKey(_name))
        {
            //Debug.LogError("Pool has already exist");
            return dicPools[_name];
        }

        GameObject poolGO = new GameObject(_name);
        poolGO.transform.SetParent(transform, false);
        poolGO.transform.position = objPos;
        ObjectPool pool = poolGO.AddComponent<ObjectPool>();
        pool.InitPoolAsync(_name, _preloadAmount, _maxAmount, _autoSetActive, _progress);
        dicPools.Add(_name, pool);
        return pool;
    }

    private void OnPreLoadProgress(float progress)
    {
        Debug.Log(progress);
    }

}
