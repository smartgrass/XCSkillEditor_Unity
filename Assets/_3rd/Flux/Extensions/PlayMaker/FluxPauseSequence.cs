#if FLUX_PLAYMAKER
using UnityEngine;
using System.Collections;

using Flux;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("Flux")]
	[Tooltip("Pauses Flux sequence")]
	public class FluxPauseSequence : FsmStateAction {

		[RequiredField]
		[CheckForComponent(typeof(FSequence))]
		[Tooltip("GameObject that contains the sequence")]
		public FsmOwnerDefault sequenceGO;
		
		private FSequence sequence = null;
		
		public override void Awake()
		{
			base.Awake();
			sequence = Fsm.GetOwnerDefaultTarget( sequenceGO ).GetComponent<FSequence>();
		}

		public override void OnEnter()
		{
			sequence.Pause();
			Finish();
		}
	}
}
#endif
