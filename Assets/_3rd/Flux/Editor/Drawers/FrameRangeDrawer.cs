using UnityEngine;
using UnityEditor;

using Flux;

[CustomPropertyDrawer(typeof(FrameRange))]
public class FrameRangeDrawer : PropertyDrawer {

	private const int PREFIX_WIDTH = 15;
	private const int ELEMENT_SPACE = 10;
	private const int HALF_ELEMENT_SPACE = ELEMENT_SPACE / 2;

	public override float GetPropertyHeight( SerializedProperty property, GUIContent label )
	{
		return base.GetPropertyHeight (property, label);
	}

	public override void OnGUI( Rect position, SerializedProperty property, GUIContent label )
	{
		SerializedProperty start = property.FindPropertyRelative( "_start" );
		SerializedProperty end = property.FindPropertyRelative( "_end" );

		Rect r = position;
		r.height = EditorGUIUtility.singleLineHeight;
		r.width = EditorGUIUtility.labelWidth;

		EditorGUI.PrefixLabel( r, label );

		float fieldWidth = (position.width - EditorGUIUtility.labelWidth - PREFIX_WIDTH - PREFIX_WIDTH - ELEMENT_SPACE) * 0.5f;

		r.xMin = r.xMax;
		r.width = PREFIX_WIDTH;

		GUI.Label( r, "S:", EditorStyles.label );

		r.xMin = r.xMax;
		r.width = fieldWidth;

		start.intValue = EditorGUI.IntField( r, start.intValue );

		r.xMin = r.xMax + ELEMENT_SPACE;
		r.width = PREFIX_WIDTH;

		GUI.Label( r, "E:", EditorStyles.label );

		r.xMin = r.xMax;
		r.width = fieldWidth;

		end.intValue = EditorGUI.IntField( r, end.intValue );
	}
}
