using LHZ.FastJson.Exceptions;
using LHZ.FastJson.Interface;
using LHZ.FastJson.JsonClass;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace LHZ.FastJson
{
    /// <summary>
    /// Json字符串解析类
    /// </summary>
    public class JsonReader : IJsonReader
    {
        private char[] _jsonCharArray;

        public JsonReader(string jsonStr)
        {
            StringToCharArray(jsonStr);
        }

        public JsonObject JsonRead()
        {
            int index = 0;
            return GetJsonObject(ref index);
        }

        /// <summary>
        /// 解析Json对象
        /// </summary>
        /// <param name="index">当前解析字符索引</param>
        /// <returns>Json对象</returns>
        private JsonObject GetJsonObject(ref int index)
        {
            index = SkipBlank(index);
            if (_jsonCharArray[index] == '{')
            {
                return GetJsonContent(ref index);
            }
            else if (_jsonCharArray[index] == '"')
            {
                return GetJsonString(ref index);
            }
            else if (_jsonCharArray[index] >= '0' && _jsonCharArray[index] <= '9')
            {
                return GetJsonNumber(ref index);
            }
            else if (_jsonCharArray[index] == 'n')
            {
                return GetJsonNull(ref index);
            }
            else if (_jsonCharArray[index] == '[')
            {
                return GetJsonArray(ref index);
            }
            else if (_jsonCharArray[index] == 't' || _jsonCharArray[index] == 'f')
            {
                return GetJsonBoolean(ref index);
            }
            else
            {
                throw new JsonReadException(index, "字符位置[" + index + "]处，解析错误，未知Json类型");
            }

        }

        /// <summary>
        /// 把string转化为字符数组
        /// </summary>
        /// <param name="jsonStr">json字符串</param>
        private void StringToCharArray(string jsonStr)
        {
            if (string.IsNullOrEmpty(jsonStr))
            {
                throw new Exception("Json解析错误，字符串为空");
            }
            _jsonCharArray = jsonStr.ToCharArray();
        }

        /// <summary>
        /// 跳过空白字符
        /// </summary>
        /// <param name="index">当前解析字符索引</param>
        /// <returns>跳过空白字符后的索引</returns>
        private int SkipBlank(int index)
        {
            while (_jsonCharArray[index] == ' ' || _jsonCharArray[index] == '\r' || _jsonCharArray[index] == '\n' || _jsonCharArray[index] == '\t')
            {
                index++;
            }
            return index;
        }

        /// <summary>
        /// 解析Json属性名称
        /// </summary>
        /// <param name="startIndex">当前解析字符索引</param>
        /// <returns>属性名称</returns>
        private string GetAttrName(ref int startIndex)
        {
            if (_jsonCharArray[startIndex] != '"')
            {
                throw new JsonReadException(startIndex, "字符位置[" + startIndex + "]处，Json字符串解析属性名错误");
            }
            StringBuilder stringBuilder = new StringBuilder(8);

            while (_jsonCharArray[++startIndex] != '"')
            {
                if (_jsonCharArray[startIndex] == '\\')
                {
                    switch (_jsonCharArray[++startIndex])
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
                        default: throw new Exception(startIndex + "处，Json属性名解析错误，'\\" + _jsonCharArray[startIndex] + "'转义失败");
                    }
                    continue;
                }
                stringBuilder.Append(_jsonCharArray[startIndex]);
            }
            startIndex++;
            return stringBuilder.ToString();
        }

        /// <summary>
        /// 解析Json String对象
        /// </summary>
        /// <param name="startIndex">当前解析字符索引</param>
        /// <returns>Json对象</returns>
        private JsonObject GetJsonString(ref int startIndex)
        {
            int position = startIndex;
            if (_jsonCharArray[startIndex] != '"')
            {
                throw new Exception(startIndex + "处，Json字符串解析string错误");
            }
            StringBuilder stringBuilder = new StringBuilder(8);

            while (_jsonCharArray[++startIndex] != '"')
            {
                if (_jsonCharArray[startIndex] == '\\')
                {
                    switch (_jsonCharArray[++startIndex])
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
                        default: throw new JsonReadException(startIndex, "字符位置[" + startIndex + "]处，Json字符串解析错误，'\\" + _jsonCharArray[startIndex] + "'转义失败");
                    }
                    continue;
                }
                stringBuilder.Append(_jsonCharArray[startIndex]);
            }
            startIndex++;
            return new JsonString(stringBuilder.ToString(), position);
        }

        /// <summary>
        /// 解析Json Number对象
        /// </summary>
        /// <param name="startIndex">当前解析字符索引</param>
        /// <returns>Json对象</returns>
        private JsonObject GetJsonNumber(ref int startIndex)
        {
            int position = startIndex;
            if (_jsonCharArray[startIndex] < '0' || _jsonCharArray[startIndex] > '9')
            {
                throw new JsonReadException(startIndex, "字符位置[" + startIndex + "]处，Json字符串解析number错误");
            }
            StringBuilder stringBuilder = new StringBuilder(8);
            stringBuilder.Append(_jsonCharArray[startIndex++]);
            short pointNums = 0;
            while (startIndex < _jsonCharArray.Length && ((_jsonCharArray[startIndex] == '.') || (_jsonCharArray[startIndex] >= '0' && _jsonCharArray[startIndex] <= '9')))
            {
                if (_jsonCharArray[startIndex] == '.')
                {
                    if (pointNums > 0)
                    {
                        throw new JsonReadException(startIndex, "字符位置[" + startIndex + "]处，Json字符串解析错误，'\\" + _jsonCharArray[startIndex] + "'解析number出错");
                    }
                    pointNums++;
                }
                stringBuilder.Append(_jsonCharArray[startIndex++]);
            }
            return new JsonNumber(pointNums > 0 ? Enum.NumberType.Double: Enum.NumberType.Long, stringBuilder.ToString(), position);
        }

        /// <summary>
        /// 解析Json Boolean对象
        /// </summary>
        /// <param name="startIndex">当前解析字符索引</param>
        /// <returns>Json对象</returns>
        private JsonObject GetJsonBoolean(ref int startIndex)
        {
            int position = startIndex;
            if (_jsonCharArray[startIndex] != 't' && _jsonCharArray[startIndex] != 'f')
            {
                throw new JsonReadException(startIndex, "字符位置[" + startIndex + "]处，Json字符串解析boolean错误"); 
            }
            StringBuilder stringBuilder = new StringBuilder(4);
            while (startIndex < _jsonCharArray.Length && _jsonCharArray[startIndex] >= 'a' && _jsonCharArray[startIndex] <= 'z')
            {
                stringBuilder.Append(_jsonCharArray[startIndex++]);
            }
            string result = stringBuilder.ToString();
            if (result != "true" && result != "false")
            {
                throw new JsonReadException(startIndex, "字符位置[" + startIndex + "]处，Json字符串解析boolean错误");
            }
            return new JsonBoolean(result == "true" ? Enum.BooleanType.True : Enum.BooleanType.False, result, position);
        }

        /// <summary>
        /// 解析Json Null
        /// </summary>
        /// <param name="startIndex">当前解析字符索引</param>
        /// <returns>Json对象</returns>
        private JsonObject GetJsonNull(ref int startIndex)
        {
            if (_jsonCharArray[startIndex] != 'n')
            {
                throw new JsonReadException(startIndex, "字符位置[" + startIndex + "]处，Json字符串解析null错误");
            }
            string result = new string(_jsonCharArray, startIndex, 4);
            if (result != "null")
            {
                throw new JsonReadException(startIndex, "字符位置[" + startIndex + "]处，Json字符串解析null错误");
            }
            startIndex += 4;
            return new JsonNull(result, startIndex - 4);
        }

        /// <summary>
        /// 解析Json Content对象
        /// </summary>
        /// <param name="startIndex">当前解析字符索引</param>
        /// <returns>Json对象</returns>
        private JsonObject GetJsonContent(ref int startIndex)
        {
            if (_jsonCharArray[startIndex] != '{')
            {
                throw new JsonReadException(startIndex, "字符位置[" + startIndex + "]处，Json字符串解析content错");
            }
            JsonContent content = new JsonContent(startIndex);
            startIndex = SkipBlank(++startIndex);
            if (_jsonCharArray[startIndex] == '}')
            {
                startIndex++;
                return content;
            }
            while (true)
            {
                string attrName = GetAttrName(ref startIndex);
                startIndex = SkipBlank(startIndex);
                if (_jsonCharArray[startIndex++] != ':')
                {
                    throw new JsonReadException(startIndex, "字符位置[" + startIndex + "]处，Json字符串解析content错");
                }
                JsonObject value = GetJsonObject(ref startIndex);
                content.AddJsonAttr(attrName, value);
                startIndex = SkipBlank(startIndex);
                if (_jsonCharArray[startIndex] == ',')
                {
                    startIndex = SkipBlank(++startIndex);
                    continue;
                }
                else if (_jsonCharArray[startIndex] == '}')
                {
                    break;
                }
                else
                {
                    throw new JsonReadException(startIndex, "字符位置[" + startIndex + "]处，Json字符串解析content出错");
                }
            }
            startIndex++;
            return content;
        }

        /// <summary>
        /// 解析Json Array对象
        /// </summary>
        /// <param name="startIndex">当前解析字符索引</param>
        /// <returns>Json对象</returns>
        private JsonObject GetJsonArray(ref int startIndex)
        {
            if (_jsonCharArray[startIndex] != '[')
            {
                throw new JsonReadException(startIndex, "字符位置[" + startIndex + "]处，Json字符串解析Array错");
            }
            JsonArray jsonArray = new JsonArray(startIndex);
            startIndex = SkipBlank(++startIndex);
            if (_jsonCharArray[startIndex] == ']')
            {
                startIndex++;
                return jsonArray;
            }
            while (true)
            {
                jsonArray.AddJsonObject(GetJsonObject(ref startIndex));
                startIndex = SkipBlank(startIndex);
                if (_jsonCharArray[startIndex] == ',')
                {
                    startIndex++;
                    continue;
                }
                else if (_jsonCharArray[startIndex] == ']')
                {
                    break;
                }
                else
                {
                    throw new JsonReadException(startIndex, "字符位置[" + startIndex + "]处，Json字符串解析Array错");
                }
            }
            startIndex++;
            return jsonArray;
        }
    }
}
