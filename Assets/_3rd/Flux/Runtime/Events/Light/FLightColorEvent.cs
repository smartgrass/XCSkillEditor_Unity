using UnityEngine;

namespace Flux
{
	//[FEvent("Light/Color")]
	public class FLightColorEvent : FTweenEvent<FTweenColor>
	{
		private Light _light = null;

		private Color _startColor;
		
		protected override void OnInit()
		{
			_light = Owner.GetComponent<Light>();
		}
		
		protected override void SetDefaultValues()
		{
			_tween = new FTweenColor( Color.white, Color.red );
		}

		protected override void OnTrigger( float timeSinceTrigger )
		{
			_startColor = _light.color;
		}
		
		protected override void ApplyProperty( float t )
		{
			_light.color = _tween.GetValue( t );
		}

		protected override void OnStop ()
		{
			_light.color = _startColor;
		}
	}
}
