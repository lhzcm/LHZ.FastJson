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
    }
}
