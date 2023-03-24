using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

using Flux;

namespace FluxEditor
{
	public abstract class FMultiTypeInspector<T> : ScriptableObject where T : FObject {

		protected T[] _objects = new T[0];

		public void SetObjects( T[] objects )
		{
			_objects = objects;
		}

		public abstract void OnInspectorGUI();
	}
}
