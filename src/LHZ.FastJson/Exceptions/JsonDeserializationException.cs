using LHZ.FastJson.Enum;
using LHZ.FastJson.JsonClass;
using System;
using System.Collections.Generic;
using System.Text;

namespace LHZ.FastJson.Exceptions
{
    public class JsonDeserializationException : JsonReadException
    {
        private JsonType _jsonType;
        //private ObjectType _objType;
        private Type _targetType;
        public JsonDeserializationException(IJsonObject jsonObject, Type targetType, string msg) : base(jsonObject.Position, msg)
        {
            this._jsonType = jsonObject.Type;
            this._targetType = targetType;
        }

        public JsonType JsonType { get { return this._jsonType; } }
        public Type TargetType { get { return this._targetType; } }
    }
}
