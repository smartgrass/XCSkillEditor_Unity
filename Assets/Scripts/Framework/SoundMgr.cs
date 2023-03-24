using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundMgr : MonoSingleton<SoundMgr>
{
    private void Awake()
    {
        _instance = this;
        AsPool = new TypeLoopPool<AudioSource>(audioSourcePrefab, 5);
        HitAsPool = new TypeLoopPool<AudioSource>(audioSourcePrefab, 6);
    }

    public AudioSource audioSourcePrefab;

    TypeLoopPool<AudioSource> AsPool;
    TypeLoopPool<AudioSource> HitAsPool;

    public void PlayHitAudio(int id,bool isBreak)
    {
        if (id == 0)
        {
            return;
        }

        string soundId = isBreak ? id + "Break" : id.ToString();
        AudioClip audioClip = RunTimePoolManager.Instance.GetAudioClip(soundId, true);
        var AS = HitAsPool.GetOne();
        AS.transform.SetParent(transform);
        AS.clip = audioClip;
        AS.volume = 1;
        AS.Play();
    }

    public void PlayAudio(string id, float volume = 1, uint netId = 0)
    {
        AudioClip audioClip = RunTimePoolManager.Instance.GetAudioClip(id,false);
        var AS =  AsPool.GetOne();
        AS.transform.SetParent(transform);
        AS.clip = audioClip;
        AS.volume = volume;
        AS.Play();
    }

}
