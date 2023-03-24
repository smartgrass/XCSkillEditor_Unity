using System.Collections;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

namespace XiaoCao
{
    public class UIMrg :MonoBehaviour
    {
        protected static UIMrg _instance = null;

        public static UIMrg Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Instantiate( Resources.Load<UIMrg>(PrefabPath.UIMrg));
                    DontDestroyOnLoad(_instance);
                }
                return _instance;
            }
        }

        public Dictionary<SingletonUIType, UIState> uiStateDic = new Dictionary<SingletonUIType, UIState>();

        public Dictionary<SingletonUIType, UIBase> singletonUITDic = new Dictionary<SingletonUIType, UIBase>();

        public Canvas[] canvasList;

        public MainUIPanel mainUIPanel;

        private void Awake()
        {
            _instance = this;
            DontDestroyOnLoad(this);
            Init();
        }

        private void Init()
        {
            canvasList = transform.GetComponentsInChildren<Canvas>();
            if(canvasList.Length < Enum.GetValues(typeof(UICanvasParent)).Length)
            {
                Debug.LogError("no enough cans");
            }
        }

        public UIBase CallPanel(SingletonUIType uiType, UICanvasParent canvasParent = UICanvasParent.Mid, bool isMoveTop = true)
        {
            UIState uIState = GetUIState(uiType);
            if (uIState == UIState.Null)
            {
                var ui = Instantiate(Resources.Load<UIBase>(PrefabPath.SingletonUI(uiType)));
                ui.transform.SetParent(GetCanvasParenet(canvasParent));
                ui.InitCanvas(canvasList[(int)uiType]);
                if (isMoveTop)
                    ui.transform.SetAsLastSibling();

                return ui;
            }
            else
            {
                return singletonUITDic[uiType];
            }
        }


        private void Hide(SingletonUIType uiType)
        {
            UIState uIState = GetUIState(uiType);
            if (uIState != UIState.Null)
            {
                singletonUITDic[uiType].Hide();
            }
        }
        private void Show(SingletonUIType uiType)
        {
            UIState uIState = GetUIState(uiType);
            if (uIState != UIState.Null)
            {
                singletonUITDic[uiType].Show();
            }
        }
        private void DestroyUI(SingletonUIType uiType)
        {
            UIState uIState = GetUIState(uiType);
            if (uIState != UIState.Null)
            {
                var ui = singletonUITDic[uiType];
                GameObject.Destroy(ui.gameObject);
                uiStateDic.Remove(uiType);
                singletonUITDic.Remove(uiType);
            }
        }


        private UIState GetUIState(SingletonUIType uiType)
        {
            if (uiStateDic.ContainsKey(uiType) && singletonUITDic.ContainsKey(uiType))
            {
                return uiStateDic[uiType];
            }
            return UIState.Null;
        }

        private Transform GetCanvasParenet(UICanvasParent type)
        {
            if(canvasList.Length > (int)type)
            {
                return canvasList[(int)type].transform;
            }
            return null;
        }

        public void PlayDamageText(float damageValue, Vector3 vector3)
        {        
            mainUIPanel.ShowDamageText(string.Format("-{0}", (int)damageValue), vector3);
        }
    }

    public enum SingletonUIType
    {
        FloatPanel,

    }
    public enum UIState
    {
        Null,
        OnShow,
        OnHide
    }
    public enum UICanvasParent:int
    {
        Lowest,
        Low,
        Mid,
        Top,
        Topest
    }


}