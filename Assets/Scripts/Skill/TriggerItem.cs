using System;
using System.Collections.Generic;
using UnityEngine;

namespace XiaoCao
{
    public class TriggerItem : MonoBehaviour
    {
        public AckInfoObject ackInfoObject = new AckInfoObject();

        public MonoAttacker attacker;

        private void OnTriggerEnter(Collider other)
        {
            if (attacker.isEnableAck)
            {
                ackInfoObject.skillPos = other.ClosestPointOnBounds(transform.position);
                ackInfoObject.skillDir = transform.forward;
                attacker.OnAckTrigger(other, ackInfoObject.ToAckInfo());
            }
            else
            {
                Debug.LogError("yns ?? has Trigger?");
            }
        }


    }

    //引用类 减少内存分配
    public class AckInfoObject
    {
        public uint netId;
        public float ackValue; //伤害数值
        public string baseSkillId; //主技能Id
        public string ackId; //子攻击段id 用于伤害数值

        public Vector3 skillPos;  //技能坐标
        public Vector3 skillDir;  //技能朝向
        public float angleY; //技能正方向  0表示推,180相当于吸附

        public AckInfo ToAckInfo() 
        { 
            return new AckInfo()
            {
                netId = netId,
                ackId = ackId,
                baseSkillId = baseSkillId,
                angleY = angleY,
                skillPos= skillPos,
                skillDir= skillDir
            }; 
        }
    }

}
