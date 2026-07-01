using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using LHZ.FastJson.Enum;

namespace LHZ.FastJson.JsonClass
{
    /// <summary>
    /// JSON object class
    /// </summary>
    public abstract class JsonObject : IJsonObject
    {
        /// <summary>
        /// Parsed string, the position of the object in the string, -1 indicates the object was not obtained from string parsing
        /// </summary>
        internal protected int _position;

        internal JsonObject(int position)
        {
            this._position = position;
        }
        internal JsonObject()
        {
            _position = -1;
        }
        /// <summary>
        /// JSON object type
        /// </summary>
        public abstract JsonType Type { get;}
        /// <summary>
        /// Get object by string index
        /// </summary>
        /// <param name="index">String index</param>
        /// <returns>JSON object</returns>
        public virtual IJsonObject this[string index]
        {
            get
            {
                throw new InvalidOperationException($"{this.Type}并非是{JsonType.Content}无法调用该索引方法！");
            }
        }
        /// <summary>
        /// Get object by numeric index
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>JSON object</returns>
        public virtual IJsonObject this[int index]
        {
            get
            {
                throw new InvalidOperationException($"{this.Type}并非是{JsonType.Array}或{JsonType.Content}无法调用该索引方法！");
            }
        }
        /// <summary>
        /// Get object by JsonPropertyName index
        /// </summary>
        /// <param name="index">Property name index</param>
        /// <returns>JSON object</returns>
        public virtual IJsonObject this[JsonPropertyName index]
        {
            get
            {
                throw new InvalidOperationException($"{this.Type}并非是{JsonType.Content}无法调用该索引方法！");
            }
        }

        /// <summary>
        /// Determine if a child node with the specified name exists
        /// </summary>
        /// <param name="name">Node name</param>
        /// <returns>Whether it exists</returns>
        public bool HasChildrenNode(string name)
        {
            Dictionary<string, IJsonObject> obj = Value as Dictionary<string, IJsonObject>;
            if (obj == null)
            {
                return false;
            }
            return obj.ContainsKey(name);
        }

        /// <summary>
        /// JSON object value
        /// </summary>
        public virtual object Value => null;
        /// <summary>
        /// String position
        /// </summary>
        public int Position
        {
            get
            {
                return _position;
            }
        }

        /// <summary>
        /// Convert object to string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString()
        {
            return Value.ToString();
        }

        /// <summary>
        /// Convert object to JSON string
        /// </summary>
        /// <returns>JSON string</returns>
        public virtual string ToJsonString()
        {
            return ToJsonStringBuilder().ToString();
        }
        /// <summary>
        /// Serialize JSON object to StringBuilder
        /// </summary>
        public abstract StringBuilder ToJsonStringBuilder(StringBuilder stringBuilder = null);
    }
}
