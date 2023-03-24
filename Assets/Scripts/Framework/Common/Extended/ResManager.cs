using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AssetInfo
{
    private UnityEngine.Object _Object;
    public Type AssetType { get; set; }
    public string Path { get; set; }
    public int RefCount { get; set; }
    public bool IsLoaded
    {
        get
        {
            return _Object != null;
        }
    }

    public UnityEngine.Object AssetObject
    {
        get
        {
            if (null == _Object)
            {
                _ResourcesLoad();
            }
            return _Object;
        }
    }

    public string Name
    {
        get
        {
            return AssetObject.name;
        }
    }

    public IEnumerator GetCoroutineObject(Action<UnityEngine.Object> OnLoadEnd)
    {
        while (true)
        {
            yield return null;

            if (_Object == null)
            {
                _ResourcesLoad();
                yield return null;
            }

            if (OnLoadEnd != null)
            {
                OnLoadEnd(_Object);
            }

            yield break;
        }
    }

    private void _ResourcesLoad()
    {
        try
        {
            _Object = Resources.Load(Path);
            //if (null == _Object)
            //    Debug.Log("Resources Load Failure! Path:" + Path);
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }

    public IEnumerator GetAsyncObject(Action<UnityEngine.Object> OnLoadEnd, Action<float> progress)
    {
        if (_Object != null)
        {
            OnLoadEnd(_Object);
            yield break;
        }
        
        ResourceRequest _resRequest = Resources.LoadAsync(Path);

        while (_resRequest.progress < 0.9)
        {
            if (null != progress)
                progress(_resRequest.progress);
            yield return null;
        }

        while (!_resRequest.isDone)
        {
            if (null != progress)
                progress(_resRequest.progress);
            yield return null;
        }


        _Object = _resRequest.asset;
        if (null != OnLoadEnd)
            OnLoadEnd(_Object);

        yield return _resRequest;

    }

}

public class ResManager : Singleton<ResManager>
{
    private Dictionary<string, AssetInfo> dicAssetInfo = null;

    protected override void Init()
    {
        dicAssetInfo = new Dictionary<string, AssetInfo>();
    }
    #region 加载并获取对应类
    public T LoadPrefab<T>(string path) where T : UnityEngine.Object
    {
        GameObject game = Resources.Load<GameObject>(path);
        T t = game.GetComponent<T>();
        if (t != null)
            return t;
        else
        {
            if (game != null)
                Debug.Log("获取组件失败");
            else
                Debug.Log("找不到文件:" + path);
            return null;
        }
    }
    #endregion

    #region 加载并实例化

    public UnityEngine.Object LoadInstance(string _path)
    {
        UnityEngine.Object _obj = Load(_path);
        return Instantiate(_obj);
    }

    public void LoadCoroutineInstance(string _path, Action<UnityEngine.Object> OnLoadEnd)
    {
        LoadCoroutine(_path, (_obj) => Instantiate(_obj, OnLoadEnd));
    }

    public void LoadAsyncInstance(string _path, Action<UnityEngine.Object> OnLoadEnd)
    {
        LoadAsync(_path, (_obj) => Instantiate(_obj, OnLoadEnd));
    }

    public void LoadAsyncInstance(string _path, Action<UnityEngine.Object> OnLoadEnd, Action<float> _progress)
    {
        LoadAsync(_path, (_obj) => Instantiate(_obj, OnLoadEnd), _progress);
    }

    #endregion

    #region 普通加载

    public UnityEngine.Object Load(string _path)
    {
        AssetInfo _assetInfo = GetAssetInfo(_path);
        if (null != _assetInfo)
            return _assetInfo.AssetObject;
        return null;
    }

    #endregion

    #region 协程加载

    public void LoadCoroutine(string _path, Action<UnityEngine.Object> OnLoadEnd)
    {
        AssetInfo _assetInfo = GetAssetInfo(_path, OnLoadEnd);
        if (_assetInfo != null)
            CoroutineController.Instance.StartCoroutine(_assetInfo.GetCoroutineObject(OnLoadEnd));
    }

    #endregion

    #region 异步加载

    public void LoadAsync(string _path, Action<UnityEngine.Object> _loaded)
    {
        LoadAsync(_path, _loaded, null);
    }


    public void LoadAsync(string _path, Action<UnityEngine.Object> _loaded, Action<float> _progress)
    {
        AssetInfo _assetInfo = GetAssetInfo(_path, _loaded);
        if (null != _assetInfo)
            CoroutineController.Instance.StartCoroutine(_assetInfo.GetAsyncObject(_loaded, _progress));
    }

    #endregion

    #region 获取 AssetInfo 并 实例化

    private AssetInfo GetAssetInfo(string _path)
    {
        return GetAssetInfo(_path, null);
    }

    private AssetInfo GetAssetInfo(string _path, Action<UnityEngine.Object> OnLoadEnd)
    {
        if (string.IsNullOrEmpty(_path))
        {
            Debug.LogError("Error: null _path name.");
            if (null != OnLoadEnd)
                OnLoadEnd(null);
        }

        AssetInfo _assetInfo = null;

        if (!dicAssetInfo.TryGetValue(_path, out _assetInfo))
        {
            _assetInfo = new AssetInfo();
            _assetInfo.Path = _path;
            dicAssetInfo.Add(_path, _assetInfo);
        }

        _assetInfo.RefCount++;
        return _assetInfo;
    }

    private UnityEngine.Object Instantiate(UnityEngine.Object _obj)
    {
        return Instantiate(_obj, null);
    }

    private UnityEngine.Object Instantiate(UnityEngine.Object _obj, Action<UnityEngine.Object> OnLoadEnd)
    {
        UnityEngine.Object _retObj = null;

        if (_obj != null)
        {
            _retObj = MonoBehaviour.Instantiate(_obj);
            if (_retObj != null)
            {
                if (OnLoadEnd != null)
                {
                    OnLoadEnd(_retObj);
                    return null;
                }
                return _retObj;
            }
            else
            {
                Debug.LogError("Error: null Instantiate _retObj.");
            }
        }
        else
        {
            Debug.LogError("Error: null Resources Load return _obj.");
        }
        return null;
    }

    #endregion

}
