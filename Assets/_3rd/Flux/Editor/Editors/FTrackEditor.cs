using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditorInternal;
using System;
using System.Collections.Generic;
using System.Reflection;
using Flux;

namespace FluxEditor
{
    public class FTrackEditor : FEditor
    {
        public const int DEFAULT_TRACK_HEIGHT = 20;

        public const int KEYFRAME_WIDTH = 4;
        public const int KEYFRAME_HALF_WIDTH = KEYFRAME_WIDTH / 2;

        public FTrack Track { get { return (FTrack)Obj; } }

        public List<FEventEditor> _eventEditors = new List<FEventEditor>();

        public FTimelineEditor TimelineEditor { get { return (FTimelineEditor)Owner; } }

        private GUIContent _enableContent;

        public float HeaderWidth { get; private set; }

        protected override void OnEnable()
        {
            base.OnEnable();

            _enableContent = new GUIContent((Texture2D)AssetDatabase.LoadAssetAtPath(FUtility.GetFluxSkinPath() + "View.png", typeof(Texture2D)), "Enable/Disable Track");
        }

        public override void Init(FObject obj, FEditor owner)
        {
            base.Init(obj, owner);

            _eventEditors.Clear();

            List<FEvent> events = Track.Events;

            for (int i = 0; i < events.Count; ++i)
            {
                FEvent evt = events[i];
                FEventEditor evtEditor = SequenceEditor.GetEditor<FEventEditor>(evt);
                evtEditor.Init(evt, this);
                _eventEditors.Add(evtEditor);
            }
        }


        public virtual void OnStop()
        {
        }

        public override float Height
        {
            get
            {
                return _eventEditors != null && _eventEditors.Count > 0 ? _eventEditors[0].Height : DEFAULT_TRACK_HEIGHT;
            }
        }

        public override void ReserveGuiIds()
        {
            base.ReserveGuiIds();
            for (int i = 0; i != _eventEditors.Count; ++i)
            {
                _eventEditors[i].ReserveGuiIds();
            }
        }

        public void SelectEvents(FrameRange range)
        {
            for (int i = 0; i != _eventEditors.Count; ++i)
            {
                if (range.Overlaps(_eventEditors[i].Evt.FrameRange))
                    SequenceEditor.Select(_eventEditors[i]);
                else if (_eventEditors[i].Evt.Start > range.End)
                    break;
            }
        }

        public void DeselectEvents(FrameRange range)
        {
            for (int i = 0; i != _eventEditors.Count; ++i)
            {
                if (range.Overlaps(_eventEditors[i].Evt.FrameRange))
                    SequenceEditor.Deselect(_eventEditors[i]);
                else if (_eventEditors[i].Evt.Start > range.End)
                    break;
            }
        }

        protected virtual void RenderHeader(Rect labelRect, GUIContent label)
        {
            GUI.Label(labelRect, label, FGUI.GetTrackHeaderStyle());
        }

        public override void Render(Rect rect, float headerWidth)
        {
            Rect = rect;

            HeaderWidth = headerWidth;

            Rect headerRect = rect;
            headerRect.width = headerWidth;

            Rect enableButtonRect = rect;
            enableButtonRect.xMax = rect.xMin + headerWidth;
            enableButtonRect.xMin = enableButtonRect.xMax - 16;
            enableButtonRect.height = 16;

            Rect trackHeaderRect = rect;
            trackHeaderRect.width = headerWidth;

            Color guiColor = GUI.color;

            bool selected = _isSelected;

            if (selected)
            {
                Color c = FGUI.GetSelectionColor();
                c.a = GUI.color.a;
                GUI.color = c;
                GUI.DrawTexture(trackHeaderRect, EditorGUIUtility.whiteTexture);
                GUI.color = guiColor;
            }

            GUI.color = GetPreviewIconColor();

            if (!Track.enabled)
            {
                Color c = guiColor;
                c.a = 0.5f;
                GUI.color = c;
            }

            if (FGUI.Button(enableButtonRect, _enableContent))
            {
                if (Event.current.shift) // turn all?
                {
                    SequenceEditor.EnableAllTracks(!Track.enabled);
                }
                else
                {
                    OnToggle(!Track.enabled);
                }
                FUtility.RepaintGameView();
                Event.current.Use();
            }

            Rect trackLabelRect = trackHeaderRect;
            trackLabelRect.xMin += 8;

            RenderHeader(trackLabelRect, new GUIContent(Track.name));

            rect.xMin = trackHeaderRect.xMax;

            if (rect.Contains(Event.current.mousePosition))
                SequenceEditor.SetMouseHover(Event.current.mousePosition.x - rect.xMin, this);

            FrameRange validKeyframeRange = new FrameRange(0, SequenceEditor.Sequence.Length);

            _contentOffset = rect.min;

            GUI.BeginGroup(rect);

            rect.x = 0;
            rect.y = 0;

            for (int i = 0; i != _eventEditors.Count; ++i)
            {
                if (i == 0)
                    validKeyframeRange.Start = 0;
                else
                    validKeyframeRange.Start = _eventEditors[i - 1].Evt.End;

                if (i == _eventEditors.Count - 1)
                    validKeyframeRange.End = SequenceEditor.Sequence.Length;
                else
                    validKeyframeRange.End = _eventEditors[i + 1].Evt.Start;

                rect.xMin = SequenceEditor.GetXForFrame(_eventEditors[i].Evt.Start);
                rect.xMax = SequenceEditor.GetXForFrame(_eventEditors[i].Evt.End);
                _eventEditors[i].Render(rect, SequenceEditor.ViewRange, SequenceEditor.PixelsPerFrame, validKeyframeRange);
            }

            GUI.EndGroup();

            switch (Event.current.type)
            {
                case EventType.ContextClick:
                    if (trackHeaderRect.Contains(Event.current.mousePosition))
                    {
                        OnHeaderContextClick();
                    }
                    else if (Rect.Contains(Event.current.mousePosition))
                    {
                        Debug.Log("yns  OnBodyContextClick ");
                        OnBodyContextClick();
                    }
                    break;
                case EventType.MouseDown:
                    if (EditorGUIUtility.hotControl == 0 && trackHeaderRect.Contains(Event.current.mousePosition))
                    {
                        if (Event.current.button == 0) // selecting
                        {
                            if (Event.current.control)
                            {
                                if (IsSelected)
                                    SequenceEditor.Deselect(this);
                                else
                                    SequenceEditor.Select(this);
                            }
                            else if (Event.current.shift)
                            {
                                SequenceEditor.Select(this);
                            }
                            else
                            {
                                SequenceEditor.SelectExclusive(this);
                            }
                            Event.current.Use();
                        }
                    }
                    break;
                case EventType.MouseUp:
                    break;

                case EventType.MouseDrag:
                    //Debug.Log("yns  drag");
                    break;
                case EventType.DragPerform:
                    //int t = SequenceEditor.GetFrameForX(Event.current.mousePosition.x - HeaderWidth);
                    //Debug.Log("yns  DragPerform " + t);
                    //TryAddEvent()
                    break;
            }

            Handles.color = FGUI.GetLineColor();
            Handles.DrawLine(Rect.min, Rect.min + new Vector2(Rect.width, 0));
            Handles.DrawLine(Rect.max, Rect.max - new Vector2(Rect.width, 0));

            GUI.color = guiColor;
        }

        public virtual void OnToggle(bool on)
        {
            Undo.RecordObject(Track, on ? "enable" : "disable");
            Track.enabled = on;
            EditorUtility.SetDirty(Track);

            if (Track.RequiresEditorCache)
            {
                if (on)
                {
                    Track.CreateCache();
                }
                else
                {
                    Track.ClearCache();
                }
            }

            if (!SequenceEditor.Sequence.IsStopped)
            {
                int currentFrame = SequenceEditor.Sequence.CurrentFrame;
                SequenceEditor.Stop();
                SequenceEditor.SetCurrentFrame(currentFrame);
            }

            SequenceEditor.SetDirty(this);
            SequenceEditor.NotifyDirtyTracks();
        }

        public override void OnDelete()
        {
            base.OnDelete();

            if (Track.HasCache)
                Track.ClearCache();
        }

        protected virtual Color GetPreviewIconColor()
        {
            return FGUI.GetIconColor();
        }

        public virtual void OnToolsGUI()
        {
        }

        public virtual bool HasTools() { return false; }

        protected virtual void OnHeaderContextClick()
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Copy"), false, Copy);
            menu.AddItem(new GUIContent("Cut"), false, Cut);
            menu.AddItem(new GUIContent("Duplicate"), false, Duplicate);
            menu.AddItem(new GUIContent("Delete"), false, Delete);
            menu.ShowAsContext();

            Event.current.Use();
        }

        protected virtual void OnBodyContextClick()
        {
            int t = SequenceEditor.GetFrameForX(Event.current.mousePosition.x - HeaderWidth);

            GUIContent selectAllEvents = new GUIContent("Select All Events");
            GUIContent addEventAtFrame = new GUIContent("Add Event At Frame");
            GUIContent addEventFillGap = new GUIContent("Add Event Fill Gap");

            GUIContent pasteEvent = new GUIContent("Paste Event");

            GenericMenu menu = new GenericMenu();
            menu.AddItem(selectAllEvents, false, SelectAllEvents);
            if (Track.CanAddAt(t))
            {
                menu.AddItem(addEventAtFrame, false, AddEventAtPoint, t);
                menu.AddItem(addEventFillGap, false, AddEventFillGap, t);
            }
            else
            {
                menu.AddDisabledItem(addEventAtFrame);
                menu.AddDisabledItem(addEventFillGap);
            }

            if (FSequenceEditor.CopyObject != null)
            {
                FEvent copyEvt = FSequenceEditor.CopyObject as FEvent;

                if (copyEvt != null)
                {
                    if (FSequenceEditor.CopyObject.GetType() != Track.GetEventType())
                    {
                        menu.AddDisabledItem(pasteEvent);
                    }
                    else
                    {
                        FEvent eventBefore = Track.GetEventBefore(t);
                        FEvent eventAfter = Track.GetEventAfter(t);

                        FrameRange validRange = new FrameRange(eventBefore == null ? 0 : eventBefore.End, eventAfter == null ? SequenceEditor.Sequence.Length : eventAfter.Start);

                        if (validRange.Length >= copyEvt.Length)
                            menu.AddItem(pasteEvent, false, PasteEvent, t);
                        else
                            menu.AddDisabledItem(pasteEvent);
                    }
                }
            }

            menu.ShowAsContext();
            Event.current.Use();
        }

        private void PasteEvent(object frameBoxed)
        {
            int frame = (int)frameBoxed;

            FEvent evt = Instantiate<FEvent>((FEvent)FSequenceEditor.CopyObject);
            evt.hideFlags = Track.hideFlags;
            Undo.RegisterCreatedObjectUndo(evt.gameObject, "Paste " + FSequenceEditor.CopyObject.name);
            Undo.RecordObject(Track, string.Empty);

            int evtLength = evt.Length;

            int maxEvtLength;
            if (Track.CanAddAt(frame, out maxEvtLength) && maxEvtLength >= evtLength)
            {
                int delta = frame - evt.Start;
                evt.Start += delta;
                evt.End += delta;
                Debug.Log("yns  pasteEvent ");
            }
            else
            {
                Debug.Log("yns  else ");
                evt.Start = frame;
                evt.End = frame + 10;
            }
            Undo.SetTransformParent(evt.transform, Track.transform, string.Empty);
            Track.Add(evt);
        }

        private void Copy()
        {
            SequenceEditor.CopyEditor(this);
        }

        private void Cut()
        {
            SequenceEditor.CutEditor(this);
        }

        private void Duplicate()
        {
            Undo.RecordObjects(new UnityEngine.Object[] { TimelineEditor, Track.Timeline }, string.Empty);
            GameObject duplicateTrack = (GameObject)Instantiate(Track.gameObject);
            duplicateTrack.name = Track.gameObject.name;
            Undo.SetTransformParent(duplicateTrack.transform, Track.Timeline.transform, string.Empty);
            Undo.RegisterCreatedObjectUndo(duplicateTrack, "duplicate Track");

            if (!SequenceEditor.Sequence.IsStopped)
                duplicateTrack.GetComponent<FTrack>().Init();
        }

        private void Delete()
        {
            SequenceEditor.DestroyEditor(this);
        }

        private void SelectAllEvents()
        {
            SelectEvents(new FrameRange(0, Track.Sequence.Length));
        }

        private void AddEventAtPoint(object userData)
        {
            int t = (int)userData;

            TryAddEvent(t);
        }

        private void AddEventFillGap(object userData)
        {
            int t = (int)userData;

            int newT = -1;

            List<FEvent> evts = Track.Events;
            for (int i = 0; i != evts.Count; ++i)
            {
                if (evts[i].FrameRange.ContainsExclusive(t))
                    return; // can't add
                if (evts[i].FrameRange.Start >= t)
                {
                    newT = i == 0 ? 0 : evts[i - 1].End;
                    break;
                }
            }

            if (newT == -1)
            {
                newT = evts.Count == 0 ? 0 : evts[evts.Count - 1].End;
            }

            TryAddEvent(newT);
        }

        public override FSequenceEditor SequenceEditor { get { return TimelineEditor != null ? TimelineEditor.SequenceEditor : null; } }

        private Type[] _fcEventTypes = null;

        public void ShowTrackMenu(FTrack track)
        {
            if (_fcEventTypes == null)
            {
                _fcEventTypes = new Type[0];
                Type[] allTypes = typeof(FEvent).Assembly.GetTypes();

                foreach (Type type in allTypes)
                {
                    if (type.IsSubclassOf(typeof(FEvent)) && !type.IsAbstract)
                    {
                        object[] attributes = type.GetCustomAttributes(typeof(FEventAttribute), false);
                        if (attributes.Length == 1)
                        {
                            ArrayUtility.Add<Type>(ref _fcEventTypes, type);
                        }
                    }
                }
            }

            GenericMenu menu = new GenericMenu();
            foreach (Type t in _fcEventTypes)
            {
                TimelineMenuData param = new TimelineMenuData();
                param.track = track; param.evtType = t;
                object[] attributes = t.GetCustomAttributes(typeof(FEventAttribute), false);
                menu.AddItem(new GUIContent(((FEventAttribute)attributes[0]).menu), false, AddEventToTrack, param);
            }
            menu.ShowAsContext();
        }

        private struct TimelineMenuData
        {
            public FTrack track;
            public Type evtType;
        }

        private void AddEventToTrack(object obj)
        {
            TimelineMenuData menuData = (TimelineMenuData)obj;
            GameObject go = new GameObject(menuData.evtType.ToString());
            FEvent evt = (FEvent)go.AddComponent(menuData.evtType);
            menuData.track.Add(evt);
            Debug.Log("yns  add menu ");
            SequenceEditor.Refresh();
        }

        public FrameRange GetValidRange(FEventEditor evtEditor)
        {
            int index = 0;
            for (; index < _eventEditors.Count; ++index)
            {
                if (_eventEditors[index] == evtEditor)
                {
                    break;
                }
            }

            FrameRange range = new FrameRange(0, SequenceEditor.Sequence.Length);

            if (index > 0)
            {
                range.Start = _eventEditors[index - 1].Evt.End + 1;
            }
            if (index < _eventEditors.Count - 1)
            {
                range.End = _eventEditors[index + 1].Evt.Start - 1;
            }

            return range;
        }

        public FEvent TryAddEvent(int t)
        {
            FEvent newEvt = null;
            if (Track.CanAddAt(t))
            {
                FEvent evtAfterT = Track.GetEventAfter(t);
                int newEventEndT;
                if (evtAfterT == null)
                    newEventEndT =Mathf.Clamp(t+10,0, SequenceEditor.Sequence.Length);
                else
                    newEventEndT = evtAfterT.Start;

                newEvt = FEvent.Create(Track.GetEventType(), new FrameRange(t, newEventEndT));

                Undo.RecordObject(Track, string.Empty);
                Undo.RegisterCreatedObjectUndo(newEvt.gameObject, "create Event");

                Track.Add(newEvt);
                Debug.Log("yns  add new " +t);
            }
            return newEvt;
        }

        public virtual void OnTrackChanged()
        {

        }

        public virtual void UpdateEventsEditor(int frame, float time)
        {
            //			Track.UpdateEventsEditor( frame, time );
        }
    }
}
