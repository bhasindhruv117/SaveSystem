using System;

namespace SaveSystem.Core
{
    /// <summary>
    /// Base class for save serializers that implements ISaveSerializer
    /// </summary>
    public abstract class BaseSaveSerializer : ISaveSerializer
    {
        /// <summary>
        /// Serializes an object to a string representation
        /// </summary>
        public abstract string Serialize<T>(T data);
        
        /// <summary>
        /// Deserializes a string to an object of type T
        /// </summary>
        public abstract T Deserialize<T>(string serializedData);
        
        /// <summary>
        /// Deserializes a string to an object of the specified type
        /// </summary>
        public abstract object Deserialize(string serializedData, Type type);
    }
}
