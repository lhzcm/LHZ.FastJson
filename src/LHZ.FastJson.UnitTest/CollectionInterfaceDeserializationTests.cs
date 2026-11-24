using System;
using System.Collections;
using System.Collections.Generic;
using LHZ.FastJson.Json;
using NUnit.Framework;

namespace LHZ.FastJson.UnitTest
{
    /// <summary>
    /// 验证反序列化能把集合/字典接口属性落到默认实现上，并且可以再次序列化。
    /// </summary>
    public class CollectionInterfaceDeserializationTests
    {
        private class CollectionsDto
        {
            public Dictionary<string, int> ConcreteDict { get; set; }
            public IDictionary<string, int> Dict { get; set; }
            public IReadOnlyDictionary<string, int> ReadOnlyDict { get; set; }
            public IDictionary UntypedDict { get; set; }
            public List<int> ConcreteList { get; set; }
            public IList<int> List { get; set; }
            public ICollection<int> Collection { get; set; }
            public IEnumerable<int> Enumerable { get; set; }
            public IReadOnlyList<int> ReadOnlyList { get; set; }
            public IList UntypedList { get; set; }
            public Dictionary<int, string> IntKeys { get; set; }
            public int[] Array { get; set; }
            public object Boxed { get; set; }
        }

        private static CollectionsDto Sample()
        {
            return new CollectionsDto
            {
                ConcreteDict = new Dictionary<string, int> { ["a"] = 1 },
                Dict = new Dictionary<string, int> { ["a"] = 1 },
                ReadOnlyDict = new Dictionary<string, int> { ["a"] = 1 },
                UntypedDict = new Dictionary<string, object> { ["a"] = 1 },
                ConcreteList = new List<int> { 1, 2 },
                List = new List<int> { 1, 2 },
                Collection = new List<int> { 1, 2 },
                Enumerable = new List<int> { 1, 2 },
                ReadOnlyList = new List<int> { 1, 2 },
                UntypedList = new List<object> { 1, "x" },
                IntKeys = new Dictionary<int, string> { [7] = "seven" },
                Array = new[] { 1, 2 },
                Boxed = new Dictionary<string, object> { ["b"] = 2 }
            };
        }

        /// <summary>
        /// 验证接口属性落到 Dictionary/List 上，且内容与重新序列化的文本一致。
        /// </summary>
        [Test]
        public void InterfaceCollectionsRoundTrip()
        {
            var json = new JsonSerializer(Sample()).Serialize();
            var back = JsonConvert.Deserialize<CollectionsDto>(json);

            Assert.That(back.Dict, Is.TypeOf<Dictionary<string, int>>());
            Assert.That(back.ReadOnlyDict, Is.TypeOf<Dictionary<string, int>>());
            Assert.That(back.UntypedDict, Is.TypeOf<Dictionary<string, object>>());
            Assert.That(back.List, Is.TypeOf<List<int>>());
            Assert.That(back.Collection, Is.TypeOf<List<int>>());
            Assert.That(back.Enumerable, Is.TypeOf<List<int>>());
            Assert.That(back.ReadOnlyList, Is.TypeOf<List<int>>());
            Assert.That(back.UntypedList, Is.TypeOf<List<object>>());

            Assert.AreEqual(1, back.Dict["a"]);
            Assert.AreEqual(1, back.ReadOnlyDict["a"]);
            Assert.AreEqual("seven", back.IntKeys[7]);
            CollectionAssert.AreEqual(new[] { 1, 2 }, back.List);
            CollectionAssert.AreEqual(new[] { 1, 2 }, back.Collection);
            CollectionAssert.AreEqual(new[] { 1, 2 }, back.Enumerable);
            CollectionAssert.AreEqual(new[] { 1, 2 }, back.ReadOnlyList);
            CollectionAssert.AreEqual(new[] { 1, 2 }, back.Array);

            Assert.AreEqual(json, new JsonSerializer(back).Serialize());
        }

        /// <summary>
        /// 验证字典属性按 JSON 对象、集合属性按 JSON 数组输出（与反序列化对称）。
        /// </summary>
        [Test]
        public void InterfaceCollectionsUseObjectAndArrayShape()
        {
            var json = new JsonSerializer(Sample()).Serialize();

            StringAssert.Contains("\"Dict\":{\"a\":1}", json);
            StringAssert.Contains("\"ReadOnlyDict\":{\"a\":1}", json);
            StringAssert.Contains("\"List\":[1,2]", json);
            StringAssert.Contains("\"ReadOnlyList\":[1,2]", json);
            StringAssert.Contains("\"IntKeys\":{\"7\":\"seven\"}", json);
        }

        /// <summary>
        /// 验证 JSON 节点（object 类型属性的反序列化结果）可以按原样写回。
        /// </summary>
        [Test]
        public void JsonNodesSerializeBackVerbatim()
        {
            var json = "{\"a\":[1,\"x\",true,null],\"b\":{\"c\":2}}";

            var tree = JsonConvert.Deserialize(json);

            Assert.AreEqual(json, JsonConvert.Serialize(tree));
        }
    }
}
