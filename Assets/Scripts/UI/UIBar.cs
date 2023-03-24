using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;
using DG.Tweening;
using System;

namespace XiaoCao
{
    public class UIBar : UIBase
    {
        [SerializeField]
        public Image barImg;        
        public Image barImg_noBreak;

        public Image trigleImg;
        public Transform barImgTF;
        public Transform trigleImgTF;

        public Image barImgSlow;

        public Text numText;

        public Color fullColor;
        public Color emptyColor;

        public Color playerAColor;
        public Color playerBColor;
        public Color NpcColor;

        public Vector3 offSet;
        public Vector3 offSet2;



        [OnValueChanged("TestFill")]
        public float curFill = 1;

        [HideInInspector]
        public Transform target;

        public AgentTag tag;

        public float tweenDuration = 0.2f;

        public bool isShowNum;

        public bool isMove = true;

        public bool autoSize = false;

        private bool isStart;

        private Tween uiTween;

        private void Start()
        {
            numText.gameObject.SetActive(isShowNum);
            //transform.localScale = Vector3.one * scaleRate_Bar;
        }

        public void OnUpdate()
        {
            if (IsCanvasInited && target != null)
            {
                if (isMove)
                {
                    barImgTF.position = UITool.WorldToUiPostion(target.position + offSet, MainCam);

                    if (autoSize)
                    {
                        float dis = Vector3.Distance(target.position, MainCam.transform.position);
                        barImgTF.localScale = GetScaleByDistance(dis,scaleRate_Bar) * Vector3.one;
                       
                        trigleImgTF.localScale = GetScaleByDistance(dis, scaleRate_Trig) *  Vector3.one;
                    }
                }

                //if (tag == AgentTag.playerA || tag == AgentTag.playerB)
                {
                    //Debug.Log($"yns player {trigleImgTF.position } ");
                    trigleImgTF.position = UITool.WorldToUiPostion(target.position + offSet2, MainCam);
                }
            }
        }

        public void SetFillValue(int value, int count)
        {
            SetFill(value / (float)count);
            if (isShowNum)
            {
                numText.text = string.Format("{0}/{1}", value, count);
            }

        }

        private void SetFill(float p)
        {
            if (uiTween != null)
            {
                uiTween.Kill();
            }
            uiTween = DOTween.To(x => barImgSlow.fillAmount = x, barImg.fillAmount, p, tweenDuration);
            barImg.fillAmount = p;
            //barImg.color = Color.Lerp(emptyColor, fullColor, p);
        }

        public void SetFillValueNoBreak(int value, int count)
        {
            if(count == 0)
            {
                count = 1;
                value = 0;
            }

            SetFillNoBreak(value / (float)count);
        }

        public void SetFillNoBreak(float p)
        {
            if (barImg_noBreak)
            {
                barImg_noBreak.fillAmount = p;
            }

        }



        private void TestFill()
        {
            SetFill(curFill);
        }


        public void SetTarget(Transform transform)
        {
            target = transform;
        }


        public float scaleRate_Bar = 2;
        public float scaleRate_Trig = 2;

        private float GetScaleByDistance(float d2,float h2)
        {
            float d1 = MainCam.nearClipPlane;
            float h1 = h2 * d1 / d2;
            return h1 * scaleRate_Bar;
        }

        public void SetTagUI(AgentTag agentTag)
        {
            tag = agentTag;
            if (agentTag == AgentTag.PlayerA)
            {
                trigleImg.color = playerAColor;

            }
            else if (agentTag == AgentTag.PlayerB)
            {
                trigleImg.color = playerBColor;

            }
            else
            {
                //Color color = Color.white;
                //color.a = 0;
                trigleImg.color = NpcColor;
            }
        }
    }
}