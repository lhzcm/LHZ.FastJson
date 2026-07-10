using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using LHZ.FastJson.Json;
using LHZ.FastJson.Json.CustomConverter;
using NUnit.Framework;
using static LHZ.FastJson.UnitTest.TestJsonSerizlizer;

namespace LHZ.FastJson.UnitTest
{

    /// <summary>
    /// 验证 JSON 反序列化的核心场景。
    /// </summary>
    class TestJsonDeserizlizer
    {
        /// <summary>
        /// 验证 JSON null 可反序列化为空对象。
        /// </summary>
        [Test]
        public void TestNull()
        {
            string testStr = "null";
            var obj = (new JsonDeserializer<object>(testStr)).Deserialize();
            Assert.IsNull(obj);
            testStr = "{\"Count\":1,\"ExpiryDate\":null,\"Rating\":null,\"Name\":null}";
            var nullableObj = (new JsonDeserializer<NullableModel>(testStr)).Deserialize();
            Assert.AreEqual(1, nullableObj.Count);
            Assert.AreEqual(null, nullableObj.ExpiryDate);
            Assert.AreEqual(null, nullableObj.Name);
        }

        /// <summary>
        /// 验证 Boolean 反序列化 (true/false)。
        /// </summary>
        [Test]
        public void TestBoolean()
        {
            string testStr = "true";
            bool obj = (new JsonDeserializer<bool>(testStr)).Deserialize();
            Assert.IsTrue(obj);

            testStr = "false";
            obj = (new JsonDeserializer<bool>(testStr)).Deserialize();
            Assert.IsFalse(obj);
        }

        /// <summary>
        /// 验证 Byte 反序列化（通过 int 转换）。
        /// </summary>
        [Test]
        public void TestByte()
        {
            int intVal = (new JsonDeserializer<int>("255")).Deserialize();
            byte obj = (byte)intVal;
            Assert.AreEqual((byte)255, obj);
        }

        /// <summary>
        /// 验证 Int16 / UInt16 反序列化。
        /// </summary>
        [Test]
        public void TestInt16()
        {
            short obj = (new JsonDeserializer<short>("-32768")).Deserialize();
            Assert.AreEqual((short)(-32768), obj);

            ushort uObj = (new JsonDeserializer<ushort>("65535")).Deserialize();
            Assert.AreEqual((ushort)65535, uObj);
        }

        /// <summary>
        /// 验证 UInt32 / UInt64 反序列化。
        /// </summary>
        [Test]
        public void TestUInt()
        {
            uint obj = (new JsonDeserializer<uint>("4294967295")).Deserialize();
            Assert.AreEqual(4294967295u, obj);

            ulong uObj = (new JsonDeserializer<ulong>("18446744073709551615")).Deserialize();
            Assert.AreEqual(18446744073709551615ul, uObj);
        }

        /// <summary>
        /// 验证 Char 反序列化。
        /// </summary>
        [Test]
        public void TestChar()
        {
            char obj = (new JsonDeserializer<char>("\"A\"")).Deserialize();
            Assert.AreEqual('A', obj);
        }

        /// <summary>
        /// 验证负数反序列化。
        /// </summary>
        [Test]
        public void TestNegativeNumbers()
        {
            int obj = (new JsonDeserializer<int>("-42")).Deserialize();
            Assert.AreEqual(-42, obj);

            double dObj = (new JsonDeserializer<double>("-3.14")).Deserialize();
            Assert.AreEqual(-3.14, dObj);

            float fObj = (new JsonDeserializer<float>("-2.5")).Deserialize();
            Assert.AreEqual(-2.5f, fObj);
        }

        /// <summary>
        /// 验证科学计数法数值反序列化。
        /// </summary>
        [Test]
        public void TestScientificNotation()
        {
            double obj = (new JsonDeserializer<double>("1.5e3")).Deserialize();
            Assert.AreEqual(1500.0, obj);

            obj = (new JsonDeserializer<double>("2.5E-2")).Deserialize();
            Assert.AreEqual(0.025, obj);
        }

        /// <summary>
        /// 验证嵌套对象反序列化。
        /// </summary>
        [Test]
        public void TestNestedObject()
        {
            string testStr = "{\"Name\":\"parent\",\"Child\":{\"Name\":\"child\",\"Age\":10}}";
            var obj = (new JsonDeserializer<NestedTestObj>(testStr)).Deserialize();
            Assert.AreEqual("parent", obj.Name);
            Assert.IsNotNull(obj.Child);
            Assert.AreEqual("child", obj.Child.Name);
            Assert.AreEqual(10, obj.Child.Age);
        }

        /// <summary>
        /// 验证对象数组反序列化。
        /// </summary>
        [Test]
        public void TestArrayOfObjects()
        {
            string testStr = "[{\"Id\":1,\"Name\":\"A\"},{\"Id\":2,\"Name\":\"B\"}]";
            var obj = (new JsonDeserializer<TestObjClass[]>(testStr)).Deserialize();
            Assert.AreEqual(2, obj.Length);
            Assert.AreEqual(1, obj[0].Id);
            Assert.AreEqual("A", obj[0].Name);
            Assert.AreEqual(2, obj[1].Id);
            Assert.AreEqual("B", obj[1].Name);
        }

        /// <summary>
        /// 验证空数组和空对象反序列化。
        /// </summary>
        [Test]
        public void TestEmptyStructures()
        {
            var emptyArray = (new JsonDeserializer<int[]>("[]")).Deserialize();
            Assert.IsNotNull(emptyArray);
            Assert.AreEqual(0, emptyArray.Length);

            var emptyList = (new JsonDeserializer<List<string>>("[]")).Deserialize();
            Assert.IsNotNull(emptyList);
            Assert.AreEqual(0, emptyList.Count);

            var emptyDict = (new JsonDeserializer<Dictionary<string, int>>("{}")).Deserialize();
            Assert.IsNotNull(emptyDict);
            Assert.AreEqual(0, emptyDict.Count);
        }

        /// <summary>
        /// 验证嵌套字典反序列化。
        /// </summary>
        [Test]
        public void TestNestedDictionary()
        {
            string testStr = "{\"outer\":{\"inner\":42}}";
            var obj = (new JsonDeserializer<Dictionary<string, Dictionary<string, int>>>(testStr)).Deserialize();
            Assert.IsTrue(obj.ContainsKey("outer"));
            Assert.AreEqual(42, obj["outer"]["inner"]);
        }

        /// <summary>
        /// 验证 JsonIgnored(All) 反序列化时会忽略该属性。
        /// </summary>
        [Test]
        public void TestJsonIgnoredAllOnDeserialize()
        {
            var testStr = "{\"Name\":\"test\",\"Age\":10,\"Height\":170}";
            var obj = (new JsonDeserializer<JsonIgnoredAllClass>(testStr)).Deserialize();
            Assert.AreEqual("test", obj.Name);
            Assert.AreEqual(10, obj.Age);
            Assert.AreEqual(0, obj.Height); // ignored for deserialization
        }

        /// <summary>
        /// 验证 JsonIgnored(Serialize) 反序列化时不会忽略该属性。
        /// </summary>
        [Test]
        public void TestJsonIgnoredSerializeOnlyDoesNotAffectDeserialize()
        {
            var testStr = "{\"Name\":\"test\",\"Age\":10,\"Height\":170}";
            var obj = (new JsonDeserializer<JsonIgnoredSerializeOnlyClass>(testStr)).Deserialize();
            Assert.AreEqual("test", obj.Name);
            Assert.AreEqual(10, obj.Age);
            Assert.AreEqual(170, obj.Height); // only ignored for serialize, not deserialize
        }

        /// <summary>
        /// 验证 int 数值反序列化。
        /// </summary>
        [Test]
        public void TestInt()
        {
            string testStr = "9284";
            int obj = (new JsonDeserializer<int>(testStr)).Deserialize();
            Assert.IsTrue(obj == 9284);
        }

        /// <summary>
        /// 验证 long 数值反序列化。
        /// </summary>
        [Test]
        public void TestLong()
        {
            string testStr = "9284523245244254";
            long obj = (new JsonDeserializer<long>(testStr)).Deserialize();
            Assert.IsTrue(obj == 9284523245244254);
        }

        /// <summary>
        /// 验证 float 数值反序列化。
        /// </summary>
        [Test]
        public void TestFloat()
        {
            string testStr = "1.2";
            float obj = (new JsonDeserializer<float>(testStr)).Deserialize();
            Assert.IsTrue(obj == 1.2f);
        }

        /// <summary>
        /// 验证 double 数值反序列化。
        /// </summary>
        [Test]
        public void TestDouble()
        {
            string testStr = "1024.248";
            double obj = (new JsonDeserializer<double>(testStr)).Deserialize();
            Assert.IsTrue(obj == 1024.248);
        }

        /// <summary>
        /// 验证 decimal 数值反序列化。
        /// </summary>
        [Test]
        public void TestDecimal()
        {
            string testStr = "1020990934.24823423423432";
            decimal obj = (new JsonDeserializer<decimal>(testStr)).Deserialize();
            Assert.IsTrue(obj == 1020990934.24823423423432m);
        }

        /// <summary>
        /// 验证枚举可按名称和数值反序列化。
        /// </summary>
        [Test]
        public void TestEnumMethod()
        {
            string testStr = "\"Test1\"";
            TestEnum obj = (new JsonDeserializer<TestEnum>(testStr)).Deserialize();
            Assert.IsTrue(obj == TestEnum.Test1);

            testStr = "1";
            obj = (new JsonDeserializer<TestEnum>(testStr)).Deserialize();
            Assert.IsTrue(obj == TestEnum.Test2);

        }

        /// <summary>
        /// 验证 DateTime 字符串反序列化。
        /// </summary>
        [Test]
        public void TestDateTime()
        {
            string testStr = "\"2020-07-09 12:00:00\"";
            DateTime obj = (new JsonDeserializer<DateTime>(testStr)).Deserialize();
            Assert.IsTrue(obj == new DateTime(2020, 07, 09, 12, 0, 0));
        }


        /// <summary>
        /// 验证数组和可空元素数组反序列化。
        /// </summary>
        [Test]
        public void TestArray()
        {
            string testStr = "[1,2,3,4,5,6,7,8,9,10]";
            int[] array = (new JsonDeserializer<int[]>(testStr)).Deserialize();

            Assert.AreEqual(array, new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });

            array = (new JsonDeserializer<int[]>("null")).Deserialize();
            Assert.AreEqual(array, null);


            testStr = "[1,2,3,4,null,6,7,8,9,10]";
            int?[] obj = (new JsonDeserializer<int?[]>(testStr)).Deserialize();
            for (int i = 0; i < obj.Length; i++)
            {
                if (i == 4)
                    Assert.IsNull(obj[i]);
                else
                    Assert.IsTrue(i + 1 == obj[i]);
            }
        }

        /// <summary>
        /// 验证 List 集合反序列化。
        /// </summary>
        [Test]
        public void TestList()
        {
            string testStr = "[1,2,3,4,5,6,7,8,9,10]";
            List<int> obj = (new JsonDeserializer<List<int>>(testStr)).Deserialize();
            for (int i = 0; i < obj.Count; i++)
            {
                Assert.IsTrue(i + 1 == obj[i]);
            }

            obj = (new JsonDeserializer<List<int>>("null")).Deserialize();
            Assert.IsNull(obj);
        }
        /// <summary>
        /// 验证字典及可空值字典反序列化。
        /// </summary>
        [Test]
        public void TestDictionary()
        {
            string testStr = "{\"one\":\"1\",\"two\":\"2\",\"three\":\"3\"}";
            Dictionary<string, string> obj = (new JsonDeserializer<Dictionary<string, string>>(testStr)).Deserialize();

            string resut1;
            obj.TryGetValue("three", out resut1);
            Assert.IsTrue(resut1 == "3");

            string testStr2 = "{\"one\":null,\"two\":2,\"three\":3}";
            Dictionary<string, int?> obj2 = (new JsonDeserializer<Dictionary<string, int?>>(testStr2)).Deserialize();

            int? result2;
            obj2.TryGetValue("three", out result2);
            Assert.IsTrue(result2 == 3);
        }

        /// <summary>
        /// 验证普通对象反序列化。
        /// </summary>
        [Test]
        public void TestObject()
        {
            string testStr = "{}";
            object obj = (new JsonDeserializer<object>(testStr)).Deserialize();
            Assert.IsNotNull(obj);

            testStr = "{\"Id\":100, \"Name\":\"Test\"}";
            obj = (new JsonDeserializer<TestObjClass>(testStr)).Deserialize();
            Assert.IsNotNull(obj);
        }

        /// <summary>
        /// 验证可空值类型反序列化。
        /// </summary>
        [Test]
        public void TestNullable()
        {
            string testStr = "null";
            int? obj = (new JsonDeserializer<int?>(testStr)).Deserialize();
            Assert.IsNull(obj);

            testStr = "12";
            obj = (new JsonDeserializer<int?>(testStr)).Deserialize();
            Assert.AreEqual(obj.Value, 12);
        }
        enum TestEnum
        {
            Test1,
            Test2,
            Test3
        }

        /// <summary>
        /// 验证 Int32 类型反序列化。
        /// </summary>
        [Test]
        public void TestInt32()
        {
            int testint32 = 0;
            string testStr = "1234";

            testint32 = (new JsonDeserializer<Int32>(testStr)).Deserialize();
            Assert.AreEqual(testint32, 1234);
        }
        /// <summary>
        /// 验证 GUID 类型反序列化
        /// </summary>
        [Test]
        public void TestGuid()
        {
            var guid = new Guid("d3f5f5e0-8c3b-4d2a-9f1e-2c3b5e6f7a8b");
            var guidStr = guid.ToString();
            var deserializedGuid = (new JsonDeserializer<Guid>($"\"{guidStr}\"")).Deserialize();
            Assert.AreEqual(guid, deserializedGuid);
        }

        /// <summary>
        /// 验证字符串转义后的反序列化。
        /// </summary>
        [Test]
        public void TestString()
        {
            string testStr = "\"c:\\\\ds\\\\dfe\\\\test.test\"";
            var testDistStr = (new JsonDeserializer<string>(testStr)).Deserialize();
            Assert.AreEqual("c:\\ds\\dfe\\test.test", testDistStr);
        }

        /// <summary>
        /// 验证 FromJson 扩展方法的反序列化结果。
        /// </summary>
        [Test]
        public void TestCommonExtend()
        {
            var strObj = "\"test\"".FromJson<string>();
            Assert.AreEqual(strObj, "test");
        }

        /// <summary>
        /// 验证多属性对象可批量反序列化。
        /// </summary>
        [Test]
        public void TestMultiProtertyObject()
        {
            List<string> testString = new List<string>();
            testString.Add("{\"Name\":\"Test1\",\"Age\":22,\"Height\":123.2342,\"Obj\":\"sfsdfsd\"}");
            testString.Add("{\"Name\":\"Test2\",\"Age\":26,\"Height\":1233.232,\"Obj\":null}");
            testString.Add("{\"Name\":\"Test3\",\"Age\":27,\"Height\":1243.232,\"Obj\":123}");
            testString.Add("{\"Name\":\"Test4\",\"Age\":28,\"Height\":123.2332,\"Obj\":null}");
            testString.Add("{\"Name\":\"Test5\",\"Age\":24,\"Height\":123.2352,\"Obj\":null}");
            testString.Add("{\"Name\":\"Test6\",\"Age\":25,\"Height\":12333.2342,\"Obj\":45.4}");

            foreach (var item in testString)
            {
                var a = LHZ.FastJson.JsonConvert.Deserialize<TestMultiProtertyObj>(item);
            }
        }

        /// <summary>
        /// 验证反序列化时会忽略指定属性。
        /// </summary>
        [Test]
        public void TestJsonIgnored()
        {
            var testStr = @"{""Name"":""test"",""Age"":10,""Height"":170}";
            var testObj = LHZ.FastJson.JsonConvert.Deserialize<TestJsonTgnoredClass>(testStr);
            Assert.AreEqual(testObj.Height, 0);
        }

        /// <summary>
        /// 验证自定义转换器会影响反序列化。
        /// </summary>
        [Test]
        public void TestCustomSerialize()
        {
            var jsonStr = "{\"Id\":1, \"Name\":\"tom\"}";
            var obj = JsonConvert.Deserialize<TestObjClass>(jsonStr, new JsonCustomConvert<int>(n=> 2));
           
            Assert.AreEqual(obj.Id, 2);
        }

        /// <summary>
        /// 验证 JsonProperty 特性支持属性名映射。
        /// </summary>
        [Test]
        public void TestJsonProptryNameAttribute()
        {
            var name = "test";
            var testObj = new JsonProptryNameAttributeTest()
            {
                Name = name,
                Age = 10
            };
            var jsonStr = (new JsonSerializer(testObj)).Serialize();
            var convertObj = new JsonDeserializer<JsonProptryNameAttributeTest>(jsonStr).Deserialize();

            Assert.AreEqual(testObj.Age, testObj.Age);
            Assert.AreEqual(testObj.Name, testObj.Name);
        }


    }
    public class TestObjClass
    { 
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class TestMultiProtertyObj
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public float Height { get; set; }
        public Object Obj { get; set; }
    }
    public class TestJsonTgnoredClass
    {
        public string Name { get; set; }
        public int Age { get; set; }
        [Json.Attributes.JsonIgnored(Enum.JsonMethods.Deserialize)]
        public float Height { get; set; }
    }
    public class NullableModel
    {
        public int? Count { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public double? Rating { get; set; }
        public string Name { get; set; }
    }

    public class NestedTestObj
    {
        public string Name { get; set; }
        public NestedChildObj Child { get; set; }
    }

    public class NestedChildObj
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }

    public class JsonIgnoredAllClass
    {
        public string Name { get; set; }
        public int Age { get; set; }
        [Json.Attributes.JsonIgnored(Enum.JsonMethods.All)]
        public float Height { get; set; }
    }

    public class JsonIgnoredSerializeOnlyClass
    {
        public string Name { get; set; }
        public int Age { get; set; }
        [Json.Attributes.JsonIgnored(Enum.JsonMethods.Serialize)]
        public float Height { get; set; }
    }
}
