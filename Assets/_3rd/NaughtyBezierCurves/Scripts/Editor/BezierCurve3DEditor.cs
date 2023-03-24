using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace NaughtyBezierCurves.Editor
{
    [CustomEditor(typeof(BezierCurve3D))]
    [CanEditMultipleObjects]
    public class BezierCurve3DEditor : UnityEditor.Editor
    {
        private const float AddButtonWidth = 80f;
        private const float RemoveButtonWidth = 19f;

        private BezierCurve3D curve;
        private ReorderableList keyPoints;
        private bool showPoints = true;

        [MenuItem("GameObject/Create Other/Naughty Bezier Curve")]
        private static void CreateBezeirCurve()
        {
            BezierCurve3D curve = new GameObject("Bezier Curve", typeof(BezierCurve3D)).GetComponent<BezierCurve3D>();
            Vector3 position = Vector3.zero;
            if (Camera.current != null)
            {
                position = Camera.current.transform.position + Camera.current.transform.forward * 10f;
            }

            curve.transform.position = position;

            BezierCurve3DEditor.AddDefaultPoints(curve);

            Undo.RegisterCreatedObjectUndo(curve.gameObject, "Create Curve");

            Selection.activeGameObject = curve.gameObject;
        }

        private static void AddDefaultPoints(BezierCurve3D curve)
        {
            BezierPoint3D startPoint = curve.AddKeyPoint();
            startPoint.LocalPosition = new Vector3(-1f, 0f, 0f);
            startPoint.LeftHandleLocalPosition = new Vector3(-0.35f, -0.35f, 0f);

            BezierPoint3D endPoint = curve.AddKeyPoint();
            endPoint.LocalPosition = new Vector3(1f, 0f, 0f);
            endPoint.LeftHandleLocalPosition = new Vector3(-0.35f, 0.35f, 0f);
        }

        protected virtual void OnEnable()
        {
            this.curve = (BezierCurve3D)this.target;
            if (curve.KeyPointsCount < 2)
            {
                while (curve.KeyPointsCount != 0)
                {
                    curve.RemoveKeyPointAt(this.curve.KeyPointsCount - 1);
                }

                BezierCurve3DEditor.AddDefaultPoints(this.curve);
            }

            this.keyPoints = new ReorderableList(this.serializedObject, serializedObject.FindProperty("keyPoints"), true, true, false, false);
            this.keyPoints.drawElementCallback = this.DrawElementCallback;
            this.keyPoints.drawHeaderCallback =
                (Rect rect) =>
                {
                    EditorGUI.LabelField(rect, string.Format("Points: {0}", this.keyPoints.serializedProperty.arraySize), EditorStyles.boldLabel);
                };
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            this.serializedObject.Update();

            if (GUILayout.Button("Log Length"))
            {
                Debug.Log(this.curve.GetApproximateLength());
            }

            this.showPoints = EditorGUILayout.Foldout(this.showPoints, "Key Points");
            if (this.showPoints)
            {
                if (GUILayout.Button("Add Point"))
                {
                    AddKeyPointAt(this.curve, this.curve.KeyPointsCount);
                }

                if (GUILayout.Button("Add Point and Select"))
                {
                    var point = AddKeyPointAt(this.curve, this.curve.KeyPointsCount);
                    Selection.activeGameObject = point.gameObject;
                }

                this.keyPoints.DoLayoutList();
            }

            this.serializedObject.ApplyModifiedProperties();
        }

        protected virtual void OnSceneGUI()
        {
            BezierCurve3DEditor.DrawPointsSceneGUI(this.curve);
        }

        private void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = this.keyPoints.serializedProperty.GetArrayElementAtIndex(index);
            rect.y += 2;

            // Draw "Add Before" button
            if (GUI.Button(new Rect(rect.x, rect.y, AddButtonWidth, EditorGUIUtility.singleLineHeight), new GUIContent("Add Before")))
            {
                AddKeyPointAt(this.curve, index);
            }

            // Draw point name
            EditorGUI.PropertyField(
                new Rect(rect.x + AddButtonWidth + 5f, rect.y, rect.width - AddButtonWidth * 2f - 35f, EditorGUIUtility.singleLineHeight), element, GUIContent.none);

            // Draw "Add After" button
            if (GUI.Button(new Rect(rect.width - AddButtonWidth + 8f, rect.y, AddButtonWidth, EditorGUIUtility.singleLineHeight), new GUIContent("Add After")))
            {
                AddKeyPointAt(this.curve, index + 1);
            }

            // Draw remove button
            if (this.curve.KeyPointsCount > 2)
            {
                if (GUI.Button(new Rect(rect.width + 14f, rect.y, RemoveButtonWidth, EditorGUIUtility.singleLineHeight), new GUIContent("x")))
                {
                    RemoveKeyPointAt(this.curve, index);
                }
            }
        }

        public static void DrawPointsSceneGUI(BezierCurve3D curve, BezierPoint3D exclude = null)
        {
            for (int i = 0; i < curve.KeyPointsCount; i++)
            {
                if (curve.KeyPoints[i] == exclude)
                {
                    continue;
                }

                BezierPoint3DEditor.handleCapSize = BezierPoint3DEditor.CircleCapSize;
                BezierPoint3DEditor.DrawPointSceneGUI(curve.KeyPoints[i]);
            }
        }

        private static void RenamePoints(BezierCurve3D curve)
        {
            for (int i = 0; i < curve.KeyPointsCount; i++)
            {
                curve.KeyPoints[i].name = "Point " + i;
            }
        }

        private static BezierPoint3D AddKeyPointAt(BezierCurve3D curve, int index)
        {
            BezierPoint3D newPoint = new GameObject("Point " + curve.KeyPointsCount, typeof(BezierPoint3D)).GetComponent<BezierPoint3D>();
            newPoint.transform.parent = curve.transform;
            newPoint.transform.localRotation = Quaternion.identity;
            newPoint.Curve = curve;

            if (curve.KeyPointsCount == 0 || curve.KeyPointsCount == 1)
            {
                newPoint.LocalPosition = Vector3.zero;
            }
            else
            {
                if (index == 0)
                {
                    newPoint.Position = (curve.KeyPoints[0].Position - curve.KeyPoints[1].Position).normalized + curve.KeyPoints[0].Position;
                }
                else if (index == curve.KeyPointsCount)
                {
                    newPoint.Position = (curve.KeyPoints[index - 1].Position - curve.KeyPoints[index - 2].Position).normalized + curve.KeyPoints[index - 1].Position;
                }
                else
                {
                    newPoint.Position = BezierCurve3D.GetPointOnCubicCurve(0.5f, curve.KeyPoints[index - 1], curve.KeyPoints[index]);
                }
            }

            Undo.IncrementCurrentGroup();
            Undo.RegisterCreatedObjectUndo(newPoint.gameObject, "Create Point");
            Undo.RegisterCompleteObjectUndo(curve, "Save Curve");

            curve.KeyPoints.Insert(index, newPoint);
            RenamePoints(curve);

            //Undo.RegisterCompleteObjectUndo(curve, "Save Curve");

            return newPoint;
        }

        private static bool RemoveKeyPointAt(BezierCurve3D curve, int index)
        {
            if (curve.KeyPointsCount < 2)
            {
                return false;
            }

            var point = curve.KeyPoints[index];

            Undo.IncrementCurrentGroup();
            Undo.RegisterCompleteObjectUndo(curve, "Save Curve");

            curve.KeyPoints.RemoveAt(index);
            RenamePoints(curve);

            //Undo.RegisterCompleteObjectUndo(curve, "Save Curve");
            Undo.DestroyObjectImmediate(point.gameObject);

            return true;
        }
    }
}
