using UnityEngine;

namespace Flux
{
    [FEvent("UnUse/Stop Sequence")]
    public class FStopSequenceEvent : FEvent 
	{
		private FSequence _sequence ;
		
		protected override void OnInit()
		{
			_sequence = Owner.GetComponent<FSequence>();
		}
		
		protected override void OnTrigger( float timeSinceTrigger )
		{
			_sequence.Stop();
		}
	}
}
