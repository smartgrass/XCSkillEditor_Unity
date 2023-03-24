using UnityEngine;
using System.Collections;
using XiaoCao;
using DG.Tweening;

public class HitStop : MonoSingleton<HitStop>
{
    public float shakeTime = 0.025f;
    public float shakeLength = 0.25f;
    public int shakeCount = 8;

    public Coroutine currentDo;

    bool waiting =false;
    float refSmooth;
    float waitingTime; //等待时间
    float lastWaitlen;

    bool isEnbleHitShop = true;

    private void OnEnable()
    {
        isEnbleHitShop =  ResFinder.SoUsingFinder.DebugSo.isHitStop;
    }

    public void DoHitStop(float time = 0.001f)
    {
        if (!isEnbleHitShop)
            return;

        if (time < 0)
        {
            return;
        }

        if (waiting)
        {
            if(lastWaitlen < time)
            {
                //时停50%累加
                waitingTime = +0.5f * time;
            }
        }
        else
        {
            lastWaitlen = time;
            StartCoroutine(Wait(time));
        }

    }

    public void DoHitStop(float time ,bool isShake)
    {
        if (!isEnbleHitShop)
            return;
        if (waiting || time==0)
        {
            if (lastWaitlen < time)
                waitingTime = +0.2f * time;
            return;
        }
        //if (isShake && CurrentPlayerData.shakeLengthRate >0)
        //    CameraController.instance.CamShake(shakeTime, shakeLength, shakeCount);
        lastWaitlen = time;
        currentDo = StartCoroutine(Wait(time));
    }


    IEnumerator Wait(float time)
    {
        waiting = true;
        waitingTime = Time.unscaledTime + time;

        while (waitingTime  > Time.unscaledTime)
        {
            Time.timeScale = Mathf.SmoothDamp(Time.timeScale, 0.1f,ref refSmooth, 0.2f);
            yield return null;
        }
        Time.timeScale = 1.0f;
        waiting = false;
    }

    public void Shake(float time = 0.2f)
    {
        Camera.main.DOShakePosition(time, 0.2f, 10);
        //if (CurrentPlayerData.shakeLengthRate > 0)
        //CameraController.instance.CamShake(shakeTime, shakeLength, shakeCount);
    }

    public void Cancel()
    {
        if(currentDo != null)
            StopCoroutine(currentDo);
        Time.timeScale = 1.0f;
        waiting = false;
    }

}
