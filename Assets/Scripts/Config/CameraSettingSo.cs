using UnityEditor;
using UnityEngine;

namespace XiaoCao
{
    [CreateAssetMenu(menuName ="MyAsset/CameraSettingSo")]
    public class CameraSettingSo : ScriptableObject
    {
        public TramsFormData FollowCameraData;
    }


    [System.Serializable]
    public class TramsFormData
    {
        public Vector3 Angle;
        public Vector3 Offset;
        public Vector3 KeepArea;
    }
}