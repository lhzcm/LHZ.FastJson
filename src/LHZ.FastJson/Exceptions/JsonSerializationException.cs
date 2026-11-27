using System;

namespace LHZ.FastJson.Exceptions
{
    /// <summary>
    /// The exception that is thrown when an object cannot be serialized, for example because the object graph
    /// contains a circular reference, or because two properties of a type map to the same JSON property name.
    /// </summary>
    public class JsonSerializationException : Exception
    {
        private readonly Type _serializedType;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonSerializationException"/> class.
        /// </summary>
        /// <param name="message">The error message</param>
        public JsonSerializationException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonSerializationException"/> class for a specific type.
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="serializedType">The type that could not be serialized</param>
        public JsonSerializationException(string message, Type serializedType) : base(message)
        {
            this._serializedType = serializedType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonSerializationException"/> class with an inner exception.
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="innerException">The exception that is the cause of this exception</param>
        public JsonSerializationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// The type that could not be serialized, or <c>null</c> when it is not known.
        /// </summary>
        public Type SerializedType { get { return _serializedType; } }
    }
}
