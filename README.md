# Unity Game Save System

A generic, modular save system for Unity games with dependency resolution and extensible serialization options.

## Features

- **Modular Architecture**: Save different parts of your game state independently
- **Dependency Resolution**: Modules can depend on each other and are restored in the correct order
- **Extensible Serialization**: Easily swap between different serialization formats
- **Async Operations**: Save operations run asynchronously to avoid frame drops
- **Auto-Save**: Configurable periodic auto-saving
- **Editor Tools**: Convenient editor utilities for managing saves

## Quick Start

1. Add a `SaveManager` component to a GameObject in your scene (preferably one that will persist across scenes)
2. Create classes that inherit from `SaveModule` to represent parts of your game state you want to save
3. Register your modules with the SaveManager
4. Set up a serializer (Newtonsoft.Json is included)
5. Request saves when you need them!

## Usage Example

```csharp
// Create a module for player data
public class PlayerDataModule : SaveModule
{
    public override string SaveId => "PlayerData";
    
    // Data you want to save
    public string PlayerName { get; set; }
    public int Score { get; set; }
    public Vector3 Position { get; set; }
    
    // Optional: Define dependencies if this module depends on others
    public override IEnumerable<string> Dependencies => new[] { "GameSettings" };
    
    // Optional: Customize save/load priority (lower numbers are processed first)
    public override int SavePriority => 10;
    public override int LoadPriority => 10;
    
    // Optional: Do something before saving
    public override void OnBeforeSave()
    {
        Debug.Log("Preparing to save player data...");
    }
    
    // Optional: Do something after loading
    public override void OnAfterLoad()
    {
        Debug.Log("Player data loaded!");
    }
}

// In your game startup code:
void Start()
{
    // Set up the save system with Newtonsoft JSON serializer
    SaveManager.Instance.SetSerializer(new NewtonsoftJsonSerializer());
    
    // Create and register your modules
    var playerDataModule = new PlayerDataModule();
    SaveManager.Instance.RegisterModule(playerDataModule);
    
    // Load saved data
    SaveManager.Instance.LoadAllModules();
    
    // Later, request a save when something changes
    playerDataModule.Score = 100;
    SaveManager.Instance.RequestSave(playerDataModule.SaveId);
    
    // Or use the extension method
    playerDataModule.Score = 200;
    playerDataModule.RequestSave();
    
    // Force an immediate save of all pending modules
    SaveManager.Instance.ForceSave();
}
```

## Extending with Custom Serializers

You can implement your own serializer by creating a class that implements `ISaveSerializer`:

```csharp
public class MyCustomSerializer : ISaveSerializer
{
    public string Serialize<T>(T data) 
    {
        // Your serialization logic here
    }
    
    public T Deserialize<T>(string serializedData)
    {
        // Your deserialization logic here
    }
    
    public object Deserialize(string serializedData, Type type)
    {
        // Your deserialization logic here
    }
}

// Then use it:
SaveManager.Instance.SetSerializer(new MyCustomSerializer());
```
