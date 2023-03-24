using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace XiaoCao
{

    public class ResUsing : MonoBehaviour
    {


    }

    public static class ResFinder
    {
        private static ResUsing _resUsingFinder;
        public static ResUsing ResUsingFinder
        {
            get
            {
                if (_resUsingFinder == null)
                {
                    _resUsingFinder = Resources.Load<ResUsing>(PrefabPath.ResUsing);
                }
                return _resUsingFinder;
            }
        }

        public static SoUsing SoUsingFinder
        {
            get
            {
                if (_soUsingFinder == null)
                {
                    _soUsingFinder = Resources.Load<SoUsing>(PrefabPath.SoUsing);
                }
                return _soUsingFinder;
            }
        }

        private static SoUsing _soUsingFinder;


        public static SkillEventData GetSkillData(string skillId,bool isEnemy =false)
        {
            return GetResObject<SkillEventData>(PrefabPath.GetSkillDataPath(isEnemy) + skillId);
        }

        public static GameObject GetEnmeyPrefab(AgentModelType agentName)
        {
            return Resources.Load<GameObject>(PrefabPath.GetEnemyPrefabPath(agentName).LogStr());
        }

        public static T GetResObject<T>(string path) where T : UnityEngine.Object
        {
            return Resources.Load<T>(path);
        }
    }

}
