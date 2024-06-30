using LHZ.FastJson.Enum.CustomConverter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LHZ.FastJson.Json.CustomConverter
{
    // /// <summary>
    // /// 自定义转换类型
    // /// </summary>
    // /// <typeparam name="T">需要自定义转换的类型</typeparam>
    // public interface IJsonCustomConverter<T> : IJsonCustomConverter
    // {
    //     /// <summary>
    //     /// 自定义序列化方法
    //     /// </summary>
    //     /// <param name="dist">序列化对象</param>
    //     /// <returns>序列化字符串</returns>
    //     public string Serialize(T dist);

    //     /// <summary>
    //     /// 自定义反序列方法
    //     /// </summary>
    //     /// <param name="jsonObject">json对象</param>
    //     /// <returns>反序列化对象</returns>
    //     public T Deserialize(IJsonObject jsonObject);
    // }


    /// <summary>
    /// 自定义转换类型
    /// </summary>
    public interface IJsonCustomConverter
    {
        /// <summary>
        /// 自定义序列化方法
        /// </summary>
        /// <param name="dist">序列化对象</param>
        /// <returns>序列化字符串</returns>
        string Serialize(object dist);

        /// <summary>
        /// 自定义反序列方法
        /// </summary>
        /// <param name="jsonObject">json对象</param>
        /// <returns>反序列化对象</returns>
        object Deserialize(IJsonObject jsonObject);

        /// <summary>
        /// 自定义转换类型
        /// </summary>
        Type ConvertType { get; }

        /// <summary>
        /// 包含的自定义项（序列化|反序列化）
        /// </summary>
        JsonCustomConvertItem CustomItem{get;}
    }
}
