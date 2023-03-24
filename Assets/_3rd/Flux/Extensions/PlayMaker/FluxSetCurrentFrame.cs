#if FLUX_PLAYMAKER
using UnityEngine;
using System.Collections;

using Flux;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("Flux")]
	[Tooltip("Jumps the Flux sequence to a certain frame")]
	public class FluxSetCurrentFrame : FsmStateAction {

		[RequiredField]
		[CheckForComponent(typeof(FSequence))]
		[Tooltip("GameObject that contains the sequence")]
		public FsmOwnerDefault sequenceGO;
		
		public FsmInt frame = new FsmInt();

		private FSequence sequence = null;
		
		public override void Awake()
		{
			base.Awake();
			sequence = Fsm.GetOwnerDefaultTarget( sequenceGO ).GetComponent<FSequence>();
		}


		public override void OnEnter()
		{
			sequence.SetCurrentFrame(frame.Value);
			Finish();
		}

		public override void Reset()
		{
			base.Reset();
			frame.Value = 0;
		}
	}
}
#endif
