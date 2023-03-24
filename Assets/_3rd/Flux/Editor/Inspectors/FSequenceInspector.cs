using UnityEngine;
using UnityEditor;

using System.Collections.Generic;

using Flux;
using FluxEditor;

namespace FluxEditor
{
	[CustomEditor(typeof(FSequence))]
	public class FSequenceInspector : Editor {

		private FSequence _sequence;

		private bool _advancedInspector = false;

		private const string CHANGE_FRAME_RATE_TITLE = "Change Frame Rate?";
		private const string CHANGE_FRAME_RATE_MSG = "Changing Frame Rate may cause the Sequence to " +
			"drop certain events that are Frame Rate dependent " +
			"(e.g. Animations). Are you sure you want to change Frame Rate from {0} to {1}?";
		private const string CHANGE_FRAME_RATE_OK = "Change";
		private const string CHANGE_FRAME_RATE_CANCEL = "Cancel";

//		private SerializedProperty _timelineContainer = null;
//
//		private SerializedProperty _containers = null;

		private SerializedProperty _content = null;
		private SerializedProperty _onFinishedCallback = null;

		void OnEnable()
		{
			_sequence = (FSequence)target;

			_content = serializedObject.FindProperty("_content");
			_onFinishedCallback = serializedObject.FindProperty("_onFinishedCallback");
//			_timelineContainer = serializedObject.FindProperty("_timelineContainer");
//
//			_containers = serializedObject.FindProperty("_timelineContainers");
		}

		public override void OnInspectorGUI ()
		{
			base.OnInspectorGUI();

			Rect r = GUILayoutUtility.GetRect( EditorGUIUtility.currentViewWidth, EditorGUIUtility.singleLineHeight );

			r.width -= EditorGUIUtility.labelWidth;

			EditorGUI.PrefixLabel( r, new GUIContent( "Frame Rate" ) );

			r.width += EditorGUIUtility.labelWidth;

			EditorGUI.BeginChangeCheck();
			r.xMin += EditorGUIUtility.labelWidth;

			int frameRate = FGUI.FrameRatePopup( r, _sequence.FrameRate );

			if( EditorGUI.EndChangeCheck() )
			{
				if( frameRate == -1 )
				{
					FChangeFrameRateWindow.Show( new Vector2( r.xMin-EditorGUIUtility.labelWidth, r.yMax ), _sequence, FSequenceInspector.Rescale );
					EditorGUIUtility.ExitGUI();
				}
				else
					Rescale( _sequence, frameRate, true );
			}

			serializedObject.Update();

			EditorGUILayout.PropertyField( _onFinishedCallback );

			EditorGUILayout.Space();

			if( GUILayout.Button( "Open In Flux Editor" ) )
			{
				FSequenceEditorWindow.Open( _sequence );
			}

			EditorGUILayout.Space();

			if( GUILayout.Button( _advancedInspector ? "Normal Inspector" : "Advanced Inspector") )
				_advancedInspector = !_advancedInspector;

            //强制显示
			if( _advancedInspector ||true)
			{
//				serializedObject.Update();
				EditorGUILayout.PropertyField( _content );
//				serializedObject.ApplyModifiedProperties();

				bool showContent = (_sequence.Content.hideFlags & HideFlags.HideInHierarchy) == 0;
                showContent = true; //强制显示
				EditorGUI.BeginChangeCheck();
				showContent = EditorGUILayout.Toggle( "Show Content", showContent );
//				bool showTimelines = EditorGUILayout.Toggle( "Show Timelines", (_timelineContainer.objectReferenceValue.hideFlags & HideFlags.HideInHierarchy) == 0 );
				if( EditorGUI.EndChangeCheck() )
				{
					if( showContent )
					{
//						_timelineContainer.objectReferenceValue.hideFlags &= ~HideFlags.HideInHierarchy;
						_sequence.Content.transform.hideFlags &= ~HideFlags.HideInHierarchy;
//						for( int i = 0; i != _sequence.Containers.Count; ++i )
//							_sequence.Containers[i].transform.hideFlags &= ~HideFlags.HideInHierarchy;
					}
					else
					{
						_sequence.Content.transform.hideFlags |= HideFlags.HideInHierarchy;
//						_timelineContainer.objectReferenceValue.hideFlags |= HideFlags.HideInHierarchy;
//						for( int i = 0; i != _sequence.Containers.Count; ++i )
//							_sequence.Containers[i].transform.hideFlags |= HideFlags.HideInHierarchy;
					}

				}
			}
			serializedObject.ApplyModifiedProperties();
//			serializedObject.ApplyModifiedProperties();
//			if( GUILayout.Button("Play") )
//				_sequence.Play();
		}

		public static void Rescale( FSequence sequence, int frameRate, bool confirm )
		{
			if( sequence.FrameRate == frameRate )
				return;

			if( !confirm || sequence.IsEmpty() || EditorUtility.DisplayDialog( CHANGE_FRAME_RATE_TITLE, string.Format(CHANGE_FRAME_RATE_MSG, sequence.FrameRate, frameRate), CHANGE_FRAME_RATE_OK, CHANGE_FRAME_RATE_CANCEL ) )
			{
				Rescale( sequence, frameRate );
			}
		}

		public static void Rescale( FSequence sequence, int frameRate )
		{
			if( sequence.FrameRate == frameRate )
				return;

			float scaleFactor = (float)frameRate / sequence.FrameRate;

			Undo.RecordObject( sequence, "Change Frame Rate" );

			sequence.Length = Mathf.RoundToInt( sequence.Length * scaleFactor );
			sequence.FrameRate = frameRate;

			foreach( FContainer container in sequence.Containers )
			{
				foreach( FTimeline timeline in container.Timelines )
				{
					Rescale( timeline, scaleFactor );
				}
			}

			EditorUtility.SetDirty( sequence );
		}

		private static void Rescale( FTimeline timeline, float scaleFactor )
		{
			foreach( FTrack track in timeline.Tracks )
				Rescale( track, scaleFactor );
		}

		private static void Rescale( FTrack track, float scaleFactor )
		{
			List<FEvent> events = track.Events;
			foreach( FEvent evt in events )
				Rescale( evt, scaleFactor );
		}

		private static void Rescale( FEvent evt, float scaleFactor )
		{
			FrameRange newFrameRange = evt.FrameRange;
	        newFrameRange.Start = Mathf.RoundToInt( newFrameRange.Start * scaleFactor );
	        newFrameRange.End = Mathf.RoundToInt( newFrameRange.End * scaleFactor );

	        FUtility.Rescale( evt, newFrameRange );
		}

		public static bool IsMultipleOf( int a, int b )
		{
			return (b / a >= 1) && (b % a == 0);
		}
	}
}
