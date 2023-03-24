using UnityEngine;
using UnityEditor;

using Flux;

namespace FluxEditor
{
	[CustomEditor(typeof(FRendererFloatEvent))]
	public class FRendererFloatEventInspector : FRendererEventInspector {

		private SerializedProperty _tween = null;

		protected override void OnEnable()
		{
			_customPropertyName = "_Alpha";

			_tween = serializedObject.FindProperty("_tween");
			_tween.isExpanded = true;

			base.OnEnable();
		}

		protected override bool IsValidProperty( ShaderUtil.ShaderPropertyType shaderPropertyType )
		{
			return shaderPropertyType == ShaderUtil.ShaderPropertyType.Float || shaderPropertyType == ShaderUtil.ShaderPropertyType.Range;
		}
	}
}
