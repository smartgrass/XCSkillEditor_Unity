using UnityEngine;
using System.Collections;

namespace Flux
{
	//[FEvent("Audio/Volume")]
	public class FVolumeAudioEvent : FTweenEvent<FTweenFloat>
	{
		private AudioSource _source;

		protected override void OnInit()
		{
			_source = Owner.GetComponent<AudioSource>();
		}

		protected override void ApplyProperty( float t )
		{
			_source.volume = _tween.GetValue( t );
		}
	}
}
