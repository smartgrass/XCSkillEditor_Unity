using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace XiaoCao
{
    public static class XiaocaoEditorTool
    {
        public static void SelectSelf(this UnityEngine.Object self)
        {
            Selection.activeObject = self;
        }

        public static Object[] ToObjectArray(this IEnumerable<UnityEngine.Object> list)
        {
            return list.ToArray();
        }


        public static List<Object> FindAssetListByName(string nameStr, string TypeName, string path = "Assets")
        {
            List<Object> objList = new List<Object>();
            string[] guids = AssetDatabase.FindAssets($"{nameStr} t:{TypeName}", new string[] { path });
            List<string> paths = new List<string>();
            new List<string>(guids).ForEach(m => paths.Add(AssetDatabase.GUIDToAssetPath(m)));
            for (int i = 0; i < paths.Count; i++)
            {
                objList.Add(AssetDatabase.LoadAssetAtPath(paths[i], typeof(Object)));
            }
            return objList;
        }

        public static Object FindAssetByName(string nameStr, string TypeName, string path = "Assets")
        {
            Object obj = null;
            string[] guids = AssetDatabase.FindAssets($"{nameStr} t:{TypeName}", new string[] { path });
            List<string> paths = new List<string>();
            new List<string>(guids).ForEach(m => paths.Add(AssetDatabase.GUIDToAssetPath(m)));
            if (paths.Count > 0)
                obj = AssetDatabase.LoadAssetAtPath(paths[0], typeof(Object));
            return obj;
        }

        public static List<Object> FindAssetsByDir(string TypeName, string path = "Assets")
        {
            List<Object> objList = new List<Object>();
            string[] guids = AssetDatabase.FindAssets($"t:{TypeName}", new string[] { path });
            List<string> paths = new List<string>();
            new List<string>(guids).ForEach(m => paths.Add(AssetDatabase.GUIDToAssetPath(m)));
            for (int i = 0; i < paths.Count; i++)
            {
                objList.Add(AssetDatabase.LoadAssetAtPath(paths[i], typeof(Object)));
            }
            return objList;
        }

    }



    public static class XCEditorGUI
    {
        //竖直间隔
        public static float YSpance => EditorGUIUtility.standardVerticalSpacing;
        //宽度
        public static float CurWidth => EditorGUIUtility.currentViewWidth;
        //单行高度
        public static float OneLineHeight => EditorGUIUtility.singleLineHeight;

        public static Rect GetNextLine(this Rect rect)
        {
            EditorGUILayout.Separator();
            rect.y +=OneLineHeight*1.28f;
            return rect;
        }
        //获取默认高度
        //PropertyDrawer
        //base.GetPropertyHeight(property, label);
    }


    public static class XiaocaoEditorAnimTool
    {
        //添加一个Clip到动画机
        public static bool AddClipToAnimator(AnimatorController ac, AnimationClip item)
        {
            if (!ac.animationClips.Contains(item))
            {
                AnimatorStateMachine sm = ac.layers[0].stateMachine;
                AnimatorState state = sm.AddState(item.name, sm.exitPosition + Vector3.up * Random.Range(100, 800));
                state.motion = item;
                state.AddExitTransition(true);
                Debug.Log($"anim add {item.name}");
                EditorUtility.SetDirty(ac);
                return true;
            }
            else
            {
                Debug.Log($"yns has {item.name}");
                return false;
            }
        }


        [MenuItem("Assets/AnimatorTool/移除所有连线")]
        public static void RemoveAllTransmiss()
        {
            OnRemoveAllTransmiss(Selection.activeObject as AnimatorController);
        }
        public static void OnRemoveAllTransmiss(AnimatorController ac)
        {
            var stateMachine = ac.layers[0].stateMachine;
            foreach (var item in stateMachine.entryTransitions)
            {

                Debug.Log("yns  remove " + item.destinationState);
                stateMachine.RemoveEntryTransition(item);
            }
            foreach (var item in stateMachine.anyStateTransitions)
            {
                Debug.Log("yns  remove " + item.destinationState);
                stateMachine.RemoveAnyStateTransition(item);
            }

            foreach (var item in ac.layers[0].stateMachine.states)
            {
                Debug.Log("yns  states " + item.state.ToString());
                foreach (var tr in item.state.transitions)
                {
                    item.state.RemoveTransition(tr);
                }
            }

            EditorUtility.SetDirty(ac);
            AssetDatabase.SaveAssets();

        }

    }
}
