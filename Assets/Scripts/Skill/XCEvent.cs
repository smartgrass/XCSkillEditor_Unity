using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace XiaoCao
{
    public static class XCSetting
    {
        public static readonly int FrameRate = 30;
        public static readonly float FramePerSec = 1f / FrameRate;
    }
    [Serializable]
    public struct XCRange
    {
        // start frame
        [SerializeField]
        private int _start;

        // end frame
        [SerializeField]
        private int _end;

        /// @brief Returns the start frame.
        public int Start
        {
            get { return _start; }
            set
            {
                _start = value;
            }
        }

        /// @brief Returns the end frame.
        public int End
        {
            get { return _end; }
            set
            {
                _end = value;
            }
        }

        /// @brief Sets / Gets the length.
        /// @note It doesn't cache the value.
        public int Length { set { End = _start + value; } get { return _end - _start; } }

        /**
		 * @brief Create a frame range
		 * @param start Start frame
		 * @param end End frame
		 * @note It is up to you to make sure start is smaller than end.
		 */
        public XCRange(int start, int end)
        {
            this._start = start;
            this._end = end;
        }

        /// @brief Returns \e i clamped to the range.
        public int Cull(int i)
        {
            return Mathf.Clamp(i, _start, _end);
        }

        /// @brief Returns if \e i is inside [start, end], i.e. including borders
        public bool Contains(int i)
        {
            return i >= _start && i <= _end;
        }

        /// @brief Returns if \e i is inside ]start, end[, i.e. excluding borders
        public bool ContainsExclusive(int i)
        {
            return i > _start && i < _end;
        }

        /// @brief Returns if the ranges intersect, i.e. touching returns false
        /// @note Assumes They are both valid
        public bool Collides(XCRange range)
        {
            return _start < range._end && _end > range._start;
            //			return (range.start > start && range.start < end) || (range.end > start && range.end < end );
        }

        /// @brief Returns if the ranges overlap, i.e. touching return true
        /// @note Assumes They are both valid
        public bool Overlaps(XCRange range)
        {
            return range.End >= _start && range.Start <= _end;
        }
        public override string ToString()
        {
            return string.Format("[{0}; {1}]", _start, _end);
        }
    }
    [Serializable]
    public class XCEvent
    {
        public XCEvent() { }
        public XCEvent(XCRange range)
        {
            this.range = range;
        }
        public XCRange range;

        public string eName;
        //isLocal truePlyaer
        public bool isLocalTrueOnly = false;

        [NonSerialized]
        public SkillOwner skillOwner;

        public Transform OwnerTF=>skillOwner.eventOwnerTF;

        [NonSerialized]
        public XCEventsTrack eventTrack;

        public XCEventsRunner SelfRunner { get => eventTrack.selfRunner; }
        public SkillEventData SkillData => SelfRunner.skillData;
        public string BaseSkillId { get => SkillData.skillId; }
        public uint NetId { get => skillOwner.netId; }

        [NonSerialized]
        private bool _hasTriggered = false;
        public bool HasTriggered { get { return _hasTriggered; } }

        [NonSerialized]
        protected bool _hasFinished = false;
        public bool HasFinished { get { return _hasFinished; } }

        public void SetFinished()
        {
            _hasFinished = true;
            _hasTriggered = true;
        }

        public int Start
        {
            get { return range.Start; }
            set { range.Start = value; }
        }
        public int End
        {
            get { return range.End; }
            set { range.End = value; }
        }

        public float StartTime
        {
            get { return range.Start * XCSetting.FramePerSec; }
        }

        /// @brief What this the event ends.
        /// @note This value isn't cached.
        public float EndTime
        {
            get { return range.End * XCSetting.FramePerSec; }
        }
        public float LengthTime
        {
            get { return range.Length * XCSetting.FramePerSec; }
        }
        public virtual void OnTrigger(float timeSinceTrigger)
        {
            _hasTriggered = true;
            _hasFinished = false;
            //Debug.Log("yns OnTrigger 2");
        }
        public virtual void OnFinish() { }
        public virtual void OnUpdateEvent(int frame, float timeSinceTrigger) { }
        public virtual void OnReset()
        {
            _hasFinished = false;
            _hasTriggered = false;
        }
        public void UpdateEvent(int frame, float timeSinceTrigger)
        {
            OnUpdateEvent(frame, timeSinceTrigger);

            if (range.End <= frame)
            {
                _hasFinished = true;
                OnFinish();
            }
        }

        public virtual void Init(SkillOwner owner, XCEventsTrack eventOwner)
        {
            this.skillOwner = owner;
            this.eventTrack = eventOwner;
            _hasFinished = false;
            _hasTriggered = false;
        }
    }
    [Serializable]
    public class XCAnimEvent : XCEvent
    {
        #region private
        private Animator _animator = null;

        private AnimationClip Clip;

        #endregion
        public int clipHash => Animator.StringToHash(eName);

        public float blenderLength = 0;

        public float startOffset = 0;

        public float speed = 1f;

        public bool isBackToIdle;

        void SetClip()
        {
            foreach (var item in _animator.runtimeAnimatorController.animationClips)
            {
                if (item.name == eName)
                {
                    Clip = item;
                }
            }
            if (true)
            {

            }
        }


        public override void OnTrigger(float timeSinceTrigger)
        {
            if (_animator == null)
            {
                if (skillOwner.isCustomObject)
                {
                    _animator = skillOwner.eventOwnerTF.GetComponentInChildren<Animator>(true);
                }
                else
                {
                    //默认获取玩家动画机
                    _animator = skillOwner.attacker.animator;
                }
            }
            if (_animator == null)
            {
                Debug.LogError("no _animator " + eName);
                return;
            }
            SetClip();

            if (Clip == null)
            {
                Debug.LogError("no clip " + eName);
                return;
            }

            _animator.speed = SelfRunner.speed;

            base.OnTrigger(timeSinceTrigger);
            //blenderLength =;

            //如果主技能被打断则不播放动画
            if (SelfRunner.isMainSkill && SelfRunner.isBreak)
            {
                return;
            }

            _animator.CrossFade(clipHash, blenderLength / Clip.length, 0, startOffset / Clip.length);


            //修正偏差值
            if (timeSinceTrigger > 0)
            {
                _animator.Update(timeSinceTrigger - 0.001f);
            }
        }

        public override void OnFinish()
        {
            if (isBackToIdle)
            {
                _animator.CrossFade(AnimHash.Idle, 0.05f);
            }
            base.OnFinish();
        }
    }

    [Serializable]
    public class XCObjEvent : XCEvent
    {
        public bool isEffect;

        public TransfromType transfromType;

        private ParticleSystem ps;

        public GameObject LoadObj;

        //[NonSerialized]
        public Vector3 playerOffset;

        //初始状态
        public Vector3 startPos = Vector3.zero;
        public Vector3 startRotation = Vector3.zero; //其实是eulerAngles
        public Vector3 startScale = Vector3.one;

        public override void Init(SkillOwner owner, XCEventsTrack eventOwner)
        {
            base.Init(owner, eventOwner);
            //冷加载
            LoadObj = RunTimePoolManager.Instance.LoadResPoolObj(eName, 0);
            skillOwner.eventOwnerTF = LoadObj.transform;
            LoadObj.SetActive(false);
        }

        public override void OnTrigger(float timeSinceTrigger)
        {
            SetFirstPos();
            if (isEffect)
            {
                ps = LoadObj.GetComponentInChildren<ParticleSystem>();
                ps.Play(true);
            }
            LoadObj.SetActive(true);
            Debug.Log($"yns LoadObj true {LoadObj} ");
            base.OnTrigger(timeSinceTrigger);
        }

        //设置起始位置
        private void SetFirstPos()
        {
            if (transfromType == TransfromType.FollowPlayer || transfromType == TransfromType.PlyerUnFollow)
            {
                //实时获取 Acker的坐标系 ,同步上会出现偏差,似乎不可避免,不想加同步数据就只能模拟了
                LoadObj.transform.eulerAngles = skillOwner.AckerTF.eulerAngles + startRotation;
                LoadObj.transform.position = skillOwner.AckerTF.TransformPoint(startPos);
            }
            if (transfromType == TransfromType.WorldPos)
            {
                LoadObj.transform.eulerAngles = SelfRunner.castEuler + startRotation;

                LoadObj.transform.position = SelfRunner.castPos + Quaternion.Euler(SelfRunner.castEuler) * startPos;
            }
            LoadObj.transform.localScale = startScale;

            if (transfromType == TransfromType.FollowPlayer)
            {
                LoadObj.transform.SetParent(skillOwner.AckerTF, true);
            }
        }

        public override void OnFinish()
        {
            base.OnFinish();
            if (ps)
                ps.Stop(true);
            LoadObj.gameObject.SetActive(false);
        }
        public void CheckReCycle()
        {
            if (LoadObj!=null)
            {
                Debug.Log($"yns CheckReCycle {LoadObj.name}");
                RunTimePoolManager.Instance.ReCycle(eName, LoadObj);
            }
    
        }
    }

    [Serializable]
    public class XCMoveEvent : XCLineEvent
    {
        public CharacterController cc;

        public Vector3 startDetal = Vector3.zero; //Move事件之间可能存在空隙,需要补全

        public bool isBezier;
        public bool lookForward;
        [NaughtyAttributes.ShowIf(nameof(isBezier))]
        public Vector3 _handlePoint;


        private Matrix4x4 m4;

        public override void Init(SkillOwner owner, XCEventsTrack eventOwner)
        {
            base.Init(owner, eventOwner);
            m4 = skillOwner.AckerTF.localToWorldMatrix;
        }

        public override void OnTrigger(float timeSinceTrigger)
        {
            base.OnTrigger(timeSinceTrigger);

            if (startVec != Vector3.zero)
            {
                ApplyDetalVec(startDetal);
            }
        }

        ///<see cref="XCLineEvent.OnUpdateEvent"/>
        public override void ApplyDetalVec(Vector3 detalMove)
        {

            cc = OwnerTF.GetComponent<CharacterController>();
            if (cc != null)
            {
                if (SelfRunner.isMainSkill && SelfRunner.isBreak)
                {
                    _hasFinished = true;
                    return;
                }
                cc.Move(m4.MultiplyVector(detalMove));
            }
            else
            {
                 OwnerTF.Translate(m4.MultiplyVector(detalMove), Space.World);
            }

            if (lookForward)
            {
                OwnerTF.forward = m4.MultiplyVector(detalMove);
            }

        }
        public override Vector3 GetVec3Value(float t)
        {
            float easingT = DOVirtual.EasedValue(0, 1, t, easeType);
            if (isBezier)
            {
                return MathTool.GetBezierPoint2(startVec, endVec, _handlePoint, easingT);
            }
            else
            {
                return MathTool.LinearVec3(startVec, endVec, easingT);
            }
        }

    }
    [Serializable]
    public class XCScaleEvent : XCLineEvent
    {
        public override void ApplyDetalVec(Vector3 detalMove)
        {
            //修改local Scale 试试
            OwnerTF.localScale += detalMove;
        }
    }

    [Serializable]
    public class XCRotateEvent : XCLineEvent
    {
        //Quaternion startQ;
        Vector3 angle;

        public override void OnTrigger(float timeSinceTrigger)
        {
            base.OnTrigger(timeSinceTrigger);
            //startQ =  SelfRunner.castEuler
            angle = SelfRunner.castEuler + startVec;
            OwnerTF.eulerAngles = angle;
        }

        public override void ApplyDetalVec(Vector3 detalMove)
        {
            angle += detalMove;
            OwnerTF.eulerAngles = angle;
        }
    }

    [Serializable]
    //Vec线性变化事件 基类
    public class XCLineEvent : XCEvent
    {
        public Vector3 startVec;
        public Vector3 endVec;
        [NonSerialized]
        public float lastTime = 0;

        public Vector3 move => endVec - startVec;

        public Ease easeType = Ease.Linear;


        public override void OnTrigger(float timeSinceTrigger)
        {
            base.OnTrigger(timeSinceTrigger);
            lastTime = 0;

        }
        public override void OnUpdateEvent(int frame, float timeSinceTrigger)
        {
            base.OnUpdateEvent(frame, timeSinceTrigger);
            float t = timeSinceTrigger / LengthTime;

            var move = GetVec3Value(t) - GetVec3Value(lastTime);
            lastTime = t;
            ApplyDetalVec(move);
        }
        /// <summary>
        /// 主要修改
        /// </summary>
        /// <param name="detalMove"></param>
        public virtual void ApplyDetalVec(Vector3 detalMove) { }


        public virtual Vector3 GetVec3Value(float t)
        {
            float easingT = DOVirtual.EasedValue(0, 1, t, easeType);
            return MathTool.LinearVec3(startVec, endVec, easingT);
        }

        public void ChageDir(float angle)
        {
            //angle旋转角度 axis围绕旋转轴 position自身坐标 自身坐标 center旋转中心
            //Quaternion.AngleAxis(angle, axis) * (position - center) + center;

            startVec = MathTool.ChageDir(startVec, angle);
            endVec = MathTool.ChageDir(endVec, angle);
        }
        public void ChangeOffset(Vector3 offset)
        {
            startVec += offset;
            endVec += offset;
        }

        public Vector3 RotateRound(Vector3 position, Vector3 center, Vector3 axis, float angle)
        {
            return Quaternion.AngleAxis(angle, axis) * (position - center) + center;
        }
    }

    [Serializable]
    public class XCTriggerEvent : XCEvent
    {
        [SerializeField]
        public CubeRange cubeRange;

        public TriggerItem triggerItem;
        private BoxCollider collider;

        public override void Init(SkillOwner skillOwner, XCEventsTrack eventOwner)
        {
            base.Init(skillOwner, eventOwner);

            ///XCTriggerEvent每一次都是新的,但由于特效会复用
            ///所以特效中可能会 上次使用留下的"Trigger"
            ///
            ///对于没有攻击权限的玩家 也会拿到带Trigger的特效. 
            ///这时就需要将triggerLayer关闭 或者设置为Friend

            bool HasTrigger =  FindTrigger(skillOwner.enableAck);

            if (skillOwner.enableAck)
            {
                collider.enabled = false;
                triggerItem.gameObject.SetActive(true);

                triggerItem.gameObject.layer = skillOwner.triggerLayer;


                triggerItem.attacker = skillOwner.attacker;
                AckInfoObject ackInfoObject = triggerItem.ackInfoObject;

                ackInfoObject.netId = this.skillOwner.netId;
                ackInfoObject.ackId = SkillData.skillName;
                ackInfoObject.baseSkillId = BaseSkillId;
                ackInfoObject.angleY = cubeRange.rotation.y;
            }
            else
            {
                if (HasTrigger)
                {
                    triggerItem.gameObject.SetActive(false);
                    triggerItem.gameObject.layer = skillOwner.triggerLayer;
                }
            }
        }

        private bool FindTrigger(bool isNeed)
        {
            Transform TriggerTF = skillOwner.eventOwnerTF.transform.Find("Trigger");
            if (TriggerTF == null) //->说明没有 BoxCollider 和 TriggerItem
            {
                if (isNeed)
                {
                    TriggerTF = new GameObject("Trigger").transform;
                    collider = TriggerTF.gameObject.AddComponent<BoxCollider>();
                    triggerItem = TriggerTF.gameObject.AddComponent<TriggerItem>();
                    collider.isTrigger = true;
                    triggerItem.transform.SetParent(skillOwner.eventOwnerTF, false);
                    return true;
                }
                return false;
            }
            else
            {
                collider = TriggerTF.GetComponent<BoxCollider>();
                triggerItem = TriggerTF.GetComponent<TriggerItem>();
                return true;
            }
        }

        public override void OnTrigger(float timeSinceTrigger)
        {
            base.OnTrigger(timeSinceTrigger);
            if (skillOwner.enableAck)
            {
                //OnTrigger 一个轨道可能会触发多次,每次触发前都需要重调下
                //设置大小 位置 角度
                triggerItem.transform.localEulerAngles = cubeRange.rotation;
                collider.size = cubeRange.size;
                collider.center = cubeRange.pos;
                collider.enabled = true;
            }

        }
        public override void OnFinish()
        {
            base.OnFinish();
            if (skillOwner.enableAck)
            {
                collider.enabled = false;
            }
        }

    }

    [Serializable]
    public class XCSwitchEvent : XCEvent
    {

        [NonSerialized]
        public GameObject self;

        public float blend = 0;

        public int ToFrame = 0; //目的跳转帧

        public float UnMoveTime = 0;

        public InputEventType InputType;

        public KeyCode keyCode;


        public override void OnTrigger(float timeSinceTrigger)
        {
            base.OnTrigger(timeSinceTrigger);
            if (InputType == InputEventType.Exit)
            {
                SelfRunner.BreakSkill();
            }
            else if (InputType == InputEventType.Finish)
            {
                Debug.Log("yns  Finish Even ");
                if (skillOwner.enableAck)
                {
                    PlayerMgr.Instance.SendAll(NetId, isLocalTrueOnly, PlayEventMsg.SetUnMoveTime, UnMoveTime);
                }
                SelfRunner.Finish();
            }
        }
    }
    [Serializable]
    public class XCMsgEvent : XCEvent
    {
        public MsgType msgEType;
        public string msgName;
        public string strMsg;
        public float floatdMsg;
        public bool boolMsg;
        public bool setOppositeOnFinish;
        //public bool isLocalOnly;

        public override void OnTrigger(float timeSinceTrigger)
        {
            base.OnTrigger(timeSinceTrigger);

            if (isLocalTrueOnly)
            {
                if (NetId != PlayerMgr.Instance.LocalNetId)
                {
                    Debug.Log($"yns isLocalOnly {msgName}");
                    return;
                }
            }
            //TODO 修改为本地
            switch (msgEType)
            {
                case MsgType.Bool:
                    PlayerMgr.Instance.SendBool(NetId, isLocalTrueOnly, msgName, boolMsg);
                    break;
                case MsgType.All:
                    PlayerMgr.Instance.SendAll(NetId, isLocalTrueOnly, msgName, floatdMsg, boolMsg, strMsg);
                    break;
            }
        }

        public override void OnFinish()
        {
            if (msgEType == MsgType.Bool)
            {
                if (setOppositeOnFinish)
                {
                    PlayerMgr.Instance.SendBool(NetId, isLocalTrueOnly, msgName, !boolMsg);
                }
            }
            base.OnFinish();
        }
    }

}
