using LHZ.FastJson.Json;
using LHZ.FastJson.JsonClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LHZ.FastJson
{
    public static class JsonCommonExtend
    {

        /// <summary>
        /// 把Json对象转化成目标对象
        /// </summary>
        /// <typeparam name="T">需要转化的目标对象</typeparam>
        /// <returns>目标对象</returns>
        public static T ToObject<T>(this IJsonObject jsonObj)
        {
            JsonDeserializer<T> deserializer = new JsonDeserializer<T>(jsonObj);
            return deserializer.Deserialize();
        }

        /// <summary>
        /// 把Json字符串反序列化成T对象
        /// </summary>
        /// <typeparam name="T">反序列化的对象类型</typeparam>
        /// <param name="jsonString">Json字符串</param>
        /// <returns>反序列化成的对象</returns>
        public static T FromJson<T>(this String jsonString)
        {
            return JsonConvert.Deserialize<T>(jsonString);
        }

        /// <summary>
        /// 把对象序列化成Json字符串
        /// </summary>
        /// <param name="target">目标对象</param>
        /// <returns>json字符串</returns>
        public static string ToJson(this Object target)
        {
            return JsonConvert.Serialize(target);
        }
    }
}
