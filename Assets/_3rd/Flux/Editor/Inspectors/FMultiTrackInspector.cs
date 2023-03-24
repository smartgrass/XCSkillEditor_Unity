using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

using Flux;

namespace FluxEditor
{
	public class FMultiTrackInspector : FMultiTypeInspector<FTrack> {

		public override void OnInspectorGUI()
		{
			GUILayout.Label("Different Track Types");
		}
	}
}
