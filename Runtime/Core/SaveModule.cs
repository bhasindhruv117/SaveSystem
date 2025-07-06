using System;
using System.Collections.Generic;
using UnityEngine;

namespace SaveSystem.Core
{
    /// <summary>
    /// Base class for savable modules with dependency resolution
    /// </summary>
    [Serializable]
    public abstract class SaveModule : ISavable
    {
        /// <summary>
        /// Unique identifier for this save module
        /// </summary>
        public abstract string SaveId { get; }
        
        /// <summary>
        /// List of other module IDs that this module depends on
        /// </summary>
        public virtual IEnumerable<string> Dependencies { get; } = Array.Empty<string>();
        
        /// <summary>
        /// Priority for saving (lower values are saved first)
        /// </summary>
        public virtual int SavePriority { get; } = 100;
        
        /// <summary>
        /// Priority for loading (lower values are loaded first)
        /// </summary>
        public virtual int LoadPriority { get; } = 100;
        
        /// <summary>
        /// Called before this module is serialized
        /// </summary>
        public virtual void OnBeforeSave() { }
        
        /// <summary>
        /// Called after this module is deserialized
        /// </summary>
        public virtual void OnAfterLoad() { }
    }
}
