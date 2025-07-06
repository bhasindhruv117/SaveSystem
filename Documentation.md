# Unity Save System - Documentation

## Overview

This package provides a flexible, modular save system for Unity games. It features:

- Modular save/load architecture
- Dependency resolution between save modules
- Queue-based saving system
- Extensible serialization support
- Async file operations
- Auto-save capabilities

## Quick Start

1. Add the SaveManager to a GameObject in your scene (preferably one marked with DontDestroyOnLoad)
2. Create one or more classes that inherit from SaveModule
3. Register your modules with the SaveManager
4. Set up a serializer (default: NewtonsoftJsonSerializer)
5. Request saves as needed

## Core Components

### SaveManager

The central coordinator for the save system. It manages module registration, save requests, and loading.

```csharp
// Get the singleton instance
SaveManager.Instance.SetSerializer(new NewtonsoftJsonSerializer());

// Register a module
SaveManager.Instance.RegisterModule(myModule);

// Request a save
SaveManager.Instance.RequestSave("MyModuleId");

// Force immediate save
SaveManager.Instance.ForceSave();

// Load all modules
SaveManager.Instance.LoadAllModules();
```

### SaveModule

Base class for all savable game modules.

```csharp
[Serializable]
public class PlayerStatsModule : SaveModule
{
    public override string SaveId => "PlayerStats";
    
    // Define dependencies
    public override IEnumerable<string> Dependencies => new[] { "GameSettings" };
    
    // Data fields
    public int Health { get; set; }
    public int Score { get; set; }
    
    // Called before saving
    public override void OnBeforeSave() { }
    
    // Called after loading
    public override void OnAfterLoad() { }
}
```

### ISaveSerializer

Interface for serialization implementations. The package includes NewtonsoftJsonSerializer, but you can implement your own.

```csharp
public class MyCustomSerializer : ISaveSerializer
{
    public string Serialize<T>(T data) { ... }
    public T Deserialize<T>(string serializedData) { ... }
    public object Deserialize(string serializedData, Type type) { ... }
}
```

## Best Practices

1. **Module Design**: Break down your game state into logical modules (player stats, inventory, quests, settings, etc.)

2. **Dependencies**: Use the dependency system to ensure modules load in the correct order:
   ```csharp
   public override IEnumerable<string> Dependencies => new[] { "Settings", "PlayerStats" };
   ```

3. **Save Priorities**: Use SavePriority and LoadPriority to fine-tune loading order:
   ```csharp
   public override int SavePriority => 10; // Lower numbers save first
   public override int LoadPriority => 20; // Lower numbers load first
   ```

4. **Auto-Save**: The system can auto-save periodically or you can trigger saves manually:
   ```csharp
   // Manual save request
   myModule.RequestSave();
   
   // Force immediate save
   SaveManager.Instance.ForceSave();
   ```

5. **OnBeforeSave/OnAfterLoad**: Use these hooks for pre/post-processing:
   ```csharp
   public override void OnBeforeSave()
   {
       // Prepare data before serialization
       LastSaveTime = DateTime.Now;
   }
   
   public override void OnAfterLoad()
   {
       // Process data after deserialization
       ApplySettings();
   }
   ```

## Integration with Existing Games

To integrate with an existing game:

1. Create a game-specific SaveManager that initializes the core SaveManager
2. Create SaveModule implementations for your game's data
3. Modify your game to use these modules instead of direct serialization
4. Add save requests at appropriate points (level completion, item pickup, settings change, etc.)

## Thread Safety

The save system uses async operations for file writing to avoid frame drops. However, ensure that you're not modifying module data while a save is in progress.

## Editor Support

The package includes editor utilities to manage saves during development. Use the SaveManagerEditor to:

- Open the save directory
- Force save all pending modules
- Clear all save data
