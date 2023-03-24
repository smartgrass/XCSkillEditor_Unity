using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Flux
{
    //这个事件好像没有什么用了
    //原用处是: 在指定长度内触发 轨道enable的控制
    //[FEvent("Sequence/InputEvent", typeof(FInputTrack))]
    //public class FInputEvent : FEvent
    //{
    //    [Header("Select")]
    //    public int SelectEvent;

    //    public TrackAble[] TimelineAbles;
    //    //[SerializeField]
    //    //public TrackAble[] trackAbles;

    //    public TrackAbleCache trackCache => Sequence.trackAbleCache;

    //    protected override void OnTrigger(float timeSinceTrigger)
    //    {
    //        if(TimelineAbles.Length > SelectEvent)
    //        {
    //            var item = TimelineAbles[SelectEvent];

    //            foreach (var enables in item.EnableList)
    //            {
    //                SetTimelineActive(enables, true);
    //            }
    //            foreach (var disable in item.DisabelList)
    //            {
    //                SetTimelineActive(disable, false);
    //            }
    //        }
    //    }
    //    protected override void OnStop()
    //    {
    //        if (TimelineAbles.Length > SelectEvent)
    //        {
    //            var item = TimelineAbles[SelectEvent];
    //            foreach (var enables in item.EnableList)
    //            {
    //                SetTimelineActive(enables, false);
    //            }
    //            foreach (var disable in item.DisabelList)
    //            {
    //                SetTimelineActive(disable, true);
    //            }
    //        }
    //    }

    //    private void SetTimelineActive(FTimeline timeline, bool act)
    //    {
    //        if(timeline!= null)
    //        {
    //            foreach (var tra in timeline.Tracks)
    //            {
    //                tra.enabled = act;
    //            }
    //        }
    //    }
    //}

    //[Serializable]
    //public class TrackAble
    //{
    //    public FTimeline[] EnableList;
    //    public FTimeline[] DisabelList;
    //}

    //public class TrackAbleCache
    //{
    //    public Dictionary<FTrack, bool> trackDic = new Dictionary<FTrack, bool>();

    //    public void AddState(FTrack track)
    //    {
    //        if (! trackDic.ContainsKey(track))
    //        {
    //            trackDic.Add(track, track.enabled);
    //        }
    //    }

    //    public void AddStates(List<FTrack> trackList)
    //    {
    //        trackDic = new Dictionary<FTrack, bool>();
    //        foreach (var track in trackList)
    //        {
    //            if (!trackDic.ContainsKey(track))
    //            {
    //                trackDic.Add(track, track.enabled);
    //            }
    //        }
    //    }
    //}

}
