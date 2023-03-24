using NaughtyAttributes.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace XiaoCao
{

    /// <summary>
    /// 小草做的编辑器窗口  特性只用于编辑器
    /// </summary>
    public class XiaoCaoWindow : EditorWindow
    {
        private Vector2 m_ScrollPosition;

        private IEnumerable<MethodInfo> methods;
        private Dictionary<int, List<ButtonAttribute>> methodDic;
        private List<ButtonAttribute> noPosMethod;  //无顺序方法放最后
        //private int horCount = 1;

        SerializedObject obj;

        public static T OpenWindow<T>(string title = "") where T : XiaoCaoWindow
        {
            if (title.IsEmpty())
            {
                title = typeof(T).Name;
            }
            T win = GetWindow<T>(title);
            win.Show();
            return win;
        }

        public virtual void OnEnable()
        {
            obj = new SerializedObject(new UnityEngine.Object[] { this }, this);
            //获取按钮方法
            methodDic = new Dictionary<int, List<ButtonAttribute>>();
            noPosMethod = new List<ButtonAttribute>();
            methods = this.GetType()
           .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public |
                BindingFlags.NonPublic).Where(o => Attribute.IsDefined(o, typeof(ButtonAttribute)));
            foreach (var item in methods)
            {
                var att = item.GetCustomAttribute<ButtonAttribute>();
                att.method = item;
                if (string.IsNullOrEmpty(att.name))
                    att.name = item.Name;
                if (att.pos > -10)
                {
                    if (!methodDic.ContainsKey(att.pos))
                    {
                        methodDic.Add(att.pos, new List<ButtonAttribute>() { att });
                    }
                    else
                    {
                        methodDic[att.pos].Add(att);
                    }
                }
                else
                    noPosMethod.Add(att);
            }

            //const BindingFlags kInstanceInvokeFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy;
            //updateMethod = GetType().GetMethod("OnValueChange", kInstanceInvokeFlags);
        }


        private void OnGUI()
        {

            EditorGUIUtility.labelWidth = 150;
            //, "OL Box"
            m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition, GUI.skin.horizontalScrollbar, GUI.skin.verticalScrollbar);
            GUIUtility.GetControlID(645789, FocusType.Passive);

            bool modified = DrawInspector();
            EditorGUILayout.EndScrollView();
            if (modified)
            {
                this.Repaint();
                //UpdateView();
            }

            GUILayout.Space(8);
        }

        internal bool DrawInspector()
        {
            EditorGUI.BeginChangeCheck();
            
            obj.UpdateIfRequiredOrScript();
            SerializedProperty property = obj.GetIterator();

            DrawHeader(obj);

            DrawContent(property);
            obj.ApplyModifiedProperties();
            return EditorGUI.EndChangeCheck();
        }

        //绘制头部
        public static void DrawHeader(SerializedObject obj)
        {
            SerializedProperty scriptProperty = obj.FindProperty("m_Script");
            if (scriptProperty != null)
            {
                EditorGUI.BeginDisabledGroup(true);
                //MonoScript targetScript = scriptProperty?.objectReferenceValue as MonoScript;
                //EditorGUILayout.ObjectField("", targetScript, typeof(UnityEngine.Object));
                EditorGUILayout.PropertyField(scriptProperty);
                EditorGUI.EndDisabledGroup();
            }
        }

        //开始绘制
        private void DrawContent(SerializedProperty property)
        {
            bool isExpend = true; //第一次是整个类, 需要展开子项
            int index = 0;
            while (property.NextVisible(isExpend))
            {
                if (!isExpend)
                {
                    if (PropertyUtility.IsVisible(property))
                    {
                        EditorGUI.BeginChangeCheck();


                        EditorGUILayout.PropertyField(property, new GUIContent(PropertyUtility.GetLabel(property)), true);


                        if (EditorGUI.EndChangeCheck())
                        {
                            //EditorUtility.SetDirty(this);
                            //this.SaveChanges();
                            PropertyUtility.CallOnValueChangedCallbacks(property);
                        }
                    }
                }
                isExpend = false; //关闭展开子项

                DrawButtonPos(index);
                index++;
            }

            for (int i = -1; i > -10; i--)
            {
                DrawButtonPos(i);
            }
            DrawEndBtn();
        }


        void DrawEndBtn()
        {
            foreach (var item in noPosMethod)
            {
                if (GUILayout.Button(item.name))
                {
                    item.method.Invoke(this, null);
                }
            }
        }

        void DrawButtonPos(int index)
        {
            if (methodDic.ContainsKey(index))
            {
                EditorGUILayout.Separator();
                EditorGUILayout.BeginHorizontal();
                foreach (var att in methodDic[index])
                {
                    bool visible = ButtonUtility.IsVisible(this, att.method);
                    if (visible)
                    {
                        if (GUILayout.Button(att.name))
                        {
                            att.method.Invoke(this, null);
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
                //EditorGUILayout.Separator();
            }
        }

        public void UpdateView()
        {
            //typeof(EditorUtility).GetMethod("ForceRebuildInspectors").Invoke(null, null);
            this.Repaint();
        }


    }


    #region Attributes
    public class HorLayoutAttribute : Attribute
    {
        public bool isHor;
        /// <summary>
        /// 使字段在Inspector中显示自定义的名称。
        /// </summary>
        /// <param name="name">自定义名称</param>
        public HorLayoutAttribute(bool isHor = true)
        {
            this.isHor = isHor;
        }
    }
    public class ButtonAttribute : Attribute
    {
        public string name;

        public int pos;

        //public bool isHor;

        public MethodInfo method;
        /// <summary>
        /// 使字段在Inspector中显示自定义的名称。
        /// </summary>
        /// <param name="name">自定义名称</param>
        public ButtonAttribute(string name = "", int pos = -10)
        {
            this.name = name;
            this.pos = pos;
        }
    }


    #endregion
}
