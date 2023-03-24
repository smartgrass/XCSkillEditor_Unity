using UnityEngine;

namespace Flux
{
	public abstract class FRendererEvent : FEvent {

		protected MaterialPropertyBlock _matPropertyBlock;
		
		[SerializeField]
		[HideInInspector]
		private string _propertyName;
		public string PropertyName { get { return _propertyName; } set { _propertyName = value; } }

		protected override void OnInit()
		{
			base.OnInit();

			_matPropertyBlock = ((FRendererTrack)_track).GetMaterialPropertyBlock();
		}

		protected override void OnUpdateEvent( float timeSinceTrigger )
		{
			ApplyProperty( timeSinceTrigger / LengthTime );
		}

		protected override void OnStop ()
		{
			if( _matPropertyBlock != null )
				_matPropertyBlock.Clear();
		}

		protected abstract void ApplyProperty( float t );
	}
}
