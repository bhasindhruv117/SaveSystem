namespace SaveSystem.Core
{
    /// <summary>
    /// Interface for objects that can be saved by the save system
    /// </summary>
    public interface ISavable
    {
        /// <summary>
        /// Unique identifier for this savable module
        /// </summary>
        string SaveId { get; }
    }
}
