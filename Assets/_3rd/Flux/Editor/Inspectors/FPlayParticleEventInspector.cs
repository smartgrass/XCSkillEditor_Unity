using UnityEngine;
using UnityEditor;

using Flux;

namespace FluxEditor
{
	[CustomEditor(typeof(FPlayParticleEvent))]
	public class FPlayParticleEventInspector : FEventInspector {

		private FPlayParticleEvent _playParticleEvent = null;

		private SerializedProperty _randomSeed = null;

		protected override void OnEnable()
		{
			base.OnEnable();
			_playParticleEvent = (FPlayParticleEvent)target;

			_randomSeed = serializedObject.FindProperty("_randomSeed");
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			serializedObject.Update();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PropertyField(_randomSeed);
			if( GUILayout.Button(new GUIContent("R", "Randomize Seed"), GUILayout.Width(25)) )
				_randomSeed.intValue = Random.Range(0, int.MaxValue);
			EditorGUILayout.EndHorizontal();

			if( _randomSeed.intValue == 0 )
			{
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.HelpBox("A random seed of 0 means that it always randomizes the particle system, which breaks backwards play of particle systems", MessageType.Warning);
				if( GUILayout.Button("Fix", GUILayout.Width(40), GUILayout.Height(40)) )
					_randomSeed.intValue = Random.Range(0, int.MaxValue);
				EditorGUILayout.EndHorizontal();
			}

			serializedObject.ApplyModifiedProperties();

			ParticleSystem particleSystem = _playParticleEvent.Owner.GetComponentInChildren<ParticleSystem>();

			if( particleSystem == null )
			{
				EditorGUILayout.HelpBox(string.Format("GameObject {0} doesn't have a ParticleSystem attached.", _playParticleEvent.Owner.name), MessageType.Warning);
				return;
			}

			if( _playParticleEvent.LengthTime > particleSystem.main.duration || _playParticleEvent.LengthTime > particleSystem.main.startLifetime.constant )
			{
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.HelpBox("Particle system's duration and/or startLifetime is smaller than the length of the event, which will make backwards play of the particle system broken.", MessageType.Warning);
				if( GUILayout.Button("Fix", GUILayout.Width(40), GUILayout.Height(40)) )
				{
					int selectedOption = EditorUtility.DisplayDialogComplex("Fix Particle Event?", "Unfortunately, Unity's particle system doesn't fully support playing backwards. " +
						"How would you like to correct this?" +
						"\n - Increase Particle Duration: make particle duration the same as the length of the event" +
						"\n - Shrink Event: resize event to be the length of the particle system", "Increase Particle Duration", "Cancel", "Shrink Event");
					switch( selectedOption )
					{
					case 0: // Increase duration
						SerializedObject particleSystemSO = new SerializedObject(particleSystem);
						particleSystemSO.FindProperty("lengthInSec").floatValue = _playParticleEvent.LengthTime;
						particleSystemSO.FindProperty("InitialModule.startLifetime.scalar").floatValue = _playParticleEvent.LengthTime;
						particleSystemSO.ApplyModifiedProperties();
						break;
					case 1: // Cancel
						break;
					case 2: // Shrink event
						FUtility.Rescale( _playParticleEvent, 
							new FrameRange(_playParticleEvent.FrameRange.Start, 
								Mathf.RoundToInt( Mathf.Min(particleSystem.main.duration, particleSystem.main.startLifetime.constant)  * _playParticleEvent.Sequence.FrameRate)) );
						break;
					}
				}
				EditorGUILayout.EndHorizontal();
			}
		}
	}
}