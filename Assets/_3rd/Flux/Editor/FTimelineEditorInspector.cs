using UnityEngine;
using UnityEditor;

using Flux;

namespace FluxEditor
{
    [System.Serializable]
    public class FTimelineEditorInspector : FEditorInspector<FTimelineEditor, FTimeline>
    {

        public override string Title
        {
            get
            {
                if (Editors.Count == 1)
                    return "Timeline:";
                return "Timelines:";
            }
        }

        public override void OnInspectorGUI(float contentWidth)
        {
            base.OnInspectorGUI(contentWidth);
        }
    }
}
