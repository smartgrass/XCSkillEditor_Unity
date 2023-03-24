
using Assets.Scripts.Enemy;
using cfg;
using DG.Tweening;
using Mirror;
using NaughtyAttributes;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace XiaoCao
{
    public class PlayerState : MonoAttacker
    {
        #region meaber

        public PlayerInputSetting inputSetting = new PlayerInputSetting();
        [HideInInspector]
        public PlayerMover playerMover;
        [HideInInspector]
        public CharacterController cc;
        [Header("受击Trigger(空则使用cc)")]
        public List<Collider> damCols = new List<Collider>();

        SoUsing SoUsing => ResFinder.SoUsingFinder;
        PlayerMgr playerMgr => PlayerMgr.Instance;
        [Header("设置数据(必填)")]
        public PlayerMoveSettingSo PlayerMoveSettingSo;
        SkiillKeyCodeSo SkiillKeyCodeSo;
        public DebugSo DebugSo => SoUsing.DebugSo;

        float LockEnemyRad => PlayerMoveSettingSo.LookEnemyRad; //攻击时辅助锁敌
        float LockEnemyAngle => PlayerMoveSettingSo.LockEnemyAngle;
        #region private
        private XCEventsRunner lastSkillRuner;
       
        protected XCEventsRunner curSkillRuner; //主动技能
        private Renderer[] renders = new Renderer[0];

        private SkillCDTimer _cDTimer;
        public SkillCDTimer CDTimer
        {
            get
            {
                if (_cDTimer == null)
                {
                    _cDTimer = playerMgr.skillCD;
                }
                return _cDTimer;
            }
        }
        #endregion

        #endregion


        #region SyncVar
        [SyncVar]
        private float RandomAck = 1;
        #endregion

        public Vector3 rollVec = default;



        public override int MaxBreakPower => breakPower.maxBreakPower;
        public override int NoBreakPower => breakPower.noBreakPower;

        public override int MaxHp => playerData.MaxHp;
        public override int Hp { get => playerData.hp; set => playerData.hp = value; }
        public override float Ack { get => playerData.BaseAck * DebugSo.ackRate; }

        public SkillOwner SkillOwenr
        {
            get
            {
                if (_skillOwenr == null)
                {
                    _skillOwenr = new SkillOwner(this);  
                }
                return _skillOwenr;
            }
        }
        private SkillOwner _skillOwenr;

        public PlayerStateEnum curState = PlayerStateEnum.Idle;
        bool isStarted = false;
        bool isLayerReady = false;
        public PlayerSkin skin;


        #region 生命周期
        //Awake->SyncVarHook ->OnStartServer -> OnStartAuthority
        //->OnStartClient ->OnStartLocalPlayer ->Start

        //根据Tag阵营 决定攻击和受击Layer
        public override void ApplyAgentTag()
        {
            if (playerMgr.LocalPlayer==null)
            {
                Debug.Log($"yns WaitLocalPlayer");
            }
            else
            {
                if (IsLocal)
                {
                    playerMgr.ApplyAllAgentTag();
                }
                else
                {
                    Debug.Log($"yns SetAgentTag {netId} {AgentTag}");
                    SetColiderLayer();
                }
            }
        }
        [Button]
        private void Test()
        {
            Debug.Log($"yns Test ");
            playerData.hp = 1;
        }

        private void CommpentInit()
        {
            DontDestroyOnLoad(gameObject);
            SkiillKeyCodeSo = SoUsing.SkiillKeyCodeSo;

            cc = GetComponent<CharacterController>();
            playerMover = GetComponent<PlayerMover>();
            playerMgr.Register(this);
            playerMover.Init(this);
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            if (IsLocal)
            {
                playerMgr.LocalPlayer = this;
            }

            Debug.Log($"yns Start {netId}");
            CommpentInit();

            StartClientSetting();

            ApplyAgentTag();

            if (IsLocalTruePlayer)
            {
                //输入设置
                //Cursor.visible = ResFinder.SoUsingFinder.DebugSo.IsNoLockMousce;
                //Cursor.lockState = ResFinder.SoUsingFinder.DebugSo.IsNoLockMousce ? CursorLockMode.None : CursorLockMode.Locked;
                //Cursor.visible = true;
                Invoke(nameof(SetLockMouse), 1f);

                inputSetting.SkillKeyMsg = new Dictionary<KeyCode, string>();
                foreach (var item in SkiillKeyCodeSo.GetActiveSkills())
                {
                    inputSetting.SkillKeyMsg.Add(item.keyCode, item.skillId);
                }

                for (int i = (int)KeyCode.Alpha0; i <= (int)KeyCode.Alpha9; i++)
                {
                    int num = i - (int)KeyCode.Alpha0;
                    if (!inputSetting.SkillKeyMsg.ContainsKey((KeyCode)i))
                    {
                        inputSetting.SkillKeyMsg.Add((KeyCode)i, num.ToString());
                    }
                }

                CDTimer.Init(SkiillKeyCodeSo.skillKeyList);
                CDTimer.SetCdRate(DebugSo.CdRate);
                playerMgr.DoPlayerStart();
            }

            isStarted = true;

            if (AI)
            {
                AI.Init(this);
            }
        }

        public override void StartClientSetting()
        {
            //设置数值
            playerData = PlayerData.GetData();
            playerData.MaxHp = IsLocal ? DebugSo.MaxHp : (int)(DebugSo.MaxHp * 1.5f);
            playerData.SetFull();
            breakPower.maxBreakPower = IsLocal ? DebugSo.maxBreakPower : DebugSo.maxBreakPowerNPC[0];
            breakPower.SetFull();

            //设置皮肤
            skin = isTruePlayer ? DebugSo.playerSkin : DebugSo.npcSkin;
            ChangeSkin(skin);

            breakTimer.Init("damageTimer", 0.1f, null); //脱离Break ->OnBreakExit
            noDamageTimer.Init("noDamageTimer", 0.2f, OnNoDamExit);
            dieTimer.Init("dieTimer", 2, OnDieEnd);
        }


        private void SetLockMouse()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = true;
        }

        #region initSetting
        public void SetColiderLayer()
        {

            Debug.Log($"yns AgentTag {netId} {AgentTag}");

            SetAllColLayer(GameSetting.GetColiorLayer(AgentTag));


            SkillOwenr.enableAck = isEnableAck;
            SkillOwenr.triggerLayer = GameSetting.GetAckLayer(AgentTag, isEnableAck); ;
        }


        public void SetAllColLayer(int layer)
        {
            cc.gameObject.layer = layer;
            foreach (var item in damCols)
            {
                item.gameObject.layer = layer;
            }
        }


        #endregion


        private void Update()
        {
            if (IsDie)
            {
                dieTimer.Update();
                return;
            }
            if (!IsLocal)
            {
                return;
            }

            //Local Play包括 Local Npc
            UpdateStates_Local();

            playerMover.MoveUpdate();

            if (isTruePlayer)
            {
                CDTimer.OnUpdate();
                if (norAckTime > 0)
                {
                    norAckTime -= Time.deltaTime;
                }
                CheckInputSkill();
            }
        }

        private void FixedUpdate()
        {
            if (!isStarted)
                return;
            playerMover.MoveFixUpdate();
        }

        private void LateUpdate()
        {
            if (!isStarted)  
                return;
            
            if (IsDie)
                return;
            

            playerMover.MoveLateUpdate();
        }


        public override void MoveSlowDown(float lerp = 0.1f)
        {
            playerMover.m_Move = Vector3.Lerp(playerMover.m_Move, Vector3.zero, lerp);
            if (playerMover.m_Move.magnitude < 0.1f)
            {
                playerMover.m_Move = Vector3.zero;
            }
        }


        public override void OnStopClient()
        {
            base.OnStopClient();
            Debug.Log("yns  OnStopClient ");
            playerMgr.DisRegister(this.netId);
        }



        #endregion

        /// <summary>
        /// 发出伤害事件 只有本地玩家才做得到
        /// Npc想发出伤害的话, 也只能借助本地玩家 转发 信息
        /// </summary>
        /// <param name="other"></param>
        /// <param name="triggerInfo"></param>
        public override void OnAckTrigger(Collider other, AckInfo ackInfo)
        {
            //受击对象
            MonoAttacker damager = null;
            if (other.attachedRigidbody!=null)
            {
                damager = other.attachedRigidbody.GetComponent<MonoAttacker>();
            }
            else
            {
                damager = other.GetComponent<MonoAttacker>();
            }
            if (damager != null)
            {
                //利用 本地玩家转发信息
                playerMgr.LocalPlayer.CmdOnDam(damager.netId, ackInfo);
            }
        }

        [Command]
        public void CmdOnReborn()
        {
            var newData = playerData;
            newData.hp = MaxHp;
            playerData = newData;

        }
        [ClientRpc]
        public void RcpOnReborn()
        {

        }
        [Command]
        public void CmdSetTag(AgentTag tag)
        {
            Debug.Log($"yns CmdSetTag {tag}");
            RcpSetTag(tag);
        }

        [ClientRpc]
        public void RcpSetTag(AgentTag tag)
        {
            AgentTag = tag;
            Debug.Log($"yns RcpSetTag {AgentTag}");
            playerMgr.ApplyAllAgentTag();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="DamagerNetId">受击对象</param>
        /// <param name="ackInfo"></param>
        [Command]
        public void CmdOnDam(uint DamagerNetId, AckInfo ackInfo)
        {
            Debug.Log($"yns CmdOnDam {DamagerNetId} {ackInfo}");
            //数据层在服务器中执行
            SkillSetting setting = SkillSettingMgr.Instance.GetSkillSetting(ackInfo.ackId);

            //获取受击者
            var OnDamager = playerMgr.GetAcker(DamagerNetId);

            //随机波动
            RandomAck = Random.Range(0.95f, 1.05f);
            //伤害数值
            float DamageValue = OnDamager.Ack * setting.AckRate * RandomAck; // * random;


            //结算最终血量
            int targetHp = Math.Max(Mathf.RoundToInt(OnDamager.playerData.hp - DamageValue), 0);
            //用于伤害数字显示
            ackInfo.ackValue = DamageValue;

            ackInfo.lastState = OnDamager.damageState;

            if (OnDamager.damageState == DamageState.NoBreak)
            {
                ackInfo.isBreak = false;
            }
            else if(OnDamager.damageState == DamageState.OnBreak)
            {
                ackInfo.isBreak = true;

            }
            else
            {
                OnDamager.breakPower.noBreakPower -= (int)setting.BreakPower;
                if(OnDamager.breakPower.noBreakPower <= 0)
                {
                    //OnDamager.playerData.noBreakPower = OnDamager.playerData.maxBreakPower / 2;
                    OnDamager.damageState = DamageState.OnBreak;
                    ackInfo.isBreak = true;
                }
                else
                {
                    ackInfo.isBreak = false;
                }
            }


            //修改playerData
            var newData = OnDamager.playerData;
            newData.hp = targetHp;
            OnDamager.playerData = newData;

            //执行表现层
            RcpOnDam(DamagerNetId, ackInfo);
        }


        [ClientRpc]
        public void RcpOnDam(uint DamagerNetId, AckInfo ackInfo)
        {
            //被打方 执行受击表现
            IAttacker onDamgerer = playerMgr.GetAcker(DamagerNetId);
            onDamgerer.OnDam(ackInfo);
        }

        public override void OnDam(AckInfo ackInfo)
        {
            if (IsDie)
            {
                OnDie();
                return;
            }
            OnHit(ackInfo);
        }

        private void OnDie()
        {
            //Debug
            if (IsLocalTruePlayer && DebugSo.NoDead)
            {
                CmdOnReborn();
                return;
            }

            //关闭所有 colidor
            SetAllColLayer(LayerMask.NameToLayer("Ignore Raycast"));

            AI.enabled = false;
            if (curSkillRuner != null && curSkillRuner.IsRuning)
            {
                curSkillRuner.BreakSkill();
            }
            playerMover.m_Move = Vector3.zero;
            playerMover.PlayAnim(AnimHash.Dead);
            animator.SetBool(AnimConfig.IsDie, true);
            curState = PlayerStateEnum.Dead;
            //真实玩家死亡目前与Npc相同

        }

        public virtual void OnHit(AckInfo ackInfo)
        {
            DamageState lastState = ackInfo.lastState;
            SkillSetting setting = SkillSettingMgr.Instance.GetSkillSetting(ackInfo.ackId);
            if (ackInfo.isBreak)
            {
                breakTimer.ResetTimer();
                if (curSkillRuner)
                {
                    Debug.Log($"yns Break CurSkill {curSkillRuner.BaseSkillId}");
                    //打断技能
                    curSkillRuner.BreakSkill();
                }

                playerMover.PlayAnim(AnimHash.Break);

                Vector3 pushDir = MathTool.ChageDir(ackInfo.skillDir, setting.HorForward + ackInfo.angleY);
                pushDir.y = 0;
                pushDir = pushDir.normalized;

                float dur = setting.NoGravityT * 0.9f;

                cc.DOHit(setting.AddY * PlayerMoveSettingSo.AddYRate, setting.AddHor * pushDir* PlayerMoveSettingSo.AddYRate, dur);


                //transform.rotation = Quaternion.ro(transform.rotation,)
                transform.RotaToPos(ackInfo.skillPos);

                //float hitStopRate = ResFinder.SoUsingFinder.DebugSo.floats
                HitStop.Instance.DoHitStop(setting.HitStop);

                playerMover.SetNoGravityT(setting.NoGravityT);
            }
            else
            {
                if (damageState == DamageState.Nor)
                {
                    if (curState == PlayerStateEnum.Idle)
                    {
                        playerMover.PlayAnim(AnimHash.Hit);
                        Vector3 pushDir = MathTool.ChageDir(ackInfo.skillDir, setting.HorForward + ackInfo.angleY);
                        pushDir.y = 0;
                        pushDir = pushDir.normalized;
                        float dur = setting.NoGravityT * 0.9f * 0.5f;
                        playerMover.SetUnMoveTime(0.4f);
                        cc.DOHit(0, setting.AddHor * pushDir * PlayerMoveSettingSo.AddYRate * 0.8f, dur);
                    }
                }
            }


            OnDamAciton?.Invoke(ackInfo);

            PlayHitEffect(ackInfo, setting.HitEffect);

            if (!isTruePlayer)
            {
                hitCont++;
                SoundMgr.Instance.PlayHitAudio(setting.Sound, hitCont % 6 == 5);
            }
        }

        int hitCont = 0;


        public void PlayHitEffect(AckInfo ackInfo, string effectName)
        {
            var effect = RunTimePoolManager.Instance.GetHitEffect(effectName);
            effect.SetActive(false);
            effect.SetActive(true);

            Vector3 vector3 = transform.position;
            vector3.y = ackInfo.skillPos.y;
            vector3 = Vector3.Lerp(ackInfo.skillPos, vector3, 0.2f);
            effect.transform.position = vector3;
            effect.transform.forward = ackInfo.skillPos - transform.position;

            UIMrg.Instance.PlayDamageText(ackInfo.ackValue, vector3);
        }



        //硬直回复
        [Command]
        public void CmdRecoverNoDam(float value)
        {
            var newData = breakPower;
            int add = Mathf.Max(1, (int)(newData.maxBreakPower * value));
            newData.noBreakPower = Mathf.Min(newData.maxBreakPower, add + newData.noBreakPower );
            breakPower = newData;
        }


        private void UpdateStates_Local()
        {
            if (damageState == DamageState.OnBreak)
            {
                breakTimer.Update();
                if (!breakTimer.isRuning && !playerMover.isOnBreakAnim)
                {
                    ExitBreak();
                }
            }
            else if (damageState == DamageState.NoBreak)
            {
                noDamageTimer.Update();
            }
        }

        public virtual void ExitBreak()
        {
            //当breakTime计时结束 并且 不处于OnBreakAnim则恢复自由状态
            damageState = DamageState.Nor;
            CmdRecoverNoDam(0.6f);
        }

        private void OnNoDamExit()
        {
            if (damageState == DamageState.NoBreak)
            {
                damageState = DamageState.Nor;
            }
            else
            {
                //失败则继续检测
                noDamageTimer.AddMinTime(0.02f);
            }

        }

        protected void OnDieEnd()
        {
            Debug.Log($"yns DieEnd");
            if (!isTruePlayer)
            {
                playerMgr.DisRegister(netId);
                this.OnStopClient();
                this.enabled = false;
            }

        }

        #region Skill-State-Ack
        //Skill
        //private float noSkillTime;

        private bool _iSCanSkill = true;
        public override bool IsCanNorSkill => _iSCanSkill && damageState != DamageState.OnBreak;

        private float norAckTime;
        private bool IsNextAck => norAckTime > 0;



        private int norAckIndex = 0;


        private void CheckInputSkill()
        {
            KeyCode keyCode = GetInputKeyCode();

            if (keyCode == inputSetting.Roll && damageState!= DamageState.NoBreak)
            {
                int rollIndex = playerMover.GetRollIndex(ref rollVec);
                if (playerMgr.skillCD.IsCDEnd(SkillStr.Roll))
                {
                    CmdRollTo(rollIndex, rollVec);
                    damageState = DamageState.NoBreak;
                    noDamageTimer.AddMinTime(0.2f, true);
                }
            }

            if (IsCanNorSkill)
            {
                if (inputSetting.SkillKeyMsg.ContainsKey(keyCode))
                {
                    string skilld = inputSetting.SkillKeyMsg[keyCode];
                    if (playerMgr.skillCD.IsCDEnd(skilld))
                    {
                        LocalTrue_CheckAutoLookEnemy(skilld);

                        CallPlaySkill(skilld);
                    }
                }
                if (keyCode == inputSetting.NorAck)
                {
                    if (playerMover.curMoveSpeedAnim >3f)
                    {
                        //playerMover.AutoLockEnemy();
                        CallPlaySkill(SkillStr.RunAck);
                    }
                    else
                    {
                        if (!IsNextAck)
                        {
                            norAckIndex = 0;
                        }
                        //普攻
                        AutoLockEnemy(LockEnemyRad, LockEnemyAngle);
                        CallPlaySkill(SkillStr.GetNorAckName(norAckIndex));

                        norAckIndex++;
                        norAckTime = inputSetting.NorAckTime;
                        if (norAckIndex >= 3)
                        {
                            norAckIndex = 0;
                        }
                    }
                }
                else if (keyCode == inputSetting.Jump)
                {

                }
            }
        }

        private KeyCode GetInputKeyCode()
        {
            if (Input.anyKeyDown)
            {
                foreach (KeyCode keyCode in Enum.GetValues(typeof(KeyCode)))
                {
                    if (Input.GetKeyDown(keyCode))
                    {
                        return keyCode;
                    }
                }
            }
            return KeyCode.None;
        }

        public void SkillFinish(XCEventsRunner runner)
        {
            //Debug.Log("finish skillId" + runer.skillData.skillName);
            SetState(PlayerStateEnum.Idle);

            onSkillFinish?.Invoke(runner);

            if (curState == PlayerStateEnum.Idle)
            {
                _iSCanSkill = true;
                if (runner.BaseSkillId != SkillStr.Roll)
                {
                    playerMover.isBackToIdle = true;

                }
                playerMover.SetCanMoveAndRotate(true);
                animator.speed = 1;
            }
        }


        public void SetState(PlayerStateEnum nextState)
        {
            if (curState != PlayerStateEnum.Dead)
            {
                _iSCanSkill = false;
                OnExitState(curState);
                curState = nextState;
                OnEnterState(nextState);
            }
        }

        public void OnExitState(PlayerStateEnum playerState)
        {

        }

        public void OnEnterState(PlayerStateEnum playerState)
        {

        }

        private void DestorySkillData(SkillEventData skill)
        {
            Destroy(skill.gameObject, 5);
        }


        //技能辅助瞄准
        private void LocalTrue_CheckAutoLookEnemy(string skilld)
        {
            var skillSettingDic = SkiillKeyCodeSo.GetDic();
            skillSettingDic.TryGetValue(skilld, out SkillKey skillConfig);
            if (skillConfig != null)
            {
                if (skillConfig.AutoLookAngle > 0)
                {
                    var m_CamForward = Camera.main.transform.forward;
                    m_CamForward.y = 0;
                    transform.forward = m_CamForward;
                    AutoLockEnemy(skillConfig.AutoLookR, skillConfig.AutoLookAngle);
                }
            }
        }

        //SearchEnemy FindEnemy
        public bool AutoLockEnemy(float seeR = 15, float seeAngle = 90, float hearR = 0)
        {
            var enemy = SearchEnemy(seeR, seeAngle, hearR);

            transform.RotaToTarget_Com(enemy);

            return enemy != null;
        }

        #endregion


        #region  Cmd&Rcp

        [Command]
        public void CmdMove(Vector3 m_Move, bool CanMove, bool CanRotate, Vector3 targetPos, Vector3 targetForward)
        {
            RcpMove(m_Move, CanMove, CanRotate, targetPos, targetForward);
        }
        [ClientRpc]
        private void RcpMove(Vector3 m_Move, bool CanMove, bool CanRotate, Vector3 targetPos, Vector3 targetForward)
        {
            if (IsLocal)
            {
                return;
            }
            if (!isStarted)
            {
                transform.position = targetPos;
                return;
            }
            //bool isForward = Vector3.Dot(transform.forward, m_Move) > 0;
            transform.forward = Vector3.Lerp(transform.forward, targetForward, 0.5f);
            
            //远端玩家不执行
            //playerMover.OnNetMove_AutoRotate(m_Move, CanRotate);
            
            playerMover.OnNetMove(m_Move, CanMove);
            transform.position = Vector3.Lerp(transform.position, targetPos, 0.5f);
        }

        [ClientRpc]
        private void RcpRollTo(int rollAnim, Vector3 vector3)
        {
            if (curSkillRuner && curSkillRuner.IsRuning)
            {
                Debug.Log($"yns Break CurSkill {curSkillRuner.BaseSkillId}");
                curSkillRuner.BreakSkill();
            }
            playerMover.DoRoll(0);
            transform.forward = vector3;
            CallPlaySkill(SkillStr.Roll);
        }
        [Command]
        private void CmdRollTo(int rollAnim, Vector3 vector3)
        {
            RcpRollTo(rollAnim, vector3);
            CmdRecoverNoDam(0.4f);
        }


        //通用消息类型
        [Command]
        public void SendNetMessage(PlayerNetEventName eventName, PlayerMessge messge)
        {
            OnMessage(eventName, messge);
        }
        [ClientRpc]
        public void OnMessage(PlayerNetEventName eventName, PlayerMessge messge)
        {
            Debug.LogFormat("event:{0} messge:{1} ", eventName, messge.ToString());

            if (eventName == PlayerNetEventName.Invoke)
            {
                Invoke(messge.strMes, 0);
            }
            else if (eventName == PlayerNetEventName.UpDateHp)
            {
                playerData.hp = messge.intMes;
                playerMgr.UpdatePlayerValue();
            }
        }

        //技能

        public void CallPlaySkill(string skillId)
        {
            CmdPlaySkill_True(skillId,transform.eulerAngles,transform.position);
        }
        [Command]
        public void CmdPlaySkill_True(string skillId, Vector3 castEuler,Vector3 castPos)
        {
            CmdRecoverNoDam(0.1f);
            RcpSKill(skillId, castEuler, castPos);
        }

        [ClientRpc]
        public void RcpSKill(string skillId, Vector3 castEuler, Vector3 castPos)
        {
            if (IsLocal)
            {
                //技能cd重新计时
                playerMgr.skillCD.ReCountCD(skillId);
                playerMover.curMoveSpeedAnim = 0;
            }
            Debug.Log("yns  " + netId + " RcpSKill " + skillId);
            var _skillPrefab = ResFinder.GetSkillData(skillId,agentType.IsEnemy()); //不可共用


            if (_skillPrefab == null)
            {
                Debug.LogError($"yns no Skill " + skillId);
                return;
            }
            SkillEventData _skill = Instantiate(_skillPrefab);


            if (curSkillRuner != null)
            {
                curSkillRuner.Finish();
            }
            playerMover.SetCanMoveAndRotate(false);
            //Debug.Log("yns   SkillOwenr.layer " + SkillOwenr.layer);

            lastSkillRuner = curSkillRuner;
            curSkillRuner = SkillLauncher.StartPlayerSkill(_skill,SkillOwenr, castEuler, castPos);
            SetState(PlayerStateEnum.PlayerSkill); //无所谓 暂时都是skill吧

            var skillSettingDic = SkiillKeyCodeSo.GetDic();
            skillSettingDic.TryGetValue(skillId, out SkillKey skillConfig);
            if (skillConfig != null && skillConfig.noBreakTime > 0)
            {
                damageState = DamageState.NoBreak;
                noDamageTimer.AddMinTime(skillConfig.noBreakTime, true);
            }

            curSkillRuner.onFinishEvent.AddListener(() =>
            {
                SkillFinish(curSkillRuner);
                DestorySkillData(_skill);
                if (skillId == "3")
                {
                    OnNoDamExit();
                }
            });
        }


        [Command]
        public void CmdRotaToPos(Vector3 pos)
        {
            RcpRotaToPos(pos);
        }
        [ClientRpc]
        public void RcpRotaToPos(Vector3 pos)
        {
            transform.RotaToPos(pos);
        }

        #endregion


        #region Render
        private void SetRenderActive(bool active)
        {
            Debug.Log($"yns SetRenderActive {active} ");
            foreach (var item in renders)
            {
                item.enabled = active;
            }
        }

        //还得考虑mod热更的话 用加载model预制体也行?
        public void ChangeSkin(PlayerSkin playerSkin)
        {
            string skinName = playerSkin.ToString();
            //切换之前 要把自身皮肤隐藏
            //Transform oldSkinTF = transform.Find("Skin" + index);
            skin = playerSkin;
            if (animator != null)
            {
                animator.gameObject.SetActive(false);
            }
            //切换之后显示新皮肤 修改当前动画机
            Transform skinTF = transform.Find(skinName);
            skinTF.gameObject.SetActive(true);
            renders = skinTF.GetComponentsInChildren<Renderer>();
            animator = skinTF.GetComponent<Animator>();

        }

        #endregion

        #region LocalMsg
        public override void SetBool(string name, bool msg)
        {
            if (name == PlayEventMsg.SetCanMove || name == PlayEventMsg.SetCanRotate)
            {
                playerMover.SetBool(name, msg);
            }
            else if (PlayEventMsg.ActivePlayerRender == name)
            {
                SetRenderActive(msg);
            }
        }

        public override void SendAll(string name,float num, bool isOn, string msg)
        {
            //if (PlayEventMsg.LookEnemy == name)
            //{
            //    if (IsLocal)
            //    {
            //       var target = SearchEnemy(LockEnemyAngle, LockEnemyRad);
            //        if (target != null)
            //        {
            //            CmdRotaToPos(target.transform.position);
            //        }
            //    }
            //}
            if(name == PlayEventMsg.TimeStop)
            {
                HitStop.Instance.DoHitStop(num);
            }
            else if (name == PlayEventMsg.SetNoGravityT)
            {
                Debug.Log($"yns PlayEventMsg.SetNoGravityT {num}");
                playerMover.SetNoGravityT(num);
            }                     
            else if (name == PlayEventMsg.PlayAudio)
            {
                PlayAudio(msg, num);
            }            
            else if (name == PlayEventMsg.SetNoBreakTime)
            {
                Debug.Log($"yns PlayEventMsg.SetNoGravityT {num}");
                damageState = DamageState.NoBreak;
                noBreakTimer.AddMinTime(num);
            }
            else if (name == PlayEventMsg.SetUnMoveTime)
            {
                playerMover.SetUnMoveTime(num);
            }
        }

        #endregion

        private void PlayAudio(string id,float volume = 1)
        {
            SoundMgr.Instance.PlayAudio(id, volume,netId);
        }

        [ContextMenu("AddIA")]
        public void ActiveAI()
        {
            AI.Init(this);
            AI.enabled = true;

        }
        [ContextMenu("SwitchAI")]
        public void SwitchAI()
        {
            var AIs = new List<AI>(GetComponentsInChildren<AI>());
        
            AI = AIs.Find((a)=> a != AI);

            Debug.Log($"yns {AI}");
        }


        public override void AIMoveTo(Vector3 dir, float speed = 1, bool isAutoRotate = true)
        {
            playerMover.m_Move = dir.normalized * speed;
            playerMover.isLocalAutoRotate = isAutoRotate;
        }

        //转动动画
        public override void SetSlowRoteAnim(bool isLeft)
        {
            if (playerMover.curMoveSpeedAnim <1.5)
            {
                playerMover.curMoveSpeedAnim = Mathf.Lerp(playerMover.curMoveSpeedAnim, 1.2f, 0.1f);
            }
        }

        public override bool SetAIEvent(ActMsgType type, string msg, object other = null)
        {
            if (type == ActMsgType.Skill)
            {
                CallPlaySkill(msg);
                return true;
            }
            else if (type == ActMsgType.OtherSkill)
            {
                if (msg == "NoBreak_Roll")
                {
                    rollVec = transform.forward;
                    CmdRollTo(0, rollVec);
                    damageState = DamageState.NoBreak;
                    noDamageTimer.AddMinTime(0.5f, true);
                    Debug.Log($"yns NoBreak");
                    return true;
                }
            }
            return false;
        }


#if UNITY_EDITOR
        private void OnValidate()
        {
            if (AI == null)
            {
                AI = GetComponentInChildren<AI>();
            }
        }
#endif

    }

}
