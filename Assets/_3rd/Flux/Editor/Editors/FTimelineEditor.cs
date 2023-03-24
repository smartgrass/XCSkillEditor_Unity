using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditorInternal;
using System;
using System.Collections.Generic;

using Flux;

namespace FluxEditor
{
	public class FTimelineEditor : FEditorList<FTrackEditor>
	{
		public const int HEADER_HEIGHT = 25;

		public FContainerEditor ContainerEditor { get { return (FContainerEditor)Owner; } }

		public FTimeline Timeline { get { return (FTimeline)Obj; } }

		public bool _showHeader = true;
		public bool _showTracks = true;


		public override void Init( FObject obj, FEditor owner )
		{
            //Debug.Log("yns FTimelineEditor init ");
			base.Init( obj, owner );

            if(Timeline.Owner&& Timeline.ownerFind != OwnerFind.ID && PrefabUtility.GetPrefabAssetType(Timeline.Owner) == PrefabAssetType.Regular)
            {
                //Timeline.FindOwnerKey = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(Timeline.Owner).AssetPathToResPath();
            }
			if( Timeline.Owner == null )
				Timeline.Awake();

			Editors.Clear();

			List<FTrack> tracks = Timeline.Tracks;

			for( int i = 0; i < tracks.Count; ++i )
			{
				FTrack track = tracks[i];
				FTrackEditor trackEditor = ContainerEditor.SequenceEditor.GetEditor<FTrackEditor>(track);
				trackEditor.Init( track, this );
				Editors.Add( trackEditor );
			}

			_icon = new GUIContent(FUtility.GetFluxTexture( "Plus.png" ));
		}

		public void OnStop()
		{
			for( int i = 0; i != Editors.Count; ++i)
            {
				Editors[i].OnStop();
			}

		}
			
		public override float Height {
			get {
				float headerHeight = _showHeader ? HEADER_HEIGHT : 0;
				float tracksHeight = 0;
				foreach( FTrackEditor trackEditor in Editors )
					tracksHeight += trackEditor.Height;
				tracksHeight *= ShowPercentage;
				return headerHeight + tracksHeight;
			}
		}

		public override float HeaderHeight {
			get {
				return HEADER_HEIGHT;
			}
		}

		protected override string HeaderText {
			get {
				return Timeline.Owner != null ? Timeline.Owner.name : Timeline.name + " (Missing)";//Obj.Owner.name;
			}
		}

		protected override bool IconOnLeft {
			get {
				return false;
			}
		}

		public FTrackEditor GetTrackEditor( Vector2 pos )
		{
			if( !_showTracks 
			   || !Rect.Contains( pos ) 
			   || (_showHeader && pos.y < Rect.yMin + HEADER_HEIGHT) ) return null;

			for( int i = 0; i != Editors.Count; ++i )
			{
				if( Editors[i].Rect.Contains( pos ) )
					return Editors[i];
			}

			return null; // shouldn't happen
		}

		protected override Color BackgroundColor {
			get {
				return FGUI.GetTimelineColor();
			}
		}

		protected override bool CanPaste(FObject obj)
		{
			// since Unity Objs can be "fake null"
			return obj != null && obj is FTrack; 
		}

		protected override void Paste(object obj)
		{
			if( !CanPaste(obj as FObject) )
				return;

			Undo.RecordObject( Timeline, string.Empty );

			FTrack track = Instantiate<FTrack>((FTrack)obj);
			track.hideFlags = Timeline.hideFlags;
			Timeline.Add( track );
			Undo.RegisterCreatedObjectUndo( track.gameObject, "Paste Track " + ((FTrack)obj).name );

		}

		protected override void Delete()
		{
			SequenceEditor.DestroyEditor( this );
		}

		protected override void OnHeaderInput (Rect labelRect, Rect iconRect)
		{
			if( Event.current.type == EventType.MouseDown && Event.current.clickCount > 1 && labelRect.Contains( Event.current.mousePosition ) )
			{
				Selection.activeTransform = Timeline.Owner;
				Event.current.Use();
			}
			base.OnHeaderInput(labelRect, iconRect);

			if( Event.current.type == EventType.MouseDown && iconRect.Contains(Event.current.mousePosition) )
			{
				ShowAddTrackMenu();
			}
		}

		private void ShowAddTrackMenu()
		{
			Event.current.Use();

			GenericMenu menu = new GenericMenu();

			System.Reflection.Assembly fluxAssembly = typeof(FEvent).Assembly;

			Type[] types = typeof(FEvent).Assembly.GetTypes();

			if( fluxAssembly.GetName().Name != "Assembly-CSharp" )
			{
				// if we are in the flux trial, basically allow to get the types in the project assembly
				ArrayUtility.AddRange<Type>( ref types, System.Reflection.Assembly.Load("Assembly-CSharp").GetTypes() );
			}

			List<KeyValuePair<Type, FEventAttribute>> validTypeList = new List<KeyValuePair<Type, FEventAttribute>>();
			
			foreach( Type t in types )
			{
				if( !typeof(FEvent).IsAssignableFrom( t ) )
					continue;
				
				object[] attributes = t.GetCustomAttributes(typeof(FEventAttribute), false);
				if( attributes.Length == 0 || ((FEventAttribute)attributes[0]).menu == null )
					continue;
				
				validTypeList.Add( new KeyValuePair<Type, FEventAttribute>(t, (FEventAttribute)attributes[0]) );
			}
			
			validTypeList.Sort( delegate(KeyValuePair<Type, FEventAttribute> x, KeyValuePair<Type, FEventAttribute> y) 
			                   {
				return x.Value.menu.CompareTo( y.Value.menu );
			});
			
			foreach( KeyValuePair<Type, FEventAttribute> kvp in validTypeList )
			{
				menu.AddItem( new GUIContent(kvp.Value.menu), false, AddTrackMenu, kvp );
			}
			
			menu.ShowAsContext();
		}


		protected override void PopulateContextMenu( GenericMenu menu )
		{
			base.PopulateContextMenu( menu );
			if( Selection.activeTransform != null && Selection.activeTransform != Timeline.Owner )
				menu.AddItem( new GUIContent("Change Owner to " + Selection.activeTransform.name), false, ChangeOwner, Selection.activeTransform );
			else
				menu.AddDisabledItem( new GUIContent( "Change Owner") );

			if( CanPaste( FSequenceEditor.CopyObject ) )
			{
				menu.AddItem( new GUIContent("Paste " + FSequenceEditor.CopyObject.name), false, Paste, FSequenceEditor.CopyObject );
			}

			menu.AddSeparator(null);
		}

		void AddTrackMenu( object param )
		{
			KeyValuePair<Type, FEventAttribute> kvp = (KeyValuePair<Type, FEventAttribute>)param;

			Undo.RecordObjects( new UnityEngine.Object[]{Timeline, this}, "add Track" );

			FTrack track = (FTrack)typeof(FTimeline).GetMethod("Add", new Type[]{typeof(FrameRange)}).MakeGenericMethod( kvp.Key ).Invoke( Timeline, new object[]{SequenceEditor.ViewRange} );

			string evtName = track.gameObject.name;

			int nameStart = 0;
			int nameEnd = evtName.Length;
			if( nameEnd > 2 && evtName[0] == 'F' && char.IsUpper(evtName[1]) )
				nameStart = 1;
			if( evtName.EndsWith("Event") )
				nameEnd = evtName.Length - "Event".Length;
			evtName = evtName.Substring( nameStart, nameEnd - nameStart );

			track.gameObject.name = ObjectNames.NicifyVariableName( evtName );
            track.OnEditorInit();
			if( !Timeline.Sequence.IsStopped )
				track.Init();

			SequenceEditor.Refresh();

			Undo.RegisterCreatedObjectUndo( track.gameObject, string.Empty );

			SequenceEditor.SelectExclusive( SequenceEditor.GetEditor<FEventEditor>( track.GetEvent(0) ) );
		}

		void ChangeOwner( object newOwnerTransform )
		{
			Transform newOwner = (Transform)newOwnerTransform;
			Undo.RecordObject( Timeline, "Change Timeline Owner" );
			Timeline.SetOwner( newOwner );

			if( !SequenceEditor.Sequence.IsStopped )
				Timeline.Init();
		}

		void DuplicateTimeline()
		{
			UnityEngine.Object[] objsToSave = new UnityEngine.Object[]{ SequenceEditor, Timeline.Container };
			Undo.RecordObjects( objsToSave, string.Empty );
			GameObject duplicateTimeline = (GameObject)Instantiate( Timeline.gameObject );
			duplicateTimeline.name = Timeline.gameObject.name;
			Undo.SetTransformParent( duplicateTimeline.transform, Timeline.Container.transform, string.Empty );
			Undo.RegisterCreatedObjectUndo( duplicateTimeline, "duplicate Timeline" );

			if( !SequenceEditor.Sequence.IsStopped )
				duplicateTimeline.GetComponent<FTimeline>().Init();
		}

		void DeleteTimeline()
		{
			UnityEngine.Object[] objsToSave = new UnityEngine.Object[]{ Timeline.Container, Timeline };
			Undo.RegisterCompleteObjectUndo( objsToSave, string.Empty );
			OnDelete();
			Undo.SetTransformParent( Timeline.transform, null, string.Empty );
			Timeline.Container.Remove( Timeline );
			Undo.DestroyObjectImmediate( Timeline.gameObject );
		}


		public override FSequenceEditor SequenceEditor { get { return ContainerEditor != null ? ContainerEditor.SequenceEditor : null; } }

		public void UpdateTracks( int frame, float time )
		{
			for( int i = 0; i != Editors.Count; ++i )
			{
				if( !Editors[i].Track.enabled ) continue;
				Editors[i].UpdateEventsEditor( frame, time );
			}
		}
	}
}
