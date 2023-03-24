using UnityEngine;
using UnityEditor;

using Flux;

using FluxEditor;
using System.Collections.Generic;
using XiaoCao;
using UnityEditorInternal;

namespace FluxEditor
{
    [CustomEditor(typeof(FTransformEvent), true)]
    public class FTransformEventInspector : FTweenEventInspector
    {
        public float btnAddY = 0.5f;

        private bool isPos = false;


        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (_tween.isExpanded)
            {
                serializedObject.Update();

                float doubleLineHeight = EditorGUIUtility.singleLineHeight * 2f;
                EditorGUILayout.Separator();
                EditorGUILayout.Separator();
                EditorGUILayout.Separator();
                EditorGUILayout.Separator();
                Rect tweenRect = GUILayoutUtility.GetLastRect();

                tweenRect.yMin = tweenRect.yMax - doubleLineHeight * btnAddY;// - doubleLineHeight*2.5f;
                tweenRect.height = EditorGUIUtility.singleLineHeight;

                tweenRect.xMin = tweenRect.xMax - 80;

                tweenRect.x = tweenRect.xMax - 100;
                if (GUI.Button(tweenRect, "Set To", EditorStyles.miniButton))
                    _to.vector3Value = GetPropertyValue();

                tweenRect.x = tweenRect.xMax - 180;

                if (GUI.Button(tweenRect, "Set From", EditorStyles.miniButton))
                    _from.vector3Value = GetPropertyValue();



                tweenRect.x = tweenRect.xMax - 180;

                DrawOtherBtn(tweenRect);

                EditorGUILayout.Separator();

                serializedObject.ApplyModifiedProperties();
            }
        }


        public virtual void DrawOtherBtn(Rect tweenRect)
        {
        }

        //设置当前值
        public Vector3 GetPropertyValue()
        {
            FTweenPositionEvent positionEvent = target as FTweenPositionEvent;
            if (positionEvent)
            {
                return positionEvent.Owner.localPosition;
            }
            else
            {
                FTransformEvent transformEvt = target as FTransformEvent;
                if (transformEvt is FTweenRotationEvent)
                    return transformEvt.Owner.localRotation.eulerAngles;
                if (transformEvt is FTweenScaleEvent)
                    return transformEvt.Owner.localScale;

            }



            Debug.LogWarning("Unexpected child of FTransformEvent, setting (0,0,0)");
            return Vector3.zero;
        }

    }

    [CustomEditor(typeof(FTweenPositionEvent), true)]
    [CanEditMultipleObjects]
    public class FTweenPositionEventEditor : FTransformEventInspector
    {

        private static Color[] colors = new[] { Color.red, Color.yellow, Color.blue };
        private int colorIndex = 0;
        private int index = 0;

        protected override void OnEnable()
        {
            self = target as FTweenPositionEvent;
            base.OnEnable();
        }

        private FTweenPositionEvent _self;
        private FTweenPositionEvent Self
        {
            get
            {
                if (_self == null)
                {
                    _self = target as FTweenPositionEvent;
                }
                return _self;
            }
        }
        private Quaternion rotation => Self.transform.rotation;

        public override void DrawOtherBtn(Rect tweenRect)
        {
            if (GUI.Button(tweenRect, "SetHandle", EditorStyles.miniButton))
            {
                SerializedProperty _handlePoint = _tween.FindPropertyRelative("_handlePoint");
                _handlePoint.vector3Value = (_to.vector3Value+ _from.vector3Value)/2;
            }

            tweenRect.x = tweenRect.xMax - 180;
            if (GUI.Button(tweenRect, "对齐前", EditorStyles.miniButton))
            {
                var track = Self.Track;
                int length = track.Events.Count;
                for (int i = 0; i < length; i++)
                {
                    if (track.Events[i].gameObject == Self.gameObject)
                    {
                        if (i - 1>=0)
                        {
                            _from.vector3Value = (track.Events[i - 1] as FTweenPositionEvent).To;
                        }
                    }              
                }
            }
            tweenRect.x = tweenRect.xMax - 180;
            if (GUI.Button(tweenRect, "对齐后", EditorStyles.miniButton))
            {
                var track = Self.Track;
                int length = track.Events.Count;
                for (int i = 0; i < length; i++)
                {
                    if (track.Events[i].gameObject == Self.gameObject)
                    {
                        if (i + 1< length)
                        {
                            _to.vector3Value = (track.Events[i + 1] as FTweenPositionEvent).From;
                        }
                    }
                }
            }

        }

        public void ShowSceneGUI(int index = 0)
        {
            this.index = index;
            colorIndex = index % 3;
            OnSceneGUI();
        }

        void OnSceneGUI()
        {
            Handles.color = colors[colorIndex];
            if (Self.IsBezier)
            {
                DrawBezier();  
            }
            else
            {
                Handles.DrawLine(Self.From, Self.To);
            }
            DrawStartEnd();
        }

        private void DrawBezier()
        {
            int len = 20;
            List<Vector3> points = new List<Vector3>();
            var _handlePoint = Self.HandlePoint;
            for (int i = 0; i < len; i++)
            {
                float t = 1f / 20 * (i + 1);
                var point = MathTool.GetBezierPoint2(Self.From, Self.To, _handlePoint, t);
                points.Add(point);
            }
            Handles.DrawLines(points.ToArray());

            Vector3 newPointPosition = Handles.DoPositionHandle(_handlePoint, rotation);
            if (_handlePoint != newPointPosition)
            {
                Self.HandlePoint = newPointPosition;
            }
            Handles.Label(newPointPosition + new Vector3(0f, HandleUtility.GetHandleSize(newPointPosition) * 0.4f, 0f), "handle");

            //if (Handles.Button(newPointPosition, rotation, 4, 4, Handles.DotHandleCap))
            //{

            //}
        }

        private void DrawStartEnd()
        {
            Vector3 pos = Self.From;

            Vector3 newPointPosition = DrawPos(pos, false);

            if (pos != newPointPosition)
            {
                Self.From = newPointPosition;
            }

            Vector3 posEnd = Self.To;

            Vector3 newPointPosEnd = DrawPos(posEnd, true);

            if (pos != newPointPosEnd)
            {
                Self.To = newPointPosEnd;
            }
        }

        private Vector3 DrawPos(Vector3 pos, bool isTo)
        {
            int num = index * 2;
            num = isTo ? num + 1 : num;
            string label = num.ToString();
            Handles.Label(pos + new Vector3(0f, HandleUtility.GetHandleSize(pos) * 0.4f, 0f), label);

            // Draw the center of the control point
            Handles.CapFunction handle = Handles.SphereHandleCap;
            if (isTo)
            {
                handle = Handles.CubeHandleCap;
            }
            Handles.FreeMoveHandle(pos, rotation, HandleUtility.GetHandleSize(pos) * 0.15f, Vector3.one, handle);


            Vector3 newPointPosition = Handles.DoPositionHandle(pos, rotation);
            return newPointPosition;
        }
    }

    //[CustomPropertyDrawer(typeof(Flux.FTweenVector3))]
    public class FTweenVector3Drawer : PropertyDrawer
    {
        private const int PREFIX_WIDTH = 15;
        private const int ELEMENT_SPACE = 10;
        private float startMinx = 0;
        private float StartOffset => 1.5f * startMinx;
             

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty _to = property.FindPropertyRelative("_to");
            SerializedProperty _from = property.FindPropertyRelative("_from");
            SerializedProperty _easingType = property.FindPropertyRelative("_easingType");

            //this.fieldInfo.FieldType

            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
            //Rect tweenRect = GUILayoutUtility.GetLastRect();


            Rect r = position;
            startMinx = position.xMin;

            r.y = position.y + EditorGUIUtility.standardVerticalSpacing;

            EditorGUI.LabelField(r, "FTweenVector3");

            r = r.GetNextLine();
            r.xMin = StartOffset;
            EditorGUI.PropertyField(r, _easingType);

            r = r.GetNextLine();

            r = DrawVec3(_to, r, "to");


            r = r.GetNextLine();
            r = DrawVec3(_from, r, "from");


            //r.position += Vector2.up * ELEMENT_SPACE;
            //EditorGUI.Vector3Field(r, "_from", _from.vector3Value);
            //EditorGUI.PropertyField(position, property, _label);
            //base.OnGUI(position, property, label);

        }

        private Rect DrawVec3(SerializedProperty _to, Rect r, string label)
        {
            float startOffset = startMinx * 1.5f;
            r.xMin = startOffset;
            r.width = EditorGUIUtility.currentViewWidth * 0.7f;
            EditorGUI.LabelField(r, label);
            r.xMin = EditorGUIUtility.labelWidth * 0.4f;
            EditorGUI.Vector3Field(r, "", _to.vector3Value);

            r.xMin = EditorGUIUtility.currentViewWidth * 0.725f + startOffset;
            r.xMax = r.xMin+ EditorGUIUtility.currentViewWidth * 0.18f;
            if (GUI.Button(r, "set"))
            {
                //ComponentUtility.PasteComponentAsNew
                //EditorUtility.CopySerializedManagedFieldsOnly()
            }
            return r;
        }
    }

}
