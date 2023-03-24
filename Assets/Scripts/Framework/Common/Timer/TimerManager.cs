using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TimerManager : MonoSingleton<TimerManager>
{
    private List<Timer> listTimer = new List<Timer>();
    private List<Timer> listLocalPasueTimer = new List<Timer>();

    private float globalPauseOffsetTime;

    private bool isGlobalPause = false;

    public bool IsGlobalPause
    {
        get
        {
            return isGlobalPause;
        }
        private set
        {
            isGlobalPause = value;
        }
    }

    // Use this for initialization
    void Start()
    {
#if UNITY_EDITOR
#pragma warning disable CS0618 // 类型或成员已过时
        UnityEditor.EditorApplication.playmodeStateChanged += () =>
#pragma warning restore CS0618 // 类型或成员已过时
        {
            if (UnityEditor.EditorApplication.isPaused)
            {
                PauseAll();

            }
            else if (UnityEditor.EditorApplication.isPlaying)
            {
                ResumeAll();
            }
        };
#endif
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < listTimer.Count; i++)
        {
            listTimer[i].Update();
        }
    }

    public void PauseAll()
    {
        if (IsGlobalPause) return;

        IsGlobalPause = true;

        listLocalPasueTimer = new List<Timer>();
        for (int i = 0; i < listTimer.Count; i++)
        {
            if (listTimer[i].State == EnumTimerState.Pause && !listLocalPasueTimer.Contains(listTimer[i]))
                listLocalPasueTimer.Add(listTimer[i]);

            listTimer[i].Pause();
        }


    }

    public void ResumeAll()
    {
        if (!IsGlobalPause) return;

        IsGlobalPause = false;

        for (int i = 0; i < listTimer.Count; i++)
        {
            if (!listLocalPasueTimer.Contains(listTimer[i]))
                listTimer[i].Resume();
        }
    }

    public Timer Register(float _duration, TimerCompleteHandler onCompelte = null, TimerUpdateHandler onUpdate = null, bool _isIgnoreTimeScale = true,
        bool _isRepeate = false, TimerCheckInterruptHandler _onCheckInterrupt = null, TimerInterruptHandler _onInterrupt = null, bool _isAutoDestroy = true)
    {
        Timer timer = new Timer(_duration, onCompelte, onUpdate, _isIgnoreTimeScale, _isRepeate, _onCheckInterrupt, _onInterrupt, _isAutoDestroy);
        Register(timer);
        return timer;
    }


    public void Register(Timer _timer)
    {
        if (listTimer.Contains(_timer)) return;

        if (_timer.State == EnumTimerState.Destroy)
        {
            Debug.LogError("Can not register a destoyed timer");
            return;
        }

        listTimer.Add(_timer);
        _timer.onPause += () => { if (TimerManager.Instance.IsGlobalPause && _timer.State == EnumTimerState.Pause && !listLocalPasueTimer.Contains(_timer)) listLocalPasueTimer.Add(_timer); };
        _timer.onResume += () => { if (listLocalPasueTimer.Contains(_timer)) { listLocalPasueTimer.Remove(_timer); } };
        _timer.onDestroy += () => UnRegister(_timer);
    }

    public void UnRegister(Timer _timer)
    {
        if (listTimer.Contains(_timer))
            listTimer.Remove(_timer);
    }

    void OnApplicationPause(bool _isPause)
    {
        if (_isPause)
        {
            PauseAll();
        }
        else
        {
            ResumeAll();
        }

    }

}
