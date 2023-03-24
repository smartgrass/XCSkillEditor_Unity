using cfg;
using SimpleJSON;
using System.Collections;
using UnityEngine;

namespace XiaoCao
{
    public class SkillSettingMgr : MonoSingleton<SkillSettingMgr>
    {
        private SkillSettingReader skillSettingReader;

        public SkillSettingReader SkillSettingReader
        {
            get
            {
                if (skillSettingReader == null)
                {
                    skillSettingReader = ReadSkillConfig();
                }
                return skillSettingReader;
            }
        }

        public SkillSettingReader ReadSkillConfig()
        {

            Tables tables = new Tables(SkillLoader);

            return tables.SkillSettingReader;
        }

        private JSONNode SkillLoader(string fileName)
        {
            string path = "Config/Luban/skillsettingreader";
            TextAsset text = Resources.Load<TextAsset>(path);
            return JSON.Parse(text.text);
        }



        public SkillSetting GetSkillSetting(string id)
        {
            SkillSetting res = SkillSettingReader.GetOrDefault(id);
            if(res == null)
            {
                res = skillSettingReader.DataList[0];
                Debug.Log($"yns GetDefaut skillSetting ");
            }
            return res;
        }






    }
}