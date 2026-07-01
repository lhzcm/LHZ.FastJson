using LHZ.FastJson.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LHZ.FastJson.Json.Format
{
    /// <summary>
    /// DateTime formatting class
    /// </summary>
    [Obsolete]
    public class DateTimeJsonFormat : IJsonFormat
    {
        private string _fromatstr = "yyyy-MM-dd HH:mm:ss";
        private Func<DateTime, string> _formatFunc;
        /// <summary>
        /// Default DateTime formatting constructor
        /// </summary>
        public DateTimeJsonFormat() { }

        /// <summary>
        /// DateTime formatting constructor
        /// </summary>
        /// <param name="formatString">Format string</param>
        public DateTimeJsonFormat(string formatString)
        {
            this._fromatstr = formatString;
        }

        /// <summary>
        /// DateTime formatting constructor
        /// </summary>
        /// <param name="formatFunc">Format function delegate</param>
        public DateTimeJsonFormat(Func<DateTime, string> formatFunc)
        {
            this._formatFunc = formatFunc;
        }
        /// <summary>
        /// Format type
        /// </summary>
        public ObjectType Type => ObjectType.DateTime;

        /// <summary>
        /// Format string, e.g. (yyyy-MM-dd)
        /// </summary>
        public string FormatString => _fromatstr;
        /// <summary>
        /// Format function delegate
        /// </summary>
        public Func<DateTime, string> FormatFunc => _formatFunc;

    }
}
