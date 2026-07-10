using LHZ.FastJson.Enum;
using LHZ.FastJson.JsonClass;
using System;
using System.Collections.Generic;
using System.Text;

namespace LHZ.FastJson
{
    /// <summary>
    /// JSON object interface
    /// </summary>
    public interface IJsonObject
    {
        /// <summary>
        /// JSON object type
        /// </summary>
        JsonType Type { get;}
        /// <summary>
        /// JSON object value
        /// </summary>
        object Value { get; }
        /// <summary>
        /// Get child node by string name index
        /// </summary>
        IJsonObject this[string index] { get; }
        /// <summary>
        /// Get child node by property name index
        /// </summary>
        IJsonObject this[JsonPropertyName index] {get;}
        /// <summary>
        /// Get child node by numeric index
        /// </summary>
        IJsonObject this[int index] { get; }
        /// <summary>
        /// Convert JSON object to JSON string StringBuilder
        /// </summary>
        /// <returns>JSON StringBuilder string</returns>
        StringBuilder ToStringBuilder(StringBuilder stringBuilder = null);
        /// <summary>
        /// Returns the JSON string representation of the object
        /// </summary>
        string ToString();
    }
}
