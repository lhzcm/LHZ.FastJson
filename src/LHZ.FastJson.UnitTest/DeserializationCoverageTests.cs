using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using LHZ.FastJson.Exceptions;
using NUnit.Framework;

namespace LHZ.FastJson.UnitTest
{
    /// <summary>
    /// 反序列化覆盖测试：对象构造、未知属性、null 处理、字符串与数值边界、集合、字典、畸形输入。
    /// 为了避免大量转义，测试里的 JSON 用单引号书写，运行前统一替换为双引号（J 方法）。
    /// </summary>
    public class DeserializationCoverageTests
    {
        #region 模型

        public enum Color { Red = 0, Green = 1 }
        [Flags] public enum Perm { None = 0, Read = 1, Write = 2 }

        public class Item
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public bool Ok { get; set; }
            public double Score { get; set; }
            public Item Child { get; set; }
        }

        public class NullableModel
        {
            public int? Count { get; set; }
            public DateTime? When { get; set; }
            public Color? Color { get; set; }
            public Guid? Id { get; set; }
        }

        public class NoDefaultConstructor
        {
            public int V { get; set; }
            public NoDefaultConstructor(int v) { V = v; }
        }

        public class PrivateConstructor
        {
            public int V { get; set; }
            private PrivateConstructor() { }
        }

        public abstract class AbstractModel { public int V { get; set; } }
        public interface IModel { int V { get; set; } }

        public class GetOnlyModel { public int V { get; } = 1; public int W { get; set; } }
        public class PrivateSetterModel { public int V { get; private set; } }
        public class FieldModel { public int F; public int P { get; set; } }
        public class Box<T> { public T Value { get; set; } }
        public struct PointStruct { public int X { get; set; } public int Y { get; set; } }
        public class BaseModel { public int A { get; set; } }
        public class DerivedModel : BaseModel { public int B { get; set; } }
        public class AnyHolder { public object Any { get; set; } public int Num { get; set; } }

        public class ConcurrentModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public List<int> Numbers { get; set; }
        }

        #endregion

        /// <summary>
        /// 把单引号替换为双引号，便于书写 JSON。
        /// </summary>
        private static string J(string json)
        {
            return json.Replace('\'', '"');
        }

        private static T De<T>(string json)
        {
            return JsonConvert.Deserialize<T>(J(json));
        }

        private static void AssertRejected<T>(string json)
        {
            Assert.That(() => JsonConvert.Deserialize<T>(J(json)), Throws.InstanceOf<JsonReadException>(), json);
        }

        private static void AssertFails<T>(string json)
        {
            Assert.That(() => JsonConvert.Deserialize<T>(J(json)), Throws.Exception, json);
        }

        #region 对象与属性

        [Test]
        public void MissingPropertiesKeepDefaults()
        {
            var item = De<Item>("{'Name':'x'}");
            Assert.That(item.Id, Is.EqualTo(0));
            Assert.That(item.Name, Is.EqualTo("x"));
            Assert.That(item.Ok, Is.False);
            Assert.That(item.Score, Is.EqualTo(0.0));
            Assert.That(item.Child, Is.Null);
        }

        [Test]
        public void EmptyObjectCreatesDefaultInstance()
        {
            var item = De<Item>("{}");
            Assert.That(item, Is.Not.Null);
            Assert.That(item.Id, Is.EqualTo(0));
            Assert.That(item.Name, Is.Null);
        }

        [Test]
        public void NullRootGivesNullReference()
        {
            Assert.That(De<Item>("null"), Is.Null);
            Assert.That(De<int[]>("null"), Is.Null);
            Assert.That(De<List<Item>>("null"), Is.Null);
            Assert.That(De<Dictionary<string, int>>("null"), Is.Null);
            Assert.That(De<string>("null"), Is.Null);
        }

        [Test]
        public void DuplicateKeyLastValueWins()
        {
            Assert.That(De<Item>("{'Id':1,'Id':2}").Id, Is.EqualTo(2));
        }

        [Test]
        public void ScalarValuesOfEveryKindAreAssigned()
        {
            var item = De<Item>("{'Id':-5,'Name':'n','Ok':true,'Score':2.5}");
            Assert.That(item.Id, Is.EqualTo(-5));
            Assert.That(item.Name, Is.EqualTo("n"));
            Assert.That(item.Ok, Is.True);
            Assert.That(item.Score, Is.EqualTo(2.5));
        }

        /// <summary>
        /// 验证未知属性不论值是什么类型都会被跳过，并且不影响前后已知属性。
        /// </summary>
        [TestCase("'zz':'v'")]
        [TestCase("'zz':''")]
        [TestCase("'zz':12.5")]
        [TestCase("'zz':-1e5")]
        [TestCase("'zz':true")]
        [TestCase("'zz':false")]
        [TestCase("'zz':null")]
        [TestCase("'zz':{}")]
        [TestCase("'zz':[]")]
        [TestCase("'zz':{'a':[1,{'b':2}],'c':'d'}")]
        [TestCase("'zz':[1,'a',[3,[4]],{'k':null}]")]
        [TestCase("'zz':'a\\'}\\\\'")]
        [TestCase("'zz':']}{['")]
        public void UnknownPropertiesAreSkipped(string fragment)
        {
            var before = De<Item>("{" + fragment + ",'Id':1,'Name':'n'}");
            Assert.That(before.Id, Is.EqualTo(1));
            Assert.That(before.Name, Is.EqualTo("n"));

            var middle = De<Item>("{'Id':1," + fragment + ",'Name':'n'}");
            Assert.That(middle.Id, Is.EqualTo(1));
            Assert.That(middle.Name, Is.EqualTo("n"));

            var after = De<Item>("{'Id':1,'Name':'n'," + fragment + "}");
            Assert.That(after.Id, Is.EqualTo(1));
            Assert.That(after.Name, Is.EqualTo("n"));
        }

        [Test]
        public void UnknownPropertiesInsideNestedObjectsAreSkipped()
        {
            var item = De<Item>("{'Id':1,'Child':{'junk':[1,2,{'x':null}],'Id':2,'Child':{'Id':3,'extra':{'a':1}}}}");
            Assert.That(item.Id, Is.EqualTo(1));
            Assert.That(item.Child.Id, Is.EqualTo(2));
            Assert.That(item.Child.Child.Id, Is.EqualTo(3));
            Assert.That(item.Child.Child.Child, Is.Null);
        }

        [Test]
        public void NullIsAcceptedForReferenceAndNullableMembers()
        {
            var item = De<Item>("{'Name':null,'Child':null}");
            Assert.That(item.Name, Is.Null);
            Assert.That(item.Child, Is.Null);

            var nullable = De<NullableModel>("{'Count':null,'When':null,'Color':null,'Id':null}");
            Assert.That(nullable.Count, Is.Null);
            Assert.That(nullable.When, Is.Null);
            Assert.That(nullable.Color, Is.Null);
            Assert.That(nullable.Id, Is.Null);
        }

        [Test]
        public void NullableMembersAcceptValues()
        {
            var nullable = De<NullableModel>("{'Count':5,'When':'2024-01-02T03:04:05Z','Color':1,'Id':'11111111-2222-3333-4444-555555555555'}");
            Assert.That(nullable.Count, Is.EqualTo(5));
            Assert.That(nullable.When, Is.EqualTo(new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc)));
            Assert.That(nullable.Color, Is.EqualTo(Color.Green));
            Assert.That(nullable.Id, Is.EqualTo(Guid.Parse("11111111-2222-3333-4444-555555555555")));
        }

        [TestCase("{'Id':null}")]
        [TestCase("{'Ok':null}")]
        [TestCase("{'Score':null}")]
        public void NullForNonNullableValueTypeIsRejected(string json)
        {
            AssertRejected<Item>(json);
        }

        [Test]
        public void SetterRulesAreRespected()
        {
            var getOnly = De<GetOnlyModel>("{'V':5,'W':6}");
            Assert.That(getOnly.V, Is.EqualTo(1));
            Assert.That(getOnly.W, Is.EqualTo(6));

            Assert.That(De<PrivateSetterModel>("{'V':5}").V, Is.EqualTo(5));

            var fields = De<FieldModel>("{'F':5,'P':6}");
            Assert.That(fields.F, Is.EqualTo(0));
            Assert.That(fields.P, Is.EqualTo(6));
        }

        [Test]
        public void TypesThatCannotBeConstructedAreRejectedWithAClearException()
        {
            Assert.That(() => De<NoDefaultConstructor>("{'V':1}"), Throws.TypeOf<JsonDirectDeserializationException>());
            Assert.That(() => De<PrivateConstructor>("{'V':1}"), Throws.TypeOf<JsonDirectDeserializationException>());
            Assert.That(() => De<AbstractModel>("{'V':1}"), Throws.TypeOf<JsonDirectDeserializationException>());
            Assert.That(() => De<IModel>("{'V':1}"), Throws.TypeOf<JsonDirectDeserializationException>());
        }

        [Test]
        public void GenericStructAndInheritedTypes()
        {
            Assert.That(De<Box<int>>("{'Value':3}").Value, Is.EqualTo(3));
            Assert.That(De<Box<List<Item>>>("{'Value':[{'Id':1}]}").Value[0].Id, Is.EqualTo(1));
            Assert.That(De<Box<Box<string>>>("{'Value':{'Value':'s'}}").Value.Value, Is.EqualTo("s"));

            var point = De<PointStruct>("{'X':1,'Y':2}");
            Assert.That(point.X, Is.EqualTo(1));
            Assert.That(point.Y, Is.EqualTo(2));
            Assert.That(De<PointStruct?>("{'X':1}").Value.X, Is.EqualTo(1));
            Assert.That(De<PointStruct?>("null"), Is.Null);

            var derived = De<DerivedModel>("{'A':1,'B':2}");
            Assert.That(derived.A, Is.EqualTo(1));
            Assert.That(derived.B, Is.EqualTo(2));
            Assert.That(De<BaseModel>("{'A':1,'B':2}").A, Is.EqualTo(1));
        }

        [Test]
        public void ObjectTypedPropertyKeepsTheParsedJson()
        {
            var holder = De<AnyHolder>("{'Any':{'a':[1,2]},'Num':3}");
            Assert.That(holder.Num, Is.EqualTo(3));
            Assert.That(holder.Any, Is.InstanceOf<IJsonObject>());
            Assert.That(holder.Any.ToString(), Is.EqualTo("{\"a\":[1,2]}"));
        }

        [Test]
        public void ObjectRootGivesParsedNodes()
        {
            Assert.That(De<object>("5"), Is.InstanceOf<IJsonObject>());
            Assert.That(De<object>("[1,2]").ToString(), Is.EqualTo("[1,2]"));
            Assert.That(De<object>("null"), Is.Null);
        }

        #endregion

        #region 字符串

        [TestCase("''", "")]
        [TestCase("'abc'", "abc")]
        [TestCase("'a\\\\b'", "a\\b")]
        [TestCase("'a\\/b'", "a/b")]
        [TestCase("'\\b\\f\\n\\r\\t'", "\b\f\n\r\t")]
        [TestCase("'\\u4e2d\\u6587'", "中文")]
        [TestCase("'\\u0041\\u00e9'", "Aé")]
        [TestCase("'\\ud83d\\ude00'", "😀")]
        [TestCase("'中文😀é'", "中文😀é")]
        public void StringsAreUnescaped(string json, string expected)
        {
            Assert.That(De<string>(json), Is.EqualTo(expected));
        }

        [Test]
        public void EscapedQuoteInsideStringDoesNotEndTheString()
        {
            Assert.That(JsonConvert.Deserialize<string>("\"a\\\"b\""), Is.EqualTo("a\"b"));
            Assert.That(De<Item>("{'Name':'say \\\"hi\\\"'}").Name, Is.EqualTo("say \"hi\""));
        }

        [TestCase("'a\\qb'")]
        [TestCase("'\\u12G4'")]
        [TestCase("'\\u12'")]
        [TestCase("'abc")]
        [TestCase("'")]
        [TestCase("")]
        [TestCase("   ")]
        public void MalformedStringsAreRejected(string json)
        {
            AssertRejected<string>(json);
        }

        [Test]
        public void RawControlCharactersInsideStringsAreRejected()
        {
            AssertRejected<string>("'a\nb'");
            AssertRejected<string>("'a\tb'");
            AssertRejected<string>("'a\rb'");
        }

        [Test]
        public void SingleQuotedStringsAreNotJson()
        {
            Assert.That(() => JsonConvert.Deserialize<string>("'abc'"), Throws.InstanceOf<JsonReadException>());
        }

        [TestCase("5")]
        [TestCase("true")]
        [TestCase("[1]")]
        [TestCase("{}")]
        public void NonStringValueForStringIsRejected(string json)
        {
            AssertRejected<string>(json);
        }

        [TestCase("'a'", 'a')]
        [TestCase("'\\u0041'", 'A')]
        [TestCase("'\\n'", '\n')]
        [TestCase("'中'", '中')]
        public void CharIsReadFromSingleCharacterString(string json, char expected)
        {
            Assert.That(De<char>(json), Is.EqualTo(expected));
        }

        [TestCase("''")]
        [TestCase("'ab'")]
        [TestCase("1")]
        public void InvalidCharIsRejected(string json)
        {
            AssertRejected<char>(json);
        }

        #endregion

        #region 数值

        [Test]
        public void IntegerBoundariesAreAccepted()
        {
            Assert.That(De<int>("42"), Is.EqualTo(42));
            Assert.That(De<int>("-42"), Is.EqualTo(-42));
            Assert.That(De<int>("-2147483648"), Is.EqualTo(int.MinValue));
            Assert.That(De<int>("2147483647"), Is.EqualTo(int.MaxValue));
            Assert.That(De<long>("-9223372036854775808"), Is.EqualTo(long.MinValue));
            Assert.That(De<long>("9223372036854775807"), Is.EqualTo(long.MaxValue));
            Assert.That(De<ulong>("18446744073709551615"), Is.EqualTo(ulong.MaxValue));
            Assert.That(De<uint>("4294967295"), Is.EqualTo(uint.MaxValue));
            Assert.That(De<short>("-32768"), Is.EqualTo(short.MinValue));
            Assert.That(De<short>("32767"), Is.EqualTo(short.MaxValue));
            Assert.That(De<ushort>("65535"), Is.EqualTo(ushort.MaxValue));
            Assert.That(De<byte>("255"), Is.EqualTo(byte.MaxValue));
            Assert.That(De<byte>("0"), Is.EqualTo(0));
        }

        /// <summary>
        /// 验证超出目标类型范围的整数、以及带小数或指数的数字写进整数类型时会被拒绝，而不是静默截断。
        /// </summary>
        [TestCase(typeof(int), "2147483648")]
        [TestCase(typeof(int), "-2147483649")]
        [TestCase(typeof(int), "3000000000")]
        [TestCase(typeof(int), "1.0")]
        [TestCase(typeof(int), "1.5")]
        [TestCase(typeof(int), "1e2")]
        [TestCase(typeof(byte), "256")]
        [TestCase(typeof(byte), "-1")]
        [TestCase(typeof(short), "32768")]
        [TestCase(typeof(ushort), "-1")]
        [TestCase(typeof(uint), "-1")]
        [TestCase(typeof(long), "9223372036854775808")]
        [TestCase(typeof(ulong), "18446744073709551616")]
        [TestCase(typeof(decimal), "79228162514264337593543950336")]
        public void OutOfRangeNumbersAreRejected(Type target, string json)
        {
            var method = typeof(JsonConvert).GetMethods().First(m => m.Name == "Deserialize" && m.IsGenericMethodDefinition && m.GetParameters().Length == 1);
            var closed = method.MakeGenericMethod(target);
            Assert.That(() => closed.Invoke(null, new object[] { json }), Throws.Exception, target.Name + " <- " + json);
        }

        [Test]
        public void FloatingPointValuesKeepTheirPrecision()
        {
            Assert.That(De<double>("0.1"), Is.EqualTo(0.1));
            Assert.That(De<double>("1e308"), Is.EqualTo(1e308));
            Assert.That(De<double>("1E-5"), Is.EqualTo(1e-5));
            Assert.That(De<double>("1E+5"), Is.EqualTo(100000.0));
            Assert.That(De<double>("1.7976931348623157E+308"), Is.EqualTo(double.MaxValue));
            Assert.That(De<double>("0.12345678901234568"), Is.EqualTo(0.12345678901234568));
            Assert.That(De<double>("5E-324"), Is.EqualTo(double.Epsilon));
            Assert.That(double.IsNegative(De<double>("-0")), Is.True);
            Assert.That(De<float>("3.14"), Is.EqualTo(3.14f));
            Assert.That(De<float>("3.4028235E+38"), Is.EqualTo(float.MaxValue));
        }

        [Test]
        public void DecimalKeepsAllDigits()
        {
            Assert.That(De<decimal>("0.1"), Is.EqualTo(0.1m));
            Assert.That(De<decimal>("1.2345678901234567890123456789"), Is.EqualTo(1.2345678901234567890123456789m));
            Assert.That(De<decimal>("79228162514264337593543950335"), Is.EqualTo(decimal.MaxValue));
            Assert.That(De<decimal>("-79228162514264337593543950335"), Is.EqualTo(decimal.MinValue));
        }

        [Test]
        public void IntegerIsAcceptedWhereFloatingPointIsExpected()
        {
            Assert.That(De<double>("3"), Is.EqualTo(3.0));
            Assert.That(De<float>("3"), Is.EqualTo(3f));
            Assert.That(De<decimal>("3"), Is.EqualTo(3m));
            Assert.That(De<Item>("{'Score':3}").Score, Is.EqualTo(3.0));
        }

        /// <summary>
        /// 验证不符合 JSON 数字语法的写法被拒绝。
        /// </summary>
        [TestCase("01")]
        [TestCase("+1")]
        [TestCase(".5")]
        [TestCase("1.")]
        [TestCase("--1")]
        [TestCase("-")]
        [TestCase("1e")]
        [TestCase("1e+")]
        [TestCase("NaN")]
        [TestCase("Infinity")]
        [TestCase("-Infinity")]
        public void NumbersThatAreNotJsonAreRejected(string json)
        {
            AssertRejected<double>(json);
        }

        [TestCase("'5'")]
        [TestCase("true")]
        [TestCase("null")]
        [TestCase("[1]")]
        [TestCase("{}")]
        public void NonNumberValueForIntegerIsRejected(string json)
        {
            AssertRejected<int>(json);
        }

        [Test]
        public void NumbersAreIndependentOfCurrentCulture()
        {
            var original = CultureInfo.CurrentCulture;
            try
            {
                foreach (var name in new[] { "de-DE", "fr-FR", "tr-TR", "ar-SA" })
                {
                    CultureInfo.CurrentCulture = new CultureInfo(name);
                    Assert.That(De<double>("1.5"), Is.EqualTo(1.5), name);
                    Assert.That(De<decimal>("2.25"), Is.EqualTo(2.25m), name);
                    Assert.That(De<float>("0.5"), Is.EqualTo(0.5f), name);
                    Assert.That(De<int>("-1234567"), Is.EqualTo(-1234567), name);
                }
            }
            finally
            {
                CultureInfo.CurrentCulture = original;
            }
        }

        #endregion

        #region 布尔、Guid、枚举

        [Test]
        public void BooleansAreReadFromLiterals()
        {
            Assert.That(De<bool>("true"), Is.True);
            Assert.That(De<bool>("false"), Is.False);
        }

        [TestCase("1")]
        [TestCase("0")]
        [TestCase("'true'")]
        [TestCase("True")]
        [TestCase("TRUE")]
        [TestCase("tru")]
        [TestCase("null")]
        public void NonBooleanValueForBoolIsRejected(string json)
        {
            AssertRejected<bool>(json);
        }

        [TestCase("'11111111-2222-3333-4444-555555555555'")]
        [TestCase("'11111111222233334444555555555555'")]
        [TestCase("'{11111111-2222-3333-4444-555555555555}'")]
        [TestCase("'(11111111-2222-3333-4444-555555555555)'")]
        public void GuidAcceptsTheStandardFormats(string json)
        {
            Assert.That(De<Guid>(json), Is.EqualTo(Guid.Parse("11111111-2222-3333-4444-555555555555")));
        }

        [TestCase("'nope'")]
        [TestCase("''")]
        [TestCase("1")]
        [TestCase("null")]
        public void InvalidGuidIsRejected(string json)
        {
            AssertFails<Guid>(json);
        }

        [Test]
        public void EnumsAreReadFromNumbersAndNames()
        {
            Assert.That(De<Color>("0"), Is.EqualTo(Color.Red));
            Assert.That(De<Color>("1"), Is.EqualTo(Color.Green));
            Assert.That(De<Color>("'Green'"), Is.EqualTo(Color.Green));
            Assert.That(De<Color>("'green'"), Is.EqualTo(Color.Green));
            Assert.That(De<Color>("'1'"), Is.EqualTo(Color.Green));
            Assert.That(De<Color?>("1"), Is.EqualTo(Color.Green));
            Assert.That(De<Color?>("null"), Is.Null);
            Assert.That(De<List<Color>>("[0,1,'Green']"), Is.EqualTo(new List<Color> { Color.Red, Color.Green, Color.Green }));
            Assert.That(De<Perm>("1"), Is.EqualTo(Perm.Read));
            Assert.That(De<Perm>("0"), Is.EqualTo(Perm.None));
            Assert.That(De<Perm>("'Read, Write'"), Is.EqualTo(Perm.Read | Perm.Write));
        }

        [Test]
        public void UnknownEnumNameIsRejected()
        {
            AssertFails<Color>("'Purple'");
        }

        [Test]
        public void DateTimeValueMustBeAString()
        {
            Assert.That(De<DateTime>("'2024-01-02T03:04:05Z'"), Is.EqualTo(new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc)));
            AssertRejected<DateTime>("null");
            AssertRejected<DateTime>("5");
        }

        #endregion

        #region 数组与集合

        [Test]
        public void ArraysOfPrimitives()
        {
            Assert.That(De<int[]>("[1,2,3]"), Is.EqualTo(new[] { 1, 2, 3 }));
            Assert.That(De<int[]>("[]"), Is.Empty);
            Assert.That(De<string[]>("['a',null,'c']"), Is.EqualTo(new[] { "a", null, "c" }));
            Assert.That(De<int?[]>("[1,null,3]"), Is.EqualTo(new int?[] { 1, null, 3 }));
            Assert.That(De<bool[]>("[true,false]"), Is.EqualTo(new[] { true, false }));
            Assert.That(De<double[]>("[1,2.5,-3e2]"), Is.EqualTo(new[] { 1.0, 2.5, -300.0 }));
            Assert.That(De<byte[]>("[1,2,255]"), Is.EqualTo(new byte[] { 1, 2, 255 }));
            Assert.That(De<List<Guid>>("['11111111-2222-3333-4444-555555555555']"), Has.Count.EqualTo(1));
        }

        [Test]
        public void NestedArrays()
        {
            var jagged = De<int[][]>("[[1],[2,3],[]]");
            Assert.That(jagged.Length, Is.EqualTo(3));
            Assert.That(jagged[0], Is.EqualTo(new[] { 1 }));
            Assert.That(jagged[1], Is.EqualTo(new[] { 2, 3 }));
            Assert.That(jagged[2], Is.Empty);

            var lists = De<List<List<int>>>("[[1],[2,3]]");
            Assert.That(lists[1], Is.EqualTo(new List<int> { 2, 3 }));
            Assert.That(De<List<List<List<int>>>>("[[[1]],[[]]]")[0][0][0], Is.EqualTo(1));
        }

        [Test]
        public void ListOfObjectsAllowsNullElements()
        {
            var list = De<List<Item>>("[null,{'Id':2}]");
            Assert.That(list.Count, Is.EqualTo(2));
            Assert.That(list[0], Is.Null);
            Assert.That(list[1].Id, Is.EqualTo(2));

            var array = De<Item[]>("[{'Id':1,'Name':'a'}]");
            Assert.That(array[0].Name, Is.EqualTo("a"));
        }

        [Test]
        public void CollectionInterfacesAreMaterializedAsList()
        {
            Assert.That(De<IList<int>>("[1,2]"), Is.InstanceOf<List<int>>());
            Assert.That(De<ICollection<int>>("[1,2]"), Is.InstanceOf<List<int>>());
            Assert.That(De<IEnumerable<int>>("[1,2]"), Is.InstanceOf<List<int>>());
            Assert.That(De<IReadOnlyList<int>>("[1,2]"), Is.InstanceOf<List<int>>());
            Assert.That(De<Collection<int>>("[1,2]"), Is.EqualTo(new[] { 1, 2 }));
        }

        [Test]
        public void UntypedCollectionsHoldParsedNodes()
        {
            var array = De<object[]>("[1,'a',null]");
            Assert.That(array.Length, Is.EqualTo(3));
            Assert.That(array[0], Is.InstanceOf<IJsonObject>());
            Assert.That(array[2], Is.Null);

            var list = De<ArrayList>("[1,'a']");
            Assert.That(list.Count, Is.EqualTo(2));
        }

        [TestCase("[1,2,]")]
        [TestCase("[,1]")]
        [TestCase("[1,,2]")]
        [TestCase("[1,2")]
        [TestCase("[")]
        [TestCase("]")]
        public void MalformedArraysAreRejected(string json)
        {
            AssertRejected<int[]>(json);
        }

        [TestCase("{'a':1}")]
        [TestCase("['1']")]
        [TestCase("[null]")]
        [TestCase("[true]")]
        [TestCase("[[1]]")]
        [TestCase("5")]
        public void WrongShapesForIntegerArrayAreRejected(string json)
        {
            AssertRejected<int[]>(json);
        }

        #endregion

        #region 字典

        [Test]
        public void DictionariesWithStringKeys()
        {
            var map = De<Dictionary<string, int>>("{'a':1,'b':2}");
            Assert.That(map.Count, Is.EqualTo(2));
            Assert.That(map["a"], Is.EqualTo(1));
            Assert.That(map["b"], Is.EqualTo(2));
            Assert.That(De<Dictionary<string, int>>("{}"), Is.Empty);
            Assert.That(De<Dictionary<string, string>>("{'a':null}")["a"], Is.Null);
            Assert.That(De<Dictionary<string, int>>("{'':1}")[""], Is.EqualTo(1));
        }

        [Test]
        public void DictionariesWithConvertibleKeys()
        {
            Assert.That(De<Dictionary<int, string>>("{'1':'a','-2':'b'}")[-2], Is.EqualTo("b"));
            Assert.That(De<Dictionary<long, int>>("{'5000000000':1}")[5000000000L], Is.EqualTo(1));
            Assert.That(De<Dictionary<double, int>>("{'1.5':1}")[1.5], Is.EqualTo(1));
            Assert.That(De<Dictionary<bool, int>>("{'True':1}")[true], Is.EqualTo(1));
            Assert.That(De<Dictionary<char, int>>("{'c':1}")['c'], Is.EqualTo(1));
        }

        [Test]
        public void DictionaryValuesOfAllShapes()
        {
            Assert.That(De<Dictionary<string, List<int>>>("{'a':[1,2]}")["a"], Is.EqualTo(new List<int> { 1, 2 }));
            Assert.That(De<Dictionary<string, Item>>("{'a':{'Id':1}}")["a"].Id, Is.EqualTo(1));
            Assert.That(De<Dictionary<string, Dictionary<string, int>>>("{'a':{'b':1}}")["a"]["b"], Is.EqualTo(1));
            Assert.That(De<Dictionary<string, Item>>("{'a':null}")["a"], Is.Null);
            Assert.That(De<SortedDictionary<string, int>>("{'b':2,'a':1}").Keys, Is.EqualTo(new[] { "a", "b" }));
        }

        [Test]
        public void UntypedDictionaryValuesHoldParsedNodes()
        {
            var map = De<Dictionary<string, object>>("{'a':1,'b':'x','c':[1],'d':{'e':null}}");
            Assert.That(map.Count, Is.EqualTo(4));
            Assert.That(map["a"], Is.InstanceOf<IJsonObject>());
            Assert.That(map["c"].ToString(), Is.EqualTo("[1]"));
            Assert.That(map["d"].ToString(), Is.EqualTo("{\"e\":null}"));
        }

        [Test]
        public void DuplicateDictionaryKeyIsRejected()
        {
            AssertFails<Dictionary<string, int>>("{'a':1,'a':2}");
        }

        [Test]
        public void KeyThatCannotBeConvertedIsRejected()
        {
            AssertFails<Dictionary<int, string>>("{'x':'a'}");
        }

        [TestCase("{'a':null}")]
        [TestCase("{'a':'1'}")]
        [TestCase("[1]")]
        [TestCase("5")]
        public void WrongShapesForIntegerDictionaryAreRejected(string json)
        {
            AssertRejected<Dictionary<string, int>>(json);
        }

        #endregion

        #region 空白字符

        /// <summary>
        /// 验证值之前的各种空白（开括号后、冒号前后、逗号后、换行和制表符）都被接受。
        /// </summary>
        [TestCase("{ 'a':1}")]
        [TestCase("{'a' :1}")]
        [TestCase("{'a': 1}")]
        [TestCase("{'a':\t1}")]
        [TestCase("{\n'a':\r\n1}")]
        public void WhitespaceBeforeValuesIsAccepted(string json)
        {
            Assert.That(De<Dictionary<string, int>>(json)["a"], Is.EqualTo(1));
        }

        [TestCase("{'a':1, 'b':2}")]
        [TestCase("{'a':1,\n\t'b':2}")]
        [TestCase("{'a':1,\r\n    'b':2}")]
        public void WhitespaceAfterCommaIsAccepted(string json)
        {
            var map = De<Dictionary<string, int>>(json);
            Assert.That(map["a"], Is.EqualTo(1));
            Assert.That(map["b"], Is.EqualTo(2));
        }

        [TestCase("[ 1]")]
        [TestCase("[1, 2]")]
        [TestCase("[\n1,\n2]")]
        public void WhitespaceInsideArraysBeforeValuesIsAccepted(string json)
        {
            Assert.That(De<int[]>(json).Length, Is.GreaterThan(0));
        }

        /// <summary>
        /// 验证嵌套对象或数组的结束括号后面的空白被接受（标量值后面的空白见 WhitespaceHandlingTests）。
        /// </summary>
        [TestCase("{'Child':{} }")]
        [TestCase("{'Child':{}\n}")]
        [TestCase("{'Numbers':[1] }")]
        [TestCase("{'Numbers':[1]\r\n}")]
        [TestCase("{'Child':{} ,'Id':1}")]
        [TestCase("{'Numbers':[1] ,'Id':1}")]
        [TestCase("{'Child':{'Child':{} } }")]
        public void WhitespaceAfterNestedContainersIsAccepted(string json)
        {
            Assert.That(De<WhitespaceModel>(json), Is.Not.Null);
        }

        public class WhitespaceModel
        {
            public int Id { get; set; }
            public WhitespaceModel Child { get; set; }
            public List<int> Numbers { get; set; }
        }

        [Test]
        public void WhitespaceAroundTheRootValueIsAccepted()
        {
            Assert.That(De<Item>("   {'Id':1}  \n").Id, Is.EqualTo(1));
            Assert.That(De<int>(" \t42\r\n"), Is.EqualTo(42));
            Assert.That(De<string>("  'x'  "), Is.EqualTo("x"));
            Assert.That(De<int[]>("\n[1]\n"), Is.EqualTo(new[] { 1 }));
        }

        /// <summary>
        /// 验证无类型解析（JsonConvert.Deserialize(string)）接受各处的空白，并输出紧凑 JSON。
        /// </summary>
        [Test]
        public void UntypedParsingAcceptsWhitespaceEverywhere()
        {
            var tree = JsonConvert.Deserialize(J("{ 'a' : [ 1 , 2 ] ,\n  'b' : { } ,\t'c' : 'x' }"));
            Assert.That(tree.ToString(), Is.EqualTo("{\"a\":[1,2],\"b\":{},\"c\":\"x\"}"));
        }

        #endregion

        #region 畸形输入

        [TestCase("{'Id':1,}")]
        [TestCase("{'Id' 1}")]
        [TestCase("{Id:1}")]
        [TestCase("{'Id':1")]
        [TestCase("{")]
        [TestCase("}")]
        [TestCase("{1:2}")]
        [TestCase("{'Ok':tru}")]
        [TestCase("{'Name':nul}")]
        [TestCase("{'Ok':TRUE}")]
        [TestCase("/* c */{'Id':1}")]
        public void MalformedObjectsAreRejected(string json)
        {
            AssertRejected<Item>(json);
        }

        [Test]
        public void SingleQuotedKeysAreNotJson()
        {
            Assert.That(() => JsonConvert.Deserialize<Item>("{'Id':1}"), Throws.InstanceOf<JsonReadException>());
        }

        [TestCase("[1]")]
        [TestCase("'x'")]
        [TestCase("5")]
        [TestCase("true")]
        [TestCase("[")]
        public void NonObjectValueForObjectIsRejected(string json)
        {
            AssertRejected<Item>(json);
        }

        [Test]
        public void EmptyAndNullInputsAreRejected()
        {
            Assert.That(() => JsonConvert.Deserialize<Item>(null), Throws.InstanceOf<JsonReadException>());
            Assert.That(() => JsonConvert.Deserialize<Item>(""), Throws.InstanceOf<JsonReadException>());
            Assert.That(() => JsonConvert.Deserialize<Item>("   "), Throws.InstanceOf<JsonReadException>());
            Assert.That(() => JsonConvert.Deserialize(""), Throws.Exception);
        }

        [Test]
        public void TryDeserializeReportsFailureWithoutThrowing()
        {
            Item item;
            Assert.That(JsonConvert.TryDeserialize<Item>(null, out item), Is.False);
            Assert.That(item, Is.Null);
            Assert.That(JsonConvert.TryDeserialize<Item>("", out item), Is.False);
            Assert.That(JsonConvert.TryDeserialize<Item>(J("{'Id':"), out item), Is.False);
            Assert.That(JsonConvert.TryDeserialize<Item>(J("[1]"), out item), Is.False);
            Assert.That(JsonConvert.TryDeserialize<Item>(J("{'Id':'x'}"), out item), Is.False);

            Assert.That(JsonConvert.TryDeserialize<Item>(J("{'Id':7}"), out item), Is.True);
            Assert.That(item.Id, Is.EqualTo(7));

            IJsonObject tree;
            Assert.That(JsonConvert.TryDeserialize(null, out tree), Is.False);
            Assert.That(JsonConvert.TryDeserialize(J("{'a':[1,2]}"), out tree), Is.True);
            Assert.That(tree.ToString(), Is.EqualTo("{\"a\":[1,2]}"));
        }

        [Test]
        public void ErrorsReportTheCharacterPosition()
        {
            var ex = Assert.Throws<JsonReadException>(() => JsonConvert.Deserialize<Item>(J("{'Id':1,}")));
            Assert.That(ex.Position, Is.GreaterThan(0));
        }

        #endregion

        #region 并发

        /// <summary>
        /// 验证多线程首次反序列化同一批类型时，生成的委托缓存没有竞争问题。
        /// 这里使用本测试独有的泛型实例化，保证首次使用发生在并行阶段。
        /// </summary>
        [Test]
        public void ConcurrentFirstUseOfATypeIsConsistent()
        {
            string json = J("{'Value':{'Id':1,'Name':'n','Numbers':[1,2,3]}}");
            var failures = new ConcurrentBag<string>();
            Parallel.For(0, 400, _ =>
            {
                var box = JsonConvert.Deserialize<Box<ConcurrentModel>>(json);
                if (box.Value.Id != 1 || box.Value.Name != "n" || !box.Value.Numbers.SequenceEqual(new[] { 1, 2, 3 }))
                {
                    failures.Add("unexpected result");
                }
            });
            Assert.That(failures, Is.Empty);
        }

        #endregion
    }
}
