using UnityEngine;

namespace Flux
{
	[FEvent("Transform/Tween Position", typeof(FTransformTrack))]
	public class FTweenPositionEvent : FTweenEvent<FTweenVector3_Ex>
	{
        public bool IsBezier =>Tween.isBezier;

		public bool IsConstrain;


		public Vector3 From
        {
            get
            {
                if (IsConstrain && Owner.parent!=null)
                {	
					return Owner.parent.TransformPoint(Tween.From);
				}
				return Tween.From;
            }
            set
            {
				if (IsConstrain && Owner.parent != null)
				{
					Tween.From = Owner.parent.InverseTransformPoint(value);
				}
                else
                {
					Tween.From = value;
				}
			}
        }

		public Vector3 To
		{
			get
			{
				if (IsConstrain && Owner.parent != null)
				{
					return Owner.parent.TransformPoint(Tween.To);
				}
				return Tween.To;
			}
			set
			{
				if (IsConstrain && Owner.parent != null)
				{
					Tween.To = Owner.parent.InverseTransformPoint(value);
				}
				else
				{
					Tween.To = value;
				}
			}
		}

		public Vector3 HandlePoint
        {
			get
			{
				if (IsConstrain && Owner.parent != null)
				{
					return Owner.parent.TransformPoint(Tween.HandlePoint);
				}
				return Tween.HandlePoint;
			}
			set
			{
				if (IsConstrain && Owner.parent != null)
				{
					Tween.HandlePoint = Owner.parent.InverseTransformPoint(value);
				}
				else
				{
					Tween.HandlePoint = value;
				}
			}
		}


		protected override void OnTrigger( float timeSinceTrigger )
		{
			base.OnTrigger( timeSinceTrigger );
		}

		protected override void OnStop()
		{
			base.OnStop();
			ApplyProperty(0);
		}

		protected override void SetDefaultValues()
		{
			_tween = new FTweenVector3_Ex( Vector3.zero, Vector3.forward );
		}

		protected override void ApplyProperty( float t )
		{
			Owner.localPosition = _tween.GetValue( t ); 
            if (_tween.isBezier && _tween.lookForward)
            {
				Vector3 vector = _tween.GetSpeed(t);
                Owner.transform.forward = Owner.transform.parent.TransformDirection(vector);
            }
		}




	}
}
