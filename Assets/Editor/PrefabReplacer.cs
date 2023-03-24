using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class PrefabReplacer : EditorWindow
{

    GameObject prefabToReplace;
    List<GameObject> objectsToReplace = new List<GameObject>();

    [MenuItem("Tools/Prefab Replacer")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow<PrefabReplacer>("Prefab Replacer");
    }

    void OnGUI()
    {
        GUILayout.Label("Prefab Replacer", EditorStyles.boldLabel);
        prefabToReplace = EditorGUILayout.ObjectField("Prefab to Replace", prefabToReplace, typeof(GameObject), false) as GameObject;

        if (GUILayout.Button("Ñ¡ÖÐ"))
        {
            objectsToReplace = new List<GameObject>();
            foreach (GameObject obj in Selection.gameObjects)
            {
                if (PrefabUtility.GetPrefabType(obj) != PrefabType.None)
                {
                    objectsToReplace.Add(obj);
                }
            }
        }
        for (int i = 0; i < objectsToReplace.Count; i++)
        {
            objectsToReplace[i] = (GameObject)EditorGUILayout.ObjectField(objectsToReplace[i], typeof(GameObject), true);
        }


        if (GUILayout.Button("Ìæ»»"))
        {
            if (prefabToReplace == null)
            {
                Debug.LogError("Please select a prefab to replace with.");
                return;
            }

            foreach (GameObject obj in objectsToReplace)
            {
                string path = AssetDatabase.GetAssetPath(obj);
                GameObject newObject = PrefabUtility.InstantiatePrefab(prefabToReplace) as GameObject;
                newObject.transform.position = obj.transform.position;
                newObject.transform.rotation = obj.transform.rotation;
                newObject.transform.localScale = obj.transform.localScale;
                PrefabUtility.SaveAsPrefabAsset(newObject, path);
                DestroyImmediate(obj);
            }
        }
    }
}
