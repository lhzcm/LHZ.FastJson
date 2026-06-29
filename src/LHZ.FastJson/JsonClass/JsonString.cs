using System;
using System.Collections.Generic;
using System.Text;
using LHZ.FastJson.Enum;

namespace LHZ.FastJson.JsonClass
{
    /// <summary>
    /// Json字符串类型
    /// </summary>
    public class JsonString : JsonObject
    {
        private string _value;
        public override object Value => _value;
        [Obsolete("This method is deprecated and will be removed in the next official release.")]
        public string GetValue()
        {
            return this._value.ToString();
        }
        internal JsonString(string value, int position) : base(position)
        {
            this._value = value;
        }
        public JsonString(string value)
        {
            if(value == null)
            {
                throw new ArgumentNullException(nameof(value), "value is not allow null");
            }
            this._value = value;
        }
        public override StringBuilder ToJsonStringBuilder(StringBuilder stringBuilder = null)
        {
            if(stringBuilder == null)
            {
                stringBuilder = new StringBuilder();
            }

            stringBuilder.Append('\"');
            foreach(var item in _value)
            {
                if (item == '"' || item == '\\' || item < 0x20)
                {
                    stringBuilder.Append(CharParaphrase(item));
                }
                else
                {
                    stringBuilder.Append(item);
                }
            }
            stringBuilder.Append('\"');
            return stringBuilder;
        }
        /// <summary>
        /// 字符转义
        /// </summary>
        /// <param name="paraphrase">需要转义的字符</param>
        /// <returns>转义好的字符串</returns>
        private string CharParaphrase(char paraphrase)
        {
            if (paraphrase == '"')
                return "\\\"";
            else if (paraphrase == '\\')
                return "\\\\";
            else if (paraphrase == '\n')
                return "\\n";
            else if (paraphrase == '\t')
                return "\\t";
            else if (paraphrase == '\b')
                return "\\b";
            else if (paraphrase == '\f')
                return "\\f";
            else if (paraphrase == '\r')
                return "\\r";
            else if (paraphrase < 0x20)
                return "\\u" + ((int)paraphrase).ToString("x4");
            return paraphrase.ToString();
        }
        /// <summary>
        /// Json对象类型
        /// </summary>
        public override JsonType Type => JsonType.String;
        public override string ToString()
        {
            return _value;
        }
    }
}
