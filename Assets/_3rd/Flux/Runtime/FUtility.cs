using UnityEngine;
using System.Collections.Generic;

namespace Flux
{
	/**
	 * @brief Runtime Utility class for Flux
	 */
	public class FUtility {

		public const int FLUX_VERSION = 210;

		public static bool IsAnimationEditable( AnimationClip clip )
		{
			return clip == null || ( ((clip.hideFlags & HideFlags.NotEditable) == 0) && !clip.isLooping );
		}

		public static void ResizeAnimationCurve( AnimationCurve curve, float newLength )
		{
			float frameRate = 60;

			float oldLength = curve.length == 0 ? 0 : curve.keys[curve.length-1].time;
//			float newLength = newLength;
			
			if( oldLength == 0 )
			{
				// handle no curve
				curve.AddKey(0, 1);
				curve.AddKey(newLength, 1);
				return;
			}
			
			float ratio = newLength / oldLength;
			float inverseRatio = 1f / ratio;
			
			int start = 0;
			int limit = curve.length;
			int increment = 1;
			
			if( ratio > 1 )
			{
				start = limit - 1;
				limit = -1;
				increment = -1;
			}
			
			for( int i = start; i != limit; i += increment )
			{
				Keyframe newKeyframe = new Keyframe( Mathf.RoundToInt(curve.keys[i].time * ratio * frameRate)/frameRate, curve.keys[i].value, curve.keys[i].inTangent*inverseRatio, curve.keys[i].outTangent*inverseRatio );
				newKeyframe.tangentMode = curve.keys[i].tangentMode;
				curve.MoveKey(i, newKeyframe );
			}
		}
	}
}
