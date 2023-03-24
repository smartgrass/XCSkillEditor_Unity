using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TriggerUnityEvent : MonoBehaviour
{
    public UnityEvent triggerEvent;


    public bool isOnce = true;
    private bool isFinish = false;



    private void OnTriggerEnter(Collider other)
    {
        if (isOnce)
        {
            if (!isFinish && other.tag == "Player")
            {
                triggerEvent?.Invoke();
                isFinish = true;
            }
        }
        else
        {
            if (other.tag == "Player")
            {
                triggerEvent?.Invoke();
            }
        }
     
    }


}