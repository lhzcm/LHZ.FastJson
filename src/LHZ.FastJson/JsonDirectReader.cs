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
    internal class JsonDirectReader
    {
        /// <summary>
        /// Initializes a new instance of the StringReader class with the specified string.
        /// </summary>
        /// <param name="content">The string to read.</param>
        internal JsonDirectReader(string content)
        {
            Content = content ?? throw new ArgumentNullException(nameof(content));
            Position = 0;
            Length = content.Length;
        }
        /// <summary>
        /// Gets the content of the string being read.
        /// </summary>
        public string Content { get; private set; }
        /// <summary>
        /// Gets the current position in the string being read.
        /// </summary>
        public int Position { get; private set; }
        /// <summary>
        /// Gets the length of the string being read.
        /// </summary>
        public int Length { get; private set; }
        public bool IsReadEnd => Position >= Length;
        public JsonType JsonType
        {
            get
            {
                SkipWhitespace();
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
                    default: throw new JsonReadException(Position, "字符位置[" + Position + "]处，解析错误，未知Json类型");
                }
            }
        }
        /// <summary>
        /// Reads the next character from the string and advances the position by one character.
        /// </summary>
        /// <returns>The next character in the string.</returns>
        /// <exception cref="InvalidOperationException"></exception>
#if NET45_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        #endif
        public char Read()
        {
            return Content[Position++];
        }
        /// <summary>
        /// inscrease the specified number of characters in the string and advances the position accordingly.
        /// </summary>
        /// <param name="count">The number of characters to inscrease.</param>
        /// <exception cref="InvalidOperationException"></exception>
        #if NET45_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        #endif
        private void MoveNext(int count)
        {
            if (Position + count > Length)
            {
                throw new InvalidOperationException("End of string reached.");
            }
            Position += count;
        }
        /// <summary>
        /// Gets the current character in the string without advancing the position.
        /// </summary>
        public char CurrentChar => Content[Position]; 
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
            while (Position < (Length - 1) && (curChar == ' ' || curChar == '\r' || curChar == '\n' || curChar == '\t'))
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
            if (CurrentChar != '"')
            {
                int index = Position;
                throw new JsonReadException(index, "字符位置[" + index + "]处，Json字符串解析属性名错误");
            }
            Position++;
            int startPosition = Position;
            uint hash = 5381;
            while (true)
            {
                var curChar = Content[Position];
                if (Position >= Length)
                {
                    throw new JsonReadException(Position, "字符位置[" + Position + "]处，Json字符串解析错误，字符串未闭合");
                }
                if (curChar < 0x20)
                {
                    int curIndex = Position;
                    throw new JsonReadException(curIndex, "字符位置[" + curIndex + "]处，Json字符串解析错误，字符串中存在未转义控制字符");
                }
                if (curChar == '"')
                {
                    int length = (int)(Position - startPosition);
                    if (length < 0)
                    {
                        throw new JsonReadException(startPosition, "字符位置[" + startPosition + "]处，Json字符串解析错误，属性名不能为空");
                    }
                    Position++;
                    SkipWhitespace();
                    if(Content[Position] != ':')
                    {
                        throw new JsonReadException(Position, "字符位置[" + Position + "]处，期望出现':'但是出现了意外字符'"+Content[Position]+"'");
                    }
                    MoveNext(1);
                    return new JsonPropertyName(new StringView(Content, startPosition, length), (int)hash);
                }
                hash = (hash << 5) + hash + curChar;
                Position++;
            }
        }
        #if NET45_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        #endif
        public IEnumerable<KeyValuePair<JsonPropertyName, JsonDirectReader>> ReadContent()
        {
            while(true)
            {
                SkipWhitespace();
                switch(Content[Position])
                {
                    case '{' : MoveNext(1); continue;
                    case ',' : MoveNext(1); break;
                    case '}' : MoveNext(1); SkipWhitespace(); yield break;
                }
                SkipWhitespace();
                var propertyName = ReadJsonPropertyName();
                SkipWhitespace();
                yield return new KeyValuePair<JsonPropertyName, JsonDirectReader>(propertyName, this);
            }
        }
        #if NET45_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        #endif
        public IEnumerable<JsonDirectReader> ReadArray()
        {
            while(true)
            {
                SkipWhitespace();
                switch(Content[Position])
                {
                    case '[' : MoveNext(1); continue;
                    case ',' : MoveNext(1); break;
                    case ']' : MoveNext(1); SkipWhitespace(); yield break;
                }
                SkipWhitespace();
                yield return this;
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
            if (CurrentChar != '"')
            {
                throw new JsonReadException(Position, "字符位置[" + Position + "]处，Json字符串解析string错误");
            }
            MoveNext(1);
            var startPosition = Position;
            StringBuilder stringBuilder = null;
            while (true)
            {
                if (Position >= Length)
                {
                    int curIndex = Position;
                    throw new JsonReadException(curIndex, "字符位置[" + curIndex + "]处，Json字符串解析错误，字符串未闭合");
                }
                char current = Content[Position];
                if (current == '"')
                {
                    if(stringBuilder == null)
                    {
                        var str = Content.Substring(startPosition, (int)(Position - startPosition));
                        MoveNext(1);
                        return str;
                    }
                    stringBuilder.Append(Content, startPosition, (int)(Position - startPosition));
                    MoveNext(1);
                    return stringBuilder.ToString();
                }

                if (current < 0x20)
                {
                    throw new JsonReadException(Position, "字符位置[" + Position + "]处，Json字符串解析错误，字符串中存在未转义控制字符");
                }
                if (current == '\\')
                {
                    if(stringBuilder == null)
                    {
                        stringBuilder = new StringBuilder();
                    }
                    stringBuilder.Append(Content, startPosition, (int)(Position - startPosition));
                    MoveNext(1);
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
                                        if(JsonClass.Internal.JsonString.TryHexToInt(Content[Position], out int hexValue))
                                        {
                                            value = (value << 4) + hexValue;
                                        }
                                        else
                                        {
                                            throw new JsonReadException(Position, "字符位置[" + Position + "]处，Json字符串解析错误，Unicode转义字符格式错误");
                                        }
                                    }
                                    stringBuilder.Append((char)value);
                                }; break;
                        default:
                            throw new JsonReadException(Position, "字符位置[" + Position + "]处，Json字符串解析错误，'\\" + current + "'转义失败");
                    }
                    MoveNext(1);
                    startPosition = Position;
                    continue;
                }
                MoveNext(1);
            }
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
                MoveNext(1);
                if (Position >= Length || !JsonReader.IsDigit(Content[Position]))
                {
                    throw new JsonReadException(Position, "字符位置[" + Position + "]处，Json字符串解析错误，负号后缺少数字，解析number出错");
                }
            }
            if (Content[Position] == '0')
            {
                MoveNext(1);
                if (Position < Length && JsonReader.IsDigit(Content[Position]))
                {
                    throw new JsonReadException(Position, "字符位置[" + Position + "]处，Json字符串解析错误，number不能包含前导零");
                }
            }
            else if (JsonReader.IsOneToNine(Content[Position]))
            {
                while (Position < Length && JsonReader.IsDigit(Content[Position]))
                {
                    MoveNext(1);
                }
            }
            else
            {
                throw new JsonReadException(Position, "字符位置[" + Position + "]处，Json字符串解析错误，解析number出错");
            }
            if (Position < Length && Content[Position] == '.')
            {
                MoveNext(1);
                if (Position >= Length || !JsonReader.IsDigit(Content[Position]))
                {
                    throw new JsonReadException(Position, "字符位置[" + Position + "]处，Json字符串解析错误，小数点后缺少数字，解析number出错");
                }
                while (Position < Length && JsonReader.IsDigit(Content[Position]))
                {
                    MoveNext(1);
                }
            }

            if (Position < Length && (Content[Position] == 'e' || Content[Position] == 'E'))
            {
                MoveNext(1);
                if (Position < Length && (Content[Position] == '+' || Content[Position] == '-'))
                {
                    MoveNext(1);
                }
                if (Position >= Length || !JsonReader.IsDigit(Content[Position]))
                {
                    throw new JsonReadException(Position, "字符位置[" + Position + "]处，Json字符串解析错误，指数后缺少数字，解析number出错");
                }
                while (Position < Length && JsonReader.IsDigit(Content[Position]))
                {
                    MoveNext(1);
                }
            }
            return new  JsonClass.Internal.StringView(Content, startPosition, (int)(Position - startPosition));
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
                MoveNext(4);
                if (Content[startPosition + 1] == 'r' && Content[startPosition + 2] == 'u' && Content[startPosition + 3] == 'e')
                {
                    return true;
                }
            }
            else
            {
                MoveNext(5);
                if (Content[startPosition + 1] == 'a' && Content[startPosition + 2] == 'l' && Content[startPosition + 3] == 's' && Content[startPosition + 4] == 'e')
                {
                    return false;
                }
            }
            throw new JsonReadException(startPosition, "字符位置[" + startPosition + "]处，Json字符串解析boolean错误");
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
            MoveNext(4);
            if (Content[Position - 3] == 'u' && Content[Position - 2] == 'l' && Content[Position - 1] == 'l')
            {
                return null;
            }
            throw new JsonReadException(Position - 4, "字符位置[" + (Position - 4) + "]处，Json字符串解析null错误");
        }
        /// <summary>
        /// Skip current json object
        /// </summary>
        #if NET45_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        #endif
        internal void SkipCurrentObject()
        {
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
                        foreach (var item in ReadContent())
                        {
                            item.Value.SkipCurrentObject();
                        }
                        break;
                    }
                case 't':
                case 'f': ReadBoolean(); break;
                default: throw new JsonReadException(Position, "字符位置[" + Position + "]处，解析错误，未知Json类型");
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
            return obj;
        }
    }
}
