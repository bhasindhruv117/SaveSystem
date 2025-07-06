using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace SaveSystem.Core
{
    /// <summary>
    /// Central manager for the save system
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        #region Singleton

        private static SaveManager _instance;
        public static SaveManager Instance 
        {
            get
            {
                if (_instance == null)
                {
                    GameObject saveManagerObject = new GameObject("SaveManager");
                    _instance = saveManagerObject.AddComponent<SaveManager>();
                    DontDestroyOnLoad(saveManagerObject);
                }
                return _instance;
            }
        }

        #endregion

        [SerializeField] private float _autoSaveInterval = 60f;
        [SerializeField] private bool _autoSaveEnabled = true;
        [SerializeField] private string _saveDirectoryName = "Saves";
        
        private Dictionary<string, SaveModule> _registeredModules = new Dictionary<string, SaveModule>();
        private Queue<string> _saveQueue = new Queue<string>();
        private bool _isSaving = false;
        private ISaveSerializer _serializer;
        private float _lastSaveTime;

        /// <summary>
        /// Directory where save files are stored
        /// </summary>
        public string SaveDirectory => Path.Combine(Application.persistentDataPath, _saveDirectoryName);

        /// <summary>
        /// Event triggered when all pending saves are completed
        /// </summary>
        public event Action OnSaveCompleted;
        
        /// <summary>
        /// Event triggered when a module is loaded
        /// </summary>
        public event Action<SaveModule> OnModuleLoaded;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Ensure save directory exists
            if (!Directory.Exists(SaveDirectory))
            {
                Directory.CreateDirectory(SaveDirectory);
            }
        }

        private void Start()
        {
            _lastSaveTime = Time.time;
        }

        private void Update()
        {
            if (_autoSaveEnabled && Time.time - _lastSaveTime >= _autoSaveInterval)
            {
                _lastSaveTime = Time.time;
                ProcessSaveQueue();
            }
        }

        /// <summary>
        /// Set the serializer implementation to use
        /// </summary>
        /// <param name="serializer">The serializer implementation</param>
        public void SetSerializer(ISaveSerializer serializer)
        {
            _serializer = serializer;
        }

        /// <summary>
        /// Register a module with the save system
        /// </summary>
        /// <param name="module">The module to register</param>
        public void RegisterModule(SaveModule module)
        {
            if (module == null)
            {
                Debug.LogError("Cannot register null module");
                return;
            }
            
            if (string.IsNullOrEmpty(module.SaveId))
            {
                Debug.LogError("Module has invalid SaveId");
                return;
            }
            
            _registeredModules[module.SaveId] = module;
            Debug.Log($"Registered save module: {module.SaveId}");
        }

        /// <summary>
        /// Unregister a module from the save system
        /// </summary>
        /// <param name="moduleId">ID of the module to unregister</param>
        public void UnregisterModule(string moduleId)
        {
            if (_registeredModules.ContainsKey(moduleId))
            {
                _registeredModules.Remove(moduleId);
                Debug.Log($"Unregistered save module: {moduleId}");
            }
        }

        /// <summary>
        /// Request a module to be saved
        /// </summary>
        /// <param name="moduleId">ID of the module to save</param>
        public void RequestSave(string moduleId)
        {
            if (!_registeredModules.ContainsKey(moduleId))
            {
                Debug.LogWarning($"Cannot request save for unregistered module: {moduleId}");
                return;
            }
            
            if (!_saveQueue.Contains(moduleId))
            {
                _saveQueue.Enqueue(moduleId);
                Debug.Log($"Module queued for save: {moduleId}");
            }
        }

        /// <summary>
        /// Force processing of the save queue immediately
        /// </summary>
        public async void ForceSave()
        {
            await ProcessSaveQueue();
        }

        /// <summary>
        /// Process all pending saves in the queue
        /// </summary>
        private async Task ProcessSaveQueue()
        {
            if (_isSaving || _saveQueue.Count == 0 || _serializer == null)
            {
                return;
            }
            
            _isSaving = true;
            
            try
            {
                // Process all modules in the queue
                var modulesToSave = new List<string>();
                
                while (_saveQueue.Count > 0)
                {
                    string moduleId = _saveQueue.Dequeue();
                    if (!modulesToSave.Contains(moduleId))
                    {
                        modulesToSave.Add(moduleId);
                    }
                }
                
                // Sort by priority and save each module
                var sortedModules = modulesToSave
                    .Select(id => _registeredModules[id])
                    .OrderBy(m => m.SavePriority)
                    .ToList();
                
                foreach (var module in sortedModules)
                {
                    await SaveModuleAsync(module);
                }
                
                OnSaveCompleted?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error during save process: {ex.Message}");
            }
            finally
            {
                _isSaving = false;
            }
        }

        /// <summary>
        /// Save a specific module
        /// </summary>
        private async Task SaveModuleAsync(SaveModule module)
        {
            try
            {
                // Call before save hook
                module.OnBeforeSave();
                
                // Serialize the module
                string serializedData = _serializer.Serialize(module);
                string filePath = GetSaveFilePath(module.SaveId);
                
                // Write to file asynchronously
                await Task.Run(() => File.WriteAllText(filePath, serializedData));
                
                Debug.Log($"Saved module: {module.SaveId}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error saving module {module.SaveId}: {ex.Message}");
            }
        }

        /// <summary>
        /// Load all registered modules
        /// </summary>
        public void LoadAllModules()
        {
            if (_serializer == null)
            {
                Debug.LogError("Cannot load modules: No serializer set");
                return;
            }
            
            // Build dependency graph
            Dictionary<string, HashSet<string>> dependencyGraph = BuildDependencyGraph();
            
            // Sort modules by load order
            List<SaveModule> sortedModules = GetSortedModules(dependencyGraph);
            
            // Load modules in order
            foreach (var module in sortedModules)
            {
                LoadModule(module.SaveId);
            }
        }

        /// <summary>
        /// Load a specific module
        /// </summary>
        /// <param name="moduleId">ID of the module to load</param>
        /// <returns>True if module was loaded successfully</returns>
        public bool LoadModule(string moduleId)
        {
            if (!_registeredModules.ContainsKey(moduleId))
            {
                Debug.LogWarning($"Cannot load unregistered module: {moduleId}");
                return false;
            }
            
            string filePath = GetSaveFilePath(moduleId);
            if (!File.Exists(filePath))
            {
                Debug.Log($"No save file found for module: {moduleId}");
                return false;
            }
            
            try
            {
                // Read serialized data
                string serializedData = File.ReadAllText(filePath);
                
                // Get the module type
                Type moduleType = _registeredModules[moduleId].GetType();
                
                // Deserialize the module
                SaveModule loadedModule = (SaveModule)_serializer.Deserialize(serializedData, moduleType);
                
                if (loadedModule != null)
                {
                    // Register the loaded module
                    _registeredModules[moduleId] = loadedModule;
                    
                    // Call after load hook
                    loadedModule.OnAfterLoad();
                    
                    // Notify listeners
                    OnModuleLoaded?.Invoke(loadedModule);
                    
                    Debug.Log($"Loaded module: {moduleId}");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error loading module {moduleId}: {ex.Message}");
            }
            
            return false;
        }

        /// <summary>
        /// Delete save data for a specific module
        /// </summary>
        /// <param name="moduleId">ID of the module to delete</param>
        public void DeleteModuleSaveData(string moduleId)
        {
            string filePath = GetSaveFilePath(moduleId);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Debug.Log($"Deleted save data for module: {moduleId}");
            }
        }

        /// <summary>
        /// Get the path to the save file for a module
        /// </summary>
        private string GetSaveFilePath(string moduleId)
        {
            return Path.Combine(SaveDirectory, $"{moduleId}.save");
        }

        /// <summary>
        /// Build a dependency graph for all registered modules
        /// </summary>
        private Dictionary<string, HashSet<string>> BuildDependencyGraph()
        {
            var graph = new Dictionary<string, HashSet<string>>();
            
            foreach (var module in _registeredModules.Values)
            {
                if (!graph.ContainsKey(module.SaveId))
                {
                    graph[module.SaveId] = new HashSet<string>();
                }
                
                foreach (string dependency in module.Dependencies)
                {
                    graph[module.SaveId].Add(dependency);
                }
            }
            
            return graph;
        }

        /// <summary>
        /// Sort modules by load order, considering dependencies and load priority
        /// </summary>
        private List<SaveModule> GetSortedModules(Dictionary<string, HashSet<string>> dependencyGraph)
        {
            var visited = new HashSet<string>();
            var sorted = new List<SaveModule>();
            
            // Visit all nodes in the graph
            foreach (var moduleId in _registeredModules.Keys)
            {
                if (!visited.Contains(moduleId))
                {
                    TopologicalSort(moduleId, dependencyGraph, visited, sorted);
                }
            }
            
            // Further sort by load priority
            sorted.Sort((a, b) => a.LoadPriority.CompareTo(b.LoadPriority));
            
            return sorted;
        }

        /// <summary>
        /// Perform topological sort on the dependency graph
        /// </summary>
        private void TopologicalSort(string moduleId, Dictionary<string, HashSet<string>> graph, HashSet<string> visited, List<SaveModule> sorted)
        {
            visited.Add(moduleId);
            
            // Visit all dependencies first
            if (graph.ContainsKey(moduleId))
            {
                foreach (var dependency in graph[moduleId])
                {
                    if (_registeredModules.ContainsKey(dependency) && !visited.Contains(dependency))
                    {
                        TopologicalSort(dependency, graph, visited, sorted);
                    }
                }
            }
            
            // Add current module to sorted list
            if (_registeredModules.ContainsKey(moduleId))
            {
                sorted.Add(_registeredModules[moduleId]);
            }
        }

        /// <summary>
        /// Get a registered module by its type
        /// </summary>
        /// <typeparam name="T">Type of the module to get</typeparam>
        /// <returns>The module of type T, or null if not found</returns>
        public T GetModule<T>() where T : SaveModule
        {
            foreach (var module in _registeredModules.Values)
            {
                if (module is T typedModule)
                {
                    return typedModule;
                }
            }
            
            Debug.LogWarning($"No module of type {typeof(T).Name} found");
            return null;
        }

        /// <summary>
        /// Get a module by its ID
        /// </summary>
        /// <typeparam name="T">Type to cast the module to</typeparam>
        /// <param name="moduleId">ID of the module to get</param>
        /// <returns>The module cast to type T, or null if not found or not castable</returns>
        public T GetModuleById<T>(string moduleId) where T : SaveModule
        {
            if (_registeredModules.TryGetValue(moduleId, out SaveModule module))
            {
                if (module is T typedModule)
                {
                    return typedModule;
                }
                
                Debug.LogWarning($"Module with ID {moduleId} is not of type {typeof(T).Name}");
            }
            
            return null;
        }

        private void OnApplicationQuit()
        {
            // Force save any pending modules when the application quits
            if (_saveQueue.Count > 0)
            {
                ForceSave();
            }
        }
    }
}
