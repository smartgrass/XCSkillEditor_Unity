using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace XiaoCao
{
    public class SkillIcon : MonoBehaviour
    {
        public Image mainImg;
        public Image maskImg;
        public Image greyImg;

        public Text text;

        public XCTimer timer;


        public void Init(string skillID)
        {
            this.timer = PlayerMgr.Instance.skillCD.GetTimer(skillID);
            var skillSetting = ResFinder.SoUsingFinder.SkiillKeyCodeSo;
            mainImg.sprite = skillSetting.GetSprite(skillID);

            var dic = skillSetting.GetDic();
            dic.TryGetValue(skillID, out SkillKey skill);
            if (skill != null)
            {
                if (skill.IsDisActive)
                {
                    text.gameObject.transform.parent.gameObject.SetActive(false);
                }
                else
                {
                    text.gameObject.transform.parent.gameObject.SetActive(true);
                    text.text = KeyCodeToString(skill.keyCode);
                }
            }
        }

        public void OnUpdate()
        {
            if (timer != null)
            {
                maskImg.fillAmount = timer.isRuning? 1 - timer.FillAmount :0;
                greyImg.enabled = timer.isRuning;
            }
        }

        public void OnDisUpdate()
        {
            if (timer != null)
            {
                if (timer.isRuning)
                {
                    maskImg.fillAmount = timer.isRuning ? 1 - timer.FillAmount : 0;
                    greyImg.enabled = timer.isRuning;
                }
                gameObject.SetActive(timer.isRuning);
            }
        }

        public string KeyCodeToString(KeyCode key)
        {
            return Convert.ToChar(key).ToString().ToUpper();
        }

        //str->转KeyCode
        public KeyCode StringToKeyCode(string str)
        {
            if (str.Length <= 0) return KeyCode.None;
            if (char.IsDigit(str[0]))
            {
                return (KeyCode)System.Enum.Parse(typeof(KeyCode), ("Alpha" + str.ToUpper().Substring(str.Length - 1, 1)));
            }
            else if (char.IsLetter(str[0]))
            {
                return (KeyCode)System.Enum.Parse(typeof(KeyCode), str.ToUpper().Substring(str.Length - 1, 1));
            }

            return KeyCode.None;
        }
    }
}
