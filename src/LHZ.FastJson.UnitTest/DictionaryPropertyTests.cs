using System;
using System.Collections;
using System.Collections.Generic;
using LHZ.FastJson.Json;
using NUnit.Framework;

namespace LHZ.FastJson.UnitTest
{
    /// <summary>
    /// 验证字典属性按 JSON 对象输出，无论属性声明的是具体字典类型还是泛型字典接口。
    /// </summary>
    public class DictionaryPropertyTests
    {
        private class DictDto
        {
            public Dictionary<string, int> Concrete { get; set; }
            public IDictionary<string, int> GenericInterface { get; set; }
            public IReadOnlyDictionary<string, int> ReadOnlyInterface { get; set; }
            public IDictionary NonGeneric { get; set; }
            public Dictionary<string, int> Empty { get; set; }
            public Dictionary<string, int> Missing { get; set; }
        }

        private class IntKeyDto
        {
            public Dictionary<int, string> Values { get; set; }
        }

        private class NestedDto
        {
            public Dictionary<string, Dictionary<string, int>> Nested { get; set; }
        }

        private class Pairs<T> : Dictionary<string, T>
        {
        }

        private class GenericSubclassDto
        {
            public Pairs<int> Values { get; set; }
        }

        private static Dictionary<string, int> PairsValue()
        {
            return new Dictionary<string, int> { ["k"] = 1 };
        }

        /// <summary>
        /// 验证各种声明类型的字典属性都序列化为 JSON 对象。
        /// </summary>
        [Test]
        public void DictionaryPropertiesAreObjects()
        {
            var dto = new DictDto
            {
                Concrete = PairsValue(),
                GenericInterface = PairsValue(),
                ReadOnlyInterface = PairsValue(),
                NonGeneric = (IDictionary)PairsValue(),
                Empty = new Dictionary<string, int>(),
                Missing = null
            };

            var json = new JsonSerializer(dto).Serialize();

            Assert.AreEqual(
                "{\"Concrete\":{\"k\":1},\"GenericInterface\":{\"k\":1},\"ReadOnlyInterface\":{\"k\":1},\"NonGeneric\":{\"k\":1},\"Empty\":{},\"Missing\":null}",
                json);
        }

        /// <summary>
        /// 验证字典接口类型的根对象也序列化为 JSON 对象。
        /// </summary>
        [Test]
        public void DictionaryInterfaceRootIsObject()
        {
            Assert.AreEqual("{\"k\":1}", new JsonSerializer((IDictionary<string, int>)PairsValue()).Serialize());
            Assert.AreEqual("{\"k\":1}", new JsonSerializer((IReadOnlyDictionary<string, int>)PairsValue()).Serialize());
        }

        /// <summary>
        /// 验证非字符串键按字符串输出。
        /// </summary>
        [Test]
        public void DictionaryKeysAreWrittenAsStrings()
        {
            var dto = new IntKeyDto { Values = new Dictionary<int, string> { [7] = "seven" } };

            Assert.AreEqual("{\"Values\":{\"7\":\"seven\"}}", new JsonSerializer(dto).Serialize());
        }

        /// <summary>
        /// 验证嵌套字典和泛型字典子类仍然是对象而不是数组。
        /// </summary>
        [Test]
        public void NestedAndSubclassDictionariesAreObjects()
        {
            var nested = new NestedDto
            {
                Nested = new Dictionary<string, Dictionary<string, int>>
                {
                    ["a"] = new Dictionary<string, int> { ["b"] = 1 }
                }
            };
            Assert.AreEqual("{\"Nested\":{\"a\":{\"b\":1}}}", new JsonSerializer(nested).Serialize());

            var subclass = new GenericSubclassDto { Values = new Pairs<int> { ["k"] = 1 } };
            Assert.AreEqual("{\"Values\":{\"k\":1}}", new JsonSerializer(subclass).Serialize());
        }
    }
}
