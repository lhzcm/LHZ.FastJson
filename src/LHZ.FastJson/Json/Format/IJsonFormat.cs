using LHZ.FastJson.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LHZ.FastJson.Json.Format
{
    /// <summary>
    /// JSON formatting interface (deprecated)
    /// </summary>
    [Obsolete("This interface is obsolete. Use alternative formatting methods instead.")]
    public interface IJsonFormat
    {
        /// <summary>
        /// Format object type
        /// </summary>
        ObjectType Type { get; }
    }
}
