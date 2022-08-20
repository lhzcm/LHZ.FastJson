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
            if (string.IsNullOrEmpty(_jsonString))
            {
                throw new Exception("Json解析错误，字符串为空");
            }
            fixed (char* point = _jsonString)
            {
                _startPoint = point;
                _endPoint = point + _jsonString.Length;
                _curPoint = point;

                return GetJsonObject();
            }
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
            else if (*_curPoint >= '0' && *_curPoint <= '9')
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
            if (*_curPoint != '"')
            {
                int index = (int)(_curPoint - _startPoint);
                throw new JsonReadException(index, "字符位置[" + index + "]处，Json字符串解析属性名错误");
            }

            MoveNext();
            char* startPoint = _curPoint;
            int startIndex = (int)(_curPoint - _startPoint);
            while (*_curPoint != '"')
            {
                if (*_curPoint == '\\')
                {
                    int index = (int)(_curPoint - _startPoint);
                    throw new JsonReadException(index, "字符位置[" + index + "]处，Json属性名解析错误，属性名不能有转义字符");
                }
                MoveNext();
            }
            if (startPoint == _curPoint)
            {
                int index = (int)(startPoint - _startPoint);
                throw new JsonReadException(index, "属性名称不能为空！");
            }
            string name = new String(_startPoint, (int)(startPoint - _startPoint), (int)(_curPoint - startPoint));
            MoveNext();
            return name;
        }
#if NET40 || NET45 || NET46
        /// <summary>
        /// 解析Json String对象
        /// </summary>
        /// <returns>Json对象</returns>
        private unsafe JsonObject GetJsonString()
        {
            int index = (int)(_curPoint - _startPoint);
            int count = 0;
            if (*_curPoint != '"')
            {
                throw new JsonReadException(index, "字符位置[" + index + "]处，Json字符串解析string错误");
            }
            StringBuilder stringBuilder = new StringBuilder();
            MoveNext();

            int startIndex = (int)(_curPoint - _startPoint);
            while (*_curPoint != '"')
            {
                if (*_curPoint == '\\')
                {
                    count = (int)(_curPoint - _startPoint) - startIndex;
                    if (count > 0)
                    {
                        stringBuilder.Append(_jsonString, startIndex, count);
                    }

                    MoveNext();
                    switch (*_curPoint)
                    {
                        case '\'': stringBuilder.Append('\''); break;
                        case '"': stringBuilder.Append('\"'); break;
                        case '\\': stringBuilder.Append('\\'); break;
                        case '0': stringBuilder.Append('\0'); break;
                        case 'a': stringBuilder.Append('\a'); break;
                        case 'b': stringBuilder.Append('\b'); break;
                        case 'f': stringBuilder.Append('\f'); break;
                        case 'n': stringBuilder.Append('\n'); break;
                        case 'r': stringBuilder.Append('\r'); break;
                        case 't': stringBuilder.Append('\t'); break;
                        case 'v': stringBuilder.Append('\v'); break;
                        default:
                            index = (int)(_curPoint - _startPoint);
                            throw new JsonReadException(index, "字符位置[" + index + "]处，Json字符串解析错误，'\\" + *_curPoint + "'转义失败");
                    }
                    MoveNext();
                    startIndex = (int)(_curPoint - _startPoint);
                    continue;
                }
                MoveNext();
            }
            count = (int)(_curPoint - _startPoint) - startIndex;
            if (count > 0)
            {
                stringBuilder.Append(_jsonString, startIndex, count);
            }
            MoveNext();

            return new JsonString(stringBuilder.ToString(), index);
        }
#else
        /// <summary>
        /// 解析Json String对象
        /// </summary>
        /// <returns>Json对象</returns>
        private unsafe JsonObject GetJsonString()
        {
            int index = (int)(_curPoint - _startPoint);
            int count = 0;
            if (*_curPoint != '"')
            {
                throw new JsonReadException(index, "字符位置[" + index + "]处，Json字符串解析string错误");
            }
            StringBuilder stringBuilder = new StringBuilder();
            MoveNext();
            
            char* startPoint = _curPoint;
            while (*_curPoint != '"')
            {
                if (*_curPoint == '\\')
                {
                    count = (int)(_curPoint - startPoint);
                    if (count > 0)
                    {
                        stringBuilder.Append(startPoint, count);
                    }

                    MoveNext();
                    switch (*_curPoint)
                    {
                        case '\'': stringBuilder.Append('\''); break; 
                        case '"': stringBuilder.Append('\"'); break;
                        case '\\': stringBuilder.Append('\\'); break;
                        case '0': stringBuilder.Append('\0'); break;
                        case 'a': stringBuilder.Append('\a'); break;
                        case 'b': stringBuilder.Append('\b'); break;
                        case 'f': stringBuilder.Append('\f'); break;
                        case 'n': stringBuilder.Append('\n'); break;
                        case 'r': stringBuilder.Append('\r'); break;
                        case 't': stringBuilder.Append('\t'); break;
                        case 'v': stringBuilder.Append('\v'); break;
                        default:
                            index = (int)(_curPoint - _startPoint);
                            throw new JsonReadException(index, "字符位置[" + index + "]处，Json字符串解析错误，'\\" + *_curPoint + "'转义失败");
                    }
                    MoveNext();
                    startPoint = _curPoint;
                    continue;
                }
                MoveNext();
            }
            count = (int)(_curPoint - startPoint);
            if (count > 0)
            {
                stringBuilder.Append(startPoint, count);
            }
            MoveNext();

            return new JsonString(stringBuilder.ToString(), index);
        }
#endif
        /// <summary>
        /// 解析Json Number对象
        /// </summary>
        /// <returns>Json对象</returns>
        private JsonObject GetJsonNumber()
        {
            int index = (int)(_curPoint - _startPoint);
            char* startPorint = _curPoint;
            bool hasPoint = false;

            while ((*_curPoint >= '0' && *_curPoint <= '9') || *_curPoint == '.')
            {
                if (*_curPoint == '.')
                {
                    if (hasPoint)
                    {
                        throw new JsonReadException(index, "字符位置[" + index + "]处，Json字符串解析错误，出现多个小数点，解析number出错");
                    }
                    hasPoint = true;
                }
                MoveNext();
            }
            string numberStr = new string(startPorint, 0, (int)(_curPoint - startPorint));
            return new JsonNumber(hasPoint ? Enum.NumberType.Double: Enum.NumberType.Long, numberStr, index);
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
