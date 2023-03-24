using UnityEngine;
using System.Collections.Generic;

namespace Flux
{
	public class FSequencePlayer : MonoBehaviour {

		[SerializeField]
		private List<FSequence> _sequences = new List<FSequence>();
		public List<FSequence> Sequences { get { return _sequences; } }
		
		[SerializeField]
		[Tooltip("Init all the sequences at the start? Use this to avoid frame drops at runtime")]
		private bool _initAllOnStart = true; 
		public bool InitAllOnStart { get { return _initAllOnStart; } set { _initAllOnStart = value; } }
		
		[SerializeField]
		private bool _playOnStart = true;
		public bool PlayOnStart { get { return _playOnStart; } set { _playOnStart = value; } }

		[SerializeField]
		[Tooltip("At which update rate should we update the sequences and check if they are finished")]
		private AnimatorUpdateMode _updateMode = AnimatorUpdateMode.Normal;
		public AnimatorUpdateMode UpdateMode { get { return _updateMode; } set { _updateMode = value; } }

		private int _currentSequence = -1;

		void Start()
		{
			if( InitAllOnStart )
				InitAll();
			if( PlayOnStart )
				Play();
		}

		/// @brief Init all sequences, to avoid bumps at play time
		public void InitAll()
		{
			foreach( FSequence sequence in _sequences )
			{
				if( sequence != null )
					sequence.Init();
			}
		}

		/// @override Starts playing from first sequence.
		public void Play()
		{
			Play( 0 );
		}

		/**
		 * @brief Start playing from a specific sequence index.
		 * @param sequenceIndex Index of the list
		 */
		public void Play( int sequenceIndex )
		{
            //如果是正在播放则暂停
			if( IsPlaying )
				_sequences[_currentSequence].Pause();

			_currentSequence = sequenceIndex;
			_sequences[_currentSequence].UpdateMode = UpdateMode;
			if( !_sequences[_currentSequence].IsStopped )
				_sequences[_currentSequence].Stop();
			_sequences[_currentSequence].Play();
		}

		/**
		 * @brief Stop playing
		 * @param reset Reset all the sequences?
		 */
		public void Stop( bool reset )
		{
			foreach( FSequence sequence in _sequences )
				sequence.Stop( reset );
		}

		/// @brief Only checks if we have any sequence already running
		public bool IsPlaying { get { return _currentSequence >= 0; } }

		// checks if the current sequence finished, and if so starts the next one
		private void CheckSequence()
		{
			if( !IsPlaying ) return;

			if( _sequences[_currentSequence].IsFinished )
			{
				++_currentSequence;
				if( _currentSequence < _sequences.Count )
				{
					Play( _currentSequence );
				}
				else
					_currentSequence = -1;
			}
		}

		void LateUpdate()
		{
			if( UpdateMode != AnimatorUpdateMode.AnimatePhysics )
				CheckSequence();
		}

		void FixedUpdate()
		{
			if( UpdateMode == AnimatorUpdateMode.AnimatePhysics )
				CheckSequence();
		}
	}
}
