using UnityEngine;
using System;
using System.Reflection;

namespace Flux
{
	//[FEvent("Script/Call Function")]
	public class FCallFunctionEvent : FEvent {

		public const BindingFlags METHOD_FLAGS = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

		[SerializeField]
		[Tooltip("If false, it gets called every tick")]
		private bool _callOnlyOnTrigger = true;
		public bool CallOnlyOnTrigger { get { return _callOnlyOnTrigger; } set { _callOnlyOnTrigger = value; } }

		[SerializeField]
		[HideInInspector]
		private string _className = null;
		public string ClassName { get { return _className; } set { _className = value; } }

		[SerializeField]
		[HideInInspector]
		private string _methodName = null;
		public string MethodName { get { return _methodName; } set { _methodName = value; } }

		private Type _classType = null;
		private object _scriptReference = null;
		private MethodInfo _methodInfo = null;

		protected override void OnInit()
		{
			_classType = GetType(_className);
			if (_classType != null)
			{
				_scriptReference = Owner.GetComponent(_classType);
				if (_scriptReference != null)
				{
					#if NETFX_CORE
					_methodInfo = TypeExtensions.GetMethod( _classType, _methodName, METHOD_FLAGS );
					#else
					_methodInfo = _classType.GetMethod(_methodName, METHOD_FLAGS);
					#endif
				}
			}
		}

		protected override void OnTrigger( float timeSinceTrigger )
		{
			CallFunction();
		}

		protected override void OnUpdateEvent( float timeSinceTrigger )
		{
			if( _callOnlyOnTrigger )
				return;
			CallFunction();
		}

		private Type GetType(string className)
		{
			Type type = Type.GetType(className);

			if( type == null )
			{
				Assembly[] assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
				for(int i = 0; i != assemblies.Length; ++i )
				{
					type = assemblies[i].GetType(className);
					if( type != null )
						break;
				}
			}


			return type;
		}

		private void CallFunction()
		{
			if( _methodInfo != null )
			{
				if( _methodInfo.IsStatic )
					_methodInfo.Invoke( null, null );
				else
					_methodInfo.Invoke( _scriptReference, null );
			}
		}
	}
}
