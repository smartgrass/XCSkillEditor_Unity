using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

using Flux;

namespace FluxEditor
{
	[Serializable]
	public abstract class FEditorInspector<FEDITOR,FOBJECT> where FEDITOR : FEditor where FOBJECT : FObject {

		public virtual string Title { get { return "Inspetor"; } }

		[SerializeField]
		protected List<FEDITOR> _editors = new List<FEDITOR>();
		public List<FEDITOR> Editors { get { return _editors; }}

		[SerializeField]
		protected List<FOBJECT> _objects = new List<FOBJECT>();
		public List<FOBJECT> Objects { get { return _objects; }}

		[SerializeField]
		public Editor _inspector = null;

		[SerializeField]
		protected FMultiTypeInspector<FOBJECT> _multiTypeInspector = null;

		private bool _allSameType = true;
		public bool AllSameType { get { return _allSameType; } }

		private bool _isDirty = false;
		public bool IsDirty { get { return _isDirty; } }

		protected virtual FMultiTypeInspector<FOBJECT> CreateMultiTypeInspector(){ return null; }

		//		public void SetEditors( List<FEDITOR> editors )
//		{
//			_editors = editors;
//			Refresh();
//		}

		public void Add( FEDITOR editor )
		{
			_editors.Add( editor );
			_isDirty = true;
		}

		public void Remove( FEDITOR editor )
		{
			_editors.Remove( editor );
			_isDirty = true;
		}

		public void Clear()
		{
			_editors.Clear();
			_isDirty = true;
		}

		public virtual void Refresh()
		{
			_objects.Clear();

			_allSameType = true;

			for( int i = 0; i != _editors.Count; ++i )
			{
				if( !_editors[i].IsSelected )
					_editors[i].OnSelect();

				FOBJECT obj = (FOBJECT)_editors[i].Obj;
				_objects.Add( obj );

				if( obj.GetType() != _editors[0].Obj.GetType() )
					_allSameType = false;
			}

			if( _inspector != null )
			{
				Editor.DestroyImmediate( _inspector );
			}

			if( _multiTypeInspector != null )
				ScriptableObject.DestroyImmediate( _multiTypeInspector );

			if( _objects.Count > 0 )
			{
				if( _allSameType )
					_inspector = Editor.CreateEditor( _objects.ToArray() );
				else
					_multiTypeInspector = CreateMultiTypeInspector();
			}

			_isDirty = false;
		}

		public virtual void OnInspectorGUI( float contentWidth )
		{
			if( IsDirty )
				Refresh();
            if ( _objects.Count > 0 )
			{
				EditorGUILayout.BeginVertical(EditorStyles.textArea, GUILayout.Width(contentWidth));
				EditorGUILayout.LabelField(Title, EditorStyles.boldLabel);
				if( _inspector != null && _inspector.target != null )
					_inspector.OnInspectorGUI();
				else if( _multiTypeInspector != null )
					_multiTypeInspector.OnInspectorGUI();
				EditorGUILayout.EndVertical();
			}
		}
	}
}
