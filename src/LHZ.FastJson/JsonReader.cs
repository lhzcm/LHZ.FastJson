using LHZ.FastJson.Exceptions;
using LHZ.FastJson.JsonClass.Internal;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;

namespace LHZ.FastJson
{
    /// <summary>
    /// JSON string parsing class
    /// </summary>
    unsafe
    public class JsonReader : IJsonReader
    {
        private readonly string _jsonString;
        private readonly JsonConvertOptions _options;
        private char* _startPoint;
        private char* _curPoint;
        private char* _endPoint;
        private int _startPosition;
        private JsonClass.JsonObject _jsonObject;
        /// <summary>
        /// end read position
        /// </summary>
        internal int EndPosition => _jsonObject == null ? -1 : (int)(_curPoint - _startPoint);
        /// <summary>
        /// Determine if it is a JSON string
        /// </summary>
        /// <param name="jsonString">JSON string</param>
        /// <param name="exception">Exception information</param>
        /// <returns></returns>
        public static bool IsJsonString(string jsonString, out Exception exception)
        {
            return IsJsonString(jsonString, null, out exception);
        }

        /// <summary>
        /// Determine if it is a JSON string
        /// </summary>
        /// <param name="jsonString">JSON string</param>
        /// <param name="options">Options (null uses the defaults)</param>
        /// <param name="exception">Exception information</param>
        /// <returns></returns>
        public static bool IsJsonString(string jsonString, JsonConvertOptions options, out Exception exception)
        {
            var jsonReader = new JsonReader(jsonString, options);
            try
            {
                jsonReader.JsonRead();
                exception = null;
                return true;
            }
            catch(Exception ex)
            {
                exception = ex;
                return false;
            }
        }

        /// <summary>
        /// Whether it is a valid JSON string
        /// </summary>
        public bool IsValidJson
        {
            get
            {
                //TODO: A separate method for validating JSON can be implemented later to improve performance
                try
                {
                    JsonRead();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Initialize JsonReader
        /// </summary>
        /// <param name="jsonString">JSON string</param>
        public JsonReader(string jsonString)
            : this(jsonString, (JsonConvertOptions)null)
        {
        }
        /// <summary>
        /// Initialize JsonReader with options
        /// </summary>
        /// <param name="jsonString">JSON string</param>
        /// <param name="options">Options (null uses the defaults)</param>
        public JsonReader(string jsonString, JsonConvertOptions options)
        {
            _jsonString = jsonString;
            _options = JsonConvertOptions.Resolve(options);
        }
        /// <summary>
        /// Initialize JsonReader
        /// </summary>
        /// <param name="jsonString">JSON string</param>
        /// <param name="startPosition">start read position</param>
        public JsonReader(string jsonString, int startPosition)
            : this(jsonString, startPosition, null)
        {
        }
        /// <summary>
        /// Initialize JsonReader with options
        /// </summary>
        /// <param name="jsonString">JSON string</param>
        /// <param name="startPosition">start read position</param>
        /// <param name="options">Options (null uses the defaults)</param>
        public JsonReader(string jsonString, int startPosition, JsonConvertOptions options)
        {
            _jsonString = jsonString;
            _options = JsonConvertOptions.Resolve(options);
            if(_jsonString.Length <= startPosition)
            {
                throw new ArgumentException("startPosition must be smaller than jsonstring length!");
            }
            else if(startPosition < 0)
            {
                throw new ArgumentException("startPosition must be >= 0");
            }
            _startPosition = startPosition;
        }
        /// <summary>
        /// Parse string
        /// </summary>
        /// <returns>JSON object</returns>
        public JsonClass.JsonObject JsonRead()
        {
            if (_jsonObject != null)
            {
                return _jsonObject;
            }
            if (string.IsNullOrEmpty(_jsonString))
            {
                throw new ArgumentNullException("jsonString", "The JSON string must not be null or empty.");
            }
            fixed (char* point = _jsonString)
            {
                _startPoint = point;
                _endPoint = point + _jsonString.Length;
                _curPoint = point + _startPosition;

                var jsonObject = GetJsonObject(0);
                SkipWhitespace();
                if (_curPoint != _endPoint && _startPosition ==0)
                {
                    int index = (int)(_curPoint - _startPoint);
                    throw JsonExceptions.TrailingCharacters(index);
                }
                _jsonObject = jsonObject;
            }
            return _jsonObject;
        }
        /// <summary>
        /// Parse JSON object
        /// </summary>
        /// <returns>JSON object</returns>
        private JsonClass.JsonObject GetJsonObject(int depth)
        {
            SkipWhitespace();
            if (*_curPoint == '{')
            {
                return GetJsonContent(depth + 1);
            }
            else if (*_curPoint == '"')
            {
                return GetJsonString();
            }
            else if ((*_curPoint >= '0' && *_curPoint <= '9') || *_curPoint == '-')
            {
                return GetJsonNumber();
            }
            else if (*_curPoint == 'n')
            {
                return GetJsonNull();
            }
            else if (*_curPoint == '[')
            {
                return GetJsonArray(depth + 1);
            }
            else if (*_curPoint == 't' || *_curPoint == 'f')
            {
                return GetJsonBoolean();
            }
            else
            {
                int index = (int)(_curPoint - _startPoint);
                throw _curPoint >= _endPoint ? JsonExceptions.UnexpectedEnd(index) : JsonExceptions.ExpectedValue(index, *_curPoint);
            }

        }
        /// <summary>
        /// Skip whitespace characters
        /// </summary>
        #if NET45_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        #endif
        private void SkipWhitespace()
        {
            while (*_curPoint == ' ' || *_curPoint == '\r' || *_curPoint == '\n' || *_curPoint == '\t')
            {
                MoveNext(1);
            }
        }
        /// <summary>
        /// Move to next character
        /// </summary>
        #if NET45_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        #endif
        private void MoveNext(int step)
        {
            _curPoint += step;
            if (_curPoint > _endPoint)
            {
                throw JsonExceptions.UnexpectedEnd((int)(_endPoint - _startPoint));
            }
        }
        /// <summary>
        /// Parse JSON String object
        /// </summary>
        /// <returns>JSON object</returns>
        #if NET45_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        #endif
        private JsonString GetJsonString()
        {
            int index = (int)(_curPoint - _startPoint);
            var stringView = ReadStringLiteral(index, out int length);
            return new JsonString(stringView, length);
        }
        private JsonClass.JsonPropertyName ReadPropertyNameString()
        {
            if (*_curPoint != '"')
            {
                int index = (int)(_curPoint - _startPoint);
                throw JsonExceptions.ExpectedPropertyName(index);
            }
            _curPoint++;
            int startIndex = (int)(_curPoint - _startPoint);
            uint hash = 5381;
            while (true)
            {
                if (_curPoint >= _endPoint)
                {
                    int curIndex = (int)(_curPoint - _startPoint);
                    throw JsonExceptions.UnterminatedString(curIndex);
                }
                if (*_curPoint < 0x20)
                {
                    int curIndex = (int)(_curPoint - _startPoint);
                    throw JsonExceptions.UnescapedControlCharacter(curIndex);
                }
                if (*_curPoint == '"')
                {
                    int length = (int)(_curPoint - _startPoint - startIndex);
                    MoveNext(1);
                    return new JsonClass.JsonPropertyName(new JsonClass.Internal.StringView(_jsonString, startIndex, length), (int)hash);
                }
                hash = (hash << 5) + hash + (*_curPoint);
                _curPoint++;
            }
        }
        #if NET45_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        #endif
        private JsonClass.Internal.StringView ReadStringLiteral(int index, out int length)
        {
            length = 0;
            if (*_curPoint != '"')
            {
                throw JsonExceptions.ExpectedString(index);
            }
            MoveNext(1);
            var startIndex = _curPoint;
            while (true)
            {
                if (_curPoint >= _endPoint)
                {
                    int curIndex = (int)(_curPoint - _startPoint);
                    throw JsonExceptions.UnterminatedString(curIndex);
                }

                char current = *_curPoint;
                if (current == '"')
                {
                    var ret = new JsonClass.Internal.StringView(_jsonString, (int)(startIndex - _startPoint), (int)(_curPoint - startIndex));
                    length += ret.Length;
                    MoveNext(1);
                    return ret;
                }

                if (current < 0x20)
                {
                    int curIndex = (int)(_curPoint - _startPoint);
                    throw JsonExceptions.UnescapedControlCharacter(curIndex);
                }
                if (current == '\\')
                {
                    MoveNext(1);
                    if (_curPoint >= _endPoint)
                    {
                        int curIndex = (int)(_curPoint - _startPoint);
                        throw JsonExceptions.IncompleteEscapeSequence(curIndex);
                    }
                    switch (*_curPoint)
                    {
                        case '"':
                        case '\\':
                        case '/':
                        case 'b':
                        case 'f':
                        case 'n':
                        case 'r':
                        case 't': length--; MoveNext(1); break;
                        case 'u': MoveNext(5); length -= 5; break;
                        default:
                            int curIndex = (int)(_curPoint - _startPoint);
                            throw JsonExceptions.InvalidEscapeSequence(curIndex, *_curPoint);
                    }
                    continue;
                }
                MoveNext(1);
            }
        }
        /// <summary>
        /// Check if it's a number
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        #if NET45_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        #endif
        public static bool IsDigit(char value)
        {
            return value >= '0' && value <= '9';
        }
        /// <summary>
        /// Check if it's 1 to 9
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        #if NET45_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        #endif
        public static bool IsOneToNine(char value)
        {
            return value >= '1' && value <= '9';
        }
        /// <summary>
        /// Parse JSON Number object
        /// </summary>
        /// <returns>JSON object</returns>
        #if NET45_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        #endif
        private JsonNumber GetJsonNumber()
        {
            int index = (int)(_curPoint - _startPoint);
            char* startPorint = _curPoint;
            bool hasPoint = false;
            bool hasExponent = false;

            if (*_curPoint == '-')
            {
                MoveNext(1);
                if (_curPoint >= _endPoint || !IsDigit(*_curPoint))
                {
                    throw JsonExceptions.MissingDigitsAfterMinus(index);
                }
            }

            if (*_curPoint == '0')
            {
                MoveNext(1);
                if (_curPoint < _endPoint && IsDigit(*_curPoint))
                {
                    throw JsonExceptions.LeadingZeroInNumber(index);
                }
            }
            else if (IsOneToNine(*_curPoint))
            {
                while (_curPoint <= _endPoint && IsDigit(*_curPoint))
                {
                    MoveNext(1);
                }
            }
            else
            {
                throw JsonExceptions.InvalidNumber(index);
            }

            if (_curPoint < _endPoint && *_curPoint == '.')
            {
                hasPoint = true;
                MoveNext(1);
                if (_curPoint >= _endPoint || !IsDigit(*_curPoint))
                {
                    throw JsonExceptions.MissingDigitsAfterDecimalPoint(index);
                }
                while (_curPoint < _endPoint && IsDigit(*_curPoint))
                {
                    MoveNext(1);
                }
            }

            if (_curPoint < _endPoint && (*_curPoint == 'e' || *_curPoint == 'E'))
            {
                hasExponent = true;
                MoveNext(1);
                if (_curPoint < _endPoint && (*_curPoint == '+' || *_curPoint == '-'))
                {
                    MoveNext(1);
                }
                if (_curPoint >= _endPoint || !IsDigit(*_curPoint))
                {
                    throw JsonExceptions.MissingDigitsInExponent(index);
                }
                while (_curPoint < _endPoint && IsDigit(*_curPoint))
                {
                    MoveNext(1);
                }
            }
            return new JsonNumber(new JsonClass.Internal.StringView(_jsonString, index, (int)(_curPoint - startPorint)), hasPoint || hasExponent ? Enum.NumberType.Double : Enum.NumberType.Long, index);
        }
        /// <summary>
        /// Parse JSON Boolean object
        /// </summary>
        /// <returns>JSON object</returns>
        #if NET45_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        #endif
        private JsonBoolean GetJsonBoolean()
        {
            int index = (int)(_curPoint - _startPoint);
            char * startPorint = _curPoint;
            if (*_curPoint == 't')
            {
                MoveNext(4);
                if (*(startPorint + 1) == 'r' && *(startPorint + 2) == 'u' && *(startPorint + 3) == 'e')
                {
                    return new JsonBoolean(true, index);
                }
            }
            else
            {
                MoveNext(5);
                if (*(startPorint + 1) == 'a' && *(startPorint + 2) == 'l' && *(startPorint + 3) == 's' && *(startPorint + 4) == 'e')
                {
                    return new JsonBoolean(false, index);
                }
            }
            throw JsonExceptions.InvalidLiteral(index, "'true' or 'false'");
        }
        /// <summary>
        /// Parse JSON Null
        /// </summary>
        /// <returns>JSON object</returns>
        #if NET45_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        #endif
        private JsonNull GetJsonNull()
        {
            int index = (int)(_curPoint - _startPoint);
            char* startPorint = _curPoint;

            MoveNext(4);
            if (*(startPorint + 1) == 'u' && *(startPorint + 2) == 'l' && *(startPorint + 3) == 'l')
            {
                return new JsonNull(index);
            }
            throw JsonExceptions.InvalidLiteral(index, "'null'");
        }
        /// <summary>
        /// Parse JSON Content object
        /// </summary>
        /// <returns>JSON object</returns>
        #if NET45_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        #endif
        private JsonContent GetJsonContent(int depth)
        {
            int index = (int)(_curPoint - _startPoint);
            if (depth > _options.MaxDepth)
            {
                throw JsonExceptions.MaxDepthExceeded(index, _options.MaxDepth);
            }
            JsonContent content = new JsonContent(index);
            MoveNext(1);
            SkipWhitespace();
            if (*_curPoint == '}')
            {
                MoveNext(1);
                return content;
            }
            while (true)
            {
                int nameIndex = (int)(_curPoint - _startPoint);
                JsonClass.JsonPropertyName propertyName = ReadPropertyNameString();
                SkipWhitespace();
                if (*_curPoint != ':')
                {
                    index = (int)(_curPoint - _startPoint);
                    throw _curPoint >= _endPoint ? JsonExceptions.ExpectedColonButEnd(index) : JsonExceptions.ExpectedColon(index, *_curPoint);
                }
                MoveNext(1);
                SkipWhitespace();

                JsonClass.JsonObject value = GetJsonObject(depth);
                try
                {
                    content.AddJsonProperty(propertyName, value);
                }
                catch (ArgumentException e)
                {
                    throw JsonExceptions.DuplicateProperty(nameIndex, propertyName.ToString(), e);
                }

                SkipWhitespace();
                if (*_curPoint == ',')
                {
                    MoveNext(1);
                    SkipWhitespace();
                    continue;
                }
                else if (*_curPoint == '}')
                {
                    break;
                }
                else
                {
                    index = (int)(_curPoint - _startPoint);
                    throw _curPoint >= _endPoint ? JsonExceptions.UnexpectedEnd(index) : JsonExceptions.ExpectedCommaOrEndOfObject(index, *_curPoint);
                }
            }
            MoveNext(1);
            return content;
        }
        /// <summary>
        /// Parse JSON Array object
        /// </summary>
        /// <returns>JSON object</returns>
        #if NET45_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        #endif
        private JsonArray GetJsonArray(int depth)
        {
            int index = (int)(_curPoint - _startPoint);
            if (depth > _options.MaxDepth)
            {
                throw JsonExceptions.MaxDepthExceeded(index, _options.MaxDepth);
            }
            JsonArray jsonArray = new JsonArray(index);

            MoveNext(1);
            SkipWhitespace();

            while (*_curPoint != ']')
            {
                jsonArray.AddJsonObject(GetJsonObject(depth));
                SkipWhitespace();
                if (*_curPoint == ',')
                {
                    MoveNext(1);
                    SkipWhitespace();
                    if (*_curPoint == ']')
                    {
                        index = (int)(_curPoint - _startPoint);
                        throw JsonExceptions.TrailingCommaInArray(index);
                    }
                    continue;
                }
                else if (*_curPoint == ']')
                {
                    break;
                }
                else
                {
                    index = (int)(_curPoint - _startPoint);
                    throw _curPoint >= _endPoint ? JsonExceptions.UnexpectedEnd(index) : JsonExceptions.ExpectedCommaOrEndOfArray(index, *_curPoint);
                }
            }
            MoveNext(1);
            return jsonArray;
        }
    }
}
