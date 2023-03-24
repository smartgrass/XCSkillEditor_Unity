using UnityEngine;
using UnityEditor;

using System;

using Flux;
using FluxEditor;

namespace FluxEditor
{
	[CustomEditor(typeof(FTrack), true)]
	public class FTrackInspector : Editor {

		private SerializedProperty _events = null;

		private bool _allTracksSameType = true;

		private bool _showEvents = true;
		public bool ShowEvents { get { return _showEvents; } set { _showEvents = value; } }

		public virtual void OnEnable()
		{
			if( target == null )
				return;

			FTrack track = (FTrack)target;

			Type trackType = track.GetType();

			for( int i = 0; i != targets.Length; ++i )
			{
				if( trackType != targets[i].GetType() )
				{
					_allTracksSameType = false;
					break;
				}
			}

			if( _allTracksSameType )
			{
				_events = serializedObject.FindProperty("_events");
			}
			else
				_showEvents = false;
		}

		public override void OnInspectorGUI()
		{
            if (_allTracksSameType)
                base.OnInspectorGUI();

            FTrack track = (FTrack)target;
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Timeline", track.Timeline, typeof(UnityEngine.Object));
            EditorGUILayout.ObjectField("Track", track, typeof(UnityEngine.Object));
            GUI.enabled = true;

            EditorGUI.BeginChangeCheck();
			bool enabled = EditorGUILayout.Toggle( "Enabled", ((FTrack)target).enabled );
			if( EditorGUI.EndChangeCheck() )
			{
				Undo.RecordObject( target, (enabled ? "enable" : "disable") + " Track" );
				track.enabled = enabled;
				EditorUtility.SetDirty( target );
			}

			EditorGUI.BeginChangeCheck();
			string newName = EditorGUILayout.TextField( "Name",  target.name );
			if( EditorGUI.EndChangeCheck() )
			{
				Undo.RecordObject( track.gameObject, "rename Track" );
				target.name = newName;
				EditorUtility.SetDirty( target );
			}

			if( track.AllowedCacheMode != track.RequiredCacheMode )
			{
				EditorGUI.BeginChangeCheck();
				CacheMode cacheMode = (CacheMode)EditorGUILayout.EnumFlagsField( "Cache Mode", track.CacheMode );
				if( EditorGUI.EndChangeCheck() )
				{
					Undo.RecordObject( track, "change Cache Mode" );
					track.CacheMode = (cacheMode | track.RequiredCacheMode) & track.AllowedCacheMode;
					EditorUtility.SetDirty( target );
				}
			}

			if( _showEvents && _events != null )
			{
				serializedObject.Update();
				EditorGUILayout.PropertyField( _events, true );
				serializedObject.ApplyModifiedProperties();
			}
		}
	}
}
