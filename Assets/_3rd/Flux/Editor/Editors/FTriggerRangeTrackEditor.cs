
using Flux;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XiaoCao;
namespace FluxEditor
{
    [FEditor(typeof(FTriggerRangeTrack))]
    public class FTriggerRangeTrackEditor : FTrackEditor
    {
        public List<CubeRange> cubes = new List<CubeRange>();

        Dictionary<CubeRange, Transform> cubeDic = new Dictionary<CubeRange, Transform>();

        //SceneªÊ÷∆
        Test_DrawCube drawCube;

        public override void Init(FObject obj, FEditor owner)
        {
            base.Init(obj, owner);
            drawCube = GameObject.FindObjectOfType<Test_DrawCube>();
            if(drawCube == null)
            {
                drawCube = new GameObject("drawCube").AddComponent<Test_DrawCube>();
            }
        }


        protected override void OnEnable()
        {
            base.OnEnable();
        }


        public override void OnStop()
        {
            base.OnStop();
            drawCube.RemoveAll(Track);
        }

        public override void UpdateEventsEditor(int frame, float time)
        {
            drawCube.Regist(Track);
            base.UpdateEventsEditor(frame, time);
            FEvent[] evts = new FEvent[2];
            int numEvents = Track.GetEventsAt(frame, evts);
            if (numEvents == 0)
            {
                drawCube.RemoveAll(Track);
                return;
            }

            foreach (var item in Track.Events)
            {
                var ev = (FTriggerRangeEvent)item;
                if (item.FrameRange.Contains(frame))
                {

                    drawCube.Add(Track ,ev.cubeRange, ev.Owner);
                }
                else
                {
                    drawCube.Remove(Track,ev.cubeRange);
                }
            }
        }
    }
}
