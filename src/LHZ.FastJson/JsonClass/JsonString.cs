using System;
using System.Collections.Generic;
using System.Text;

namespace LHZ.FastJson.JsonClass
{
    /// <summary>
    /// Json字符串类型
    /// </summary>
    public class JsonString : JsonObject
    {
        private string _value;
        public override object Value => _value;
        public string GetValue()
        {
            return this._value;
        }
        public JsonString(string value, int position) : base(position)
        {
            this.Type = Enum.JsonType.String;
            this._value = value;
        }

        public override string ToJsonString()
        {
            StringBuilder strBuilder = new StringBuilder();
            strBuilder.Append('\"');
            foreach (var item in _value)
            {
                if (item > '"')
                    strBuilder.Append(item);
                else
                    strBuilder.Append(CharParaphrase(item));
            }
            strBuilder.Append('\"');
            return strBuilder.ToString();
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
            else if (paraphrase == '\a')
                return "\\a";
            else if (paraphrase == '\b')
                return "\\b";
            else if (paraphrase == '\f')
                return "\\f";
            else if (paraphrase == '\r')
                return "\\r";
            else if (paraphrase == '\v')
                return "\\v";
            return paraphrase.ToString();
        }
    }
}
