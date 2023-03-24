
using Assets.Scripts.Enemy;
using UnityEngine;

namespace XiaoCao
{
    public struct AckInfo
    {
        public uint netId;
        public bool isBreak;

        public float ackValue; //伤害数值
        public string baseSkillId; //主技能Id
        public string ackId; //子攻击段id 用于伤害数值

        public Vector3 skillPos;  //技能坐标
        public Vector3 skillDir;  //技能朝向
        public float angleY; //技能正方向  0表示推,180相当于吸附
        internal DamageState lastState;
    }


    public interface DamageInfo
    {


    }

    public interface IAttacker : IDamage
    {
        public float Ack { get; }
    }

    public interface IMessager
    {
        public bool SetAIEvent(ActMsgType name, string msg,object other = null);
    }

    public interface IDamage
    {
        public GameObject SelfObject { get; set; }

        public int Hp { get; set; }
        public int MaxHp { get; set; }
        public bool IsDie { get; }
        //public AgentTag AgentTag { get; set; }
        public void OnDam(AckInfo ackInfo = default);

    }

}
