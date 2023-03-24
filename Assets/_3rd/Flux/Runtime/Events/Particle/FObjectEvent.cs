using UnityEngine;
using System.Collections;

namespace Flux
{
	[FEvent("GamoObject/FObjectEvent", typeof(FTrack))]
	public class FObjectEvent : FEvent
	{
        [SerializeField]
        private bool _active = true;
        private GameObject _ownerGO = null;

        protected override void OnTrigger(float timeSinceTrigger)
        {
            if (_ownerGO == null)
            {
                _ownerGO = Owner.gameObject;
            }
            //			_ownerGO.SetActive( _active ); <- not needed since it will be handled OnUpdateEvent
        }

        protected override void OnUpdateEvent(float timeSinceTrigger)
        {
            if (_ownerGO.activeSelf != _active)
            {

                _ownerGO.SetActive(_active);
            }
        }

        protected override void OnFinish()
        {
            _ownerGO.SetActive(!_active);
        }

    }
}

