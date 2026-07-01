using LHZ.FastJson.Json.Format;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LHZ.FastJson.Exceptions
{
    /// <summary>
    /// JSON formatting exception
    /// </summary>
    public class JsonFormatterException : Exception
    {
        private IJsonFormat _jsonFormat;
        /// <summary>
        /// Initialize JSON formatting exception
        /// </summary>
        /// <param name="jsonFormat">Formatter</param>
        /// <param name="msg">Exception message</param>
        public JsonFormatterException(IJsonFormat jsonFormat, string msg) : base(msg)
        {
            this._jsonFormat = jsonFormat;
        }
        /// <summary>
        /// Formatter
        /// </summary>
        public IJsonFormat JsonFormat => _jsonFormat;
    }
}
