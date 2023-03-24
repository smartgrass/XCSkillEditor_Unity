using UnityEngine;
using UnityEditor;

using Flux;
using System.Collections.Generic;
using XiaoCao;

namespace FluxEditor
{
    [FEditor(typeof(FInputTrack))]
    public class FInputTrackEditor : FTrackEditor
    {


        public override void UpdateEventsEditor(int frame, float time)
        {
            base.UpdateEventsEditor(frame, time);

            FEvent[] evts = new FEvent[2];

            int numEvents = Track.GetEventsAt(frame, evts);
            if (numEvents == 0)
                return;

            var _event0 = evts[0];
            if (!_event0.EnableEvent)
                return;

            if (Track.GetEventType() == typeof(FSwitchEvent))
            {
                var ev = (FSwitchEvent)_event0;
                if (ev.InputType == InputEventType.Exit)
                {
                    FSequenceEditorWindow.instance.GetSequenceEditor().Stop();
                }
                //if (ev.InputType == InputEventType.Switch)
                //{
                //    if( (ev.HasTriggered && ev.End == frame )|| ev.SwitchFrame ==frame)
                //    {
                //        //Debug.Log("yns  switch");
                //        FSequenceEditorWindow.instance.GetSequenceEditor().SetSwitchEvent(ev);
                //    }
                //}
                //else

            }

        }

    }
}
