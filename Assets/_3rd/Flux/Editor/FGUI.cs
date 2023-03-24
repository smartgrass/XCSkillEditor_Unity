using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;

using Flux;

using EventType = UnityEngine.EventType;

namespace FluxEditor
{
	public class FGUI
	{
		public const float DEFAULT_PIXEL_PER_SEC = 30;
		
	//	public const float TIMELINE_SECOND_TICK_SIZE = 20;
	//	
	//	public const float TIMELINE_HALF_SECOND_TICK_SIZE = 10;
	//	
	//	public const float TIMELINE_TENTH_SECOND_TICK_SIZE = 5;

		public const float TIMELINE_SCRUBBER_TEXT_HEIGHT = 15;

		public const float TIMELINE_SCRUBBER_TICK_HEIGHT = 4;

		// height of the scrubber area / timer
		public const float TIMELINE_SCRUBBER_HEIGHT = TIMELINE_SCRUBBER_TEXT_HEIGHT + TIMELINE_SCRUBBER_TICK_HEIGHT + TIMELINE_SCRUBBER_TICK_HEIGHT;
		
		// base height of the timeline rows
		public const float TIMELINE_HEIGHT = 50;

		public static readonly Color ANIMATION_BLEND_COLOR_PRO = new Color( 0.8f, 0.8f, 0.8f, 0.2f );

		public static readonly Color ANIMATION_BLEND_COLOR = new Color( 0.8f, 0.8f, 0.8f, 0.2f );

		public static readonly Color LINE_COLOR_PRO = new Color( 0.8f, 0.8f, 0.8f, 0.4f );

		public static readonly Color LINE_COLOR = new Color( 0.2f, 0.2f, 0.2f, 0.8f );

		public static readonly Color TIMELINE_COLOR_PRO = new Color( 0.2f, 0.2f, 0.2f, 1f );

		public static readonly Color TIMELINE_COLOR = new Color(0.6f, 0.6f, 0.6f, 1f );

		public static readonly Color EVENT_COLOR_PRO = new Color( 0.24f, 0.36f, 0.60f, 0.8f );

		public static readonly Color EVENT_COLOR = new Color( 0.24f, 0.36f, 0.60f, 0.8f );

		public static readonly Color TEXT_COLOR_PRO = new Color( 1f, 1f, 1f, 1f );

		public static readonly Color TEXT_COLOR = new Color( 0.2f, 0.2f, 0.2f, 1f );

		public static readonly Color SELECTION_COLOR_PRO = new Color(0.25f, 0.372f, 0.588f, 0.5f);
		public static readonly Color SELECTION_COLOR = SELECTION_COLOR_PRO;

		public static readonly Color WINDOW_COLOR_PRO = new Color( 0.22f, 0.22f, 0.22f, 1f );
		public static readonly Color WINDOW_COLOR = new Color( 0.76f, 0.76f, 0.76f, 1f );

		public static float CalcTimelineSize( float length, float zoomLevel )
		{
			return length * zoomLevel * DEFAULT_PIXEL_PER_SEC;
		}

		public const int MIN_PIXELS_BETWEEN_FRAMES = 100;

		private static GUIStyle _trackHeaderStyle = null;

		public static GUIStyle GetTrackHeaderStyle() 
		{
			if( _trackHeaderStyle == null )
			{
				_trackHeaderStyle = new GUIStyle( EditorStyles.label );
				_trackHeaderStyle.stretchWidth = false;
			}
			return _trackHeaderStyle;
		}

		private static GUIStyle _timelineHeaderStyle = null;

		public static GUIStyle GetTimelineHeaderStyle() 
		{
			if( _timelineHeaderStyle == null )
			{
				_timelineHeaderStyle = new GUIStyle( EditorStyles.boldLabel );
				_timelineHeaderStyle.fontStyle = FontStyle.Bold;
				_timelineHeaderStyle.stretchWidth = false;
			}
			return _timelineHeaderStyle;
		}

		public static int TimeScrubber( Rect rect, int t, int frameRate, FrameRange range )
		{	
	//		Rect actualRect = rect;
	//		actualRect.xMax -= 20; // buffer on the right

			Rect clickRect = rect;
			clickRect.yMin = clickRect.yMax - TIMELINE_SCRUBBER_HEIGHT;

			int length = range.Length;

			int controlId = EditorGUIUtility.GetControlID( FocusType.Passive );

			switch( Event.current.type )
			{
			case EventType.Repaint:
				int frames = range.Start;

				float width = rect.width;

				int maxFramesBetweenSteps = Mathf.Max( 1, Mathf.FloorToInt( width / MIN_PIXELS_BETWEEN_FRAMES ) );

				int numFramesPerStep = Mathf.Max( 1, length / maxFramesBetweenSteps);

				// multiple of 60 fps?
				if( numFramesPerStep < 30 )
				{
					if( numFramesPerStep <= 12 )
					{
						if( numFramesPerStep != 5 )
						{
							if( 12 % numFramesPerStep != 0 )
							{
								numFramesPerStep = 12;
							}
						}
					}
					else
					{
						numFramesPerStep = 30;
					}
				}
				else if( numFramesPerStep < 60 )
				{
					numFramesPerStep = 60;
				}
				else
				{
					int multiplesOf60 = numFramesPerStep / 60;
					numFramesPerStep = (multiplesOf60+1) * 60;
				}

				int numFramesIter = numFramesPerStep < 30 ? 1 : numFramesPerStep / 10;

				Vector3 pt = new Vector3(rect.x, rect.yMax-TIMELINE_SCRUBBER_TEXT_HEIGHT, 0);
			
				Rect backgroundRect = clickRect;
				backgroundRect.xMin = 0;
				backgroundRect.xMax += FSequenceEditor.RIGHT_BORDER;

//				GUI.color = new Color( 0.15f, 0.15f, 0.15f, 1f );//FGUI.GetTimelineColor();
//				GUI.DrawTexture( backgroundRect, EditorGUIUtility.whiteTexture );
//				GUI.color = Color.white;
				GUI.color = GetTextColor(); // a little darker than it is originally to stand out
				GUI.Label( backgroundRect, GUIContent.none, FGUI.GetTimeScrubberStyle() );

				Handles.color = GetLineColor();

	//			int framesBetweenSteps = maxFramesBetweenSteps / (maxFramesBetweenSteps * 10 / MIN_PIXELS_BETWEEN_FRAMES);
	//			float pixelsBetweenSteps = minPixelsBetweenSteps / framesBetweenSteps;
	//
	//			Debug.Log ( maxFramesBetweenSteps + " " + minPixelsBetweenSteps + " vs " + framesBetweenSteps + " " + pixelsBetweenSteps );

				GUIStyle labelStyle = new GUIStyle(EditorStyles.boldLabel);
				labelStyle.normal.textColor = Color.white;
				labelStyle.alignment = TextAnchor.UpperCenter;

				GUI.contentColor = FGUI.GetLineColor();

				frames = (frames / numFramesIter)*numFramesIter;
				while( frames <= range.End )
				{
					pt.x = rect.x + (width * ((float)(frames -  range.Start)/length));

					if( pt.x >= rect.x )
					{
						if( frames % numFramesPerStep == 0 )
						{
							Handles.DrawLine( pt, pt - new Vector3( 0, rect.height-TIMELINE_SCRUBBER_TEXT_HEIGHT, 0 ) );

							GUI.Label( new Rect( pt.x-30, pt.y, 60, TIMELINE_SCRUBBER_TEXT_HEIGHT ), FUtility.GetTime( frames, frameRate ), labelStyle );
						}
						else
						{
							Vector3 smallTickPt = pt;
							smallTickPt.y -= TIMELINE_SCRUBBER_TICK_HEIGHT;
							Handles.DrawLine( smallTickPt, smallTickPt - new Vector3( 0, TIMELINE_SCRUBBER_TICK_HEIGHT, 0 ) );
						}
					}

					frames += numFramesIter;
				}

				if( t >= 0 && range.Contains(t) )
				{
					Vector3 tStart = new Vector3( rect.x + (width * ((float)(t -  range.Start)/length)), rect.yMin, 0 );
					Vector3 tEnd = tStart;
					tEnd.y = rect.yMax-TIMELINE_SCRUBBER_TEXT_HEIGHT;

					Handles.color = Color.red;
					Handles.DrawLine( tStart, tEnd );

					GUI.contentColor = Color.red;
					GUI.Label( new Rect( tEnd.x-30, tEnd.y, 60, TIMELINE_SCRUBBER_TEXT_HEIGHT ), FUtility.GetTime(t, frameRate), labelStyle );
					GUI.contentColor = FGUI.GetTextColor();
				}

				GUI.color = Color.white;
				GUI.contentColor = Color.white;

				Handles.color = GetLineColor();

				Handles.DrawLine( new Vector3( rect.x, rect.yMin, 0 ), new Vector3( rect.x, rect.yMax-TIMELINE_SCRUBBER_HEIGHT, 0 ) );

				break;

			case EventType.MouseDown:
				if( EditorGUIUtility.hotControl == 0 && clickRect.Contains( Event.current.mousePosition ) )
				{
					EditorGUIUtility.hotControl = controlId;
				}
				goto case EventType.MouseDrag;
			case EventType.MouseDrag:
				if( EditorGUIUtility.hotControl == controlId )
				{
					Rect touchRect = rect;
					touchRect.yMin = touchRect.yMax - TIMELINE_SCRUBBER_HEIGHT;
		//			if( touchRect.Contains( Event.current.mousePosition ) )
					{
		//				Debug.Log( (Event.current.mousePosition.x - touchRect.xMin /
						t = Mathf.Clamp( range.Start + Mathf.RoundToInt(((Event.current.mousePosition.x-touchRect.xMin) / touchRect.width)*range.Length), range.Start, range.End );
						Event.current.Use();
					}
				}
				break;
			case EventType.MouseUp:
				if( EditorGUIUtility.hotControl == controlId )
				{
					EditorGUIUtility.hotControl = 0;
					Event.current.Use();
				}
				break;
			}
			rect.height = TIMELINE_SCRUBBER_HEIGHT;

			return t;
		}
		
		public static Vector2 BeginTimeline( Rect rect, Vector2 scrollPos )
		{

	//		scrollPos = GUI.BeginScrollView( rect, scrollPos, viewRect );
	//		GUI.BeginScrollView( rect, Vector2.zero, viewRect );
			rect.yMin -= scrollPos.y;
	//		rect.x -= 16;
			GUI.BeginGroup( rect );
	//		GUI.matrix = Matrix4x4.TRS( new Vector3(0, -scrollPos.y, 0), Quaternion.identity, Vector3.one );
			return scrollPos;
		}
		
		public static void EndTimeline( bool handleScrollWheel )
		{
	//		GUI.matrix = Matrix4x4.identity;
	//		GUI.EndScrollView( handleScrollWheel );
			GUI.EndGroup();
		}

		private static Vector3 _offset;

		public static FrameRange ViewRangeBar( Rect rect, FrameRange viewRange, int totalFrames )
		{
			GUISkin previousGUI = GUI.skin;
			GUI.skin = null;

			int leftArrowId = EditorGUIUtility.GetControlID(FocusType.Passive);
			int rightArrowId = EditorGUIUtility.GetControlID(FocusType.Passive);

			int leftHandleId = EditorGUIUtility.GetControlID(FocusType.Passive);
			int rightHandleId = EditorGUIUtility.GetControlID(FocusType.Passive);

			int midHandleId = EditorGUIUtility.GetControlID(FocusType.Passive);

			GUIStyle leftArrowStyle = GUI.skin.GetStyle("horizontalscrollbarleftbutton");
			GUIStyle rightArrowStyle = GUI.skin.GetStyle("horizontalscrollbarrightbutton");

			GUIStyle timelineScrubberStyle = GUI.skin.GetStyle("HorizontalMinMaxScrollbarThumb");

			Rect leftArrowRect = rect;
			leftArrowRect.width = leftArrowStyle.fixedWidth;

			Rect rightArrowRect = rect;
			rightArrowRect.xMin = rightArrowRect.xMax - rightArrowStyle.fixedWidth;

			Rect scrubberRect = rect;
			scrubberRect.xMin += leftArrowRect.width;
			scrubberRect.xMax -= rightArrowRect.width;

			float scrubberLeftEdge = scrubberRect.xMin;

			float minWidth = 50;
			float maxWith = scrubberRect.width-minWidth;

			scrubberRect.xMin += ((float)viewRange.Start / totalFrames)*maxWith;
			scrubberRect.width = ((float)viewRange.Length / totalFrames)*maxWith+minWidth;

			Rect leftHandleRect = scrubberRect;
			leftHandleRect.width = timelineScrubberStyle.border.left;

			Rect rightHandRect = scrubberRect;
			rightHandRect.xMin = rightHandRect.xMax - timelineScrubberStyle.border.right;

			switch( Event.current.type )
			{
			case EventType.MouseDown:

				if( Event.current.clickCount > 1 )
				{
					viewRange.Start = 0;
					viewRange.End = totalFrames;
				}
				else if( EditorGUIUtility.hotControl == 0 )
				{
					Vector3 mousePos = Event.current.mousePosition;
					if( leftArrowRect.Contains( mousePos ) )
					{
						EditorGUIUtility.hotControl = leftArrowId;
					}
					else if( rightArrowRect.Contains( mousePos ) )
					{
						EditorGUIUtility.hotControl = rightArrowId;
					}
					else if( leftHandleRect.Contains( mousePos ) )
					{
						EditorGUIUtility.hotControl = leftHandleId;
					}
					else if( rightHandRect.Contains( mousePos ) )
					{
						EditorGUIUtility.hotControl = rightHandleId;
					}
					else if( scrubberRect.Contains( mousePos ) )
					{
						EditorGUIUtility.hotControl = midHandleId;
						_offset = Event.current.mousePosition - new Vector2( scrubberRect.xMin, scrubberRect.yMin);
					}

					if( EditorGUIUtility.hotControl != 0 )
					{
	//					cachedMousePos = Input.mousePosition;
						Event.current.Use();
					}
				}
				break;
			case EventType.MouseUp:
				if( EditorGUIUtility.hotControl == leftArrowId || EditorGUIUtility.hotControl == rightArrowId 
				   || EditorGUIUtility.hotControl == leftHandleId || EditorGUIUtility.hotControl == rightHandleId 
				   || EditorGUIUtility.hotControl == midHandleId )
				{
					EditorGUIUtility.hotControl = 0;
					Event.current.Use();
				}
				break;
			case EventType.MouseDrag:
				int delta = 0;

				if( EditorGUIUtility.hotControl == leftHandleId )
				{
					delta = Mathf.RoundToInt(((Event.current.mousePosition.x-scrubberLeftEdge)/maxWith)*totalFrames);
					viewRange.Start = Mathf.Clamp( delta, 0, viewRange.End-1 );
					GUI.changed = true;
					Event.current.Use();
				}
				else if( EditorGUIUtility.hotControl == rightHandleId )
				{
					delta = Mathf.RoundToInt(((Event.current.mousePosition.x-(scrubberLeftEdge+minWidth))/maxWith)*totalFrames);
					viewRange.End = Mathf.Clamp( delta, viewRange.Start+1, totalFrames );
					GUI.changed = true;
					Event.current.Use();
				}
				else if( EditorGUIUtility.hotControl == midHandleId )
				{
	//				int startX = Mathf.RoundToInt(((cachedMousePos.x-scrubberLeftEdge)/maxWith)*totalFrames);
					delta = Mathf.RoundToInt(((Event.current.mousePosition.x-_offset.x-scrubberLeftEdge)/maxWith)*totalFrames);
					int x = delta-viewRange.Start;
					viewRange.Start = delta;
					viewRange.End += x;
					if( viewRange.Start < 0 )
					{
						int diff = -viewRange.Start;
						viewRange.Start += diff;
						viewRange.End += diff;
					}
					if( viewRange.End > totalFrames )
					{
						int diff = viewRange.End-totalFrames;
						viewRange.Start -= diff;
						viewRange.End -= diff;
					}
					GUI.changed = true;
					Event.current.Use();
				}
				break;

			case EventType.Repaint:

				GUIStyle bar = GUI.skin.GetStyle("horizontalscrollbar");

				bar.Draw( rect, false, false, false, false );

				leftArrowStyle.Draw( leftArrowRect, true, EditorGUIUtility.hotControl == leftArrowId, false, false );

				rightArrowStyle.Draw( rightArrowRect, true, EditorGUIUtility.hotControl == rightArrowId, false, false );

				timelineScrubberStyle.Draw( scrubberRect, false, false, false, false );

				GUIStyle label = new GUIStyle( EditorStyles.boldLabel );

				string viewRangeStartStr = viewRange.Start.ToString();
				string viewRangeEndStr = viewRange.End.ToString();
				string viewRangeLengthStr = viewRange.Length.ToString();

				scrubberRect.xMin += 20;
				scrubberRect.xMax -= 20;
				Vector2 sizeAllLabels = label.CalcSize( new GUIContent(viewRangeStartStr+viewRangeEndStr+viewRangeLengthStr ) );

				if( scrubberRect.width > sizeAllLabels.x + 40 )
				{
					label.Draw( scrubberRect, new GUIContent(viewRangeStartStr), 0 );

					label.alignment = TextAnchor.MiddleRight;
					label.Draw( scrubberRect, new GUIContent(viewRangeEndStr), 0 );
				}

				label.alignment = TextAnchor.MiddleCenter;
				label.Draw( scrubberRect, new GUIContent(viewRangeLengthStr), 0 );

	//			GUI.HorizontalScrollbar( rect, 0, 100, 0, 100);

				break;
			}

			GUI.skin = previousGUI;

			return viewRange;
		}


		private static readonly int[] DEFAULT_FRAME_RATE_VALUES = new int[]{ 15, 30, 60 };
		private const string CUSTOM_FRAME_RATE_STR = "Other..";

		private static int[] FRAME_RATE_VALUES = null;
		private static string[] FRAME_RATE_OPTIONS = null;

		public static int FrameRatePopup( Rect rect, int frameRate )
		{
			int i = 0;

			if( FRAME_RATE_VALUES == null )
			{
				FRAME_RATE_VALUES = new int[DEFAULT_FRAME_RATE_VALUES.Length+1];
				FRAME_RATE_OPTIONS = new string[FRAME_RATE_VALUES.Length];

				for( ; i != DEFAULT_FRAME_RATE_VALUES.Length; ++i )
				{
					FRAME_RATE_VALUES[i] = DEFAULT_FRAME_RATE_VALUES[i];
					FRAME_RATE_OPTIONS[i] = FRAME_RATE_VALUES[i].ToString();
				}
				FRAME_RATE_VALUES[i] = -1;
				FRAME_RATE_OPTIONS[i] = CUSTOM_FRAME_RATE_STR;

				i = 0; // clear i for the next cycle
			}

			for( ; i != DEFAULT_FRAME_RATE_VALUES.Length; ++i )
			{
				if( frameRate == DEFAULT_FRAME_RATE_VALUES[i] )
					break;
			}

			// didn't find it, add it to the one before last
			if( i == DEFAULT_FRAME_RATE_VALUES.Length )
			{
				if( FRAME_RATE_VALUES.Length == DEFAULT_FRAME_RATE_VALUES.Length+1 ) // doesn't contain a custom value
				{
					ArrayUtility.Insert<int>( ref FRAME_RATE_VALUES, FRAME_RATE_VALUES.Length-1, frameRate );
					ArrayUtility.Insert<string>( ref FRAME_RATE_OPTIONS, FRAME_RATE_OPTIONS.Length-1, frameRate.ToString() );
				}
				else if( FRAME_RATE_VALUES[FRAME_RATE_VALUES.Length-2] != frameRate ) // already contains, lets see if different
				{
					FRAME_RATE_VALUES[FRAME_RATE_VALUES.Length-2] = frameRate;
					FRAME_RATE_OPTIONS[FRAME_RATE_OPTIONS.Length-2] = frameRate.ToString();
				}
			}

	//		if( GUI.Button( rect, frameRate.ToString(), EditorStyles.popup ) )
	//		{
	//			GenericMenu frameRateMenu = new GenericMenu();
	//			for( i = 0; i != FRAME_RATE_VALUES.Length; ++i )
	//				frameRateMenu.AddItem( new GUIContent(FRAME_RATE_OPTIONS[i]), false, ChangeFrameRateFunc, FRAME_RATE_VALUES[i] );
	////			frameRateMenu.ShowAsContext();
	//			frameRateMenu.DropDown( rect );
	//		}
			return EditorGUI.IntPopup( rect, frameRate, FRAME_RATE_OPTIONS, FRAME_RATE_VALUES );
		}

		private static void ChangeFrameRateFunc( object obj )
		{
			Debug.Log( (int)obj + " chosen"  );
		}

		public static Color GetAnimationBlendingColor()
		{
			return EditorGUIUtility.isProSkin ? ANIMATION_BLEND_COLOR_PRO : ANIMATION_BLEND_COLOR;
		}

		public static Color GetLineColor()
		{
			return EditorGUIUtility.isProSkin ? LINE_COLOR_PRO : LINE_COLOR;
		}

		public static Color GetTimelineColor()
		{
			return EditorGUIUtility.isProSkin ? TIMELINE_COLOR_PRO : TIMELINE_COLOR;
		}

		public static Color GetEventColor()
		{
			return EditorGUIUtility.isProSkin ? EVENT_COLOR_PRO : EVENT_COLOR;
		}

		public static Color GetTextColor()
		{
			return EditorGUIUtility.isProSkin ? TEXT_COLOR_PRO : TEXT_COLOR;
		}

		public static Color GetSelectionColor()
		{
			return EditorGUIUtility.isProSkin ? SELECTION_COLOR_PRO : SELECTION_COLOR;
		}

		public static Color GetIconColor()
		{
//			Color c = EditorGUIUtility.isProSkin ? TEXT_COLOR_PRO : TEXT_COLOR;
			Color c = TEXT_COLOR_PRO;
			if( Application.isPlaying )
				c.a *= 0.5f;
			return c;
		}

		public static Color GetWindowColor()
		{
			return EditorGUIUtility.isProSkin ? WINDOW_COLOR_PRO : WINDOW_COLOR;
		}

		public static Color DefaultContainerColor()
		{
			return FContainer.DEFAULT_COLOR;
		}

		public static GUIStyle GetTimeScrubberStyle()
		{
			if( EditorGUIUtility.isProSkin )
			{
				GUIStyle toolbarStyle = new GUIStyle(EditorStyles.toolbar);
				toolbarStyle.fixedHeight = 0;
				return toolbarStyle;
			}

			return GUIStyle.none;
		}

		public static bool ButtonLogic( Rect rect )
		{
			int buttonGuid = EditorGUIUtility.GetControlID( FocusType.Passive );

			bool result = false;

			switch( Event.current.type )
			{
			case EventType.MouseDown:
				if( rect.Contains(Event.current.mousePosition) )
				{
					GUIUtility.hotControl = buttonGuid;

					Event.current.Use();
				}
				break;
			case EventType.MouseUp:
				if( GUIUtility.hotControl == buttonGuid )
				{
					if( rect.Contains(Event.current.mousePosition) )
					{
						result = true;
						Event.current.Use();
					}
					GUIUtility.hotControl = 0;
				}
				break;
			}

			return result;
		}
			
		public static bool Button( Rect rect, GUIContent content )
		{
			bool result = false;

			GUIStyle style = new GUIStyle();

			Color prevColor = GUI.color;

			if( Event.current.type == EventType.Repaint && GUIUtility.hotControl != 0 && rect.Contains(Event.current.mousePosition) )
			{
				style.padding = new RectOffset(1, 1, 1, 1);
				Color pressedColor = prevColor;
				pressedColor.a = 0.5f;
				GUI.color = pressedColor;
			}

			result = GUI.Button(rect, content, style);

			if( Event.current.type == EventType.Repaint )
				GUI.color = prevColor;
								
			return result;
		}


//		private static string _smartFieldText = "";

//		public static int SmartIntField( Rect rect, int value )
//		{
//			
////			int id = EditorGUIUtility.GetControlID(FocusType.Keyboard);
////
////			string text = EditorGUIUtility.keyboardControl == id ? _smartFieldText : (value == int.MinValue ? "--" : value.ToString());
//
//			GUIStyle textFieldStyle = EditorStyles.textField;
//
//			EditorGUI.BeginChangeCheck();
//
//			TextEditor editor = GUIUtility.GetStateObject( typeof(TextEditor), EditorGUIUtility.keyboardControl );
//
//			if( editor.con
//
//			if( EditorGUI.EndChangeCheck() )
//			{
//
//			}
//		}
	}
}
