using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using LHZ.FastJson.Exceptions;
using LHZ.FastJson.Enum;
using System.Reflection;

namespace LHZ.FastJson.JsonClass
{
    /// <summary>
    /// JSON container object
    /// </summary>
    public class JsonContent : JsonObject, IEnumerable<KeyValuePair<JsonPropertyName, IJsonObject>>
    {
        protected readonly Dictionary<JsonPropertyName, IJsonObject> _value;
        /// <summary>
        /// Default constructor
        /// </summary>
        public JsonContent()
        {
            this._value = new Dictionary<JsonPropertyName, IJsonObject>();
        }
        ///<inheritdoc/>
        public override object Value => _value;
        /// <summary>
        /// Add a JSON object property to the JSON container (string name overload)
        /// </summary>
        /// <param name="name">Property name</param>
        /// <param name="value">Property value</param>
        public void AddJsonProperty(string name, IJsonObject value)
        {
            AddJsonProperty(new JsonPropertyName(name), value);
        }
        /// <summary>
        /// Add a JSON object property to the JSON container
        /// </summary>
        /// <param name="jsonPropertyName">Property name</param>
        /// <param name="value">JSON object</param>
        public void AddJsonProperty(JsonPropertyName jsonPropertyName, IJsonObject value)
        {
#if NET5_0_OR_GREATER
            if (!_value.TryAdd(jsonPropertyName, value))
            {
                throw new Exception($"Json对象中已经存在属性名为[{jsonPropertyName}]的属性，无法添加重复属性！");
            }
#else
            if (_value.ContainsKey(jsonPropertyName))
            {
                throw new Exception($"Json对象中已经存在属性名为[{jsonPropertyName}]的属性，无法添加重复属性！");
            }
            _value.Add(jsonPropertyName, value);
#endif
        }
        /// <summary>
        /// Serialize the JSON container to a JSON string
        /// </summary>
        /// <returns></returns>
        public override StringBuilder ToStringBuilder(StringBuilder stringBuilder = null)
        {
            if(stringBuilder == null)
            {
                stringBuilder = new StringBuilder();
            }
            stringBuilder.Append('{');
            foreach (var item in this)
            {
                stringBuilder.Append('"');
                stringBuilder.Append(item.Key);
                stringBuilder.Append('"');
                stringBuilder.Append(':');
                item.Value.ToStringBuilder(stringBuilder);
                stringBuilder.Append(',');
            }
            if (_value.Count > 0)
            {
                stringBuilder.Remove(stringBuilder.Length - 1, 1);
            }
            stringBuilder.Append('}');
            return stringBuilder;
        }
        /// <summary>
        /// Get the enumerator for the JSON container
        /// </summary>
        public virtual IEnumerator<KeyValuePair<JsonPropertyName, IJsonObject>> GetEnumerator()
        {
            return _value.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this._value.GetEnumerator();
        }

        /// <summary>
        /// Get object by string index
        /// </summary>
        /// <param name="index">String index</param>
        /// <returns>JSON object</returns>
        public override IJsonObject this[string index]
        {
            get
            {
                return this[new JsonPropertyName(index)];
            }
        }
        /// <inheritdoc/>
        public override IJsonObject this[JsonPropertyName jsonPropertyName]
        {
            get
            {
                if (!_value.TryGetValue(jsonPropertyName, out var value))
                {
                    return null;
                }
                return value;
            }
        }
        /// <summary>
        /// Get object by numeric index
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>JSON object</returns>
        public override IJsonObject this[int index]
        {
            get
            {
                int i = 0;
                foreach (var item in this)
                {
                    if (index == i)
                    {
                        return item.Value;
                    }
                    i++;
                }
                throw new ArgumentOutOfRangeException();
            }
        }
        /// <summary>
        /// JSON object type
        /// </summary>
        public override JsonType Type => JsonType.Content;
    }
}
