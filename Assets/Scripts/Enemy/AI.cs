using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using XiaoCao;
using Debug = UnityEngine.Debug;

namespace Assets.Scripts.Enemy
{
    //控制权应该只在一个之上
    public class AI : MonoBehaviour
    {
        #region 组件
        private MonoAttacker selfAck;

        private MonoAttacker curTarget;
        private MonoAttacker lastAcker; //查找上将有50%的权重加成
        private Transform lastAckerTF => lastAcker.transform; //记录上一个打自己的攻击者

        #endregion

        #region Feild
        //视觉范围
        public float CheckR = 20;

        public float seeAngle = 180;

        [ReadOnly]
        public float curDistance;//监视用
        [ReadOnly]
        public string _curActName = "";
        [ReadOnly]
        public AIActState _curActState;

        //远程技能范围
        public float FarActR_Rate = 0.6f;
        public float FarActR => CheckR * FarActR_Rate;

        //时间配置
        [XCLabel("被打断僵直时间")]
        public float DamageBreakTime = 0.6f;

        public float HideSpeedRate = 1f;

        //持续被打破后 会使用逃脱技能
        public float BreakDefTime = 5f;

        [Header("转速")]
        public float angleSpeed = 5f;
        [XCLabel("瞄准角度误差")]
        public float minDetalAngle = 10f;

        //每隔1s检查 敌人和cd
        private float searchTargetTime = 1; //无目标 每隔1s检查
        private float searchTime_hasTarget = 5;//有目标 每隔5s检查
        private float searchTimer = 0;
        private float moveTimer = 0;
        private float aimTimer = 0;

        public float ackTime = 0.8f; //act切换间隔,防止瞬间多次触发
        public float aimTime = 0.8f; //攻击前的停顿
        private float ackTimer = 0;
        private float hideTimer = 0;
        private float ackEndWaitTimer = 0;
        private HideAct curHideAct;
        private bool isFarHide;
        private bool isAimEnd;

        [Header("事件池")]
        public List<ActGroup> aIActions;
        [Header("事件池(远)")]
        public ActGroup FarActGroup;

        [Header("默认事件")]
        public AIAct breakDefAct;//持续被打破后 会使用逃脱技能

        [Header("DebugView")]


        public int curGroupIndex = 0;

        #endregion

        private Vector3 bornPoint;

        private AIAct _curAct;
        private AIAct CurAction { get=>_curAct; set {
                _curAct = value;
                if (_curAct!=null)
                {
                    _curActName = _curAct.actName;
                    ChangeState(AIActState.Start);
                }
            }
        }
        private bool IsAcking => CurAction != null && CurAction.state == AIActState.Acking;

        private XCTimer breakDefTimer = new XCTimer();   //持续被打破后 会使用逃脱技能
        private PlayerMoveSettingSo moveSettingSo; //暂无使用

        public void Init(PlayerState monoAttacker)
        {
            selfAck = monoAttacker;
            moveSettingSo = monoAttacker.PlayerMoveSettingSo; 

            selfAck.breakTimer.exitTime = DamageBreakTime;
            bornPoint = transform.position;
        }

        private void Start()
        {

            selfAck.onSkillFinish += OnSkillFinish;
            selfAck.OnDamAciton += OnDamAciton;

            //持续被打破后 会使用逃脱技能
            breakDefTimer.Init("defTimer", BreakDefTime, OnBreakExit);
            breakDefTimer.isLoop = true;

             searchTimer = searchTargetTime;
        }

        private void OnDestroy()
        {
            //self.onSkillFinish -= OnSkillFinish;
        }

        #region StateAct



        void Update()
        {
            if (selfAck.IsDie)
            {
                return;
            }

            if (selfAck.damageState == DamageState.OnBreak)
            {
                breakDefTimer.Update(); //=> OnBreakDef
                return;
            }

            //查找目标
            UpdateTargetTimer();
            //执行动作
            UpdateAction();
        }

        private void UpdateTargetTimer()
        {
            searchTimer += Time.deltaTime;
            if (curTarget != null)
            {
                //无目标 每隔1s检查
                if (searchTimer > searchTargetTime)
                {
                    OnSearchTarget();
                }
            }
            else
            {
                //有目标 每隔5s检查
                if (searchTimer > searchTime_hasTarget)
                {
                    OnSearchTarget();
                }
            }
        }
        //执行动作
        private void UpdateAction()
        {
            if (CurAction == null && curTarget == null)
            {
                //处于Idle状态 要么巡逻 要么睡觉 //考虑给一个idle组件
                UpdateIdle();
                //巡逻在出生点附近
                return;
            }

            if (CurAction == null )
            {
                //如果无动作,但有目标则 =>获取动作
                GetAct();
                if (CurAction == null)
                {
                    return;
                }
            }

            _curActState = CurAction.state;

            if (CurAction.state == AIActState.Start)
            {
                OnMoveStateEnter();
            }

            //OnAckStart
            if (CurAction.state == AIActState.MoveTo)
            {
                //靠近 瞄准
                OnMoveState();
            }
            else if (CurAction.state == AIActState.Acking)
            {
                ackEndWaitTimer += Time.deltaTime;
                if (ackEndWaitTimer > CurAction.endWaitTime)
                {
                    ackTimer += Time.deltaTime;
                    if (ackTimer > ackTime && selfAck.IsCanNorSkill)
                    {
                        hideTimer = 0;
                        GetHideAct();
                        ChangeState(AIActState.Hide); //结束执行下一个动作
                    }
                }
            }

            if (CurAction.state == AIActState.Hide)
            {
                hideTimer += Time.deltaTime;
                if (hideTimer > CurAction.hideTime)
                {
                    ChangeState(AIActState.End);
                }
                else
                {
                    //躲避
                    HideMove(HideSpeedRate);
                }
            }
            //逃离逻辑要做修正
            else if(CurAction.state == AIActState.End)
            {
                if (CurAction.nextActName.IsEmpty())
                {
                    CurAction = null;
                }
                else
                {
                    CurAction = aIActions[curGroupIndex].aIActions.Find(a => a.actName == CurAction.nextActName);
                }
                return;
            }
        }
        //获取一个动作
        private void GetAct()
        {
            curDistance = GetDis(curTarget.transform);

            //近程
            if (curDistance < FarActR || FarActGroup.IsEmpty)
            {
                int len = aIActions.Count;
                if (len > 0)
                {
                    var group = aIActions[curGroupIndex];
                    if (group.IsDisable)
                    {
                        curGroupIndex = (curGroupIndex + 1) % len;//下一组
                        group = aIActions[curGroupIndex]; 
                    }
                    CurAction = group.GetOneAct();
                }
            }
            // 远程
            else
            {
                Debug.Log($"yns FarAct!");
                CurAction = FarActGroup.GetOneAct();
            }
        }

        private void UpdateIdle()
        {
            //TODO
        }

        private void OnMoveStateEnter()
        {
            searchTimer = 0;
            moveTimer = 0;
            aimTimer = 0;
            isAimEnd = false;
            ChangeState(AIActState.MoveTo);
        }

        private void OnMoveState()
        {
            if (curTarget == null)
            {
                OnAckStart();
                return;
            }

            curDistance = GetDis(curTarget.transform);

            bool isFar = curDistance > CurAction.targetDis;

            bool isMoveEnd = moveTimer >= CurAction.moveTime;

            if (!isFar)
            {
                isMoveEnd = true;
            }
            if (!isMoveEnd)
            {
                moveTimer += Time.deltaTime;
                //float moveSpeed = isFar ? 1 : 0.1f;
                Move(1);
            }

            if(isMoveEnd)
            {
                bool isWaitEnd = aimTimer >= aimTime;
                aimTimer += Time.deltaTime;

                isAimEnd = RoateTo_Slow(curTarget, angleSpeed, minDetalAngle);
                
                if (isWaitEnd || isAimEnd)
                {
                    OnAckStart();
                }
            }
        }

        private void OnAckStart()
        {
            selfAck.SetAIEvent(CurAction.actType, CurAction.actMsg);
            ackTimer = 0;
            ackEndWaitTimer = 0;
            ChangeState(AIActState.Acking);
        }

        private void ChangeState(AIActState newState)
        {
            CurAction.state = newState;
        }

        //查找目标，每隔一段时间查找
        //或者当前目标死亡，或者目标逃离，或者自身受到Break攻击，重查找
        private void OnSearchTarget()
        {
            searchTimer = 0;

            var nearAcker = selfAck.SearchEnemy(CheckR, seeAngle);

            if (lastAcker != null && lastAcker.IsDie)
            {
                lastAcker = null;
            }

            //最近的敌人
            if (nearAcker != lastAcker && nearAcker != null && lastAcker != null)
            {
                float nearDis = Vector3.Distance(transform.position, nearAcker.transform.position);
                //上一位攻击者缩短为 0.75f的距离 
                float lastAckerDis = 0.75f * Vector3.Distance(transform.position, lastAcker.transform.position);

                MonoAttacker res = nearDis < lastAckerDis ? nearAcker : lastAcker;
                SwitchTarget(res);
            }
            else if (nearAcker != null)
            {
                SwitchTarget(nearAcker);
            }
            else if (lastAcker != null)
            {
                SwitchTarget(lastAcker);
            }
            else
            {
                SwitchTarget(null);
            }
        }
        //[Button]
        //void testfream()
        //{
        //    // 获取当前线程的堆栈跟踪信息
        //    StackTrace stackTrace = new StackTrace();

        //    // 遍历堆栈帧并输出调用信息
        //    for (int i = 0; i < stackTrace.FrameCount; i++)
        //    {
        //        StackFrame frame = stackTrace.GetFrame(i);
        //        Debug.LogFormat("{0}:{1} {2}()",
        //            frame.GetType(),
        //            frame.GetFileLineNumber(),
        //            frame.GetMethod().Name);
        //    }
        //}

        [Button]
        private void TestSearchEnmey()
        {
            var nearAcker = selfAck.SearchEnemy(CheckR, seeAngle);
            if (nearAcker)
            {
                var curdis = GetDis(nearAcker.transform);
                Debug.Log($"yns find {nearAcker.name}");
            }
            else
            {
                nearAcker = selfAck.SearchEnemy(2*CheckR, 180);
                var curdis = GetDis(nearAcker.transform);
                Debug.Log($"yns no find {nearAcker.name}");
            }
        }

        private void SwitchTarget(MonoAttacker attacker)
        {
            curTarget = attacker;
        }

        private float GetDis(Transform tf)
        {
            return Vector3.Distance(transform.position, tf.position);
        }
        #endregion

        #region OnDamage Break, SkillFinish 
        //TODO
        private void OnBreakExit()
        {
            Debug.Log($"yns DefExit "); //暂时不处理
            //逃离Break
            //selfAck.SetAIEvent(breakDefAct.actType, breakDefAct.actMsg);//isSkill?
            OnSearchTarget();
        }

        private void OnDamAciton(AckInfo obj)
        {
            if (PlayerMgr.Instance.MonoAttackerDic.ContainsKey(obj.netId))
            {
                //标记攻击者
                lastAcker = PlayerMgr.Instance.MonoAttackerDic[obj.netId];
                //如果技能被击破 则清空技能数字
                if (selfAck.damageState == DamageState.OnBreak)
                {
                }
            }
        }

        private void OnSkillFinish(XCEventsRunner runner)
        {
            Debug.Log($"yns AI OnSkillFinish {runner.BaseSkillId}");
            //暂无处理
        }
        #endregion



        #region Excute, Move Roate

        private void GetHideAct()
        {
            if (curTarget == null)
                return;
            curDistance = GetDis(curTarget.transform);
            //处于玩家 偏左边 就左转, 但要look
            //远-> 靠近 内心圆   近->远离 外心圆
            isFarHide = curDistance > CurAction.targetDis / 0.75f;
            curHideAct =(HideAct)(UnityEngine.Random.Range(0, 2)); //随机取一个方向
            //Debug.Log($"yns Get HideAct {curHideAct}");
        }

        private void HideMove(float speedRate = 0.5f)
        {
            if (curTarget == null)
                return;

            float targetAngle = isFarHide ? 90 : 110;

            targetAngle = curHideAct == HideAct.MoveLeft ? targetAngle : -targetAngle;

            var dir = curTarget.transform.position - transform.position;

            var targetDir= MathTool.ChageDir(dir, targetAngle);
            //TODO-> SetLookAt
            selfAck.AIMoveTo(targetDir, speedRate, !CurAction.isHide_LookAt); //HideMove

            if (CurAction.isHide_LookAt)
            {
                RoateTo_Slow( curTarget, angleSpeed);
            }

        }

        private void Move(float speedRate = 1)
        {
            if (curTarget!=null)
            {
                Vector3 dir = (curTarget.transform.position -transform.position).normalized;
                selfAck.AIMoveTo(dir,speedRate);
            }
            else
            {
                Vector3 dir = transform.forward;
                selfAck.AIMoveTo(dir, speedRate);
            }
        }

        //private void RoateToFast(float rate, MonoAttacker target)
        //{
        //    selfAck.transform.RotaToTarget_Com(target, rate);
        //}

        private bool RoateTo_Slow(MonoAttacker target,float angleSpeed,float minDetal = 5)
        {
           selfAck.SetSlowRoteAnim(true);
           return selfAck.transform.RoateY_Slow(target.transform.position, angleSpeed, minDetal);
        }


        public void CallSetAIEvent(ActMsgType skill, string actMsg)
        {
            selfAck.SetAIEvent(skill, actMsg);
        }

        #endregion

        [Button("添加默认事件", enabledMode: EButtonEnableMode.Editor)]
        private void AddDefautAct()
        {
            breakDefAct = new AIAct()
            {
                actType = ActMsgType.OtherSkill,
                actMsg = "NoBreak_Roll",
                targetDis = 0,
            };
        }

        [Button(enabledMode: EButtonEnableMode.Playmode)]
        private void AddTestCommpent()
        {
            gameObject.AddComponent<AI_Test>().aI = this;
        }

    }

    #region SubClass

    [System.Serializable]
    public class ActGroup
    {
        public string des = "";
        //public int maxTime = 20;
        //public float tardis; //目前AI没有根据tardis选择技能组的能力
        [SerializeField]
        public List<AIAct> aIActions;
        [NonSerialized]
        private List<AIAct> enableActs = new List<AIAct>(); //可以选的Act,使用一个都会移除一个,并且计数
        [HideInInspector]
        public bool IsDisable = false;

        public AIAct GetOneAct()
        {
            if (enableActs.Count == 0)
            {
                ReStart();
            }
            int index = 0;
            var res = enableActs.GetRandom(out index);
            res.useTimer++;
            if (res.useTimer >= res.maxUseTime)
            {
                enableActs.RemoveAt(index);
            }
            if (enableActs.Count == 0)
            {
                //切换下一组
                IsDisable = true;
            }
            return res;
        }

        public void ReStart()
        {
            IsDisable = false;
            enableActs = new List<AIAct>(aIActions);
            foreach (var item in enableActs)
            {
                item.Reset();
            }
        }

        public bool IsEmpty => aIActions.Count == 0;
    }

    [System.Serializable]
    public class AIAct : PowerModel
    {
        public string actName; //id
        public ActMsgType actType;//事件      
        public string actMsg = "NorAck"; //信息  当
        public float targetDis = 3; //执行距离


        public float moveTime = 1.5f; //追踪时间
        public float endWaitTime = 0; //攻击结束的后摇
        public float hideTime = 0.5f; //结束后躲避时间,默认是后退
        public bool isHide_LookAt =  false; // 躲避时是否盯着目标

        public int maxUseTime = 2; //一个组内最多使用次数
        public string nextActName;
        public int useTimer = 0;
        [ReadOnly]
        public AIActState state;

        public void Reset()
        {
            useTimer = 0;
            state = AIActState.Start;
        }
    }

    [System.Serializable]
    public class PowerModel
    {
        public int power = 1; //权重
    }

    public static class RandomHelper
    {
        public static T GetRandom<T>(this List<T> powerModel,out int index) where T : PowerModel
        {
            index = 0;
            int total = 0;
            foreach (var item in powerModel)
            {
                total += item.power;
            }
            if (powerModel.Count == 0)
            {
                index = -1;
                return null;
            }
            if (powerModel.Count == 1 || total == 0)
            {
                return powerModel[0];
            }

            int random = UnityEngine.Random.Range(0, total);
            int rangeMax = 0;

            int length = powerModel.Count;
            for (int i = 0; i < length; i++)
            {
                rangeMax += powerModel[i].power;
                //当随机数小于 rangeMax 说明在范围内
                if (rangeMax > random && powerModel[i].power > 0)
                {
                    index = i;
                    return powerModel[i];
                }
            }
            Debug.LogError("yns ??? power = 0");
            return null;
        }
    }

    //行为类型
    public enum ActMsgType
    {
        Skill,
        OtherSkill,
        Move, //不使用技能
    }

    public enum AIState
    {
        Runing, //运行
        Break //被打断
    }

    //子行为状态
    public enum AIActState
    {
        Start,
        MoveTo, //靠近或者瞄准时间
        Acking, //等待技能结束时间
        Hide, //躲避时间
        End, //切换下一技能
    }

    public enum HideAct
    {
        MoveLeft, //左移
        MoveRight, //右移
        //Back, //后退
        //Dash //后闪
    }

    #endregion
}
