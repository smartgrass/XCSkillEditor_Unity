#if FLUX_PLAYMAKER
using UnityEngine;
using System.Collections;

using Flux;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("Flux")]
	[Tooltip("Stops Flux sequence, basically rewind it to the start. If reset is true, it will re-init the sequence next time it plays")]
	public class FluxStopSequence : FsmStateAction {

		[RequiredField]
		[CheckForComponent(typeof(FSequence))]
		[Tooltip("GameObject that contains the sequence")]
		public FsmOwnerDefault sequenceGO;
		
		[Tooltip("If reset is true, it will re-init the events next time it plays")]
		public FsmBool reset = new FsmBool();

		private FSequence sequence = null;
		
		public override void Awake()
		{
			base.Awake();
			sequence = Fsm.GetOwnerDefaultTarget( sequenceGO ).GetComponent<FSequence>();
		}

		public override void OnEnter()
		{
			sequence.Stop( reset.Value );
			Finish();
		}

		public override void Reset()
		{
			base.Reset();
			reset.Value = false;
		}
	}
}
#endif
