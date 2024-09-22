using LHZ.FastJson.Enum;
using LHZ.FastJson.Exceptions;
using LHZ.FastJson.Interface;
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
        public T Deserialize(params IJsonCustomConverter[] jsonCustomConverters)
        {
            Dictionary<Type, IJsonCustomConverter> customConverters = null;
            if (jsonCustomConverters != null && jsonCustomConverters.Length > 0)
            {
                customConverters = new Dictionary<Type, IJsonCustomConverter>(jsonCustomConverters.Length + 4);
                foreach (var item in jsonCustomConverters)
                {
                    if ((item.CustomItem & Enum.CustomConverter.JsonCustomConvertItem.CustomDeSerialize) != Enum.CustomConverter.JsonCustomConvertItem.CustomDeSerialize)
                    {
                        continue;
                    }
                    if (customConverters.ContainsKey(item.ConvertType))
                    {
                        customConverters[item.ConvertType] = item;
                    }
                    else
                    {
                        customConverters.Add(item.ConvertType, item);
                    }
                }
            }
            return JsonDeserialzerExpression<T>.Deserialzer(_obj, customConverters);
        }
    }
}
