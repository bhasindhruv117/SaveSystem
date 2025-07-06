using System;
using UnityEngine;

namespace SaveSystem.Core
{
    /// <summary>
    /// Extension methods for the save system
    /// </summary>
    public static class SaveSystemExtensions
    {
        /// <summary>
        /// Register a module with the save manager
        /// </summary>
        public static void RegisterWithSaveManager(this SaveModule module)
        {
            SaveManager.Instance.RegisterModule(module);
        }
        
        /// <summary>
        /// Request that a module be saved
        /// </summary>
        public static void RequestSave(this SaveModule module)
        {
            SaveManager.Instance.RequestSave(module.SaveId);
        }
        
        /// <summary>
        /// Add a MonoBehaviour component and register it with the save manager
        /// </summary>
        public static T AddSaveComponent<T>(this GameObject gameObject) where T : Component, ISavable
        {
            T component = gameObject.AddComponent<T>();
            if (component is SaveModule saveModule)
            {
                SaveManager.Instance.RegisterModule(saveModule);
            }
            return component;
        }
    }
}
