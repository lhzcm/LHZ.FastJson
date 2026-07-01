using LHZ.FastJson.JsonClass;
using System;
using System.Collections.Generic;
using System.Text;

namespace LHZ.FastJson
{
    /// <summary>
    /// JSON reader interface
    /// </summary>
    public interface IJsonReader
    {
        /// <summary>
        /// Read JSON
        /// </summary>
        /// <returns></returns>
        JsonObject JsonRead();

        /// <summary>
        /// Whether it is a valid JSON string
        /// </summary>
        bool IsValidJson { get; }
    }
}
