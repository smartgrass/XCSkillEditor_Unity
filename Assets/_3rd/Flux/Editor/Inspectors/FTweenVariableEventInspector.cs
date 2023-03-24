using UnityEngine;
using UnityEditor;

using System;
using System.Reflection;
using System.Collections.Generic;

using Flux;

namespace FluxEditor
{
	public class FTweenVariableEventInspector<T, U> : FTweenEventInspector where T : FTweenVariableEvent<U> where U : FTweenBase
	{	
		private T _tweenVariableEvent = null;

		private string[] _displayOptions = new string[0];
		private string[] _scriptNames = new string[0];
		private string[] _variableNames = new string[0];
		
		private int _selectedScript = 0;
		private int _selectedVariable = 0;

		protected override void OnEnable ()
		{
			base.OnEnable();

			_tweenVariableEvent = (T)target;

			UpdateScriptNames();

			for( int i = 0; i != _scriptNames.Length; ++i )
			{
				if( _scriptNames[i] == _tweenVariableEvent.ClassName )
				{
					_selectedScript = i;
					break;
				}
			}
			
			UpdateVariableNames();
			
			for( int i = 0; i != _variableNames.Length; ++i )
			{
				if( _variableNames[i] == _tweenVariableEvent.VariableName )
				{
					_selectedVariable = i;
					break;
				}
			}
		}


		public override void OnInspectorGUI ()
		{
			base.OnInspectorGUI();

			EditorGUI.BeginChangeCheck();
			_selectedScript = EditorGUILayout.Popup( "Component", _selectedScript, _displayOptions );
			if( EditorGUI.EndChangeCheck() )
			{
				if( _selectedScript == 0 )
					_tweenVariableEvent.ClassName = null;
				else
					_tweenVariableEvent.ClassName = _scriptNames[_selectedScript];
				UpdateVariableNames();
			}

			if( _selectedScript == 0 )
				GUI.enabled = false;

			EditorGUI.BeginChangeCheck();
			_selectedVariable = EditorGUILayout.Popup( "Variable", _selectedVariable, _variableNames );
			if( EditorGUI.EndChangeCheck() )
			{
				if( _selectedVariable == 0 )
					_tweenVariableEvent.VariableName = null;
				else
					_tweenVariableEvent.VariableName = _variableNames[_selectedVariable];
			}

			GUI.enabled = true;
		}

		private void UpdateScriptNames()
		{
			Component[] components = _tweenVariableEvent.Owner.GetComponents<Component>();
			
			string previousScriptName = _selectedScript == 0 || _selectedScript >= _scriptNames.Length ? null : _scriptNames[_selectedScript];
			
			_displayOptions = new string[components.Length+1];
			_scriptNames = new string[components.Length+1];

			_displayOptions[0] = "Choose Component...";
			_scriptNames[0] = "Choose Component...";

			for( int i = 0; i != components.Length; ++i )
			{
            _displayOptions[i + 1] = components[i].GetType().FullName;
            _scriptNames[i + 1] = components[i].GetType().AssemblyQualifiedName;
			}
			
			_selectedScript = 0;
			
			if( previousScriptName != null )
			{
				for( int i = 0; i != _scriptNames.Length; ++i )
				{
					if( _scriptNames[i] == previousScriptName )
					{
						_selectedScript = i;
						break;
					}
				}
			}
			
			UpdateVariableNames();
		}
		
		private void UpdateVariableNames()
		{
			//string typeName = _selectedScript == 0 ? null : _scriptNames[_selectedScript];
         /*
			Assembly assembly = null;
			
			if( typeName != null )
			{
				
				Assembly[] allAssemblies = System.AppDomain.CurrentDomain.GetAssemblies();
				
				foreach( Assembly ass in allAssemblies )
				{
					if( ass.GetType(typeName) != null )
					{
						assembly = ass;
						break;
					}
				}
			}
			*/
         Type scriptType = Type.GetType(_scriptNames[_selectedScript]);// assembly == null ? null : assembly.GetType(_scriptNames[_selectedScript]);
			
			if( scriptType == null )
			{
				_selectedVariable = 0;
				_variableNames = new string[]{ "Choose Variable..." };
				return;
			}
			
			string previousVariableName = _selectedVariable == 0 || _selectedVariable >= _variableNames.Length ? null : _variableNames[_selectedVariable];
			
			FieldInfo[] fields = scriptType.GetFields( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static );

			List<string> variableNames = new List<string>();

			foreach( FieldInfo field in fields )
			{
				if( field.FieldType == ((object)_tweenVariableEvent.Tween).GetType().BaseType.GetGenericArguments()[0] )
					variableNames.Add( field.Name );
			}

			PropertyInfo[] properties = scriptType.GetProperties( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static );

			foreach( PropertyInfo property in properties )
			{
				if( property.CanWrite && property.PropertyType == ((object)_tweenVariableEvent.Tween).GetType().BaseType.GetGenericArguments()[0] )
					variableNames.Add( property.Name );
			}

			_variableNames = new string[variableNames.Count+1];
			_variableNames[0] = "Choose Variable...";
			
			for( int i = 0; i != variableNames.Count; ++i )
			{
				_variableNames[i+1] = variableNames[i];
			}
			
			_selectedVariable = 0;
			
			for( int i = 0; i != _variableNames.Length; ++i )
			{
				if( _variableNames[i] == previousVariableName )
				{
					_selectedVariable = i;
					break;
				}
			}
		}
	}

	[CustomEditor(typeof(FTweenFloatEvent), true)]
	public class FTweenFloatEventInspector : FTweenVariableEventInspector<FTweenFloatEvent, FTweenFloat>
	{
	}

	[CustomEditor(typeof(FTweenColorEvent), true)]
	public class FTweenColorEventInspector : FTweenVariableEventInspector<FTweenColorEvent, FTweenColor>
	{
	}

	[CustomEditor(typeof(FTweenVector2Event), true)]
	public class FTweenVector2EventInspector : FTweenVariableEventInspector<FTweenVector2Event, FTweenVector2>
	{
	}

	[CustomEditor(typeof(FTweenVector3Event), true)]
	public class FTweenVector3EventInspector : FTweenVariableEventInspector<FTweenVector3Event, FTweenVector3>
	{
	}

	[CustomEditor(typeof(FTweenVector4Event), true)]
	public class FTweenVector4EventInspector : FTweenVariableEventInspector<FTweenVector4Event, FTweenVector4>
	{
	}
}
