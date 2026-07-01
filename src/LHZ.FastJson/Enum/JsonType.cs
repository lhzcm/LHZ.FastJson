using System;
using System.Collections.Generic;
using System.Text;

namespace LHZ.FastJson.Enum
{
    /// <summary>
    /// JSON data type enumeration
    /// </summary>
    public enum JsonType
    {
        /// <summary>
        /// JSON object type
        /// </summary>
        Content,
        /// <summary>
        /// JSON array type
        /// </summary>
        Array,
        /// <summary>
        /// JSON string type
        /// </summary>
        String,
        /// <summary>
        /// JSON number type
        /// </summary>
        Number,
        /// <summary>
        /// JSON boolean type
        /// </summary>
        Boolean,
        /// <summary>
        /// JSON null type
        /// </summary>
        Null
    }
}
