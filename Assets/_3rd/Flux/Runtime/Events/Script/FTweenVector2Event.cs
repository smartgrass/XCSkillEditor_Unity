using UnityEngine;
using System;
using System.Reflection;

namespace Flux
{
	//[FEvent("Script/Tween Vector2")]
	public class FTweenVector2Event : FTweenVariableEvent<FTweenVector2>  {

		protected override void SetDefaultValues ()
		{
			_tween = new FTweenVector2(Vector2.zero, Vector2.one);
		}

		protected override object GetValueAt( float t )
		{
			return _tween.GetValue( t );
		}
	}
}
