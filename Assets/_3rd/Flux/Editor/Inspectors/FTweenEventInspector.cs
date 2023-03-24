
using UnityEditor;
using Flux;
using UnityEngine;

namespace FluxEditor
{
	[CustomEditor(typeof( FTweenEvent<> ), true)]
	public class FTweenEventInspector : FEventInspector {

		protected SerializedProperty _tween;
		protected SerializedProperty _from;
		protected SerializedProperty _to;
		protected Object self;

		protected override void OnEnable()
		{
			base.OnEnable();

			_tween = serializedObject.FindProperty("_tween");
			_tween.isExpanded = true;

			_from = _tween.FindPropertyRelative( "_from" );
			_to = _tween.FindPropertyRelative( "_to" );

			self = target;
		}
	}

}
