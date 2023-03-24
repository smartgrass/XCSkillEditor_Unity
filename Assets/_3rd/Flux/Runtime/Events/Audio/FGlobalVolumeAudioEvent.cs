using UnityEngine;
using System.Collections;

namespace Flux
{
	//[FEvent("Audio/Global Volume")]
	public class FGlobalVolumeAudioEvent : FTweenEvent<FTweenFloat>
	{
		protected override void ApplyProperty( float t )
		{
			AudioListener.volume = _tween.GetValue( t );
		}

		protected override void SetDefaultValues ()
		{
			_tween = new FTweenFloat( 0, 1 );
		}
	}
}
