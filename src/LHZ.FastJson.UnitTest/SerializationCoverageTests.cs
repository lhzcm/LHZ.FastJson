using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using LHZ.FastJson.Json.Attributes;
using NUnit.Framework;

namespace LHZ.FastJson.UnitTest
{
    /// <summary>
    /// 序列化覆盖测试：字符串转义、数值格式、集合、字典、对象形状、枚举、循环引用和并发。
    /// 期望值来自对库实际输出的核对，并且都是合法的 JSON。
    /// </summary>
    public class SerializationCoverageTests
    {
        #region 模型

        public enum Color { Red = 0, Green = 1 }
        public enum ByteEnum : byte { A, B }
        public enum LongEnum : long { Small = 1, Big = 5000000000 }
        [Flags] public enum Perm { None = 0, Read = 1, Write = 2 }

        public class Point { public int X { get; set; } public int Y { get; set; } }
        public class PointPair { public Point A { get; set; } public Point B { get; set; } }
        public struct PointStruct { public int X { get; set; } public int Y { get; set; } }
        public class StructHolder { public PointStruct? P { get; set; } }

        public class BaseModel { public int A { get; set; } }
        public class DerivedModel : BaseModel { public int B { get; set; } }
        public class TypeHolder
        {
            public BaseModel Base { get; set; }
            public object Any { get; set; }
        }

        public class GetOnlyModel { public int V => 7; public string S { get; } = "s"; }
        public class PrivateSetterModel { public int V { get; private set; } = 9; }
        public class FieldModel { public int F = 5; public int P { get; set; } = 1; }
        public class VisibilityModel
        {
            public int Pub { get; set; } = 1;
            private int Hidden { get; set; } = 2;
            protected int Prot { get; set; } = 3;
            internal int Intern { get; set; } = 4;
            public int ReadHidden() { return Hidden + Prot + Intern; }
        }
        public interface IThing { int A { get; set; } }
        public class ExplicitModel : IThing
        {
            int IThing.A { get; set; } = 5;
            public int Own { get; set; } = 6;
        }
        public class Box<T> { public T Value { get; set; } }
        public class EnumHolder { public Color? C { get; set; } public ByteEnum B { get; set; } }

        public class CycleNode { public CycleNode Next { get; set; } }
        public class ListHolder { public List<object> Items { get; set; } }

        public class ConcurrentModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public List<string> Tags { get; set; }
            public Dictionary<string, int> Map { get; set; }
        }

        #endregion

        private static string Ser(object value)
        {
            return JsonConvert.Serialize(value);
        }

        #region 字符串与字符

        [TestCase("", "\"\"")]
        [TestCase("abc", "\"abc\"")]
        [TestCase("a\"b", "\"a\\\"b\"")]
        [TestCase("a\\b", "\"a\\\\b\"")]
        [TestCase("a/b", "\"a/b\"")]
        [TestCase("\b\f\n\r\t", "\"\\b\\f\\n\\r\\t\"")]
        [TestCase("\u0000\u0001\u001f", "\"\\u0000\\u0001\\u001f\"")]
        [TestCase("中文😀é", "\"中文😀é\"")]
        public void StringsAreEscaped(string value, string expected)
        {
            Assert.That(Ser(value), Is.EqualTo(expected));
        }

        [TestCase('a', "\"a\"")]
        [TestCase('"', "\"\\\"\"")]
        [TestCase('\\', "\"\\\\\"")]
        [TestCase('\n', "\"\\n\"")]
        [TestCase('\0', "\"\\u0000\"")]
        [TestCase('中', "\"中\"")]
        public void CharsAreEscapedLikeStrings(char value, string expected)
        {
            Assert.That(Ser(value), Is.EqualTo(expected));
        }

        /// <summary>
        /// 验证所有 0x00-0x1F 控制字符都被转义，输出里不会出现原始控制字符。
        /// </summary>
        [Test]
        public void EveryControlCharacterIsEscaped()
        {
            for (int c = 0; c < 0x20; c++)
            {
                string json = Ser(new string((char)c, 1));
                Assert.That(json.Any(ch => ch < 0x20), Is.False, "控制字符 U+" + c.ToString("X4") + " 未转义");
            }
        }

        [Test]
        public void NullStringPropertyIsWrittenAsNull()
        {
            Assert.That(Ser(new Box<string>()), Is.EqualTo("{\"Value\":null}"));
        }

        #endregion

        #region 数值

        [TestCase(0, "0")]
        [TestCase(int.MinValue, "-2147483648")]
        [TestCase(int.MaxValue, "2147483647")]
        [TestCase(long.MinValue, "-9223372036854775808")]
        [TestCase(long.MaxValue, "9223372036854775807")]
        [TestCase(ulong.MaxValue, "18446744073709551615")]
        [TestCase(uint.MaxValue, "4294967295")]
        [TestCase(short.MinValue, "-32768")]
        [TestCase(ushort.MaxValue, "65535")]
        [TestCase(byte.MaxValue, "255")]
        public void IntegersUseFullRange(object value, string expected)
        {
            Assert.That(Ser(value), Is.EqualTo(expected));
        }

        [TestCase(0.1, "0.1")]
        [TestCase(100.0, "100")]
        [TestCase(-1.5, "-1.5")]
        [TestCase(1e21, "1E+21")]
        [TestCase(double.MaxValue, "1.7976931348623157E+308")]
        [TestCase(double.MinValue, "-1.7976931348623157E+308")]
        [TestCase(double.Epsilon, "5E-324")]
        public void DoublesUseShortestRoundTripFormat(double value, string expected)
        {
            Assert.That(Ser(value), Is.EqualTo(expected));
        }

        [Test]
        public void FloatsAndDecimalsKeepTheirPrecision()
        {
            Assert.That(Ser(0.1f), Is.EqualTo("0.1"));
            Assert.That(Ser(float.MaxValue), Is.EqualTo("3.4028235E+38"));
            Assert.That(Ser(1.10m), Is.EqualTo("1.10"));
            Assert.That(Ser(decimal.MaxValue), Is.EqualTo("79228162514264337593543950335"));
            Assert.That(Ser(decimal.MinValue), Is.EqualTo("-79228162514264337593543950335"));
        }

        [Test]
        public void NegativeZeroIsWrittenAsMinusZero()
        {
            Assert.That(Ser(-0.0), Is.EqualTo("-0"));
        }

        /// <summary>
        /// 验证数值输出不受当前区域设置影响（小数点、负号、数字形态）。
        /// </summary>
        [TestCase("de-DE")]
        [TestCase("fr-FR")]
        [TestCase("tr-TR")]
        [TestCase("ar-SA")]
        public void NumbersAreIndependentOfCurrentCulture(string cultureName)
        {
            var original = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo(cultureName);
                Assert.That(Ser(1.5), Is.EqualTo("1.5"));
                Assert.That(Ser(2.25m), Is.EqualTo("2.25"));
                Assert.That(Ser(0.5f), Is.EqualTo("0.5"));
                Assert.That(Ser(-1234567), Is.EqualTo("-1234567"));
                Assert.That(Ser(5000000000L), Is.EqualTo("5000000000"));
            }
            finally
            {
                CultureInfo.CurrentCulture = original;
            }
        }

        [Test]
        public void BooleansAndNullRoots()
        {
            Assert.That(Ser(true), Is.EqualTo("true"));
            Assert.That(Ser(false), Is.EqualTo("false"));
            Assert.That(Ser(null), Is.EqualTo("null"));
        }

        [Test]
        public void DateTimeExtremesAreWrittenInIsoFormat()
        {
            Assert.That(Ser(DateTime.MinValue), Is.EqualTo("\"0001-01-01T00:00:00\""));
            Assert.That(Ser(DateTime.MaxValue), Is.EqualTo("\"9999-12-31T23:59:59.9999999\""));
        }

        [Test]
        public void GuidIsWrittenInHyphenatedLowerCaseForm()
        {
            Assert.That(Ser(Guid.Parse("11111111-2222-3333-4444-555555555555")), Is.EqualTo("\"11111111-2222-3333-4444-555555555555\""));
            Assert.That(Ser(Guid.Empty), Is.EqualTo("\"00000000-0000-0000-0000-000000000000\""));
        }

        #endregion

        #region 集合

        [Test]
        public void ArraysAndNestedArrays()
        {
            Assert.That(Ser(new[] { 1, 2, 3 }), Is.EqualTo("[1,2,3]"));
            Assert.That(Ser(new int[0]), Is.EqualTo("[]"));
            Assert.That(Ser(new[] { new[] { 1 }, new[] { 2, 3 }, new int[0] }), Is.EqualTo("[[1],[2,3],[]]"));
            Assert.That(Ser(new List<List<int>> { new List<int> { 1 }, new List<int>() }), Is.EqualTo("[[1],[]]"));
        }

        [Test]
        public void NullItemsAreWrittenAsNull()
        {
            Assert.That(Ser(new List<string> { null, "a" }), Is.EqualTo("[null,\"a\"]"));
            Assert.That(Ser(new int?[] { 1, null }), Is.EqualTo("[1,null]"));
            Assert.That(Ser(new List<Point> { null }), Is.EqualTo("[null]"));
        }

        [Test]
        public void NonListEnumerablesAreWrittenAsArrays()
        {
            Assert.That(Ser(new HashSet<int> { 1, 2 }), Is.EqualTo("[1,2]"));
            Assert.That(Ser(new Queue<int>(new[] { 1, 2 })), Is.EqualTo("[1,2]"));
            Assert.That(Ser(new Stack<int>(new[] { 1, 2 })), Is.EqualTo("[2,1]"));
            Assert.That(Ser(new LinkedList<int>(new[] { 1, 2 })), Is.EqualTo("[1,2]"));
            Assert.That(Ser(new[] { 1, 2 }.Select(x => x * 2)), Is.EqualTo("[2,4]"));
            Assert.That(Ser(YieldNumbers()), Is.EqualTo("[1,2,3]"));
        }

        private static IEnumerable<int> YieldNumbers()
        {
            yield return 1;
            yield return 2;
            yield return 3;
        }

        [Test]
        public void MixedUntypedCollections()
        {
            Assert.That(Ser(new ArrayList { 1, "a", null, 2.5 }), Is.EqualTo("[1,\"a\",null,2.5]"));
            var mixed = new List<object> { 1, "a", null, new[] { 1 }, new Dictionary<string, int> { ["k"] = 1 }, new Point { X = 1 } };
            Assert.That(Ser(mixed), Is.EqualTo("[1,\"a\",null,[1],{\"k\":1},{\"X\":1,\"Y\":0}]"));
        }

        [Test]
        public void ByteArrayIsWrittenAsNumberArray()
        {
            Assert.That(Ser(new byte[] { 1, 2, 3 }), Is.EqualTo("[1,2,3]"));
        }

        [Test]
        public void StringRootIsNotTreatedAsCharArray()
        {
            Assert.That(Ser("abc"), Is.EqualTo("\"abc\""));
        }

        [Test]
        public void LargeCollectionsAreSerializedCompletely()
        {
            var list = Enumerable.Range(0, 50000).ToList();
            string json = Ser(list);
            Assert.That(json, Does.StartWith("[0,1,2,"));
            Assert.That(json, Does.EndWith(",49998,49999]"));
            Assert.That(json.Count(c => c == ','), Is.EqualTo(49999));
        }

        #endregion

        #region 字典

        [Test]
        public void DictionaryKeysAreWrittenAsStrings()
        {
            Assert.That(Ser(new Dictionary<int, string> { [1] = "a" }), Is.EqualTo("{\"1\":\"a\"}"));
            Assert.That(Ser(new Dictionary<Guid, int> { [Guid.Empty] = 1 }), Is.EqualTo("{\"00000000-0000-0000-0000-000000000000\":1}"));
            Assert.That(Ser(new Dictionary<string, int> { [""] = 1 }), Is.EqualTo("{\"\":1}"));
        }

        [Test]
        public void DictionaryKeysAreEscaped()
        {
            Assert.That(Ser(new Dictionary<string, int> { ["a\"b\\c\n"] = 1 }), Is.EqualTo("{\"a\\\"b\\\\c\\n\":1}"));
        }

        [Test]
        public void DictionaryValuesOfAllShapes()
        {
            Assert.That(Ser(new Dictionary<string, string> { ["a"] = null }), Is.EqualTo("{\"a\":null}"));
            Assert.That(Ser(new Dictionary<string, Point> { ["p"] = new Point { X = 1, Y = 2 } }), Is.EqualTo("{\"p\":{\"X\":1,\"Y\":2}}"));
            Assert.That(Ser(new Dictionary<string, object> { ["i"] = 1, ["s"] = "x", ["n"] = null, ["l"] = new[] { 1 } }),
                Is.EqualTo("{\"i\":1,\"s\":\"x\",\"n\":null,\"l\":[1]}"));
            Assert.That(Ser(new Dictionary<string, int>()), Is.EqualTo("{}"));
        }

        [Test]
        public void OtherDictionaryTypes()
        {
            Assert.That(Ser(new SortedDictionary<string, int> { ["b"] = 2, ["a"] = 1 }), Is.EqualTo("{\"a\":1,\"b\":2}"));
            Assert.That(Ser(new Hashtable { ["k"] = 1 }), Is.EqualTo("{\"k\":1}"));
        }

        #endregion

        #region 对象形状

        [Test]
        public void GetOnlyAndPrivateSetterPropertiesAreWritten()
        {
            Assert.That(Ser(new GetOnlyModel()), Is.EqualTo("{\"V\":7,\"S\":\"s\"}"));
            Assert.That(Ser(new PrivateSetterModel()), Is.EqualTo("{\"V\":9}"));
        }

        [Test]
        public void FieldsAndNonPublicMembersAreNotWritten()
        {
            Assert.That(Ser(new FieldModel()), Is.EqualTo("{\"P\":1}"));
            var visibility = new VisibilityModel();
            Assert.That(Ser(visibility), Is.EqualTo("{\"Pub\":1}"));
            Assert.That(visibility.ReadHidden(), Is.EqualTo(9));
            Assert.That(Ser(new ExplicitModel()), Is.EqualTo("{\"Own\":6}"));
        }

        [Test]
        public void GenericAndAnonymousTypes()
        {
            Assert.That(Ser(new Box<int> { Value = 3 }), Is.EqualTo("{\"Value\":3}"));
            Assert.That(Ser(new Box<Box<string>> { Value = new Box<string> { Value = "s" } }), Is.EqualTo("{\"Value\":{\"Value\":\"s\"}}"));
            Assert.That(Ser(new Box<List<Point>> { Value = new List<Point> { new Point { X = 1 } } }), Is.EqualTo("{\"Value\":[{\"X\":1,\"Y\":0}]}"));
            Assert.That(Ser(new { A = 1, B = "x" }), Is.EqualTo("{\"A\":1,\"B\":\"x\"}"));
        }

        /// <summary>
        /// 验证属性声明为基类、接口或 object 时，序列化的是运行时类型，派生类的属性不会丢失。
        /// </summary>
        [Test]
        public void RuntimeTypeIsUsedForDerivedValues()
        {
            var derived = new DerivedModel { A = 1, B = 2 };
            string asBase = Ser(new TypeHolder { Base = derived });
            string asObject = Ser(new TypeHolder { Any = derived });
            foreach (var json in new[] { asBase, asObject, Ser((BaseModel)derived) })
            {
                Assert.That(json, Does.Contain("\"A\":1"));
                Assert.That(json, Does.Contain("\"B\":2"));
            }
        }

        [Test]
        public void StructsAndNullableStructs()
        {
            Assert.That(Ser(new PointStruct { X = 1, Y = 2 }), Is.EqualTo("{\"X\":1,\"Y\":2}"));
            Assert.That(Ser(new StructHolder()), Is.EqualTo("{\"P\":null}"));
            Assert.That(Ser(new StructHolder { P = new PointStruct { X = 3 } }), Is.EqualTo("{\"P\":{\"X\":3,\"Y\":0}}"));
        }

        [Test]
        public void EmptyObjectAndNullProperties()
        {
            Assert.That(Ser(new object()), Is.EqualTo("{}"));
            Assert.That(Ser(new PointPair()), Is.EqualTo("{\"A\":null,\"B\":null}"));
        }

        /// <summary>
        /// 验证同一对象被多处引用（不构成环）时可以正常序列化。
        /// </summary>
        [Test]
        public void SharedReferencesAreNotCycles()
        {
            var shared = new Point { X = 1 };
            Assert.That(Ser(new PointPair { A = shared, B = shared }), Is.EqualTo("{\"A\":{\"X\":1,\"Y\":0},\"B\":{\"X\":1,\"Y\":0}}"));
            Assert.That(Ser(new List<Point> { shared, shared }), Is.EqualTo("[{\"X\":1,\"Y\":0},{\"X\":1,\"Y\":0}]"));
            Assert.That(Ser(new Dictionary<string, Point> { ["a"] = shared, ["b"] = shared }), Is.EqualTo("{\"a\":{\"X\":1,\"Y\":0},\"b\":{\"X\":1,\"Y\":0}}"));
        }

        #endregion

        #region 枚举

        [Test]
        public void EnumsAreWrittenAsTheirUnderlyingNumbers()
        {
            Assert.That(Ser(Color.Green), Is.EqualTo("1"));
            Assert.That(Ser(ByteEnum.B), Is.EqualTo("1"));
            Assert.That(Ser(LongEnum.Big), Is.EqualTo("5000000000"));
            Assert.That(Ser(Perm.Read | Perm.Write), Is.EqualTo("3"));
            Assert.That(Ser(Perm.None), Is.EqualTo("0"));
            Assert.That(Ser((Color)99), Is.EqualTo("99"));
            Assert.That(Ser(new[] { Color.Red, Color.Green }), Is.EqualTo("[0,1]"));
        }

        [Test]
        public void EnumPropertiesAndNullableEnums()
        {
            Assert.That(Ser(new EnumHolder { C = Color.Green, B = ByteEnum.B }), Is.EqualTo("{\"C\":1,\"B\":1}"));
            Assert.That(Ser(new EnumHolder()), Is.EqualTo("{\"C\":null,\"B\":0}"));
        }

        #endregion

        #region 循环引用

        [Test]
        public void SelfReferencingObjectThrows()
        {
            var node = new CycleNode();
            node.Next = node;
            var ex = Assert.Catch<Exception>(() => Ser(node));
            Assert.That(ex.Message, Does.Contain("循环引用"));
        }

        [Test]
        public void IndirectCycleThrows()
        {
            var a = new CycleNode();
            var b = new CycleNode { Next = a };
            a.Next = b;
            Assert.Catch<Exception>(() => Ser(a));
        }

        [Test]
        public void CycleThroughDictionaryAndListThrows()
        {
            var dictionary = new Dictionary<string, object>();
            dictionary["self"] = dictionary;
            Assert.Catch<Exception>(() => Ser(dictionary));

            var holder = new ListHolder();
            holder.Items = new List<object> { holder };
            Assert.Catch<Exception>(() => Ser(holder));
        }

        /// <summary>
        /// 验证抛出循环引用异常后，同类型的后续序列化不受影响（缓存的委托没有被破坏）。
        /// </summary>
        [Test]
        public void SerializerStillWorksAfterCycleException()
        {
            var node = new CycleNode();
            node.Next = node;
            Assert.Catch<Exception>(() => Ser(node));
            Assert.That(Ser(new CycleNode()), Is.EqualTo("{\"Next\":null}"));
            Assert.That(Ser(new CycleNode { Next = new CycleNode() }), Is.EqualTo("{\"Next\":{\"Next\":null}}"));
        }

        #endregion

        #region 并发与幂等

        /// <summary>
        /// 验证多线程首次序列化同一批类型时，生成的委托缓存没有竞争问题。
        /// 这里使用本测试独有的泛型实例化，保证首次使用发生在并行阶段。
        /// </summary>
        [Test]
        public void ConcurrentFirstUseOfATypeIsConsistent()
        {
            const string expected = "{\"Value\":{\"Id\":1,\"Name\":\"n\",\"Tags\":[\"a\",\"b\"],\"Map\":{\"k\":1}}}";
            var results = new ConcurrentBag<string>();
            Parallel.For(0, 400, _ =>
            {
                var model = new Box<ConcurrentModel>
                {
                    Value = new ConcurrentModel
                    {
                        Id = 1,
                        Name = "n",
                        Tags = new List<string> { "a", "b" },
                        Map = new Dictionary<string, int> { ["k"] = 1 }
                    }
                };
                results.Add(Ser(model));
            });
            Assert.That(results.Count, Is.EqualTo(400));
            Assert.That(results.Where(r => r != expected), Is.Empty);
        }

        [Test]
        public void SerializingTwiceGivesTheSameResult()
        {
            var model = new ConcurrentModel { Id = 1, Name = "n", Tags = new List<string> { "a" }, Map = new Dictionary<string, int> { ["k"] = 1 } };
            Assert.That(Ser(model), Is.EqualTo(Ser(model)));
        }

        [Test]
        public void SeparateSerializerInstancesDoNotShareState()
        {
            var first = new LHZ.FastJson.Json.JsonSerializer(new[] { 1, 2 });
            var second = new LHZ.FastJson.Json.JsonSerializer(new[] { 3 });
            Assert.That(first.Serialize(), Is.EqualTo("[1,2]"));
            Assert.That(second.Serialize(), Is.EqualTo("[3]"));
        }

        #endregion
    }
}
