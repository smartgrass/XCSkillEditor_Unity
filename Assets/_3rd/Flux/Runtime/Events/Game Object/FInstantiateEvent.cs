using UnityEngine;

namespace Flux
{
	//[FEvent("Game Object/Instantiate")]
	public class FInstantiateEvent : FEvent
	{
		[SerializeField]
		private GameObject _prefab = null;

		private GameObject _instance;

		protected override void OnTrigger( float timeSinceTrigger )
		{
			_instance = (GameObject)Instantiate( _prefab );
		}

		protected override void OnStop ()
		{
			if( _instance != null )
			{
				if( Application.isPlaying )
					Destroy( _instance );
				else
					DestroyImmediate( _instance );
			}
		}
	}
}
