
using System.Collections.Generic;

using System.Text;

using UnityEngine;

namespace XiaoCao
{
    public class SkillLauncher
    {
        //后面考虑改写附加条件
        public static XCEventsRunner StartPlayerSkill(SkillEventData skillData, SkillOwner owner, Vector3 castEuler, Vector3 castPos)
        {
            XCEventsRunner runner = StartRuner(owner, skillData, castEuler, castPos);
            runner.isMainSkill = true;
            foreach (var subSkill in skillData.subSkillData)
            {
                XCEventsRunner subRunner = StartRuner(owner, subSkill,castEuler, castPos);
                if (subRunner)
                {
                    runner.subRuners.Add(subRunner);
                }
            }
            return runner;
        }
        public static XCEventsRunner StartRuner(SkillOwner skillOwner, SkillEventData skillData, Vector3 castEuler, Vector3 castPos)
        {
            if (skillData.HasObjEvent)
            {
                bool isRemove = IsRemoveLocalTrue(skillOwner.attacker.IsLocalTruePlayer, skillData.ObjEvent);
                if (isRemove)
                {
                    Debug.Log($"yns IsRemove");
                    return null;
                }
            }

            XCEventsRunner runner = (new GameObject(skillData.skillName)).AddComponent<XCEventsRunner>();
            runner.transform.SetParent(RunTimePoolManager.Instance.FindOrCreatIDObj("EventRuner").transform);
            runner.InitData(skillData,castEuler, castPos);

            if (skillData.HasObjEvent)
            {
                skillOwner = SkillOwner.CopyNew(skillOwner);
                runner.objEvent = skillData.ObjEvent;
                AddTrackToRunner(runner, skillOwner, new List<XCEvent>() { skillData.ObjEvent });
                skillOwner.isCustomObject = true;
            }

            AddTrackToRunner(runner, skillOwner, skillData.TriggerEvents.ToXCEventList());

            AddTrackToRunner(runner, skillOwner, skillData.AnimEvents.ToXCEventList());

            AddTrackToRunner(runner, skillOwner, skillData.MoveEvents.ToXCEventList());

            AddTrackToRunner(runner, skillOwner, skillData.ScaleEvents.ToXCEventList());

            AddTrackToRunner(runner, skillOwner, skillData.RotateEvents.ToXCEventList());

            AddTrackToRunner(runner, skillOwner, skillData.MsgEvents.ToXCEventList());
 
            AddTrackToRunner(runner, skillOwner, skillData.SwitchEvents.ToXCEventList());


            //由于Runner是倒叙遍历 , 所以Obj放最后可提前执行
            if (skillData.HasObjEvent)
            {
                //int len = runner.updateTrack.Count;
                //var lastTrack = runner.updateTrack[0];
                //runner.updateTrack[0] = runner.updateTrack[len-1];
                //runner.updateTrack[len-1] = lastTrack;
            }
            return runner;
        }


        private static XCEventsTrack AddTrackToRunner(XCEventsRunner runner, SkillOwner owner, List<XCEvent> xcevents)
        {
            int length = xcevents.Count;
            if (length == 0)
                return null;

            for (int i = length - 1; i >= 0; i--)
            {
                //移除本地事件
                bool isRemove = IsRemoveLocalTrue(owner.attacker.IsLocalTruePlayer, xcevents[i]);
                if (isRemove)
                {
                    xcevents.RemoveAt(i);
                }
            }
            if (xcevents.Count == 0)
                return null;

            XCEventsTrack track = new XCEventsTrack();
            track.selfRunner = runner;
            track.Init(xcevents, owner);
            runner.AddTrack(track);
            return track;
        }



        private static bool IsRemoveLocalTrue(bool IsLocalTruePlayer, XCEvent xcevent)
        {
            return xcevent.isLocalTrueOnly && !IsLocalTruePlayer;
        }

    }
}
