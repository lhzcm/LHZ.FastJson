using LHZ.FastJson.Json;
using LHZ.FastJson.Json.CustomConverter;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using static LHZ.FastJson.UnitTest.TestJsonSerizlizer;

namespace LHZ.FastJson.UnitTest
{

    class TestJsonDeserizlizer
    {
        [Test]
        public void TestNull()
        {
            string testStr = "null";
            var obj = (new JsonDeserializer<object>(testStr)).Deserialize();
            Assert.IsNull(obj);
        }

        [Test]
        public void TestInt()
        {
            string testStr = "9284";
            int obj = (new JsonDeserializer<int>(testStr)).Deserialize();
            Assert.IsTrue(obj == 9284);
        }

        [Test]
        public void TestLong()
        {
            string testStr = "9284523245244254";
            long obj = (new JsonDeserializer<long>(testStr)).Deserialize();
            Assert.IsTrue(obj == 9284523245244254);
        }

        [Test]
        public void TestFloat()
        {
            string testStr = "1.2";
            float obj = (new JsonDeserializer<float>(testStr)).Deserialize();
            Assert.IsTrue(obj == 1.2f);
        }

        [Test]
        public void TestDouble()
        {
            string testStr = "1024.248";
            double obj = (new JsonDeserializer<double>(testStr)).Deserialize();
            Assert.IsTrue(obj == 1024.248);
        }

        [Test]
        public void TestDecimal()
        {
            string testStr = "1020990934.24823423423432";
            decimal obj = (new JsonDeserializer<decimal>(testStr)).Deserialize();
            Assert.IsTrue(obj == 1020990934.24823423423432m);
        }

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

        [Test]
        public void TestDateTime()
        {
            string testStr = "\"2020-07-09 12:00:00\"";
            DateTime obj = (new JsonDeserializer<DateTime>(testStr)).Deserialize();
            Assert.IsTrue(obj == new DateTime(2020, 07, 09, 12, 0, 0));
        }


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

        [Test]
        public void TestInt32()
        {
            int testint32 = 0;
            string testStr = "1234";

            testint32 = (new JsonDeserializer<Int32>(testStr)).Deserialize();
            Assert.AreEqual(testint32, 1234);
        }

        [Test]
        public void TestString()
        {
            string testStr = "\"c:\\\\ds\\\\dfe\\\\test.test\"";
            var testDistStr = (new JsonDeserializer<string>(testStr)).Deserialize();
            Assert.AreEqual("c:\\ds\\dfe\\test.test", testDistStr);
        }

        [Test]
        public void TestCommonExtend()
        {
            var strObj = "\"test\"".FromJson<string>();
            Assert.AreEqual(strObj, "test");
        }

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

        [Test]
        public void TestJsonIgnored()
        {
            var testStr = @"{""Name"":""test"",""Age"":10,""Height"":170}";
            var testObj = LHZ.FastJson.JsonConvert.Deserialize<TestJsonTgnoredClass>(testStr);
            Assert.AreEqual(testObj.Height, 0);
        }

        [Test]
        public void TestCustomSerialize()
        {
            var jsonStr = "{\"Id\":1, \"Name\":\"tom\"}";
            var obj = JsonConvert.Deserialize<TestObjClass>(jsonStr, new JsonCustomConvert<int>(n=> 2));
           
            Assert.AreEqual(obj.Id, 2);
        }

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
}
