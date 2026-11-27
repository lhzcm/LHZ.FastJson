using System;
using LHZ.FastJson.Enum;

namespace LHZ.FastJson.Exceptions
{
    /// <summary>
    /// The exception thrown by <c>JsonConvert.Deserialize&lt;T&gt;</c> when valid JSON cannot be converted to the
    /// requested type. It derives from <see cref="JsonDeserializationException"/>, so catching that type (or
    /// <see cref="JsonReadException"/>) also catches it.
    /// </summary>
    public class JsonDirectDeserializationException : JsonDeserializationException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonDirectDeserializationException"/> class.
        /// </summary>
        /// <param name="position">The zero-based character position in the JSON text where the value starts</param>
        /// <param name="jsonType">The type of the JSON value that was found</param>
        /// <param name="targetType">The type the value could not be converted to</param>
        /// <param name="msg">The error message</param>
        public JsonDirectDeserializationException(int position, JsonType jsonType, Type targetType, string msg) : base(position, jsonType, targetType, msg)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonDirectDeserializationException"/> class with an inner exception.
        /// </summary>
        /// <param name="position">The zero-based character position in the JSON text where the value starts</param>
        /// <param name="jsonType">The type of the JSON value that was found</param>
        /// <param name="targetType">The type the value could not be converted to</param>
        /// <param name="msg">The error message</param>
        /// <param name="innerException">The exception that is the cause of this exception</param>
        public JsonDirectDeserializationException(int position, JsonType jsonType, Type targetType, string msg, Exception innerException) : base(position, jsonType, targetType, msg, innerException)
        {
        }
    }
}
