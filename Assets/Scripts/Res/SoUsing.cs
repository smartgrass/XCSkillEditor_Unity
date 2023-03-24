using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace XiaoCao
{

    public class SoUsing : MonoBehaviour
    {
        public CameraSettingSo CameraSetting;

        public DebugSo DebugSo;

        public PlayerMoveSettingSo PlayerMoveSettingSo;

        public SkiillKeyCodeSo SkiillKeyCodeSo;

        [NaughtyAttributes.Button()]
        private void FindOrCreat()
        {
#if UNITY_EDITOR
            var fields = this.GetType().GetFields();
            fields.IELogStr();
            foreach (var item in fields)
            {
                Type subType = item.FieldType;
                var value = item.GetValue(this);
                if (value == null && subType.IsSubclassOf(typeof(ScriptableObject)))
                {
                    ScriptableObject newObj = ScriptableObject.CreateInstance(subType.Name);
                    item.SetValue(this,newObj);
                    AssetDatabase.CreateAsset(newObj, "Assets/Resources/ResUsing/" + subType.Name+".asset");
                    AssetDatabase.Refresh();

                   //var loadobj =  AssetDatabase.LoadAssetAtPath<ScriptableObject>("Assets/Resources/ResUsing/" + subType.Name + ".asset");
                   // Debug.Log($"yns ? {newObj.GetInstanceID() == loadobj.GetInstanceID()}");
                }
            }
#endif
        }


    }


}
