using UnityEngine;

namespace Flux
{
	[FEvent("UnUse/Play Sequence", typeof(FSequenceTrack))]
	public class FPlaySequenceEvent : FEvent 
	{
		public FSequence _sequence = null;

        public bool IsSwitch = false;

        public FSequence switchSequence;

        [SerializeField]
		private int _startOffset = 0;
		public int StartOffset { get { return _startOffset; } }

		protected override void OnInit()
		{
            FSequence sq = Owner.GetComponent<FSequence>();
            if(sq != null)
            {
                _sequence = sq;
            }
            switchSequence = sq;
        }

		protected override void OnTrigger( float timeSinceTrigger )
		{
			if( Sequence.IsPlaying && Application.isPlaying )
			{
				_sequence.Play( _startOffset * _sequence.InverseFrameRate + timeSinceTrigger );
			}
		}

		protected override void OnUpdateEvent( float timeSinceTrigger )
		{
			_sequence.Speed = Mathf.Sign( Sequence.Speed ) * Mathf.Abs(_sequence.Speed);
		}

//		protected override void OnUpdateEvent( float timeSinceTrigger )
//		{
////			if( !Application.isPlaying )
////				_sequence.SetCurrentTime( StartOffset * Sequence.InverseFrameRate + timeSinceTrigger );
//		}
//
//		protected override void OnUpdateEventEditor( float timeSinceTrigger )
//		{
////			_sequence.SetCurrentTime( StartOffset * Sequence.InverseFrameRate + timeSinceTrigger );
////			_sequence.SetCurrentFrameEditor( _startOffset + framesSinceTrigger );
////			_sequence.SetCurrentFrame( _startOffset + framesSinceTrigger );
//		}

		protected override void OnStop()
		{
			_sequence.Stop( true );
		}

		protected override void OnFinish()
		{
			_sequence.Pause();
		}

		protected override void OnPause ()
		{
			_sequence.Pause();
		}

		protected override void OnResume()
		{
			_sequence.Play(_sequence.CurrentTime);
//			_sequence.Resume();
		}
	}
}
