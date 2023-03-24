using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XiaoCao
{
    //同一个Track的Evnet为同个类型的事件
    public class XCEventsTrack
    {
        //事件队列 按时间排序
        public List<XCEvent> _events = new List<XCEvent>();

        public XCAnimEvent curEvent;

        [NonSerialized]
        public XCEventsRunner selfRunner; //当前子技能

        public Animator animator = null;

        public SkillOwner owner;

        public bool HasFinished = false;

        private float _currentTime = -0.1f; //提前1点

        private int _currentFrame = 0;

        private int _currentEvent = 0; //记录队头


        public void Init(List<XCEvent> xcEvents, SkillOwner _owner)
        {
            owner = _owner;
            int length = xcEvents.Count;
            for (int i = length - 1; i >= 0; i--)
            {
                xcEvents[i].Init(_owner, this);
            }
            _events = xcEvents;
        }

        public void StartEvent()
        {
            HasFinished = false;
            HasTrigger = false;
        }

        public bool HasTrigger = false;

        public void OnEventUpdate(float deltaTime)
        {
            int limit = _events.Count;

            if (limit == 0)
            {
                HasFinished = true;
                //Debug.Log("yns  limit 0 ");
                return;
            }


            _currentTime += deltaTime;//Mathf.Clamp( time, 0, LengthTime );
            //帧数是用时间累加计算出来的
            //delta不是稳定的
            //当前的1帧,指的的是动画帧,即1/30s,而不是update的一帧
            _currentFrame = Mathf.FloorToInt(_currentTime * XCSetting.FrameRate);
            //Debug.Log("yns  _curFrame " + _currentFrame + "_curTime "+ _currentTime + " curEvent" + _currentEvent);

            for (int i = _currentEvent; i < limit; i++)
            {
                //还没开始时
                if (_currentFrame < _events[i].Start)
                {
                    if (_events[i].HasTriggered)
                        _events[i].OnReset();
                }
                else if (_currentFrame >= _events[i].Start && _currentFrame <= _events[i].End)
                {
                    if (!_events[i].HasFinished)
                    {
                        if (!_events[i].HasTriggered)
                        {
                            _events[i].OnTrigger(_currentTime - _events[i].StartTime);
                        }

                        HasTrigger = true;
                        _events[i].UpdateEvent(_currentFrame, _currentTime - _events[i].StartTime);

                    }
                }
                else
                {
                    //当 frame > end ,既已经完成 可以退出了
                    if (!_events[i].HasFinished && _events[i].HasTriggered)
                    {
                        _events[i].OnFinish();
                    }
                    //对于 frame > end 的事件不再检测

                    if (i == limit - 1)
                    {
                        //Debug.Log("yns owner HasFinished " + _currentEvent);
                        HasFinished = true;
                    }
                    _currentEvent = Mathf.Clamp(i + 1, 0, limit - 1);

                }
            }
        }
    }
}

