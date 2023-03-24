using System.Collections;
using UnityEngine;

namespace XiaoCao
{
    public enum MoveType : uint
    {
        Nor,
        SpeedUp,
        SpeedDown,
        Smooth
    }
    public enum TransfromType
    {
        PlyerUnFollow =0, //以玩家实时的坐标参考系 (发射后不受玩家影响)
        FollowPlayer = 1, //以玩家实时的坐标参考系, (发射后跟随玩家)
        WorldPos = 2, //技能启动时的玩家的参考系
    }

    public enum InputEventType
    {
        Wait, //占时间用,防止技能走完动画就结束, 用于..
        Exit = 1,   //直接中断,并解除输入锁定
        Finish = 2//解除输入锁定
    }

    public enum MsgType
    {
        All,
        Bool
    }



}
