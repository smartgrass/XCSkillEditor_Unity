//using System;
using NaughtyAttributes;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using XiaoCao;

namespace Flux
{
    [FEvent("Sequence/FPlayMsgEvent", typeof(FInputTrack))]
    public class FPlayMsgEvent : FEvent
    {
        public MsgType msgType;
        [NaughtyAttributes.OnValueChanged(nameof(OnValueChange))]
        [NaughtyAttributes.Dropdown(nameof(GetValues))]
        public string msgName;
        public string strMsg;
        public float floatMsg;
        public bool boolMsg;
        public bool setOppositeOnFinish = true;


        private DropdownList<string> GetValues()
        {
            return new DropdownList<string>()
            {
                { "SetCanMove(bool)",   PlayEventMsg.SetCanMove},
                { "SetCanRotate(bool)",  PlayEventMsg.SetCanRotate },
                { "ActivePlayerRender(bool)",  PlayEventMsg.ActivePlayerRender },
                { "TimeStop(F_time)",  PlayEventMsg.TimeStop },
                { "SetNoGravityT(F_time)",  PlayEventMsg.SetNoGravityT },
                { "SetNoBreakTime(F_time)",  PlayEventMsg.SetNoBreakTime },
                { "PlayAudio(id,volume)",  PlayEventMsg.PlayAudio },
            };
        }


        //[NaughtyAttributes.ShowIf(nameof(IsRenderEvent))] 


        private bool IsRenderEvent => msgName == PlayEventMsg.ActivePlayerRender;

        private Renderer[] renders = new Renderer[0];

        private void OnValueChange()
        {
            Debug.Log($"yns  Selet " + msgName);

        }


        protected override void OnTrigger(float timeSinceTrigger)
        {
            if (IsRenderEvent)
            {
                renders = Owner.transform.GetComponentsInChildren<Renderer>();
            }
        }


        protected override void OnUpdateEvent(float timeSinceTrigger)
        {
            if (IsRenderEvent)
            {
                SetRenderActive(boolMsg);
            }
        }

        protected override void OnFinish()
        {
            if (IsRenderEvent)
            {
                if (setOppositeOnFinish)
                {
                    SetRenderActive(!boolMsg);
                }
            }
        }

        protected override void OnStop()
        {
            SetRenderActive(!boolMsg);
        }




        private void SetRenderActive(bool active)
        {

            //Debug.Log($"yns SetRenderActive {active} ");
            foreach (var item in renders)
            {
                item.enabled = active;
            }
        }




    }





}
