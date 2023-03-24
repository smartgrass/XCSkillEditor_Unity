using UnityEngine;

namespace Flux
{
	public abstract class FTweenEvent<T> : FEvent where T : FTweenBase {

		[SerializeField]
		protected T _tween = default(T);
		public T Tween { get { return _tween; } }

		protected override void OnTrigger( float timeSinceTrigger )
		{
            if (!EnableEvent)
                return;
            OnUpdateEvent( timeSinceTrigger );
		}

		protected override void OnUpdateEvent( float timeSinceTrigger )
		{
            if (!EnableEvent)
                return;
			float t = timeSinceTrigger / LengthTime;

			ApplyProperty( t );
		}

		protected override void OnFinish()
		{
            if (!EnableEvent)
                return;
            ApplyProperty( 1f );
		}

		protected override void OnStop()
		{
            if (!EnableEvent)
                return;
            ApplyProperty( 0f );
		}

		protected abstract void ApplyProperty( float t );
	}
}
