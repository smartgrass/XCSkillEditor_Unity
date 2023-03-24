using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
using System;

using Flux;

namespace FluxEditor
{
    [FEditor(typeof(FAnimationTrack))]
    public class FAnimationTrackEditor : FTrackEditor
    {

        #region Debug tools

        private bool _syncWithAnimationWindow = false;
        private bool SyncWithAnimationWindow
        {
            get { return _syncWithAnimationWindow; }
            set
            {

                if (value)
                {
                    SequenceEditor.OnUpdateEvent.AddListener(OnUpdate);
                    foreach (FContainerEditor containerEditor in SequenceEditor.Editors)
                    {
                        List<FTimelineEditor> timelineEditors = containerEditor.Editors;
                        foreach (FTimelineEditor timelineEditor in timelineEditors)
                        {
                            List<FTrackEditor> trackEditors = timelineEditor.Editors;
                            foreach (FTrackEditor trackEditor in trackEditors)
                            {
                                if (trackEditor is FAnimationTrackEditor && ((FAnimationTrackEditor)trackEditor).SyncWithAnimationWindow)
                                    ((FAnimationTrackEditor)trackEditor).SyncWithAnimationWindow = false;
                            }
                        }
                    }

                    AnimationWindowProxy.OpenAnimationWindow();
                }
                else
                {
                    if (TimelineEditor != null)
                        SequenceEditor.OnUpdateEvent.RemoveListener(OnUpdate);

                    if (AnimationMode.InAnimationMode())
                        AnimationMode.StopAnimationMode();
                }

                _syncWithAnimationWindow = value;

                if (TimelineEditor != null)
                    SequenceEditor.Repaint();
            }
        }

        private bool _showKeyframes = false;
        public bool ShowKeyframes
        {
            get { return _showKeyframes; }
            set { _showKeyframes = value; }
        }
        private bool _showKeyframeTimes = false;
        public bool ShowKeyframeTimes
        {
            get { return _showKeyframeTimes; }
            set { _showKeyframeTimes = value; }
        }


        private bool _showTransformPath = false;
        public bool ShowTransformPath
        {
            get { return _showTransformPath; }
            set { _showTransformPath = value; }
        }

        #endregion

        protected override void OnEnable()
        {
            base.OnEnable();

            SceneView.onSceneGUIDelegate += OnSceneGUI;

            // was it syncing anim? hook it up again, because UnityEvent will 
            // lose references on compile
            if (_syncWithAnimationWindow)
            {
                _syncWithAnimationWindow = false;
                //				SyncWithAnimationWindow = true;
            }
        }

        private FEvent _previousEvent = null;

        private void OnUpdate()
        {
            if (!_syncWithAnimationWindow)
                return;

            if (Selection.activeTransform != Track.Owner)
            {
                Selection.activeTransform = Track.Owner;
            }

            if (!AnimationMode.InAnimationMode())
            {
                AnimationWindowProxy.StartAnimationMode();
            }

            int animWindowFrame = AnimationWindowProxy.GetCurrentFrame();

            FEvent[] evts = new FEvent[2];
            int numEvts = Track.GetEventsAt(SequenceEditor.Sequence.CurrentFrame, evts);

            if (numEvts > 0)
            {
                if (numEvts == 1)
                {
                    _previousEvent = evts[0];
                }
                else if (numEvts == 2)
                {
                    if (_previousEvent != evts[0] && _previousEvent != evts[1])
                    {
                        _previousEvent = evts[1];
                    }
                }

                FPlayAnimationEvent animEvt = (FPlayAnimationEvent)_previousEvent;
                if (animEvt.ControlsAnimation)
                {
                    int normCurrentFrame = SequenceEditor.Sequence.CurrentFrame - animEvt.Start;

                    if (AnimationWindowProxy.GetSelectedAnimationClip() != animEvt._animationClip)
                    {
                        AnimationWindowProxy.SelectAnimationClip(animEvt._animationClip);
                        AnimationWindowProxy.SetCurrentFrame(normCurrentFrame, SequenceEditor.Sequence.CurrentTime - animEvt.StartTime);
                    }

                    if (animWindowFrame > animEvt.Length)
                    {
                        animWindowFrame = animEvt.Length;
                        AnimationWindowProxy.SetCurrentFrame(animWindowFrame, animEvt.LengthTime);
                    }

                    if (animWindowFrame >= 0 && animWindowFrame != normCurrentFrame)
                    {
                        SequenceEditor.SetCurrentFrame(animEvt.Start + animWindowFrame);
                        SequenceEditor.Repaint();
                    }
                }
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            SceneView.onSceneGUIDelegate -= OnSceneGUI;

            if (SyncWithAnimationWindow)
                SyncWithAnimationWindow = false;
        }

        public override void Init(FObject obj, FEditor owner)
        {
            base.Init(obj, owner);

            FAnimationTrack animTrack = (FAnimationTrack)Obj;

            if (animTrack.Owner.GetComponent<Animator>() == null)
            {
                Animator animator = animTrack.Owner.gameObject.AddComponent<Animator>();
                Undo.RegisterCreatedObjectUndo(animator, string.Empty);
            }
        }

        public override void OnTrackChanged()
        {
            FAnimationTrackInspector.RebuildStateMachine((FAnimationTrack)Track);
        }


        public override void Render(Rect rect, float headerWidth)
        {
            //			bool isPreviewing = _track.IsPreviewing;

            base.Render(rect, headerWidth);

            //			if( isPreviewing != _track.IsPreviewing )
            //			{
            //				if( Event.current.alt )
            //					SyncWithAnimationWindow = true;
            //			}

            switch (Event.current.type)
            {
                case EventType.DragUpdated:
                    if (rect.Contains(Event.current.mousePosition))
                    {
                        int numAnimationsDragged = FAnimationEventInspector.NumAnimationsDragAndDrop(Track.Sequence.FrameRate);
                        int frame = SequenceEditor.GetFrameForX(Event.current.mousePosition.x);

                        DragAndDrop.visualMode = numAnimationsDragged > 0 && Track.CanAddAt(frame) ? DragAndDropVisualMode.Copy : DragAndDropVisualMode.Rejected;
                        Event.current.Use();
                        //Debug.Log("yns  animtrack use "  + numAnimationsDragged  + "  " + frame);
                    }
                    break;
                case EventType.DragPerform:
                    if (rect.Contains(Event.current.mousePosition))
                    {

                        AnimationClip animClip = FAnimationEventInspector.GetAnimationClipDragAndDrop(Track.Sequence.FrameRate);
                        if (animClip && Mathf.Approximately(animClip.frameRate, Track.Sequence.FrameRate))
                        {
                            int frame = SequenceEditor.GetFrameForX(Event.current.mousePosition.x);
                            int maxLength;
                            Debug.Log($"yns  CanAddAt frame {frame} {Track.CanAddAt(frame, out maxLength)}");
                            if (Track.CanAddAt(frame, out maxLength))
                            {
                                int end = frame + (int)(animClip.length * Track.Sequence.FrameRate);

                                FrameRange Range = new FrameRange(frame,Mathf.Min(end, frame + maxLength));
                                FPlayAnimationEvent animEvt = FEvent.Create<FPlayAnimationEvent>(Range);
                                Track.Add(animEvt);
                                Debug.Log("yns  add " + Range);
                                FAnimationEventInspector.SetAnimationClip(animEvt, animClip);
                                DragAndDrop.AcceptDrag();
                            }
                        }
                        else if (animClip)
                        {
                            Debug.Log("yns  frameRate animClip = " + animClip.frameRate + "  Sequence.FrameRate= " + Track.Sequence.FrameRate);
                        }

                        Event.current.Use();
                    }
                    break;
            }
        }

        public override bool HasTools()
        {
            bool controlsAllAnimations = true;
            foreach (FAnimationEventEditor animEvtEditor in _eventEditors)
            {
                if (!((FPlayAnimationEvent)animEvtEditor.Evt).ControlsAnimation)
                {
                    controlsAllAnimations = false;
                    break;
                }
            }
            return controlsAllAnimations;
        }

        public override void OnToolsGUI()
        {
            //			bool canSyncWithAnimationWindow = true;
            //			foreach( FAnimationEventEditor animEvtEditor in _eventEditors )
            //			{
            //				if(! ((FPlayAnimationEvent)animEvtEditor._evt).ControlsAnimation )
            //				{
            //					canSyncWithAnimationWindow = false;
            //					break;
            //				}
            //			}
            //
            //			if( canSyncWithAnimationWindow )
            {
                bool syncWithAnimationWindow = EditorGUILayout.Toggle("Sync w/ Animation Window", SyncWithAnimationWindow);
                if (syncWithAnimationWindow != SyncWithAnimationWindow)
                    SyncWithAnimationWindow = syncWithAnimationWindow;
            }

            bool showTransformPath = EditorGUILayout.Toggle("Show Transform Path", ShowTransformPath);
            if (showTransformPath != ShowTransformPath)
                ShowTransformPath = showTransformPath;

            if (!ShowTransformPath)
                GUI.enabled = false;

            bool showKeyframes = EditorGUILayout.Toggle("Show Keyframes", ShowKeyframes);
            if (showKeyframes != ShowKeyframes)
                ShowKeyframes = showKeyframes;

            bool showKeyframeTimes = EditorGUILayout.Toggle("Show Keyframe Times", ShowKeyframeTimes);
            if (showKeyframeTimes != ShowKeyframeTimes)
                ShowKeyframeTimes = showKeyframeTimes;

            GUI.enabled = true;
        }

        protected override Color GetPreviewIconColor()
        {
            return _syncWithAnimationWindow ? Color.red : base.GetPreviewIconColor();
        }

        void OnSceneGUI(SceneView sceneView)
        {
            if (Track == null)
                return;

            for (int i = 0; i != _eventEditors.Count; ++i)
            {
                FAnimationEventEditor animEvtEditor = (FAnimationEventEditor)_eventEditors[i];
                FPlayAnimationEvent animEvt = (FPlayAnimationEvent)_eventEditors[i].Evt;
                if (animEvt._animationClip != null && Flux.FUtility.IsAnimationEditable(animEvt._animationClip) && ShowTransformPath /*_track.IsPreviewing*/ )
                {
                    animEvtEditor.RenderTransformCurves(animEvt.Sequence.FrameRate);
                }
            }

            SceneView.RepaintAll();
        }

        private void RenderTransformPath(TransformCurves transformCurves, float length, float samplingDelta)
        {
            float t = 0;

            int numberSamples = Mathf.RoundToInt(length / samplingDelta) + 1;

            float delta = length / numberSamples;

            Vector3[] pts = new Vector3[numberSamples];

            int index = 0;

            while (index < numberSamples)
            {
                pts[index++] = transformCurves.GetPosition(t);
                t += delta;
            }

            if (index != pts.Length)
                Debug.LogError("Number of samples doesn't match: " + (index + 1) + " instead of " + pts.Length);

            Handles.DrawPolyLine(pts);
        }

        private void RenderTransformAnimation(TransformCurves transformCurves, float time)
        {
            Vector3 pos = transformCurves.GetPosition(time);
            Quaternion rot = transformCurves.GetRotation(time);
            Vector3 scale = transformCurves.GetScale(time);

            transformCurves.bone.localScale = scale;
            transformCurves.bone.localRotation = rot;
            transformCurves.bone.localPosition = pos;

            Handles.RectangleHandleCap(0, pos, rot, 0.1f, EventType.MouseDown);
            Handles.RectangleHandleCap(0, pos + rot * Vector3.forward, rot, 0.4f, EventType.MouseDown);
        }

        public override void UpdateEventsEditor(int frame, float time)
        {
            if (Track.RequiresEditorCache && !Track.HasCache && Track.CanCreateCache())
            {
                OnToggle(true);
            }

            base.UpdateEventsEditor(frame, time);

            if (_syncWithAnimationWindow)
            {
                FEvent[] evts = new FEvent[2];
                int numEvts = Track.GetEventsAt(frame, evts);
                if (numEvts > 0)
                {
                    if (numEvts == 1)
                    {
                        _previousEvent = evts[0];
                    }
                    else if (numEvts == 2)
                    {
                        if (_previousEvent != evts[0] && _previousEvent != evts[1])
                        {
                            _previousEvent = evts[1];
                        }
                    }

                    FPlayAnimationEvent animEvt = (FPlayAnimationEvent)_previousEvent;
                    if (animEvt._animationClip != AnimationWindowProxy.GetSelectedAnimationClip())
                    {
                        AnimationWindowProxy.SelectAnimationClip(animEvt._animationClip);
                    }

                    AnimationWindowProxy.SetCurrentFrame(frame - animEvt.Start, time - animEvt.StartTime);
                }
            }
        }
    }
}
