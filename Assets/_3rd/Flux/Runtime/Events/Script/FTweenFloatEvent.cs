using UnityEngine;
using System;
using System.Reflection;

namespace Flux
{
	//[FEvent("Script/Tween Float")]
	public class FTweenFloatEvent : FTweenVariableEvent<FTweenFloat>  {

		protected override void SetDefaultValues ()
		{
			_tween = new FTweenFloat(0f, 1f);
		}

		protected override object GetValueAt( float t )
		{
			return _tween.GetValue( t );
		}
	}
}
