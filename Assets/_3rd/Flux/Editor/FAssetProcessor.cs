using UnityEngine;
using UnityEditor;

namespace FluxEditor
{
	public class FAssetProcessor : UnityEditor.AssetModificationProcessor {

		public static string[] OnWillSaveAssets( string[] paths )
		{
			bool isSavingScene = false;

			foreach( string path in paths )
			{
				if( path.EndsWith(".unity") )
				{
					isSavingScene = true;
					break;
				}
			}

			if( isSavingScene )
			{
				if( FSequenceEditorWindow.instance != null && FSequenceEditorWindow.instance.GetSequenceEditor()!=null)
				{
					FSequenceEditorWindow.instance.GetSequenceEditor().Stop();
					FSequenceEditorWindow.instance.Repaint();
				}
			}

			return paths;
		}
	}
}
