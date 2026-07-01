using LHZ.FastJson.Enum;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace LHZ.FastJson.JsonClass
{
    /// <summary>
    /// JSON array object
    /// </summary>
    public class JsonArray : JsonObject, IEnumerable<IJsonObject>
    {
        private List<IJsonObject> _value;
        /// <inheritdoc/>
        public override object Value => _value;
        /// <summary>
        /// Get the array list (deprecated, use Value property instead)
        /// </summary>
        [Obsolete("This method is deprecated and will be removed in the next official release.")]
        public List<IJsonObject> GetValue()
        {
            return this._value;
        }
        internal JsonArray(int position) : base(position)
        {
            this._value = new List<IJsonObject>();
        }
        /// <summary>
        /// Default constructor
        /// </summary>
        public JsonArray()
        {
            this._value = new List<IJsonObject>();
        }

        /// <summary>
        /// Array length
        /// </summary>
        public int Length => _value.Count;

        /// <summary>
        /// Add a JSON object to the array
        /// </summary>
        /// <param name="obj">JSON object</param>
        public void AddJsonObject(JsonObject obj)
        {
            this._value.Add(obj);
        }

        /// <inheritdoc/>
        public override StringBuilder ToJsonStringBuilder(StringBuilder stringBuilder = null)
        {
            if(stringBuilder == null)
            {
                stringBuilder = new StringBuilder();
            }
            stringBuilder.Append("[");
            for (int i = 0; i < _value.Count; i++)
            {
                _value[i].ToJsonStringBuilder(stringBuilder);
                stringBuilder.Append(",");
            }
            if (_value.Count > 0)
            {
                stringBuilder.Remove(stringBuilder.Length - 1, 1);
            }
            stringBuilder.Append("]");
            return stringBuilder;
        }
        /// <summary>
        /// Get the enumerator for the JSON array
        /// </summary>
        
        public IEnumerator<IJsonObject> GetEnumerator()
        {
            return _value.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _value.GetEnumerator();
        }
        /// <summary>
        /// Get object by index
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>JSON object</returns>
        public override IJsonObject this[int index]
        {
            get
            {
                return _value[index];
            }
        }
        /// <summary>
        /// JSON object type
        /// </summary>
        public override JsonType Type => JsonType.Array;
    }
}
