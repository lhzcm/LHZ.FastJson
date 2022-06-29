using LHZ.FastJson.Json.Format;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LHZ.FastJson.Exceptions
{
    public class JsonFormatterException : Exception
    {
        private IJsonFormat _jsonFormat;
        public JsonFormatterException(IJsonFormat jsonFormat, string msg) : base(msg)
        {
            this._jsonFormat = jsonFormat;
        }
        public IJsonFormat JsonFormat => _jsonFormat;
    }
}
