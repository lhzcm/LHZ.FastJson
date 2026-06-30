using LHZ.FastJson.Interface;
using LHZ.FastJson.Json.CustomConverter;
using System;
using System.Collections.Generic;
using System.Text;

namespace LHZ.FastJson.Exceptions
{
    /// <summary>
    /// 自定义json转换异常类
    /// </summary>
    public class JsonCustomConverterException : Exception
    {
        private IJsonCustomConverter _customConverter;
        /// <summary>
        /// 初始化自定义转换异常
        /// </summary>
        /// <param name="customConverter">自定义转换器</param>
        /// <param name="message">异常消息</param>
        public JsonCustomConverterException(IJsonCustomConverter customConverter, string message) : base(message)
        {
            _customConverter = customConverter;
        }

        /// <summary>
        /// 初始化自定义转换异常（包含内部异常）
        /// </summary>
        /// <param name="customConverter">自定义转换器</param>
        /// <param name="message">异常消息</param>
        /// <param name="innerException">内部异常</param>
        public JsonCustomConverterException(IJsonCustomConverter customConverter, string message, Exception innerException) : base(message, innerException)
        {
            _customConverter = customConverter;
        }

        /// <summary>
        /// 自定义Json转换
        /// </summary>
        public IJsonCustomConverter JsonCustomConverter => _customConverter;
    }
}
