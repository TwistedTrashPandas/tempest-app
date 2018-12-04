using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace MastersOfTempest.PlayerControls
{
    /// <summary>
    /// This singleton provides access to the UI canvas and allows spawning of UI prefabs
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public Canvas MainCanvas;

        private static UIManager instance;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                if (MainCanvas == null)
                {
                    throw new InvalidOperationException($"{nameof(MainCanvas)} is not specified!");
                }
            }
            else
            {
                Destroy(this);
            }
        }

        public static UIManager GetInstance()
        {
            if (instance != null)
            {
                return instance;
            }
            else
            {
                throw new InvalidOperationException($"Scene doesn't contain {nameof(UIManager)}!");
            }
        }

        /// <summary>
        /// Instantiates given prefab and attaches it to the canvas
        /// </summary>
        /// <param name="prefab">Prefab to instantiate</param>
        /// <typeparam name="T">MonoBehaviour with attached RectTransform</typeparam>
        /// <returns>Returns instance</returns>
        public T SpawnUIElement<T>(T prefab) where T : MonoBehaviour
        {
            var instance = Instantiate(prefab);
            instance.transform.SetParent(MainCanvas.transform, false);
            return instance;
        }

        /// <summary>
        /// Loads prefab from resources and instantiates it on the Canvas
        /// </summary>
        /// <param name="path">Path to the prefab in the Resources folder</param>
        /// <typeparam name="T">MonoBehaviour with RectTransform</typeparam>
        /// <returns>Returns instance</returns>
        public T SpawnUIElement<T>(string path) where T : MonoBehaviour
        {
            var prefab = Resources.Load<T>(path);
            if (prefab == null)
            {
                throw new ArgumentException($"There is no prefab {nameof(T)} under the path {path} in Resources!", nameof(path));
            }
            return SpawnUIElement(prefab);
        }
    }
}
