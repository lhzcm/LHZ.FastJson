using System;
using System.Collections.Generic;
using System.Text;
using LHZ.FastJson.Enum;

namespace LHZ.FastJson.JsonClass
{
    /// <summary>
    /// JSON string type
    /// </summary>
    public class JsonString : JsonObject
    {
        private string _value;
        /// <inheritdoc/>
        public override object Value => _value;
        /// <summary>
        /// Get string value (deprecated, use Value property instead)
        /// </summary>
        [Obsolete("This method is deprecated and will be removed in the next official release.")]
        public string GetValue()
        {
            return this._value.ToString();
        }
        internal JsonString(string value, int position) : base(position)
        {
            this._value = value;
        }
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
            this._value = value;
        }
        /// <inheritdoc/>
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
        /// <inheritdoc/>
        public override string ToString()
        {
            return _value;
        }
    }
}
