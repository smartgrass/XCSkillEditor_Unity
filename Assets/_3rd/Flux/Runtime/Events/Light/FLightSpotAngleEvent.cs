using UnityEngine;

namespace Flux
{
	//[FEvent("Light/Spot Angle")]
	public class FLightSpotAngleEvent : FTweenEvent<FTweenFloat>
	{
		private Light _light = null;
		
		protected override void OnInit()
		{
			_light = Owner.GetComponent<Light>();
		}
		
		protected override void SetDefaultValues()
		{
			_tween = new FTweenFloat( 30, 45 );
		}
		
		protected override void ApplyProperty( float t )
		{
			_light.spotAngle = _tween.GetValue( t );
		}
	}
}
