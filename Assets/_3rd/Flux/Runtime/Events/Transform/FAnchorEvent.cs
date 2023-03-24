using UnityEngine;

namespace Flux
{
    [FEvent("UnUse/TransformAnchor", typeof(FTransformTrack))]
    public class FAnchorEvent : FEvent
	{
		[SerializeField]
		private Transform _anchor;
		public Transform Anchor { get { return _anchor; } set { _anchor = value; } }

		[SerializeField]
		[Tooltip("If not set, it will just place it on the same spot, won't change parenting")]
		private bool _parentToAnchor = true;
		private bool ParentToAnchor { get { return _parentToAnchor; } set { _parentToAnchor = value; } }

		private TransformSnapshot _snapshot = null;

		protected override void OnInit()
		{
			_snapshot = null;
		}

		protected override void OnTrigger( float timeSinceTrigger )
		{
			if( _snapshot == null )
			{
				_snapshot = new TransformSnapshot( Owner );
			}

			if( _anchor != null )
			{
				Owner.position = _anchor.position;
				Owner.rotation = _anchor.rotation;
			}

			if( _parentToAnchor )
			{
				Owner.parent = _anchor;
			}
		}

		protected override void OnStop()
		{
			_snapshot.Restore();
        }
	}
}
