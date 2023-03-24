using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BezierCurve : MonoBehaviour
{
    void OnDrawGizmos()
    {
        Handles.BeginGUI();

        Vector3 start = new Vector3(0, 0, 0);
        Vector3 end = new Vector3(2, 2, 0);
        Vector3 startTangent = new Vector3(1, 0, 0);
        Vector3 endTangent = new Vector3(1, 2, 0);

        Handles.DrawBezier(start, end, startTangent, endTangent, Color.white, null, 2f);

        Handles.EndGUI();
    }
}