using UnityEngine;
using System.Collections;
using XiaoCao;
using System.Collections.Generic;

namespace Flux
{
    [FEvent("GamoObject/TriggerRangeEvent", typeof(FTriggerRangeTrack))]
    public class FTriggerRangeEvent : FEvent
    {
        public CubeRange cubeRange;
        public string ackId = "0";
    }
}
