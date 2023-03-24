using UnityEngine;
using System.Collections.Generic;

namespace Flux
{
	//[FEvent("Camera/Camera Manager")]
	public class FCameraManagerEvent : FEvent {

		[Tooltip("Camera that should become active")]
		[SerializeField]
		private Camera _camera = null;
		public Camera Camera { get { return _camera; } set { _camera = value; } }

		protected override void OnInit ()
		{
			// only one of the events will turn off all the cameras at the start
			// this is simply to not have a camera manager track which would only have to do this
			if( GetId() == 0 )
			{
				List<FEvent> events = Track.Events;
				foreach( FCameraManagerEvent evt in events )
					if( evt.Camera != null )
						evt.Camera.gameObject.SetActive( false );
			}
		}

		protected override void OnTrigger( float timeSinceTrigger )
		{
			_camera.gameObject.SetActive( true );
		}

		protected override void OnFinish()
		{
			_camera.gameObject.SetActive( false );
		}

		public override string Text {
			get {
				return _camera == null ? "!Missing!" : _camera.name;
			}
			set {
				// cannot set
			}
		}
	}
}
