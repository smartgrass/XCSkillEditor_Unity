using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

using Flux;

namespace FluxEditor
{
	[FEditor(typeof(FPlayAudioEvent))]
	public class FPlayAudioEventEditor : FEventEditor {

		private float[] _clipMinMaxData = null;
		private FPlayAudioEvent _playAudioEvt = null;
		private AudioClip _currentClip = null;

		private SerializedProperty _startOffset;

		private int _frameMouseDown = int.MaxValue;

		public override float Height {
			get {
				return base.Height+base.Height;
			}
		}

		public override void Init( FObject obj, FEditor owner )
		{
			base.Init( obj, owner);
			_playAudioEvt = (FPlayAudioEvent)obj;

			_startOffset = new SerializedObject( _playAudioEvt ).FindProperty( "_startOffset" );
		}

		protected override void RenderEvent( FrameRange viewRange, FrameRange validKeyframeRange )
		{
			int startOffsetHandleId = EditorGUIUtility.GetControlID(FocusType.Passive);

			switch( Event.current.type )
			{
			case EventType.MouseDown:
				if( Event.current.alt && EditorGUIUtility.hotControl == 0 && _eventRect.Contains(Event.current.mousePosition) )
				{
					EditorGUIUtility.hotControl = startOffsetHandleId;
					_frameMouseDown = SequenceEditor.GetFrameForX( Event.current.mousePosition.x ) - _playAudioEvt.Start;
					Event.current.Use();
				}
				break;

			case EventType.MouseDrag:
				if( EditorGUIUtility.hotControl == startOffsetHandleId )
				{
					int mouseCurrentFrame = Mathf.Clamp( SequenceEditor.GetFrameForX( Event.current.mousePosition.x ) - _playAudioEvt.Start, 0, _playAudioEvt.Length );

					int mouseDelta =  _frameMouseDown - mouseCurrentFrame;

					if( mouseDelta != 0 )
					{
						_frameMouseDown = mouseCurrentFrame;
						_startOffset.serializedObject.Update();
						_startOffset.intValue = Mathf.Clamp( _startOffset.intValue + mouseDelta, 0, _playAudioEvt.GetMaxStartOffset() );
						_startOffset.serializedObject.ApplyModifiedProperties();
					}
					Event.current.Use();
				}
				break;
			case EventType.MouseUp:
				if( EditorGUIUtility.hotControl == startOffsetHandleId )
				{
					EditorGUIUtility.hotControl = 0;
					Event.current.Use();
				}
				break;
			}

			int frameRangeLength = _playAudioEvt.Length;

			base.RenderEvent( viewRange, validKeyframeRange );

			if( frameRangeLength != _playAudioEvt.Length )
			{
				_startOffset.serializedObject.Update();
				_startOffset.intValue = Mathf.Clamp( _startOffset.intValue, 0, _playAudioEvt.GetMaxStartOffset() );
				_startOffset.serializedObject.ApplyModifiedProperties();
			}

			if( _currentClip != _playAudioEvt.AudioClip || _clipMinMaxData == null )
			{
				_clipMinMaxData = FUtility.GetAudioMinMaxData(_playAudioEvt.AudioClip);
				_currentClip = _playAudioEvt.AudioClip;
			}

			if( Event.current.type == EventType.Repaint && _clipMinMaxData != null )
			{
				float border = 2;

				Rect waveformRect = _eventRect;
				waveformRect.xMin += border;
				waveformRect.xMax -= border;

				FrameRange visibleEvtRange = _playAudioEvt.FrameRange;
				float startOffset = _playAudioEvt.StartOffset;
				if( viewRange.Start > visibleEvtRange.Start )
				{
					startOffset += viewRange.Start - visibleEvtRange.Start;
					visibleEvtRange.Start = viewRange.Start;
				}

				if( viewRange.End < visibleEvtRange.End )
				{
					visibleEvtRange.End = viewRange.End;
				}
					
				int numChannels = _currentClip.channels;
				int numSamples = _clipMinMaxData.Length / (2 * numChannels);

				Rect fullWaveformRect = _eventRect;

				fullWaveformRect.width = SequenceEditor.PixelsPerFrame*_currentClip.length*SequenceEditor.Sequence.FrameRate;

				GUI.BeginClip(waveformRect);

				fullWaveformRect.yMin -= 5;
				fullWaveformRect.yMax += 15;
				fullWaveformRect.x = -startOffset*SequenceEditor.PixelsPerFrame;

				AudioCurveRendering.DrawMinMaxFilledCurve( fullWaveformRect, delegate(float x, out Color col, out float minValue, out float maxValue) {
					col = new Color(1f, 0.55f, 0f, 1f);
					float p = Mathf.Clamp(x * (numSamples - 2), 0.0f, numSamples - 2);
					int i = (int)Mathf.Floor(p);

					minValue = 10;
					maxValue = -10;

					for( int channel = 0; channel != numChannels; ++channel ) 
					{
						int offset1 = (i * numChannels + channel) * 2;
						int offset2 = offset1 + numChannels * 2;
						minValue = Mathf.Min(minValue, _clipMinMaxData[offset1 + 1], _clipMinMaxData[offset2 + 1]);
						maxValue = Mathf.Max(maxValue, _clipMinMaxData[offset1 + 0], _clipMinMaxData[offset2 + 0]);
						if (minValue > maxValue)
						{ 
							float tmp = minValue; 
							minValue = maxValue; 
							maxValue = tmp; 
						}
					}
				});
				GUI.EndClip();
			}
		}
	}
}
