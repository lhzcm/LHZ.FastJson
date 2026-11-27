using System;
using System.Collections.Generic;
using System.Linq;
using LHZ.FastJson.Exceptions;
using NUnit.Framework;

namespace LHZ.FastJson.UnitTest
{
    public enum WsColor { Red = 0, Green = 1 }

    public class WsItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Ok { get; set; }
        public WsItem Child { get; set; }
        public List<int> Numbers { get; set; }
    }

    public class WsModel
    {
        public int Id { get; set; }
        public Guid G { get; set; }
        public WsColor E { get; set; }
        public char C { get; set; }
        public DateTime D { get; set; }
        public decimal M { get; set; }
        public long L { get; set; }
        public bool B { get; set; }
        public string S { get; set; }
        public int? N { get; set; }
        public double X { get; set; }
        public ulong U { get; set; }
        public float F { get; set; }
        public string Z { get; set; }
        public WsModel Child { get; set; }
        public List<int> Nums { get; set; }
        public Dictionary<string, int> Map { get; set; }
    }

    /// <summary>
    /// 空白字符处理：带类型的反序列化必须在任意值的前后接受 JSON 允许的空白（空格、制表符、换行、回车），
    /// 缩进格式的 JSON 才能读入；同时跳过空白不能让缺少逗号之类的畸形输入蒙混过关。
    /// 为了避免大量转义，测试里的 JSON 用单引号书写，运行前统一替换为双引号。
    /// </summary>
    public class WhitespaceHandlingTests
    {
        private const string GuidText = "11111111-2222-3333-4444-555555555555";

        private static T De<T>(string json)
        {
            return JsonConvert.Deserialize<T>(json.Replace('\'', '"'));
        }

        private static readonly (string Fragment, Func<WsModel, bool> Check)[] Scalars =
        {
            ("'G':'" + GuidText + "'", m => m.G == Guid.Parse(GuidText)),
            ("'E':1", m => m.E == WsColor.Green),
            ("'E':'Green'", m => m.E == WsColor.Green),
            ("'C':'x'", m => m.C == 'x'),
            ("'D':'2024-01-02T03:04:05Z'", m => m.D == new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc)),
            ("'D':'2024-01-02'", m => m.D.Day == 2),
            ("'M':1.25", m => m.M == 1.25m),
            ("'L':-5", m => m.L == -5),
            ("'B':true", m => m.B),
            ("'B':false", m => !m.B),
            ("'S':'text'", m => m.S == "text"),
            ("'S':null", m => m.S == null),
            ("'N':null", m => m.N == null),
            ("'N':7", m => m.N == 7),
            ("'X':1e5", m => m.X == 100000),
            ("'U':18446744073709551615", m => m.U == ulong.MaxValue),
            ("'F':0.5", m => m.F == 0.5f),
            ("'Z':'a\\nb'", m => m.Z == "a\nb"),
            ("'Child':null", m => m.Child == null),
            ("'Nums':null", m => m.Nums == null),
            ("'Map':null", m => m.Map == null),
        };

        private static readonly string[] UnknownProperties =
        {
            "'u':1", "'u':'s'", "'u':true", "'u':false", "'u':null", "'u':1.5e3", "'u':'a\\\"b'",
            "'u':{}", "'u':[]", "'u':[1 ,'a' , null ]", "'u':{'a' : 1 , 'b' : [ true ] }",
        };

        #region 各种空白出现在标量值后面

        [TestCase("{'a':1 ,'b':2}")]
        [TestCase("{'a':1\t,'b':2}")]
        [TestCase("{'a':1\n,'b':2}")]
        public void WhitespaceBeforeCommaInObjectIsAccepted(string json)
        {
            var map = De<Dictionary<string, int>>(json);
            Assert.That(map["a"], Is.EqualTo(1));
            Assert.That(map["b"], Is.EqualTo(2));
        }

        [TestCase("[1 ,2]")]
        [TestCase("[1\t,2]")]
        [TestCase("[1\r\n,2]")]
        public void WhitespaceBeforeCommaInArrayIsAccepted(string json)
        {
            Assert.That(De<int[]>(json), Is.EqualTo(new[] { 1, 2 }));
        }

        [TestCase("{'a':1 }")]
        [TestCase("{'a':1\n}")]
        [TestCase("{'a':1\r\n}")]
        public void WhitespaceBeforeClosingBraceIsAccepted(string json)
        {
            Assert.That(De<Dictionary<string, int>>(json)["a"], Is.EqualTo(1));
        }

        [TestCase("[1 ]")]
        [TestCase("[1\n]")]
        [TestCase("[ 1 ]")]
        public void WhitespaceBeforeClosingBracketIsAccepted(string json)
        {
            Assert.That(De<int[]>(json), Is.EqualTo(new[] { 1 }));
        }

        [TestCase("{'Name':'x' }")]
        [TestCase("{'Ok':true }")]
        [TestCase("{'Child':null }")]
        [TestCase("{'Id':1 ,'Name':'x'}")]
        [TestCase("{'Name':'x' ,'Id':1}")]
        [TestCase("{'Ok':true ,'Id':1}")]
        [TestCase("{'Child':{} }")]
        [TestCase("{'Child':{}\n}")]
        [TestCase("{'Numbers':[1] }")]
        [TestCase("{'Numbers':[1]\r\n}")]
        [TestCase("{'Child':{} ,'Id':1}")]
        [TestCase("{'Numbers':[1] ,'Id':1}")]
        [TestCase("{'Child':{'Child':{} } }")]
        public void WhitespaceAfterEveryKindOfValueIsAccepted(string json)
        {
            Assert.That(De<WsItem>(json), Is.Not.Null);
        }

        #endregion

        #region 缩进格式

        [Test]
        public void IndentedJsonIsAccepted()
        {
            string json = "{\n  \"Id\": 1,\n  \"Name\": \"n\",\n  \"Child\": {\n    \"Id\": 2\n  },\n  \"Numbers\": [\n    1,\n    2\n  ]\n}";
            var item = JsonConvert.Deserialize<WsItem>(json);
            Assert.That(item.Id, Is.EqualTo(1));
            Assert.That(item.Name, Is.EqualTo("n"));
            Assert.That(item.Child.Id, Is.EqualTo(2));
            Assert.That(item.Numbers, Is.EqualTo(new[] { 1, 2 }));
        }

        [Test]
        public void IndentedJsonWithCrLfAndTabsIsAccepted()
        {
            string json = "{\r\n\t\"Id\": 1,\r\n\t\"Numbers\": [\r\n\t\t1,\r\n\t\t2\r\n\t]\r\n}";
            var item = JsonConvert.Deserialize<WsItem>(json);
            Assert.That(item.Id, Is.EqualTo(1));
            Assert.That(item.Numbers, Is.EqualTo(new[] { 1, 2 }));
        }

        [Test]
        public void IndentedArrayOfObjectsIsAccepted()
        {
            string json = "[\n  {\n    \"Id\": 1\n  },\n  {\n    \"Id\": 2\n  }\n]";
            var list = JsonConvert.Deserialize<List<WsItem>>(json);
            Assert.That(list.Select(i => i.Id), Is.EqualTo(new[] { 1, 2 }));
        }

        #endregion

        #region 空白 × 值类型矩阵

        [TestCase(" ")]
        [TestCase("\t")]
        [TestCase("\n")]
        [TestCase("\r\n")]
        [TestCase("  \n\t ")]
        public void ScalarPropertiesFollowedByWhitespace(string ws)
        {
            foreach (var scalar in Scalars)
            {
                var before = De<WsModel>("{" + scalar.Fragment + ws + ",'Id':1" + ws + "}");
                Assert.That(before.Id, Is.EqualTo(1), scalar.Fragment);
                Assert.That(scalar.Check(before), Is.True, scalar.Fragment);

                var last = De<WsModel>("{'Id':1," + scalar.Fragment + ws + "}");
                Assert.That(last.Id, Is.EqualTo(1), scalar.Fragment);
                Assert.That(scalar.Check(last), Is.True, scalar.Fragment);
            }
        }

        [TestCase(" ")]
        [TestCase("\t")]
        [TestCase("\n")]
        [TestCase("\r\n")]
        [TestCase("  \n\t ")]
        public void UnknownPropertiesFollowedByWhitespaceAreSkipped(string ws)
        {
            foreach (var unknown in UnknownProperties)
            {
                Assert.That(De<WsModel>("{" + unknown + ws + ",'Id':1" + ws + "}").Id, Is.EqualTo(1), unknown);
                Assert.That(De<WsModel>("{'Id':1," + unknown + ws + "}").Id, Is.EqualTo(1), unknown);
            }
        }

        [TestCase(" ")]
        [TestCase("\t")]
        [TestCase("\n")]
        [TestCase("\r\n")]
        [TestCase("  \n\t ")]
        public void ArrayElementsFollowedByWhitespace(string ws)
        {
            Assert.That(De<bool[]>("[true" + ws + "," + ws + "false" + ws + "]"), Is.EqualTo(new[] { true, false }));
            Assert.That(De<string[]>("['a'" + ws + ",'b'" + ws + "]"), Is.EqualTo(new[] { "a", "b" }));
            Assert.That(De<string[]>("[null" + ws + ",'b'" + ws + "]"), Is.EqualTo(new string[] { null, "b" }));
            Assert.That(De<double[]>("[1.5" + ws + ",2" + ws + ",-3e2" + ws + "]"), Is.EqualTo(new[] { 1.5, 2, -300 }));
            Assert.That(De<decimal[]>("[1.10" + ws + ",2" + ws + "]"), Is.EqualTo(new[] { 1.10m, 2m }));
            Assert.That(De<char[]>("['a'" + ws + ",'b'" + ws + "]"), Is.EqualTo(new[] { 'a', 'b' }));
            Assert.That(De<int?[]>("[1" + ws + ",null" + ws + ",3" + ws + "]"), Is.EqualTo(new int?[] { 1, null, 3 }));
            Assert.That(De<WsColor[]>("[0" + ws + ",1" + ws + "]"), Is.EqualTo(new[] { WsColor.Red, WsColor.Green }));
            Assert.That(De<WsColor[]>("['Red'" + ws + ",'Green'" + ws + "]"), Is.EqualTo(new[] { WsColor.Red, WsColor.Green }));
            Assert.That(De<Guid[]>("['" + GuidText + "'" + ws + "]"), Is.EqualTo(new[] { Guid.Parse(GuidText) }));
            Assert.That(De<DateTime[]>("['2024-01-02T03:04:05Z'" + ws + ",'2024-01-02T03:04:06Z'" + ws + "]").Select(d => d.Second), Is.EqualTo(new[] { 5, 6 }));
            Assert.That(De<List<List<int>>>("[[1" + ws + ",2" + ws + "]" + ws + ",[3" + ws + "]" + ws + "]").Select(l => l.Count), Is.EqualTo(new[] { 2, 1 }));
        }

        [TestCase(" ")]
        [TestCase("\t")]
        [TestCase("\n")]
        [TestCase("\r\n")]
        [TestCase("  \n\t ")]
        public void DictionaryValuesFollowedByWhitespace(string ws)
        {
            Assert.That(De<Dictionary<string, int>>("{'a':1" + ws + ",'b':2" + ws + "}")["b"], Is.EqualTo(2));
            Assert.That(De<Dictionary<string, string>>("{'a':'x'" + ws + ",'b':null" + ws + "}")["b"], Is.Null);
            Assert.That(De<Dictionary<string, bool>>("{'a':true" + ws + ",'b':false" + ws + "}")["a"], Is.True);
            Assert.That(De<Dictionary<string, DateTime>>("{'a':'2024-01-02T03:04:05Z'" + ws + ",'b':'2024-01-02T03:04:06Z'" + ws + "}")["b"].Second, Is.EqualTo(6));
            Assert.That(De<Dictionary<string, Guid>>("{'a':'" + GuidText + "'" + ws + "}").Count, Is.EqualTo(1));
            Assert.That(De<Dictionary<string, WsColor>>("{'a':1" + ws + ",'b':'Red'" + ws + "}")["a"], Is.EqualTo(WsColor.Green));
            Assert.That(De<Dictionary<string, object>>("{'a':1" + ws + ",'b':'x'" + ws + ",'c':null" + ws + ",'d':true" + ws + "}").Count, Is.EqualTo(4));
            Assert.That(De<Dictionary<string, List<int>>>("{'a':[1" + ws + "]" + ws + ",'b':[]" + ws + "}")["a"], Is.EqualTo(new[] { 1 }));
        }

        [TestCase(" ")]
        [TestCase("\t")]
        [TestCase("\n")]
        [TestCase("\r\n")]
        [TestCase("  \n\t ")]
        public void RootValuesSurroundedByWhitespace(string ws)
        {
            Assert.That(De<int>(ws + "42" + ws), Is.EqualTo(42));
            Assert.That(De<string>(ws + "'x'" + ws), Is.EqualTo("x"));
            Assert.That(De<bool>(ws + "true" + ws), Is.True);
            Assert.That(De<double>(ws + "1.5e2" + ws), Is.EqualTo(150));
            Assert.That(De<string>(ws + "null" + ws), Is.Null);
            Assert.That(De<Guid>(ws + "'" + GuidText + "'" + ws), Is.EqualTo(Guid.Parse(GuidText)));
            Assert.That(De<DateTime>(ws + "'2024-01-02T03:04:05Z'" + ws).Year, Is.EqualTo(2024));
            Assert.That(De<WsColor>(ws + "1" + ws), Is.EqualTo(WsColor.Green));
            Assert.That(De<int[]>(ws + "[1]" + ws), Is.EqualTo(new[] { 1 }));
            Assert.That(De<WsModel>(ws + "{" + ws + "'Id'" + ws + ":" + ws + "1" + ws + "}" + ws).Id, Is.EqualTo(1));
        }

        #endregion

        #region 跳过空白不能放过畸形输入

        /// <summary>
        /// 验证值之间缺少逗号时，即使隔着空白也会被拒绝（空白后面紧跟的是下一个值，不是逗号）。
        /// </summary>
        [TestCase("[1 2]")]
        [TestCase("[1  2]")]
        [TestCase("[1\n2]")]
        [TestCase("[1 ,]")]
        [TestCase("[1 , ,2]")]
        [TestCase("[ , 1]")]
        [TestCase("[1 2 ,3]")]
        public void MissingOrExtraCommasBetweenNumbersAreRejected(string json)
        {
            Assert.That(() => De<int[]>(json), Throws.InstanceOf<JsonReadException>(), json);
        }

        [Test]
        public void MissingCommaBetweenOtherScalarsIsRejected()
        {
            Assert.That(() => De<bool[]>("[true false]"), Throws.InstanceOf<JsonReadException>());
            Assert.That(() => De<string[]>("['a' 'b']"), Throws.InstanceOf<JsonReadException>());
            Assert.That(() => De<int?[]>("[null null]"), Throws.InstanceOf<JsonReadException>());
        }

        [TestCase("{'Id':1 'Name':'x'}")]
        [TestCase("{'Id':1  'Name':'x'}")]
        [TestCase("{'Name':'x' 'Id':1}")]
        [TestCase("{'Ok':true 'Id':1}")]
        [TestCase("{'Name':null 'Id':1}")]
        [TestCase("{'Id':1 ,}")]
        [TestCase("{'Id':1 , ,'Name':'x'}")]
        [TestCase("{'Id' 1}")]
        [TestCase("{'Id' : }")]
        [TestCase("{'Id':1 :2}")]
        [TestCase("{'Id':1 x}")]
        public void MalformedObjectsWithWhitespaceAreRejected(string json)
        {
            Assert.That(() => De<WsModel>(json), Throws.InstanceOf<JsonReadException>(), json);
        }

        /// <summary>
        /// 验证以空白结尾的未完成输入仍然被拒绝（跳过空白不能把“已到末尾”误当成完整）。
        /// </summary>
        [TestCase("{'Id':1 ")]
        [TestCase("{'Id':1  ")]
        [TestCase("{'Id':'x' ")]
        [TestCase("{'Name':'x' ")]
        [TestCase("{'Ok':true ")]
        [TestCase("{'Name':null ")]
        [TestCase("{'Child':{} ")]
        [TestCase("{'Nums':[1] ")]
        [TestCase("{ ")]
        [TestCase("[ ")]
        public void IncompleteObjectsEndingInWhitespaceAreRejected(string json)
        {
            Assert.That(() => De<WsModel>(json), Throws.InstanceOf<JsonReadException>(), json);
        }

        [TestCase("[1 ")]
        [TestCase("[1  ")]
        [TestCase("[1\n")]
        [TestCase("['a' ")]
        [TestCase("[true ")]
        [TestCase("[null ")]
        public void IncompleteArraysEndingInWhitespaceAreRejected(string json)
        {
            Assert.That(() => De<object[]>(json), Throws.InstanceOf<JsonReadException>(), json);
            Assert.That(() => De<int?[]>(json), Throws.InstanceOf<JsonReadException>(), json);
        }

        #endregion

        #region DateTime 快速路径

        [Test]
        public void DateTimeStringsWithEscapesAreStillParsed()
        {
            Assert.That(De<DateTime>("'2024-01-02T03:04:05\\u005a'").Year, Is.EqualTo(2024));
            var array = De<DateTime[]>("['2024-01-02T03:04:05\\u005a' , '2024-01-02T03:04:06Z' ]");
            Assert.That(array.Select(d => d.Second), Is.EqualTo(new[] { 5, 6 }));
        }

        [TestCase("''")]
        [TestCase("'2024-01-02T03:04:05Z")]
        [TestCase("'not a date'")]
        [TestCase("'2024-01-02\n'")]
        [TestCase("'2024-13-45T00:00:00Z'")]
        public void InvalidDateTimeStringsAreRejected(string json)
        {
            Assert.That(() => De<DateTime>(json), Throws.InstanceOf<JsonReadException>(), json);
        }

        [Test]
        public void DateTimeFormatsAreAcceptedFollowedByMoreProperties()
        {
            var model = De<WsModel>("{'D':'2024-01-02T03:04:05+08:00' ,'Id':1 ,'S':'x' }");
            Assert.That(model.Id, Is.EqualTo(1));
            Assert.That(model.S, Is.EqualTo("x"));
            Assert.That(model.D.Year, Is.EqualTo(2024));
            Assert.That(De<DateTime>("'2024-01-02 03:04:05'").Hour, Is.EqualTo(3));
        }

        #endregion
    }

    /// <summary>
    /// 截断输入：一个完整 JSON 的任意真前缀都是不完整的，必须抛出 JsonReadException，
    /// 不能泄漏 IndexOutOfRangeException、InvalidOperationException 之类的内部异常，也不能被当成合法输入接受。
    /// </summary>
    public class TruncatedInputTests
    {
        private static void AssertEveryPrefixIsRejected<T>(string json)
        {
            json = json.Replace('\'', '"');
            for (int length = 0; length < json.Length; length++)
            {
                string prefix = json.Substring(0, length);
                Assert.That(() => JsonConvert.Deserialize<T>(prefix), Throws.InstanceOf<JsonReadException>(),
                    typeof(T).Name + " 前缀(" + length + ")：" + prefix.Replace("\n", "\\n").Replace("\r", "\\r"));
            }
        }

        private const string ModelJson =
            "{'Id':1,'G':'11111111-2222-3333-4444-555555555555','E':'Green','C':'x','D':'2024-01-02T03:04:05Z','M':1.25,'L':-5," +
            "'B':true,'S':null,'N':7,'X':1e5,'U':1,'F':0.5,'Z':'a\\nb\\u00e9\\\"','Child':{'Id':2,'Child':null},'Nums':[1,2,3],'Map':{'k':1,'jj':2}}";

        private const string PrettyModelJson =
            "{\n  'Id': 1,\n  'S': 'text',\n  'B': false,\n  'N': null,\n  'D': '2024-01-02T03:04:05Z',\n" +
            "  'Child': {\n    'Id': 2,\n    'Nums': [\n      1,\n      2\n    ]\n  },\n  'Map': {\n    'k': 1\n  }\n}";

        [Test]
        public void ObjectWithEveryKindOfValue()
        {
            AssertEveryPrefixIsRejected<WsModel>(ModelJson);
        }

        [Test]
        public void IndentedObject()
        {
            AssertEveryPrefixIsRejected<WsModel>(PrettyModelJson);
        }

        /// <summary>
        /// 键之后直接结束（{"a"）曾经抛出 IndexOutOfRangeException。
        /// </summary>
        [TestCase("{'a'")]
        [TestCase("{'a' ")]
        [TestCase("{'a'\n")]
        [TestCase("{'Id':1,'Name'")]
        [TestCase("{'Id':1,'Name' ")]
        public void PropertyNameAtTheEndOfInputIsRejected(string json)
        {
            Assert.That(() => De<WsModel>(json), Throws.InstanceOf<JsonReadException>());
            Assert.That(() => De<Dictionary<string, int>>(json), Throws.InstanceOf<JsonReadException>());
            Assert.That(() => De<Dictionary<string, object>>(json), Throws.InstanceOf<JsonReadException>());
        }

        private static T De<T>(string json)
        {
            return JsonConvert.Deserialize<T>(json.Replace('\'', '"'));
        }

        /// <summary>
        /// 转义序列写到一半（以反斜杠结尾、\u 后面不足 4 位十六进制）时也必须抛出 JsonReadException。
        /// </summary>
        [Test]
        public void StringsCutInsideAnEscapeSequence()
        {
            AssertEveryPrefixIsRejected<string>("'a\\\\b\\u00e9\\n\\\"c\\/\\t'");
            AssertEveryPrefixIsRejected<string[]>("['\\u00e9','\\\\']");
            AssertEveryPrefixIsRejected<Dictionary<string, string>>("{'k':'\\u00e9\\n'}");
            Assert.That(() => JsonConvert.Deserialize<string>("\"abc\\"), Throws.InstanceOf<JsonReadException>());
            Assert.That(() => JsonConvert.Deserialize<string>("\"\\u"), Throws.InstanceOf<JsonReadException>());
            Assert.That(() => JsonConvert.Deserialize<string>("\"\\u00"), Throws.InstanceOf<JsonReadException>());
        }

        [Test]
        public void Dictionaries()
        {
            AssertEveryPrefixIsRejected<Dictionary<string, int>>("{'a':1,'bb':22,'ccc':333}");
            AssertEveryPrefixIsRejected<Dictionary<string, string>>("{'a':'x','b':null,'c':'yy'}");
            AssertEveryPrefixIsRejected<Dictionary<string, List<int>>>("{'a':[1,2],'b':[]}");
            AssertEveryPrefixIsRejected<Dictionary<string, object>>("{'a':1,'b':'x','c':[1,{'d':null}]}");
        }

        [Test]
        public void Arrays()
        {
            AssertEveryPrefixIsRejected<int[]>("[1,22,333]");
            AssertEveryPrefixIsRejected<string[]>("['a',null,'ccc']");
            AssertEveryPrefixIsRejected<bool[]>("[true,false]");
            AssertEveryPrefixIsRejected<DateTime[]>("['2024-01-02T03:04:05Z','2024-01-02T03:04:06Z']");
            AssertEveryPrefixIsRejected<int[][]>("[[1],[2,3],[]]");
            AssertEveryPrefixIsRejected<List<WsModel>>("[{'Id':1},{'Id':2,'S':'x'}]");
            AssertEveryPrefixIsRejected<object[]>("[1,'a',null,[true],{'k':1}]");
        }
    }
}
