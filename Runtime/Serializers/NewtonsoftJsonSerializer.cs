using System;
using Newtonsoft.Json;
using SaveSystem.Core;

namespace SaveSystem.Serializers
{
    /// <summary>
    /// Implementation of ISaveSerializer using Newtonsoft.Json
    /// </summary>
    public class NewtonsoftJsonSerializer : ISaveSerializer
    {
        private readonly JsonSerializerSettings _settings;
        
        public NewtonsoftJsonSerializer(JsonSerializerSettings settings = null)
        {
            _settings = settings ?? new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented
            };
        }
        
        /// <summary>
        /// Serializes an object to JSON string
        /// </summary>
        public string Serialize<T>(T data)
        {
            return JsonConvert.SerializeObject(data, _settings);
        }
        
        /// <summary>
        /// Deserializes a JSON string to an object of type T
        /// </summary>
        public T Deserialize<T>(string serializedData)
        {
            return JsonConvert.DeserializeObject<T>(serializedData, _settings);
        }
        
        /// <summary>
        /// Deserializes a JSON string to an object of the specified type
        /// </summary>
        public object Deserialize(string serializedData, Type type)
        {
            return JsonConvert.DeserializeObject(serializedData, type, _settings);
        }
    }
}
