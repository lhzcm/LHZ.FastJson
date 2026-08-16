using System;
using LHZ.FastJson.Json.Attributes;
using NUnit.Framework;

namespace LHZ.FastJson.UnitTest
{
    /// <summary>
    /// 验证全局 Camel-Case 配置（JsonConvertConfig.UseCamelCase）的序列化 / 反序列化行为。
    /// </summary>
    public class TestJsonCamelCaseConfig
    {
        /// <summary>
        /// 验证 ToCamelCase 的转换规则。
        /// </summary>
        [Test]
        public void TestToCamelCase()
        {
            Assert.AreEqual("userName", JsonConvertConfig.ToCamelCase("UserName"));
            Assert.AreEqual("userName", JsonConvertConfig.ToCamelCase("userName"));
            Assert.AreEqual("name", JsonConvertConfig.ToCamelCase("Name"));
            Assert.AreEqual("", JsonConvertConfig.ToCamelCase(""));
            Assert.AreEqual(null, JsonConvertConfig.ToCamelCase(null));
            Assert.AreEqual("urlValue", JsonConvertConfig.ToCamelCase("URLValue"));
        }

        /// <summary>
        /// 验证开启 Camel-Case 配置后，序列化输出 camelCase 属性名。
        /// </summary>
        [Test]
        public void TestCamelCaseSerialize()
        {
            JsonConvertConfig.UseCamelCase = true;
            try
            {
                var obj = new CamelCaseTestModel
                {
                    UserName = "test",
                    UserAge = 10,
                    Child = new CamelCaseChild { FirstName = "c" }
                };
                var json = JsonConvert.Serialize(obj);
                Assert.AreEqual("{\"userName\":\"test\",\"userAge\":10,\"child\":{\"firstName\":\"c\"}}", json);
            }
            finally
            {
                JsonConvertConfig.UseCamelCase = false;
            }
        }

        /// <summary>
        /// 验证开启 Camel-Case 配置后，反序列化能匹配 camelCase 属性名。
        /// </summary>
        [Test]
        public void TestCamelCaseDeserialize()
        {
            JsonConvertConfig.UseCamelCase = true;
            try
            {
                var obj = JsonConvert.Deserialize<CamelCaseTestModel>("{\"userName\":\"test\",\"userAge\":10,\"child\":{\"firstName\":\"c\"}}");
                Assert.AreEqual("test", obj.UserName);
                Assert.AreEqual(10, obj.UserAge);
                Assert.IsNotNull(obj.Child);
                Assert.AreEqual("c", obj.Child.FirstName);
            }
            finally
            {
                JsonConvertConfig.UseCamelCase = false;
            }
        }

        /// <summary>
        /// 验证 Camel-Case 配置下序列化与反序列化可以往返。
        /// </summary>
        [Test]
        public void TestCamelCaseRoundTrip()
        {
            JsonConvertConfig.UseCamelCase = true;
            try
            {
                var obj = new CamelCaseTestModel
                {
                    UserName = "round",
                    UserAge = 42,
                    Child = new CamelCaseChild { FirstName = "trip" }
                };
                var json = JsonConvert.Serialize(obj);
                var back = JsonConvert.Deserialize<CamelCaseTestModel>(json);
                Assert.AreEqual("round", back.UserName);
                Assert.AreEqual(42, back.UserAge);
                Assert.AreEqual("trip", back.Child.FirstName);
            }
            finally
            {
                JsonConvertConfig.UseCamelCase = false;
            }
        }

        /// <summary>
        /// 验证 JsonProperty 特性在 Camel-Case 配置下仍然优先。
        /// </summary>
        [Test]
        public void TestJsonPropertyAttributeOverridesCamelCase()
        {
            JsonConvertConfig.UseCamelCase = true;
            try
            {
                var obj = new CamelCaseAttributeModel { Name = "test", Age = 10 };

                var json = JsonConvert.Serialize(obj);
                Assert.AreEqual("{\"studentName\":\"test\",\"age\":10}", json);

                var back = JsonConvert.Deserialize<CamelCaseAttributeModel>("{\"studentName\":\"t2\",\"age\":20}");
                Assert.AreEqual("t2", back.Name);
                Assert.AreEqual(20, back.Age);
            }
            finally
            {
                JsonConvertConfig.UseCamelCase = false;
            }
        }

        /// <summary>
        /// 验证默认配置（未开启 Camel-Case）时输出 PascalCase 属性名。
        /// </summary>
        [Test]
        public void TestDefaultPascalCaseWhenConfigOff()
        {
            JsonConvertConfig.UseCamelCase = false;
            var obj = new CamelCaseTestModel { UserName = "test", UserAge = 10, Child = null };
            var json = JsonConvert.Serialize(obj);
            Assert.AreEqual("{\"UserName\":\"test\",\"UserAge\":10,\"Child\":null}", json);

            var back = JsonConvert.Deserialize<CamelCaseTestModel>("{\"UserName\":\"t2\",\"UserAge\":20,\"Child\":null}");
            Assert.AreEqual("t2", back.UserName);
            Assert.AreEqual(20, back.UserAge);
        }

        /// <summary>
        /// 验证切换配置后，已编译的表达式缓存会失效并重新编译。
        /// </summary>
        [Test]
        public void TestConfigSwitchInvalidatesCompiledCache()
        {
            //先用默认配置编译一次缓存
            var pascal = JsonConvert.Serialize(new CamelCaseTestModel { UserName = "a", UserAge = 1, Child = null });
            Assert.AreEqual("{\"UserName\":\"a\",\"UserAge\":1,\"Child\":null}", pascal);

            //切换为 Camel-Case，缓存应失效并输出 camelCase
            JsonConvertConfig.UseCamelCase = true;
            try
            {
                var camel = JsonConvert.Serialize(new CamelCaseTestModel { UserName = "b", UserAge = 2, Child = null });
                Assert.AreEqual("{\"userName\":\"b\",\"userAge\":2,\"child\":null}", camel);

                //反序列化缓存同样失效
                var back = JsonConvert.Deserialize<CamelCaseTestModel>("{\"userName\":\"c\",\"userAge\":3,\"child\":null}");
                Assert.AreEqual("c", back.UserName);
                Assert.AreEqual(3, back.UserAge);
            }
            finally
            {
                JsonConvertConfig.UseCamelCase = false;
            }

            //关闭后恢复 PascalCase
            var pascalAgain = JsonConvert.Serialize(new CamelCaseTestModel { UserName = "d", UserAge = 4, Child = null });
            Assert.AreEqual("{\"UserName\":\"d\",\"UserAge\":4,\"Child\":null}", pascalAgain);
        }

        public class CamelCaseTestModel
        {
            public string UserName { get; set; }
            public int UserAge { get; set; }
            public CamelCaseChild Child { get; set; }
        }

        public class CamelCaseChild
        {
            public string FirstName { get; set; }
        }

        public class CamelCaseAttributeModel
        {
            [JsonProperty("studentName")]
            public string Name { get; set; }
            public int Age { get; set; }
        }
    }
}
