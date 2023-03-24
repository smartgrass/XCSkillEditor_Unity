using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace FluxEditor
{
	[CustomEditor(typeof(FSettings))]
	public class FSettingsInspector : Editor {

		private FSettings _fluxSettings = null;
		private Texture2D _plusTexture = null;
		private Texture2D _minusTexture = null;

		private const string EVENT_COLOR_MSG = "The name you associate with the color needs to be the full "
			+ "path of the event type, i.e. Namespace.EventType, e.g. Flux.FPlayAnimationEvent";

		private const string CONTAINER_COLOR_MSG = "Default containers can be added by right clicking the add "
			+ "container button in the Flux editor window.";

		void OnEnable()
		{
			_fluxSettings = (FSettings)target;
			_plusTexture = FUtility.GetFluxTexture( "Plus.png" );
			_minusTexture = FUtility.GetFluxTexture( "Minus.png" );
			
		}

		public override void OnInspectorGUI ()
		{
			GUIStyle centeredLabel = new GUIStyle( EditorStyles.largeLabel );
			centeredLabel.alignment = TextAnchor.MiddleCenter;
			GUILayout.Label( "Flux Color Settings", centeredLabel );

			EditorGUI.BeginChangeCheck();

			RenderColorList( "Event Colors", _fluxSettings.EventColors, "<Flux.EventType>", FGUI.GetEventColor(), EVENT_COLOR_MSG );

			GUILayout.Space(10);

			RenderColorList( "Default Containers", _fluxSettings.DefaultContainers, "<Container Name>", Flux.FContainer.DEFAULT_COLOR, CONTAINER_COLOR_MSG );

			if( EditorGUI.EndChangeCheck() )
			{
				RebuildSettingsCache();
			}
		}

		private void RenderColorList( string title, List<FColorSetting> colors, string defaultName, Color defaultColor, string helpStr )
		{
			EditorGUI.BeginChangeCheck();

			EditorGUILayout.BeginVertical("box");
			EditorGUILayout.BeginHorizontal();
			GUIContent label = new GUIContent( title, helpStr );
			GUILayout.Label( label );
			if( GUILayout.Button( _plusTexture, GUILayout.Width(40), GUILayout.Height(16) ) )
				colors.Add( new FColorSetting(defaultName, defaultColor) );
			EditorGUILayout.EndHorizontal();

			for( int i = 0; i != colors.Count; ++i )
			{
				EditorGUILayout.BeginHorizontal();
				colors[i]._str = EditorGUILayout.TextField( colors[i]._str );
				colors[i]._color = EditorGUILayout.ColorField( colors[i]._color );
				if( GUILayout.Button( _minusTexture, GUILayout.Width(40), GUILayout.Height(16) ) )
				{
					colors.RemoveAt( i );
					RebuildSettingsCache();
					EditorGUIUtility.ExitGUI();
				}
				EditorGUILayout.EndHorizontal();
			}
//			EditorGUILayout.HelpBox( helpStr, MessageType.Info );
			EditorGUILayout.EndVertical();

			if( EditorGUI.EndChangeCheck() )
				EditorUtility.SetDirty( _fluxSettings );
		}

		private void RebuildSettingsCache()
		{
			_fluxSettings.Init();
			if( FSequenceEditorWindow.instance != null )
				FSequenceEditorWindow.instance.Repaint();
		}
	}
}
