using System.Collections;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text;
using DG.Tweening;
using System;
using Random = UnityEngine.Random;

namespace XiaoCao
{
    public class MainUIPanel : UIBase
    {
        #region UI
        public Transform CustomHpBarParent;
        public UIBar localUIBar;

        public Transform DamageTextParent;
        public Transform skillIconParent;
        public Transform disSkillIconParent;

        public DamageTextSetting DamageUITSetting;

        public List<Text> DamageTexts;

        //可以考虑加入 被动技能如翻滚
        private List<SkillIcon> skillIcons = new List<SkillIcon>();
        private List<SkillIcon> disSkillIcons = new List<SkillIcon>();

        private List<DamageTextTween> DamageTextTweens = new List<DamageTextTween>();

        private AgentModelType modelType = AgentModelType.Player;

        private int nextText;

        private Vector2 changeVec2;

        #endregion

        #region prefab
        //public GameObject uiBarPrefab_local;
        public GameObject uiBarPrefab;
        public SkillIcon skillIconPrefab;

        #endregion



        #region data
        //改为netID 比较安全
        public Dictionary<uint, UIBar> uiBarDic = new Dictionary<uint, UIBar>();
        public TypePool<UIBar> barPool;



        #endregion


        public PlayerMgr playerMrg => PlayerMgr.Instance;


        private bool isReady = false;

        void Awake()
        {
            InitCanvas(GetComponentInParent<Canvas>());
            //Manager.AddListener(ClientEventType.Change, OnPlayerChange);
            playerMrg.AddListener(ClientEventType.ValueChange, OnPlayerValueChange);

            barPool = new TypePool<UIBar>(uiBarPrefab.GetComponent<UIBar>(), OnRecyleUIBar);
        }


        private void Start()
        {
            foreach (var item in DamageTexts)
            {
                DamageTextTweens.Add(new DamageTextTween { text = item });
            }
            playerMrg.AddPlayerStartedAction(WaitStart);
            playerMrg.removeAckerAction += RemoveOne;
        }
        private void WaitStart()
        {
            isReady = true;
            InitSkillIcons();
        }

        private void InitSkillIcons()
        {


            //主动技能个数补全
            int skillCount = 5;
            var allSkillIcon = skillIconParent.GetComponentsInChildren<SkillIcon>();
            skillIcons.AddRange(allSkillIcon);
            int needCount = skillCount - allSkillIcon.Length;
            if (needCount > 0)//技能数未定
            {
                for (int i = 0; i < needCount; i++)
                {
                    SkillIcon skillIcon = Instantiate<SkillIcon>(skillIconPrefab, skillIconParent);
                    skillIcons.Add(skillIcon);
                }
            }
            //被动技能个数补全
            int disSkillCount = 5;
            var allDisSkillIcon = disSkillIconParent.GetComponentsInChildren<SkillIcon>();
            disSkillIcons.AddRange(allDisSkillIcon);
            int disNeedCount = disSkillCount - allDisSkillIcon.Length;
            if (disNeedCount > 0)//技能数未定
            {
                for (int i = 0; i < disNeedCount; i++)
                {
                    SkillIcon skillIcon = Instantiate<SkillIcon>(skillIconPrefab, disSkillIconParent);
                    disSkillIcons.Add(skillIcon);
                }
            }


            foreach (var item in skillIcons)
            {
                item.gameObject.SetActive(false);
            }

            foreach (var item in disSkillIcons)
            {
                item.gameObject.SetActive(false);
            }

            int x = 0;
            foreach (var item in ResFinder.SoUsingFinder.SkiillKeyCodeSo.GetActiveSkills())
            {
                skillIcons[x].Init(item.skillId);
                skillIcons[x].gameObject.SetActive(true);
                x++;
            }

            int y = 0;
            foreach (var item in ResFinder.SoUsingFinder.SkiillKeyCodeSo.GetDisActiveSkills())
            {
                disSkillIcons[y].Init(item.skillId);
                y++;
            }

        }

        private void OnDestroy()
        {
            //Manager.RemoveListener(ClientEventType.Change, OnPlayerChange);
            playerMrg.RemoveListener(ClientEventType.ValueChange, OnPlayerValueChange);
        }


        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F2))
            {
                var player = playerMrg.LocalPlayer;
                Vector3 forward = player.transform.forward;
                forward.y = 0;

                playerMrg.AddFakePlayer(player.transform.position + forward * 5, GameSetting.HasAIEnable, AgentTag.enemy, modelType);
            }

            if (Input.GetKeyDown(KeyCode.F3))
            {
                var player = playerMrg.LocalPlayer;
                Vector3 forward = player.transform.forward;
                forward.y = 0;

                playerMrg.AddFakePlayer(player.transform.position + forward * 5, GameSetting.HasAIEnable, AgentTag.PlayerA, modelType);
            }
            if (Input.GetKeyDown(KeyCode.F5))
            {
                //playerMrg.LocalPlayer.skin
                int len = Enum.GetValues(typeof(PlayerSkin)).Length;
                int next = (int)playerMrg.LocalPlayer.skin + 1;
                if (next >= len)
                {
                    next = 0;
                }
                playerMrg.LocalPlayer.ChangeSkin((PlayerSkin)next);
            }

            //需要的功能 (AI开关)
            if (Input.GetKeyDown(KeyCode.F1))
            {
                bool isEnbale = true;
                bool isFrist = true;
                foreach (var item in playerMrg.MonoAttackerDic.Values)
                {
                    if (!item.isTruePlayer)
                    {
                        if (isFrist)
                        {
                            isEnbale = !item.AI.enabled;
                            isFrist = false;
                            Debug.Log($"yns AI enble {isEnbale} count {playerMrg.MonoAttackerDic.Count}");
                        }
                        GameSetting.HasAIEnable = isEnbale;
                        item.AI.enabled = isEnbale;
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.F4))
            {
                var enums = Enum.GetValues(typeof(AgentModelType));
                int len = enums.Length;
                modelType = (AgentModelType)(((int)modelType + 1) % len);
                ShowDamageText(modelType.ToString(), PlayerMgr.Instance.LocalPlayer.transform.position,true);
            }

            if (Input.GetKeyDown(KeyCode.F6))
            {
                int len = playerMrg.MonoAttackerDic.Count;
                Debug.Log($"len = {len}");

                foreach (var item in playerMrg.MonoAttackerDic)
                {
                    Debug.Log($"yns {item.Key} {item.Value.gameObject}");
                }
            }

            foreach (var item in skillIcons)
            {
                item.OnUpdate();
            }
            foreach (var item in disSkillIcons)
            {
                item.OnDisUpdate();
            }
        }

        private void FixedUpdate()
        {
            if (playerMrg.IsLocalPlayerReady && isReady)
            {
                UpdateUIBars();
            }
        }

        private void RemoveOne(uint netID)
        {
            if (uiBarDic.ContainsKey(netID))
            {
                //Debug.LogError($"yns bar RemoveOne netID {netID}");
                barPool.Recyle(uiBarDic[netID]);
                uiBarDic.Remove(netID);
            }
            else
            {
                Debug.LogWarning($"yns bar RemoveOne no netID {netID}");
            }
        }
        private void OnPlayerValueChange(uint netID)
        {
            //TODO 暂时偷懒
            UpdateUIBars();
        }
        [NaughtyAttributes.Button("当前角色")]
        private void fun1()
        {
            playerMrg.MonoAttackerDic.Keys.IELogStr();
            foreach (var item in playerMrg.MonoAttackerDic.Values)
            {
                item.gameObject.transform.name.LogStr();
            }

        }

        public void UpdateUIBars()
        {
            foreach (var kv in playerMrg.MonoAttackerDic)
            {
                var item = kv.Value;
                var netId = kv.Key;
                if (item == null)
                {
                    RemoveOne(netId);
                    playerMrg.MonoAttackerDic.Remove(netId);
                }
                else
                {
                    if (item.IsHideHpBar)
                    {
                        RemoveOne(netId);
                    }
                    else
                    {
                        if (!uiBarDic.ContainsKey(netId))
                        {
                            AddNewBar(item);
                        }
                        uiBarDic[netId].SetFillValue(item.Hp, item.MaxHp);
                        uiBarDic[netId].SetFillValueNoBreak(item.NoBreakPower, item.MaxBreakPower);
                        uiBarDic[netId].SetTagUI(item.AgentTag);
                        uiBarDic[netId].OnUpdate();
                    }
                }
            }
        }



        private void AddNewBar(MonoAttacker item)
        {
            UIBar newUIBar = null;
            if (item.IsLocal)
            {
                newUIBar = localUIBar;
                newUIBar.SetTarget(item.TopTranform);
            }
            else
            {
                newUIBar = barPool.GetOne();
                newUIBar.gameObject.SetActive(true);
                newUIBar.transform.SetParent(CustomHpBarParent, true);
                newUIBar.transform.localScale = Vector3.one;
                newUIBar.SetTarget(item.TopTranform);
            }
            newUIBar.InitCanvas(canvas);
            uiBarDic.Add(item.netId, newUIBar);
            newUIBar.SetTagUI(item.AgentTag);
            Debug.Log($"yns add uiBar {item.netId} {item.AgentTag}");
        }

        public void ShowDamageText(string num, Vector3 mTarget, bool isBlod = false)
        {


            //获取屏幕坐标  
            Vector3 mScreen = Camera.main.WorldToScreenPoint(mTarget);
            if (mScreen.z > 0)
            {
                Text t = DamageTextTweens[nextText].text;

                Sequence tween = DamageTextTweens[nextText].tween;

                if (tween != null)
                {
                    tween.Kill();
                }

                DamageTextTweens[nextText].tween = DOTween.Sequence();
                tween = DamageTextTweens[nextText].tween;

                nextText++;
                if (nextText >= DamageTexts.Count)
                {
                    nextText = 0;
                }

                changeVec2 = Vector2.Scale(DamageUITSetting.randomVec2, Random.insideUnitCircle);  //波动值
                changeVec2 += DamageUITSetting.offSet;

                mScreen.x += changeVec2.x;
                mScreen.y += changeVec2.y;
                t.transform.localScale = Random.Range(DamageUITSetting.randomScaleVec2.x, DamageUITSetting.randomScaleVec2.y) * Vector3.one;

                float randomY = DamageUITSetting.MoveY * Random.Range(DamageUITSetting.randomScaleVec2.x, DamageUITSetting.randomScaleVec2.y);

                t.text = num;
                t.transform.position = mScreen;
                //tween.SetEase(DamageUITSetting.ease);
                //大小
                tween.Join(DOTween.To(x => t.fontSize = (int)x, DamageUITSetting.frontSizeStart, DamageUITSetting.frontSizeEnd, DamageUITSetting.flyTime / 2).SetLoops(2, LoopType.Yoyo));

                //位置
                tween.Join(t.transform.DOMoveY(mScreen.y + DamageUITSetting.MoveY, DamageUITSetting.flyTime / 2));
                //颜色
                t.color = DamageUITSetting.startColor;
                tween.Join(t.DOColor(DamageUITSetting.endColor, DamageUITSetting.flyTime / 2));


                //tween.Append(DOTween.To(x => t.fontSize = (int)x, DamageUITSetting.frontSizeMid , DamageUITSetting.frontSizeStart, DamageUITSetting.flyTime/2));

                Color ac = Color.white;
                ac.a = 0;
                tween.OnComplete(() => { t.color = ac; });

                gameObject.SetActive(true);
            }
        }

        private void OnRecyleUIBar(UIBar bar)
        {
            bar.gameObject.SetActive(false);
            Debug.Log($"yns OnRecyleUIBar");
        }

    }

    public class DamageTextTween
    {
        public Text text;
        public Sequence tween;
    }


    [System.Serializable]
    public class DamageTextSetting
    {
        public Ease ease;
        public float frontSizeStart = 10;
        public float frontSizeEnd = 32;

        public float MoveY = 5;
        public float flyTime = 0.5f;
        public Color startColor;
        public Color endColor;
        public Vector2 randomVec2;
        public Vector2 randomScaleVec2;
        public Vector2 offSet;
    }


    public static class UITool
    {

        public static Vector2 WorldToAnchorPos(Vector3 position, RectTransform canvasRectTransform)
        {
            Vector3 screenPoint3 = Camera.main.WorldToScreenPoint(position);//世界坐标转换为屏幕坐标
            if (screenPoint3.z < 0)
            {
                screenPoint3 = -screenPoint3;
            }
            Vector2 screenPoint = screenPoint3;
            Vector2 screenSize = new Vector2(Screen.width, Screen.height);
            screenPoint -= screenSize / 2;//将屏幕坐标变换为以屏幕中心为原点
            Vector2 anchorPos = screenPoint / screenSize * canvasRectTransform.sizeDelta;//缩放得到UGUI坐标
            return anchorPos;
        }

        public static Vector3 WorldToUiPostion(Vector3 position, Camera cam = null)
        {
            if (cam == null)
            {
                cam = Camera.main;
            }
            Vector3 screenPoint3 = cam.WorldToScreenPoint(position);//世界坐标转换为屏幕坐标
            if (screenPoint3.z < 0)
            {
                screenPoint3 = -screenPoint3;
            }
            return screenPoint3;
        }

    }
}