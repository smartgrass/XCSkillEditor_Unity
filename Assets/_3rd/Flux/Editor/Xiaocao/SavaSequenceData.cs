
using System.Collections.Generic;
using Flux;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using XiaoCao;
using System.Linq;
using System;
using Random = UnityEngine.Random;
using DG.Tweening;
using Object = UnityEngine.Object;

namespace FluxEditor
{
    public class SavaSequenceData
    {
        public static AgentModelType curAgentName;
        private static List<FPlayAnimationEvent> tmpSeqFAnimEvents = new List<FPlayAnimationEvent>();

        public static void GetData()
        {
            var editor = FSequenceEditorWindow.instance.GetSequenceEditor();
            FSequence Sequence = editor.Sequence;
            SavaOneSeq(Sequence);
            AssetDatabase.SaveAssets();     //保存改动的资源
            AssetDatabase.Refresh();
        }


        [MenuItem("GameObject/XiaoCao/保存选中Seq技能", priority = 0)]
        private static void SavaSelectSeq()
        {
            foreach (var item in Selection.objects)
            {
                var seq = (item as GameObject).GetComponent<FSequence>();
                if (seq)
                {
                    SavaOneSeq(seq);
                }
            }
            AssetDatabase.SaveAssets();     //保存改动的资源
            AssetDatabase.Refresh();
        }
        [MenuItem("GameObject/XiaoCao/选中预制体", priority = 0)]
        private static void SelectPrefab()
        {
            List<Object> objs = new List<Object>();
            foreach (var item in Selection.objects)
            {
               var path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(item);
                var obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (obj!=null)
                {
                    objs.Add(obj);
                }
            }
            Selection.objects = objs.ToArray();
        }
        [MenuItem("GameObject/XiaoCao/收集选中Seq技能Anim", priority = 0)]
        private static void MoveNeedAnim()
        {
            foreach (var item in Selection.objects)
            {
                var seq = (item as GameObject).GetComponent<FSequence>();
                if (seq)
                {
                    CheckMoveAnim(seq);
                }
            }
            AssetDatabase.SaveAssets();     //保存改动的资源
            AssetDatabase.Refresh();
        }

        private static void CheckMoveAnim(FSequence sequence)
        {
            AnimatorController ac = sequence.FSeqSetting.targetAnimtorController as AnimatorController;
            tmpSeqFAnimEvents.Clear();
            var _track = sequence.Containers[0].Timelines[0].Tracks[0];
            
            if (_track is FAnimationTrack)
            {
                foreach (var ev in _track.Events)
                {
                    var fEvent = ev as FPlayAnimationEvent;
                    tmpSeqFAnimEvents.Add(fEvent);
                }
            }
            
            foreach (var item in tmpSeqFAnimEvents)
            {

                string path =AssetDatabase.GetAssetPath(item._animationClip);
                path.LogStr();
                EditorAssetExtend.MoveToDir(path, "Assets/_Res/Anim/Using");

                //}
            }
        }

        private static void SavaOneSeq(FSequence Sequence)
        {
            if (Sequence.FSeqSetting == null)
            {
                Debug.LogError("yns FSeqSetting null");
                return;
            }

            Debug.Log($"yns Sava Seq {Sequence.name}");
            tmpSeqFAnimEvents.Clear();
            SavaSequnce(Sequence, Sequence.SkillId);
            CheckAddAnim(Sequence);
        }


        private static void CheckAddAnim(FSequence sequence)
        {
            AnimatorController ac = sequence.FSeqSetting.targetAnimtorController as AnimatorController;
            if (ac == null)
            {
                Debug.LogError($"yns FSeqSetting.targetAnimtorController is null ");
            }

            bool ischage = false;


            Dictionary<string, float> otherSpeedDic = new Dictionary<string, float>();
            AnimatorStateMachine sm = ac.layers[0].stateMachine;

            foreach (var item in tmpSeqFAnimEvents)
            {
                if (!ac.animationClips.Contains(item._animationClip))
                {
                    AnimatorState state = sm.AddState(item._animationClip.name, sm.exitPosition + Vector3.up * Random.Range(100, 800));
                    //state.name = item.name;
                    state.motion = item._animationClip;
                    state.speed = item._speed;
                    if (state.tag == "NoExit")
                    {

                    }else
                    {
                        state.AddExitTransition(true);
                    }
                   
                    ischage = true;
                    Debug.Log($"anim add {item._animationClip.name}");
                }
                else
                {
                    //检查state速度
                    otherSpeedDic[item._animationClip.name] = item._speed;
                }
            }

            foreach (var item in sm.states)
            {
                if (otherSpeedDic.ContainsKey(item.state.name))
                {
                    var speed = otherSpeedDic[item.state.name];
                    if (!Mathf.Approximately(item.state.speed, speed))
                    {
                        Debug.Log("Speed Change");
                        item.state.speed = speed;
                        ischage = true;
                    }
                }
            }

            if (ischage)
            {
                Debug.Log($"anim controller Sava  ");
                //AssetDatabase.ForceReserializeAssets(new[] { path });
                EditorUtility.SetDirty(ac);
            }
        }


        [MenuItem("Assets/AnimatorTool/Log选择中技能的动画名")]
        private static void CheckSkillDataAnim()
        {
            List<SkillEventData> list = new List<SkillEventData>();
            foreach (var item in Selection.objects)
            {
                var date = (item as GameObject).GetComponent<SkillEventData>();
                if (date!=null)
                {
                    list.Add(date);
                }
            }
            HashSet<string> nameSet = new HashSet<string>();
            foreach (var item in list)
            {
                foreach (var sub in item.AnimEvents.Events)
                {
                    if (!nameSet.Contains(sub.eName))
                    {
                        nameSet.Add(sub.eName);
                    }
                }
            }
            nameSet.IELogStr("动画");
        }

        private static void SavaSequnce(FSequence Sequence, string skillId)
        {
            string skillName = skillId;
            if (string.IsNullOrEmpty(skillId))
            {
                Debug.LogError("yns skillId empty!");
                return;
            }


            float speed = Sequence.Speed;
            int objIndex = 0;
            Transform playerTF = Sequence.Containers[0].Timelines[0]._owner;

            List<SkillEventData> skillEvents = new List<SkillEventData>();
            foreach (var _timeline in Sequence.Containers[0].Timelines)
            {
                GameObject temp;
                if (objIndex > 0)
                    skillName = skillId + "_" + objIndex;
                temp = new GameObject(skillName);
                SkillEventData skillData = temp.AddComponent<SkillEventData>();
                skillData.skillName = skillName;
                skillData.skillId = skillId;
                skillData.speed = speed;
                foreach (var _track in _timeline.Tracks)
                {
                    //对于disactive的轨道不保存
                    if (_track.enabled)
                        ReadTrack(skillData, _track, playerTF);
                }
                skillEvents.Add(skillData);
                objIndex++;
            }

            List<SkillEventData> skillObjs = skillEvents;

            int count = skillObjs.Count;
            skillObjs[0].subSkillData.Clear();
            for (int i = 1; i < count; i++)
            {
                skillObjs[0].subSkillData.Add(skillObjs[i]);
                skillObjs[i].transform.SetParent(skillObjs[0].transform);
            }

            curAgentName = Sequence.FSeqSetting.agentName;
            string resDir = "Resources/" + curAgentName.GetSkillPath();
            resDir.LogStr("Sava");
            FileTool.CheckDirOrCreat(Application.dataPath+ "/" + resDir);
            string prefabPath = "Assets/" + resDir + skillObjs[0].skillName + ".Prefab";
            GameObject game = PrefabUtility.SaveAsPrefabAsset(skillObjs[0].gameObject, prefabPath);//保存
            GameObject.DestroyImmediate(skillObjs[0].gameObject);
            EditorGUIUtility.PingObject(game);
        }

        //获取已保存的动画名
        public static List<string> GetAllAnimName()
        {
            var skills = Resources.LoadAll<SkillEventData>("SkillData");
            List<string> nameList = new List<string>();
            foreach (var item in skills)
            {
                foreach (var anim in item.AnimEvents.Events)
                {
                    if (!nameList.Contains(anim.eName))
                    {
                        nameList.Add(anim.eName);
                    }
                }
            }
            string[] AddNameList = { AnimNameStr.RollTree, AnimNameStr.Break }; //额外添加
            foreach (var item in AddNameList)
            {
                if (!nameList.Contains(item))
                {
                    nameList.Add(item); //额外添加
                }
            }
            nameList.IELogStr();
            return nameList;
        }

        //playerTF 作为参考坐标
        private static SkillEventData ReadTrack(SkillEventData res, FTrack _track, Transform playerTF)
        {
            Debug.Log("yns  " + _track.GetEventType());
            var trackType = _track.GetEventType();
            if (trackType == typeof(FPlayAnimationEvent))
            {
                res.AnimEvents.Events.Clear();
                foreach (var ev in _track.Events)
                {
                    var fEvent = ev as FPlayAnimationEvent;
                    tmpSeqFAnimEvents.Add(fEvent);
                    var xce = ToXCAnimEvent(fEvent);
                    //xce.speed = res.speed;
                    res.AnimEvents.Events.Add(xce);
                }
            }
            else if (trackType == typeof(FTweenPositionEvent))
            {
                res.MoveEvents.Events.Clear();
                FTweenPositionEvent lastEvent = null;
                foreach (var ev in _track.Events)
                {
                    FTweenPositionEvent fEvent = ev as FTweenPositionEvent;
                    var xce = new XCMoveEvent();
                    SetLineEventTween_Ex(xce, fEvent);
                    if (lastEvent != null)
                    {
                        //位移事件之间的间隙
                        xce.startDetal = xce.startVec - lastEvent.Tween.To;
                    }
                    res.MoveEvents.Events.Add(xce);
                    lastEvent = fEvent;
                }
            }
            else if (trackType == typeof(FTweenScaleEvent))
            {
                res.ScaleEvents.Events.Clear();
                foreach (var ev in _track.Events)
                {
                    var fEvent = ev as FTweenScaleEvent;
                    var xce = new XCScaleEvent();
                    SetLineEventTween(xce, fEvent);
                    res.ScaleEvents.Events.Add(xce);
                }
            }           
            else if (trackType == typeof(FTweenRotationEvent))
            {
                res.RotateEvents.Events.Clear();
                foreach (var ev in _track.Events)
                {
                    var fEvent = ev as FTweenRotationEvent;
                    var xce = new XCRotateEvent();
                    SetLineEventTween(xce, fEvent);
                    res.RotateEvents.Events.Add(xce);
                }
            }
            else if (trackType == typeof(FPlayParticleEvent))
            {
                if (_track.Events.Count == 1)
                {
                    //ObjEvent 一个track默认只有一个
                    var fEvent = _track.Events[0] as FPlayParticleEvent;
                    var xce = TOXCObjEvent(fEvent, playerTF);
                    xce.isEffect = true;
                    res.ObjEvent = xce;
                }
                else
                {
                    Debug.LogError($"yns _track.Events.Count? {_track.Events.Count}");
                }
            }
            else if (trackType == typeof(FObjectEvent))
            {
                if (_track.Events.Count == 1)
                {
                    //ObjEvent 一个track默认只有一个
                    var fEvent = _track.Events[0] as FObjectEvent;
                    var xce = TOXCObjEvent(fEvent, playerTF);
                    res.ObjEvent = xce;
                }
                else
                {
                    Debug.LogError($"yns _track.Events.Count? {_track.Events.Count}");
                }
            }
            else if (trackType == typeof(FSwitchEvent))
            {
                foreach (var ev in _track.Events)
                {
                    var fEvent = ev as FSwitchEvent;
                    var xce = TOXCSwitchEvent(fEvent);
                    res.SwitchEvents.Events.Add(xce);
                }
            }
            else if (trackType == typeof(FPlayMsgEvent))
            {
                foreach (var ev in _track.Events)
                {
                    var fEvent = ev as FPlayMsgEvent;
                    var xce = TOXCMsgEvent(fEvent);
                    res.MsgEvents.Events.Add(xce);
                }
            }
            else if (trackType == typeof(FTriggerRangeEvent))
            {
                res.TriggerEvents.Events.Clear();
                foreach (var ev in _track.Events)
                {
                    var fEvent = ev as FTriggerRangeEvent;
                    var xce = TOXCTriggerEvent(fEvent);
                    res.TriggerEvents.Events.Add(xce);
                }
            }
            else
            {
                Debug.Log($"yns  no type " + trackType);
            }


            return res;
        }
        public static XCTriggerEvent TOXCTriggerEvent(FTriggerRangeEvent fe)
        {
            var xce = new XCTriggerEvent();
            xce.range = new XCRange(fe.Start, fe.End);
            xce.isLocalTrueOnly = fe.isLocalTrueOnly;
            xce.cubeRange = fe.cubeRange;
            return xce;
        }
        public static XCSwitchEvent TOXCSwitchEvent(FSwitchEvent fe)
        {
            var xce = new XCSwitchEvent();
            xce.range = new XCRange(fe.Start, fe.End);
            xce.isLocalTrueOnly = fe.isLocalTrueOnly;
            xce.ToFrame = fe.ToFrame;
            xce.UnMoveTime = fe.UnMoveFrames * XCSetting.FramePerSec;
            xce.InputType = fe.InputType;
            xce.keyCode = fe.keyCode;
            return xce;
        }
        public static XCMsgEvent TOXCMsgEvent(FPlayMsgEvent fe)
        {
            var xce = new XCMsgEvent();
            xce.isLocalTrueOnly = fe.isLocalTrueOnly;
            xce.range = new XCRange(fe.Start, fe.End);
            xce.msgEType = fe.msgType;
            xce.msgName = fe.msgName.ToString();
            xce.boolMsg = fe.boolMsg;
            xce.setOppositeOnFinish = fe.setOppositeOnFinish;
            xce.strMsg = fe.strMsg;
            xce.floatdMsg = fe.floatMsg;
            return xce;
        }

        public static XCObjEvent TOXCObjEvent(FEvent fe, Transform playerTF)
        {
            XCObjEvent xce = new XCObjEvent();
            xce.range = new XCRange(fe.Start, fe.End);
            xce.isLocalTrueOnly = fe.isLocalTrueOnly;
            string path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(fe.Owner);

            if (path == "")
            {
                Debug.LogError("yns  path null ");
                path = "SkillEffet/Using/Sword_1";
            }
            else
            {
                if (!path.StartsWith("Assets/Resources"))
                {
                    string newPath = EditorAssetExtend.MoveToResources(path);
                    Debug.LogError($"Move {path} to {newPath}");
                    path = newPath;
                }
            }

            xce.eName = path.AssetPathToResPath();
            Debug.Log("yns  path " + xce.eName);

            SetObjectEventTF(fe, playerTF, xce);
            return xce;
        }

        private static void SetObjectEventTF(FEvent fe, Transform playerTF, XCObjEvent xce)
        {
            xce.transfromType = fe.Track.Timeline.transfromType;

            xce.startPos = fe.Owner.transform.position - playerTF.transform.position;
            xce.startRotation = fe.Owner.transform.localEulerAngles;
            xce.startScale = fe.Owner.transform.localScale;
            if (fe.Owner.transform.parent == playerTF)
            {
                xce.startScale /= playerTF.localScale.x; //player尽量不要有缩放
            }

        }

        public static XCAnimEvent ToXCAnimEvent(FPlayAnimationEvent fe)
        {
            var xce = new XCAnimEvent();
            xce.range = new XCRange(fe.Start, fe.End);
            xce.isLocalTrueOnly = fe.isLocalTrueOnly;
            xce.startOffset = fe._startOffset * XCSetting.FramePerSec;
            xce.blenderLength = fe._blendLength * XCSetting.FramePerSec;
            xce.isBackToIdle = fe.isBackToIdle;
            xce.eName = fe._animationClip.name;
            return xce;
        }

        public static void SetLineEventTween(XCLineEvent xce, FTweenEvent<FTweenVector3> fe)
        {
            xce.range = new XCRange(fe.Start, fe.End);
            xce.startVec = fe.Tween.From;
            xce.isLocalTrueOnly = fe.isLocalTrueOnly;
            xce.endVec = fe.Tween.To;
            xce.easeType = fe.Tween.EasingType.ToDotweenEase();
        }        

        public static void SetLineEventTween_Ex(XCMoveEvent xce, FTweenEvent<FTweenVector3_Ex> fe)
        {
            xce.range = new XCRange(fe.Start, fe.End);
            xce.startVec = fe.Tween.From;
            xce.isLocalTrueOnly = fe.isLocalTrueOnly;
            xce.endVec = fe.Tween.To;
            xce.easeType = fe.Tween.EasingType.ToDotweenEase();

            xce.isBezier = fe.Tween.isBezier;
            xce._handlePoint = fe.Tween.HandlePoint;
            xce.lookForward = fe.Tween.lookForward;


    }
}




}
