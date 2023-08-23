using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LHZ.FastJson.Json.CustomConverter
{
    /// <summary>
    /// 自定义转换类型
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    public abstract class JsonCustomConvert<T> : IJsonCustomConverter
    {
        /// <summary>
        /// 自定义转换的类型
        /// </summary>
        public Type ConvertType => typeof(T);

        /// <summary>
        /// 自定义序列化方法
        /// </summary>
        /// <param name="dist">序列化对象</param>
        /// <returns>序列化字符串</returns>
        public virtual string Serialize(T dist)
        {
            return (new JsonSerializer(dist)).Serialize();
        }

        /// <summary>
        /// 自定义反序列方法
        /// </summary>
        /// <param name="jsonObject">json对象</param>
        /// <returns>反序列化对象</returns>
        public virtual T Deserialize(IJsonObject jsonObject)
        {
            return JsonDeserialzerExpression<T>.Deserialzer(jsonObject);
        }

        string IJsonCustomConverter.Serialize(object dist)
        {
            return Serialize((T)dist);
        }

        object IJsonCustomConverter.Deserialize(IJsonObject jsonObject)
        {
            return Deserialize(jsonObject);
        }
    }
}
