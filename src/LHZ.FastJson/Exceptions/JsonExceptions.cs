using System;
using System.Globalization;
using System.Linq;
using LHZ.FastJson.Enum;
using LHZ.FastJson.JsonClass;

namespace LHZ.FastJson.Exceptions
{
    /// <summary>
    /// Creates the exceptions thrown by the readers, the deserializer and the serializer.
    /// All message wording lives here so that it stays consistent: a short description of the problem
    /// followed by the position in the JSON text ("... at position 12.").
    /// </summary>
    internal static class JsonExceptions
    {
        #region Formatting helpers

        private static string At(int position)
        {
            return " at position " + position.ToString(CultureInfo.InvariantCulture) + ".";
        }

        /// <summary>
        /// Quote a character for use in a message; control characters are escaped so the message stays readable.
        /// </summary>
        private static string Quote(char c)
        {
            switch (c)
            {
                case '\n': return "'\\n'";
                case '\r': return "'\\r'";
                case '\t': return "'\\t'";
            }
            if (char.IsControl(c))
            {
                return "'\\u" + ((int)c).ToString("x4", CultureInfo.InvariantCulture) + "'";
            }
            return "'" + c + "'";
        }

        /// <summary>
        /// A short, readable type name for messages (generic arguments are expanded: List&lt;Int32&gt;).
        /// </summary>
        internal static string TypeName(Type type)
        {
            if (type == null)
            {
                return "(unknown)";
            }
            if (!type.IsGenericType)
            {
                return type.Name;
            }
            string name = type.Name;
            int tick = name.IndexOf('`');
            if (tick > 0)
            {
                name = name.Substring(0, tick);
            }
            return name + "<" + string.Join(", ", type.GetGenericArguments().Select(TypeName).ToArray()) + ">";
        }

        private static string Describe(JsonType type)
        {
            switch (type)
            {
                case JsonType.Content: return "object";
                case JsonType.Array: return "array";
                case JsonType.String: return "string";
                case JsonType.Number: return "number";
                case JsonType.Boolean: return "boolean";
                default: return "null";
            }
        }

        #endregion

        #region Reading: malformed or unexpectedly truncated JSON text (JsonReadException)

        /// <summary>
        /// The input is empty or null.
        /// </summary>
        public static JsonReadException EmptyInput(string paramName)
        {
            const string message = "The JSON string must not be null or empty.";
            return new JsonReadException(0, message, new ArgumentNullException(paramName, message));
        }

        public static JsonReadException UnexpectedEnd(int position)
        {
            return new JsonReadException(position, "Unexpected end of the JSON input" + At(position));
        }

        public static JsonReadException UnexpectedEnd(int position, Exception innerException)
        {
            return new JsonReadException(position, "Unexpected end of the JSON input" + At(position), innerException);
        }

        public static JsonReadException UnterminatedString(int position)
        {
            return new JsonReadException(position, "Unterminated string" + At(position));
        }

        public static JsonReadException UnescapedControlCharacter(int position)
        {
            return new JsonReadException(position, "Unescaped control character in string" + At(position));
        }

        public static JsonReadException InvalidEscapeSequence(int position, char escaped)
        {
            return new JsonReadException(position, "Invalid escape sequence '\\" + escaped + "' in string" + At(position));
        }

        public static JsonReadException InvalidUnicodeEscape(int position)
        {
            return new JsonReadException(position, "Invalid \\uXXXX escape sequence in string" + At(position));
        }

        public static JsonReadException IncompleteEscapeSequence(int position)
        {
            return new JsonReadException(position, "Incomplete escape sequence in string" + At(position));
        }

        public static JsonReadException ExpectedString(int position)
        {
            return new JsonReadException(position, "A string in double quotes was expected" + At(position));
        }

        public static JsonReadException ExpectedPropertyName(int position)
        {
            return new JsonReadException(position, "A property name (a string in double quotes) was expected" + At(position));
        }

        public static JsonReadException ExpectedColon(int position, char found)
        {
            return new JsonReadException(position, "Expected ':' after the property name but found " + Quote(found) + At(position));
        }

        public static JsonReadException ExpectedColonButEnd(int position)
        {
            return new JsonReadException(position, "Expected ':' after the property name but reached the end of the input" + At(position));
        }

        public static JsonReadException ExpectedCommaOrEndOfObject(int position, char found)
        {
            return new JsonReadException(position, "Expected ',' or '}' but found " + Quote(found) + At(position));
        }

        public static JsonReadException ExpectedCommaOrEndOfArray(int position, char found)
        {
            return new JsonReadException(position, "Expected ',' or ']' but found " + Quote(found) + At(position));
        }

        public static JsonReadException TrailingCommaInArray(int position)
        {
            return new JsonReadException(position, "A trailing comma is not allowed in an array" + At(position));
        }

        public static JsonReadException UnexpectedCharacter(int position, char found)
        {
            return new JsonReadException(position, "Unexpected character " + Quote(found) + At(position));
        }

        public static JsonReadException ExpectedValue(int position, char found)
        {
            return new JsonReadException(position, "Unexpected character " + Quote(found) + " where a JSON value was expected" + At(position));
        }

        public static JsonReadException TrailingCharacters(int position)
        {
            return new JsonReadException(position, "Unexpected characters after the end of the JSON value" + At(position));
        }

        public static JsonReadException MissingDigitsAfterMinus(int position)
        {
            return new JsonReadException(position, "A digit is required after the minus sign in a number" + At(position));
        }

        public static JsonReadException LeadingZeroInNumber(int position)
        {
            return new JsonReadException(position, "A number must not have leading zeros" + At(position));
        }

        public static JsonReadException InvalidNumber(int position)
        {
            return new JsonReadException(position, "Invalid number" + At(position));
        }

        public static JsonReadException MissingDigitsAfterDecimalPoint(int position)
        {
            return new JsonReadException(position, "A digit is required after the decimal point in a number" + At(position));
        }

        public static JsonReadException MissingDigitsInExponent(int position)
        {
            return new JsonReadException(position, "A digit is required in the exponent of a number" + At(position));
        }

        /// <summary>
        /// A literal that starts like true / false / null but is not spelled correctly.
        /// </summary>
        /// <param name="position">Position of the literal</param>
        /// <param name="expected">The accepted spelling(s), for example <c>true</c> or <c>false</c></param>
        public static JsonReadException InvalidLiteral(int position, string expected)
        {
            return new JsonReadException(position, "Invalid literal; expected " + expected + At(position));
        }

        public static JsonReadException DuplicateProperty(int position, string name, Exception innerException)
        {
            return new JsonReadException(position, "Duplicate property name '" + name + "' in a JSON object" + At(position), innerException);
        }

        public static JsonReadException MaxDepthExceeded(int position, int maxDepth)
        {
            return new JsonReadException(position, "The maximum nesting depth of " + maxDepth.ToString(CultureInfo.InvariantCulture) + " was exceeded" + At(position));
        }

        #endregion

        #region Deserializing: valid JSON that cannot be converted to the requested type (JsonDirectDeserializationException)

        /// <summary>
        /// The message for a JSON value of the wrong kind. Used directly by compiled expression trees.
        /// </summary>
        public static string TypeMismatchMessage(int position, JsonType actual, JsonType expected, Type target)
        {
            return "Cannot deserialize a JSON " + Describe(actual) + " as '" + TypeName(target) + "'; a JSON " + Describe(expected) + " is required" + At(position);
        }

        /// <summary>
        /// The reader is positioned on a JSON value whose kind is not <paramref name="expected"/>.
        /// </summary>
        public static JsonDirectDeserializationException TypeMismatch(JsonDirectReader reader, JsonType expected, Type target)
        {
            JsonType actual = reader.JsonType;
            return new JsonDirectDeserializationException(reader.Position, actual, target, TypeMismatchMessage(reader.Position, actual, expected, target));
        }

        /// <summary>
        /// A parsed JSON node (used when the target type is itself a JSON node type) has the wrong kind.
        /// </summary>
        public static JsonDirectDeserializationException NodeMismatch(JsonObject node, Type target)
        {
            return new JsonDirectDeserializationException(node.Position, node.Type, target,
                "Cannot convert a JSON " + Describe(node.Type) + " to '" + TypeName(target) + "'" + At(node.Position));
        }

        public static JsonDirectDeserializationException NullForValueType(JsonDirectReader reader, Type target)
        {
            return new JsonDirectDeserializationException(reader.Position, JsonType.Null, target,
                "Cannot deserialize a JSON null as the value type '" + TypeName(target) + "'" + At(reader.Position));
        }

        public static JsonDirectDeserializationException NoDefaultConstructor(JsonDirectReader reader, Type target)
        {
            return new JsonDirectDeserializationException(reader.Position, reader.JsonType, target,
                "Cannot create an instance of '" + TypeName(target) + "' because it has no public parameterless constructor" + At(reader.Position));
        }

        public static JsonDirectDeserializationException AbstractType(JsonDirectReader reader, Type target)
        {
            return new JsonDirectDeserializationException(reader.Position, reader.JsonType, target,
                "Cannot create an instance of '" + TypeName(target) + "' because it is an interface or an abstract type" + At(reader.Position));
        }

        public static JsonDirectDeserializationException NoCollectionImplementation(JsonDirectReader reader, Type target)
        {
            return new JsonDirectDeserializationException(reader.Position, reader.JsonType, target,
                "Cannot create an instance of the collection interface '" + TypeName(target) + "' because no default implementation is known" + At(reader.Position));
        }

        public static JsonDirectDeserializationException InvalidNullableType(JsonDirectReader reader, Type target)
        {
            return new JsonDirectDeserializationException(reader.Position, reader.JsonType, target,
                "The generic argument of '" + TypeName(target) + "' could not be determined" + At(reader.Position));
        }

        /// <summary>
        /// The type cannot be deserialized at all (for example the compiled deserializer could not be built for it).
        /// </summary>
        public static JsonDirectDeserializationException UnsupportedType(JsonDirectReader reader, Type target, Exception innerException)
        {
            return new JsonDirectDeserializationException(reader.Position, JsonType.Content, target,
                "The type '" + TypeName(target) + "' is not supported for deserialization: " + innerException.Message, innerException);
        }

        /// <summary>
        /// A JSON number is syntactically valid but does not fit the target numeric type (overflow, or a fraction for an integer).
        /// </summary>
        public static JsonDirectDeserializationException InvalidNumber(int position, string text, Type target, Exception innerException)
        {
            return new JsonDirectDeserializationException(position, JsonType.Number, target,
                "The JSON number '" + text + "' cannot be converted to '" + TypeName(target) + "' because it is out of range or not in a valid format" + At(position), innerException);
        }

        public static JsonDirectDeserializationException InvalidChar(int position)
        {
            return new JsonDirectDeserializationException(position, JsonType.String, typeof(char),
                "A JSON string must contain exactly one character to be deserialized as 'Char'" + At(position));
        }

        public static JsonDirectDeserializationException EnumValueNotDefined(int position, Type enumType, int value)
        {
            return new JsonDirectDeserializationException(position, JsonType.Number, enumType,
                "The value " + value.ToString(CultureInfo.InvariantCulture) + " is not defined in the enum '" + TypeName(enumType) + "'" + At(position));
        }

        public static JsonDirectDeserializationException EnumTypeMismatch(JsonDirectReader reader, Type enumType)
        {
            JsonType actual = reader.JsonType;
            return new JsonDirectDeserializationException(reader.Position, actual, enumType,
                "Cannot deserialize a JSON " + Describe(actual) + " as the enum '" + TypeName(enumType) + "'; a JSON number or string is required" + At(reader.Position));
        }

        public static JsonDirectDeserializationException InvalidEnumNumber(int position, string text, Type enumType, Exception innerException)
        {
            return new JsonDirectDeserializationException(position, JsonType.Number, enumType,
                "The JSON number '" + text + "' cannot be converted to the enum '" + TypeName(enumType) + "'" + At(position), innerException);
        }

        public static JsonDirectDeserializationException InvalidEnumName(int position, Type enumType, string text, Exception innerException)
        {
            return new JsonDirectDeserializationException(position, JsonType.String, enumType,
                "'" + text + "' is not a valid value of the enum '" + TypeName(enumType) + "'" + At(position), innerException);
        }

        public static JsonDirectDeserializationException InvalidGuid(int position, string text, Exception innerException)
        {
            return new JsonDirectDeserializationException(position, JsonType.String, typeof(Guid),
                "'" + text + "' is not a valid GUID" + At(position), innerException);
        }

        public static JsonDirectDeserializationException InvalidDateTime(int position, string text)
        {
            return new JsonDirectDeserializationException(position, JsonType.String, typeof(DateTime),
                "The string '" + text + "' cannot be parsed as a DateTime" + At(position));
        }

        public static JsonDirectDeserializationException InvalidDictionaryKey(JsonDirectReader reader, Type dictionaryType, Type keyType, string key, Exception innerException)
        {
            return new JsonDirectDeserializationException(reader.Position, JsonType.Content, dictionaryType,
                "The property name '" + key + "' cannot be converted to the dictionary key type '" + TypeName(keyType) + "'" + At(reader.Position), innerException);
        }

        public static JsonDirectDeserializationException DuplicateDictionaryKey(JsonDirectReader reader, Type dictionaryType, string key, Exception innerException)
        {
            return new JsonDirectDeserializationException(reader.Position, JsonType.Content, dictionaryType,
                "The dictionary '" + TypeName(dictionaryType) + "' already contains the key '" + key + "'" + At(reader.Position), innerException);
        }

        #endregion

        #region Serializing (JsonSerializationException)

        public static JsonSerializationException CircularReference(Type type)
        {
            return new JsonSerializationException(
                "A circular reference was detected while serializing an object of type '" + TypeName(type) + "'.", type);
        }

        public static JsonSerializationException DuplicatePropertyName(Type type, string propertyName)
        {
            return new JsonSerializationException(
                "The type '" + TypeName(type) + "' has more than one property that maps to the JSON property name '" + propertyName + "'.", type);
        }

        #endregion
    }
}
