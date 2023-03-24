using UnityEngine;
using System.Collections;

namespace Flux
{
    //[FEvent("Game Object/Set Active")]
    public class FSetActiveEvent : FEvent
    {
        [SerializeField]
        private bool _active = true;

        [SerializeField]
        [Tooltip("Does the event set the opposite on the last frame?")]
        private bool _setOppositeOnFinish = true;

        private bool _wasActive = false;

        private GameObject _ownerGO = null;

        protected override void OnTrigger(float timeSinceTrigger)
        {
            if (_ownerGO == null)
            {
                _ownerGO = Owner.gameObject;
                _wasActive = _ownerGO.activeSelf;
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
            if (_setOppositeOnFinish)
                _ownerGO.SetActive(!_active);
        }

        protected override void OnStop()
        {
            Owner.gameObject.SetActive(_wasActive);
        }
    }
}
