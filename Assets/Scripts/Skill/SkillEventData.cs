using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XiaoCao;

public class SkillEventData : MonoBehaviour
{
    //BaseSkillID
    public string skillId = ""; 
    //SubSkillID
    public string skillName = "";

    public float speed = 1f;

    //ObjEvent 是生成物体,一般是子技能的载体
    [ShowIf(nameof(HasObjEvent), "no")]
    public XCObjEvent ObjEvent;

    //动画事件
    [ShowIf(nameof(ShowAnimEvents), "no")]
    public XCEventOwnerData<XCAnimEvent> AnimEvents = new XCEventOwnerData<XCAnimEvent>();

    //位移 旋转 缩放
    [ShowIf(nameof(ShowMoveEvents), "no")]
    public XCEventOwnerData<XCMoveEvent> MoveEvents = new XCEventOwnerData<XCMoveEvent>();
    [ShowIf(nameof(ShowRotateEvents), "no")]
    public XCEventOwnerData<XCRotateEvent> RotateEvents = new XCEventOwnerData<XCRotateEvent>();
    [ShowIf(nameof(ShowScale), "no")]
    public XCEventOwnerData<XCScaleEvent> ScaleEvents = new XCEventOwnerData<XCScaleEvent>();

    //SwitchEvents 用于控制技能后摇, 和结束帧
    [ShowIf(nameof(ShowSwitchEvents), "no")]
    public XCEventOwnerData<XCSwitchEvent> SwitchEvents = new XCEventOwnerData<XCSwitchEvent>();

    ///MsgEvents 用于发送消息,如重力开关 <see cref="PlayEventMsg"/> 
    [ShowIf(nameof(ShowMsgEvents), "no")]
    public XCEventOwnerData<XCMsgEvent> MsgEvents = new XCEventOwnerData<XCMsgEvent>();
    //伤害范围
    [ShowIf(nameof(ShowTriggerEvents), "no")]
    public XCEventOwnerData<XCTriggerEvent> TriggerEvents = new XCEventOwnerData<XCTriggerEvent>();


    //一个Object 可以为一个子技能
    [Header("嵌套子技能")]
    public List<SkillEventData> subSkillData = new List<SkillEventData>();
    
    public bool HasObjEvent
    {
        get
        {
            return ObjEvent != null && !string.IsNullOrEmpty(ObjEvent.eName);
        }
    }

    public bool ShowAnimEvents => IsHasLen(AnimEvents);
    public bool ShowMoveEvents => IsHasLen(MoveEvents);
    public bool ShowRotateEvents => IsHasLen(RotateEvents);
    public bool ShowScale => IsHasLen(ScaleEvents);
    public bool ShowSwitchEvents => IsHasLen(SwitchEvents);
    public bool ShowMsgEvents => IsHasLen(MsgEvents);
    public bool ShowTriggerEvents => IsHasLen(TriggerEvents);

    public bool IsHasLen<T>(XCEventOwnerData<T> data) where T : XCEvent
    {
        return data.Events.Count > 0;
    }
}




[Serializable]
public class XCEventOwnerData<T>  where T : XCEvent
{
    //[HideListIfEmpty]
    public List<T> Events = new List<T>();

    public List<XCEvent> ToXCEventList()
    {
        List<XCEvent> xcevents = new List<XCEvent>();
        foreach (var item in Events)
        {
            xcevents.Add(item);
        }
        return xcevents;
    }
}


