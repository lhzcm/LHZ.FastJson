using LHZ.FastJson.Json;
using LHZ.FastJson.Json.Attributes;
using LHZ.FastJson.Json.CustomConverter;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace LHZ.FastJson.UnitTest
{
    public class RegressionTests
    {
        [Test]
        public void JsonReaderRejectsTrailingAndMalformedNumbers()
        {
            Exception exception;

            Assert.IsFalse(JsonReader.IsJsonString("{\"a\":1}xxx", out exception));
            Assert.IsFalse(JsonReader.IsJsonString("01", out exception));
            Assert.IsFalse(JsonReader.IsJsonString("1.", out exception));
            Assert.IsFalse(JsonReader.IsJsonString("[1,]", out exception));
        }

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

        [Test]
        public void SerializeDateTimeWithoutFormatterUsesDefaultFormat()
        {
            var value = new DateTime(2020, 7, 18, 1, 2, 3);

            var json = JsonConvert.Serialize(value);

            Assert.AreEqual("\"2020-07-18 01:02:03\"", json);
        }

        [Test]
        public void SerializeNullableProperties()
        {
            Assert.AreEqual("{\"Count\":12}", JsonConvert.Serialize(new NullablePropertyClass { Count = 12 }));
            Assert.AreEqual("{\"Count\":null}", JsonConvert.Serialize(new NullablePropertyClass { Count = null }));
        }

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

        [Test]
        public void SerializeDuplicateJsonPropertyNamesThrows()
        {
            Assert.Throws<Exception>(() => JsonConvert.Serialize(new DuplicatePropertyNameClass { A = 1, B = 2 }));
        }

        [Test]
        public void DeserializeIgnoredPropertyDoesNotSkipFollowingProperties()
        {
            var value = JsonConvert.Deserialize<IgnoredMiddlePropertyClass>("{\"A\":\"one\",\"Ignored\":99,\"B\":\"two\"}");

            Assert.AreEqual("one", value.A);
            Assert.AreEqual(0, value.Ignored);
            Assert.AreEqual("two", value.B);
        }

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
    }
}
