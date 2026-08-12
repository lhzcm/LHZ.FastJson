using System;
using LHZ.FastJson.Enum;

namespace LHZ.FastJson.Exceptions
{
    public class JsonDirectDeserializationException : JsonReadException
    {
        private JsonType _jsonType;
        //private ObjectType _objType;
        private Type _targetType;
        /// <summary>
        /// Initialize JSON deserialization exception
        /// </summary>
        /// <param name="Position">Position</param>
        /// <param name="targetType">Target type</param>
        /// <param name="msg">Exception message</param>
        public JsonDirectDeserializationException(int Position, JsonType jsonType, Type targetType, string msg) : base(Position, msg)
        {
            this._targetType = targetType;
            this._jsonType = jsonType;
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
