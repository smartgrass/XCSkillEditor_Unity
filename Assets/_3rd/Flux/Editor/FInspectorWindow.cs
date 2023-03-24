using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

using Flux;

namespace FluxEditor
{
	public class FInspectorWindow : EditorWindow {

		public static FInspectorWindow _instance = null;

		[MenuItem(FSequenceEditorWindow.MENU_PATH+FSequenceEditorWindow.PRODUCT_NAME+"/Open Inspector", false, 1)]
		public static void Open()
		{
			_instance = GetWindow<FInspectorWindow>();

			_instance.Show();
			_instance.titleContent = new GUIContent("Flux Inspector");

		}

		private Vector2 _scroll = Vector2.zero;

		private FSequenceEditor _sequenceEditor = null;
		public void SetSequenceEditor( FSequenceEditor sequenceEditor ){ _sequenceEditor = sequenceEditor; }

		private Rect _viewRect;
		
		void OnEnable()
		{
			_instance = this;

			hideFlags = HideFlags.DontSave;

			wantsMouseMove = true;

			autoRepaintOnSceneChange = true;
		}
		
		void OnDestroy()
		{
		}
		

		public void Render( Rect rect )
		{
			float contentWidth = rect.width;
			
			contentWidth -= rect.height < _viewRect.height ? 20 : 8;
			
			_scroll = GUI.BeginScrollView( rect, _scroll, _viewRect );

			EditorGUI.BeginChangeCheck();

			GUI.skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Scene);


			_sequenceEditor.EventSelection.OnInspectorGUI( contentWidth );

			if ( _sequenceEditor.EventSelection.Editors.Count > 0)
            {
				GUILayout.Space(10);
			}

			_sequenceEditor.TrackSelection.OnInspectorGUI( contentWidth );

			_sequenceEditor.TimelineSelection.OnInspectorGUI( contentWidth );

			_sequenceEditor.ContainerSelection.OnInspectorGUI( contentWidth );

			if( EditorGUI.EndChangeCheck() )
			{
				_sequenceEditor.Repaint();
			}

			GUILayout.Space(1);

			if( Event.current.type == EventType.Repaint )
			{
				Rect lastElementRect = GUILayoutUtility.GetLastRect();
				
				_viewRect = rect;
				
				_viewRect.height = Mathf.Max( _viewRect.height, lastElementRect.y + lastElementRect.height );
			}


			GUI.EndScrollView();
		}


		void OnGUI()
		{
			if( _sequenceEditor == null )
				return;
			Rect rect = position;
			rect.x = 0; rect.y = 0;
            Render( rect );
		}

		void Update()
		{
			if( _sequenceEditor == null && FSequenceEditorWindow.instance != null )
			{
				_sequenceEditor = FSequenceEditorWindow.instance.GetSequenceEditor();
			}

			if( _sequenceEditor != null && (_sequenceEditor.EventSelection.IsDirty
			   || _sequenceEditor.TrackSelection.IsDirty 
			   || _sequenceEditor.TimelineSelection.IsDirty 
			   || _sequenceEditor.ContainerSelection.IsDirty) )
			{
				Repaint();
			}
		}
	}
}