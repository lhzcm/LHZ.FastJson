using LHZ.FastJson.Json;
using LHZ.FastJson.Json.Attributes;
using LHZ.FastJson.Json.CustomConverter;
using LHZ.FastJson.JsonClass;
using LHZ.FastJson.Exceptions;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;

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
            Assert.AreEqual("\"c:\\\\temp\"", slashString.ToJsonString());
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
        /// 验证对象反序列化会拒绝非对象根节点。
        /// </summary>
        [Test]
        public void DeserializeObjectRejectsNonObjectJsonRoot()
        {
            Assert.Throws<JsonDeserializationException>(() => JsonConvert.Deserialize<SimpleObjectRootClass>("1"));
            Assert.Throws<JsonDeserializationException>(() => JsonConvert.Deserialize<SimpleObjectRootClass>("[1]"));
            Assert.Throws<JsonDeserializationException>(() => JsonConvert.Deserialize<SimpleObjectRootClass>("\"abc\""));
        }

        /// <summary>
        /// 验证 JsonObject 类型会保留读取后的节点结构。
        /// </summary>
        [Test]
        public void DeserializeJsonObjectTypesPreservesParsedJson()
        {
            var content = JsonConvert.Deserialize<JsonContent>("{\"a\":1}");
            Assert.AreEqual("1", content["a"].Value);

            var jsonObject = JsonConvert.Deserialize<JsonObject>("{\"a\":1}");
            Assert.IsInstanceOf<JsonContent>(jsonObject);
            Assert.AreEqual("1", jsonObject["a"].Value);
        }

        /// <summary>
        /// 验证字典键名可转换为目标键类型。
        /// </summary>
        [Test]
        public void DeserializeDictionaryConvertsPropertyNamesToTargetKeyType()
        {
            var dictionary = JsonConvert.Deserialize<Dictionary<int, string>>("{\"1\":\"one\",\"2\":\"two\"}");

            Assert.AreEqual("one", dictionary[1]);
            Assert.AreEqual("two", dictionary[2]);
        }

        /// <summary>
        /// 验证非字符串键字典可序列化后反序列化。
        /// </summary>
        [Test]
        public void DictionaryWithNonStringKeysRoundTrips()
        {
            var source = new Dictionary<int, string>
            {
                { 1, "one" },
                { 2, "two" }
            };

            var json = JsonConvert.Serialize(source);
            var result = JsonConvert.Deserialize<Dictionary<int, string>>(json);

            Assert.AreEqual(source, result);
        }

        /// <summary>
        /// 验证集合接口会反序列化为具体集合。
        /// </summary>
        [Test]
        public void DeserializeCollectionInterfacesUsesConcreteCollections()
        {
            var list = JsonConvert.Deserialize<IList<int>>("[1,2]");
            CollectionAssert.AreEqual(new[] { 1, 2 }, list);

            var enumerable = JsonConvert.Deserialize<IEnumerable<int>>("[3,4]");
            CollectionAssert.AreEqual(new[] { 3, 4 }, enumerable);

            var readOnlyList = JsonConvert.Deserialize<IReadOnlyList<int>>("[5,6]");
            CollectionAssert.AreEqual(new[] { 5, 6 }, readOnlyList);
        }

        /// <summary>
        /// 验证特殊浮点值不会产生非法 JSON。
        /// </summary>
        [Test]
        public void SerializeSpecialFloatingPointValuesDoesNotProduceInvalidJson()
        {
            AssertSerializationProducesValidJsonOrThrows(double.NaN);
            AssertSerializationProducesValidJsonOrThrows(double.PositiveInfinity);
            AssertSerializationProducesValidJsonOrThrows(double.NegativeInfinity);
            AssertSerializationProducesValidJsonOrThrows(float.NaN);
            AssertSerializationProducesValidJsonOrThrows(float.PositiveInfinity);
            AssertSerializationProducesValidJsonOrThrows(float.NegativeInfinity);
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

            Assert.Throws<Exception>(() => JsonConvert.Serialize(list));
        }

        private static void AssertSerializationProducesValidJsonOrThrows(object value)
        {
            try
            {
                var json = JsonConvert.Serialize(value);
                Exception exception;

                Assert.IsTrue(JsonReader.IsJsonString(json, out exception),
                    $"Serializing {value.GetType().Name} value '{value}' produced invalid JSON '{json}'. Reader error: {exception?.Message}");
            }
            catch (AssertionException)
            {
                throw;
            }
            catch (Exception)
            {
                // Throwing is acceptable for non-standard JSON values like NaN and Infinity.
            }
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
    }
}
