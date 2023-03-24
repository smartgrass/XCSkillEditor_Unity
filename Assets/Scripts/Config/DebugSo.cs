using UnityEditor;
using UnityEngine;

namespace XiaoCao
{
    [CreateAssetMenu(menuName = "MyAsset/DebugSo")]
    public class DebugSo : ScriptableObject
    {
        public bool IsNoLockMousce;
        public float hitStopRate = 1;
        public bool isHitStop = true; //是否开启顿帧
        public AgentTag addPlayerTag = AgentTag.enemy;
        public bool AI = true;
        public bool NoDead = true;
        public float damageBreakTime = 0.5f;
        public float ackRate = 1f;
        public int MaxHp = 2000;
        public int maxBreakPower = 4;
        public int[] maxBreakPowerNPC = { 2,2,2};

        public GameMode gameMode = GameMode.PVE;
        public PlayerSkin playerSkin = PlayerSkin.Elf;
        public PlayerSkin npcSkin = PlayerSkin.Nor;

        public float DebugFloat;
        public string DebugStr;


        public float[] floats;
        public string[] strings;

        public float CdRate = 1;
    }


}