//#define FLUX_DEBUG
using UnityEngine;
using System.Collections.Generic;

namespace Flux
{
	/// @brief Cache for FSequenceTrack
	public class FSequenceTrackCache : FTrackCache {

		public FSequenceTrackCache( FSequenceTrack track )
			:base( track )
		{
		}

		protected override bool BuildInternal()
		{
#if FLUX_DEBUG
			Debug.LogWarning("Creating Sequence Preview");
#endif

            FSequence sequence = Track.Owner.GetComponent<FSequence>();

            foreach ( FContainer container in sequence.Containers )
			{
				foreach( FTimeline timeline in container.Timelines )
				{
					foreach( FTrack track in timeline.Tracks )
					{
						if( (track.RequiresForwardCache && Track.Sequence.IsPlayingForward)
							|| (track.RequiresBackwardsCache && !Track.Sequence.IsPlayingForward)
							|| (track.RequiresEditorCache && !Application.isPlaying) )
						{
							track.CreateCache();
						}
					}
				}
			}

			return true;
		}

		protected override bool ClearInternal()
		{
#if FLUX_DEBUG
			Debug.LogWarning("Destroying Sequence Preview");
#endif

			FSequence sequence = Track.Owner.GetComponent<FSequence>();
			
//			foreach( FContainer container in sequence.Containers )
//			{
//				foreach( FTimeline timeline in container.Timelines )
//				{
//					foreach( FTrack track in timeline.Tracks )
//					{
//						if( track.CanTogglePreview )
//						{
//							track.CanPreview = false;
//						}
//					}
//				}
//			}

			foreach( FContainer container in sequence.Containers )
			{
				foreach( FTimeline timeline in container.Timelines )
				{
					foreach( FTrack track in timeline.Tracks )
					{
						if( track.HasCache )
						{
							track.ClearCache();
						}
					}
				}
			}

			FAnimationTrack.DeleteAnimationPreviews( sequence );

			return true;
		}

//		/// @brief Starts playback 
//		public void StartPlayback()
//		{
//			if( _inPlayback )
//				return;
//			Debug.Log("Start Playback");
//
//			_inPlayback = true;
//		}
//
//		/// @brief Stops playback
//		public void StopPlayback()
//		{
//			if( !_inPlayback )
//				return;
//			Debug.Log("Stop Playback");
//			Animator.StopPlayback();
//
//			_inPlayback = false;
//		}

		public override void GetPlaybackAt( float sequenceTime )
		{
			Track.UpdateEventsEditor( (int)(sequenceTime * Track.Sequence.FrameRate), sequenceTime );
		}
	}
}
