using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public delegate void TimerCompleteHandler();
public delegate void TimerUpdateHandler(float progress, float deltaTime);
public delegate bool TimerCheckInterruptHandler();
public delegate void TimerInterruptHandler();

public enum EnumTimerState
{
    StandBy,
    Timing,
    Pause,
    Destroy
}

[System.Serializable]
public class Timer
{
    private event TimerCompleteHandler onTimerComplete;
    private event TimerUpdateHandler onTimerUpdate;
    private event TimerCheckInterruptHandler onCheckInterrupt;
    private event TimerInterruptHandler onInterrupt;
    public event Action onDestroy;
    public event Action onPause;
    public event Action onResume;

    private float duration;
    private bool isIgnoreTimeScale = true;
    private bool isAutoDestroy = true;
    private bool isRepeate;

    private float startTime;
    private float pauseTime;
    private float pauseOffsetTime;
    private float timer;

    private float lastTime = 0;

    private EnumTimerState state = EnumTimerState.Timing;

    public EnumTimerState State
    {
        get
        {
            return state;
        }
    }

    private float Time_
    {
        get
        {
            return isIgnoreTimeScale ? Time.realtimeSinceStartup : Time.time;
        }
    }

    public float Duration
    {
        get
        {
            return duration;
        }
        private set
        {
            duration = value;
        }
    }

    public bool IsIgnoreTimeScale
    {
        get
        {
            return isIgnoreTimeScale;
        }
    }

    public bool IsAutoDestroy
    {
        get
        {
            return isAutoDestroy;
        }
    }

    public float CurrentTime
    {
        get
        {
            return Time_ - pauseOffsetTime - startTime;
        }
    }

    public float RemainderTime
    {
        get
        {
            return Duration - CurrentTime;
        }
    }

    public float Progress
    {
        get
        {
            return Mathf.Clamp(CurrentTime / Duration, 0, 1);
        }
    }

    public void Update()
    {
        if (state != EnumTimerState.Timing) return;

        if (onCheckInterrupt != null && onCheckInterrupt())
        {
            if (onInterrupt != null) onInterrupt();
            Pause();
        }

        timer = Time_ - pauseOffsetTime - startTime;

        if (onTimerUpdate != null)
            onTimerUpdate(Mathf.Clamp(timer / Duration, 0, 1), timer - lastTime);


        if (timer > Duration)
        {
            if (onTimerComplete != null)
                onTimerComplete();


            if (isRepeate)
            {
                Reset();
            }
            else if (IsAutoDestroy)
            {
                Destroy();
            }
            else
            {
                ResetAndStandBy();
            }
        }

        lastTime = timer;
    }

    public void Pause()
    {
        if (onPause != null)
            onPause();

        if (state == EnumTimerState.Timing)
        {
            state = EnumTimerState.Pause;
            pauseTime = Time_;
        }
    }

    public void Resume()
    {
        if (onResume != null)
            onResume();

        if (state == EnumTimerState.Pause && !TimerManager.Instance.IsGlobalPause)
        {

            state = EnumTimerState.Timing;
            pauseOffsetTime += (Time_ - pauseTime);
        }
    }

    public void Reset()
    {
        startTime = Time_;
        this.pauseOffsetTime = 0;
        state = EnumTimerState.Timing;
    }

    public void Start()
    {
        state = EnumTimerState.Timing;
    }

    public void ResetAndStandBy()
    {
        Reset();
        state = EnumTimerState.StandBy;
    }

    public void Destroy()
    {
        if (state == EnumTimerState.Destroy) return;

        onTimerComplete = null;
        onTimerUpdate = null;
        state = EnumTimerState.Destroy;
        if (onDestroy != null)
            onDestroy();
    }

    public Timer(float _duration, TimerCompleteHandler onCompelte = null, TimerUpdateHandler onUpdate = null, bool _isIgnoreTimeScale = true,
        bool _isRepeate = false, TimerCheckInterruptHandler _onCheckInterrupt = null, TimerInterruptHandler _onInterrupt = null, bool _isAutoDestroy = true)
    {
        this.Duration = _duration;
        this.isIgnoreTimeScale = _isIgnoreTimeScale;
        this.isRepeate = _isRepeate;
        this.isAutoDestroy = _isAutoDestroy;

        this.onTimerComplete = onCompelte;
        this.onTimerUpdate = onUpdate;
        this.onCheckInterrupt = _onCheckInterrupt;
        this.onInterrupt = _onInterrupt;

        this.startTime = Time_;
        this.pauseTime = 0;
        this.pauseOffsetTime = 0;
        this.state = EnumTimerState.Timing;
    }

    public void Run()
    {
        TimerManager.Instance.Register(this);
    }

}
