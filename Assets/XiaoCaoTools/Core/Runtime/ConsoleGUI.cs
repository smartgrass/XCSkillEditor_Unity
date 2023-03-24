using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Consolation
{
    /// <summary>
    /// A console to display Unity's debug logs in-game.
    /// </summary>
    class ConsoleGUI : MonoBehaviour
    {
        #region system
        private static ConsoleGUI instance;
        public static ConsoleGUI Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject obj = new GameObject("ConsoleGUI");
                    instance = obj.AddComponent<ConsoleGUI>();
                    DontDestroyOnLoad(obj);
                }
                return instance;
            }
        }

        public static string SearchKey = "";

        struct Log
        {
            public string message;
            public string stackTrace;
            public LogType type;
        }

        #endregion

        #region Inspector Settings
        public bool ShowHideButon;


        //限制log数量
        public int maxLogs = 350;

        public KeyCode HotKey = KeyCode.F12;
        [Header("Window")]
        [Range(0,1)]
        public float WinW =1f;
        [Range(0, 1)]
        public float WinH = 0.7f;
        public float WinTop = 32;
        public float SrollWith = 25;
        public int DownBarLineHight = 35;

        [Header("Button")]
        public float BtnW = 100;
        public float BtnH = 50;
        public float BtnSpace = 15;

        [Header("Toggle")]
        public float toggleH =40;
        public float toggleW =100;
        public float toggleOffsetY =10;
        public int toggleFontSize =16;
        private RectOffset toggleOffset;


        #endregion

        #region system
        readonly List<Log> logs = new List<Log>();
        Vector2 scrollPosition;

        bool collapse = true;
        bool isErrorDetail;
        bool isNorLogDetail = true;
        static bool isFtr;
        public bool isVisible = true;
        public bool isHide = false;
        //public RectOffset toggleBorder;
        //public RectOffset toggleMar;
        //public RectOffset togglePad;
        //public Texture2D texture2;
        // Visual elements:

        static readonly Dictionary<LogType, Color> logTypeColors = new Dictionary<LogType, Color>
        {
            { LogType.Assert, Color.white },
            { LogType.Error, Color.red },
            { LogType.Exception, Color.red },
            { LogType.Log, Color.white },
            { LogType.Warning, Color.yellow },
        };

        const string windowTitle = "Console";
        const int margin = 10;
        static readonly GUIContent clearLabel = new GUIContent("Clear", "Clear the contents of the console.");
        static readonly GUIContent collapseLabel = new GUIContent("Collapse  ");
        static readonly GUIContent fieldLabel = new GUIContent("ErrorShow  ");
        static readonly GUIContent NorLabel = new GUIContent("NorShow  ");
        static readonly Rect titleBarRect = new Rect(0, 0, 10000, 20);
        static GUIStyle lableStyle = new GUIStyle();
        Rect windowRect;
        static GUIStyle btnStyle;
        static GUIStyle toggleStyle;



        bool isStarted = false;

        private void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
        }

        void OnEnable()
        {
            instance = this;
            Application.logMessageReceived += HandleLog;
        }

        void OnDisable()
        {
            Application.logMessageReceived -= HandleLog;
        }

        private void SetStyle()
        {
            windowRect = new Rect(Screen.width * (1-WinW)/2, BtnH+ WinTop, Screen.width *WinW, Screen.height * WinH);
            btnStyle = new GUIStyle(GUI.skin.button);
            toggleStyle = GUI.skin.toggle;
            toggleOffset = toggleStyle.margin;
            isStarted = true; ;
        }

        #endregion

        private void Update()
        {
            if (Input.GetKeyDown(HotKey))
            {
                isHide = isVisible;
                isVisible = !isVisible;
            }

        }
        void OnGUI()
        {

            if (isHide)
                return;
            if (!isStarted)
                SetStyle();
            btnStyle.fixedHeight = BtnH;
            btnStyle.fixedWidth = BtnW;

            windowRect.width = Screen.width * WinW;
            windowRect.height = Screen.height * WinH;
            windowRect.y = BtnH + WinTop;


            GUILayout.Space(20);

            GUILayout.BeginHorizontal();
            {
                int orginBtnSize = GUI.skin.button.fontSize;

                GUI.skin.button.fontSize = 20;

                GUILayout.Space(BtnSpace);
                if (GUILayout.Button("Log", btnStyle))
                {
                    isVisible = !isVisible;
                }

                GUILayout.Space(BtnSpace);
                if (ShowHideButon)
                {
                    if (GUILayout.Button("Hide", btnStyle))
                    {
                        isHide = true;
                    }
                }

                //GUILayout.Space(BtnSpace);
                //if (GUILayout.Button("do", btnStyle))
                //{
                //    Debug.Log($"yns  dosometing");
                //}
                //GUILayout.Space(BtnSpace);

                GUI.skin.button.fontSize = 14;
            }

            GUILayout.EndHorizontal();


            if (!isVisible)
                return;
            //windowRect =
                GUILayout.Window(123456, windowRect, DrawConsoleWindow, windowTitle);



        }


        /// <summary>
        /// Displays a window that lists the recorded logs.
        /// </summary>
        /// <param name="windowID">Window ID.</param>
        void DrawConsoleWindow(int windowID)
        {
            DrawLogsList();
            DrawToolbar();
            // Allow the window to be dragged by its title bar.
            GUI.DragWindow(titleBarRect);
        }

        /// <summary>
        /// Displays a scrollable list of logs.
        /// </summary>
        void DrawLogsList()
        {
            GUIStyle verBarStyle = GUI.skin.verticalScrollbar;
            GUIStyle verBarThumbStyle = GUI.skin.verticalScrollbarThumb;


            GUIStyle horBarStyle = GUI.skin.horizontalScrollbar;
            GUIStyle horBarThumbStyle = GUI.skin.horizontalScrollbarThumb;

            var orginWidth = verBarStyle.fixedWidth;
            var orginWidth1 = verBarThumbStyle.fixedWidth;
            //var orginOffset = gs1.margin.left;


            verBarStyle.fixedWidth = SrollWith;
            verBarThumbStyle.fixedWidth = SrollWith;
            //gs1.margin.left = 0;
            //gs_2.fixedWidth = 0;
            horBarStyle.fixedHeight = SrollWith;
            horBarThumbStyle.fixedHeight = SrollWith;


            scrollPosition = GUILayout.BeginScrollView(scrollPosition, "Box");
            // Iterate through the recorded logs.
            int len = logs.Count;
            for (var i = len - 1; i >= 0; i--)
            {
                var log = logs[i];

                // Combine identical messages if collapse option is chosen.
                if (collapse && i > 0)
                {
                    var previousMessage = logs[i - 1].message;

                    if (log.message == previousMessage)
                    {
                        continue;
                    }
                }
                lableStyle.normal.textColor = logTypeColors[log.type];
                lableStyle.fontSize = 25;

                isFtr = !string.IsNullOrEmpty(SearchKey);

                DrawLabel(log.message);
                if (log.type == LogType.Error || log.type == LogType.Exception)
                {
                    if (!isErrorDetail)
                    {
                        lableStyle.normal.textColor = new Color(0.6f, 0, 0);
                        DrawLabel(log.stackTrace);
                    }
                }
                else
                {
                    if (!isNorLogDetail)
                    {
                        lableStyle.normal.textColor = Color.gray;
                        DrawLabel(log.stackTrace);
                    }
                }
                lableStyle.fontSize = 20;

            }

            verBarStyle.fixedWidth = orginWidth;
            verBarThumbStyle.fixedWidth = orginWidth1;
            horBarStyle.fixedHeight = orginWidth;
            horBarThumbStyle.fixedHeight = orginWidth1;
            //gs1.margin.left = orginOffset;
            GUILayout.EndScrollView();

            // Ensure GUI colour is reset before drawing other components.
            GUI.contentColor = Color.white;
        }



        private static void DrawLabel(string log)
        {
            if (isFtr)
            {
                if (log.ToLower().Contains(SearchKey.ToLower()))
                    GUILayout.Label(log, lableStyle);
            }
            else
            {
                GUILayout.Label(log, lableStyle);
            }
        }

        /// <summary>
        /// Displays options for filtering and changing the logs list.
        /// </summary>
        void DrawToolbar()
        {
            GUILayout.BeginHorizontal(GUILayout.Height(DownBarLineHight));
            if (GUILayout.Button(clearLabel, btnStyle))
            {
                logs.Clear();
            }
            GUILayout.Space(20);

            toggleStyle.fixedWidth = toggleW;
            toggleStyle.fixedHeight = toggleH;
            toggleStyle.fontSize = (int)toggleFontSize;
            toggleOffset.top = (int)toggleOffsetY;
            toggleStyle.margin = toggleOffset;


            collapse = GUILayout.Toggle(collapse, collapseLabel, toggleStyle);

            isErrorDetail = GUILayout.Toggle(isErrorDetail, fieldLabel, toggleStyle);

            isNorLogDetail = GUILayout.Toggle(isNorLogDetail, NorLabel, toggleStyle);

            if (GUILayout.Button("SaveLog", btnStyle))
            {
                CopyLog();
            }


            GUILayout.Space(5);

            SearchKey = GUILayout.TextField(SearchKey);

            GUILayout.EndHorizontal();
        }

        private void CopyLog()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(DateTime.Now.ToString("yyyyMMdd hh:mm:ss"));
            sb.Append("\n");
            for (var i = 0; i < logs.Count; i++)
            {
                var log = logs[i];
                sb.Append(log.message);
                sb.Append("\n Detail: \n");
                sb.Append(log.stackTrace);
                sb.Append("\n");      
            }
            WriteLogToFile(sb.ToString());
        }

        private void WriteLogToFile(string m_logStr)
        {
            string m_logFileSavePath = string.Format("{0}/output.log", Application.persistentDataPath);
            if (m_logStr.Length <= 0) return;
            Debug.Log("read to:  " + m_logFileSavePath);
            if (!File.Exists(m_logFileSavePath))
            {
                var fs = File.Create(m_logFileSavePath);
                fs.Close();
            }
            using (var sw = File.AppendText(m_logFileSavePath))
            {
                sw.WriteLine(m_logStr.ToString());
                m_logStr.Remove(0, m_logStr.Length);
            }
        }


        void HandleLog(string message, string stackTrace, LogType type)
        {
            logs.Add(new Log
            {
                message = message,
                stackTrace = stackTrace,
                type = type,
            });

            TrimExcessLogs();
        }

        void TrimExcessLogs()
        {
            var amountToRemove = Mathf.Max(logs.Count - maxLogs, 0);

            if (amountToRemove == 0)
            {
                return;
            }

            logs.RemoveRange(0, amountToRemove);
        }


    }


}