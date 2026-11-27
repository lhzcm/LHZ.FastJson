using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using LHZ.FastJson.Enum;
using LHZ.FastJson.Exceptions;
using LHZ.FastJson.JsonClass;
using LHZ.FastJson.JsonClass.Internal;
using System.Runtime.CompilerServices;

namespace LHZ.FastJson
{
    internal sealed class JsonDirectReader
    {
        /// <summary>
        /// Initializes a new instance of the StringReader class with the specified string.
        /// </summary>
        /// <param name="content">The string to read.</param>
        internal JsonDirectReader(string content)
            : this(content, null)
        {
        }
        /// <summary>
        /// Initializes a new instance of the JsonDirectReader class with the specified string and options.
        /// </summary>
        /// <param name="content">The string to read.</param>
        /// <param name="options">The options (null uses the defaults).</param>
        internal JsonDirectReader(string content, JsonConvertOptions options)
        {
            Options = JsonConvertOptions.Resolve(options);
            if(string.IsNullOrEmpty(content))
            {
                throw JsonExceptions.EmptyInput(nameof(content));
            }
            Content =  content;
            Length = content.Length;
            SkipWhitespace();
        }
        /// <summary>
        /// Gets the content of the string being read.
        /// </summary>
        public string Content { get; private set; }
        /// <summary>
        /// Gets the options used while reading and deserializing.
        /// </summary>
        internal JsonConvertOptions Options { get; private set; }
        /// <summary>
        /// Gets the current position in the string being read.
        /// </summary>
        public int Position { get; private set; }
        /// <summary>
        /// Gets the length of the string being read.
        /// </summary>
        public int Length { get; private set; }
        public bool IsReadEnd => Position >= Length;

        private int _depth;

        /// <summary>
        /// Tracks the current object/array nesting depth and rejects input that exceeds the configured limit.
        /// </summary>
        private void EnterContainer()
        {
            if (_depth >= Options.MaxDepth)
            {
                throw JsonExceptions.MaxDepthExceeded(Position, Options.MaxDepth);
            }
            _depth++;
        }
        public JsonType JsonType
        {
            get
            {
                if(Position >= Length)
                {
                    throw JsonExceptions.UnexpectedEnd(Position - 1);
                }
                var currentChar = Content[Position];
                switch (currentChar)
                {
                    case '{': return JsonType.Content;
                    case '"': return JsonType.String;
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                    case '-':
                    case '+': return JsonType.Number;
                    case 'n': return JsonType.Null;
                    case '[': return JsonType.Array;
                    case 't':
                    case 'f': return JsonType.Boolean;
                    default: throw JsonExceptions.ExpectedValue(Position, currentChar);
                }
            }
        }
        /// <summary>
        /// inscrease the specified number of characters in the string and advances the position accordingly.
        /// </summary>
        /// <param name="count">The number of characters to inscrease.</param>
        /// <exception cref="JsonReadException"></exception>
        #if NET45_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        #endif
        private void MoveNext(int count)
        {
            if (Position + count > Length)
            {
                throw JsonExceptions.UnexpectedEnd(Length, new InvalidOperationException("End of string reached."));
            }
            Position += count;
        }
        /// <summary>
        /// inscrease the specified number of characters in the string and advances the position accordingly.
        /// and
        /// Skips whitespace characters in the string and advances the position accordingly.
        /// </summary>
        /// <param name="count">The number of characters to inscrease.</param>
        /// <exception cref="JsonReadException"></exception>
        #if NET45_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        #endif
        private void MoveNextAndSkipWhitespace(int count)
        {
            if (Position + count > Length)
            {
                throw JsonExceptions.UnexpectedEnd(Length, new InvalidOperationException("End of string reached."));
            }
            Position += count;
            if(Position >= Length)
                return;
            var curChar = Content[Position];
            while ((curChar == ' ' || curChar == '\r' || curChar == '\n' || curChar == '\t') && Position < (Length - 1))
            {
                curChar = Content[++Position];
            }
        }
        /// <summary>
        /// Skips whitespace characters in the string and advances the position accordingly.
        /// </summary>
        #if NET45_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        #endif
        public void SkipWhitespace()
        {
            if(Position >= Length)
                return;
            var curChar = Content[Position];
            while ((curChar == ' ' || curChar == '\r' || curChar == '\n' || curChar == '\t') && Position < (Length - 1))
            {
                curChar = Content[++Position];
            }
        }
        /// <summary>
        /// Reads a JSON property name from the string and returns it as a JsonPropertyName object.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="JsonReadException"></exception>
        #if NET45_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        #endif
        public JsonPropertyName ReadJsonPropertyName()
        {
            if (Position >= Length)
            {
                throw JsonExceptions.UnexpectedEnd(Position);
            }
            if (Content[Position] != '"')
            {
                int index = Position;
                throw JsonExceptions.ExpectedPropertyName(index);
            }
            Position++;
            int startPosition = Position;
            uint hash = 5381;
            while (Position < Length)
            {
                var curChar = Content[Position];
                if (curChar < 0x20)
                {
                    int curIndex = Position;
                    throw JsonExceptions.UnescapedControlCharacter(curIndex);
                }
                if (curChar == '"')
                {
                    int length = (int)(Position - startPosition);
                    MoveNextAndSkipWhitespace(1);
                    if (Position >= Length)
                    {
                        throw JsonExceptions.ExpectedColonButEnd(Position);
                    }
                    if(Content[Position] != ':')
                    {
                        throw JsonExceptions.ExpectedColon(Position, Content[Position]);
                    }
                    MoveNextAndSkipWhitespace(1);
                    return new JsonPropertyName(new StringView(Content, startPosition, length), (int)hash);
                }
                hash = (hash << 5) + hash + curChar;
                Position++;
            }
            throw JsonExceptions.UnterminatedString(Position);
        }
        #if NET45_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        #endif
        public IEnumerable<KeyValuePair<JsonPropertyName, JsonDirectReader>> ReadContent()
        {
            EnterContainer();
            try
            {
                while(true)
                {
                    if (Position >= Length)
                    {
                        throw JsonExceptions.UnterminatedString(Position);
                    }
                    switch (Content[Position])
                    {
                        case '{':
                            {
                                MoveNextAndSkipWhitespace(1);
                                if (Position >= Length)
                                {
                                    throw JsonExceptions.UnterminatedString(Position);
                                }
                                if(Content[Position] == '}')
                                {
                                    MoveNextAndSkipWhitespace(1);
                                    yield break;
                                }
                                break;
                            }
                        case ',' : MoveNextAndSkipWhitespace(1); break;
                        case '}' : MoveNextAndSkipWhitespace(1);yield break;
                        default: throw JsonExceptions.UnexpectedCharacter(Position, Content[Position]);
                    }
                    var propertyName = ReadJsonPropertyName();
                    yield return new KeyValuePair<JsonPropertyName, JsonDirectReader>(propertyName, this);
                }
            }
            finally
            {
                _depth--;
            }
        }
        #if NET45_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        #endif
        public IEnumerable<JsonDirectReader> ReadArray()
        {
            EnterContainer();
            try
            {
                while(true)
                {
                    if (Position >= Length)
                    {
                        throw JsonExceptions.UnterminatedString(Position);
                    }
                    switch(Content[Position])
                    {
                        case '[' :
                        {
                                MoveNextAndSkipWhitespace(1);
                                if (Position >= Length)
                                {
                                    throw JsonExceptions.UnterminatedString(Position);
                                }
                                if(Content[Position] == ']')
                                {
                                    MoveNextAndSkipWhitespace(1);
                                    yield break;
                                }
                                break;
                            }
                        case ',' : MoveNextAndSkipWhitespace(1); break;
                        case ']' : MoveNextAndSkipWhitespace(1); yield break;
                        default: throw JsonExceptions.UnexpectedCharacter(Position, Content[Position]);
                    }
                    yield return this;
                }
            }
            finally
            {
                _depth--;
            }
        }
        /// <summary>
        /// Read as String
        /// </summary>
        /// <returns></returns>
        /// <exception cref="JsonReadException"></exception>
        #if NET45_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        #endif
        public string ReadString()
        {
            if (Content[Position] != '"')
            {
                throw JsonExceptions.ExpectedString(Position);
            }
            MoveNext(1);
            var startPosition = Position;
            StringBuilder stringBuilder = null;
            while (Position < Length)
            {
                char current = Content[Position];
                if (current == '"')
                {
                    if(stringBuilder == null)
                    {
                        var str = Content.Substring(startPosition, (int)(Position - startPosition));
                        MoveNextAndSkipWhitespace(1);
                        return str;
                    }
                    stringBuilder.Append(Content, startPosition, (int)(Position - startPosition));
                    MoveNextAndSkipWhitespace(1);
                    return stringBuilder.ToString();
                }

                if (current < 0x20)
                {
                    throw JsonExceptions.UnescapedControlCharacter(Position);
                }
                if (current == '\\')
                {
                    if(stringBuilder == null)
                    {
                        stringBuilder = new StringBuilder();
                    }
                    stringBuilder.Append(Content, startPosition, (int)(Position - startPosition));
                    MoveNext(1);
                    if (Position >= Length)
                    {
                        throw JsonExceptions.UnterminatedString(Position);
                    }
                    current = Content[Position];
                    switch (current)
                    {
                            case '"': stringBuilder.Append('\"'); break;
                            case '\\': stringBuilder.Append('\\'); break;
                            case '/': stringBuilder.Append('/'); break;
                            case 'b': stringBuilder.Append('\b'); break;
                            case 'f': stringBuilder.Append('\f'); break;
                            case 'n': stringBuilder.Append('\n'); break;
                            case 'r': stringBuilder.Append('\r'); break;
                            case 't': stringBuilder.Append('\t'); break;
                            case 'u': 
                                {
                                    int value = 0;
                                    for (int i = 0; i < 4; i++)
                                    {
                                        MoveNext(1);
                                        if (Position >= Length)
                                        {
                                            throw JsonExceptions.UnterminatedString(Position);
                                        }
                                        if(JsonClass.Internal.JsonString.TryHexToInt(Content[Position], out int hexValue))
                                        {
                                            value = (value << 4) + hexValue;
                                        }
                                        else
                                        {
                                            throw JsonExceptions.InvalidUnicodeEscape(Position);
                                        }
                                    }
                                    stringBuilder.Append((char)value);
                                }; break;
                        default:
                            throw JsonExceptions.InvalidEscapeSequence(Position, current);
                    }
                    MoveNext(1);
                    startPosition = Position;
                    continue;
                }
                Position++;
            }
            throw JsonExceptions.UnterminatedString(Position);
        }
        /// <summary>
        /// Read a JSON string and parse it as a DateTime.
        /// Strings without escape sequences are parsed in place, without allocating a substring.
        /// </summary>
        /// <returns>The parsed value</returns>
        /// <exception cref="JsonReadException"></exception>
        internal DateTime ReadDateTime()
        {
            int startPosition = Position;
            while (++Position < Length)
            {
                char c = Content[Position];
                if (c == '"')
                {
                    break;
                }
                if (c == '\\' || c < 0x20)
                {
                    //Escapes or control characters: let ReadString decode / report them
                    Position = -1;
                    break;
                }
            }

            DateTime result;
            if (Position > startPosition + 1 && Position < Length)
            {
                int offset = startPosition + 1;
                int length = Position - offset;
                MoveNextAndSkipWhitespace(1);
                if (TryParseDateTime(Content, offset, length, out result))
                {
                    return result;
                }
                throw JsonExceptions.InvalidDateTime(startPosition, Content.Substring(offset, length));
            }
            Position = startPosition;
            var str = ReadString();
            if (TryParseDateTime(str, 0, str.Length, out result))
            {
                return result;
            }
            throw JsonExceptions.InvalidDateTime(startPosition, str);
        }

        private bool TryParseDateTime(string source, int offset, int length, out DateTime result)
        {
            if (Utils.DateTimeFormatter.TryParseIso(source, offset, length, out result))
            {
                return true;
            }
            string text = source.Substring(offset, length);
            string format = Options.DateTimeFormat;
            if (format != null && DateTime.TryParseExact(text, format, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.RoundtripKind, out result))
            {
                return true;
            }
            return DateTime.TryParse(text, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.RoundtripKind, out result);
        }
        /// <summary>
        /// Read JSON Number object
        /// </summary>
        /// <returns>StringView object</returns>
        #if NET45_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        #endif
        internal StringView ReadNumber()
        {
            int startPosition = Position;
            if (Content[Position] == '-')
            {
                if (++Position >= Length || !JsonReader.IsDigit(Content[Position]))
                {
                    throw JsonExceptions.MissingDigitsAfterMinus(Position);
                }
            }
            if (Content[Position] == '0')
            {
                if (++Position < Length && JsonReader.IsDigit(Content[Position]))
                {
                    throw JsonExceptions.LeadingZeroInNumber(Position);
                }
            }
            else if (JsonReader.IsOneToNine(Content[Position]))
            {
                while (Position < Length && JsonReader.IsDigit(Content[Position]))
                {
                    Position++;
                }
            }
            else
            {
                throw JsonExceptions.InvalidNumber(Position);
            }
            if (Position < Length && Content[Position] == '.')
            {
                if (++Position >= Length || !JsonReader.IsDigit(Content[Position]))
                {
                    throw JsonExceptions.MissingDigitsAfterDecimalPoint(Position);
                }
                while (Position < Length && JsonReader.IsDigit(Content[Position]))
                {
                    Position++;
                }
            }
            if (Position < Length && (Content[Position] == 'e' || Content[Position] == 'E'))
            {
                if (++Position < Length && (Content[Position] == '+' || Content[Position] == '-'))
                {
                    MoveNext(1);
                }
                if (Position >= Length || !JsonReader.IsDigit(Content[Position]))
                {
                    throw JsonExceptions.MissingDigitsInExponent(Position);
                }
                while (Position < Length && JsonReader.IsDigit(Content[Position]))
                {
                    Position++;
                }
            }
            var ret = new JsonClass.Internal.StringView(Content, startPosition, (int)(Position - startPosition));
            SkipWhitespace();
            return ret;
        }
         /// <summary>
        /// Parse JSON Boolean object
        /// </summary>
        /// <returns>JSON object</returns>
        #if NET45_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        #endif
        internal bool ReadBoolean()
        {
            int startPosition = Position;
            if (Content[startPosition] == 't')
            {
                MoveNextAndSkipWhitespace(4);
                if (Content[startPosition + 1] == 'r' && Content[startPosition + 2] == 'u' && Content[startPosition + 3] == 'e')
                {
                    return true;
                }
            }
            else
            {
                MoveNextAndSkipWhitespace(5);
                if (Content[startPosition + 1] == 'a' && Content[startPosition + 2] == 'l' && Content[startPosition + 3] == 's' && Content[startPosition + 4] == 'e')
                {
                    return false;
                }
            }
            throw JsonExceptions.InvalidLiteral(startPosition, "'true' or 'false'");
        }
        /// <summary>
        /// Parse JSON Null
        /// </summary>
        /// <returns>JSON object</returns>
        #if NET45_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        #endif
        internal T ReadNull<T>() where T : class
        {
            var startPosition = Position;
            MoveNextAndSkipWhitespace(4);
            if (Content[++startPosition] == 'u' && Content[++startPosition] == 'l' && Content[++startPosition] == 'l')
            {
                return null;
            }
            throw JsonExceptions.InvalidLiteral(Position - 4, "'null'");
        }
        /// <summary>
        /// Skip current json object
        /// </summary>
        #if NET45_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        #endif
        internal void SkipCurrentObject()
        {
            if (Position >= Length)
            {
                throw JsonExceptions.UnterminatedString(Position);
            }
            switch (Content[Position])
            {
                case '{':
                    {
                        foreach (var item in ReadContent())
                        {
                            item.Value.SkipCurrentObject();
                        }
                        break;
                    }
                case '"': ReadString(); break;
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                case '-':
                case '+': ReadNumber(); break;
                case 'n': ReadNull<object>(); break;
                case '[':
                    {
                        foreach (var item in ReadArray())
                        {
                            item.SkipCurrentObject();
                        }
                        break;
                    }
                case 't':
                case 'f': ReadBoolean(); break;
                default: throw JsonExceptions.ExpectedValue(Position, Content[Position]);
            }
        }
        #if NET45_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        #endif
        public JsonObject ReadAsJsonObject()
        {
            var reader = new JsonReader(Content, Position);
            var obj = reader.JsonRead();
            Position = reader.EndPosition;
            SkipWhitespace();
            return obj;
        }
        public char CurrentChar => Content[Position];
    }
}
