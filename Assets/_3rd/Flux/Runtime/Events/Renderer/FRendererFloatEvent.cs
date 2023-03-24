using UnityEngine;

namespace Flux
{
	//[FEvent("Renderer/Tween Float((UnUse))", typeof(FRendererTrack))]
	public class FRendererFloatEvent : FRendererEvent {

		[SerializeField]
		private FTweenFloat _tween = null;

		protected override void SetDefaultValues()
		{
			if( PropertyName == null )
				PropertyName = "_Alpha";
			
			if( _tween == null )
				_tween = new FTweenFloat(0f, 1f);
		}

		protected override void ApplyProperty( float t )
		{
			_matPropertyBlock.SetFloat(PropertyName, _tween.GetValue(t));
      	}
   	}
}
