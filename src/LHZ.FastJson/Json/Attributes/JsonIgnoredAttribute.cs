using LHZ.FastJson.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LHZ.FastJson.Json.Attributes
{
    /// <summary>
    /// JSON ignored attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class JsonIgnoredAttribute : Attribute
    {
        private JsonMethods _jsonIgnoredMethods = JsonMethods.All;
        /// <summary>
        /// Default constructor, ignore both serialization and deserialization
        /// </summary>
        public JsonIgnoredAttribute()
        { 
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ignoredMethod">The JSON method to ignore</param>
        public JsonIgnoredAttribute(JsonMethods ignoredMethod)
        {
            this._jsonIgnoredMethods = ignoredMethod;
        }
        /// <summary>
        /// Get the JSON method to ignore
        /// </summary>
        public JsonMethods JsonIgnoredMethod => _jsonIgnoredMethods;
    }
}
