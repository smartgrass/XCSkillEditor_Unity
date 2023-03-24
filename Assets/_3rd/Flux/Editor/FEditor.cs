using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using System;
using System.Collections.Generic;

using Flux;

namespace FluxEditor
{
	/**
	 * @brief Base for all the editor classes used in the sequence window.
	 */
	public abstract class FEditor : ScriptableObject, ISelectableElement
	{
		/** @brief Reference to the sequence editor this editor belongs to */
		public abstract FSequenceEditor SequenceEditor{ get; }

		/** @brief Is this element selected? */
		[SerializeField]
		protected bool _isSelected;
		/** @brief Is this element selected? */
		public bool IsSelected { get { return _isSelected; } }

		/** @brief What's the rect used to draw this element? */
//		protected Rect _rect;
		public Rect Rect { 
			get
			{
				return _rect;
			} 
			set
			{
				_rect = value;
			}
		}

		private Rect _rect = new Rect();

		private AnimVector3 _offset = new AnimVector3();
		public AnimVector3 Offset { get { return _offset; } }
		public virtual void ClearOffset()
		{
			_offset.target = _offset.value = Vector3.zero;
		}

		private int _guiId = 0;
		public int GuiId { get { return _guiId; } protected set { _guiId = value; } }

		[SerializeField]
		private FObject _obj = null;
		public FObject Obj { get { return _obj; } set { _obj = value; } }

		private FEditor _owner = null;
		public FEditor Owner { get { return _owner; } }

		public virtual void ReserveGuiIds()
		{
			GuiId = EditorGUIUtility.GetControlID( FocusType.Passive );
		}

		/** @brief Called on selection. */
		public virtual void OnSelect()
		{
			_isSelected = true;
		}

		/** @brief Called on deselection. */
		public virtual void OnDeselect()
		{
			_isSelected = false;
		}

		/** @brief Called when the editor is deleted, e.g. deleting an event
		 * from the track.
		 * @note it is different from OnDestroy because it is called while
		 * the object is still "proper" (e.g. event still belongs to track).
		 */
		public virtual void OnDelete()
		{
		}

//		public virtual float ContentIndent { get { return 0; } }
		protected Vector2 _contentOffset = Vector2.zero;
		public Vector2 ContentOffset { get { return _contentOffset; } }

		protected virtual void OnEnable()
		{
			hideFlags = HideFlags.DontSave;
		}

		protected virtual void OnDestroy()
		{
			_owner = null;
			_offset.valueChanged.RemoveAllListeners();
		}

		public void RefreshRuntimeObject()
		{
			_obj = (FObject)EditorUtility.InstanceIDToObject( _obj.GetInstanceID() );
		}

		/** @brief Inits the editor object.
		 * @param obj CObject the editor manages
		 */
		public virtual void Init( FObject obj, FEditor owner )
		{
			_obj = obj;
			_owner = owner;
			_offset.valueChanged.AddListener(SequenceEditor.Repaint);
		}

		public virtual Rect GetGlobalRect()
		{
			Rect r = Rect;
			FEditor owner = Owner;
			while( owner != null )
			{
				r.x += owner.ContentOffset.x;
				r.y += owner.ContentOffset.y;
				owner = owner.Owner;
			}
			return r;
		}

		public abstract void Render( Rect rect, float headerWidth );

		public abstract float Height { get; }
	}

	/**
	 * @brief Attribute to specify which editor will handle the representation
	 * of a specific FObject class. It works in the same way as Unity's
	 * CustomEditor, but here it is for specifying how that FObject will be 
	 * represented inside sequence window.
	 */
	public class FEditorAttribute : Attribute
	{
		public Type type;
		
		public FEditorAttribute( Type type )
		{
			this.type = type;
		}
	}
}