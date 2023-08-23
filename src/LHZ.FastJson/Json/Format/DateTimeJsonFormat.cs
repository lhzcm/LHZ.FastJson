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
    [Obsolete]
    public class DateTimeJsonFormat : IJsonFormat
    {
        private string _fromatstr = "yyyy-MM-dd HH:mm:ss";
        private Func<DateTime, string> _formatFunc;
        public DateTimeJsonFormat() { }

        /// <summary>
        /// 日期格式化构造函数
        /// </summary>
        /// <param name="formatString">格式化字符串</param>
        public DateTimeJsonFormat(string formatString)
        {
            this._fromatstr = formatString;
        }

        /// <summary>
        /// 日期格式化构造函数
        /// </summary>
        /// <param name="formatFunc">格式化方法委托</param>
        public DateTimeJsonFormat(Func<DateTime, string> formatFunc)
        {
            this._formatFunc = formatFunc;
        }
        /// <summary>
        /// 格式化类型
        /// </summary>
        public ObjectType Type => ObjectType.DateTime;

        /// <summary>
        /// 格式化字符串如（yyyy-MM-dd）
        /// </summary>
        public string FormatString => _fromatstr;
        public Func<DateTime, string> FormatFunc => _formatFunc;

    }
}
