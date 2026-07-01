using LHZ.FastJson.Enum;
using LHZ.FastJson.JsonClass;
using System;
using System.Collections.Generic;
using System.Text;

namespace LHZ.FastJson.Exceptions
{
    /// <summary>
    /// JSON deserialization exception
    /// </summary>
    public class JsonDeserializationException : JsonReadException
    {
        private JsonType _jsonType;
        //private ObjectType _objType;
        private Type _targetType;
        /// <summary>
        /// Initialize JSON deserialization exception
        /// </summary>
        /// <param name="jsonObject">JSON object</param>
        /// <param name="targetType">Target type</param>
        /// <param name="msg">Exception message</param>
        public JsonDeserializationException(IJsonObject jsonObject, Type targetType, string msg) : base(jsonObject.Position, msg)
        {
            this._jsonType = jsonObject.Type;
            this._targetType = targetType;
        }

        /// <summary>
        /// JSON type
        /// </summary>
        public JsonType JsonType { get { return this._jsonType; } }
        /// <summary>
        /// Target type
        /// </summary>
        public Type TargetType { get { return this._targetType; } }
    }
}
