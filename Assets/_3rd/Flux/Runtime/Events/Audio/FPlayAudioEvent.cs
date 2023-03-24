using UnityEngine;
using System.Collections;

namespace Flux
{
	//[FEvent("Audio/Play Audio")]
	public class FPlayAudioEvent : FEvent {

		[SerializeField]
		private AudioClip _audioClip = null;
		public AudioClip AudioClip { get { return _audioClip; } }

		[Range(0f, 1f)]
		[SerializeField]
		private float _volume = 1f;

		[SerializeField]
		private bool _loop = false;
		public bool Loop { get { return _loop; } }

		[SerializeField]
		[HideInInspector]
		private int _startOffset = 0;
		public int StartOffset { get { return _startOffset; } }

		[SerializeField]
		private bool _speedDeterminesPitch = true;
		public bool SpeedDeterminesPitch { get { return _speedDeterminesPitch; } set { _speedDeterminesPitch = value; } }

		private AudioSource _source;

		protected override void OnTrigger( float timeSinceTrigger )
		{
			_source = Owner.GetComponent<AudioSource>();
			if( _source == null )
				_source = Owner.gameObject.AddComponent<AudioSource>();
			_source.volume = _volume;
			_source.loop = _loop;
			_source.clip = _audioClip;

			if( Sequence.IsPlaying )
				_source.Play();
			_source.time = _startOffset * Sequence.InverseFrameRate + timeSinceTrigger;
			if( SpeedDeterminesPitch )
				_source.pitch = Sequence.Speed * Time.timeScale;
		}

		protected override void OnPause()
		{
			_source.Pause();
		}

		protected override void OnResume()
		{
			if( Sequence.IsPlaying )
				_source.Play();
		}

		protected override void OnFinish()
		{
			if( _source.clip == _audioClip && _source.isPlaying )
			{
				_source.Stop();
				_source.clip = null;
			}
		}

		protected override void OnStop()
		{
			if( _source.clip == _audioClip && _source.isPlaying )
			{
				_source.Stop();
				_source.clip = null;
			}
		}

		public override int GetMaxLength()
		{
			if( _loop || _audioClip == null )
				return base.GetMaxLength();

			return Mathf.RoundToInt(_audioClip.length * Sequence.FrameRate);
		}

		public int GetMaxStartOffset()
		{
			if( _audioClip == null )
				return 0;

			int maxFrames = Mathf.RoundToInt( _audioClip.length * Sequence.FrameRate );

			if( _loop )
				return maxFrames;

			return maxFrames - Length;
		}

		public override string Text {
			get {
				return _audioClip == null ? "!Missing!" : _audioClip.name;
			}
			set {

			}
		}
	}

}
