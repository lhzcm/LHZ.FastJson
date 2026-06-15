using LHZ.FastJson.Exceptions;
using LHZ.FastJson.JsonClass;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security;
using System.Text;

namespace LHZ.FastJson
{
    /// <summary>
    /// Json字符串解析类
    /// </summary>
    unsafe
    public class JsonReader : IJsonReader
    {
        private readonly string _jsonString;
        private char* _startPoint;
        private char* _curPoint;
        private char* _endPoint;

        private JsonObject _jsonObject;

        /// <summary>
        /// 判断是否是Json字符串
        /// </summary>
        /// <param name="jsonString">Json字符串</param>
        /// <param name="exception">异常信息</param>
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
        /// 是否是有效的json字符串
        /// </summary>
        public bool IsValidJson
        {
            get
            {
                //TODO 以后可以单独实现一个判断json是否有效的方法,提高性能
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

        public JsonReader(string jsonString)
        {
            _jsonString = jsonString;
        }
        /// <summary>
        /// 解析字符串
        /// </summary>
        /// <returns>Json对象</returns>
        public JsonObject JsonRead()
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
        /// 解析Json对象
        /// </summary>
        /// <returns>Json对象</returns>
        private JsonObject GetJsonObject()
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
        /// 跳过空白字符
        /// </summary>
        private void SkipWhitespace()
        {
            while (*_curPoint == ' ' || *_curPoint == '\r' || *_curPoint == '\n' || *_curPoint == '\t')
            {
                MoveNext();
            }
        }
        /// <summary>
        /// 移动到下一个字符
        /// </summary>
        private void MoveNext()
        {
            _curPoint++;
            if (_curPoint > _endPoint)
            {
                int index = (int)(_curPoint - _startPoint);
                throw new JsonReadException(index, "索引溢出，字符串已经读取完，但json却未完全解析");
            }
        }
        /// <summary>
        /// 解析Json属性名称
        /// </summary>
        /// <returns>属性名称</returns>
        private string GetAttrName()
        {
            int startIndex = (int)(_curPoint - _startPoint);
            string name = ReadStringLiteral(startIndex, "属性名");
            if (name.Length == 0)
            {
                throw new JsonReadException(startIndex, "属性名称不能为空！");
            }
            return name;
        }
        /// <summary>
        /// 解析Json String对象
        /// </summary>
        /// <returns>Json对象</returns>
        private unsafe JsonObject GetJsonString()
        {
            int index = (int)(_curPoint - _startPoint);
            return new JsonString(ReadStringLiteral(index, "string"), index);
        }

        private string ReadStringLiteral(int index, string targetName)
        {
            if (*_curPoint != '"')
            {
                throw new JsonReadException(index, "字符位置[" + index + "]处，Json字符串解析" + targetName + "错误");
            }

            StringBuilder stringBuilder = new StringBuilder();
            MoveNext();

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
                    MoveNext();
                    return stringBuilder.ToString();
                }

                if (current < 0x20)
                {
                    int curIndex = (int)(_curPoint - _startPoint);
                    throw new JsonReadException(curIndex, "字符位置[" + curIndex + "]处，Json字符串解析错误，字符串中存在未转义控制字符");
                }

                if (current == '\\')
                {
                    MoveNext();
                    if (_curPoint >= _endPoint)
                    {
                        int curIndex = (int)(_curPoint - _startPoint);
                        throw new JsonReadException(curIndex, "字符位置[" + curIndex + "]处，Json字符串解析错误，转义字符未完成");
                    }

                    switch (*_curPoint)
                    {
                        case '"': stringBuilder.Append('\"'); break;
                        case '\\': stringBuilder.Append('\\'); break;
                        case '/': stringBuilder.Append('/'); break;
                        case 'b': stringBuilder.Append('\b'); break;
                        case 'f': stringBuilder.Append('\f'); break;
                        case 'n': stringBuilder.Append('\n'); break;
                        case 'r': stringBuilder.Append('\r'); break;
                        case 't': stringBuilder.Append('\t'); break;
                        case 'u': stringBuilder.Append(ReadUnicodeEscape()); break;
                        default:
                            int curIndex = (int)(_curPoint - _startPoint);
                            throw new JsonReadException(curIndex, "字符位置[" + curIndex + "]处，Json字符串解析错误，'\\" + *_curPoint + "'转义失败");
                    }
                    MoveNext();
                    continue;
                }

                stringBuilder.Append(current);
                MoveNext();
            }
        }

        private char ReadUnicodeEscape()
        {
            int value = 0;
            for (int i = 0; i < 4; i++)
            {
                MoveNext();
                if (_curPoint >= _endPoint || !IsHexDigit(*_curPoint))
                {
                    int index = (int)(_curPoint - _startPoint);
                    throw new JsonReadException(index, "字符位置[" + index + "]处，Json字符串解析错误，Unicode转义字符格式错误");
                }
                value = value * 16 + HexToInt(*_curPoint);
            }
            return (char)value;
        }

        private static bool IsDigit(char value)
        {
            return value >= '0' && value <= '9';
        }

        private static bool IsOneToNine(char value)
        {
            return value >= '1' && value <= '9';
        }

        private static bool IsHexDigit(char value)
        {
            return (value >= '0' && value <= '9') ||
                   (value >= 'a' && value <= 'f') ||
                   (value >= 'A' && value <= 'F');
        }

        private static int HexToInt(char value)
        {
            if (value >= '0' && value <= '9')
            {
                return value - '0';
            }
            if (value >= 'a' && value <= 'f')
            {
                return value - 'a' + 10;
            }
            return value - 'A' + 10;
        }

        /// <summary>
        /// 解析Json Number对象
        /// </summary>
        /// <returns>Json对象</returns>
        private JsonObject GetJsonNumber()
        {
            int index = (int)(_curPoint - _startPoint);
            char* startPorint = _curPoint;
            bool hasPoint = false;
            bool hasExponent = false;

            if (*_curPoint == '-')
            {
                MoveNext();
                if (_curPoint >= _endPoint || !IsDigit(*_curPoint))
                {
                    throw new JsonReadException(index, "字符位置[" + index + "]处，Json字符串解析错误，负号后缺少数字，解析number出错");
                }
            }

            if (*_curPoint == '0')
            {
                MoveNext();
                if (_curPoint < _endPoint && IsDigit(*_curPoint))
                {
                    throw new JsonReadException(index, "字符位置[" + index + "]处，Json字符串解析错误，number不能包含前导零");
                }
            }
            else if (IsOneToNine(*_curPoint))
            {
                while (_curPoint < _endPoint && IsDigit(*_curPoint))
                {
                    MoveNext();
                }
            }
            else
            {
                throw new JsonReadException(index, "字符位置[" + index + "]处，Json字符串解析错误，解析number出错");
            }

            if (_curPoint < _endPoint && *_curPoint == '.')
            {
                hasPoint = true;
                MoveNext();
                if (_curPoint >= _endPoint || !IsDigit(*_curPoint))
                {
                    throw new JsonReadException(index, "字符位置[" + index + "]处，Json字符串解析错误，小数点后缺少数字，解析number出错");
                }
                while (_curPoint < _endPoint && IsDigit(*_curPoint))
                {
                    MoveNext();
                }
            }

            if (_curPoint < _endPoint && (*_curPoint == 'e' || *_curPoint == 'E'))
            {
                hasExponent = true;
                MoveNext();
                if (_curPoint < _endPoint && (*_curPoint == '+' || *_curPoint == '-'))
                {
                    MoveNext();
                }
                if (_curPoint >= _endPoint || !IsDigit(*_curPoint))
                {
                    throw new JsonReadException(index, "字符位置[" + index + "]处，Json字符串解析错误，指数后缺少数字，解析number出错");
                }
                while (_curPoint < _endPoint && IsDigit(*_curPoint))
                {
                    MoveNext();
                }
            }
            string numberStr = new string(startPorint, 0, (int)(_curPoint - startPorint));
            return new JsonNumber(hasPoint || hasExponent ? Enum.NumberType.Double: Enum.NumberType.Long, numberStr, index);
        }
        /// <summary>
        /// 解析Json Boolean对象
        /// </summary>
        /// <returns>Json对象</returns>
        private JsonObject GetJsonBoolean()
        {
            int index = (int)(_curPoint - _startPoint);
            char * startPorint = _curPoint;
            
            if (*_curPoint == 't')
            {
                MoveNext();
                MoveNext();
                MoveNext();
                MoveNext();
            }
            else
            {
                MoveNext();
                MoveNext();
                MoveNext();
                MoveNext();
                MoveNext();
            }
            string boolStr = new string(startPorint, 0, (int)(_curPoint - startPorint));
            if (boolStr != "true" && boolStr != "false")
            {
                throw new JsonReadException(index, "字符位置[" + index + "]处，Json字符串解析boolean错误");
            }
            return new JsonBoolean(boolStr == "true" ? Enum.BooleanType.True : Enum.BooleanType.False, boolStr, index);
        }
        /// <summary>
        /// 解析Json Null
        /// </summary>
        /// <returns>Json对象</returns>
        private JsonObject GetJsonNull()
        {
            int index = (int)(_curPoint - _startPoint);
            char* startPorint = _curPoint;

            MoveNext();
            MoveNext();
            MoveNext();
            MoveNext();

            string nullStr = new string(startPorint, 0, (int)(_curPoint - startPorint));

            if (nullStr != "null")
            {
                throw new JsonReadException(index, "字符位置[" + index + "]处，Json字符串解析null错误");
            }
            return new JsonNull(nullStr, index);
        }
        /// <summary>
        /// 解析Json Content对象
        /// </summary>
        /// <returns>Json对象</returns>
        private JsonObject GetJsonContent()
        {
            int index = (int)(_curPoint - _startPoint);
            JsonContent content = new JsonContent(index);
            MoveNext();
            SkipWhitespace();
            if (*_curPoint == '}')
            {
                MoveNext();
                return content;
            }
            while (true)
            {
                string attrName = GetAttrName();
                SkipWhitespace();
                if (*_curPoint != ':')
                {
                    index = (int)(_curPoint - _startPoint);
                    throw new JsonReadException(index, "字符位置[" + index + "]处出现意外字符‘"+*_curPoint+"’，期望字符‘:’，Json字符串解析content出错");
                }
                MoveNext();
                SkipWhitespace();

                JsonObject value = GetJsonObject();
                content.AddJsonAttr(attrName, value);

                SkipWhitespace();
                if (*_curPoint == ',')
                {
                    MoveNext();
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
            MoveNext();
            return content;
        }
        /// <summary>
        /// 解析Json Array对象
        /// </summary>
        /// <returns>Json对象</returns>
        private JsonObject GetJsonArray()
        {
            int index = (int)(_curPoint - _startPoint);
            JsonArray jsonArray = new JsonArray(index);

            MoveNext();
            SkipWhitespace();

            while (*_curPoint != ']')
            {
                jsonArray.AddJsonObject(GetJsonObject());
                SkipWhitespace();
                if (*_curPoint == ',')
                {
                    MoveNext();
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
                    throw new JsonReadException(index, "字符位置[" + index + "]处，Json字符串解析Array错");
                }
            }
            MoveNext();
            return jsonArray;
        }
    }
}
