using UnityEngine;


namespace Flux
{
	//[FEvent("Transform/Look At", typeof(FTransformTrack))]
	public class FLookAtEvent : FEvent
	{
		[SerializeField]
		private Transform _target;
		public Transform Target { get { return _target; } set { _target = value; } }

		[SerializeField]
		[HideInInspector]
		private bool _isInstant = true;

		[SerializeField]
		[HideInInspector]
		private FEasingType _easingType = FEasingType.Linear;

		private Quaternion _startRotation;

		protected override void OnTrigger( float timeSinceTrigger )
		{
			_startRotation = Owner.rotation;
		}

		protected override void OnUpdateEvent( float timeSinceTrigger )
		{
			if( _isInstant )
				LookAtTarget( 1f );
			else
				LookAtTarget( timeSinceTrigger / LengthTime );
		}

		protected override void OnFinish()
		{
			OnUpdateEvent( LengthTime );
		}

		protected override void OnStop ()
		{
			Owner.rotation = _startRotation;
		}

		private void LookAtTarget( float t )
		{
			if( _isInstant )
				Owner.LookAt( _target );
			else
			{
				Quaternion r = Quaternion.LookRotation( _target.position - Owner.position, Vector3.up );
				Owner.rotation = Quaternion.Lerp( _startRotation, r, FEasing.Tween(0f, 1f, t, _easingType) );
			}
		}
	}
}
