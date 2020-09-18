using LHZ.FastJson.Json;
using LHZ.FastJson.Json.Format;
using LHZ.FastJson.JsonClass;
using System;
using System.Collections.Generic;
using System.Text;

namespace LHZ.FastJson
{
    /// <summary>
    /// Json对象解析转换静态类型
    /// 可以对Json String序列化成object
    /// 可以对object反序列化成Json String
    /// </summary>
    public static class JsonConvert
    {
        /// <summary>
        /// Json字符串反序列化成JsonObject类型
        /// </summary>
        /// <param name="jsonString">Json字符串</param>
        /// <returns>JsonObject类型对象</returns>
        public static IJsonObject Deserialize(string jsonString)
        {
            JsonReader reader = new JsonReader(jsonString);
            return reader.JsonRead();
        }

        /// <summary>
        /// Json字符串反序列化成T类型
        /// </summary>
        /// <typeparam name="T">反序列化目标类型</typeparam>
        /// <param name="jsonString">JsonObject类型</param>
        /// <returns>反序列化目标对象</returns>
        public static T Deserialize<T>(string jsonString)
        {
            JsonDeserializer<T> deserializer = new JsonDeserializer<T>(jsonString);
            return deserializer.Deserialize();
        }

        /// <summary>
        /// 把对象进行序列化成Json字符串
        /// </summary>
        /// <param name="obj">需要序列化的对象</param>
        /// <returns>Json字符串</returns>
        public static string Serialize(object obj)
        {
            JsonSerializer serializer = new JsonSerializer(obj);
            return serializer.Serialize();
        }

        /// <summary>
        /// 把对象进行序列化成Json字符串（带格式化）
        /// </summary>
        /// <param name="obj">需要序列化的对象</param>
        /// <param name="formats">格式化类型</param>
        /// <returns>Json字符串</returns>
        public static string Serialize(object obj, params IJsonFormat[] formats)
        {
            JsonSerializer serializer = new JsonSerializer(obj, formats);
            return serializer.Serialize();
        }
    }
}
