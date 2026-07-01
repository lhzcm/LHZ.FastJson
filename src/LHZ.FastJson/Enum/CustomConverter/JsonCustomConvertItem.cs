using System;
using System.Collections.Generic;
using System.Text;

namespace LHZ.FastJson.Enum.CustomConverter
{
    /// <summary>
    /// Custom conversion items
    /// </summary>
    [Flags]
    public enum JsonCustomConvertItem
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,
        /// <summary>
        /// Custom serialization
        /// </summary>
        CustomSerialize = 1,
        /// <summary>
        /// Custom deserialization
        /// </summary>
        CustomDeSerialize = 2,
        /// <summary>
        /// All
        /// </summary>
        All = 3,
    }
}
