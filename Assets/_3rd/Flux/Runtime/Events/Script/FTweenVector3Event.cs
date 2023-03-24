using UnityEngine;
using System;
using System.Reflection;

namespace Flux
{
	//[FEvent("Script/Tween Vector3")]
	public class FTweenVector3Event : FTweenVariableEvent<FTweenVector3>  {

		protected override void SetDefaultValues ()
		{
			_tween = new FTweenVector3(Vector3.zero, Vector3.one);
		}

		protected override object GetValueAt( float t )
		{
			return _tween.GetValue( t );
		}
	}
}
