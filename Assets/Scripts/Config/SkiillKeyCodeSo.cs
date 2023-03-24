using NaughtyAttributes;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace XiaoCao
{
    [CreateAssetMenu(menuName = "MyAsset/SkiillKeyCodeSo")]
    public class SkiillKeyCodeSo : ScriptableObject
    {
        [RuntimeInitializeOnLoadMethod]
        private static void RuntimeInitializeOnLoadMethod()
        {
            ResFinder.SoUsingFinder.SkiillKeyCodeSo.ResetCache();
        }

        public List<SkillKey> skillKeyList;

        public Sprite defautSprite;

        private Dictionary<string, SkillKey> _getDic =null;

        public Dictionary<string, SkillKey> GetDic()
        {
            if (_getDic == null)
            {
                _getDic = new Dictionary<string, SkillKey>();
                foreach (var item in skillKeyList)
                {
                    _getDic.Add(item.skillId, item);
                }
            }
            return _getDic;
        }

        [HideInInspector]
        private List<SkillKey> active = null; //主动技能
        [HideInInspector]
        private List<SkillKey> disActive; //被动技能
        public List<SkillKey> GetActiveSkills()
        {
            TypeSKills();
            return active;
        }
        public List<SkillKey> GetDisActiveSkills()
        {
            TypeSKills();
            return disActive;
        }

        private void TypeSKills()
        {
            if (active == null)
            {
                disActive = new List<SkillKey>();
                active = new List<SkillKey>();
                foreach (var item in skillKeyList)
                {
                    if (item.keyCode != KeyCode.None)
                    {
                        active.Add(item);
                    }
                }
            }
        }

        public Sprite GetSprite(string skill)
        {
            if (GetDic().ContainsKey(skill))
            {
                return GetDic()[skill].sprite;
            }
            else
            {
                return defautSprite;
            }
        }

        public void ResetCache()
        {
            active = null;
            disActive = null;
            _getDic = null;
        }

        [Button]
        private void AutoKeyCode()
        {
            int index = 0;
            foreach (var item in skillKeyList)
            {
                if (item.keyCode!= KeyCode.None)
                {
                    item.keyCode = KeyCode.Alpha1 + index;
                    index++;
                }
            }
        }


    }
    [System.Serializable]
    public class SkillKey
    {
        public string skillId;
        public KeyCode keyCode;
        public float cdTime = 5;
        public float noBreakTime = 0;
        public float AutoLookAngle = 10;
        public float AutoLookR = 10;
        public Sprite sprite;
        public bool IsDisActive => keyCode == KeyCode.None;

    }

}