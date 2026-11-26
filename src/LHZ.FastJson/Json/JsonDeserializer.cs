using LHZ.FastJson.Enum;
using LHZ.FastJson.Exceptions;
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
        private readonly JsonDirectReader _obj;
        /// <summary>
        /// Initialize with a JSON string
        /// </summary>
        /// <param name="jsonString">JSON string</param>
        public JsonDeserializer(string jsonString)
            : this(jsonString, null)
        {
        }

        /// <summary>
        /// Initialize with a JSON string and options
        /// </summary>
        /// <param name="jsonString">JSON string</param>
        /// <param name="options">Deserialization options (null uses the defaults)</param>
        public JsonDeserializer(string jsonString, JsonConvertOptions options)
        {
            JsonDirectReader reader = new JsonDirectReader(jsonString, options);
            this._obj = reader;
        }

        /// <summary>
        /// Perform deserialization
        /// </summary>
        /// <returns>Returns the deserialized typed object</returns>
        public T Deserialize()
        {
            return JsonDirectDeserialzerExpression<T>.Deserialzer(_obj);
        }
    }
}
