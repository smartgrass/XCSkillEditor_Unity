using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using System.Collections.Generic;

using Flux;

namespace FluxEditor
{
	public abstract class FEditorList<T> : FEditor where T : FEditor {

		private List<T> _editors = new List<T>();
		public List<T> Editors { get { return _editors; } }

		private AnimFloat _showPercentage = new AnimFloat(1);
		public float ShowPercentage { get { return _showPercentage.value; } }

		public bool ShowEditorList { 
			get { return _showPercentage.target > 0; } 
			set { _showPercentage.target = value ? 1 : 0; }
		}

		public override void ClearOffset()
		{
			base.ClearOffset();
			foreach( T editor in Editors )
			{
				editor.ClearOffset();
			}
		}

		public virtual float HeaderHeight { get { return 0; } } 

		private Rect _headerRect = new Rect();

		public Rect HeaderRect { get { return _headerRect; } set { _headerRect = value; } }

		private float _height = 0;
		public override float Height { get { return _height; } }

		protected GUIContent _icon = null;
		protected virtual Color BackgroundColor { get { return FGUI.GetTimelineColor(); } }

		protected abstract string HeaderText { get; }

		protected virtual float IconSize { get { return 16; } }

		protected virtual bool IconOnLeft { get { return true; } }

		protected virtual Transform ContentTransform { get { return Obj.transform; } }

		public override void ReserveGuiIds()
		{
			base.ReserveGuiIds();
			
			_height = HeaderHeight;

			if( ShowPercentage > 0 )
			{
				float contentHeight = 0;
				for( int i = 0; i != Editors.Count; ++i )
				{
					Editors[i].ReserveGuiIds();
					contentHeight += Editors[i].Height;
				}
				_height += contentHeight * ShowPercentage;
			}
		}

		public override void Init( FObject obj, FEditor owner )
		{
			base.Init( obj, owner );
			_showPercentage.valueChanged.RemoveAllListeners();
			_showPercentage.valueChanged.AddListener( SequenceEditor.Repaint );

			_contentOffset = new Vector2( 8, HeaderHeight );
		}

		public override void Render( Rect rect, float headerWidth )
		{
			Rect = rect;

			Rect headerRect = rect;
			headerRect.height = HeaderHeight;
			RenderHeader( headerRect, headerWidth );

			Rect headerTimelineRect = rect;
			headerTimelineRect.xMin += headerWidth;
			if( headerTimelineRect.Contains( Event.current.mousePosition ) )
				SequenceEditor.SetMouseHover( Event.current.mousePosition.x - headerWidth, this );

			headerRect.width = headerWidth;
			HeaderRect = headerRect;

			Rect listRect = rect;
			listRect.yMin += HeaderHeight;

			_contentOffset.y = listRect.y;

			if( _showPercentage.value > 0 )
			{
				listRect.height *= _showPercentage.value;
				RenderList( listRect, headerWidth );
			}

			if( IsSelected && Event.current.type == EventType.Repaint )
			{
				float lineWidth = 3;
				float halfLineWidth = lineWidth * 0.33f;
				Color c = Handles.selectedColor;
				c.a = 1f;
				Handles.color = c;
				Vector3 pt0 = new Vector3( rect.xMin+halfLineWidth, rect.yMin+halfLineWidth );
				Vector3 pt1 = new Vector3( rect.xMin+halfLineWidth, rect.yMax-halfLineWidth );
				Vector3 pt2 = new Vector3( rect.xMax-halfLineWidth, rect.yMax-halfLineWidth );
				Vector3 pt3 = new Vector3( rect.xMax-halfLineWidth, rect.yMin+halfLineWidth );
//				Handles.DrawAAPolyLine( lineWidth, pt0, pt1, pt2, pt3, pt0 );
				Handles.DrawSolidRectangleWithOutline( new Vector3[]{ pt0, pt1, pt2, pt3 }, new Color( 0, 0, 0, 0 ), c );
				//				Handles.DrawPolyLine( pt0, pt1, pt2, pt3, pt0 );
			}

			Handles.color = FGUI.GetLineColor();
			Handles.DrawLine( Rect.min, Rect.min + new Vector2(Rect.width, 0) );
			Handles.DrawLine( Rect.max - new Vector2(Rect.width, 0), Rect.max );
		}

		protected virtual void RenderHeader( Rect headerRect, float headerWidth )
		{
			if( HeaderHeight == 0 )
				return;

			if( Event.current.type == EventType.Repaint )
			{
				GUI.color = BackgroundColor;
				GUI.DrawTexture( headerRect, EditorGUIUtility.whiteTexture );
				GUI.color = Color.white;
			}

			Rect foldoutRect = headerRect;
			foldoutRect.width = 16;

			GUI.backgroundColor = Color.white;
			EditorGUI.BeginChangeCheck();
			ShowEditorList = EditorGUI.Foldout( foldoutRect, ShowEditorList, string.Empty );
			if( EditorGUI.EndChangeCheck() )
			{
				if( Event.current.shift )
				{
					if( this is FContainerEditor )
						ShowAllContainers( ShowEditorList );
					else if( this is FTimelineEditor )
						ShowAllTimelines( ShowEditorList );
				}
			}

			Rect labelRect = headerRect;

			labelRect.width = headerWidth;
			labelRect.xMin += 16;

			Rect iconRect = labelRect;

			if( _icon != null )
			{
				if( IconOnLeft )
				{
					labelRect.xMin += IconSize;
				}
				else
				{
					labelRect.xMax -= IconSize;
					iconRect.xMin = labelRect.xMax;
				}

				iconRect.width = IconSize;
				iconRect.height = IconSize;
				
				GUI.color = FGUI.GetIconColor();
				GUI.DrawTexture( iconRect, _icon.image );
				GUI.color = Color.white;
				
			}

			GUI.contentColor = FGUI.GetTextColor();
			GUI.Label( labelRect, HeaderText, EditorStyles.boldLabel );

			OnHeaderInput( labelRect, iconRect );
		}

		protected virtual void OnHeaderInput( Rect labelRect, Rect iconRect )
		{
			if( Event.current.type == EventType.MouseDown && labelRect.Contains( Event.current.mousePosition ) )
			{
				if( Event.current.button == 0 )
				{
					if( Event.current.shift )
						SequenceEditor.Select( this );
					else
						SequenceEditor.SelectExclusive( this );
				} else if( Event.current.button == 1 )
					ShowHeaderContextMenu();
				Event.current.Use();
			}
		}

		protected virtual void ShowHeaderContextMenu()
		{
			GenericMenu menu = new GenericMenu();
			PopulateContextMenu(menu);
			if( CanPaste( FSequenceEditor.CopyObject ) )
			{
				menu.AddItem( new GUIContent( "Paste " + FSequenceEditor.CopyObject.name ), false, Paste, FSequenceEditor.CopyObject );
			}
			menu.AddItem( new GUIContent( "Copy" ), false, Copy );
			menu.AddItem( new GUIContent( "Cut" ), false, Cut );
			menu.AddItem( new GUIContent( "Duplicate" ), false, Duplicate );
			menu.AddItem( new GUIContent( "Delete" ), false, Delete );
			menu.ShowAsContext();
		}

		private void Copy()
		{
			SequenceEditor.CopyEditor( this );
		}

		private void Cut()
		{
			SequenceEditor.CutEditor( this );
		}

		protected abstract bool CanPaste(FObject obj);

		protected abstract void Paste( object obj );

		private void Duplicate()
		{
			GameObject go = GameObject.Instantiate( Obj.gameObject );
			go.transform.parent = Obj.transform.parent;
			go.name = SequenceEditor.GetUniqueContainerName( Obj.gameObject.name );
			Undo.RegisterCreatedObjectUndo( go, "duplicate " + Obj.gameObject.name );
		}
		
		protected abstract void Delete();

		protected virtual void PopulateContextMenu( GenericMenu menu )
		{

		}

		protected virtual void RenderList( Rect listRect, float headerWidth )
		{
			if( ContentOffset.x > 0 )
			{
				Rect sideRect = listRect;
				sideRect.width = ContentOffset.x;
				
				if( Event.current.type == EventType.Repaint )
				{
					GUI.color = BackgroundColor;
					GUI.DrawTexture( sideRect, EditorGUIUtility.whiteTexture );
					GUI.color = Color.white;
				}
			}

			listRect.xMin += ContentOffset.x;
			headerWidth -= ContentOffset.x;

			Rect editorRect = listRect;

			GUI.BeginGroup( listRect );
			editorRect.y = 0;
			editorRect.x = 0;
			editorRect.height = 0;

			for( int i = 0; i != _editors.Count; ++i )
			{
				editorRect.yMin = editorRect.yMax;
				editorRect.height = _editors[i].Height;

                if ( SequenceEditor.EditorDragged == _editors[i] )
				{
					Rect editorDraggedRect = editorRect;
					editorDraggedRect.y = - _editors[i].Offset.value.y;

					SequenceEditor.EditorDraggedRect = editorDraggedRect;

					SequenceEditor.Repaint();
				}
				else
				{
					if( SequenceEditor.EditorDragged != null && SequenceEditor.EditorDragged.Owner == this )
					{
						float mouseOffsetY = SequenceEditor.EditorDragged.Offset.value.y;
						float editorDraggedTop = Event.current.mousePosition.y - mouseOffsetY;
						float editorDraggedBot = editorDraggedTop + SequenceEditor.EditorDraggedRect.height;

						if( i < SequenceEditor.EditorDragged.Obj.GetId() )
						{
							if( _editors[i].Offset.target.y == 0 )
							{
								if( editorDraggedTop < editorRect.center.y )
									_editors[i].Offset.target = new Vector3(0, SequenceEditor.EditorDragged.Height, 0);
								else
									_editors[i].Offset.target = Vector3.zero;
							}
							else
							{
								if( editorDraggedBot > editorRect.center.y + SequenceEditor.EditorDragged.Height )
									_editors[i].Offset.target = Vector3.zero;
								else
									_editors[i].Offset.target = new Vector3(0, SequenceEditor.EditorDragged.Height, 0);
							}
							
						}
						else if( i > SequenceEditor.EditorDragged.Obj.GetId() )
						{
							if( _editors[i].Offset.target.y == 0 )
							{
								if( editorDraggedBot > editorRect.center.y )
									_editors[i].Offset.target = new Vector3(0, -SequenceEditor.EditorDragged.Height, 0);
								else
									_editors[i].Offset.target = Vector3.zero;
							}
							else
							{
								if( editorDraggedTop < editorRect.center.y - SequenceEditor.EditorDragged.Height )
									_editors[i].Offset.target = Vector3.zero;
								else
									_editors[i].Offset.target = new Vector3(0, -SequenceEditor.EditorDragged.Height, 0);
							}
						}
					}

					Rect editorRenderRect = editorRect;
					editorRenderRect.y += _editors[i].Offset.value.y;

					_editors[i].Render( editorRenderRect, headerWidth );

					Rect editorHeaderRect = editorRect;
					editorHeaderRect.width = headerWidth;

					if( EditorGUIUtility.hotControl == 0 && Event.current.type == EventType.MouseDrag && Event.current.button == 0 && editorHeaderRect.Contains( Event.current.mousePosition ) )
					{
						SequenceEditor.StartDraggingEditor( _editors[i] );
						if( EditorGUIUtility.hotControl == _editors[i].GuiId ) // make sure we're the one being dragged
							_editors[i].Offset.target = _editors[i].Offset.value = new Vector3( 0, Event.current.mousePosition.y - editorRect.y, 0 );
					}

				}
			}
			
			GUI.EndGroup();
		}

		private void ShowAllContainers( bool show )
		{
			foreach( FContainerEditor containerEditor in SequenceEditor.Editors )
			{
				containerEditor.ShowEditorList = show;
			}
		}

		private void ShowAllTimelines( bool show )
		{
			foreach( FContainerEditor containerEditor in SequenceEditor.Editors )
			{
				foreach( FTimelineEditor timelineEditor in containerEditor.Editors )
				{
					timelineEditor.ShowEditorList = show;
				}
			}
		}

		public void UpdateListFromOffsets()
		{
			if( SequenceEditor.EditorDragged == null ) return;

			int currentIndex = SequenceEditor.EditorDragged.Obj.GetId();
			int newIndex = currentIndex;

			for( int i = 0; i != Editors.Count; ++i )
			{
				if( i == currentIndex )
					continue;

				if( _editors[i].Offset.target.y > 0 )
					--newIndex;
				else if( _editors[i].Offset.target.y < 0 )
					++newIndex;
			}

			if( newIndex != currentIndex )
			{
				string undoText = "reorder";

				Undo.SetTransformParent( SequenceEditor.EditorDragged.Obj.transform, ContentTransform, undoText );
				SequenceEditor.EditorDragged.Obj.transform.SetSiblingIndex( newIndex );
				
				Editors.RemoveAt(currentIndex);
				Editors.Insert( newIndex, (T)SequenceEditor.EditorDragged );
			}
		}
	}
}
