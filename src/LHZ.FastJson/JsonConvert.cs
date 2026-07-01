using LHZ.FastJson.Interface;
using LHZ.FastJson.Json;
using LHZ.FastJson.Json.Format;
using LHZ.FastJson.JsonClass;
using System;
using System.Collections.Generic;
using System.Text;

namespace LHZ.FastJson
{
    /// <summary>
    /// Static type for JSON object parsing and conversion.
    /// Can serialize JSON String to object.
    /// Can deserialize object to JSON String.
    /// </summary>
    public static class JsonConvert
    {
        /// <summary>
        /// Deserialize JSON string to JsonObject type
        /// </summary>
        /// <param name="jsonString">JSON string</param>
        /// <returns>JsonObject type object</returns>
        public static IJsonObject Deserialize(string jsonString)
        {
            JsonReader reader = new JsonReader(jsonString);
            return reader.JsonRead();
        }

        /// <summary>
        /// Try to deserialize JSON string to T type
        /// </summary>
        /// <param name="jsonString">JSON string</param>
        /// <param name="dist">Deserialized target object</param>
        /// <returns>Whether deserialization was successful</returns>
        public static bool TryDeserialize(string jsonString, out IJsonObject dist)
        {
            try
            {
                JsonReader reader = new JsonReader(jsonString);
                dist = reader.JsonRead();
            }
            catch
            {
                dist = null;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Deserialize JSON string to T type
        /// </summary>
        /// <typeparam name="T">Deserialization target type</typeparam>
        /// <param name="jsonString">JsonObject type</param>
        /// <returns>Deserialized target object</returns>
        public static T Deserialize<T>(string jsonString)
        {
            JsonDeserializer<T> deserializer = new JsonDeserializer<T>(jsonString);
            return deserializer.Deserialize();
        }
        /// <summary>
        /// Deserialize JSON string to T type
        /// </summary>
        /// <typeparam name="T">Deserialization target type</typeparam>
        /// <param name="jsonString">JsonObject type</param>
        /// <param name="jsonCustomConverters">Custom JSON converters</param>
        /// <returns>Deserialized target object</returns>
        public static T Deserialize<T>(string jsonString, params IJsonCustomConverter[] jsonCustomConverters)
        {
            JsonDeserializer<T> deserializer = new JsonDeserializer<T>(jsonString);
            return deserializer.Deserialize(jsonCustomConverters);
        }

        /// <summary>
        /// Try to deserialize JSON string to T type
        /// </summary>
        /// <typeparam name="T">Deserialization target type</typeparam>
        /// <param name="jsonString">JSON string</param>
        /// <param name="dist">Deserialized target object</param>
        /// <returns>Whether deserialization was successful</returns>
        public static bool TryDeserialize<T>(string jsonString, out T dist)
        {
            try
            {
                JsonDeserializer<T> deserializer = new JsonDeserializer<T>(jsonString);
                dist = deserializer.Deserialize();
            }
            catch 
            {
                dist = default(T);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Serialize an object to a JSON string
        /// </summary>
        /// <param name="obj">The object to serialize</param>
        /// <returns>JSON string</returns>
        public static string Serialize(object obj)
        {
            JsonSerializer serializer = new JsonSerializer(obj);
            return serializer.Serialize();
        }

        /// <summary>
        /// Serialize an object to a JSON string (with formatting)
        /// </summary>
        /// <param name="obj">The object to serialize</param>
        /// <param name="formats">Format types</param>
        /// <returns>JSON string</returns>
        [Obsolete("This method is obsolete. Use Serialize(object obj) instead.")]
        public static string Serialize(object obj, params IJsonFormat[] formats)
        {
            JsonSerializer serializer = new JsonSerializer(obj, formats);
            return serializer.Serialize();
        }
    }
}
