using Assets.Scripts.Enemy;
using cfg;
using DG.Tweening;
using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace XiaoCao
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(Rigidbody))]
    public class MonoAttacker : NetworkBehaviour, IAttacker, IMessager
    {
        #region NetWork
        public bool IsLocal => hasAuthority;

        //本地玩家 不包括npc
        public bool IsLocalTruePlayer => isTruePlayer && IsLocal;

        public bool IsLocalNpc => !isTruePlayer && IsLocal;

        //isLocalPlayer 本地玩家包括本地Npc

        [Header("MainSetting")]

        //模型
        public AgentModelType agentType;

        public bool isEnableAck = false;
        [SyncVar()]
        public bool isTruePlayer = false; //是客户端控制的角色


        #endregion
        public Animator animator;
        [SerializeField]
        [Header("血条位置")]
        private Transform _topTranform;
        public Transform TopTranform
        {
            get
            {
                if (_topTranform == null) { _topTranform = transform; }
                return _topTranform;
            }
        }
        public GameObject SelfObject { get => transform.gameObject; set => SelfObject = value; }
        public AI AI;

        [SyncVar(hook = nameof(UpdatePlayData))]
        public PlayerData playerData;

        [SyncVar]
        public BreakPower breakPower;

        public virtual int Hp { get; set; }

        public virtual int MaxHp { get; set; }

        public virtual int MaxBreakPower { get; set; }

        public virtual int NoBreakPower { get; set; }

        public virtual float Ack { get; }

        public bool IsDie => Hp <= 0;

        public virtual bool IsCanNorSkill { get; }

        [Header("DebugView")]
        public bool IsHideHpBar = false;
        [SyncVar()]
        public AgentTag AgentTag;
        [SyncVar()]
        public DamageState damageState;

        public XCTimer breakTimer = new XCTimer();
        public XCTimer noDamageTimer = new XCTimer();
        public XCTimer dieTimer = new XCTimer();
        public XCTimer noBreakTimer = new XCTimer(); //ba'ti


        public Action<XCEventsRunner> onSkillFinish;
        public Action<AckInfo> OnDamAciton;
        #region 生命周期
        public override void OnStartAuthority()
        {
            base.OnStartAuthority();
            isEnableAck = true;
            Debug.Log($"yns OnStartAuthority ");
        }


        public override void OnStartClient()
        {
            base.OnStartClient();
            PlayerMgr.Instance.RegisterAttacker(this);
        }

        public override void OnStopClient()
        {
            base.OnStopClient();
            PlayerMgr.Instance.DisRegisterAttacker(netId);
            IsHideHpBar = true;
        }
        #endregion
        public void UpdateHpBar()
        {
            //TODo 偷懒了
        }

        public virtual void UpdatePlayData(PlayerData old, PlayerData newData)
        {
            PlayerMgr.Instance.UpdatePlayerValue();
        }

        private void OnTagChanged(AgentTag old, AgentTag newValue)
        {
            Debug.Log($"yns model OnTagChanged {old}->{newValue}");
            ApplyAgentTag();
        }

        public virtual void StartClientSetting() { }

        public virtual void ApplyAgentTag() { }

        ///=> <see cref="PlayerState.OnAckTrigger"/>
        public virtual void OnAckTrigger(Collider other ,AckInfo ackInfo) { }

        public virtual void OnDam(AckInfo ackInfo = default)
        {
            ///=> <see cref="PlayerState.OnDam"/>
        }

        public virtual void AIMoveTo(Vector3 dir, float speed =1, bool is_mMove = true)
        {
            ///=> <see cref="PlayerState.AIMoveTo"/>
        }

        public virtual void SetSlowRoteAnim(bool v)
        {
            ///=> <see cref="PlayerState.SetSlowRoteAnim"/>
        }

        //查找
        //seeR：视觉范围
        //angle:视觉夹角 0-180 ，大于90表示处于后方
        //hearR:听觉范围; 负数->没有听觉范围,0->根据angle自动计算听觉范围。并且规定seeR>hearR
        public MonoAttacker SearchEnemy(float seeR = 15, float seeAngle = 180, float hearR = 0)
        {
            MonoAttacker res = null;
            float minDis = 9999;
            float tmpdis;
            foreach (var item in PlayerMgr.Instance.MonoAttackerDic.Values)
            {
                if (item.AgentTag != AgentTag && !item.IsDie)
                {
                    var tf = item.transform;
                    bool isFinded = false; 
                    bool isInAngle = IsInRangeAngle(tf, seeAngle, out float curAngle);
                    bool isInRange = IsInRangeDis(tf, seeR, out float curDis);

                    if (isInAngle && isInRange)
                    {
                        isFinded = true;
                    }
                    else if (isInRange && !isInAngle && hearR >=0)
                    {
                        if (hearR == 0)
                        {
                            hearR = seeAngle / 180 * seeR;
                        }
                        //听觉查找
                        isFinded = curDis < hearR;
                    }

                    if (isFinded)
                    {
                        //curAngle 接近 0 , 算出来的距离越小
                        float reAngleRete = Mathf.Lerp(1, 1.5f, curAngle / 180);

                        tmpdis = curDis* reAngleRete;//计算距离*夹角权重
                        //比较 选出最近的单位
                        if (tmpdis > 0 && tmpdis < minDis)
                        {
                            minDis = tmpdis;
                            res = item;
                        }
                    }
                }
            }
            return res;
        }


        /// <param name="tf"></param>
        /// <param name="minAngle">正前方向与敌人方向的夹角0-180,大于90度则表示在身后</param>
        /// <param name="curAngle">输出当前夹角</param>
        /// <returns></returns>
        public bool IsInRangeAngle(Transform tf, float minAngle, out float curAngle)
        {
            Vector3 dir = tf.position - transform.position;

            float angle = Vector3.Angle(dir, transform.forward);

            curAngle = angle;

            return angle < minAngle;
        }

        public bool IsInRangeDis(Transform tf, float dis, out float curDis)
        {
            curDis = Vector3.Distance(tf.position, transform.position);
            return curDis < dis;
        }


        public virtual void MoveSlowDown(float lerp = 0.1f)
        {

        }

        public virtual void SetBool(string name, bool msg)
        {

        }

        public virtual bool SetAIEvent(ActMsgType name, string msg, object other = null)
        {
            ///=> <see cref="PlayerState.SetAIEvent"/>
            return true;
        }

        public virtual void SendAll(string name, float num, bool isOn, string str)
        {
            ///=> <see cref="PlayerState.SendAll"/>
        }
    }

    public class XCTimer
    {
        public string name;
        public float exitTime;
        public float timer;
        public float cdRate = 1;
        public Action action;
        public bool isRuning = true;
        public bool isLoop = false;//时间到自动结束计时 不循环, 循环需要ResetTimer()

        public float FillAmount
        {
            get
            {
                if (TotalTime == 0)
                {
                    return 0;
                }

                return Mathf.Min(1, timer / TotalTime);
            }
        }
        public float TotalTime => exitTime * cdRate;

        public XCTimer() { }

        public void Init(string name, float exitTime, Action action)
        {
            this.name = name;
            this.exitTime = exitTime;
            this.action = action;
        }


        public void Update()
        {
            if (isLoop || (!isLoop && isRuning))
            {
                timer += Time.deltaTime;

                if (timer > TotalTime)
                {
                    Exit();
                    isRuning = false;
                }
            }
        }

        public void Exit()
        {
            timer = 0;
            action?.Invoke();
        }

        public void ResetTimer()
        {
            timer = 0;
            isRuning = true;
        }

        public void AddMinTime(float addTime, bool isForce = false)
        {
            isRuning = true;
            if (isForce)
            {
                exitTime = addTime;
                timer = 0;
            }
            else
            {
                //如果 追加时间 大于 剩余时间 ,则把剩余时间 设置为追加时间 ->继续运行
                var resTime = exitTime - timer;
                if (addTime > resTime)
                {
                    timer = exitTime - addTime;
                }
            }

        }
    }

    public class SkillCDTimer
    {
        public void Init(List<SkillKey> skillKeyList)
        {
            skillCDDic.Clear();
            foreach (var item in skillKeyList)
            {
                XCTimer timer = new XCTimer();
                timer.exitTime = item.cdTime;
                timer.timer = item.cdTime;
                timer.isRuning = false;
                timer.name = item.skillId;
                skillCDDic.Add(item.skillId, timer);
            }
        }
        public void SetCdRate(float cdRate)
        {
            foreach (var item in skillCDDic)
            {
                item.Value.cdRate = cdRate;
            }
        }

        public XCTimer GetTimer(string skillid)
        {
            skillCDDic.TryGetValue(skillid, out XCTimer timer);
            return timer;
        }

        public bool IsCDEnd(string skillid)
        {
            var timer = GetTimer(skillid);
            if (timer != null)
            {
                return !timer.isRuning;
            }
            return false;
        }

        public void ReCountCD(string skillid)
        {
            var timer = GetTimer(skillid);
            if (timer != null)
            {
                timer.ResetTimer();
            }
        }

        public Dictionary<string, XCTimer> skillCDDic = new Dictionary<string, XCTimer>();

        public void OnUpdate()
        {
            foreach (var item in skillCDDic.Values)
            {
                item.Update();
            }
        }
    }

}
