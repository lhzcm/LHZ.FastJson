using System;

namespace LHZ.FastJson
{
    /// <summary>
    /// Per-call JSON serialization / deserialization options.
    /// Pass an instance to the <see cref="JsonConvert"/> overloads that accept options.
    /// When no options are supplied, the defaults of this class are used.
    /// </summary>
    public class JsonConvertOptions
    {
        /// <summary>
        /// The default maximum nesting depth (the same default as System.Text.Json).
        /// </summary>
        public const int DefaultMaxDepth = 64;

        internal static readonly JsonConvertOptions Default = new JsonConvertOptions();

        private int _maxDepth = DefaultMaxDepth;

        /// <summary>
        /// Number of defined <see cref="CaseStyle"/> values; the size of the per-style expression caches.
        /// </summary>
        internal const int CaseStyleCount = 4;

        private CaseStyle _caseStyle = CaseStyle.PascalCase;

        /// <summary>
        /// Naming style applied to property names during serialization and deserialization.
        /// A property marked with <see cref="Json.Attributes.JsonPropertyAttribute"/> always keeps
        /// its explicitly configured name. Default: <see cref="FastJson.CaseStyle.PascalCase"/>
        /// (property names are used exactly as declared).
        /// </summary>
        public CaseStyle CaseStyle
        {
            get { return _caseStyle; }
            set
            {
                if ((int)value < 0 || (int)value >= CaseStyleCount)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Undefined CaseStyle value.");
                }
                _caseStyle = value;
            }
        }

        /// <summary>
        /// Maximum nesting depth of JSON objects and arrays accepted by the readers.
        /// This limit prevents stack overflows caused by deeply nested input.
        /// Default: 64.
        /// </summary>
        public int MaxDepth
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
        /// Resolve the options to use: the supplied instance, or the defaults when <c>null</c>.
        /// </summary>
        internal static JsonConvertOptions Resolve(JsonConvertOptions options)
        {
            return options ?? Default;
        }

        /// <summary>
        /// Convert a property name to the given naming style.
        /// <list type="bullet">
        /// <item><description><see cref="FastJson.CaseStyle.PascalCase"/>: returned unchanged.</description></item>
        /// <item><description><see cref="FastJson.CaseStyle.CamelCase"/>: <c>UserName</c> -> <c>userName</c>, <c>URLValue</c> -> <c>urlValue</c>.</description></item>
        /// <item><description><see cref="FastJson.CaseStyle.SnakeCase"/>: <c>UserName</c> -> <c>user_name</c>, <c>HTTPServer2Id</c> -> <c>http_server2_id</c>.</description></item>
        /// <item><description><see cref="FastJson.CaseStyle.KebabCase"/>: <c>UserName</c> -> <c>user-name</c>.</description></item>
        /// </list>
        /// </summary>
        /// <param name="name">The property name</param>
        /// <param name="style">The target naming style</param>
        /// <returns>The converted name; null or empty names are returned unchanged</returns>
        public static string ConvertName(string name, CaseStyle style)
        {
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }
            switch (style)
            {
                case CaseStyle.PascalCase: return name;
                case CaseStyle.CamelCase: return ToCamelCase(name);
                case CaseStyle.SnakeCase: return ToDelimitedLowerCase(name, '_');
                case CaseStyle.KebabCase: return ToDelimitedLowerCase(name, '-');
                default: throw new ArgumentOutOfRangeException(nameof(style), "Undefined CaseStyle value.");
            }
        }

        private static string ToCamelCase(string name)
        {
            if (!char.IsUpper(name[0]))
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

        /// <summary>
        /// Split a name into words and join them in lower case with the given delimiter.
        /// Word boundaries: lower case/digit followed by upper case, the end of an upper-case run
        /// followed by a lower-case letter (URLValue -> url_value), and existing '_' / '-' characters.
        /// Digits never start a new word.
        /// </summary>
        private static string ToDelimitedLowerCase(string name, char delimiter)
        {
            var sb = new System.Text.StringBuilder(name.Length + 4);
            bool pendingBoundary = false;
            for (int i = 0; i < name.Length; i++)
            {
                char c = name[i];
                if (c == '_' || c == '-')
                {
                    pendingBoundary = sb.Length > 0;
                    continue;
                }
                if (i > 0 && char.IsUpper(c))
                {
                    char prev = name[i - 1];
                    bool nextIsLower = i + 1 < name.Length && char.IsLower(name[i + 1]);
                    if (char.IsLower(prev) || char.IsDigit(prev) || (char.IsUpper(prev) && nextIsLower))
                    {
                        pendingBoundary = true;
                    }
                }
                if (pendingBoundary)
                {
                    sb.Append(delimiter);
                    pendingBoundary = false;
                }
                sb.Append(char.ToLowerInvariant(c));
            }
            return sb.ToString();
        }
    }
}
