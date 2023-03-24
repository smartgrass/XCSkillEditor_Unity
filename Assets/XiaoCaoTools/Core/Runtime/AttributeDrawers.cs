using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
#endif

namespace XiaoCao
{


[AttributeUsage(AttributeTargets.Enum | AttributeTargets.Field)]
public class EnumLabelAttribute : PropertyAttribute
{
    public string label;
    public new int[] order = new int[0];
    public EnumLabelAttribute(string label = null)
    {
        this.label = label;
    }

    public EnumLabelAttribute(string label, params int[] order)
    {
        this.label = label;
        this.order = order;
    }

}

public class XCLabelAttribute : PropertyAttribute
{
    public string name;

    /// <summary>
    /// 使字段在Inspector中显示自定义的名称。
    /// </summary>
    /// <param name="name">自定义名称</param>
    public XCLabelAttribute(string name)
    {
        this.name = name;
    }
}

    //public class Vec3CopyAttribute : PropertyAttribute
    //{
    //    public int index;

    //    /// <summary>
    //    /// 0 表示position
    //    /// </summary>
    //    /// <param name="index"></param>
    //    public Vec3CopyAttribute(int index =0)
    //    {
    //        this.index = index;
    //    }
    //}


#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(XCLabelAttribute))]
public class LabelDrawer : PropertyDrawer
{
    private GUIContent _label = null;
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (_label == null)
        {
            string name = (attribute as XCLabelAttribute).name;
            _label = new GUIContent(name);
        }
        EditorGUI.PropertyField(position, property, _label);
    }
}


[CustomPropertyDrawer(typeof(EnumLabelAttribute))]
public class EnumLabelDrawer : PropertyDrawer
{
    private Dictionary<string, string> customEnumNames = new Dictionary<string, string>();


    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SetUpCustomEnumNames(property, property.enumNames);

        if (property.propertyType == SerializedPropertyType.Enum)
        {
            EditorGUI.BeginChangeCheck();
            string[] displayedOptions = property.enumNames
                    .Where(enumName => customEnumNames.ContainsKey(enumName))
                    .Select<string, string>(enumName => customEnumNames[enumName])
                    .ToArray();

            int[] indexArray = GetIndexArray(enumLabelAttribute.order);
            if (indexArray.Length != displayedOptions.Length)
            {
                indexArray = new int[displayedOptions.Length];
                for (int i = 0; i < indexArray.Length; i++)
                {
                    indexArray[i] = i;
                }
            }
            string[] items = new string[displayedOptions.Length];
            items[0] = displayedOptions[0];
            for (int i = 0; i < displayedOptions.Length; i++)
            {
                items[i] = displayedOptions[indexArray[i]];
            }
            int index = -1;
            for (int i = 0; i < indexArray.Length; i++)
            {
                if (indexArray[i] == property.enumValueIndex)
                {
                    index = i;
                    break;
                }
            }
            if ((index == -1) && (property.enumValueIndex != -1)) { SortingError(position, property, label); return; }

            string baseName = enumLabelAttribute.label == null ? property.displayName : enumLabelAttribute.label;

            index = EditorGUI.Popup(position, baseName, index, items);
            if (EditorGUI.EndChangeCheck())
            {
                if (index >= 0)
                    property.enumValueIndex = indexArray[index];
            }
        }
    }

    private EnumLabelAttribute enumLabelAttribute
    {
        get
        {
            return (EnumLabelAttribute)attribute;
        }
    }

    public void SetUpCustomEnumNames(SerializedProperty property, string[] enumNames)
    {
        Type enumType = fieldInfo.FieldType;
        foreach (var enumName in enumNames)
        {
            FieldInfo field = enumType.GetField(enumName);
            EnumLabelAttribute att = field.GetCustomAttribute<EnumLabelAttribute>();
            if (!customEnumNames.ContainsKey(enumName))
            {
                if (att != null)
                {
                    customEnumNames.Add(enumName, att.label);
                }
                else
                {
                    customEnumNames.Add(enumName, enumName);
                }
            }
        }
    }


    int[] GetIndexArray(int[] order)
    {
        int[] indexArray = new int[order.Length];
        for (int i = 0; i < order.Length; i++)
        {
            int index = 0;
            for (int j = 0; j < order.Length; j++)
            {
                if (order[i] > order[j])
                {
                    index++;
                }
            }
            indexArray[i] = index;
        }
        return (indexArray);
    }

    void SortingError(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.PropertyField(position, property, new GUIContent(label.text + " (sorting error)"));
        EditorGUI.EndProperty();
    }
}




#endif

}
