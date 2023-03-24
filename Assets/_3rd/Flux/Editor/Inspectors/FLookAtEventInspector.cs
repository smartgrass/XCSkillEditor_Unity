using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;

using Flux;

namespace FluxEditor
{
	[CustomEditor(typeof(FLookAtEvent))]
	public class FLookAtEventInspector : FEventInspector {

		private SerializedProperty _isInstant = null;
		private SerializedProperty _easingType = null;

		private AnimBool _showEasing = null;

		protected override void OnEnable ()
		{
			base.OnEnable ();

			_isInstant = serializedObject.FindProperty("_isInstant");
			_easingType = serializedObject.FindProperty("_easingType");

			_showEasing = new AnimBool( !_isInstant.boolValue );
		}

		public override void OnInspectorGUI ()
		{
			base.OnInspectorGUI();

			serializedObject.Update();

			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField( _isInstant );
			if( EditorGUI.EndChangeCheck() )
				_showEasing.target = !_isInstant.boolValue;

			if( EditorGUILayout.BeginFadeGroup( _showEasing.faded ) )
			{
				EditorGUILayout.PropertyField( _easingType );
			}
			EditorGUILayout.EndFadeGroup();

			if( _showEasing.isAnimating )
				Repaint();

			serializedObject.ApplyModifiedProperties();
		}

	}
}
