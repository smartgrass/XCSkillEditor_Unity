using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;
namespace NaughtyBezierCurves
{
    public class Test_BezierCurve3D : MonoBehaviour
    {
        [Range(0, 1)]
        public float time;


        public BezierCurve3D bezierCurve3D;
        public float handLenSize = 1;
        public List<Vector3> posList = new List<Vector3>();


        private void CheckLen()
        {
            int needLen = posList.Count;
            int curlen = bezierCurve3D.KeyPoints.Count;

            int dif = needLen - curlen;
            if (dif > 0)
            {
                for (int i = 0; i < dif; i++)
                {
                    bezierCurve3D.AddKeyPoint();
                }
            }
            if (dif < 0)
            {
                for (int i = 0; i < -dif; i++)
                {
                    bezierCurve3D.RemoveKeyPointAt(curlen-i);
                }
            }

        }

        [Button]
        private void SetCurve()
        {
            CheckLen();
            int len = posList.Count;
            for (int i = 0; i < len; i++)
            {
                bezierCurve3D.KeyPoints[i].Position = posList[i];

                if (i == 0)
                {
                    bezierCurve3D.KeyPoints[i].RightHandlePosition = posList[i];
                    bezierCurve3D.KeyPoints[i].LeftHandlePosition = posList[i];
                    continue;
                }
                if (i == len - 1)
                {
                    bezierCurve3D.KeyPoints[i].RightHandlePosition = posList[i];
                    bezierCurve3D.KeyPoints[i].LeftHandlePosition = posList[i];
                    continue;
                }


                Vector3 preVec3 = posList[i - 1] ;
                Vector3 nextVec3 = posList[i + 1];

                Vector3 dir1 = posList[i] - preVec3;
                Vector3 dir2 = nextVec3 - posList[i];



                float dis = Vector3.Distance(preVec3, posList[i]);
                float dis2 = Vector3.Distance(posList[i], nextVec3);

                float minDis = Mathf.Min(dis, dis2);

                Vector3 targetDir = ((dir1* dis2 + dir2* dis) / 2).normalized;
                float handLen = minDis / 50f * handLenSize;
                //float handLen = handLenSize;
                bezierCurve3D.KeyPoints[i].RightHandlePosition = targetDir * handLen + posList[i];
                bezierCurve3D.KeyPoints[i].LeftHandlePosition = targetDir * handLen * -1 + posList[i];
            }
        }

    }
}