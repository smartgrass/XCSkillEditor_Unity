using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using XiaoCao;

public static class MethodExtension 
{
    static public T GetOrAddComponent<T>(this GameObject go) where T : Component
    {
        T ret = go.GetComponent<T>();
        if (null == ret)
            ret = go.AddComponent<T>();
        return ret;
    }

    public static string GetEnumLabel(this Enum enumValue)
    {
        FieldInfo fieldInfo = enumValue.GetType().GetField(enumValue.ToString());
        EnumLabelAttribute[] attrs =
            fieldInfo.GetCustomAttributes(typeof(EnumLabelAttribute), false) as EnumLabelAttribute[];

        return attrs.Length > 0 ? attrs[0].label : enumValue.ToString();
    }


}
