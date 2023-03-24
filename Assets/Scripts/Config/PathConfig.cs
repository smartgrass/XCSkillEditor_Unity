using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using XiaoCao;

public static class PrefabPath
{
    public static readonly string Player = "man_net";

    public static readonly string EnemyPrefabPath = "Charecter/Enemy";

    public static readonly string SoundDirPath = "Sound";

    public static readonly string SkillDataPath = "SkillData/";

    public static readonly string SkillDataEnemyPath = "SkillData/SkillData_Enemy/";

    public static readonly string HitEffectPath = "HitEffect/";

    public static readonly string UIBar = "UI/MainUI/UIBar.prefab";
    public static string UIMrg = "UI/MainUI/UIMrg.prefab";

    public static readonly string ResUsing = "ResUsing/ResUsing";
    public static readonly string SoUsing = "ResUsing/SoUsing";

    internal static string SingletonUI(SingletonUIType uiType)
    {
        return "UI/SingletonUI/" + uiType.ToString()+".prefab";
    }

    public static string GetSkillDataPath(bool isEnemy = false)
    {
        return isEnemy ? SkillDataEnemyPath : SkillDataPath;
    }

    public static string GetEnemyPrefabPath(AgentModelType agentName)
    {
        return string.Format("{0}/{1}.prefab", EnemyPrefabPath, agentName); 
    }    
    public static string GetMp3Path(string id)
    {
        return string.Format("{0}/{1}", SoundDirPath, id); 
    }        
    public static string GetHitMp3Path(string id)
    {
        return string.Format("{0}/Hit/{1}", SoundDirPath, id); 
    }    

}




public static class DataPath
{
    public static readonly string playerDataJson = string.Format("{0}/playerData.json",Application.dataPath);
}

