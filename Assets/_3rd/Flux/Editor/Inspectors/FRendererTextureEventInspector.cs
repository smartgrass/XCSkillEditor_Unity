using UnityEngine;
using UnityEditor;

using Flux;

namespace FluxEditor
{
	[CustomEditor(typeof(FRendererTextureEvent))]
	public class FRendererTextureEventInspector : FRendererEventInspector {

		protected override void OnEnable()
		{
			_customPropertyName = "_MainTex";

			base.OnEnable();
		}

		protected override bool IsValidProperty( ShaderUtil.ShaderPropertyType shaderPropertyType )
		{
			return shaderPropertyType == ShaderUtil.ShaderPropertyType.TexEnv;
		}
	}
}
