using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using LHZ.FastJson.Enum;

namespace LHZ.FastJson.JsonClass
{
    /// <summary>
    /// JSON string type
    /// </summary>
    public class JsonString : JsonObject
    {
        internal StringView _value;
        ///<inheritdoc/>
        public override object Value => _value.ToString();
        /// <summary>
        /// Initialize with string value
        /// </summary>
        /// <param name="value">String value, cannot be null</param>
        /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
        public JsonString(string value)
        {
            if(value == null)
            {
                throw new ArgumentNullException(nameof(value), "value is not allow null");
            }
            this._value = new StringView(value);
        }
        internal JsonString(StringView value)
        {
            this._value = value;
        }
        /// <inheritdoc/>
        public override StringBuilder ToStringBuilder(StringBuilder stringBuilder = null)
        {
            if(stringBuilder == null)
            {
                stringBuilder = new StringBuilder();
            }

            stringBuilder.Append('\"');
            foreach(var item in _value.SourceString)
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
        /// Character escaping
        /// </summary>
        /// <param name="paraphrase">Character to escape</param>
        /// <returns>Escaped string</returns>
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
        /// JSON object type
        /// </summary>
        /// <inheritdoc/>
        public override JsonType Type => JsonType.String;
    }
}
