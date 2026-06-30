using LHZ.FastJson.Json.Format;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LHZ.FastJson.Exceptions
{
    /// <summary>
    /// Json格式化异常
    /// </summary>
    public class JsonFormatterException : Exception
    {
        private IJsonFormat _jsonFormat;
        /// <summary>
        /// 初始化Json格式化异常
        /// </summary>
        /// <param name="jsonFormat">格式化器</param>
        /// <param name="msg">异常消息</param>
        public JsonFormatterException(IJsonFormat jsonFormat, string msg) : base(msg)
        {
            this._jsonFormat = jsonFormat;
        }
        /// <summary>
        /// 格式化器
        /// </summary>
        public IJsonFormat JsonFormat => _jsonFormat;
    }
}
