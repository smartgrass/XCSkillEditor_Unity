using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;

using Flux;
using XiaoCao;

namespace FluxEditor
{
    [CustomEditor(typeof(FSwitchEvent))]
    public class FSwitchEventInspector : FEventInspector
    {
        FSwitchEvent targetEvent;

        private SerializedProperty _toFrame = null;
        private SerializedProperty _swFrame = null;
        private SerializedProperty _unMoveFrames = null;

        protected override void OnEnable()
        {
            base.OnEnable();
            targetEvent = target as FSwitchEvent;

            _toFrame = serializedObject.FindProperty("_toFrame");
            _swFrame = serializedObject.FindProperty("switchFrame");
            _unMoveFrames = serializedObject.FindProperty("unMoveFrames");
        }



        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();
            if (targetEvent)
            {
                switch (targetEvent.InputType)
                {
                    //case InputEventType.Switch:
                    //    EditorGUILayout.PropertyField(_toFrame,new GUIContent("Ä¿µÄÇÐ»»Ö¡"));
                    //    EditorGUILayout.PropertyField(_swFrame,new GUIContent("´¥·¢Ö¡"));
                    //    break;
                    case InputEventType.Exit:
                        break;
                    case InputEventType.Finish:

                        EditorGUILayout.PropertyField(_unMoveFrames, new GUIContent("½ûÖ¹ÒÆ¶¯Ö¡Êý"));
                        break;
                    default:
                        break;
                }
            }
            //Repaint();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
