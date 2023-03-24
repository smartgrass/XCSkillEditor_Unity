using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace XiaoCao
{
    public class XCGameManager : MonoSingleton<XCGameManager>
    {
        public void Start()
        {
            SceneManager.sceneLoaded += SceneLoadedFinish;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= SceneLoadedFinish;
        }

        private void SceneLoadedFinish(Scene arg0, LoadSceneMode arg1)
        {
            Debug.Log($"yns SceneLoaded {arg0.name} ");
        }
    }
}