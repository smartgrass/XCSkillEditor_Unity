using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

using Flux;

namespace FluxEditor
{
	[Serializable]
	public class FContainerEditor : FEditorList<FTimelineEditor> {

		public const int CONTAINER_HEIGHT = 25;

		public FContainer Container { get { return (FContainer)Obj; } }

		private bool _isDragSelecting = false;
		private Vector2 _dragSelectingStartPos = Vector2.zero;

		private float _timelineHeaderWidth = 0;
		private float _pixelsPerFrame = 0;

		public void OnStop()
		{
			for( int i = 0; i != Editors.Count; ++i )
				Editors[i].OnStop();
		}

		public void UpdateTimelines( int frame, float time )
		{
			for( int i = 0; i != Editors.Count; ++i )
			{
				if( !Editors[i].Timeline.enabled )
					continue;
				Editors[i].UpdateTracks( frame, time );
			}
		}

		public override void Init( FObject obj, FEditor owner )
		{
			base.Init( obj, owner );

			Editors.Clear();

			List<FTimeline> timelines = Container.Timelines;
			
			for( int i = 0; i < timelines.Count; ++i )
			{
				FTimeline timeline = timelines[i];
				FTimelineEditor timelineEditor = SequenceEditor.GetEditor<FTimelineEditor>(timeline);
				timelineEditor.Init( timeline, this );
				Editors.Add( timelineEditor );
			}

			_icon = new GUIContent( FUtility.GetFluxTexture("Folder.png") );
		}

		public override FSequenceEditor SequenceEditor { get { return (FSequenceEditor)Owner; } }

		public override float HeaderHeight { get { return CONTAINER_HEIGHT; } }

		protected override string HeaderText { get { return Obj.name; } }

		protected override Color BackgroundColor { get { return Container.Color; } }

		protected override bool CanPaste( FObject obj )
		{
			// since Unity Objs can be "fake null"
			return obj != null && obj is FTimeline;
		}

		protected override void Paste( object obj )
		{
			if( !CanPaste(obj as FObject) )
				return;

			Undo.RecordObject( Container, string.Empty );

			FTimeline timeline = Instantiate<FTimeline>((FTimeline)obj);
			timeline.hideFlags = Container.hideFlags;
			Container.Add( timeline );
			Undo.RegisterCreatedObjectUndo( timeline.gameObject, "Paste Timeline " + ((FTimeline)obj).name );
		}

		protected override void Delete()
		{
			SequenceEditor.DestroyEditor( this );
		}

        protected override void PopulateContextMenu(GenericMenu menu)
        {
            base.PopulateContextMenu(menu);
            menu.AddItem(new GUIContent("AddTimeLineTrack"), false, AddTimeLineTrack);
        }

        public bool HasTimeline( FTimelineEditor timelineEditor )
		{
			foreach( FTimelineEditor t in Editors )
			{
				if( t == timelineEditor )
					return true;
			}

			return false;
		}

        // 增加一条FTimeline轨道
        private void AddTimeLineTrack()
        {
            GameObject gameObject = new GameObject();
            gameObject.transform.SetParent(Container.transform);
            gameObject.name = "TimelineTrack";
            FTimeline fTimeline =  gameObject.AddComponent<FTimeline>();
            fTimeline.SetOwner(gameObject.transform);
        }

		private void StartDragSelecting( Vector2 mousePos )
		{
			_isDragSelecting = true;
			_dragSelectingStartPos = mousePos;
		}

		private void StopDragSelecting( Vector2 mousePos )
		{
			if( !_isDragSelecting )
				return;

			_isDragSelecting = false;

			FrameRange selectedRange = new FrameRange();
			bool isSelectingTimelines;
			
			Rect selectionRect = GetDragSelectionRect( _dragSelectingStartPos, mousePos, out selectedRange, out isSelectingTimelines );
			
			if( !Event.current.shift && !Event.current.control )
				SequenceEditor.DeselectAll();

			for( int i = 0; i != Editors.Count; ++i )
			{
				Rect timelineRect = Editors[i].Rect;
				if( timelineRect.yMin >= selectionRect.yMax )
					break;
				
				if( timelineRect.yMax <= selectionRect.yMin )
					continue;
				
				for( int j = 0; j != Editors[i].Editors.Count; ++j )
				{
					Rect trackRect = Editors[i].Editors[j].Rect;
					
					if( trackRect.yMin >= selectionRect.yMax )
						break;
					
					if( trackRect.yMax <= selectionRect.yMin )
						continue;
					
					if( Event.current.control )
					{
						Editors[i].Editors[j].DeselectEvents( selectedRange );
					}
					else
					{
						Editors[i].Editors[j].SelectEvents( selectedRange );
					}
				}
			}
		}

		private void OnDragSelecting( Vector2 mousePos )
		{
			if( !_isDragSelecting )
				return;

			if( Event.current.shift )
			{
				EditorGUIUtility.AddCursorRect( Rect, MouseCursor.ArrowPlus );
			}
			else if( Event.current.control )
			{
				EditorGUIUtility.AddCursorRect( Rect, MouseCursor.ArrowMinus );
			}

			FrameRange selectedRange;
			bool isSelectingTimelines;

			Rect selectionRect = GetDragSelectionRect( _dragSelectingStartPos, mousePos, out selectedRange, out isSelectingTimelines );

			if( selectionRect.width == 0 )
				selectionRect.width = 1;

			GUI.color = FGUI.GetSelectionColor();
			GUI.DrawTexture( selectionRect, EditorGUIUtility.whiteTexture );

			GUI.color = Color.white;
		}

		private Rect GetDragSelectionRect( Vector2 rawStartPos, Vector2 rawEndPos, out FrameRange selectedRange, out bool isSelectingTimelines )
		{
			int startFrame = GetFrameForX( rawStartPos.x );
			int endFrame = GetFrameForX( rawEndPos.x );
			
			if( startFrame > endFrame )
			{
				int temp = startFrame;
				startFrame = endFrame;
				endFrame = temp;
			}
			
			selectedRange = new FrameRange(startFrame, endFrame);
			
			Rect rect = new Rect();
			
			Vector2 startPos = new Vector2( GetXForFrame( startFrame ), rawStartPos.y );
			Vector2 endPos = new Vector2( GetXForFrame( endFrame ), rawEndPos.y );
			
			bool isStartOnHeader;
			bool isEndOnHeader;
			
			FTimelineEditor startTimeline = GetTimelineEditor( startPos, out isStartOnHeader );
			
			isSelectingTimelines = isStartOnHeader;
			
			if( startTimeline != null )
			{
				FTrackEditor startTrack = startTimeline.GetTrackEditor( startPos );
				FTrackEditor endTrack;
				
				FTimelineEditor endTimeline = GetTimelineEditor( endPos, out isEndOnHeader );
				if( endTimeline == null )
				{
					endTimeline = startTimeline;
					isEndOnHeader = isStartOnHeader;
					endTrack = startTrack;
				}
				else
					endTrack = endTimeline.GetTrackEditor( endPos );
				
				float xStart = Mathf.Min( startPos.x, endPos.x );
				float width = Mathf.Abs( startPos.x - endPos.x );
				float yStart;
				float height;
				
				
				if( startPos.y <= endPos.y )
				{
					yStart = isStartOnHeader ? startTimeline.Rect.yMin : startTrack.Rect.yMin;
					height = (isStartOnHeader ? endTimeline.Rect.yMax : (isEndOnHeader ? endTimeline.Rect.yMin + FTimelineEditor.HEADER_HEIGHT : endTrack.Rect.yMax)) - yStart;
				}
				else
				{
					yStart = isStartOnHeader || isEndOnHeader ? endTimeline.Rect.yMin : endTrack.Rect.yMin;
					height = (isStartOnHeader ? startTimeline.Rect.yMax : startTrack.Rect.yMax) - yStart;
				}
				
				rect.x = xStart;
				rect.y = yStart;
				rect.width = width;
				rect.height = height;
			}
			
			return rect;
		}

		public float GetXForFrame( int frame )
		{
			return _timelineHeaderWidth + frame * _pixelsPerFrame;
		}
		
		public int GetFrameForX( float x )
		{
			return Mathf.RoundToInt( ((x - _timelineHeaderWidth) / _pixelsPerFrame) );
		}

		private FTimelineEditor GetTimelineEditor( Vector2 pos, out bool isOnHeader )
		{
			for( int i = 0; i != Editors.Count; ++i )
			{
				if( Editors[i].Rect.Contains( pos ) )
				{
					isOnHeader = Editors[i].Rect.yMin + FTimelineEditor.HEADER_HEIGHT > pos.y;

					return Editors[i];
				}
			}

			isOnHeader = false;
			return null;
		}
	}
}
