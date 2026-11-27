using System;

namespace LHZ.FastJson.Exceptions
{
    /// <summary>
    /// The exception that is thrown when the JSON text being read is not valid JSON, or ends unexpectedly.
    /// It is the base class of all exceptions thrown while reading or deserializing JSON text:
    /// <see cref="JsonDeserializationException"/> (valid JSON that cannot be mapped to the requested type)
    /// derives from it.
    /// </summary>
    public class JsonReadException : Exception
    {
        private readonly int _position;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonReadException"/> class.
        /// </summary>
        /// <param name="position">The zero-based character position in the JSON text where the problem was detected</param>
        /// <param name="msg">The error message</param>
        public JsonReadException(int position, string msg) : base(msg)
        {
            this._position = position;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonReadException"/> class with an inner exception.
        /// </summary>
        /// <param name="position">The zero-based character position in the JSON text where the problem was detected</param>
        /// <param name="msg">The error message</param>
        /// <param name="innerException">The exception that is the cause of this exception</param>
        public JsonReadException(int position, string msg, Exception innerException) : base(msg, innerException)
        {
            this._position = position;
        }

        /// <summary>
        /// The zero-based character position in the JSON text where the problem was detected.
        /// </summary>
        public int Position { get { return _position; } }
    }
}
