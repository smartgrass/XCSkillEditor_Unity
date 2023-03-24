using UnityEditor;
using UnityEngine;

namespace Assets.Editor
{
    public class EditorAnimDisplay : EditorWindow
    {
        #region init
        private static EditorAnimDisplay instance;
        [MenuItem("Tools/XiaoCao/动画预览窗口")]
        static void PrefabWrapTool()
        {
            //获取当前窗口实例
            instance = EditorWindow.GetWindow<EditorAnimDisplay>();
            instance.Show();
            //ShowUtility() 实体窗口样式
        }
        #endregion

        public AnimationClip[] clips;
        public GameObject player;
        Vector2 pos = Vector2.zero;
        public string Fitter = "";
        public string NoFitter = "";
        
        private AnimationClip curAnimClip;
        private float Timer = 0;
        private int playCount = 0;
        private bool isStop = true;
        //private void OnEnable()
        //{

        //}
        
        void OnGUI()
        {
            var obj = new SerializedObject(new UnityEngine.Object[] { this }, this);
            XiaoCao.XiaoCaoWindow.DrawHeader(obj);

            player = EditorGUILayout.ObjectField("player", player, typeof(GameObject), true) as GameObject;
            Fitter= EditorGUILayout.TextField("包含",Fitter );
            NoFitter = EditorGUILayout.TextField("不包含", NoFitter);
            if (player)
            {
                clips = player.GetComponent<Animator>().runtimeAnimatorController.animationClips;
                pos = GUILayout.BeginScrollView(pos, false, false);
                foreach (var item in clips)
                {

                    if (IsShow(item.name)&&GUILayout.Button(item.name))
                    {
                        //复制
                        GUIUtility.systemCopyBuffer = item.name;
                        PlayAnim(item);
                    }
                }
                GUILayout.EndScrollView();
            }
        }


        private bool IsShow(string clipName)
        {
            bool isInFitter = false;

            if (Fitter.IsEmpty())
            {
                isInFitter = true;
            }
            else
            {
                isInFitter = clipName.ToLower().Contains(Fitter.ToLower());
            }

            bool isNoInFitter = false;
            if (NoFitter.IsEmpty())
            {
                isNoInFitter = true;
            }
            else
            {
                isNoInFitter = ! clipName.ToLower().Contains(NoFitter.ToLower());
            }
            return isInFitter && isNoInFitter;
        }

        private void PlayAnim(AnimationClip clip)
        {
            Timer = 0;
            playCount = 0;
            curAnimClip = clip;
            Selection.activeObject = clip;
            isStop = false;
            //DragAndDrop.objectReferences[0] = clip;
            //Debug.Log("yns  play");
        }

        private void Update()
        {
            //if(Timer<10)
            UpdateAnim(Time.deltaTime);

        }

        private void UpdateAnim(float delta)
        {
            if (curAnimClip != null)
            {

                if (!isStop)
                {
                    Timer += delta;

                    if (Timer > curAnimClip.length)
                    {
                        playCount++;
                        Timer = 0;
                    }

                    if(playCount < 2)
                    {
                        curAnimClip.SampleAnimation(player, Timer);                
                    }
                    else
                    {
                        isStop = true;
                    }
                }


            }
        }


    }


}
