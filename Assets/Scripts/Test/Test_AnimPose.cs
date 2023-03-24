using System.Collections.Generic;
using UnityEngine;

namespace XiaoCao
{
     public class Test_AnimPose : MonoBehaviour
    {
        [Range(0,1)]
        [NaughtyAttributes.OnValueChanged("SetValue")]
        public float time;

        public GameObject model;
        public AnimationClip clip;
        [NaughtyAttributes.Button("ResPlayer")]
        public void ResetPlayer()
        {
            if (model != null && clip != null)
            {
                clip.SampleAnimation(model,0);
                model.transform.localPosition = Vector3.zero;
            }
        }

        public void SetValue()
        {

            if(model!=null && clip != null)
            {
                clip.SampleAnimation(model, time * clip.length);
            }

        }
    }
}
