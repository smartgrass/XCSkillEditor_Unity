using NaughtyAttributes;
using System;
using System.Collections.Generic;
using UnityEngine;
using XiaoCao;

namespace Assets.Scripts.Enemy
{
    public class AI_Test : MonoBehaviour
    {
        [Header("输入数字键盘1~4测试")]
        public AI aI;

        private void Update()
        {
            TestAIInput();

        }

        private void TestAIInput()
        {
            for (int i = 0; i < 4; i++)
            {
                if (Input.GetKeyDown(KeyCode.Keypad1 + i))
                {
                    if (aI == null)
                    {
                        GetAi();
                    }

                    if (aI == null)
                    {
                        Debug.Log($"yns no Enemy");
                        return;
                    }
                    if (aI.aIActions[0].aIActions.Count > i) //test
                    {
                        aI.CallSetAIEvent(ActMsgType.Skill, aI.aIActions[0].aIActions[i].actMsg);
                    }
                }
            }
        }

        private void GetAi()
        {
            foreach (var item in PlayerMgr.Instance.MonoAttackerDic)
            {
                if (item.Value.AgentTag == AgentTag.enemy)
                {
                    aI = item.Value.AI;

                }
            }
        }
    }
}
