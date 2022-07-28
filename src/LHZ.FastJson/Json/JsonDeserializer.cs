using LHZ.FastJson.Enum;
using LHZ.FastJson.Exceptions;
using LHZ.FastJson.JsonClass;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace LHZ.FastJson.Json
{ 
    /// <summary>
    /// 反序列化类
    /// </summary>
    /// <typeparam name="T">需要进行反序列化的类</typeparam>
    public class JsonDeserializer<T>
    {
        private IJsonObject _obj;
        public JsonDeserializer(IJsonObject obj)
        {
            this._obj = obj;
        }
        public JsonDeserializer(JsonReader reader)
        {
            this._obj = reader.JsonRead();
        }
        public JsonDeserializer(string jsonString)
        {
            JsonReader reader = new JsonReader(jsonString);
            this._obj = reader.JsonRead();
        }

        /// <summary>
        /// 进行反序列化
        /// </summary>
        /// <returns>返回反序列化类型对象</returns>
        public T Deserialize()
        {
            return JsonDeserialzerExpression<T>.Deserialzer(_obj);
        }
    }
}
