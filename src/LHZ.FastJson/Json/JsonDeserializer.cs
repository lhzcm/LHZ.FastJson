using LHZ.FastJson.Enum;
using LHZ.FastJson.Exceptions;
using LHZ.FastJson.Interface;
using LHZ.FastJson.JsonClass;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace LHZ.FastJson.Json
{
    /// <summary>
    /// Deserialization class
    /// </summary>
    /// <typeparam name="T">The class to deserialize</typeparam>
    public class JsonDeserializer<T>
    {
        private IJsonObject _obj;
        /// <summary>
        /// Initialize with a JSON object
        /// </summary>
        /// <param name="obj">Parsed JSON object</param>
        public JsonDeserializer(IJsonObject obj)
        {
            this._obj = obj;
        }
        /// <summary>
        /// Initialize with a JsonReader
        /// </summary>
        /// <param name="reader">JSON reader</param>
        public JsonDeserializer(JsonReader reader)
        {
            this._obj = reader.JsonRead();
        }
        /// <summary>
        /// Initialize with a JSON string
        /// </summary>
        /// <param name="jsonString">JSON string</param>
        public JsonDeserializer(string jsonString)
        {
            JsonReader reader = new JsonReader(jsonString);
            this._obj = reader.JsonRead();
        }

        /// <summary>
        /// Perform deserialization
        /// </summary>
        /// <returns>Returns the deserialized typed object</returns>
        public T Deserialize(params IJsonCustomConverter[] jsonCustomConverters)
        {
            Dictionary<Type, IJsonCustomConverter> customConverters = null;
            if (jsonCustomConverters != null && jsonCustomConverters.Length > 0)
            {
                customConverters = new Dictionary<Type, IJsonCustomConverter>(jsonCustomConverters.Length + 4);
                foreach (var item in jsonCustomConverters)
                {
                    if ((item.CustomItem & Enum.CustomConverter.JsonCustomConvertItem.CustomDeSerialize) != Enum.CustomConverter.JsonCustomConvertItem.CustomDeSerialize)
                    {
                        continue;
                    }
                    if (customConverters.ContainsKey(item.ConvertType))
                    {
                        customConverters[item.ConvertType] = item;
                    }
                    else
                    {
                        customConverters.Add(item.ConvertType, item);
                    }
                }
            }
            return JsonDeserialzerExpression<T>.Deserialzer(_obj, customConverters);
        }
    }
}
