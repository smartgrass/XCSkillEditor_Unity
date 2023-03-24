using UnityEngine;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEditor.Animations;

using System;
using System.Reflection;

namespace FluxEditor
{
	public static class AnimatorWindowProxy 
	{
		public static Type ANIMATOR_WINDOW_TYPE = typeof(Graph).Assembly.GetType("UnityEditor.Graphs.AnimatorControllerTool");

		private static PropertyInfo _animatorController = null;
		public static PropertyInfo AnimatorController {
			get {
				if( _animatorController == null )
					_animatorController = ANIMATOR_WINDOW_TYPE.GetProperty( "animatorController", BindingFlags.Public | BindingFlags.Instance );
				return _animatorController;
			}
		}

		public static EditorWindow OpenAnimatorWindow()
		{
			return EditorWindow.GetWindow( ANIMATOR_WINDOW_TYPE );
		}

		public static void OpenAnimatorWindowWithAnimatorController( AnimatorController controller )
		{
			EditorWindow animatorWindow = OpenAnimatorWindow();

			AnimatorController animatorControllerValue = AnimatorController.GetValue( animatorWindow, null ) as AnimatorController;

			if( animatorControllerValue != controller )
				AnimatorController.SetValue( animatorWindow, controller, null );
		}
	}
}
