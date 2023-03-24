using UnityEngine;
using UnityEditor;

using System;
using System.IO;
using System.Collections.Generic;

using Flux;

using Object = UnityEngine.Object;
using System.Reflection;
using XiaoCao;

namespace FluxEditor
{
    public enum TimeFormat
    {
        Frames = 0,
        Seconds,
        SecondsFormatted
    }

    public class FSequenceEditorWindow : EditorWindow
    {
        public const string MENU_PATH = "Window/";
        public const string PRODUCT_NAME = "Flux";

        public Action<KeyCode> inputAct;

        #region Menus		
        [MenuItem(MENU_PATH + PRODUCT_NAME + "/Open Editor %&c", false, 0)]
        public static void Open()
        {
            FSequenceEditorWindow window = GetWindow<FSequenceEditorWindow>();
            window.Show();

            window.titleContent = new GUIContent(PRODUCT_NAME);

            window.Update();
        }
        public static void Open(FSequence sequence)
        {
            Open();

            instance._sequenceEditor.OpenSequence(sequence);
        }

        [MenuItem(MENU_PATH + PRODUCT_NAME + "/Create Sequence", false, 100)]
        public static FSequence CreateSequence()
        {
            // find new name & priority for sequence
            string sequenceNameFormat = "Sequence_{0}";

            int sequenceId = 0;

            string sequenceName = string.Format(sequenceNameFormat, sequenceId.ToString("000"));

            FSequence[] sequences = FindObjectsOfType<FSequence>();
            for (int i = 0, limit = sequences.Length; i != limit; ++i)
            {
                if (sequences[i].name == sequenceName)
                {
                    // try new name
                    ++sequenceId;
                    sequenceName = string.Format(sequenceNameFormat, sequenceId.ToString("000"));
                    i = -1; // restart search
                }
            }

            FSequence sequence = FSequence.CreateSequence();
            sequence.name = sequenceName;
            sequence.FrameRate = FUtility.FrameRate;
            sequence.Length = sequence.FrameRate * FSequence.DEFAULT_LENGTH;

            Undo.RegisterCreatedObjectUndo(sequence.gameObject, "Create Sequence");

            return sequence;
        }

        [MenuItem(MENU_PATH + PRODUCT_NAME + "/Website", false, 200)]
        public static void OpenWebsite()
        {
            Application.OpenURL("http://www.fluxeditor.com");
        }

        [MenuItem(MENU_PATH + PRODUCT_NAME + "/Contact Support", false, 201)]
        public static void ContactSupport()
        {
            Application.OpenURL("mailto:support@fluxeditor.com");
        }
        #endregion

        public static FSequenceEditorWindow instance = null;

        // size of the whole window, cached to determine if it was resized
        private Rect _windowRect;

        // rect used for the header at the top of the window
        private Rect _windowHeaderRect;

        // header
        private FSequenceWindowHeader _windowHeader;

        // toolbar
        private FSequenceWindowToolbar _toolbar;

        // area for the toolbar
        private Rect _toolbarRect;

        [SerializeField]
        private FSequenceEditor _sequenceEditor;

        void OnEnable()
        {
            instance = this;
            wantsMouseMove = true;

            minSize = new Vector2(450, 300);

            _windowHeader = new FSequenceWindowHeader(this);

            _toolbar = new FSequenceWindowToolbar(this);

            _windowRect = new Rect();

            FUtility.LoadPreferences();
        }

        void OnSelectionChange()
        {
            if (!FUtility.OpenSequenceOnSelect)
                return;

            FSequence sequence = Selection.activeGameObject == null || PrefabUtility.GetPrefabType(Selection.activeGameObject) == PrefabType.Prefab ? null : Selection.activeGameObject.GetComponent<FSequence>();

            if (sequence != null)
            {
                Open(sequence);
            }
        }

        public FSequenceEditor GetSequenceEditor()
        {
            return _sequenceEditor;
        }

        void OnDestroy()
        {
            if (_sequenceEditor != null)
            {
                _sequenceEditor.Stop();
                DestroyImmediate(_sequenceEditor);
            }
        }

        void OnLostFocus()
        {
        }

        #region Editor state changes hookups
        private void OnDidOpenScene()
        {
            if (_sequenceEditor)
                _sequenceEditor.OpenSequence(null);
        }

        #endregion


        public bool IsPlaying { get { return _sequenceEditor.IsPlaying; } }

        void Update()
        {
            if (Application.isPlaying)
            {
                return;
            }

#if FLUX_PROFILE
			Profiler.BeginSample("flux Update");
#endif
                if (_sequenceEditor == null)
            {
                _sequenceEditor = FSequenceEditor.CreateInstance<FSequenceEditor>();
                _sequenceEditor.Init(this);
            }

            FSequence sequence = _sequenceEditor.Sequence;

            if (Application.isPlaying && sequence != null && FUtility.RenderOnEditorPlay)
            {
                Repaint();
            }

            _sequenceEditor.Update();

#if FLUX_PROFILE
			Profiler.EndSample();
#endif
        }
        static MethodInfo clearMethod = null;
        /// <summary>
        /// 清空log信息
        /// </summary>
        private static void ClearConsole()
        {
            if (clearMethod == null)
            {
                Type log = typeof(EditorWindow).Assembly.GetType("UnityEditor.LogEntries");
                clearMethod = log.GetMethod("Clear");
            }
            clearMethod.Invoke(null, null);
        }

        public void Play(bool restart)
        {
            _sequenceEditor.IsPlayingForward = true;
            _sequenceEditor.Play(restart);

            ClearConsole();
            Debug.Log("yns  play");
            //SetTrackCache();
        }


        public void PlayBackwards(bool restart)
        {
            _sequenceEditor.IsPlayingForward = false;
            _sequenceEditor.Play(restart);
        }

        public void Pause()
        {
            _sequenceEditor.Pause();
        }

        public void Stop()
        {
            _sequenceEditor.Stop();

            var test = GameObject.FindObjectOfType<Test_AnimPose>();
            if (test != null)
            {
                test.ResetPlayer();
            }
            //ClearAllUndo();
            //ReTurnTrackCache();
        }

        private void RebuildLayout()
        {
            _windowRect = position;
            _windowRect.x = 0;
            _windowRect.y = 0;

            _windowHeaderRect = _windowRect;
            _windowHeaderRect.height = FSequenceWindowHeader.HEIGHT_HEADER;

            _windowHeader.RebuildLayout(_windowHeaderRect);

            _toolbarRect = _windowRect;
            _toolbarRect.yMin = _windowRect.yMax - FSequenceWindowHeader.HEIGHT_ONE;

            _toolbar.RebuildLayout(_toolbarRect);

            Rect timelineViewRect = _windowRect;
            timelineViewRect.yMin += FSequenceWindowHeader.HEIGHT_HEADER;
            timelineViewRect.yMax -= FSequenceWindowToolbar.HEIGHT;

            _sequenceEditor.RebuildLayout(timelineViewRect);

            Repaint();
        }


        public void Refresh()
        {
            if (_sequenceEditor != null)
            {
                _sequenceEditor.OpenSequence(_sequenceEditor.Sequence);
            }

            Repaint();
        }

        public static void RefreshIfOpen()
        {
            if (instance != null)
                instance.Refresh();
        }

        void OnGUI()
        {
#if FLUX_PROFILE
			Profiler.BeginSample("Flux OnGUI");
#endif
            if (_sequenceEditor == null)
                return;

            Rect currentWindowRect = position;
            currentWindowRect.x = 0;
            currentWindowRect.y = 0;

            if (currentWindowRect != _windowRect)
            {
                RebuildLayout();
            }

            if (!FUtility.RenderOnEditorPlay && EditorApplication.isPlaying && !EditorApplication.isPaused)
            {
                GUI.Label(_windowRect, "Draw in play mode is disabled. You can change it on Flux Preferences");
                return;
            }

            FSequence sequence = _sequenceEditor.Sequence;

            if (sequence == null)
                ShowNotification(new GUIContent("Select Or Create Sequence"));
            else if (Event.current.isKey)
            {
                if (Event.current.keyCode == KeyCode.Space)
                {
                    if (Event.current.type == EventType.KeyUp)
                    {
                        if (_sequenceEditor.IsPlaying)
                        {
                            if (Event.current.shift)
                                Stop();
                            else
                                Pause();
                        }
                        else
                            Play(Event.current.shift);


                        Repaint();
                    }
                    Event.current.Use();
                }

                if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
                {
                    EditorGUIUtility.keyboardControl = 0;
                    Event.current.Use();
                    Repaint();
                }
            }

            // header
            _windowHeader.OnGUI();

            if (sequence == null)
                return;

            // toolbar
            _toolbar.OnGUI();

            switch (Event.current.type)
            {
                case EventType.KeyDown:

                    if (Event.current.keyCode == KeyCode.Backspace || Event.current.keyCode == KeyCode.Delete)
                    {
                        _sequenceEditor.DestroyEvents(_sequenceEditor.EventSelection.Editors);
                        Event.current.Use();
                    }
                    else if (Event.current.keyCode == KeyCode.K && _sequenceEditor.Sequence.CurrentFrame >= 0)
                    {
                        _sequenceEditor.AddEvent(_sequenceEditor.Sequence.CurrentFrame);
                        Event.current.Use();
                    }
                    //				else if( Event.current.keyCode == KeyCode.C && _sequenceEditor.GetSequence().GetCurrentFrame() >= 0 )
                    //				{
                    //					_sequenceEditor.AddCommentEvent( _sequenceEditor.GetSequence().GetCurrentFrame() );
                    //					Event.current.Use();
                    //				}
                    break;

                case EventType.MouseDown:
                    break;

                case EventType.MouseUp:
                    break;
            }

            if (Event.current.type == EventType.ValidateCommand)
            {
                Repaint();
            }

            _sequenceEditor.OnGUI();


            // because of a bug with windows editor, we have to not catch right button
            // otherwise ContextClick doesn't get called
            if (Event.current.type == EventType.MouseUp && Event.current.button != 1)
            {
                Event.current.Use();
            }

            if (Event.current.type == EventType.Ignore)
            {
                EditorGUIUtility.hotControl = 0;
            }

            //			// handle drag & drop
            //			if( Event.current.type == EventType.DragUpdated )
            //			{
            //				if( _windowRect.Contains( Event.current.mousePosition ) )
            //				{
            //					DragAndDrop.visualMode = DragAndDropVisualMode.Link;
            //					Event.current.Use();
            //				}
            //			}
            //			else if( Event.current.type == EventType.DragPerform )
            //			{
            //				if( _windowRect.Contains( Event.current.mousePosition ) )
            //				{
            //					foreach( UnityEngine.Object obj in DragAndDrop.objectReferences )
            //					{
            //						if( !(obj is GameObject) )
            //						{
            //							continue;
            //						}
            //
            //						PrefabType prefabType = PrefabUtility.GetPrefabType(obj);
            //						if( prefabType == PrefabType.ModelPrefab || prefabType == PrefabType.Prefab )
            //							continue;
            //
            //						Undo.IncrementCurrentGroup();
            //						UnityEngine.Object[] objsToSave = new UnityEngine.Object[]{ sequence, this };
            //						
            //						Undo.RegisterCompleteObjectUndo( objsToSave, string.Empty );
            //						
            //						GameObject timelineGO = new GameObject(obj.name);
            //						// TODO
            ////						FTimeline timeline = timelineGO.AddComponent<Flux.FTimeline>();
            ////						timeline.SetOwner( ((GameObject)obj).transform );
            ////						sequence.Add( timeline );
            //						
            ////						Undo.RegisterCompleteObjectUndo( objsToSave, string.Empty );
            ////						Undo.RegisterCreatedObjectUndo( timeline.gameObject, "create Timeline" );
            ////						Undo.CollapseUndoOperations( Undo.GetCurrentGroup() );
            //					}
            //					RemoveNotification();
            //					Event.current.Use();
            //					DragAndDrop.AcceptDrag();
            //					Refresh();
            //					EditorGUIUtility.ExitGUI();
            //				}
            //			}

            if (Event.current.type == EventType.Repaint)
            {
                Handles.DrawLine(new Vector3(_windowHeaderRect.xMin, _windowHeaderRect.yMax, 0), new Vector3(_windowHeaderRect.xMax - FSequenceEditor.RIGHT_BORDER, _windowHeaderRect.yMax, 0));
            }
#if FLUX_PROFILE
			Profiler.EndSample();
#endif
        }

        public float GetXForFrame(int frame)
        {
            return _sequenceEditor.GetXForFrame(frame);
        }

        public int GetFrameForX(float x)
        {
            return _sequenceEditor.GetFrameForX(x);
        }

        private string FormatTime(float time)
        {
            int timeInInt = (int)time;
            int minutes = timeInInt / 60;
            int seconds = timeInInt % 60;
            int deciSeconds = (int)((time - timeInInt) * 100f);

            return minutes > 0 ? string.Format("{0}:{1}.{2}",
                minutes.ToString("00"),
                seconds.ToString("00"),
                deciSeconds.ToString("00")) : string.Format("{0}.{1}", seconds.ToString("00"), deciSeconds.ToString("00"));
        }

        float timelineViewRectHeight;

        private void RenderHeader(Rect rect)
        {
            FSequence sequence = _sequenceEditor.Sequence;

            GUI.Label(rect, sequence.name);

            Rect r = rect;

            r.xMin = r.xMax - 100;

            EditorGUI.IntField(r, sequence.Length);

            GUIContent lengthLabel = new GUIContent("Length");
            r.x -= EditorStyles.label.CalcSize(lengthLabel).x + 5;

            EditorGUI.PrefixLabel(r, lengthLabel);

            r.x -= 50;
            r.width = 40;

            EditorGUI.IntField(r, sequence.FrameRate);

            GUIContent framerateLabel = new GUIContent("Frame Rate");

            r.x -= EditorStyles.label.CalcSize(framerateLabel).x + 5;
            EditorGUI.PrefixLabel(r, framerateLabel);

            r.x -= 110;
            r.width = 100;

            EditorGUI.EnumPopup(r, sequence.UpdateMode);

            GUIContent updateModeLabel = new GUIContent("UpdateMode");

            Vector2 updateModeLabelSize = EditorStyles.label.CalcSize(updateModeLabel);

            r.x -= updateModeLabelSize.x + 5;
            r.width = updateModeLabelSize.x;

            EditorGUI.PrefixLabel(r, updateModeLabel);
        }
    }
}
