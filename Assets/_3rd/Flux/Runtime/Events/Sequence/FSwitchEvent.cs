//using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using XiaoCao;

namespace Flux
{
    [FEvent("Sequence/FSwitchEvent", typeof(FInputTrack))]
    public class FSwitchEvent : FEvent
    {
        [SerializeField]
        [HideInInspector]
        private int _toFrame;
        public int ToFrame { get => _toFrame; set => _toFrame = value; }
        [SerializeField]
        [HideInInspector]
        private int switchFrame;
        public int SwitchFrame { get => switchFrame; set => switchFrame = value; }       
        [HideInInspector]
        [SerializeField]
        private int unMoveFrames;
        public int UnMoveFrames { get => unMoveFrames; set => unMoveFrames = value; }

        public InputEventType InputType = InputEventType.Finish;

        public KeyCode keyCode;
 
        public override string Text
        {
            get
            {
                if( InputType == InputEventType.Finish)
                {
                    return InputType.ToString() +UnMoveFrames.ToString();
                }
                else
                {
                    return InputType.ToString();
                }
            }
        }


    }

}
