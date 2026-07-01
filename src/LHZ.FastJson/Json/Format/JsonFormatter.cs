using LHZ.FastJson.Enum;
using LHZ.FastJson.Exceptions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace LHZ.FastJson.Json.Format
{
    /// <summary>
    /// Formatter class (deprecated, use JsonCustomConvert instead)
    /// </summary>
    [Obsolete]
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
        /// DateTime type format conversion
        /// </summary>
        /// <param name="dateTime">DateTime object</param>
        /// <param name="execCharParaphrase">Whether to perform string conversion</param>
        /// <returns>Formatted DateTime string</returns>
        public string DateTimeFormat(DateTime dateTime, out bool execCharParaphrase)
        {
            execCharParaphrase = false;
            if (_dateTimeFormat == null)
            {
                return dateTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            }
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
