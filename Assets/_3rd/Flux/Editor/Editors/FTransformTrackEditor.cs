using UnityEngine;
using UnityEditor;

using Flux;

namespace FluxEditor
{
	[FEditor(typeof(FTransformTrack))]
	public class FTransformTrackEditor : FTrackEditor {

		public override void OnTrackChanged()
		{
			if( Track.Timeline == null ) // for copy paste reasons, the track may not have timeline
				return;
			FAnimationTrackCache animationTrackCache = FAnimationTrack.GetAnimationPreview( Track.Sequence, Track.Owner );

			if( animationTrackCache != null )
			{
				animationTrackCache.Build(true);
			}
		}
    }


}
