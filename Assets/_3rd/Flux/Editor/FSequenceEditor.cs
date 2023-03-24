
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

using Flux;
using System;
using Object = UnityEngine.Object;

namespace FluxEditor
{
    [System.Serializable]
    public class FSequenceEditor : FEditorList<FContainerEditor>
    {
        private const double NOTIFY_DIRTY_TRACKS_DELTA = 1;

        private const int VERTICAL_SCROLLER_WIDTH = 16;

        public const int RIGHT_BORDER = 20;

        private const int FRAME_RANGE_SCROLLER_HEIGHT = 16;

        private const int MINIMUM_HEADER_WIDTH = 150;

        private const int SCROLL_WHEEL_SPEED = 5;


        // keep track if we came out of play mode
        private bool _wasUnityInPlaymode = false;

        [SerializeField]
        private Editor _renderingOnEditor;
        [SerializeField]
        private EditorWindow _renderingOnEditorWindow;

        private int _headerResizerGuiId = 0;

        // current view range of the sequence
        [SerializeField]
        private FrameRange _viewRange;
        public FrameRange ViewRange { get { return _viewRange; } }

        // cached sequence.Length, to know when it got chanced
        // and adjust the _viewRange accordingly
        private int _sequenceLength = int.MaxValue;

        // how many pixels do we have to render per frame
        private float _pixelsPerFrame;
        public float PixelsPerFrame { get { return _pixelsPerFrame; } }

        // editor cache
        [SerializeField]
        private FEditorCache _editorCache = null;

        // sequence being edited
        public FSequence Sequence
        {
            get
            {
                return (FSequence)Obj;
            }
        }

        [SerializeField]
        private FTrackEditorInspector _trackSelection = new FTrackEditorInspector();
        public FTrackEditorInspector TrackSelection { get { return _trackSelection; } }

        [SerializeField]
        private FEventEditorInspector _eventSelection = new FEventEditorInspector();
        public FEventEditorInspector EventSelection { get { return _eventSelection; } }

        [SerializeField]
        private FTimelineEditorInspector _timelineSelection = new FTimelineEditorInspector();
        public FTimelineEditorInspector TimelineSelection { get { return _timelineSelection; } }

        [SerializeField]
        private FContainerEditorInspector _containerSelection = new FContainerEditorInspector();
        public FContainerEditorInspector ContainerSelection { get { return _containerSelection; } }

        public override void Init(FObject obj, FEditor owner)
        {
            base.Init(obj, owner);

            Editors.Clear();
            if (Sequence != null)
            {
                for (int i = 0; i != Sequence.Containers.Count; ++i)
                {
                    FContainerEditor editor = _editorCache.GetEditor<FContainerEditor>(Sequence.Containers[i]);
                    editor.Init(Sequence.Containers[i], this);
                    Editors.Add(editor);
                }
            }

            _contentOffset.x = 0; // no left margin, and Y offset needs to be calculated based on scroll
        }

        public override FSequenceEditor SequenceEditor { get { return this; } }

        #region Cached rects to speed up render and not use GUILayout
        // rect for the scroll bar on the left
        [SerializeField]
        private Rect _verticalScrollerRect;

        // rect where the timelines will be displayed
        [SerializeField]
        private Rect _viewRect;

        // rect for the view range bar
        [SerializeField]
        private Rect _viewRangeRect;

        // rect for the time scrubber
        [SerializeField]
        private Rect _timeScrubberRect;

        // origin-starting rect for the sequence timelines, to be used
        // inside the GUI.BeginGroup
        [SerializeField]
        private Rect _sequenceRect;

        [SerializeField]
        private Rect _timelineRect;

        // rect of the area where the timeline headers are
        [SerializeField]
        private Rect _timelineHeaderResizerRect;

        // width of the timeline header
        [SerializeField]
        private float _headerWidth;
        public float HeaderWidth { get { return _headerWidth; } }

        // timeline scroll bar pos, only handles Y
        [SerializeField]
        private Vector2 _scrollPos;

        public FEditor EditorDragged { get; set; }
        public Rect EditorDraggedRect { get; set; }

        #endregion

        [SerializeField]
        private UnityEvent _onUpdateEvent = new UnityEvent();
        public UnityEvent OnUpdateEvent { get { return _onUpdateEvent; } }

        private bool _isEditorCompiling = false;

        protected override void OnEnable()
        {
            base.OnEnable();
            EditorApplication.hierarchyWindowChanged += Refresh;
            AnimationUtility.onCurveWasModified += OnAnimationCurveChanged;

            Undo.undoRedoPerformed += OnUndo;
            Undo.postprocessModifications += PostProcessModifications;

            EditorApplication.playmodeStateChanged += OnPlaymodeChanged;

            EditorSceneManager.sceneUnloaded += OnSceneUnloaded;
        }
        private double _timeNotifyDirtyTracks = 0;
        void OnAnimationCurveChanged(AnimationClip clip, EditorCurveBinding binding, AnimationUtility.CurveModifiedType curveModifiedType)
        {
            //			Debug.LogWarning( "OnAnimationCurveChanged" );
            SetAllDirty();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventSelection.Clear();
            TrackSelection.Clear();
            TimelineSelection.Clear();
            ContainerSelection.Clear();

            if (_editorCache != null)
            {
                _editorCache.Clear();
                DestroyImmediate(_editorCache);
            }

            EditorApplication.hierarchyWindowChanged -= Refresh;

            AnimationUtility.onCurveWasModified -= OnAnimationCurveChanged;

            Undo.undoRedoPerformed -= OnUndo;
            Undo.postprocessModifications -= PostProcessModifications;

            EditorApplication.playmodeStateChanged -= OnPlaymodeChanged;

            EditorSceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        private void OnSceneUnloaded(Scene scene)
        {
            if (Sequence != null && Sequence.gameObject.scene == scene)
            {
                OpenSequence(null);
            }
        }

        protected override bool CanPaste(FObject obj)
        {
            return obj is FContainer;
        }

        protected override void Paste(object obj)
        {
            Undo.RecordObject(Sequence, string.Empty);

            FContainer container = Instantiate<FContainer>((FContainer)obj);
            container.hideFlags = Sequence.Content.hideFlags;
            Sequence.Add(container);
            Undo.RegisterCreatedObjectUndo(container.gameObject, "Paste Container " + ((FContainer)obj).name);
        }

        public void Init(Editor editor)
        {
            Init(editor, null);
        }

        public void Init(EditorWindow editorWindow)
        {
            Init(null, editorWindow);
        }

        private void Init(Editor editor, EditorWindow editorWindow)
        {
            _renderingOnEditor = editor;
            _renderingOnEditorWindow = editorWindow;

            _editorCache = CreateInstance<FEditorCache>();
        }

        public void Repaint()
        {
            if (_renderingOnEditor)
                _renderingOnEditor.Repaint();
            else if (_renderingOnEditorWindow)
                _renderingOnEditorWindow.Repaint();
        }

        public void Refresh()
        {
            OpenSequence(Sequence);
        }

        public T GetEditor<T>(FObject obj) where T : FEditor
        {
            return _editorCache.GetEditor<T>(obj);
        }

        public void OnUndo()
        {
            Debug.Log("yns undo");
            return;
            _editorCache.Refresh();

            if (FInspectorWindow._instance)
            {
                EventSelection.Refresh();
                TrackSelection.Refresh();
                TimelineSelection.Refresh();
                ContainerSelection.Refresh();
                FInspectorWindow._instance.Repaint();
            }

            OpenSequence(Sequence);

            SetAllDirty();

            if (Sequence && !Sequence.IsStopped)
            {
                int currentFrame = Sequence.CurrentFrame;
                Stop();
                SetCurrentFrame(currentFrame);
            }
        }

        UndoPropertyModification[] PostProcessModifications(UndoPropertyModification[] modifications)
        {
            foreach (UndoPropertyModification modification in modifications)
            {
                FTrack track = null;

                PropertyModification propertyModification = modification.currentValue;

                if (propertyModification.target is FEvent)

                    if (propertyModification.target is FEvent)
                    {
                        track = ((FEvent)propertyModification.target).Track;
                    }
                    else if (propertyModification.target is FTrack)
                    {
                        track = (FTrack)propertyModification.target;
                    }

                if (track != null && track.Sequence == Sequence)
                {
                    FTrackEditor trackEditor = GetEditor<FTrackEditor>(track);
                    SetDirty(trackEditor);
                }
            }

            return modifications;
        }

        public void OnPlaymodeChanged()
        {
            if (_wasUnityInPlaymode)
            {
                // make sure it is like it is opening it for the first time
                _wasUnityInPlaymode = false;
                FSequence sequence = Sequence;
                OpenSequence(null);
                OpenSequence(sequence);
            }
        }
        //清除cache
        public void OnPlaymodeWillChange()
        {
            Stop();
        }

        private void OnWillChange()
        {
            Debug.Log($"OnWillChange !!");
            foreach (FContainerEditor container in Editors)
            {
                List<FTimelineEditor> timelineEditors = container.Editors;

              for (int i = 0; i != timelineEditors.Count; ++i)
                {
                    for (int j = 0; j != timelineEditors[i].Editors.Count; ++j)
                    {
                        if (timelineEditors[i].Editors[j].Track.HasCache)
                        {
                            timelineEditors[i].Editors[j].Track.ClearCache();
                        }
                    }
                }
            }

            FAnimationTrack.DeleteAnimationPreviews(Sequence);

            _editorCache.Refresh();
            EventSelection.Refresh();
            TrackSelection.Refresh();
            TimelineSelection.Refresh();
            ContainerSelection.Refresh();

            Repaint();
        }

        public void SetViewRange(FrameRange viewRange)
        {
            FrameRange sequenceRange = new FrameRange(0, Sequence.Length);
            viewRange.Start = sequenceRange.Cull(viewRange.Start);
            viewRange.End = sequenceRange.Cull(viewRange.End);

            if (viewRange.End <= viewRange.Start)
            {
                if (viewRange.Start == Sequence.Length)
                {
                    viewRange.End = Sequence.Length;
                    viewRange.Start = viewRange.End - 1;
                }
                else
                {
                    viewRange.End = viewRange.Start + 1;
                }
            }

            _viewRange = viewRange;
        }

        /**
		 * @brief Returns the mouse x normalized to the start of the timeline, so 0 = viewRange.Start
		 * @param frame Frame.
		 */
        public float GetXForFrame(int frame)
        {
            return ((frame - _viewRange.Start) * _pixelsPerFrame);
        }

        /**
		 * @brief Get frame for x, normalized to the start of timeline, so 0 = viewRange.Start
		 * @param x Position.
		 */
        public int GetFrameForX(float x)
        {
            return _viewRange.Start + Mathf.RoundToInt(x / _pixelsPerFrame);
        }

        protected override string HeaderText { get { return null; } }

        protected override Transform ContentTransform { get { return Sequence.Content; } }

        public override void Render(Rect rect, float headerWidth)
        {
            RenderList(rect, headerWidth);
        }

        public void RebuildLayout(Rect rect)
        {
            Rect = rect;

            _viewRect = rect;
            _viewRect.xMin += VERTICAL_SCROLLER_WIDTH;
            _viewRect.yMax -= (FRAME_RANGE_SCROLLER_HEIGHT + FGUI.TIMELINE_SCRUBBER_HEIGHT);

            _sequenceRect = _viewRect;
            _sequenceRect.xMin -= VERTICAL_SCROLLER_WIDTH;
            _sequenceRect.xMax -= VERTICAL_SCROLLER_WIDTH + RIGHT_BORDER;

            _timelineRect = _sequenceRect;
            _timelineRect.xMin += _headerWidth;

            _headerWidth = Mathf.Max(MINIMUM_HEADER_WIDTH, _headerWidth);

            _timeScrubberRect = _viewRect;
            _timeScrubberRect.xMin += _headerWidth;
            _timeScrubberRect.xMax -= RIGHT_BORDER;
            _timeScrubberRect.yMax += FGUI.TIMELINE_SCRUBBER_HEIGHT;

            _timelineHeaderResizerRect = _timeScrubberRect;
            _timelineHeaderResizerRect.xMin = 0;
            _timelineHeaderResizerRect.xMax = _timeScrubberRect.xMin;
            _timelineHeaderResizerRect.yMin = _timelineHeaderResizerRect.yMax - FGUI.TIMELINE_SCRUBBER_HEIGHT;

            _viewRangeRect = rect;
            _viewRangeRect.yMin = _viewRangeRect.yMax - FRAME_RANGE_SCROLLER_HEIGHT;

            _verticalScrollerRect = rect;
            _verticalScrollerRect.width = VERTICAL_SCROLLER_WIDTH;
            _verticalScrollerRect.yMax -= (FRAME_RANGE_SCROLLER_HEIGHT + FGUI.TIMELINE_SCRUBBER_HEIGHT);
        }

        public void OpenSequence(FSequence sequence)
        {
#if FLUX_DEBUG
			Debug.Log( "Opening sequence: " + sequence + " isPlaying " + EditorApplication.isPlaying + " isPlayingOrEtc " + EditorApplication.isPlayingOrWillChangePlaymode );
#endif
            if (sequence == null)
            {
                if (!object.Equals(sequence, null))
                {
                    sequence = (FSequence)EditorUtility.InstanceIDToObject(sequence.GetInstanceID());
                    Debug.Log($"yns  nul obj");
                }
            }

            bool sequenceChanged = Sequence != sequence && (object.Equals(Sequence, null) || object.Equals(sequence, null) || Sequence.GetInstanceID() != sequence.GetInstanceID());

            if (sequenceChanged || sequence == null)
            {
                if (Sequence != null)
                {
                    //bool IsStopped = Sequence.IsStopped;
                    Stop();
                }

                OnUpdateEvent.RemoveAllListeners();

                _editorCache.Clear();
                EventSelection.Clear();
                TrackSelection.Clear();
                TimelineSelection.Clear();
                ContainerSelection.Clear();
                Editors.Clear();

                //if (sequence != null)
                //    sequence.Speed = sequence.DefaultSpeed;
            }
            else if (!EditorApplication.isPlaying)
            {

                _editorCache.Refresh();
                EventSelection.Refresh();
                TrackSelection.Refresh();
                TimelineSelection.Refresh();
                ContainerSelection.Refresh();
            }


            if (sequence != null)
            {
                if (!EditorApplication.isPlaying && sequence.Version != Flux.FUtility.FLUX_VERSION)
                {
                    Debug.Log($"yns Upgrade {sequence.Version}");
                    FluxEditor.FUtility.Upgrade(sequence);
                }

                if (_viewRange.Length == 0)
                    _viewRange = new FrameRange(0, sequence.Length);

                if (!EditorApplication.isPlaying)
                {
                    sequence.Rebuild();
                }


                if (_viewRange.Length == 0)
                {
                    _viewRange = new FrameRange(0, sequence.Length);
                }
            }
            else
            {
                _dirtyTracks.Clear();
            }

            Init(sequence, null);

            if (!Application.isPlaying && sequenceChanged && sequence != null)
            {
                sequence.Init();
                FAnimationTrack animationTrack = null;
                sequence.GetAnimTrack(ref animationTrack);
                if (animationTrack != null)
                {
                    FAnimationTrackInspector.RebuildStateMachine((FAnimationTrack)animationTrack);
                    Debug.Log($"yns RebuildStateMachine Anim {animationTrack.Sequence.name} {sequence.name}");
                }
                //Debug.Log($"Init");
            }

            Repaint();

            if (FInspectorWindow._instance != null)
                FInspectorWindow._instance.Repaint();
        }

        public void ClearDirty()
        {
            _editorCache.Clear();
            EventSelection.Clear();
            TrackSelection.Clear();
            TimelineSelection.Clear();
            ContainerSelection.Clear();
            Editors.Clear();
            _dirtyTracks.Clear();
        }

        public void CreateContainer(FColorSetting containerInfo)
        {
            Undo.RecordObject(Sequence, null);

            FContainer container = FContainer.Create(containerInfo._color);
            container.gameObject.name = GetUniqueContainerName(containerInfo._str);
            Sequence.Add(container);
            Undo.RegisterCreatedObjectUndo(container.gameObject, "create Container");
        }

        private bool HasContainerWithName(string name)
        {
            foreach (FContainer container in Sequence.Containers)
            {
                if (container.gameObject.name == name)
                    return true;
            }

            return false;
        }

        public string GetUniqueContainerName(string defaultName)
        {
            if (!HasContainerWithName(defaultName))
                return defaultName;

            string nameFormat = defaultName + " {0}";
            int id = 1;
            while (HasContainerWithName(string.Format(nameFormat, id)))
            {
                ++id;
            }

            return string.Format(nameFormat, id);
        }

        public void DestroyEditor(FEditor editor)
        {
            if (editor is FEventEditor)
            {
                List<FEventEditor> eventEditors = new List<FEventEditor>();
                eventEditors.Add((FEventEditor)editor);
                DestroyEvents(eventEditors);
            }
            else if (editor is FTrackEditor)
            {
                DestroyEditor((FTrackEditor)editor);
            }
            else if (editor is FTimelineEditor)
            {
                DestroyEditor((FTimelineEditor)editor);
            }
            else if (editor is FContainerEditor)
            {
                DestroyEditor((FContainerEditor)editor);
            }
        }

        public void DestroyEditor(FContainerEditor editor)
        {
            FContainer container = editor.Container;

            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("delete Container");

            Undo.RecordObjects(new Object[] { Sequence, container, editor, _editorCache, this }, "delete Container");

            editor.OnDelete();

            editor.SequenceEditor.Editors.Remove(editor);

            _editorCache.Remove(editor);
            ContainerSelection.Remove(editor);

            Undo.SetTransformParent(container.transform, null, null);
            Sequence.Remove(container);

            while (editor.Editors.Count > 0)
            {
                DestroyEditor(editor.Editors[editor.Editors.Count - 1]);
            }

            Undo.DestroyObjectImmediate(editor);
            Undo.DestroyObjectImmediate(container.gameObject);

            Undo.CollapseUndoOperations(undoGroup);
        }

        public void DestroyEditor(FTrackEditor editor)
        {
            FTrack track = editor.Track;

            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("delete Track");

            Undo.RecordObjects(new UnityEngine.Object[] { track.Timeline, editor.TimelineEditor, track, editor, _editorCache, this }, "delete Track");

            editor.OnDelete();

            editor.TimelineEditor.Editors.Remove(editor);

            _editorCache.Remove(editor);
            TrackSelection.Remove(editor);
            Undo.SetTransformParent(track.transform, null, null);

            track.Timeline.Remove(track);

            DestroyEvents(editor._eventEditors);

            Undo.DestroyObjectImmediate(editor);
            Undo.DestroyObjectImmediate(track.gameObject);

            Undo.CollapseUndoOperations(undoGroup);
        }

        public void DestroyEditor(FTimelineEditor editor)
        {
            FTimeline timeline = editor.Timeline;

            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("delete Timeline");

            Undo.RecordObjects(new UnityEngine.Object[] { timeline.Container, editor.ContainerEditor, timeline, editor, _editorCache, this }, "delete Timeline");

            editor.OnDelete();

            while (editor.Editors.Count > 0)
            {
                DestroyEditor(editor.Editors[editor.Editors.Count - 1]);
            }

            editor.ContainerEditor.Editors.Remove(editor);
            _editorCache.Remove(editor);
            TimelineSelection.Remove(editor);
            Undo.SetTransformParent(timeline.transform, null, null);

            timeline.Container.Remove(timeline);

            Undo.DestroyObjectImmediate(editor);
            Undo.DestroyObjectImmediate(timeline.gameObject);

            Undo.CollapseUndoOperations(undoGroup);
        }

        protected override void Delete()
        {
            // cannot be deleted this way since it has no header to right click on
        }


        public void AddEvent(int t)
        {
            List<ISelectableElement> newEvents = new List<ISelectableElement>();

            int undoGroup = Undo.GetCurrentGroup();

            foreach (FTrackEditor trackEditor in TrackSelection.Editors)
            {
                FEvent evt = trackEditor.TryAddEvent(t);
                if (evt)
                {
                    FEventEditor evtEditor = GetEditor<FEventEditor>(evt);
                    evtEditor.Init(evt, trackEditor);
                    newEvents.Add(evtEditor);
                }
            }

            if (newEvents.Count > 0)
            {
                int currentFrame = Sequence.CurrentFrame;

                DeselectAll();
                Select(newEvents);

                if (currentFrame >= 0)
                {
                    foreach (FTrackEditor trackEditor in TrackSelection.Editors)
                    {
                        trackEditor.Track.Init();
                    }
                }
            }
            else
                EditorApplication.Beep();

            Undo.CollapseUndoOperations(undoGroup);
        }

        public void CallEvent(KeyCode key, FSequenceEditor _sequenceEditor)
        {
            //foreach (var con in _sequenceEditor.Sequence.Containers)
            //{
            //    foreach (var timeline in con.Timelines)
            //    {
            //        if(timeline.Tracks[0].)
            //    }
            //} 
        }

        public void DestroyEvents(List<FEventEditor> events)
        {
            if (events.Count == 0)
                return;

            List<FTrackEditor> trackEditors = new List<FTrackEditor>();
            List<Object> objs = new List<Object>();

            for (int i = 0; i != events.Count; ++i)
            {
                objs.Add(events[i]);
                objs.Add(events[i].Evt);

                if (!trackEditors.Contains(events[i].TrackEditor))
                    trackEditors.Add(events[i].TrackEditor);
            }

            for (int i = 0; i != trackEditors.Count; ++i)
            {
                objs.Add(trackEditors[i]);
                objs.Add(trackEditors[i].Track);
            }
            //			objs.Add( _editorCache );
            objs.Add(this); // add itself, to save selection lists

            Undo.SetCurrentGroupName("delete Events");
            Undo.RecordObjects(objs.ToArray(), "delete Events");

            for (int j = events.Count - 1; j > -1; --j)
            {
                FEventEditor evtEditor = events[j];
                evtEditor.OnDelete();

                _editorCache.Remove(evtEditor);
                EventSelection.Remove(evtEditor);

                evtEditor.TrackEditor._eventEditors.Remove(evtEditor);
                Undo.SetTransformParent(evtEditor.Evt.transform, null, string.Empty);
                evtEditor.Evt.Track.Remove(evtEditor.Evt);

                GameObject evtGO = evtEditor.Evt.gameObject;
                Undo.DestroyObjectImmediate(evtEditor);
                Undo.DestroyObjectImmediate(evtGO);
            }
        }

        private class MouseHoverData
        {
            public int Frame = -1;
            public FEditor Editor = null;

            public void Copy(MouseHoverData data)
            {
                Frame = data.Frame;
                Editor = data.Editor;
            }

            public void Clear() { Frame = -1; Editor = null; }
        }

        public void SetMouseHover(float mouseX, FEditor editor)
        {
            if (editor == null)
                _mouseHoverData.Clear();
            else
            {
                _mouseHoverData.Frame = GetFrameForX(mouseX);
                _mouseHoverData.Editor = editor;
            }
        }

        public override void ReserveGuiIds()
        {
            _headerResizerGuiId = EditorGUIUtility.GetControlID(FocusType.Passive);
            base.ReserveGuiIds();
        }

        public void OnGUI()
        {
            if (Application.isPlaying)
            {
                GUI.color = new Color(1, 1, 1, 0.5f);
                GUI.enabled = false;
            }

            if (!Sequence.HasTimelines())
            {
                if (_renderingOnEditorWindow)
                    _renderingOnEditorWindow.ShowNotification(new GUIContent("Drag GameObjects Here"));
            }

            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.F && EditorGUIUtility.keyboardControl == 0)
            {
                FocusOnSelection();
                Event.current.Use();
            }

            _pixelsPerFrame = (_sequenceRect.width - _headerWidth) / _viewRange.Length;

            ReserveGuiIds();

            _scrollPos.y = GUI.VerticalScrollbar(_verticalScrollerRect, _scrollPos.y, Mathf.Min(_sequenceRect.height, Height), 0, Height);

            // content is offset based on the scrool position, since we just care about vertical forget the rest
            _contentOffset.y = -_scrollPos.y;

            Rect scrolledViewRect = _viewRect;

            GUI.BeginGroup(scrolledViewRect);

            Rect timelineRect = _sequenceRect;
            timelineRect.y = -_scrollPos.y;
            timelineRect.height = 0;

            Handles.color = FGUI.GetLineColor();

            Rect normalizedScrollViewRect = _sequenceRect;
            normalizedScrollViewRect.x = 0;
            normalizedScrollViewRect.yMin = -_scrollPos.y;

            SetMouseHover(-1, null);

            Render(normalizedScrollViewRect, _headerWidth);

            if (Event.current.type == EventType.MouseDown && Event.current.button != 1 && EditorGUIUtility.hotControl == 0)
                StartDragSelect();
            if (_isDragSelecting)
            {
                OnDragSelect();
            }

            if (Event.current.type == EventType.MouseUp || Event.current.type == EventType.Ignore)
            {
                if (EditorDragged != null)
                {
                    StopDraggingEditor();
                }
                if (_isDragSelecting)
                {
                    StopDragSelect();
                }
            }

            if (EditorDragged != null)
            {
                float indent = 0;
                FEditor owner = EditorDragged.Owner;
                while (owner != null)
                {
                    indent += owner.ContentOffset.x;
                    owner = owner.Owner;
                }
                Rect editorDraggedRect = EditorDraggedRect;
                editorDraggedRect.x += indent;
                editorDraggedRect.y += Event.current.mousePosition.y;
                //				EditorGUI.DrawRect( editorDraggedRect, FGUI.GetWindowColor() );
                EditorDragged.Render(editorDraggedRect, _headerWidth - indent);
                Repaint();
                //				EditorGUI.DrawTextureAlpha( new Rect(0, 0, 100, 100), EditorGUIUtility.whiteTexture);

                if (!(EditorDragged is FContainerEditor) && !(EditorDragged is FEventEditor))
                {
                    System.Type typeOfOwner = null;
                    if (EditorDragged is FTrackEditor)
                        typeOfOwner = typeof(FTimelineEditor);
                    else if (EditorDragged is FTimelineEditor)
                        typeOfOwner = typeof(FContainerEditor);

                    FEditor editorBelowMouse = null;
                    foreach (FContainerEditor containerEditor in Editors)
                    {
                        if (typeOfOwner == typeof(FContainerEditor))
                        {
                            if (containerEditor.GetGlobalRect().Contains(Event.current.mousePosition))
                            {
                                editorBelowMouse = containerEditor;
                                break;
                            }

                        }
                        else
                        {
                            foreach (FTimelineEditor timelineEditor in containerEditor.Editors)
                            {
                                if (typeOfOwner == typeof(FTimelineEditor))
                                {
                                    if (timelineEditor.GetGlobalRect().Contains(Event.current.mousePosition))
                                    {
                                        editorBelowMouse = timelineEditor;
                                        break;
                                    }
                                }
                                else
                                {
                                    // handle events
                                }
                            }
                        }
                    }
                    if (editorBelowMouse != null)
                    {
                        Rect r = editorBelowMouse.GetGlobalRect();
                        float lineWidth = 3;
                        float halfLineWidth = lineWidth * 0.33f;
                        Color c = Handles.selectedColor;
                        c.a = 1f;
                        Handles.color = c;
                        Vector3 pt0 = new Vector3(r.xMin + halfLineWidth, r.yMin + halfLineWidth);
                        Vector3 pt1 = new Vector3(r.xMin + halfLineWidth, r.yMax - halfLineWidth);
                        Vector3 pt2 = new Vector3(r.xMax - halfLineWidth, r.yMax - halfLineWidth);
                        Vector3 pt3 = new Vector3(r.xMax - halfLineWidth, r.yMin + halfLineWidth);
                        Handles.DrawSolidRectangleWithOutline(new Vector3[] { pt0, pt1, pt2, pt3 }, new Color(0, 0, 0, 0), c);
                    }
                }
            }

            switch (Event.current.type)
            {
                case EventType.DragUpdated:
                    if (normalizedScrollViewRect.Contains(Event.current.mousePosition))
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                        Event.current.Use();
                    }
                    break;
                case EventType.DragPerform:
                    Debug.Log("yns Seq  dragPerform");
                    if (!normalizedScrollViewRect.Contains(Event.current.mousePosition))
                        break;

                    FContainerEditor containerEditor = null;
                    foreach (FContainerEditor editor in Editors)
                    {
                        if (editor.GetGlobalRect().Contains(Event.current.mousePosition))
                        {
                            containerEditor = editor;
                            break;
                        }
                    }

                    if (containerEditor == null)
                    {
                        if (Editors.Count > 0)
                            containerEditor = Editors[Editors.Count - 1];
                        else
                        {
                            CreateContainer(new FColorSetting("Default", FGUI.DefaultContainerColor()));
                            containerEditor = GetEditor<FContainerEditor>(Sequence.Containers[0]);
                            Debug.Log($"yns  add FContainerEditor");
                        }
                    }

                    foreach (UnityEngine.Object obj in DragAndDrop.objectReferences)
                    {
                        if (!(obj is GameObject))
                        {
                            continue;
                        }

#pragma warning disable CS0618 // 类型或成员已过时
                        PrefabType prefabType = PrefabUtility.GetPrefabType(obj); ;
                        if (prefabType == PrefabType.ModelPrefab || prefabType == PrefabType.Prefab)
                            continue;
#pragma warning restore CS0618 // 类型或成员已过时
                        Undo.IncrementCurrentGroup();
                        UnityEngine.Object[] objsToSave = new UnityEngine.Object[] { Sequence, this };

                        Undo.RecordObjects(objsToSave, string.Empty);

                        FTimeline timeline = FTimeline.Create(((GameObject)obj).transform);

                        containerEditor.Container.Add(timeline);

                        //Debug.Log($"yns  add timeline {((GameObject)obj).transform.parent}");

                        ParticleSystem ps = ((GameObject)obj).GetComponent<ParticleSystem>();
                        if (ps != null)
                        {
                            
                            timeline.Add<FPlayParticleEvent>(new FrameRange(Sequence.CurrentFrame, Sequence.CurrentFrame+10));
                            Debug.Log($"yns Add FPlayParticleEvent");
                        }

                        Undo.RegisterCreatedObjectUndo(timeline.gameObject, "create Timeline");
                        Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
                    }
                    if (_renderingOnEditorWindow != null)
                        _renderingOnEditorWindow.RemoveNotification();
                    Event.current.Use();
                    DragAndDrop.AcceptDrag();
                    Refresh();
                    EditorGUIUtility.ExitGUI();
                    break;
            }

            GUI.EndGroup();

            if (Sequence.Length != _sequenceLength)
            {
                if (Sequence.Length < _sequenceLength)
                {
                    _viewRange.Start = 0;
                    _viewRange.End = Sequence.Length;
                }
                else
                {
                    _viewRange.End = _viewRange.Start + Mathf.RoundToInt(((_viewRange.End - _viewRange.Start) / (float)_sequenceLength) * Sequence.Length);
                }

                _sequenceLength = Sequence.Length;
            }

            GUI.enabled = true;
            GUI.backgroundColor = Color.white;
            int newT = FGUI.TimeScrubber(_timeScrubberRect, Sequence.CurrentFrame, Sequence.FrameRate, _viewRange);

            if (newT != Sequence.CurrentFrame)
            {
                if (_renderingOnEditorWindow)
                    _renderingOnEditorWindow.Focus();

                SetCurrentFrame(newT);

                if (_isPlaying || Sequence.IsPlaying)
                    Pause();
            }

            _viewRange = FGUI.ViewRangeBar(_viewRangeRect, _viewRange, Sequence.Length);

            if (_timelineHeaderResizerRect.Contains(Event.current.mousePosition))
                EditorGUIUtility.AddCursorRect(_timelineHeaderResizerRect, MouseCursor.ResizeHorizontal);

            switch (Event.current.type)
            {
                case EventType.MouseDown:

                    bool leftClick = Event.current.button == 0 && !Event.current.alt;

                    if (leftClick) // left button
                    {
                        if (EditorGUIUtility.hotControl == 0 && _timelineHeaderResizerRect.Contains(Event.current.mousePosition))
                        {
                            EditorGUIUtility.hotControl = _headerResizerGuiId;
                            Event.current.Use();
                        }
                        else if (Rect.Contains(Event.current.mousePosition))
                        {
                            DeselectAll();
                            Event.current.Use();
                        }
                    }
                    break;
                case EventType.MouseDrag:
                    if (EditorGUIUtility.hotControl == _headerResizerGuiId)
                    {
                        _headerWidth = (int)Mathf.Max(MINIMUM_HEADER_WIDTH, _headerWidth + Event.current.delta.x);

                        RebuildLayout(Rect);
                        EditorWindow.GetWindow<FSequenceEditorWindow>().Repaint();
                        Event.current.Use();
                    }

                    break;
                case EventType.MouseUp:
                    if (EditorGUIUtility.hotControl == _headerResizerGuiId)
                    {
                        EditorGUIUtility.hotControl = 0;
                        Event.current.Use();
                    }
                    break;
                case EventType.Repaint:
                    Rect dragArea = _timelineHeaderResizerRect;
                    dragArea.xMax -= 10;
                    dragArea.xMin = dragArea.xMax - 16;
                    GUIStyle dragStyle = FUtility.GetFluxSkin().GetStyle("HorizontalPanelSeparatorHandle");
                    dragStyle.Draw(dragArea, GUIContent.none, 0);
                    //				GUI.DrawTexture( dragArea, EditorGUIUtility.whiteTexture );
                    Handles.color = FGUI.GetLineColor();// new Color(0.8f, 0.8f, 0.8f, 0.2f);
                                                        //				Handles.DrawLine( new Vector3( _viewRect.xMin-16, _sequenceRect.yMax, 0), new Vector3(_viewRect.xMax-RIGHT_BORDER, _sequenceRect.yMax, 0) );
                                                        //				Handles.DrawLine( new Vector3( _sequenceRect.xMin, _sequenceRect.yMin, 0), new Vector3( _sequenceRect.xMax, _sequenceRect.yMin, 0) );
                                                        //				Handles.color = Color.black;
                    break;
                case EventType.ScrollWheel:
                    if (_viewRect.Contains(Event.current.mousePosition))
                    {
                        _scrollPos.y += Event.current.delta.y * SCROLL_WHEEL_SPEED;
                        Event.current.Use();
                    }
                    break;
                case EventType.ValidateCommand:
                    if (EventSelection.Editors.Count == 1)
                        Event.current.Use();
                    else if (EventSelection.Editors.Count == 0)
                    {
                        if (TrackSelection.Editors.Count == 1)
                            Event.current.Use();
                        else if (TrackSelection.Editors.Count == 0)
                        {
                            if (TimelineSelection.Editors.Count == 1)
                                Event.current.Use();
                            else if (TimelineSelection.Editors.Count == 0)
                            {
                                if (ContainerSelection.Editors.Count == 1)
                                    Event.current.Use();
                            }
                        }
                    }
                    break;
                case EventType.ExecuteCommand:
                    if (Event.current.commandName == "Copy")
                    {
                        if (EventSelection.Editors.Count == 1)
                            CopyEditor(EventSelection.Editors[0]);
                        else if (EventSelection.Editors.Count == 0)
                        {
                            if (TrackSelection.Editors.Count == 1)
                                CopyEditor(TrackSelection.Editors[0]);
                            else if (TrackSelection.Editors.Count == 0)
                            {
                                if (TimelineSelection.Editors.Count == 1)
                                    CopyEditor(TimelineSelection.Editors[0]);
                                else if (TimelineSelection.Editors.Count == 0)
                                {
                                    if (ContainerSelection.Editors.Count == 1)
                                        CopyEditor(ContainerSelection.Editors[0]);
                                }
                            }
                        }
                    }
                    break;
            }

#if FLUX_TRIAL
			GUIStyle watermarkLabel = new GUIStyle( GUI.skin.label );
			watermarkLabel.fontSize = 24;
			GUIContent watermark = new GUIContent("..::FLUX TRIAL::..");
			Vector2 watermarkSize = watermarkLabel.CalcSize( watermark );
			Rect watermarkRect = new Rect( Rect.width*0.5f-watermarkSize.x*0.5f, Rect.height*0.5f - watermarkSize.y*0.5f, watermarkSize.x, watermarkSize.y );

			GUI.color = new Color( 1f, 1f, 1f, 0.4f );
			GUI.Label( watermarkRect, watermark, watermarkLabel );
#endif
        }

        #region Drag Editor

        public void StartDraggingEditor(FEditor editorDragged)
        {
            if (_isDragSelecting)
                return; // if we're drag selecting, move on
            EditorDragged = editorDragged;
            EditorGUIUtility.hotControl = EditorDragged.GuiId;
            Event.current.Use();
        }

        public void StopDraggingEditor()
        {
            if (EditorDragged == null) return;

            FEditor editorBelowMouse = null;

            if (EditorDragged is FContainerEditor)
            {
                editorBelowMouse = this;
            }
            else
            {
                foreach (FContainerEditor containerEditor in Editors)
                {
                    if (EditorDragged is FTimelineEditor)
                    {
                        if (containerEditor.GetGlobalRect().Contains(Event.current.mousePosition))
                        {
                            editorBelowMouse = containerEditor;
                            break;
                        }
                    }
                    else
                    {
                        foreach (FTimelineEditor timelineEditor in containerEditor.Editors)
                        {
                            if (EditorDragged is FTrackEditor)
                            {
                                if (timelineEditor.GetGlobalRect().Contains(Event.current.mousePosition))
                                {
                                    editorBelowMouse = timelineEditor;
                                    break;
                                }
                            }
                            else // handle events?
                            {

                            }
                        }
                    }
                }
            }

            if (editorBelowMouse != null)
            {
                if (editorBelowMouse == EditorDragged.Owner)
                {
                    if (EditorDragged.Owner is FSequenceEditor)
                        ((FSequenceEditor)EditorDragged.Owner).UpdateListFromOffsets();
                    if (EditorDragged.Owner is FContainerEditor)
                        ((FContainerEditor)EditorDragged.Owner).UpdateListFromOffsets();
                    if (EditorDragged.Owner is FTimelineEditor)
                        ((FTimelineEditor)EditorDragged.Owner).UpdateListFromOffsets();
                }
                else
                {
                    Undo.SetTransformParent(EditorDragged.Obj.transform, editorBelowMouse.Obj.transform, "move Editor");
                }
            }

            CancelDragEditor();
        }

        public void CancelDragEditor()
        {
            if (EditorDragged == null) return;

            EditorDragged.Owner.ClearOffset();
            EditorDragged = null;
            EditorGUIUtility.hotControl = 0;
            Event.current.Use();
            Repaint();
        }

        #endregion

        private MouseHoverData _mouseDownData = new MouseHoverData();
        private MouseHoverData _mouseHoverData = new MouseHoverData();
        private bool _isDragSelecting = false;

        public void StartDragSelect()
        {
            if (_isDragSelecting)
                return;

            _mouseDownData.Copy(_mouseHoverData);
            if (_mouseDownData.Editor != null && EditorGUIUtility.hotControl == 0)
            {
                _isDragSelecting = true;
                Event.current.Use();
            }
        }

        public void StopDragSelect()
        {
            if (!_isDragSelecting)
                return;
            if (!Event.current.shift && !Event.current.alt)
                DeselectAll();

            bool select = !Event.current.alt;

            Rect selectionRect = GetSelectionRect();

            FrameRange selectRange = new FrameRange(GetFrameForX(selectionRect.xMin - _headerWidth),
                                                    GetFrameForX(selectionRect.xMax - _headerWidth));

            for (int i = 0; i != Editors.Count; ++i)
            {
                Rect containerRect = Editors[i].GetGlobalRect();
                if (selectionRect.yMin > containerRect.yMax)
                    continue;

                if (selectionRect.yMax < containerRect.yMin)
                    break;

                for (int j = 0; j != Editors[i].Editors.Count; ++j)
                {
                    List<FTrackEditor> tracks = Editors[i].Editors[j].Editors;
                    for (int k = 0; k != tracks.Count; ++k)
                    {
                        Rect trackRect = tracks[k].GetGlobalRect();
                        if (selectionRect.yMin >= trackRect.yMax)
                            continue;

                        if (selectionRect.yMax <= trackRect.yMin)
                            break;

                        if (select)
                            tracks[k].SelectEvents(selectRange);
                        else
                            tracks[k].DeselectEvents(selectRange);
                    }
                }
            }

            _isDragSelecting = false;
            _mouseDownData.Clear();

            Event.current.Use();
        }

        public void OnDragSelect()
        {
            if (!_isDragSelecting)
                return;

            if (Event.current.shift)
                EditorGUIUtility.AddCursorRect(Rect, MouseCursor.ArrowPlus);

            if (Event.current.alt)
                EditorGUIUtility.AddCursorRect(Rect, MouseCursor.ArrowMinus);

            Rect selectionRect = GetSelectionRect();
            GUI.color = FGUI.GetSelectionColor();
            GUI.DrawTexture(selectionRect, EditorGUIUtility.whiteTexture);
            Repaint();
        }

        private Rect GetSelectionRect()
        {
            MouseHoverData hoverData = _mouseHoverData;
            int hoverFrame = hoverData.Frame;
            if (hoverData.Editor == null)
            {
                hoverData = _mouseDownData;
                hoverFrame = Event.current.mousePosition.x < _headerWidth ? _viewRange.Start : _viewRange.End;
            }

            float x1 = GetXForFrame(_mouseDownData.Frame) + _headerWidth;
            float x2 = GetXForFrame(hoverFrame) + _headerWidth;

            Rect downRect = _mouseDownData.Editor.GetGlobalRect();
            Rect hoverRect = hoverData.Editor.GetGlobalRect();

            Rect r = new Rect();
            r.xMin = Mathf.Min(x1, x2);
            r.xMax = Mathf.Max(x1, x2);
            r.yMin = Mathf.Min(downRect.yMin, hoverRect.yMin);
            r.yMax = Mathf.Max(downRect.yMax, hoverRect.yMax);

            if (r.width == 0)
            {
                r.xMin -= 0.5f;
                r.xMax += 0.5f;
            }

            return r;
        }

        public void FocusOnSelection()
        {
            int start = 0;
            int end = Sequence.Length;

            if (_eventSelection.Editors.Count > 0)
            {
                start = end;
                end = 0;
                for (int i = 0; i != _eventSelection.Editors.Count; ++i)
                {
                    FEvent evt = (FEvent)_eventSelection.Editors[i].Obj;
                    if (evt.Start < start)
                        start = evt.Start;
                    if (evt.End > end)
                        end = evt.End;
                }
            }

            _viewRange = new FrameRange(start, end);
        }

        public void MoveEvents(int deltaFrames)
        {
            bool movingLeft = deltaFrames < 0;

            int howMuchCanMove = int.MaxValue;

            for (int i = 0; i != _trackSelection.Editors.Count; ++i)
            {
                if (movingLeft)
                {
                    for (int j = 0; j != _trackSelection.Editors[i]._eventEditors.Count; ++j)
                    {
                        FEventEditor evtEditor = _trackSelection.Editors[i]._eventEditors[j];
                        if (evtEditor.IsSelected)
                        {
                            if (j == 0)
                                howMuchCanMove = Mathf.Min(howMuchCanMove, evtEditor.Evt.Start);
                            else if (!_trackSelection.Editors[i]._eventEditors[j - 1].IsSelected)
                                howMuchCanMove = Mathf.Min(howMuchCanMove, evtEditor.Evt.Start - _trackSelection.Editors[i]._eventEditors[j - 1].Evt.End);
                        }
                    }
                }
                else
                {
                    int lastElementIndex = _trackSelection.Editors[i]._eventEditors.Count - 1;
                    for (int j = lastElementIndex; j != -1; --j)
                    {
                        FEventEditor evtEditor = _trackSelection.Editors[i]._eventEditors[j];
                        if (evtEditor.IsSelected)
                        {
                            if (j == lastElementIndex)
                                howMuchCanMove = Mathf.Min(howMuchCanMove, Sequence.Length - evtEditor.Evt.End);
                            else if (!_trackSelection.Editors[i]._eventEditors[j + 1].IsSelected)
                                howMuchCanMove = Mathf.Min(howMuchCanMove, _trackSelection.Editors[i]._eventEditors[j + 1].Evt.Start - evtEditor.Evt.End);
                        }
                    }
                }
            }

            if (movingLeft)
            {
                howMuchCanMove = -howMuchCanMove;
                deltaFrames = Mathf.Clamp(deltaFrames, howMuchCanMove, 0);
            }
            else
            {
                deltaFrames = Mathf.Clamp(deltaFrames, 0, howMuchCanMove);
            }

            if (deltaFrames != 0)
            {
                int start = 0;
                int limit = EventSelection.Editors.Count;
                int increment = 1;

                if (!movingLeft)
                {
                    start = limit - 1;
                    limit = -1;
                    increment = -increment;
                }
                int undoGroup = Undo.GetCurrentGroup();
                Undo.SetCurrentGroupName("move Events");
                for (int i = start; i != limit; i += increment)
                {
                    FrameRange newFrameRange = EventSelection.Editors[i].Evt.FrameRange;
                    newFrameRange.Start += deltaFrames;
                    newFrameRange.End += deltaFrames;
                    MoveEvent(EventSelection.Editors[i].Evt, newFrameRange);
                }
                Undo.CollapseUndoOperations(undoGroup);
            }

            Repaint();
        }

        public void ResizeEventsLeft(int delta)
        {
            int howMuchCanResize = int.MaxValue;

            // making them bigger?
            if (delta < 0)
            {
                for (int i = 0; i != _eventSelection.Editors.Count; ++i)
                {
                    int evtId = _eventSelection.Editors[i].Obj.GetId();
                    int howMuchCanEvtResize = _eventSelection.Editors[i].Evt.Start;
                    if (evtId > 0)
                        howMuchCanEvtResize -= _eventSelection.Editors[i].Evt.Track.GetEvent(evtId - 1).End;

                    if (howMuchCanResize > howMuchCanEvtResize)
                        howMuchCanResize = howMuchCanEvtResize;
                }

                delta = Mathf.Clamp(delta, -howMuchCanResize, 0);
            }
            else // making them smaller
            {
                for (int i = 0; i != _eventSelection.Editors.Count; ++i)
                {
                    int howMuchCanEvtResize = _eventSelection.Editors[i].Evt.Length - _eventSelection.Editors[i].Evt.GetMinLength();
                    if (howMuchCanResize > howMuchCanEvtResize)
                        howMuchCanResize = howMuchCanEvtResize;
                }

                delta = Mathf.Clamp(delta, 0, howMuchCanResize);
            }

            for (int i = 0; i != _eventSelection.Editors.Count; ++i)
            {
                FrameRange evtRange = _eventSelection.Editors[i].Evt.FrameRange;
                evtRange.Start += delta;
                MoveEvent(_eventSelection.Editors[i].Evt, evtRange);
            }
        }

        public void ResizeEventsRight(int delta)
        {
            int howMuchCanResize = int.MaxValue;

            // making them bigger?
            if (delta > 0)
            {
                for (int i = 0; i != _eventSelection.Editors.Count; ++i)
                {
                    int evtId = _eventSelection.Editors[i].Obj.GetId();
                    int howMuchCanEvtResize = _eventSelection.Editors[i].Evt.IsLastEvent ? Sequence.Length : _eventSelection.Editors[i].Evt.Track.GetEvent(evtId + 1).Start;

                    howMuchCanEvtResize -= _eventSelection.Editors[i].Evt.End;

                    if (howMuchCanResize > howMuchCanEvtResize)
                        howMuchCanResize = howMuchCanEvtResize;
                }

                delta = Mathf.Clamp(delta, 0, howMuchCanResize);
            }
            else // making them smaller
            {
                for (int i = 0; i != _eventSelection.Editors.Count; ++i)
                {
                    int howMuchCanEvtResize = _eventSelection.Editors[i].Evt.Length - _eventSelection.Editors[i].Evt.GetMinLength();
                    if (howMuchCanResize > howMuchCanEvtResize)
                        howMuchCanResize = howMuchCanEvtResize;
                }

                delta = Mathf.Clamp(delta, -howMuchCanResize, 0);
            }

            for (int i = 0; i != _eventSelection.Editors.Count; ++i)
            {
                FrameRange evtRange = _eventSelection.Editors[i].Evt.FrameRange;
                evtRange.End += delta;
                MoveEvent(_eventSelection.Editors[i].Evt, evtRange);
            }
        }


        public void MoveEvent(FEvent evt, FrameRange newFrameRange)
        {
            FrameRange oldFrameRange = evt.FrameRange;

            if (oldFrameRange == newFrameRange || newFrameRange.Start > newFrameRange.End)
                return;

            if (newFrameRange.Length < evt.GetMinLength() || newFrameRange.Length > evt.GetMaxLength())
            {
                evt.End = 300;
                evt.Start = evt.End - 20;
                Debug.LogError(string.Format("Trying to resize an Event to [{0},{1}] (length: {2}) which isn't a valid length, should be between [{3},{4}]", newFrameRange.Start, newFrameRange.End, newFrameRange.Length, evt.GetMinLength(), evt.GetMaxLength()), evt);
                //return;
            }

            bool changedLength = oldFrameRange.Length != newFrameRange.Length;

            if (!evt.Track.CanAdd(evt, newFrameRange))
                return;

            Undo.RecordObject(evt, changedLength ? "resize Event" : "move Event");

            evt.FrameRange = newFrameRange;

            GetEditor<FEventEditor>(evt).OnEventMoved(oldFrameRange);

            EditorUtility.SetDirty(evt);

            if (FSequenceEditorWindow.instance != null)
                FSequenceEditorWindow.instance.Repaint();
        }


        public void SelectExclusive(ISelectableElement e)
        {
            int undoGroup = Undo.GetCurrentGroup();
            DeselectAll();

            Select(e);
            Undo.CollapseUndoOperations(undoGroup);

            Repaint();
        }

        public void Select(List<ISelectableElement> elements)
        {
            Undo.IncrementCurrentGroup();

            foreach (ISelectableElement e in elements)
            {
                if (!e.IsSelected)
                    Select(e);
            }

            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());

            Repaint();
        }

        public void Deselect(IEnumerable<ISelectableElement> elements)
        {

            foreach (ISelectableElement e in elements)
            {
                if (e.IsSelected)
                    Deselect(e);
            }

            Repaint();
        }


        public void Select(ISelectableElement e)
        {
            if (e.IsSelected)
                return;

            int undoGroup = Undo.GetCurrentGroup();
            string undoStr = "select ";

            if (e is FEventEditor)
            {
                Select(((FEventEditor)e).TrackEditor);
                Selection.activeObject = ((FEventEditor)e).Evt.transform;
                undoStr += "Event";
            }
            else if (e is FTrackEditor)
            {
                Selection.activeObject = ((FTrackEditor)e).Track.transform;
                undoStr += "Track";
            }
            else if (e is FTimelineEditor)
            {
                Deselect(EventSelection.Editors.ToArray());
                Deselect(TrackSelection.Editors.ToArray());
                Deselect(ContainerSelection.Editors.ToArray());
                undoStr += "Timeline";
            }
            else if (e is FContainerEditor)
            {
                Deselect(EventSelection.Editors.ToArray());
                Deselect(TrackSelection.Editors.ToArray());
                Deselect(TimelineSelection.Editors.ToArray());
                undoStr += "Container";
            }

            Undo.RecordObjects(new Object[] { (UnityEngine.Object)e, this }, undoStr);

            if (e is FEventEditor)
                EventSelection.Add((FEventEditor)e);
            else if (e is FTrackEditor)
                TrackSelection.Add((FTrackEditor)e);
            else if (e is FTimelineEditor)
                TimelineSelection.Add((FTimelineEditor)e);
            else if (e is FContainerEditor)
                ContainerSelection.Add((FContainerEditor)e);

            e.OnSelect();

            Repaint();
            Undo.CollapseUndoOperations(undoGroup);
        }

        public void Deselect(ISelectableElement e)
        {
            if (!e.IsSelected)
                return;

            string undoStr = "deselect ";

            if (e is FEventEditor)
                undoStr += "Event";
            else if (e is FTrackEditor)
                undoStr += "Track";
            else if (e is FTimelineEditor)
                undoStr += "Timeline";
            else if (e is FContainerEditor)
                undoStr += "Container";

            Undo.RecordObjects(new Object[] { (UnityEngine.Object)e, this }, undoStr);

            if (e is FEventEditor)
                EventSelection.Remove((FEventEditor)e);
            else if (e is FTrackEditor)
                TrackSelection.Remove((FTrackEditor)e);
            else if (e is FTimelineEditor)
                TimelineSelection.Remove((FTimelineEditor)e);
            else if (e is FContainerEditor)
                ContainerSelection.Remove((FContainerEditor)e);

            e.OnDeselect();

            Repaint();
        }

        public void DeselectAll()
        {
            int numEvents = EventSelection.Editors.Count;
            int numTracks = TrackSelection.Editors.Count;
            int numTimelines = TimelineSelection.Editors.Count;
            int numContainers = ContainerSelection.Editors.Count;

            int totalSelected = numEvents + numTracks + numTimelines + numContainers;

            if (totalSelected == 0)
                return;

            // tracks + events + window
            Object[] objsToSave = new Object[totalSelected + 1];

            int i = 0;

            for (int j = 0; j != numEvents; ++i, ++j)
                objsToSave[i] = EventSelection.Editors[j];

            for (int j = 0; j != numTracks; ++i, ++j)
                objsToSave[i] = TrackSelection.Editors[j];

            for (int j = 0; j != numTimelines; ++i, ++j)
                objsToSave[i] = TimelineSelection.Editors[j];

            for (int j = 0; j != numContainers; ++i, ++j)
                objsToSave[i] = ContainerSelection.Editors[j];

            objsToSave[totalSelected] = this;

            Undo.RegisterCompleteObjectUndo(objsToSave, "deselect all");

            for (int j = 0; j != numEvents; ++j)
                EventSelection.Editors[j].OnDeselect();

            for (int j = 0; j != numTracks; ++j)
                TrackSelection.Editors[j].OnDeselect();

            for (int j = 0; j != numTimelines; ++j)
                TimelineSelection.Editors[j].OnDeselect();

            for (int j = 0; j != numContainers; ++j)
                ContainerSelection.Editors[j].OnDeselect();

            EventSelection.Clear();
            TrackSelection.Clear();
            TimelineSelection.Clear();
            ContainerSelection.Clear();

            Repaint();

            if (FInspectorWindow._instance)
            {
                FInspectorWindow._instance.Repaint();
            }
        }

        public void EnableAllTracks(bool enable)
        {
            foreach (FContainerEditor containerEditor in Editors)
            {
                foreach (FTimelineEditor timelineEditor in containerEditor.Editors)
                {
                    foreach (FTrackEditor trackEditor in timelineEditor.Editors)
                    {
                        if (trackEditor.Track.enabled != enable)
                        {
                            trackEditor.OnToggle(enable);
                        }
                    }
                }
            }
        }

        private Dictionary<int, FTrackEditor> _dirtyTracks = new Dictionary<int, FTrackEditor>();

        public void SetDirty(FTrackEditor trackEditor)
        {
            if (!_dirtyTracks.ContainsKey(trackEditor.GetInstanceID()))
                _dirtyTracks.Add(trackEditor.GetInstanceID(), trackEditor);
            _timeNotifyDirtyTracks = EditorApplication.timeSinceStartup + NOTIFY_DIRTY_TRACKS_DELTA;
        }

        public void SetAllDirty()
        {
            foreach (FContainerEditor containerEditor in Editors)
            {
                foreach (FTimelineEditor timelineEditor in containerEditor.Editors)
                {
                    foreach (FTrackEditor trackEditor in timelineEditor.Editors)
                    {
                        SetDirty(trackEditor);
                    }
                }
            }
        }

        public void NotifyDirtyTracks()
        {
            int currentFrame = Sequence.CurrentFrame;

            Dictionary<int, FTrackEditor>.Enumerator e = _dirtyTracks.GetEnumerator();

            while (e.MoveNext())
            {
                e.Current.Value.OnTrackChanged();
            }

            _dirtyTracks.Clear();

            if (currentFrame >= 0 && Sequence.CurrentFrame != currentFrame)
                SetCurrentFrame(currentFrame);
        }

        #region Playback functions

        private bool _isPlaying = false;
        public bool IsPlaying { get { return _isPlaying; } }

        private bool _isPlayingForward = true;
        public bool IsPlayingForward
        {
            get { return _isPlayingForward; }
            set
            {
                _isPlayingForward = value;
                if (Sequence)
                {
                    Sequence.Speed = (_isPlayingForward ? 1 : -1) * Mathf.Abs(Sequence.Speed);
                }
            }
        }

        private double _timeLastUpdate = -1;

        private float _currentTime = 0;

        public void Play()
        {
            Play(true);
        }
        /// <summary>
        /// 启动事件
        /// </summary>
        public void CheckOtherEvent()
        {

        }

        public List<FTrack> GetTracks()
        {
            var res = new List<FTrack>();
            foreach (var con in Sequence.Containers)
            {
                foreach (var timelines in con.Timelines)
                {
                    foreach (var tracks in timelines.Tracks)
                    {
                        res.Add(tracks);
                    }
                }
            }
            return res;
        }


        public void Play(bool restart)
        {
            if (!Sequence.IsStopped && restart)
                Sequence.Stop();

            CheckOtherEvent();


            int frame = _viewRange.Cull(Sequence.CurrentFrame);
            if (!Sequence.IsPlayingForward && frame == 0)
            {
                Sequence.Stop();
                frame = _viewRange.End;
            }

            _timeLastUpdate = EditorApplication.timeSinceStartup;

            _currentTime = frame * Sequence.InverseFrameRate;

            if (Sequence.Speed == 0)
                Sequence.Speed = Sequence.DefaultSpeed;

            if (Application.isPlaying)
            {
                Sequence.Play(_currentTime);
            }
            else
            {
                SetCurrentFrame(frame);

                _isPlaying = true;
            }

            FUtility.RepaintGameView();
        }

        public void Stop()
        {
            Debug.Log("yns  stop ");//FTransformTrack.Restore
            if (!object.Equals(Sequence, null))
            {
                if (!Sequence.IsStopped)
                {
                    Sequence.Stop(true);

                    for (int i = 0; i != Editors.Count; ++i)
                        Editors[i].OnStop();
                }
                else
                {
                    ClearAnimPos();
                    //Sequence.DestoryAllcache();
                    //OnWillChange();
                }

            }
            _isPlaying = false;
            FUtility.RepaintGameView();
        }

        private void ClearAnimPos()
        {
            Debug.Log($"yns  ClearAnim pos");
            foreach (var con in Sequence.Containers)
            {
                foreach (var timeline in con.Timelines)
                {
                    foreach (var track in timeline.Tracks)
                    {
                        if (track is FTransformTrack)
                        {
                            FTransformTrack fTransform = (FTransformTrack)track;
                            (track as FTransformTrack).ClearSnapshot();
                        }
                        if (track is FAnimationTrack)
                        {
                            (track as FAnimationTrack).ClearSnapshot();
                        }
                    }
    
                }
            }
        }

        public void Pause()
        {
            //			Debug.Log ("Pause");
            Sequence.Pause();

            _isPlaying = false;

            FUtility.RepaintGameView();
        }

        public void SetCurrentFrame(int frame)
        {
            SetCurrentTime(frame * Sequence.InverseFrameRate);
        }

        //FSwitchEvent switchEvent = null;
        public void SetSwitchEvent(FSwitchEvent _switchEvent)
        {
            SetCurrentTime((float)_switchEvent.ToFrame / (float)Sequence.FrameRate);
        }

        private int LastFrame = -1;

        public void SetCurrentTime(float time)
        {
            if (_dirtyTracks.Count > 0)
                NotifyDirtyTracks();

            _currentTime = time;

            if (!Sequence.IsInit)
                Sequence.Init();

            int frame = (int)(time * Sequence.FrameRate);
            if(LastFrame == frame && LastFrame!=0)
            {
                return;
            }
            LastFrame = frame;

            Sequence.SetCurrentTimeEditor(frame * Sequence.InverseFrameRate);

            for (int i = 0; i != Editors.Count; ++i)
                Editors[i].UpdateTimelines(frame, time);

            FUtility.RepaintGameView();
        }

        private bool isCompilingEnd = true;

        public void Update()
        {
            if (EditorApplication.isCompiling)
            {
                if(isCompilingEnd)
                    Stop();

                isCompilingEnd = false;
                _isEditorCompiling = true;
            }
            else
            {
                isCompilingEnd = true;
            }
              
            if (_isEditorCompiling && isCompilingEnd)
            {
                _isEditorCompiling = false;
                Refresh();   
            }

            // going into playmode
            if (EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying)
            {
                OnPlaymodeWillChange();
            }

            // coming out of playmode
            if (!EditorApplication.isPlayingOrWillChangePlaymode && EditorApplication.isPlaying)
            {
                _wasUnityInPlaymode = true;
                OnPlaymodeWillChange();
            }

            _onUpdateEvent.Invoke();

            if (_dirtyTracks.Count > 0 && _timeNotifyDirtyTracks < EditorApplication.timeSinceStartup /* && !_sequence.IsStopped */)
                NotifyDirtyTracks();

            //			if( EditorApplication.isPlayingOrWillChangePlaymode )
            //			{
            //				Debug.Log("Application.isPlaying: " + Application.isPlaying + 
            //				          "\nEditorApplication.isPlaying: " + EditorApplication.isPlaying + 
            //				          "\nisPlayingOrWillChangePlaymode: " + EditorApplication.isPlayingOrWillChangePlaymode + 
            //				          "\nisPaused: " + EditorApplication.isPaused +
            //				          "\nisUpdating: " + EditorApplication.isUpdating );
            //			}

            if (!_isPlaying)
                return;

            float delta = (float)((EditorApplication.timeSinceStartup - _timeLastUpdate) * Sequence.Speed);
;
            _timeLastUpdate = EditorApplication.timeSinceStartup;

            if (Sequence.UpdateMode != AnimatorUpdateMode.UnscaledTime)
            {
                delta *= Time.timeScale;
            }

            float newTime = _currentTime + delta;

            if (newTime > _viewRange.End * Sequence.InverseFrameRate)
            {
                Sequence.OnFinishedCallback.Invoke(Sequence);

                Play();
            }
            else if (newTime < 0 && !Sequence.IsPlayingForward)
            {
                Sequence.OnFinishedCallback.Invoke(Sequence);

                Play();
            }
            else
            {
                SetCurrentTime(newTime);
            }

            Repaint();


        }


        #endregion

        #region FEditor Copy / Paste

        public static FObject CopyObject { get { return _copyObject; } }

        private static FObject _copyObject = null;

        public void CopyEditor(FEditor editor)
        {
            Undo.RecordObject(this, string.Empty);
            if (_copyObject != null)
            {
                Undo.DestroyObjectImmediate(_copyObject.gameObject);
            }
            _copyObject = Instantiate<FObject>(editor.Obj);
            string objName = _copyObject.name;
            objName = objName.Substring(0, objName.Length - "(Clone)".Length);
            _copyObject.gameObject.name = objName;
            _copyObject.gameObject.hideFlags = HideFlags.HideAndDontSave;
            Undo.RegisterCreatedObjectUndo(CopyObject.gameObject, "Copy " + editor.Obj.name);
        }

        public void CutEditor(FEditor editor)
        {
            if (editor.IsSelected)
                Deselect(editor);
            CopyEditor(editor);
            DestroyEditor(editor);
        }

        #endregion FEditor Copy / Paste
    }
}
