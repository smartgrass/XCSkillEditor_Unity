using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test_ParticleTime : MonoBehaviour
{
    [Range(0,3)]
    public float rate = 1;

    [NaughtyAttributes.Button("SetRate")]
    public void Set()
    {
        ParticleSystem[] particles = transform.GetComponentsInChildren<ParticleSystem>();
        foreach (var item in particles)
        {
            item.startLifetime *= rate;
        }


    }

}
