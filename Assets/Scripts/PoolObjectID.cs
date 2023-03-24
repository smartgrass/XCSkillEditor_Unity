using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolObjectID : MonoBehaviour
{

    public string ID = "ID";
    void Awake()
    {
        RunTimePoolManager.Instance.RegisterID(ID, gameObject);
    }

}
