using LHZ.FastJson.Enum;
using LHZ.FastJson.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LHZ.FastJson.Json.Format
{
    /// <summary>
    /// 格式化类
    /// </summary>
    internal class JsonFormatter
    {
        private DateTimeJsonFormat _dateTimeFormat = null;
        public JsonFormatter() { }
        public JsonFormatter(IJsonFormat[] jsonFormats)
        {

            for (int i = 0; i < jsonFormats.Length; i++)
            {
                if (_dateTimeFormat == null && jsonFormats[i].Type==ObjectType.DateTime)
                {
                    _dateTimeFormat = jsonFormats[i] as DateTimeJsonFormat;
                    continue;
                }
            }
        }
        /// <summary>
        /// DateTime类型格式化转化
        /// </summary>
        /// <param name="dateTime">日期时间对象</param>
        /// <param name="execCharParaphrase">是否要执行支付串转换</param>
        /// <returns>格式化日期时间字符串</returns>
        public string DateTimeFormat(DateTime dateTime, out bool execCharParaphrase)
        {
            execCharParaphrase = false;
            if (_dateTimeFormat.FormatFunc != null)
            {
                execCharParaphrase = true;
                var dateStr = _dateTimeFormat.FormatFunc(dateTime);
                if (dateStr == null)
                {
                    throw new JsonFormatterException(_dateTimeFormat, "[DateTimeFormat] DateTime不能转化为null");
                }
                return dateStr;
            }
            else if (_dateTimeFormat != null)
                return dateTime.ToString(_dateTimeFormat.FormatString);
            return dateTime.ToString();
        }
    }
}
