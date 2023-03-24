using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventCenter : MonoSingleton<EventCenter>
{

    private Dictionary<string, IEventInfo> eventDic = new Dictionary<string, IEventInfo>();

    public void AddEventListener<T>(string name, UnityAction<T> action)
    {
        if(eventDic.ContainsKey(name))
            (eventDic[name] as EventInfo<T>).actions += action;
        else
            eventDic.Add(name, new EventInfo<T>(action));
    }
    public void AddEventListener<T0,T1>(string name, UnityAction<T0,T1> action)
    {
        if (eventDic.ContainsKey(name))
            (eventDic[name] as EventInfo<T0,T1>).actions += action;     
        else
            eventDic.Add(name, new EventInfo<T0,T1>(action));
        
    }
    public void AddEventListener(string name, UnityAction action)
    {
        if (eventDic.ContainsKey(name))
        {
            (eventDic[name] as EventInfo).actions += action;
        }  
        else
        {
            eventDic.Add(name, new EventInfo(action));
        }
    }

    public void RemoveEventListener<T>(string name, UnityAction<T> action)
    {
        if (eventDic.ContainsKey(name))
            (eventDic[name] as EventInfo<T>).actions -= action;
    }
    public void RemoveEventListener<T0,T1>(string name, UnityAction<T0,T1> action)
    {
        if (eventDic.ContainsKey(name))
            (eventDic[name] as EventInfo<T0,T1>).actions -= action;
    }
    public void RemoveEventListener(string name, UnityAction action)
    {
        if (eventDic.ContainsKey(name))
            (eventDic[name] as EventInfo).actions -= action;
    }


    public void EventTrigger<T>(string name,T info)
    {
        if (eventDic.ContainsKey(name))
        {
            if((eventDic[name] as EventInfo<T>).actions != null)
                    (eventDic[name] as EventInfo<T>).actions.Invoke(info);
        }    
    }

    public void EventTrigger<T0,T1>(string name, T0 info0,T1 info1)
    {
        if (eventDic.ContainsKey(name))
        {
            if ((eventDic[name] as EventInfo<T0,T1>).actions != null)
                (eventDic[name] as EventInfo<T0,T1>).actions.Invoke(info0,info1);
        }
    }
    //无参
    public void EventTrigger(string name)
    {
        if (eventDic.ContainsKey(name))
        {
            if ((eventDic[name] as EventInfo).actions != null)
                (eventDic[name] as EventInfo).actions.Invoke();            
        }
    }

    public  void Clear()
    {
        eventDic.Clear();
    }
}


#region sub class
public interface IEventInfo
{

}
public class EventInfo<T> : IEventInfo           //基类存子类 里氏转换原则
{
    public UnityAction<T> actions;

    public EventInfo(UnityAction<T> action)
    {
        actions += action;
    }
}

public class EventInfo<T0, T1> : IEventInfo           //基类存子类 里氏转换原则
{
    public UnityAction<T0, T1> actions;

    public EventInfo(UnityAction<T0, T1> action)
    {
        actions += action;
    }

}

//用于无参事件
public class EventInfo : IEventInfo           //基类存子类 里氏转换原则
{
    public UnityAction actions;

    public EventInfo(UnityAction action)
    {
        actions += action;
    }
}

#endregion
