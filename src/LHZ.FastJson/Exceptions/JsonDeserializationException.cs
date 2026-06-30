using LHZ.FastJson.Enum;
using LHZ.FastJson.JsonClass;
using System;
using System.Collections.Generic;
using System.Text;

namespace LHZ.FastJson.Exceptions
{
    /// <summary>
    /// Json反序列化异常
    /// </summary>
    public class JsonDeserializationException : JsonReadException
    {
        private JsonType _jsonType;
        //private ObjectType _objType;
        private Type _targetType;
        /// <summary>
        /// 初始化Json反序列化异常
        /// </summary>
        /// <param name="jsonObject">Json对象</param>
        /// <param name="targetType">目标类型</param>
        /// <param name="msg">异常消息</param>
        public JsonDeserializationException(IJsonObject jsonObject, Type targetType, string msg) : base(jsonObject.Position, msg)
        {
            this._jsonType = jsonObject.Type;
            this._targetType = targetType;
        }

        /// <summary>
        /// Json类型
        /// </summary>
        public JsonType JsonType { get { return this._jsonType; } }
        /// <summary>
        /// 目标类型
        /// </summary>
        public Type TargetType { get { return this._targetType; } }
    }
}
