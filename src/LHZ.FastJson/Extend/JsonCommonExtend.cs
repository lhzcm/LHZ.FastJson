using LHZ.FastJson.Json;
using LHZ.FastJson.JsonClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LHZ.FastJson
{
    /// <summary>
    /// Common JSON extension methods
    /// </summary>
    public static class JsonCommonExtend
    {

        /// <summary>
        /// Convert a JSON object to the target object
        /// </summary>
        /// <typeparam name="T">The target object type to convert to</typeparam>
        /// <returns>The target object</returns>
        public static T ToObject<T>(this IJsonObject jsonObj)
        {
            JsonDeserializer<T> deserializer = new JsonDeserializer<T>(jsonObj);
            return deserializer.Deserialize();
        }

        /// <summary>
        /// Deserialize a JSON string into a T object
        /// </summary>
        /// <typeparam name="T">The type of object to deserialize to</typeparam>
        /// <param name="jsonString">JSON string</param>
        /// <returns>The deserialized object</returns>
        public static T FromJson<T>(this String jsonString)
        {
            return JsonConvert.Deserialize<T>(jsonString);
        }

        /// <summary>
        /// Serialize an object to a JSON string
        /// </summary>
        /// <param name="target">The target object</param>
        /// <returns>JSON string</returns>
        public static string ToJson(this Object target)
        {
            return JsonConvert.Serialize(target);
        }
    }
}
