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
        protected readonly List<IJsonObject> _value;
        /// <summary>
        /// Default constructor
        /// </summary>
        public JsonArray()
        {
            this._value = new List<IJsonObject>();
        }
        ///<inheritdoc/>
        public override object Value => this._value;
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
        /// <summary>
        /// Add a JSON object to the array
        /// </summary>
        /// <param name="obj">JSON object</param>
        public void AddJsonObject(IJsonObject obj)
        {
            this._value.Add(obj);
        }
        /// <inheritdoc/>
        public override StringBuilder ToStringBuilder(StringBuilder stringBuilder = null)
        {
            if(stringBuilder == null)
            {
                stringBuilder = new StringBuilder();
            }
            stringBuilder.Append('[');
            for (int i = 0; i < _value.Count; i++)
            {
                _value[i].ToStringBuilder(stringBuilder);
                stringBuilder.Append(',');
            }
            if (_value.Count > 0)
            {
                stringBuilder.Remove(stringBuilder.Length - 1, 1);
            }
            stringBuilder.Append(']');
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
