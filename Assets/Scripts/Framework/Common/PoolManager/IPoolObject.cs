using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPoolObject
{
    void Init();

    void OnSpawn();

    void OnDespawn();
}
