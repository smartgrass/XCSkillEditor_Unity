
using System;
using System.Collections.Generic;
using UnityEngine;

namespace XiaoCao
{
    public static class InputConfig
    {



    }
    public static class PlayEventMsg
    {
        public static string SetCanMove = "SetCanMove"; //移动开关
        public static string SetCanRotate = "SetCanRotate"; //旋转开关
        public static string SetUnMoveTime = "SetUnMoveTime"; //设置不能动的时间
        public static string ActivePlayerRender = "ActivePlayerRender"; //隐藏玩家Mesh
        public static string TimeStop = "TimeStop"; //顿帧
        public static string SetNoGravityT = "SetNoGravityT"; //重力开关
        public static string SetNoBreakTime = "SetNoBreakTime"; //霸体开关
        public static string PlayAudio = "PlayAudio"; //霸体开关

    }



    public enum ClientEventType
    {
        Start,
        Stop,
        Change,
        ValueChange,
    }


    public enum PlayerStateEnum
    {
        Idle,
        NorAck,
        PlayerSkill,
        //Jump,
        //Roll,
        Dead
    }


    public class PlayerInputSetting
    {
        public Dictionary<KeyCode, string> SkillKeyMsg = new Dictionary<KeyCode, string>();

        public KeyCode NorAck = KeyCode.Mouse0;

        public KeyCode Roll = KeyCode.LeftShift;

        public KeyCode Jump = KeyCode.Space;

        public float NorAckTime = 1.6f;
    }
    //通用网络消息
    public struct PlayerMessge
    {
        public DataType dataType;
        public int intMes;
        public float floatMes;
        public string strMes;
        public object objMes;

        public int IntMes { get => intMes; set { intMes = value; dataType = DataType.Int; } }
        public float FloatMes { get => floatMes; set { floatMes = value; dataType = DataType.Float; } }
        public string StrMes { get => strMes; set { strMes = value; dataType = DataType.String; } }
        public object ObjMes { get => objMes; set { objMes = value; dataType = DataType.Object; } }

        public override string ToString()
        {
            switch (dataType)
            {
                case DataType.Int:
                    return intMes.ToString();
                case DataType.Float:
                    return floatMes.ToString();
                case DataType.String:
                    return strMes;
                default:
                    break;
            }
            return base.ToString();
        }

    }

    public enum DataType : int
    {
        Null,
        Int,
        Float,
        String,
        Object
    }

    //公共引用 多个技能公用一个
    //Owner是技能的主体/父节点 动画是player,技能是特效 
    public class SkillOwner
    {
        public uint netId;
        //trigger
        public int triggerLayer;

        public bool enableAck; // 真实攻击权限 相当于local

        public bool isCustomObject; //有生成物体

        public MonoAttacker attacker;

        public Transform AckerTF => attacker.transform;

        //event的实体对象, 比如特效
        public Transform eventOwnerTF;

        public SkillOwner() { }

        public SkillOwner(MonoAttacker monoAttacker)
        {
            this.attacker = monoAttacker;
            this.netId = monoAttacker.netId;
            this.enableAck = monoAttacker.isEnableAck;
            this.eventOwnerTF = monoAttacker.transform;
            this.triggerLayer = GameSetting.GetAckLayer(monoAttacker.AgentTag, enableAck);
        }

        public static SkillOwner CopyNew(SkillOwner owner)
        {
            SkillOwner newOwner = new SkillOwner()
            {
                enableAck = owner.enableAck,
                netId = owner.netId,
                triggerLayer = owner.triggerLayer,
                attacker = owner.attacker,
            };
            return newOwner;
        }
    }

    public enum PlayerNetEventName : int
    {
        Null,
        Invoke,
        String,
        UpDateHp,

    }
}
