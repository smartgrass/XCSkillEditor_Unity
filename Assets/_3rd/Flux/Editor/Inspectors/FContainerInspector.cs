using UnityEngine;
using UnityEditor;

using System;

using Flux;

namespace FluxEditor
{
	[CustomEditor(typeof(FContainer), true)]
	public class FContainerInspector : Editor {

		FContainer _container = null;

		void OnEnable()
		{
            if(target != null)
			_container = (FContainer)target;
		}

		public override void OnInspectorGUI ()
		{
			EditorGUI.BeginChangeCheck();
			string name = EditorGUILayout.TextField( "Name", _container.gameObject.name );
			if( EditorGUI.EndChangeCheck() )
			{
				Undo.RecordObject( _container.gameObject, "change name" );
				_container.gameObject.name = name;
				EditorUtility.SetDirty( _container.gameObject );
			}
			base.DrawDefaultInspector();
		}
	}
}
