using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

using Flux;

namespace FluxEditor
{
	public class FMultiEventInspector : FMultiTypeInspector<FEvent> {

		public override void OnInspectorGUI()
		{
			bool triggerOnSkipMatch = true;
			
			for( int i = 0; i != _objects.Length; ++i )
			{
				if( _objects[i].TriggerOnSkip != _objects[0].TriggerOnSkip )
				{
					triggerOnSkipMatch = false;
					break;
				}
			}
			
			EditorGUI.BeginChangeCheck();
			bool triggerOnSkip = EditorGUILayout.Toggle( "Trigger On Skip", _objects[0].TriggerOnSkip, triggerOnSkipMatch ? EditorStyles.toggle : "ToggleMixed" );
			if( EditorGUI.EndChangeCheck() )
			{
				Undo.RecordObjects( _objects, " Inspector" );
				for( int i = 0; i != _objects.Length; ++i )
				{
					_objects[i].TriggerOnSkip = triggerOnSkip;
					EditorUtility.SetDirty( _objects[i] );
				}
			}
		}
	}
}
