using UnityEngine;
using System.Collections;

namespace Flux
{
	[FEvent("Animation/Play Animation", typeof(FAnimationTrack))]
	public class FPlayAnimationEvent : FEvent
	{
		//[HideInInspector]
		public AnimationClip _animationClip = null;

		[HideInInspector]
		[SerializeField]
		private bool _controlsAnimation = false;
		public bool ControlsAnimation { get { return _controlsAnimation; } set { _controlsAnimation = FUtility.IsAnimationEditable(_animationClip) && value; } }

		[HideInInspector]
		[Tooltip("How long is the transition blending?")]
		public int _blendLength;

		[HideInInspector]
		[Tooltip("What's the offset from where we play the animation?")]
		public int _startOffset;

		[HideInInspector]
		public int _stateHash;

		public bool isBackToIdle;

		public float _speed = 1;

		private Animator _animator = null;

		private FAnimationTrack _animTrack = null;

        //protected override void OnInit()
        //{
        //    base.OnInit();
        //}

        protected override void OnTrigger( float timeSinceTrigger )
		{
			_animator = Owner.GetComponent<Animator>();
			_animTrack = (FAnimationTrack)_track;

			if( _animator.runtimeAnimatorController != _animTrack.AnimatorController )
			{
				_animator.runtimeAnimatorController = _animTrack.AnimatorController;
			}

			_animator.enabled = _animationClip != null;

			int id = GetId();

#if UNITY_EDITOR
			if( _animator.enabled && (!_track.HasCache || Application.isPlaying) )
#else
			if( _animator.enabled )
#endif
			{
//				Debug.Log( "Turning on " + _animTrack.LayerName );
				_animator.SetLayerWeight(_animTrack.LayerId, 1);
				if( id == 0 || _track.Events[id-1].End < Start )
				{
					_animator.Play( _stateHash, _animTrack.LayerId, (_startOffset * Sequence.InverseFrameRate) / _animationClip.length );
				}

				if( timeSinceTrigger > 0 )
				{
					// - 0.001f because if you pass the length of the animation
					// it seems that it will go over it and miss the condition
					_animator.Update( timeSinceTrigger-0.001f );
				}
				else
					_animator.Update( 0f );
			}
		}

		protected override void OnUpdateEvent( float timeSinceTrigger )
		{
			if( !_animator.enabled )
				_animator.enabled = true;
		}

		protected override void OnFinish()
		{
			if( _animator && (IsLastEvent || _track.GetEvent(GetId()+1).Start != End))
			{
				_animator.enabled = false;
				_animator.SetLayerWeight(_animTrack.LayerId, 0);
			}
		}

		protected override void OnStop()
		{
			int id = GetId();

			if( _animator && (id == 0 || _track.GetEvent( id - 1 ).End != Start ) )
			{
				_animator.SetLayerWeight(_animTrack.LayerId, 0);
				_animator.enabled = false;
			}
		}

		protected override void OnPause()
		{
			_animator.enabled = false;
		}

		protected override void OnResume()
		{
			_animator.enabled = true;
		}

		public override int GetMaxLength()
		{
			if( FUtility.IsAnimationEditable( _animationClip ) || _animationClip.isLooping )
				return base.GetMaxLength();

			return Mathf.RoundToInt(_animationClip.length * _animationClip.frameRate - _startOffset);
		}

		public override string Text {
			get {
				return _animationClip == null ? "!Missing!" : _animationClip.name;
			}
			set {
				// cannot set
			}
		}

		public bool IsBlending()
		{
			int id = GetId();



            return id > 0 && _track != null && _track.Events[id - 1].End == Start && ((FAnimationTrack)_track).AnimatorController != null && ((FPlayAnimationEvent)_track.Events[id - 1])._animationClip != null && _animationClip != null;
            //return   _track != null && ((FAnimationTrack)_track).AnimatorController != null && _animationClip != null ;
		}

//		public bool IsAnimationEditable()
//		{
//			return _animationClip == null || (((_animationClip.hideFlags & HideFlags.NotEditable) == 0) && !_animationClip.isLooping);
//		}

		public int GetMaxStartOffset()
		{
			if( _animationClip == null )
				return 0;
			return _animationClip.isLooping ? Length : Mathf.RoundToInt(_animationClip.length * _animationClip.frameRate) - Length;
		}
	}
}
