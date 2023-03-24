#if FLUX_PLAYMAKER
using UnityEngine;
using System.Collections;

using Flux;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("Flux")]
	[Tooltip("Jumps the Flux sequence to a certain time")]
	public class FluxSetCurrentTime : FsmStateAction {

		[RequiredField]
		[CheckForComponent(typeof(FSequence))]
		[Tooltip("GameObject that contains the sequence")]
		public FsmOwnerDefault sequenceGO;

		public FsmFloat time = new FsmFloat();

		private FSequence sequence = null;
		
		public override void Awake()
		{
			base.Awake();
			sequence = Fsm.GetOwnerDefaultTarget( sequenceGO ).GetComponent<FSequence>();
		}

		public override void OnEnter()
		{
			sequence.SetCurrentTime(time.Value);
			Finish();
		}

		public override void Reset()
		{
			base.Reset();
			time.Value = 0;
		}
	}
}
#endif
