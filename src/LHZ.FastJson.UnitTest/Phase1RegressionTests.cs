using System;
using LHZ.FastJson.Exceptions;
using LHZ.FastJson.JsonClass;
using NUnit.Framework;

namespace LHZ.FastJson.UnitTest
{
    /// <summary>
    /// Regression tests for the first batch of correctness and stability fixes.
    /// </summary>
    [NonParallelizable]
    public class Phase1RegressionTests
    {
        [Test]
        public void JsonBooleanTrueAndFalseAreNotSwapped()
        {
            Assert.That(JsonBoolean.True.Value, Is.True);
            Assert.That(JsonBoolean.False.Value, Is.False);
            Assert.That(JsonBoolean.True.ToString(), Is.EqualTo("true"));
            Assert.That(JsonBoolean.False.ToString(), Is.EqualTo("false"));

            var content = new JsonContent();
            content.AddJsonProperty("ok", JsonBoolean.True);
            content.AddJsonProperty("no", JsonBoolean.False);

            Assert.That(content.ToString(), Is.EqualTo("{\"ok\":true,\"no\":false}"));
        }

        [Test]
        public void SerializerSkipsStaticPropertiesAndIndexers()
        {
            var value = new StaticAndIndexerModel { A = 1 };

            var json = JsonConvert.Serialize(value);

            Assert.That(json, Is.EqualTo("{\"A\":1}"));
        }

        [Test]
        public void DeserializerSkipsStaticPropertiesAndIndexers()
        {
            var value = JsonConvert.Deserialize<StaticAndIndexerModel>("{\"A\":2,\"StaticValue\":123}");

            Assert.That(value.A, Is.EqualTo(2));
            Assert.That(StaticAndIndexerModel.StaticValue, Is.EqualTo(99));
        }

        [Test]
        public void NestedStructPropertyCanRoundTrip()
        {
            var value = new PointHolder
            {
                P = new Point { X = 1, Y = 2 },
                A = 3
            };

            var json = JsonConvert.Serialize(value);
            var restored = JsonConvert.Deserialize<PointHolder>(json);

            Assert.That(json, Does.Contain("\"P\":{\"X\":1,\"Y\":2}"));
            Assert.That(restored.P.X, Is.EqualTo(1));
            Assert.That(restored.P.Y, Is.EqualTo(2));
            Assert.That(restored.A, Is.EqualTo(3));
        }

        [Test]
        public void RootStructCanBeDeserialized()
        {
            var value = JsonConvert.Deserialize<Point>("{\"X\":4,\"Y\":5}");

            Assert.That(value.X, Is.EqualTo(4));
            Assert.That(value.Y, Is.EqualTo(5));
        }

        [Test]
        public void NullableStructPropertyCanRoundTrip()
        {
            var value = new NullablePointHolder { P = new Point { X = 7, Y = 8 } };

            var restored = JsonConvert.Deserialize<NullablePointHolder>(JsonConvert.Serialize(value));

            Assert.That(restored.P.HasValue, Is.True);
            Assert.That(restored.P.Value.X, Is.EqualTo(7));
            Assert.That(restored.P.Value.Y, Is.EqualTo(8));
        }

        [Test]
        public void JsonReaderRejectsInputBeyondMaxDepth()
        {
            var deep = new string('[', 1000) + new string(']', 1000);

            Assert.Throws<JsonReadException>(() => new JsonReader(deep).JsonRead());
        }

        [Test]
        public void DirectDeserializerRejectsSkippedValueBeyondMaxDepth()
        {
            var deep = new string('[', 1000) + new string(']', 1000);
            var json = "{\"A\":1,\"Unknown\":" + deep + "}";

            Assert.Throws<JsonReadException>(() => JsonConvert.Deserialize<OnlyA>(json));
        }

        [Test]
        public void MaxDepthCanBeConfigured()
        {
            var options = new JsonConvertOptions { MaxDepth = 4 };

            Assert.DoesNotThrow(() => new JsonReader("[[[[]]]]", options).JsonRead());
            Assert.Throws<JsonReadException>(() => new JsonReader("[[[[[]]]]]", options).JsonRead());
            Assert.Throws<JsonReadException>(() => JsonConvert.Deserialize("[[[[[]]]]]", options));

            //The default options are not affected
            Assert.DoesNotThrow(() => new JsonReader("[[[[[]]]]]").JsonRead());
        }

        [Test]
        public void MaxDepthAppliesToDirectDeserializer()
        {
            var options = new JsonConvertOptions { MaxDepth = 4 };
            var json = "{\"A\":1,\"Unknown\":[[[[[]]]]]}";

            Assert.Throws<JsonReadException>(() => JsonConvert.Deserialize<OnlyA>(json, options));
            Assert.DoesNotThrow(() => JsonConvert.Deserialize<OnlyA>(json));
        }

        [Test]
        public void MaxDepthMustBePositive()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new JsonConvertOptions().MaxDepth = 0);
        }

        public struct Point
        {
            public int X { get; set; }
            public int Y { get; set; }
        }

        public class PointHolder
        {
            public Point P { get; set; }
            public int A { get; set; }
        }

        public class NullablePointHolder
        {
            public Point? P { get; set; }
        }

        public class StaticAndIndexerModel
        {
            public int A { get; set; }

            public static int StaticValue { get; set; } = 99;

            public string this[int index]
            {
                get { return "indexer"; }
            }
        }

        public class OnlyA
        {
            public int A { get; set; }
        }
    }
}
