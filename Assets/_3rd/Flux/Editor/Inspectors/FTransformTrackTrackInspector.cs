using Flux;
using System.Numerics;
using UnityEditor;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace FluxEditor
{
    [CustomEditor(typeof(FTransformTrack))]
    public class FTransformTrackTrackInspector : FTrackInspector
    {
        public Vector3 addVec3;
        public int startIndex = 0;

        private FTransformTrack track;

        private bool isPos = false;

        public override void OnEnable()
        {
            base.OnEnable();
            track = (FTransformTrack)target;
            isPos = track.GetEventType() == typeof(FTweenPositionEvent);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();


            addVec3 = EditorGUILayout.Vector3Field("addVec3", addVec3);
            startIndex = EditorGUILayout.IntField("startIndex", startIndex);

            if (GUILayout.Button("AddVec3"))
            {
                int i = 0;
                foreach (var item in track.Events)
                {
                    if (i >= startIndex)
                    {
                        FTweenPositionEvent fe = (item as FTweenPositionEvent);
                        fe.Tween.From += addVec3;
                        fe.Tween.To += addVec3;
                        i++;
                    }
                }
            }
            GUILayout.Space(10);
        }

        //private FTweenPositionEventEditor _tmpEditor;
        private void OnSceneGUI()
        {
            if (isPos)
            {
                int len = track.Events.Count;
                for (int i = 0; i < len; i++)
                {
                    FTweenPositionEventEditor _tmpEditor = Editor.CreateEditor(track.Events[i]) as FTweenPositionEventEditor;
                    _tmpEditor.ShowSceneGUI(i);
                }
            }

        }
    }
}