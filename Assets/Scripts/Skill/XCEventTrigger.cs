using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace XiaoCao
{
    public class XCEventTrigger : MonoBehaviour, IXCEventTrigger
    {
        public virtual void Invoke(string str, Object other) { }

    }


    public interface IXCEventTrigger
    {
        public void Invoke(string str, Object other);
    }

}
