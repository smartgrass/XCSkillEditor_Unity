#if FLUX_PLAYMAKER
using UnityEngine;
using System.Collections;

using Flux;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("Flux")]
	[Tooltip("Plays a Flux sequence from a specific time. If pauseOnExit is true, it will pause the sequence when on state exit")]
	public class FluxPlaySequence : FsmStateAction {

		[RequiredField]
		[CheckForComponent(typeof(FSequence))]
		[Tooltip("GameObject that contains the sequence")]
		public FsmOwnerDefault sequenceGO;
		
		[Tooltip("Time to start playing from. If negative, it will start from the default time (i.e. 0 play forward, length playing backwards)")]
		public FsmFloat time = new FsmFloat();

		[Tooltip("Playback speed. If negative, it plays backwards")]
		public FsmFloat speed = new FsmFloat();

		[Tooltip("Should it pause when it exits?")]
		public FsmBool pauseOnExit = new FsmBool();

		[Tooltip("Event to trigger on finished. Note that sequences only finish if they aren't looping")]
		public FsmEvent onFinishedEvent = null;

		private FSequence sequence = null;
		
		public override void Awake()
		{
			base.Awake();
			sequence = Fsm.GetOwnerDefaultTarget( sequenceGO ).GetComponent<FSequence>();
		}

		public override void OnEnter()
		{
			sequence.OnFinishedCallback.AddListener( OnSequenceFinished );

			sequence.Speed = speed.Value;
			if( time.Value > 0 )
				sequence.Play(time.Value);
			else 
				sequence.Play();
		}

		public override void OnExit()
		{
			if( pauseOnExit.Value )
				sequence.Pause();
		}

		public override void Reset()
		{
			base.Reset();
			time.Value = -1;
			speed.Value = sequence == null ? 1f : sequence.DefaultSpeed;
			pauseOnExit.Value = true;
		}

		private void OnSequenceFinished( FSequence sequence )
		{
			Fsm.Event( onFinishedEvent );
			Finish();
		}
	}
}
#endif
