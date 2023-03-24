using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace XiaoCao
{
    //真正的执行者
    public class XCEventsRunner : MonoBehaviour
    {
        //
        public XCObjEvent objEvent;

        public List<XCEventsTrack> updateTrack = new List<XCEventsTrack>();

        public List<XCEventsRunner> subRuners = new List<XCEventsRunner>();

        public SkillEventData skillData;

        public string BaseSkillId => skillData.skillId;

        //MainSkill才有
        public UnityEvent onFinishEvent = new UnityEvent();

        //释放技能时记录角度 和 位置
        public Vector3 castEuler;

        public Vector3 castPos;

        public bool isMainSkill = false;

        public bool isBreak = false;//异常停止

        public bool isMainFinish = false;//正常停止 和 Break都算Finish

        public bool IsRuning => !isMainFinish;

        public int updateCount;

        public float speed = 1;

        private RunnerState _state;

        public RunnerState State { get => _state; set => _state = value; }


        public void InitData(SkillEventData skillData, Vector3 castEuler, Vector3 castPos)
        {
            this.skillData = skillData;
            this.speed = skillData.speed;
            this.castEuler = castEuler;
            this.castPos = castPos;
            State = RunnerState.Update;
        }


        private void Update()
        {
            if (State == RunnerState.StopEnd)
                return;
 
            if (State == RunnerState.Update)
            {
                updateCount = updateTrack.Count;
                bool isSelfEnd = updateCount == 0;
                if (!isSelfEnd)
                {
                    OnUpdate();
                }
                else
                {
                    Finish();
                }

                if (isSelfEnd && IsSubsEnd())
                {
                    State = RunnerState.Stop;
                }
            }
            else if (State == RunnerState.Stop)
            {
                State = RunnerState.StopEnd;
                if (isMainSkill)
                {
                    DestroyAll();
                }       
            }

        }

        private void OnUpdate()
        {
            //ownerUpdate
            for (int i = updateCount - 1; i >= 0; i--)
            {
                updateTrack[i].OnEventUpdate(Time.deltaTime * speed);

                if (isBreak && updateTrack[i].HasTrigger == false)
                {
                    //关闭UpdateTrack没开始的Trick
                    updateTrack[i].HasFinished = true; //Remove
                }

                if (updateTrack[i].HasFinished)
                {
                    updateTrack.RemoveAt(i);
                }
            }
        }

        public void AddTrack(XCEventsTrack xCEventTrack)
        {
            if (xCEventTrack == null)
                return;
            updateTrack.Add(xCEventTrack);
        }

        public bool IsSubsEnd()
        {           
            foreach (var item in subRuners)
            {
                if (item.State != RunnerState.StopEnd)
                {
                    return false;
                }
            }
            return true;
        }

        //结束自身 以及子Runner
        //接收到Break,OnUpdate会继续运行,只是不会触发新事件
        public void BreakSkill()
        {
            isBreak = true;
            foreach (var item in subRuners)
            {
                if (item!=null)
                {
                    item.isBreak = true;
                } 
            }
            Finish();
        }
        //Finish表示玩家脱离skill状态,而不影响skill的自我运行
        public void Finish()
        {
            if (!isMainFinish)
            {
                isMainFinish = true;
                onFinishEvent?.Invoke();
            }
        }

        public void DestroyAll()
        {
            CheckRecyle();

            foreach (var item in subRuners)
            {
                if (item != null)
                {
                    item.CheckRecyle();
                    Destroy(item.gameObject);
                }
            }
            Destroy(gameObject);
        }

        private void CheckRecyle()
        {
            if (objEvent != null)
            {
                objEvent.CheckReCycle();
            }
        }
    }

    public enum RunnerState
    {
        Start,
        Update,
        Stop,
        StopEnd
    }
}
