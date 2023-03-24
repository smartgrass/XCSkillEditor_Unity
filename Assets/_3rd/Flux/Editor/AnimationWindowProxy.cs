using UnityEngine;
using UnityEditor;

using System;
using System.Reflection;

namespace FluxEditor
{
	public class AnimationWindowProxy {

		private static Type ANIMATION_WINDOW_TYPE = typeof(EditorWindow).Assembly.GetType("UnityEditor.AnimationWindow");
		private static Type ANIMATION_WINDOW_STATE_TYPE = typeof(EditorWindow).Assembly.GetType("UnityEditorInternal.AnimationWindowState");
		private static Type ANIMATION_SELECTION_TYPE = typeof(EditorWindow).Assembly.GetType("UnityEditor.AnimationSelection");

		private static Type ANIMATION_EDITOR_TYPE = typeof(EditorWindow).Assembly.GetType("UnityEditor.AnimEditor");
		private static Type ANIMATION_WINDOW_SELECTED_ITEM_TYPE = typeof(EditorWindow).Assembly.GetType("UnityEditorInternal.AnimationWindowSelectionItem");

		private static EditorWindow _animationWindow = null;
		public static EditorWindow AnimationWindow {
			get	{
				if( _animationWindow == null )
					_animationWindow = FUtility.GetWindowIfExists( ANIMATION_WINDOW_TYPE );
				return _animationWindow;
			}
		}

		public static EditorWindow OpenAnimationWindow()
		{
			if( _animationWindow == null )
				_animationWindow = EditorWindow.GetWindow( ANIMATION_WINDOW_TYPE );
			return _animationWindow;
		}

		#region AnimationWindow variables

		private static FieldInfo _animEditorField = null;
		private static FieldInfo AnimEditorField {
			get {
				if( _animEditorField == null )
					_animEditorField = ANIMATION_WINDOW_TYPE.GetField( "m_AnimEditor", BindingFlags.Instance | BindingFlags.NonPublic );
				return _animEditorField;
			}
		}

		private static ScriptableObject _animEditor = null;
		private static ScriptableObject AnimEditor {
			get {
				if( _animEditor == null )
					_animEditor = (ScriptableObject)AnimEditorField.GetValue( AnimationWindow );
				return _animEditor;
			}
		}

		private static FieldInfo _stateField = null;
		private static FieldInfo StateField {
			get {
				if( _stateField == null )
					_stateField = ANIMATION_EDITOR_TYPE.GetField("m_State", BindingFlags.Instance | BindingFlags.NonPublic);
				return _stateField;
			}

		}


		private static PropertyInfo _selectedItemProperty = null;
		private static PropertyInfo SelectedItemProperty {
			get{
				if( _selectedItemProperty == null )
					_selectedItemProperty = ANIMATION_EDITOR_TYPE.GetProperty("selectedItem", BindingFlags.Instance | BindingFlags.Public);
				return _selectedItemProperty;
			}
		}

		private static PropertyInfo _animationClipProperty = null;
		private static PropertyInfo AnimationClipProperty {
			get {
				if( _animationClipProperty == null )
					_animationClipProperty = ANIMATION_WINDOW_SELECTED_ITEM_TYPE.GetProperty("animationClip", BindingFlags.Instance | BindingFlags.Public);
				return _animationClipProperty;
			}
		}

		#endregion

		#region AnimationWindowState variables


		private static PropertyInfo _currentTimeProperty = null;
		private static PropertyInfo CurrentTimeProperty {
			get {
				if( _currentTimeProperty == null )
					_currentTimeProperty = ANIMATION_WINDOW_STATE_TYPE.GetProperty("currentTime", BindingFlags.Instance | BindingFlags.Public);
				return _currentTimeProperty;
			}
		}
		
		private static PropertyInfo _frameProperty = null;
		private static PropertyInfo FrameProperty {
			get {
				if( _frameProperty == null )
					_frameProperty = ANIMATION_WINDOW_STATE_TYPE.GetProperty("currentFrame", BindingFlags.Instance | BindingFlags.Public);
				return _frameProperty;
			}
		}

		private static PropertyInfo _recordingProperty = null;
		private static PropertyInfo RecordingProperty {
			get {
				if( _recordingProperty == null )
					_recordingProperty = ANIMATION_WINDOW_STATE_TYPE.GetProperty("recording", BindingFlags.Instance | BindingFlags.Public);
				return _recordingProperty;
			}
		}

		#endregion

		#region AnimationSelection variables
		private static MethodInfo _chooseClipMethod = null;
		private static MethodInfo ChooseClipMethod {
			get {
				if( _chooseClipMethod == null )
					_chooseClipMethod = ANIMATION_SELECTION_TYPE.GetMethod( "ChooseClip", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[]{typeof(AnimationClip)}, null );
				return _chooseClipMethod;
			}
		}
		#endregion

		public static void StartAnimationMode()
		{
			//RecordingProperty.SetValue( GetState(), true, null );
            AnimationMode.StartAnimationMode();
		}

		private static object GetState()
		{
			return StateField.GetValue( AnimEditor );
		}

		private static object GetSelectedItem()
		{
			return SelectedItemProperty.GetValue(AnimEditor, null);
		}

		public static void SetCurrentFrame( int frame, float time )
		{
			if( AnimationWindow == null )
				return;

			object state = GetState();

			// pass a little nudge because of floating point errors which will make it go
			// to a lower frame instead of the frame we want
			CurrentTimeProperty.SetValue( state, time+0.0001f, null );

			_animationWindow.Repaint();
		}

		public static int GetCurrentFrame()
		{
			if( AnimationWindow == null )
				return -1;
		
			return (int)FrameProperty.GetValue( GetState(), null );
		}

		public static void SelectAnimationClip( AnimationClip clip )
		{
			if( AnimationWindow == null || clip == null )
				return;

			AnimationClip[] clips = AnimationUtility.GetAnimationClips(Selection.activeGameObject);

			int index = 0;
			for( ; index != clips.Length; ++index )
			{
				if( clips[index] == clip )
					break;
			}


			if( index == clips.Length )
			{
				// didn't find
				Debug.LogError("Couldn't find clip " + clip.name);
			}
			else
			{
				// found
				AnimationClipProperty.SetValue( GetSelectedItem(), clip, null );
			}
		}

		public static AnimationClip GetSelectedAnimationClip()
		{
			return AnimationClipProperty.GetValue(GetSelectedItem(), null) as AnimationClip;
		}
	}
}
