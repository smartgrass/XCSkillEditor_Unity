using UnityEditor;
using UnityEngine;

namespace XiaoCao
{
    [CreateAssetMenu(menuName = "MyAsset/PlayerMoveSettingSo")]
    public class PlayerMoveSettingSo : ScriptableObject
    {
        public LayerMask GroundLayers;
        public  float m_MovingTurnSpeed = 230;//移动中转向的速度
        public  float m_StationaryTurnSpeed = 250;//站立中转向的速度
        public  float NorMoveSpeed = 5;
        public  float LookEnemyRad = 12;//锁定敌人半径
        public  float LockEnemyAngle = 60;//锁定敌人角度
        [Header("击飞设置")]
        [XCLabel("击飞倍率")]
        public float AddYRate = 1;//击飞上升倍率
        [XCLabel("击退倍率")]
        public float AddHorRate = 1;//击飞上升倍率
        public float Gravity = -9.8f;//击飞上升倍率
        public float GravityOnGrondRate = 0.8f;//

        public float GravityOnAirAddRate = 1f;//在空中重力增强应该更大
        public float GravityMaxRate = 4f;//重力上限


    }
}