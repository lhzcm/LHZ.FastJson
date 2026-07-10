using System;
using System.Collections;
using System.Collections.Generic;
using LHZ.FastJson.Exceptions;
using LHZ.FastJson.Json;
using LHZ.FastJson.Json.Attributes;
using LHZ.FastJson.Json.CustomConverter;
using LHZ.FastJson.JsonClass;
using NUnit.Framework;

namespace LHZ.FastJson.UnitTest
{
    /// <summary>
    /// 覆盖历史缺陷和边界行为的回归测试。
    /// </summary>
    public class RegressionTests
    {
        /// <summary>
        /// 验证读取器会拒绝尾随字符和非法数字。
        /// </summary>
        [Test]
        public void JsonReaderRejectsTrailingAndMalformedNumbers()
        {
            Exception exception;

            Assert.IsFalse(JsonReader.IsJsonString("{\"a\":1}xxx", out exception));
            Assert.IsFalse(JsonReader.IsJsonString("01", out exception));
            Assert.IsFalse(JsonReader.IsJsonString("1.", out exception));
            Assert.IsFalse(JsonReader.IsJsonString("[1,]", out exception));
        }

        /// <summary>
        /// 验证读取器接受标准数字和 Unicode 转义。
        /// </summary>
        [Test]
        public void JsonReaderAcceptsStandardNumberAndUnicodeEscapes()
        {
            Exception exception;

            Assert.IsTrue(JsonReader.IsJsonString("-1", out exception));
            Assert.IsTrue(JsonReader.IsJsonString("1e2", out exception));

            var unicodeString = new JsonReader("\"\\u0041\"").JsonRead();
            Assert.AreEqual("A", unicodeString.Value);

            var slashString = new JsonReader("\"c:\\\\temp\"").JsonRead();
            Assert.AreEqual("\"c:\\\\temp\"", slashString.ToString());
        }

        /// <summary>
        /// 验证 DateTime 默认序列化格式。
        /// </summary>
        [Test]
        public void SerializeDateTimeWithoutFormatterUsesDefaultFormat()
        {
            var value = new DateTime(2020, 7, 18, 1, 2, 3);

            var json = JsonConvert.Serialize(value);

            Assert.AreEqual("\"2020-07-18 01:02:03\"", json);
        }

        /// <summary>
        /// 验证可空属性会按实际值序列化。
        /// </summary>
        [Test]
        public void SerializeNullableProperties()
        {
            Assert.AreEqual("{\"Count\":12}", JsonConvert.Serialize(new NullablePropertyClass { Count = 12 }));
            Assert.AreEqual("{\"Count\":null}", JsonConvert.Serialize(new NullablePropertyClass { Count = null }));
        }

        /// <summary>
        /// 验证集合中的 null 元素可序列化。
        /// </summary>
        [Test]
        public void SerializeNullItemsInCollections()
        {
            var list = new List<object> { 1, null, "test" };
            Assert.AreEqual("[1,null,\"test\"]", JsonConvert.Serialize(list));

            var dictionary = new Dictionary<string, object>
            {
                { "a", null },
                { "b", 2 }
            };
            Assert.AreEqual("{\"a\":null,\"b\":2}", JsonConvert.Serialize(dictionary));
        }

        /// <summary>
        /// 验证重复 JSON 属性名会抛出异常。
        /// </summary>
        [Test]
        public void SerializeDuplicateJsonPropertyNamesThrows()
        {
            Assert.Throws<Exception>(() => JsonConvert.Serialize(new DuplicatePropertyNameClass { A = 1, B = 2 }));
        }

        /// <summary>
        /// 验证忽略属性不会影响后续属性反序列化。
        /// </summary>
        [Test]
        public void DeserializeIgnoredPropertyDoesNotSkipFollowingProperties()
        {
            var value = JsonConvert.Deserialize<IgnoredMiddlePropertyClass>("{\"A\":\"one\",\"Ignored\":99,\"B\":\"two\"}");

            Assert.AreEqual("one", value.A);
            Assert.AreEqual(0, value.Ignored);
            Assert.AreEqual("two", value.B);
        }

        /// <summary>
        /// 验证集合内部也会应用自定义转换器。
        /// </summary>
        [Test]
        public void CustomConvertersApplyInsideCollections()
        {
            var converter = new JsonCustomConvert<int>(jsonObject => 2);

            var list = JsonConvert.Deserialize<List<int>>("[1]", converter);
            Assert.AreEqual(2, list[0]);

            var dictionary = JsonConvert.Deserialize<Dictionary<string, int>>("{\"a\":1}", converter);
            Assert.AreEqual(2, dictionary["a"]);

            var nullable = JsonConvert.Deserialize<int?>("1", converter);
            Assert.AreEqual(2, nullable.Value);
        }
        /// <summary>
        /// 验证 JsonObject 类型会保留读取后的节点结构。
        /// </summary>
        [Test]
        public void DeserializeJsonObjectTypesPreservesParsedJson()
        {
            var content = JsonConvert.Deserialize<JsonContent>("{\"a\":1}");
            Assert.AreEqual("1", content["a"].Value.ToString());

            var jsonObject = JsonConvert.Deserialize<JsonObject>("{\"a\":1}");
            Assert.IsInstanceOf<JsonContent>(jsonObject);
            Assert.AreEqual("1", jsonObject["a"].Value.ToString());
        }
        /// <summary>
        /// 验证截断输入会返回 JSON 读取异常。
        /// </summary>
        [Test]
        public void JsonReaderRejectsTruncatedInputWithJsonReadException()
        {
            foreach (var json in new[] { "   ", "tru", "fals", "nul", "[", "[1", "{\"a\"", "{\"a\":1" })
            {
                Exception exception;

                Assert.IsFalse(JsonReader.IsJsonString(json, out exception), json);
                Assert.IsNotNull(exception, json);
                Assert.IsInstanceOf<JsonReadException>(exception, json);
            }
        }

        /// <summary>
        /// 验证根集合自引用应抛出异常而不是递归。
        /// </summary>
        [Test]
        [Explicit("Enable after root collection circular reference detection is implemented; current behavior can terminate the test process with StackOverflowException.")]
        public void SerializeSelfReferencingRootCollectionThrowsInsteadOfRecursing()
        {
            var list = new ArrayList();
            list.Add(list);
            try
            {
                var ret = JsonConvert.Serialize(list);
            }
            catch(Exception)
            {
                Assert.Pass();
            }
            Assert.Fail();
        }

        /// <summary>
        /// 验证 TryDeserialize 对有效 JSON 返回 true。
        /// </summary>
        [Test]
        public void TryDeserializeValidJsonReturnsTrue()
        {
            bool success = JsonConvert.TryDeserialize("{\"a\":1}", out IJsonObject result);
            Assert.IsTrue(success);
            Assert.IsNotNull(result);
            Assert.AreEqual("1", result["a"].Value.ToString());
        }

        /// <summary>
        /// 验证 TryDeserialize 对无效 JSON 返回 false。
        /// </summary>
        [Test]
        public void TryDeserializeInvalidJsonReturnsFalse()
        {
            bool success = JsonConvert.TryDeserialize("{invalid json}", out IJsonObject result);
            Assert.IsFalse(success);
            Assert.IsNull(result);
        }

        /// <summary>
        /// 验证 TryDeserialize&lt;T&gt; 对有效 JSON 返回 true。
        /// </summary>
        [Test]
        public void TryDeserializeGenericValidJsonReturnsTrue()
        {
            bool success = JsonConvert.TryDeserialize("{\"Id\":42,\"Name\":\"test\"}", out SimpleObjectRootClass result);
            Assert.IsTrue(success);
            Assert.IsNotNull(result);
            Assert.AreEqual(42, result.Id);
            Assert.AreEqual("test", result.Name);
        }

        /// <summary>
        /// 验证 TryDeserialize&lt;T&gt; 对无效 JSON 返回 false。
        /// </summary>
        [Test]
        public void TryDeserializeGenericInvalidJsonReturnsFalse()
        {
            bool success = JsonConvert.TryDeserialize<SimpleObjectRootClass>("not json", out SimpleObjectRootClass result);
            Assert.IsFalse(success);
            Assert.AreEqual(default(SimpleObjectRootClass), result);
        }

        /// <summary>
        /// 验证 JsonConvert.Deserialize 无泛型方法返回 IJsonObject。
        /// </summary>
        [Test]
        public void DeserializeNonGenericReturnsJsonObject()
        {
            var result = JsonConvert.Deserialize("{\"a\":1,\"b\":\"test\"}");
            Assert.IsNotNull(result);
            Assert.AreEqual("1", result["a"].Value.ToString());
            Assert.AreEqual("test", result["b"].Value.ToString());
        }

        /// <summary>
        /// 验证深度嵌套结构可以正确序列化和反序列化。
        /// </summary>
        [Test]
        public void DeeplyNestedObjectRoundTrip()
        {
            var obj = new DeepNestedClass
            {
                Level = 1,
                Child = new DeepNestedClass
                {
                    Level = 2,
                    Child = new DeepNestedClass
                    {
                        Level = 3,
                        Child = null
                    }
                }
            };
            var json = JsonConvert.Serialize(obj);
            var restored = JsonConvert.Deserialize<DeepNestedClass>(json);

            Assert.AreEqual(1, restored.Level);
            Assert.AreEqual(2, restored.Child.Level);
            Assert.AreEqual(3, restored.Child.Child.Level);
            Assert.IsNull(restored.Child.Child.Child);
        }

        /// <summary>
        /// 验证反序列化时类型不匹配会抛出 JsonDeserializationException 异常。
        /// </summary>
        [Test]
        public void DeserializeTypeMismatchThrowsException()
        {
            Assert.Throws<JsonDeserializationException>(() => JsonConvert.Deserialize<int>("\"not a number\""));
        }

        /// <summary>
        /// 验证 JsonConvert.Serialize 对象的往返一致性。
        /// </summary>
        [Test]
        public void ObjectRoundTripPreservesData()
        {
            var original = new SimpleObjectRootClass { Id = 100, Name = "RoundTrip" };
            var json = JsonConvert.Serialize(original);
            var restored = JsonConvert.Deserialize<SimpleObjectRootClass>(json);

            Assert.AreEqual(original.Id, restored.Id);
            Assert.AreEqual(original.Name, restored.Name);
        }

        /// <summary>
        /// 验证 JsonArray 节点可正确迭代。
        /// </summary>
        [Test]
        public void JsonArrayEnumeration()
        {
            var arr = (JsonArray)new JsonReader("[1,2,3]").JsonRead();
            int sum = 0;
            foreach (var item in arr)
            {
                sum += int.Parse(item.Value.ToString());
            }
            Assert.AreEqual(6, sum);
        }

        /// <summary>
        /// 验证 JsonContent 节点可正确迭代。
        /// </summary>
        [Test]
        public void JsonContentEnumeration()
        {
            var content = (JsonContent)new JsonReader("{\"a\":1,\"b\":2}").JsonRead();
            int count = 0;
            foreach (var kvp in content)
            {
                count++;
                Assert.IsNotNull(kvp.Key);
                Assert.IsNotNull(kvp.Value);
            }
            Assert.AreEqual(2, count);
        }

        public class NullablePropertyClass
        {
            public int? Count { get; set; }
        }

        public class DuplicatePropertyNameClass
        {
            [JsonProperty("same")]
            public int A { get; set; }

            [JsonProperty("same")]
            public int B { get; set; }
        }

        public class IgnoredMiddlePropertyClass
        {
            public string A { get; set; }

            [JsonIgnored(Enum.JsonMethods.Deserialize)]
            public int Ignored { get; set; }

            public string B { get; set; }
        }

        public class SimpleObjectRootClass
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class DeepNestedClass
        {
            public int Level { get; set; }
            public DeepNestedClass Child { get; set; }
        }
    }
}
