using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.ExceptionServices;
using LHZ.FastJson.Json.Attributes;
using NUnit.Framework;

namespace LHZ.FastJson.UnitTest
{
    // 本文件记录已经确认的问题：每个类对应一个问题，类级别用 [Ignore] 标记，保持测试套件为绿色。
    // 问题修复后删除对应类的 [Ignore] 即可，这些测试就成为回归测试。

    internal static class KnownIssueSupport
    {
        public static string J(string json)
        {
            return json.Replace('\'', '"');
        }

        public static T De<T>(string json)
        {
            return JsonConvert.Deserialize<T>(J(json));
        }

        /// <summary>
        /// 序列化后再按值的运行时类型反序列化，用于参数化的往返测试。
        /// </summary>
        public static object RoundTrip(object value)
        {
            string json = JsonConvert.Serialize(value);
            var method = typeof(JsonConvert).GetMethods().First(m => m.Name == "Deserialize" && m.IsGenericMethodDefinition && m.GetParameters().Length == 1);
            try
            {
                return method.MakeGenericMethod(value.GetType()).Invoke(null, new object[] { json });
            }
            catch (System.Reflection.TargetInvocationException e)
            {
                ExceptionDispatchInfo.Capture(e.InnerException).Throw();
                throw;
            }
        }
    }

    public class KnownIssuePoint { public int X { get; set; } public int Y { get; set; } }

    public class KnownIssueRenamed
    {
        [JsonProperty("a\"b\\c")]
        public int Value { get; set; }
    }

    public enum KnownIssueByteEnum : byte { A, B }
    public enum KnownIssueSByteEnum : sbyte { A, B }
    public enum KnownIssueShortEnum : short { A, B }
    public enum KnownIssueUShortEnum : ushort { A, B }
    public enum KnownIssueUIntEnum : uint { A, B }
    public enum KnownIssueLongEnum : long { Small = 1, Big = 5000000000 }
    public enum KnownIssueULongEnum : ulong { Small = 1, Big = 18000000000000000000 }
    public enum KnownIssueColor { Red = 0, Green = 1 }
    [Flags] public enum KnownIssuePerm { None = 0, Read = 1, Write = 2 }

    /// <summary>
    /// 属性名 / 字典键里的转义序列没有被还原：\" 会让解析失败，\\、\n、\uXXXX 会原样保留反斜杠文本。
    /// 序列化时会转义这些字符，所以含特殊字符的键无法往返。
    /// </summary>
    [Ignore("已知问题：对象属性名和字典键中的转义序列（\\\"、\\\\、\\n、\\uXXXX）没有被还原。")]
    public class EscapedPropertyNameKnownIssueTests
    {
        [Test]
        public void EscapedQuoteInDictionaryKey()
        {
            var map = KnownIssueSupport.De<Dictionary<string, int>>("{'a\\\"b':1}");
            Assert.That(map.Keys, Is.EqualTo(new[] { "a\"b" }));
        }

        [Test]
        public void EscapedBackslashInDictionaryKey()
        {
            var map = KnownIssueSupport.De<Dictionary<string, int>>("{'a\\\\b':1}");
            Assert.That(map.Keys, Is.EqualTo(new[] { "a\\b" }));
        }

        [Test]
        public void EscapedNewlineInDictionaryKey()
        {
            var map = KnownIssueSupport.De<Dictionary<string, int>>("{'a\\nb':1}");
            Assert.That(map.Keys, Is.EqualTo(new[] { "a\nb" }));
        }

        [Test]
        public void UnicodeEscapeInDictionaryKey()
        {
            var map = KnownIssueSupport.De<Dictionary<string, int>>("{'\\u0041':1}");
            Assert.That(map.Keys, Is.EqualTo(new[] { "A" }));
        }

        [Test]
        public void UnicodeEscapeInObjectPropertyName()
        {
            var point = KnownIssueSupport.De<KnownIssuePoint>("{'\\u0058':7}");
            Assert.That(point.X, Is.EqualTo(7));
        }

        [Test]
        public void UnknownPropertyWithEscapedQuoteInNameIsSkipped()
        {
            var point = KnownIssueSupport.De<KnownIssuePoint>("{'z\\\"z':1,'X':2}");
            Assert.That(point.X, Is.EqualTo(2));
        }

        [Test]
        public void DictionaryWithSpecialCharactersInKeysRoundTrips()
        {
            var original = new Dictionary<string, int>
            {
                ["quote\"inside"] = 1,
                ["back\\slash"] = 2,
                ["new\nline"] = 3,
                ["tab\tchar"] = 4
            };
            var back = JsonConvert.Deserialize<Dictionary<string, int>>(JsonConvert.Serialize(original));
            Assert.That(back, Is.EquivalentTo(original));
        }

        [Test]
        public void JsonPropertyNameWithSpecialCharactersIsEscapedWhenSerializing()
        {
            Assert.That(JsonConvert.Serialize(new KnownIssueRenamed { Value = 1 }), Is.EqualTo("{\"a\\\"b\\\\c\":1}"));
        }
    }

    /// <summary>
    /// 序列化把枚举写成底层数值，但反序列化只能读回 int 底层类型且有定义的值：
    /// byte / sbyte / short / ushort / uint / long / ulong 底层类型、[Flags] 组合值、未定义的数值都读不回来。
    /// </summary>
    [Ignore("已知问题：非 int 底层类型的枚举、[Flags] 组合值、未定义的数值无法反序列化，往返失败。")]
    public class EnumRoundTripKnownIssueTests
    {
        [TestCase(KnownIssueByteEnum.B)]
        [TestCase(KnownIssueSByteEnum.B)]
        [TestCase(KnownIssueShortEnum.B)]
        [TestCase(KnownIssueUShortEnum.B)]
        [TestCase(KnownIssueUIntEnum.B)]
        [TestCase(KnownIssueLongEnum.Small)]
        [TestCase(KnownIssueLongEnum.Big)]
        [TestCase(KnownIssueULongEnum.Big)]
        public void EnumsWithNonIntUnderlyingTypesRoundTrip(object value)
        {
            Assert.That(KnownIssueSupport.RoundTrip(value), Is.EqualTo(value));
        }

        [Test]
        public void FlagsCombinationRoundTrips()
        {
            Assert.That(KnownIssueSupport.RoundTrip(KnownIssuePerm.Read | KnownIssuePerm.Write), Is.EqualTo(KnownIssuePerm.Read | KnownIssuePerm.Write));
        }

        [Test]
        public void UndefinedNumericValueRoundTrips()
        {
            Assert.That(KnownIssueSupport.RoundTrip((KnownIssueColor)99), Is.EqualTo((KnownIssueColor)99));
        }

        [Test]
        public void EnumPropertiesOfNonIntTypesRoundTrip()
        {
            var original = new Dictionary<string, KnownIssueLongEnum> { ["a"] = KnownIssueLongEnum.Big };
            var back = JsonConvert.Deserialize<Dictionary<string, KnownIssueLongEnum>>(JsonConvert.Serialize(original));
            Assert.That(back["a"], Is.EqualTo(KnownIssueLongEnum.Big));
        }
    }

    /// <summary>
    /// 字典的 Guid 键和枚举键可以序列化，但反序列化时抛出 InvalidCastException。
    /// </summary>
    [Ignore("已知问题：字典的 Guid 键和枚举键无法反序列化（InvalidCastException）。")]
    public class DictionaryKeyKnownIssueTests
    {
        [Test]
        public void GuidKeysRoundTrip()
        {
            var key = Guid.Parse("11111111-2222-3333-4444-555555555555");
            var back = JsonConvert.Deserialize<Dictionary<Guid, int>>(JsonConvert.Serialize(new Dictionary<Guid, int> { [key] = 1 }));
            Assert.That(back[key], Is.EqualTo(1));
        }

        [Test]
        public void EnumKeysRoundTrip()
        {
            var back = JsonConvert.Deserialize<Dictionary<KnownIssueColor, int>>(JsonConvert.Serialize(new Dictionary<KnownIssueColor, int> { [KnownIssueColor.Green] = 1 }));
            Assert.That(back[KnownIssueColor.Green], Is.EqualTo(1));
        }

        [Test]
        public void EnumKeysAreAcceptedByNameAndByNumber()
        {
            Assert.That(KnownIssueSupport.De<Dictionary<KnownIssueColor, int>>("{'Green':1}")[KnownIssueColor.Green], Is.EqualTo(1));
            Assert.That(KnownIssueSupport.De<Dictionary<KnownIssueColor, int>>("{'1':1}")[KnownIssueColor.Green], Is.EqualTo(1));
        }
    }

    /// <summary>
    /// double / float 的 NaN 和无穷大被写成裸的 NaN / Infinity，这不是合法的 JSON（解析器自己也读不回来）。
    /// 合理的做法是抛出异常，或者写成带引号的字符串，只要输出始终是合法 JSON。
    /// </summary>
    [Ignore("已知问题：NaN / Infinity 被写成非法的裸 token。")]
    public class NonFiniteNumberKnownIssueTests
    {
        [TestCase(double.NaN)]
        [TestCase(double.PositiveInfinity)]
        [TestCase(double.NegativeInfinity)]
        public void DoubleOutputIsAlwaysValidJson(double value)
        {
            AssertValidOrRejected(value);
        }

        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        [TestCase(float.NegativeInfinity)]
        public void FloatOutputIsAlwaysValidJson(float value)
        {
            AssertValidOrRejected(value);
        }

        [Test]
        public void NonFiniteNumberInsideAnObjectKeepsTheDocumentValid()
        {
            AssertValidOrRejected(new Dictionary<string, double> { ["v"] = double.NaN });
        }

        private static void AssertValidOrRejected(object value)
        {
            string json;
            try
            {
                json = JsonConvert.Serialize(value);
            }
            catch (Exception)
            {
                return; // 拒绝序列化是可以接受的
            }
            IJsonObject tree;
            Assert.That(JsonConvert.TryDeserialize(json, out tree), Is.True, "输出不是合法 JSON：" + json);
        }
    }

    /// <summary>
    /// sbyte 既不能序列化（写成 {}）也不能反序列化。
    /// </summary>
    [Ignore("已知问题：不支持 sbyte（序列化为 {}，反序列化抛异常）。")]
    public class SByteKnownIssueTests
    {
        [TestCase(-5)]
        [TestCase(127)]
        [TestCase(-128)]
        public void SByteRoundTrips(int number)
        {
            sbyte value = (sbyte)number;
            Assert.That(JsonConvert.Serialize(value), Is.EqualTo(number.ToString()));
            Assert.That(JsonConvert.Deserialize<sbyte>(number.ToString()), Is.EqualTo(value));
        }
    }

    /// <summary>
    /// 序列化器可以把 HashSet / Queue / Stack / LinkedList 等写成数组，但反序列化只支持数组、List 及其接口。
    /// </summary>
    [Ignore("已知问题：HashSet / Queue / Stack / LinkedList / SortedSet / ISet / ReadOnlyCollection 无法反序列化。")]
    public class CollectionTypesKnownIssueTests
    {
        [Test]
        public void HashSetIsDeserialized()
        {
            Assert.That(KnownIssueSupport.De<HashSet<int>>("[1,2,2]"), Is.EquivalentTo(new[] { 1, 2 }));
        }

        [Test]
        public void ISetIsDeserialized()
        {
            Assert.That(KnownIssueSupport.De<ISet<int>>("[1,2]"), Is.EquivalentTo(new[] { 1, 2 }));
        }

        [Test]
        public void SortedSetIsDeserialized()
        {
            Assert.That(KnownIssueSupport.De<SortedSet<int>>("[2,1]"), Is.EqualTo(new[] { 1, 2 }));
        }

        [Test]
        public void QueueIsDeserialized()
        {
            Assert.That(KnownIssueSupport.De<Queue<int>>("[1,2]"), Is.EqualTo(new[] { 1, 2 }));
        }

        [Test]
        public void StackIsDeserialized()
        {
            Assert.That(KnownIssueSupport.De<Stack<int>>("[1,2]").Count, Is.EqualTo(2));
        }

        [Test]
        public void LinkedListIsDeserialized()
        {
            Assert.That(KnownIssueSupport.De<LinkedList<int>>("[1,2]"), Is.EqualTo(new[] { 1, 2 }));
        }

        [Test]
        public void ReadOnlyCollectionIsDeserialized()
        {
            Assert.That(KnownIssueSupport.De<ReadOnlyCollection<int>>("[1,2]"), Is.EqualTo(new[] { 1, 2 }));
        }
    }

    /// <summary>
    /// DateTimeOffset / TimeSpan 被当成普通对象序列化成一大堆属性（Ticks、TotalDays ...），读不回来。
    /// </summary>
    [Ignore("已知问题：不支持 DateTimeOffset / TimeSpan（被展开成属性对象，无法往返）。")]
    public class DateTimeOffsetKnownIssueTests
    {
        [Test]
        public void DateTimeOffsetRoundTrips()
        {
            var value = new DateTimeOffset(2024, 1, 2, 3, 4, 5, TimeSpan.FromHours(8));
            Assert.That(KnownIssueSupport.RoundTrip(value), Is.EqualTo(value));
        }

        [Test]
        public void DateTimeOffsetIsWrittenAsAString()
        {
            var json = JsonConvert.Serialize(new DateTimeOffset(2024, 1, 2, 3, 4, 5, TimeSpan.FromHours(8)));
            Assert.That(json, Does.StartWith("\"").And.EndWith("\""));
        }

        [Test]
        public void TimeSpanRoundTrips()
        {
            var value = TimeSpan.FromMinutes(90);
            Assert.That(KnownIssueSupport.RoundTrip(value), Is.EqualTo(value));
        }
    }
}
