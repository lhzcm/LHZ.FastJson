using LHZ.FastJson.Enum;
using LHZ.FastJson.JsonClass;
using System;

namespace LHZ.FastJson.Exceptions
{
    /// <summary>
    /// The exception that is thrown when the JSON text is valid but its value cannot be converted to the requested type,
    /// for example a JSON string where a number is required, or a number that does not fit in the target integer type.
    /// <see cref="JsonType"/> is the type of the JSON value that was found and <see cref="TargetType"/> is the type
    /// it could not be converted to.
    /// </summary>
    public class JsonDeserializationException : JsonReadException
    {
        private readonly JsonType _jsonType;
        private readonly Type _targetType;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonDeserializationException"/> class from a parsed JSON node.
        /// </summary>
        /// <param name="jsonObject">The JSON node that could not be converted</param>
        /// <param name="targetType">The type the value could not be converted to</param>
        /// <param name="msg">The error message</param>
        public JsonDeserializationException(IJsonObject jsonObject, Type targetType, string msg) : base(((JsonObject)jsonObject).Position, msg)
        {
            this._jsonType = jsonObject.Type;
            this._targetType = targetType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonDeserializationException"/> class.
        /// </summary>
        /// <param name="position">The zero-based character position in the JSON text where the value starts</param>
        /// <param name="jsonType">The type of the JSON value that was found</param>
        /// <param name="targetType">The type the value could not be converted to</param>
        /// <param name="msg">The error message</param>
        public JsonDeserializationException(int position, JsonType jsonType, Type targetType, string msg) : base(position, msg)
        {
            this._jsonType = jsonType;
            this._targetType = targetType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonDeserializationException"/> class with an inner exception.
        /// </summary>
        /// <param name="position">The zero-based character position in the JSON text where the value starts</param>
        /// <param name="jsonType">The type of the JSON value that was found</param>
        /// <param name="targetType">The type the value could not be converted to</param>
        /// <param name="msg">The error message</param>
        /// <param name="innerException">The exception that is the cause of this exception</param>
        public JsonDeserializationException(int position, JsonType jsonType, Type targetType, string msg, Exception innerException) : base(position, msg, innerException)
        {
            this._jsonType = jsonType;
            this._targetType = targetType;
        }

        /// <summary>
        /// The type of the JSON value that was found.
        /// </summary>
        public JsonType JsonType { get { return this._jsonType; } }

        /// <summary>
        /// The type the JSON value could not be converted to.
        /// </summary>
        public Type TargetType { get { return this._targetType; } }
    }
}
