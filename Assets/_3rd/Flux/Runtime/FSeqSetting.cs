
using UnityEngine;
using System.Collections.Generic;
using XiaoCao;

namespace Flux
{
    [CreateAssetMenu(fileName = "FSeqSetting", menuName ="FSeqSetting")]
    public class FSeqSetting : ScriptableObject
    {
        public AgentModelType agentName;

        public RuntimeAnimatorController targetAnimtorController;

    }
}