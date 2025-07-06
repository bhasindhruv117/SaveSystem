using System;
using System.Collections.Generic;
using SaveSystem.Core;
using SaveSystem.Serializers;
using UnityEngine;

namespace SaveSystem.Samples
{
    /// <summary>
    /// Example script demonstrating how to use the SaveSystem in any Unity game
    /// </summary>
    public class GenericSaveSystemExample : MonoBehaviour
    {
        [SerializeField] private bool _initializeOnStart = true;
        [SerializeField] private float _autoSaveInterval = 60f;
        
        // Example save modules
        private PlayerStatsModule _playerStatsModule;
        private GameProgressModule _gameProgressModule;
        private SettingsModule _settingsModule;
        
        void Start()
        {
            if (_initializeOnStart)
            {
                InitializeSaveSystem();
            }
        }
        
        /// <summary>
        /// Initialize the save system with the necessary modules
        /// </summary>
        public void InitializeSaveSystem()
        {
            Debug.Log("Initializing save system...");
            
            // Configure the save manager
            var saveManager = SaveManager.Instance;
            
            // Set the serializer (we're using Newtonsoft.Json)
            saveManager.SetSerializer(new NewtonsoftJsonSerializer());
            
            // Create and register modules
            _playerStatsModule = new PlayerStatsModule();
            _gameProgressModule = new GameProgressModule();
            _settingsModule = new SettingsModule();
            
            // Register all modules
            saveManager.RegisterModule(_playerStatsModule);
            saveManager.RegisterModule(_gameProgressModule);
            saveManager.RegisterModule(_settingsModule);
            
            // Load all saved data
            saveManager.LoadAllModules();
            
            Debug.Log("Save system initialized.");
        }
        
        /// <summary>
        /// Example of updating player stats and requesting a save
        /// </summary>
        public void UpdatePlayerStats(int health, int score)
        {
            if (_playerStatsModule != null)
            {
                _playerStatsModule.Health = health;
                _playerStatsModule.Score = score;
                _playerStatsModule.LastUpdated = DateTime.Now;
                
                // Request the module to be saved
                _playerStatsModule.RequestSave();
            }
        }
        
        /// <summary>
        /// Example of updating game progress
        /// </summary>
        public void UpdateGameProgress(int level, float completion)
        {
            if (_gameProgressModule != null)
            {
                _gameProgressModule.CurrentLevel = level;
                _gameProgressModule.LevelCompletion = completion;
                
                // Request the module to be saved
                _gameProgressModule.RequestSave();
            }
        }
        
        /// <summary>
        /// Example of updating game settings
        /// </summary>
        public void UpdateSettings(float musicVolume, float sfxVolume, bool fullscreen)
        {
            if (_settingsModule != null)
            {
                _settingsModule.MusicVolume = musicVolume;
                _settingsModule.SfxVolume = sfxVolume;
                _settingsModule.Fullscreen = fullscreen;
                
                // Request the module to be saved
                _settingsModule.RequestSave();
            }
        }
        
        /// <summary>
        /// Force save all pending data immediately
        /// </summary>
        public void SaveGame()
        {
            Debug.Log("Forcing immediate save...");
            SaveManager.Instance.ForceSave();
        }
        
        void OnApplicationQuit()
        {
            SaveGame();
        }
    }

    #region Example Save Modules
    
    /// <summary>
    /// Example module for player stats
    /// </summary>
    [Serializable]
    public class PlayerStatsModule : SaveModule
    {
        public override string SaveId => "PlayerStats";
        
        // This module depends on settings
        public override IEnumerable<string> Dependencies => new[] { "Settings" };
        
        // Lower priority means it loads earlier
        public override int LoadPriority => 10;
        
        // Player stats data
        public int Health { get; set; } = 100;
        public int Score { get; set; } = 0;
        public DateTime LastUpdated { get; set; }
        
        public override void OnBeforeSave()
        {
            // Update any derived data before saving
            LastUpdated = DateTime.Now;
        }
        
        public override void OnAfterLoad()
        {
            Debug.Log($"Player stats loaded: Health={Health}, Score={Score}");
        }
    }
    
    /// <summary>
    /// Example module for game progress
    /// </summary>
    [Serializable]
    public class GameProgressModule : SaveModule
    {
        public override string SaveId => "GameProgress";
        
        // This module depends on player stats
        public override IEnumerable<string> Dependencies => new[] { "PlayerStats" };
        
        // Middle priority
        public override int LoadPriority => 50;
        
        // Game progress data
        public int CurrentLevel { get; set; } = 1;
        public float LevelCompletion { get; set; } = 0f;
        public List<string> CompletedObjectives { get; set; } = new List<string>();
        
        public override void OnAfterLoad()
        {
            Debug.Log($"Game progress loaded: Level={CurrentLevel}, Completion={LevelCompletion:P0}");
        }
    }
    
    /// <summary>
    /// Example module for game settings
    /// </summary>
    [Serializable]
    public class SettingsModule : SaveModule
    {
        public override string SaveId => "Settings";
        
        // No dependencies, should load first
        public override int LoadPriority => 1;
        
        // Settings data
        public float MusicVolume { get; set; } = 0.5f;
        public float SfxVolume { get; set; } = 0.7f;
        public bool Fullscreen { get; set; } = true;
        public int QualityLevel { get; set; } = 2;
        
        public override void OnAfterLoad()
        {
            Debug.Log($"Settings loaded: Music={MusicVolume}, SFX={SfxVolume}");
            
            // Apply loaded settings
            ApplySettings();
        }
        
        private void ApplySettings()
        {
            // In a real game, you would actually apply these settings to the game
            // Screen.fullScreen = Fullscreen;
            // QualitySettings.SetQualityLevel(QualityLevel);
            // AudioManager.SetVolume("Music", MusicVolume);
            // AudioManager.SetVolume("SFX", SfxVolume);
        }
    }
    
    #endregion
}
