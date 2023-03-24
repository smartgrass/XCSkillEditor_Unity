using UnityEngine;
using System;
using System.Reflection;

namespace Flux
{
	public abstract class FTweenVariableEvent<T> : FTweenEvent<T> where T : FTweenBase {

		public const BindingFlags VARIABLE_FLAGS = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

		[SerializeField]
		[HideInInspector]
		private string _className = null;
		public string ClassName { get { return _className; } set { _className = value; OnInit(); } }

		[SerializeField]
		[HideInInspector]
		private string _variableName = null;
		public string VariableName { get { return _variableName; } set { _variableName = value; OnInit(); } }

		private Type _classType = null;
		private object _scriptReference = null;

		private FieldInfo _fieldInfo = null;
		private PropertyInfo _propertyInfo = null;

		protected override void OnInit()
		{
			if( _className == null )
				return;
         _classType = Type.GetType(_className);
         /*
                  Assembly assembly = Type.GetType(_className).Assembly;
                  Assembly[] allAssemblies = System.AppDomain.CurrentDomain.GetAssemblies();

                  Assembly assembly = null;

                  foreach (Assembly ass in allAssemblies)
                  {
                     if (ass.GetType(_className) != null)
                     {
                        assembly = ass;
                        break;
                     }
                  }
                  _classType = assembly == null ? null : assembly.GetType( _className );

           */

         _fieldInfo = null;
			_propertyInfo = null;

			if( _classType != null )
			{
				_scriptReference = Owner.GetComponent( _classType );
				if( _scriptReference != null && _variableName != null )
				{
					_fieldInfo = _classType.GetField( _variableName, VARIABLE_FLAGS );

					if( _fieldInfo == null )
					{
						_propertyInfo = _classType.GetProperty( _variableName, VARIABLE_FLAGS );
					}
				}
			}
		}

		protected override void ApplyProperty( float t )
		{
			if( _fieldInfo != null )
			{
				if( _fieldInfo.IsStatic )
				{
					_fieldInfo.SetValue( null, GetValueAt(t) );
				}
				else
				{
					_fieldInfo.SetValue( _scriptReference, GetValueAt(t) );
				}
			}
			else if( _propertyInfo != null )
			{
				_propertyInfo.SetValue( _scriptReference, GetValueAt(t), null );
			}
		}

		protected abstract object GetValueAt( float t );
	}
}
