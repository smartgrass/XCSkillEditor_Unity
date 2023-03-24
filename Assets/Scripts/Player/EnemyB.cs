
using Assets.Scripts.Enemy;
using cfg;
using DG.Tweening;
using Mirror;
using NaughtyAttributes;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace XiaoCao
{
    ///敌人动画准备 <see cref="AnimNameStr"/> , 受击动画加上OnBreak的Tag
    ///动画属性  <see cref="AnimConfig"/> 
    public class EnemyB: PlayerState
    {
        [Header("敌人血量设置")]
        public SetValueDate setData;

        public override void StartClientSetting()
        {
            if (setData.MaxHp == 0)
            {

                setData.MaxHp = 100;
                Debug.LogError("yns setData hp 0 {}");
            }

            //设置数值
            playerData.MaxAndDull(setData.MaxHp);
            breakPower.SetMaxValue(setData.MaxBraekPower);

            breakTimer.Init("damageTimer", 0.1f, null); //脱离Break ->OnBreakExit
            //noDamageTimer.Init("noDamageTimer", 0.2f, OnNoDamExit);
            dieTimer.Init("dieTimer", 2, OnDieEnd);
        }


    }

    [Serializable]
    public class SetValueDate
    {
        public int MaxHp = 100;
        public int MaxBraekPower = 10;
    }
}