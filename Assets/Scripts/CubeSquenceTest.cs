using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Flux;
using NaughtyAttributes;

public class CubeSquenceTest : MonoBehaviour
{
    // Start is called before the first frame update
    public FSequence sequence;

    public List<FTrack> tracks;

    public List<FTimeline> timelines;

    public List<Transform> owners;

    [Button]
    public void Play()
    {
        sequence = GetComponent<FSequence>();
        tracks = new List<FTrack>();
        foreach (var con in sequence.Containers)
        {
            foreach (var item in con.Timelines)
            {
                timelines.Add(item);
            }
        }
        FAnimationTrack fAnimation = new FAnimationTrack();
        //fAnimation.Events
        //sequence.Play();
    }

    public void InitSequence()
    {


    }



}



