//#define FLUX_DEBUG
using UnityEngine;
using System.Collections.Generic;

namespace Flux
{
	/// @brief Cache for FSequenceTrack
	public class FInputTrackCache : FTrackCache {

		public FInputTrackCache(FInputTrack track )
			:base( track )
		{
		}

		protected override bool BuildInternal()
		{
#if FLUX_DEBUG
			Debug.LogWarning("Creating Sequence Preview");
#endif

			return true;
		}

		protected override bool ClearInternal()
		{
#if FLUX_DEBUG
			Debug.LogWarning("Destroying Sequence Preview");
#endif

			return true;
		}

		public override void GetPlaybackAt( float sequenceTime )
		{
            if (Input.anyKeyDown)
            {
                Debug.Log("yns  anyKeyDown ");
            }
            if (Event.current != null)
            {
                Debug.Log("yns  " + Event.current.keyCode);
            }

			Track.UpdateEventsEditor( (int)(sequenceTime * Track.Sequence.FrameRate), sequenceTime );
		}
	}
}
