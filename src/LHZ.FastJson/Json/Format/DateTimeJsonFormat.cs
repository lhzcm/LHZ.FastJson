using LHZ.FastJson.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LHZ.FastJson.Json.Format
{
    /// <summary>
    /// 日期格式化类
    /// </summary>
    public class DateTimeJsonFormat : IJsonFormat
    {
        private string _fromatstr = "yyyy-MM-dd HH:mm:ss";
        public DateTimeJsonFormat() { }
        public DateTimeJsonFormat(string formatString)
        {
            this._fromatstr = formatString;
        }
        /// <summary>
        /// 格式化类型
        /// </summary>
        public ObjectType Type => ObjectType.DateTime;

        /// <summary>
        /// 格式化字符串如（yyyy-MM-dd）
        /// </summary>
        public string FormatString { get { return this._fromatstr; } set { this._fromatstr = value; } }

    }
}
