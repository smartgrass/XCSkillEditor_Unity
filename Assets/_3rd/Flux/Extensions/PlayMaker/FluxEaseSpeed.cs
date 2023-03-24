#if FLUX_PLAYMAKER
using UnityEngine;
using System.Collections;

using Flux;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("Flux")]
	[Tooltip("Sets speed of a Flux sequence. If negative, it will play backwards")]
	public class FluxEaseSpeed : EaseFloat {

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

		public override void OnUpdate()
		{
			base.OnUpdate();
			sequence.Speed = resultFloats[0];
		}
	}
}
#endif
