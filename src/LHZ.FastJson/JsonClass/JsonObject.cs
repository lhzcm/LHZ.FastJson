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
        /// JSON object type
        /// </summary>
        public abstract JsonType Type { get;}
        ///<inheritdoc/>
        public abstract object Value { get; }
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
        /// String position
        /// </summary>
        internal virtual int Position 
        {
            get
            {
                throw new InvalidOperationException("Internal Class Exclusive Property");
            }
        }
        /// <summary>
        /// Convert object to JSON string
        /// </summary>
        /// <returns>String</returns>
        public override string ToString()
        {
            return ToStringBuilder().ToString();
        }
        /// <summary>
        /// Serialize JSON object to StringBuilder
        /// </summary>
        public abstract StringBuilder ToStringBuilder(StringBuilder stringBuilder = null);
    }
}
