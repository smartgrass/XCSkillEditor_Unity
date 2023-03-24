using UnityEngine;
using System;
using System.Reflection;

namespace Flux
{
	//[FEvent("Script/Tween Vector4")]
	public class FTweenVector4Event : FTweenVariableEvent<FTweenVector4>  {

		protected override void SetDefaultValues ()
		{
			_tween = new FTweenVector4(Vector4.zero, Vector4.one);
		}

		protected override object GetValueAt( float t )
		{
			return _tween.GetValue( t );
		}
	}
}
