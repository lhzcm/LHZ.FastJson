using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LHZ.FastJson.Json.Attributes
{
    /// <summary>
    /// JSON property name attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class JsonPropertyAttribute : Attribute
    {
        private readonly string _name;
        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="name">Custom property name</param>
        public JsonPropertyAttribute(string name)
        {
            _name = name ?? throw new ArgumentNullException(nameof(name));
        }

        /// <summary>
        /// Property name
        /// </summary>
        public string PropertyName => _name;
    }
}
