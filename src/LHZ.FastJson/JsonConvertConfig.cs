using System;

namespace LHZ.FastJson
{
    /// <summary>
    /// Global JSON serialization / deserialization configuration.
    /// Changing a naming setting invalidates the compiled expression caches,
    /// so subsequent serialization / deserialization calls pick up the new value.
    /// </summary>
    public static class JsonConvertConfig
    {
        private static volatile bool _useCamelCase;
        private static volatile int _configVersion;
        private static volatile int _maxDepth = 64;

        /// <summary>
        /// Whether property names should be converted to camelCase during serialization,
        /// and camelCase names should be matched during deserialization.
        /// A property marked with <see cref="Json.Attributes.JsonPropertyAttribute"/> always keeps
        /// its explicitly configured name. Default: false.
        /// </summary>
        public static bool UseCamelCase
        {
            get { return _useCamelCase; }
            set
            {
                if (_useCamelCase == value)
                {
                    return;
                }
                _useCamelCase = value;
                _configVersion++;
            }
        }

        /// <summary>
        /// Current configuration version.
        /// Increments each time a configuration value changes,
        /// and is used to invalidate the compiled expression caches.
        /// </summary>
        internal static int ConfigVersion
        {
            get { return _configVersion; }
        }

        /// <summary>
        /// Maximum nesting depth of JSON objects and arrays accepted by the readers.
        /// This limit prevents stack overflows caused by deeply nested input.
        /// Default: 64 (the same default as System.Text.Json).
        /// </summary>
        public static int MaxDepth
        {
            get { return _maxDepth; }
            set
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "MaxDepth must be greater than zero.");
                }
                _maxDepth = value;
            }
        }

        /// <summary>
        /// Convert a property name to camelCase (e.g. "UserName" -> "userName", "URLValue" -> "urlValue").
        /// Names that are empty or do not start with an uppercase letter are returned unchanged.
        /// </summary>
        /// <param name="name">The property name</param>
        /// <returns>The camelCase name</returns>
        public static string ToCamelCase(string name)
        {
            if (string.IsNullOrEmpty(name) || !char.IsUpper(name[0]))
            {
                return name;
            }
            char[] chars = name.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                if (i == 1 && !char.IsUpper(chars[i]))
                {
                    break;
                }
                bool hasNext = (i + 1 < chars.Length);
                if (i > 0 && hasNext && !char.IsUpper(chars[i + 1]))
                {
                    //The next character is lowercase: lowercase it and stop the acronym run
                    chars[i + 1] = char.ToLowerInvariant(chars[i + 1]);
                    break;
                }
                chars[i] = char.ToLowerInvariant(chars[i]);
            }
            return new string(chars);
        }
    }
}
