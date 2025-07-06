using UnityEngine;
using UnityEditor;
using SaveSystem.Core;
using System.IO;

namespace SaveSystem.Editor
{
    [CustomEditor(typeof(SaveManager))]
    public class SaveManagerEditor : UnityEditor.Editor
    {
        private bool showDebugOptions = false;
        
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            SaveManager saveManager = (SaveManager)target;
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Save System Tools", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Open Save Directory"))
            {
                if (!Directory.Exists(saveManager.SaveDirectory))
                {
                    Directory.CreateDirectory(saveManager.SaveDirectory);
                }
                EditorUtility.RevealInFinder(saveManager.SaveDirectory);
            }
            
            EditorGUILayout.Space(5);
            
            showDebugOptions = EditorGUILayout.Foldout(showDebugOptions, "Debug Options");
            if (showDebugOptions)
            {
                EditorGUI.indentLevel++;
                
                if (GUILayout.Button("Force Save All Pending Modules"))
                {
                    saveManager.ForceSave();
                }
                
                if (GUILayout.Button("Load All Modules"))
                {
                    saveManager.LoadAllModules();
                }
                
                EditorGUILayout.HelpBox("Warning: Deleting save data cannot be undone.", MessageType.Warning);
                if (GUILayout.Button("Clear All Save Data"))
                {
                    if (EditorUtility.DisplayDialog("Confirm Delete", 
                        "Are you sure you want to delete all save data? This cannot be undone.", 
                        "Yes, delete all", "Cancel"))
                    {
                        DeleteAllSaveData(saveManager.SaveDirectory);
                    }
                }
                
                EditorGUI.indentLevel--;
            }
        }
        
        private void DeleteAllSaveData(string saveDirectory)
        {
            if (Directory.Exists(saveDirectory))
            {
                string[] files = Directory.GetFiles(saveDirectory, "*.save");
                foreach (string file in files)
                {
                    File.Delete(file);
                }
                
                Debug.Log($"Deleted {files.Length} save files");
            }
        }
    }
}
