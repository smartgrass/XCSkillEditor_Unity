using UnityEngine;
using UnityEditor;

using System;

using Flux;

namespace FluxEditor
{
	[Serializable]
	public class FContainerEditorInspector : FEditorInspector<FContainerEditor, FContainer> {

		public override string Title { 
			get { 
				if( _editors.Count == 1 )
					return "Container:";
				return "Containers:";
			}
		}
	}
}
