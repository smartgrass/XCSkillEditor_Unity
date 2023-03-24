using UnityEngine;


namespace Flux
{
	//[FEvent("Time/Timescale")]
	public class FTimescaleEvent : FEvent {

		[SerializeField]
		private AnimationCurve _curve;
		public AnimationCurve Curve { get { return _curve; } set { _curve = value; } }

		[SerializeField]
		[Tooltip("Set Time.timescale back to 1 at the end?")]
		private bool _clearOnFinish = true;
		public bool ClearOnFinish { get { return _clearOnFinish; } set { _clearOnFinish = value; } }

		protected override void SetDefaultValues ()
		{
			_curve = new AnimationCurve( new Keyframe[]{ new Keyframe(0, 1) } );
		}

		protected override void OnFrameRangeChanged( FrameRange oldFrameRange )
		{
			if( oldFrameRange.Length != FrameRange.Length )
			{
				FUtility.ResizeAnimationCurve( _curve, FrameRange.Length * Sequence.InverseFrameRate );
			}
		}

		protected override void OnTrigger( float timeSinceTrigger )
		{
			
		}

		protected override void OnUpdateEvent( float timeSinceTrigger )
		{
			Time.timeScale = Mathf.Clamp( _curve.Evaluate( timeSinceTrigger ), 0, 100); // unity "breaks" if it is outside this range
		}

		protected override void OnStop()
		{
			Time.timeScale = 1;
		}

		protected override void OnFinish()
		{
			if( ClearOnFinish )
			{
				Time.timeScale = 1;
			}
		}
	}
}