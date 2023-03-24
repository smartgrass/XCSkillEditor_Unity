using UnityEngine;

namespace Flux
{
	//[FEvent("Light/Intensity")]
	public class FLightIntensityEvent : FTweenEvent<FTweenFloat>
	{
		private Light _light = null;

		private float _startIntensity;

		protected override void OnInit()
		{
			_light = Owner.GetComponent<Light>();
		}

		protected override void OnTrigger( float timeSinceTrigger )
		{
			_startIntensity = _light.intensity;
		}

		protected override void SetDefaultValues()
		{
			_tween = new FTweenFloat( 1, 2 );
		}

		protected override void ApplyProperty( float t )
		{
			_light.intensity = _tween.GetValue( t );
		}

		protected override void OnStop ()
		{
			_light.intensity = _startIntensity;

//			Debug.Log ("setting intensity stop: " + _light.intensity );
		}

		protected override void PreEvent()
		{
//			_light.hideFlags = HideFlags.DontSave;
			Owner.gameObject.hideFlags = HideFlags.DontSave;
		}

		protected override void PostEvent()
		{
//			_light.hideFlags = HideFlags.None;
			Owner.gameObject.hideFlags = HideFlags.None;
		}
	}
}
