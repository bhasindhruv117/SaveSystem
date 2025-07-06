using System;

namespace SaveSystem.Core
{
    /// <summary>
    /// Interface for serializer implementations that can be used with the save system
    /// </summary>
    public interface ISaveSerializer
    {
        /// <summary>
        /// Serializes an object to a string representation
        /// </summary>
        /// <typeparam name="T">Type of object to serialize</typeparam>
        /// <param name="data">The data to serialize</param>
        /// <returns>Serialized string representation</returns>
        string Serialize<T>(T data);
        
        /// <summary>
        /// Deserializes a string to an object of type T
        /// </summary>
        /// <typeparam name="T">Type to deserialize to</typeparam>
        /// <param name="serializedData">The serialized data</param>
        /// <returns>Deserialized object</returns>
        T Deserialize<T>(string serializedData);
        
        /// <summary>
        /// Deserializes a string to an object of the specified type
        /// </summary>
        /// <param name="serializedData">The serialized data</param>
        /// <param name="type">Type to deserialize to</param>
        /// <returns>Deserialized object</returns>
        object Deserialize(string serializedData, Type type);
    }
}
