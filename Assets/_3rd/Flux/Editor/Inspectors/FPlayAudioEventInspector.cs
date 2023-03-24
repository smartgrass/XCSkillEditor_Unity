using UnityEngine;
using UnityEditor;

using Flux;

namespace FluxEditor
{

	[CustomEditor(typeof(FPlayAudioEvent))]
	public class FPlayAudioEventInspector : FEventInspector {

		private FPlayAudioEvent _audioEvt = null;

		private SerializedProperty _startOffset = null;

		protected override void OnEnable ()
		{
			base.OnEnable();

			_audioEvt = (FPlayAudioEvent)target;

			_startOffset = serializedObject.FindProperty("_startOffset");
		}

		public override void OnInspectorGUI ()
		{
			AudioClip currentClip = _audioEvt.AudioClip;

			base.OnInspectorGUI();

			if( currentClip != _audioEvt.AudioClip && _audioEvt.AudioClip != null && !_audioEvt.Loop )
			{
				if( _audioEvt.LengthTime > _audioEvt.AudioClip.length )
				{
					Undo.RecordObject( _audioEvt, null );
					_audioEvt.Length = Mathf.RoundToInt( _audioEvt.AudioClip.length * _audioEvt.Sequence.FrameRate );
					FSequenceEditorWindow.RefreshIfOpen();
				}
			}

			serializedObject.Update();
			_startOffset.intValue = Mathf.Clamp( _startOffset.intValue, 0, _audioEvt.GetMaxStartOffset() );
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.IntSlider( _startOffset, 0, _audioEvt.GetMaxStartOffset() );
			if( EditorGUI.EndChangeCheck() )
			{
				if( FSequenceEditorWindow.instance != null )
					FSequenceEditorWindow.instance.Repaint();
			}
			serializedObject.ApplyModifiedProperties();
		}
	}
}
