using LHZ.FastJson.Interface;
using LHZ.FastJson.Json.CustomConverter;
using System;
using System.Collections.Generic;
using System.Text;

namespace LHZ.FastJson.Exceptions
{
    /// <summary>
    /// Custom JSON conversion exception class
    /// </summary>
    public class JsonCustomConverterException : Exception
    {
        private IJsonCustomConverter _customConverter;
        /// <summary>
        /// Initialize custom conversion exception
        /// </summary>
        /// <param name="customConverter">Custom converter</param>
        /// <param name="message">Exception message</param>
        public JsonCustomConverterException(IJsonCustomConverter customConverter, string message) : base(message)
        {
            _customConverter = customConverter;
        }

        /// <summary>
        /// Initialize custom conversion exception (with inner exception)
        /// </summary>
        /// <param name="customConverter">Custom converter</param>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Inner exception</param>
        public JsonCustomConverterException(IJsonCustomConverter customConverter, string message, Exception innerException) : base(message, innerException)
        {
            _customConverter = customConverter;
        }

        /// <summary>
        /// Custom JSON conversion
        /// </summary>
        public IJsonCustomConverter JsonCustomConverter => _customConverter;
    }
}
