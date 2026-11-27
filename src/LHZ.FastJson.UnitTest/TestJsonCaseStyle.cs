using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LHZ.FastJson.Enum;
using LHZ.FastJson.Exceptions;
using LHZ.FastJson.Json.Attributes;
using NUnit.Framework;

namespace LHZ.FastJson.UnitTest
{
    /// <summary>
    /// 验证按调用传入的命名风格选项（JsonConvertOptions.CaseStyle）的序列化 / 反序列化行为。
    /// </summary>
    public class TestJsonCaseStyle
    {
        private static JsonConvertOptions Options(CaseStyle style)
        {
            return new JsonConvertOptions { CaseStyle = style };
        }

        private static readonly CaseStyle[] AllStyles =
        {
            CaseStyle.PascalCase, CaseStyle.CamelCase, CaseStyle.SnakeCase, CaseStyle.KebabCase
        };

        /// <summary>
        /// 各风格下的 JSON 文本（UserName / UserAge / Child.FirstName）。
        /// </summary>
        private static string ModelJson(CaseStyle style, string userName, int userAge, string childFirstName)
        {
            string user, age, child, first;
            switch (style)
            {
                case CaseStyle.CamelCase: user = "userName"; age = "userAge"; child = "child"; first = "firstName"; break;
                case CaseStyle.SnakeCase: user = "user_name"; age = "user_age"; child = "child"; first = "first_name"; break;
                case CaseStyle.KebabCase: user = "user-name"; age = "user-age"; child = "child"; first = "first-name"; break;
                default: user = "UserName"; age = "UserAge"; child = "Child"; first = "FirstName"; break;
            }
            return "{\"" + user + "\":\"" + userName + "\",\"" + age + "\":" + userAge + ",\"" + child + "\":{\"" + first + "\":\"" + childFirstName + "\"}}";
        }

        /// <summary>
        /// 验证 ConvertName 的转换规则。
        /// </summary>
        [TestCase("UserName", CaseStyle.PascalCase, "UserName")]
        [TestCase("userName", CaseStyle.PascalCase, "userName")]
        [TestCase("UserName", CaseStyle.CamelCase, "userName")]
        [TestCase("userName", CaseStyle.CamelCase, "userName")]
        [TestCase("Name", CaseStyle.CamelCase, "name")]
        [TestCase("URLValue", CaseStyle.CamelCase, "urlValue")]
        [TestCase("ID", CaseStyle.CamelCase, "id")]
        [TestCase("UserName", CaseStyle.SnakeCase, "user_name")]
        [TestCase("userName", CaseStyle.SnakeCase, "user_name")]
        [TestCase("URLValue", CaseStyle.SnakeCase, "url_value")]
        [TestCase("HTTPServer2Id", CaseStyle.SnakeCase, "http_server2_id")]
        [TestCase("ID", CaseStyle.SnakeCase, "id")]
        [TestCase("Address2", CaseStyle.SnakeCase, "address2")]
        [TestCase("Item2Name", CaseStyle.SnakeCase, "item2_name")]
        [TestCase("user_name", CaseStyle.SnakeCase, "user_name")]
        [TestCase("user-name", CaseStyle.SnakeCase, "user_name")]
        [TestCase("User__Name", CaseStyle.SnakeCase, "user_name")]
        [TestCase("Name", CaseStyle.SnakeCase, "name")]
        [TestCase("UserName", CaseStyle.KebabCase, "user-name")]
        [TestCase("URLValue", CaseStyle.KebabCase, "url-value")]
        [TestCase("user_name", CaseStyle.KebabCase, "user-name")]
        [TestCase("HTTPServer2Id", CaseStyle.KebabCase, "http-server2-id")]
        public void TestConvertName(string name, CaseStyle style, string expected)
        {
            ClassicAssert.AreEqual(expected, JsonConvertOptions.ConvertName(name, style));
        }

        /// <summary>
        /// 验证空串和 null 在所有风格下原样返回。
        /// </summary>
        [Test]
        public void TestConvertNameEmptyAndNull()
        {
            foreach (var style in AllStyles)
            {
                ClassicAssert.AreEqual("", JsonConvertOptions.ConvertName("", style));
                ClassicAssert.IsNull(JsonConvertOptions.ConvertName(null, style));
            }
        }

        /// <summary>
        /// 验证各风格下的序列化输出。
        /// </summary>
        [Test]
        public void TestSerializeAllStyles()
        {
            var obj = new CaseStyleTestModel { UserName = "test", UserAge = 10, Child = new CaseStyleChild { FirstName = "c" } };
            foreach (var style in AllStyles)
            {
                ClassicAssert.AreEqual(ModelJson(style, "test", 10, "c"), JsonConvert.Serialize(obj, Options(style)), style.ToString());
            }
        }

        /// <summary>
        /// 验证各风格下的反序列化能匹配对应风格的属性名。
        /// </summary>
        [Test]
        public void TestDeserializeAllStyles()
        {
            foreach (var style in AllStyles)
            {
                var obj = JsonConvert.Deserialize<CaseStyleTestModel>(ModelJson(style, "test", 10, "c"), Options(style));
                ClassicAssert.AreEqual("test", obj.UserName, style.ToString());
                ClassicAssert.AreEqual(10, obj.UserAge, style.ToString());
                ClassicAssert.IsNotNull(obj.Child, style.ToString());
                ClassicAssert.AreEqual("c", obj.Child.FirstName, style.ToString());
            }
        }

        /// <summary>
        /// 验证各风格下序列化与反序列化可以往返。
        /// </summary>
        [Test]
        public void TestRoundTripAllStyles()
        {
            foreach (var style in AllStyles)
            {
                var obj = new CaseStyleTestModel { UserName = "round", UserAge = 42, Child = new CaseStyleChild { FirstName = "trip" } };
                var json = JsonConvert.Serialize(obj, Options(style));
                var back = JsonConvert.Deserialize<CaseStyleTestModel>(json, Options(style));
                ClassicAssert.AreEqual("round", back.UserName, style.ToString());
                ClassicAssert.AreEqual(42, back.UserAge, style.ToString());
                ClassicAssert.AreEqual("trip", back.Child.FirstName, style.ToString());
            }
        }

        /// <summary>
        /// 验证嵌套的集合 / 字典 / Nullable 元素类型同样遵循选项，字典 key 不受影响。
        /// </summary>
        [Test]
        public void TestStyleAppliesToNestedContainers()
        {
            var json = "{\"item_list\":[{\"first_name\":\"a\"}],\"item_map\":{\"UserKey\":{\"first_name\":\"b\"}},\"single_item\":{\"first_name\":\"c\"}}";
            var options = Options(CaseStyle.SnakeCase);
            var obj = JsonConvert.Deserialize<CaseStyleContainerModel>(json, options);
            ClassicAssert.AreEqual("a", obj.ItemList[0].FirstName);
            ClassicAssert.AreEqual("b", obj.ItemMap["UserKey"].FirstName);
            ClassicAssert.AreEqual("c", obj.SingleItem.Value.FirstName);

            ClassicAssert.AreEqual(json, JsonConvert.Serialize(obj, options));
        }

        /// <summary>
        /// 验证 JsonProperty 特性在每种风格下仍然优先。
        /// </summary>
        [Test]
        public void TestJsonPropertyAttributeOverridesStyle()
        {
            foreach (var style in AllStyles)
            {
                var options = Options(style);
                var obj = new CaseStyleAttributeModel { Name = "test", Age = 10 };

                string ageName = JsonConvertOptions.ConvertName("Age", style);
                var json = JsonConvert.Serialize(obj, options);
                ClassicAssert.AreEqual("{\"studentName\":\"test\",\"" + ageName + "\":10}", json, style.ToString());

                var back = JsonConvert.Deserialize<CaseStyleAttributeModel>("{\"studentName\":\"t2\",\"" + ageName + "\":20}", options);
                ClassicAssert.AreEqual("t2", back.Name, style.ToString());
                ClassicAssert.AreEqual(20, back.Age, style.ToString());
            }
        }

        /// <summary>
        /// 验证未传选项（null / 默认选项）时保持声明名原样（PascalCase），包括以小写开头的属性。
        /// </summary>
        [Test]
        public void TestDefaultKeepsDeclaredNames()
        {
            ClassicAssert.AreEqual(CaseStyle.PascalCase, new JsonConvertOptions().CaseStyle);

            var obj = new CaseStyleTestModel { UserName = "test", UserAge = 10, Child = null };
            const string expected = "{\"UserName\":\"test\",\"UserAge\":10,\"Child\":null}";

            ClassicAssert.AreEqual(expected, JsonConvert.Serialize(obj));
            ClassicAssert.AreEqual(expected, JsonConvert.Serialize(obj, (JsonConvertOptions)null));
            ClassicAssert.AreEqual(expected, JsonConvert.Serialize(obj, new JsonConvertOptions()));

            var back = JsonConvert.Deserialize<CaseStyleTestModel>("{\"UserName\":\"t2\",\"UserAge\":20,\"Child\":null}");
            ClassicAssert.AreEqual("t2", back.UserName);
            ClassicAssert.AreEqual(20, back.UserAge);

            ClassicAssert.AreEqual("{\"lowerName\":\"x\"}", JsonConvert.Serialize(new LowerNamedModel { lowerName = "x" }));
        }

        /// <summary>
        /// 验证同一类型在四种命名风格之间切换时，各自的编译缓存互不干扰。
        /// </summary>
        [Test]
        public void TestOptionsDoNotLeakBetweenCalls()
        {
            var obj = new CaseStyleTestModel { UserName = "a", UserAge = 1, Child = null };

            for (int i = 0; i < 3; i++)
            {
                foreach (var style in AllStyles)
                {
                    var json = JsonConvert.Serialize(obj, Options(style));
                    var expectedUser = JsonConvertOptions.ConvertName("UserName", style);
                    StringAssert.Contains("\"" + expectedUser + "\":\"a\"", json);
                    ClassicAssert.AreEqual("a", JsonConvert.Deserialize<CaseStyleTestModel>(json, Options(style)).UserName, style.ToString());
                }
            }

            //一种风格的输入在另一种风格下不会匹配（属性保持默认值）
            var snake = JsonConvert.Serialize(obj, Options(CaseStyle.SnakeCase));
            ClassicAssert.IsNull(JsonConvert.Deserialize<CaseStyleTestModel>(snake, Options(CaseStyle.KebabCase)).UserName);
            ClassicAssert.IsNull(JsonConvert.Deserialize<CaseStyleTestModel>(snake).UserName);
        }

        /// <summary>
        /// 验证并发交错使用四种风格时结果互不影响。
        /// </summary>
        [Test]
        public void TestConcurrentCallsWithDifferentStyles()
        {
            var obj = new CaseStyleTestModel { UserName = "a", UserAge = 1, Child = null };

            Parallel.For(0, 400, i =>
            {
                var style = AllStyles[i % AllStyles.Length];
                var json = JsonConvert.Serialize(obj, Options(style));
                StringAssert.Contains("\"" + JsonConvertOptions.ConvertName("UserName", style) + "\":\"a\"", json);
                ClassicAssert.AreEqual("a", JsonConvert.Deserialize<CaseStyleTestModel>(json, Options(style)).UserName);
            });
        }

        /// <summary>
        /// 验证非法枚举值会被拒绝（取值用作缓存下标，不能越界）。
        /// </summary>
        [Test]
        public void TestUndefinedCaseStyleIsRejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new JsonConvertOptions { CaseStyle = (CaseStyle)99 });
            Assert.Throws<ArgumentOutOfRangeException>(() => new JsonConvertOptions { CaseStyle = (CaseStyle)(-1) });
            Assert.Throws<ArgumentOutOfRangeException>(() => JsonConvertOptions.ConvertName("Name", (CaseStyle)99));
        }

        /// <summary>
        /// 验证转换后出现重名属性时序列化抛出异常。
        /// </summary>
        [Test]
        public void TestDuplicateNameAfterConversionThrows()
        {
            var obj = new DuplicateAfterSnakeModel { UserName = "a", userName = "b" };
            Assert.DoesNotThrow(() => JsonConvert.Serialize(obj));
            Assert.Throws<JsonSerializationException>(() => JsonConvert.Serialize(obj, Options(CaseStyle.SnakeCase)));
        }

        public class CaseStyleTestModel
        {
            public string UserName { get; set; }
            public int UserAge { get; set; }
            public CaseStyleChild Child { get; set; }
        }

        public class CaseStyleChild
        {
            public string FirstName { get; set; }
        }

        public class CaseStyleContainerModel
        {
            public List<CaseStyleChild> ItemList { get; set; }
            public Dictionary<string, CaseStyleChild> ItemMap { get; set; }
            public CaseStyleStruct? SingleItem { get; set; }
        }

        public struct CaseStyleStruct
        {
            public string FirstName { get; set; }
        }

        public class CaseStyleAttributeModel
        {
            [JsonProperty("studentName")]
            public string Name { get; set; }
            public int Age { get; set; }
        }

        public class LowerNamedModel
        {
            public string lowerName { get; set; }
        }

        public class DuplicateAfterSnakeModel
        {
            public string UserName { get; set; }
            public string userName { get; set; }
        }
    }
}
