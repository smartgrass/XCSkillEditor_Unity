using UnityEngine;
using UnityEditor;

using System;
using System.Collections.Generic;
using System.Reflection;

using Flux;

namespace FluxEditor
{
	[CustomEditor(typeof(FCallFunctionEvent))]
	public class FCallFunctionEventInspector : FEventInspector {

		private FCallFunctionEvent _callFunctionEvt = null;

		private string[] _scriptNames = new string[0];
		private string[] _methodNames = new string[0];

		private int _selectedScript = 0;
		private int _selectedMethodName = 0;

		protected override void OnEnable ()
		{
			base.OnEnable();
			_callFunctionEvt = (FCallFunctionEvent)target;

			UpdateScriptNames();

			for( int i = 0; i != _scriptNames.Length; ++i )
			{
				if( _scriptNames[i] == _callFunctionEvt.ClassName )
				{
					_selectedScript = i;
					break;
				}
			}

			UpdateMethodNames();

			for( int i = 0; i != _methodNames.Length; ++i )
			{
				if( _methodNames[i] == _callFunctionEvt.MethodName )
				{
					_selectedMethodName = i;
					break;
				}
			}
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			EditorGUI.BeginChangeCheck();
			_selectedScript = EditorGUILayout.Popup( "Component", _selectedScript, _scriptNames );
			if( EditorGUI.EndChangeCheck() )
			{
				if( _selectedScript == 0 )
					_callFunctionEvt.ClassName = null;
				else
					_callFunctionEvt.ClassName = _scriptNames[_selectedScript];
				UpdateMethodNames();
			}

			if( _selectedScript == 0 )
				GUI.enabled = false;

			EditorGUI.BeginChangeCheck();
			_selectedMethodName = EditorGUILayout.Popup( "Method", _selectedMethodName, _methodNames );
			if( EditorGUI.EndChangeCheck() )
			{
				if( _selectedMethodName == 0 )
					_callFunctionEvt.MethodName = null;
				else
					_callFunctionEvt.MethodName = _methodNames[_selectedMethodName];
			}

			GUI.enabled = true;
		}

		private void UpdateScriptNames()
		{
			Component[] components = _callFunctionEvt.Owner.GetComponents<Component>();

			string previousScriptName = _selectedScript == 0 || _selectedScript >= _scriptNames.Length ? null : _scriptNames[_selectedScript];

			_scriptNames = new string[components.Length+1];
			_scriptNames[0] = "Choose Component...";
			for( int i = 0; i != components.Length; ++i )
			{
				_scriptNames[i+1] = components[i].GetType().ToString();
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

			UpdateMethodNames();
		}

		private void UpdateMethodNames()
		{
			string typeName = _selectedScript == 0 ? null : _scriptNames[_selectedScript];

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

			Type scriptType = assembly == null ? null : assembly.GetType(_scriptNames[_selectedScript]);

			if( scriptType == null )
			{
				_selectedMethodName = 0;
				_methodNames = new string[]{ "Choose Method..." };
				return;
			}

			string previousMethodName = _selectedMethodName == 0 || _selectedMethodName >= _methodNames.Length ? null : _methodNames[_selectedMethodName];

			MethodInfo[] methods = scriptType.GetMethods( FCallFunctionEvent.METHOD_FLAGS );

			List<MethodInfo> validMethods = new List<MethodInfo>();
			foreach( MethodInfo method in methods )
			{
				if( method.GetParameters().Length == 0 && method.ReturnType == typeof(void) )
					validMethods.Add( method );
			}

			_methodNames = new string[validMethods.Count+1];
			_methodNames[0] = "Choose Method...";

			for( int i = 0; i != validMethods.Count; ++i )
			{
				_methodNames[i+1] = validMethods[i].Name;
			}

			_selectedMethodName = 0;

			for( int i = 0; i != _methodNames.Length; ++i )
			{
				if( _methodNames[i] == previousMethodName )
				{
					_selectedMethodName = i;
					break;
				}
			}
		}
	}
}
