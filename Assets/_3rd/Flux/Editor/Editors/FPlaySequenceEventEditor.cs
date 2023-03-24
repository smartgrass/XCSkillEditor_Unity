using UnityEngine;
using UnityEditor;

using Flux;

namespace FluxEditor
{
	[FEditor(typeof(FPlaySequenceEvent))]
	public class FPlaySequenceEventEditor : FEventEditor {

		private FSequenceEditor _sequenceEditor = null;

		public override void Init( FObject obj, FEditor owner )
		{
			base.Init(obj, owner);

			if( _sequenceEditor == null )
			{
				_sequenceEditor = FSequenceEditor.CreateInstance<FSequenceEditor>();
				_sequenceEditor.Init( (EditorWindow)null ); // doesn't have a window
				_sequenceEditor.OpenSequence( Evt.Owner.GetComponent<FSequence>() );
			}
        }

    }
}
