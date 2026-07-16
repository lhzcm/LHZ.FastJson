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
        private char* _startPoint;
        private char* _curPoint;
        private char* _endPoint;

        private JsonClass.JsonObject _jsonObject;

        /// <summary>
        /// Determine if it is a JSON string
        /// </summary>
        /// <param name="jsonString">JSON string</param>
        /// <param name="exception">Exception information</param>
        /// <returns></returns>
        public static bool IsJsonString(string jsonString, out Exception exception)
        {
            var jsonReader = new JsonReader(jsonString);
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
        {
            _jsonString = jsonString;
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
                throw new Exception("Json解析错误，字符串为空");
            }
            fixed (char* point = _jsonString)
            {
                _startPoint = point;
                _endPoint = point + _jsonString.Length;
                _curPoint = point;

                var jsonObject = GetJsonObject();
                SkipWhitespace();
                if (_curPoint != _endPoint)
                {
                    int index = (int)(_curPoint - _startPoint);
                    throw new JsonReadException(index, "字符位置[" + index + "]处，Json字符串已解析完成但仍存在多余字符");
                }
                _jsonObject = jsonObject;
            }
            return _jsonObject;
        }
        /// <summary>
        /// Parse JSON object
        /// </summary>
        /// <returns>JSON object</returns>
        private JsonClass.JsonObject GetJsonObject()
        {
            SkipWhitespace();
            if (*_curPoint == '{')
            {
                return GetJsonContent();
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
                return GetJsonArray();
            }
            else if (*_curPoint == 't' || *_curPoint == 'f')
            {
                return GetJsonBoolean();
            }
            else
            {
                int index = (int)(_curPoint - _startPoint);
                throw new JsonReadException(index, "字符位置[" + index + "]处，解析错误，未知Json类型");
            }

        }
        /// <summary>
        /// Skip whitespace characters
        /// </summary>
        #if NET45_OR_GREATER
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
        #if NET45_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        #endif
        private void MoveNext(int step)
        {
            _curPoint += step;
            if (_curPoint > _endPoint)
            {
                int index = (int)(_curPoint - _startPoint);
                throw new JsonReadException(index, "索引溢出，字符串已经读取完，但json却未完全解析");
            }
        }
        /// <summary>
        /// Parse JSON String object
        /// </summary>
        /// <returns>JSON object</returns>
        #if NET45_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        #endif
        private unsafe JsonClass.JsonObject GetJsonString()
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
                throw new JsonReadException(index, "字符位置[" + index + "]处，Json字符串解析属性名错误");
            }
            _curPoint++;
            int startIndex = (int)(_curPoint - _startPoint);
            uint hash = 5381;
            while (true)
            {
                if (_curPoint >= _endPoint)
                {
                    int curIndex = (int)(_curPoint - _startPoint);
                    throw new JsonReadException(curIndex, "字符位置[" + curIndex + "]处，Json字符串解析错误，字符串未闭合");
                }
                if (*_curPoint < 0x20)
                {
                    int curIndex = (int)(_curPoint - _startPoint);
                    throw new JsonReadException(curIndex, "字符位置[" + curIndex + "]处，Json字符串解析错误，字符串中存在未转义控制字符");
                }
                if (*_curPoint == '"')
                {
                    int endIndex = (int)(_curPoint - _startPoint - 1);
                    if(endIndex < startIndex)
                    {
                        int curIndex = (int)(_curPoint - _startPoint);
                        throw new JsonReadException(curIndex, "字符位置[" + curIndex + "]处，Json字符串解析错误，属性名不能为空");
                    }
                    MoveNext(1);
                    return new JsonClass.JsonPropertyName(new JsonClass.Internal.StringView(_jsonString, startIndex, endIndex), (int)hash);
                }
                hash = (hash << 5) + hash + (*_curPoint);
                _curPoint++;
            }
        }
        #if NET45_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        #endif
        private JsonClass.Internal.StringView ReadStringLiteral(int index, out int length)
        {
            length = 0;
            if (*_curPoint != '"')
            {
                throw new JsonReadException(index, "字符位置[" + index + "]处，Json字符串解析string错误");
            }
            MoveNext(1);
            var startIndex = _curPoint;
            while (true)
            {
                if (_curPoint >= _endPoint)
                {
                    int curIndex = (int)(_curPoint - _startPoint);
                    throw new JsonReadException(curIndex, "字符位置[" + curIndex + "]处，Json字符串解析错误，字符串未闭合");
                }

                char current = *_curPoint;
                if (current == '"')
                {
                    var ret = new JsonClass.Internal.StringView(_jsonString, (int)(startIndex - _startPoint), (int)(_curPoint - _startPoint - 1));
                    length += ret.Length;
                    MoveNext(1);
                    return ret;
                }

                if (current < 0x20)
                {
                    int curIndex = (int)(_curPoint - _startPoint);
                    throw new JsonReadException(curIndex, "字符位置[" + curIndex + "]处，Json字符串解析错误，字符串中存在未转义控制字符");
                }
                if (current == '\\')
                {
                    MoveNext(1);
                    if (_curPoint >= _endPoint)
                    {
                        int curIndex = (int)(_curPoint - _startPoint);
                        throw new JsonReadException(curIndex, "字符位置[" + curIndex + "]处，Json字符串解析错误，转义字符未完成");
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
                            throw new JsonReadException(curIndex, "字符位置[" + curIndex + "]处，Json字符串解析错误，'\\" + *_curPoint + "'转义失败");
                    }
                    continue;
                }
                MoveNext(1);
            }
        }
        private static bool IsDigit(char value)
        {
            return value >= '0' && value <= '9';
        }

        private static bool IsOneToNine(char value)
        {
            return value >= '1' && value <= '9';
        }


        /// <summary>
        /// Parse JSON Number object
        /// </summary>
        /// <returns>JSON object</returns>
        #if NET45_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        #endif
        private JsonClass.JsonObject GetJsonNumber()
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
                    throw new JsonReadException(index, "字符位置[" + index + "]处，Json字符串解析错误，负号后缺少数字，解析number出错");
                }
            }

            if (*_curPoint == '0')
            {
                MoveNext(1);
                if (_curPoint < _endPoint && IsDigit(*_curPoint))
                {
                    throw new JsonReadException(index, "字符位置[" + index + "]处，Json字符串解析错误，number不能包含前导零");
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
                throw new JsonReadException(index, "字符位置[" + index + "]处，Json字符串解析错误，解析number出错");
            }

            if (_curPoint < _endPoint && *_curPoint == '.')
            {
                hasPoint = true;
                MoveNext(1);
                if (_curPoint >= _endPoint || !IsDigit(*_curPoint))
                {
                    throw new JsonReadException(index, "字符位置[" + index + "]处，Json字符串解析错误，小数点后缺少数字，解析number出错");
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
                    throw new JsonReadException(index, "字符位置[" + index + "]处，Json字符串解析错误，指数后缺少数字，解析number出错");
                }
                while (_curPoint < _endPoint && IsDigit(*_curPoint))
                {
                    MoveNext(1);
                }
            }
            return new JsonNumber(new JsonClass.Internal.StringView(_jsonString, index, (int)(_curPoint - _startPoint - 1)), hasPoint || hasExponent ? Enum.NumberType.Double : Enum.NumberType.Long, index);
        }
        /// <summary>
        /// Parse JSON Boolean object
        /// </summary>
        /// <returns>JSON object</returns>
        #if NET45_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        #endif
        private JsonClass.JsonObject GetJsonBoolean()
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
            throw new JsonReadException(index, "字符位置[" + index + "]处，Json字符串解析boolean错误");
        }
        /// <summary>
        /// Parse JSON Null
        /// </summary>
        /// <returns>JSON object</returns>
        #if NET45_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        #endif
        private JsonClass.JsonObject GetJsonNull()
        {
            int index = (int)(_curPoint - _startPoint);
            char* startPorint = _curPoint;

            MoveNext(4);
            if (*(startPorint + 1) == 'u' && *(startPorint + 2) == 'l' && *(startPorint + 3) == 'l')
            {
                return new JsonNull(index);
            }
            throw new JsonReadException(index, "字符位置[" + index + "]处，Json字符串解析null错误");
        }
        /// <summary>
        /// Parse JSON Content object
        /// </summary>
        /// <returns>JSON object</returns>
        #if NET45_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        #endif
        private JsonClass.JsonObject GetJsonContent()
        {
            int index = (int)(_curPoint - _startPoint);
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
                JsonClass.JsonPropertyName propertyName = ReadPropertyNameString();
                SkipWhitespace();
                if (*_curPoint != ':')
                {
                    index = (int)(_curPoint - _startPoint);
                    throw new JsonReadException(index, "字符位置[" + index + "]处出现意外字符‘"+*_curPoint+"’，期望字符‘:’，Json字符串解析content出错");
                }
                MoveNext(1);
                SkipWhitespace();

                JsonClass.JsonObject value = GetJsonObject();
                content.AddJsonProperty(propertyName, value);

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
                    throw new JsonReadException(index, "字符位置[" + index + "]处出现意外字符‘" + *_curPoint + "’，期望字符‘:’或‘}’，Json字符串解析content出错");
                }
            }
            MoveNext(1);
            return content;
        }
        /// <summary>
        /// Parse JSON Array object
        /// </summary>
        /// <returns>JSON object</returns>
        #if NET45_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        #endif
        private JsonClass.JsonObject GetJsonArray()
        {
            int index = (int)(_curPoint - _startPoint);
            JsonArray jsonArray = new JsonArray(index);

            MoveNext(1);
            SkipWhitespace();

            while (*_curPoint != ']')
            {
                jsonArray.AddJsonObject(GetJsonObject());
                SkipWhitespace();
                if (*_curPoint == ',')
                {
                    MoveNext(1);
                    SkipWhitespace();
                    if (*_curPoint == ']')
                    {
                        index = (int)(_curPoint - _startPoint);
                        throw new JsonReadException(index, "字符位置[" + index + "]处，Json字符串解析Array错误，数组不能以逗号结尾");
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
                    throw new JsonReadException(index, "字符位置[" + index + "]处，Json字符串解析Array出错");
                }
            }
            MoveNext(1);
            return jsonArray;
        }
    }
}
