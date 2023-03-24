using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using Flux;

namespace XiaoCao
{
    public class TestSkill : MonoBehaviour
    {
        public AnimationClip idle;
        public GameObject editorPlayer;
        public GameObject player;
        public string skillId = "1";
        public string animName = "RollTree";
        //public string animName = "jump";

        private XCEventsRunner runer;

        [OnValueChanged("Run")]
        [Range(0, 1)]
        public float timeScale = 1;

        private void GetPlayer()
        {
            player = PlayerMgr.Instance.LocalPlayer.gameObject;
        }

        public void Run()
        {
            Time.timeScale = timeScale;
        }

        //[Button()]
        //private void PlayAnim()
        //{
        //    GetPlayer();
        //    var anim = player.GetComponent<Animator>();
        //    anim.Play("RollTree");
        //}

        //[Button("play")]
        //public void play()
        //{
        //    GetPlayer();
        //    SkillEventData skill = ResFinder.GetSkillData(skillId);
        //    var _skill = GameObject.Instantiate<SkillEventData>(skill);
        //    if (runer != null)
        //    {
        //        runer.Stop();
        //    }
        //    runer = SkillLauncher.StartPlayerSkill(new SkillOwner(player), skill, player.transform);

        //}


        [Button("ResetEditorPlayer")]
        public void ResetPlayer()
        {
            if (editorPlayer != null && idle != null)
            {
                idle.SampleAnimation(editorPlayer, 0);
                editorPlayer.transform.localPosition = Vector3.zero;
            }
        }

    }
}
