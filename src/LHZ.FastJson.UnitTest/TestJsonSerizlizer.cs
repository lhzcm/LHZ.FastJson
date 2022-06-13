using LHZ.FastJson.Json;
using LHZ.FastJson.Json.Format;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace LHZ.FastJson.UnitTest
{
    class TestJsonSerizlizer
    {
        [Test]
        public void TestNull()
        {
            object testObj = null;
            string obj = (new JsonSerializer(testObj)).Serialize();
            Assert.IsTrue(obj == "null");
        }

        [Test]
        public void TestEnumerable()
        {
            List<TestObj> testObj = new List<TestObj>();
            testObj.Add(new TestObj() { Name = "test1", Age = 18, Height = 230.4543f });
            testObj.Add(new TestObj() { Name = "test2", Age = 15, Height = 230.43f });
            testObj.Add(new TestObj() { Name = "test4", Age = 38, Height = 230.43f });
            testObj.Add(new TestObj() { Name = "test5", Age = 32, Height = 230.4543f });
            testObj.Add(new TestObj() { Name = "test6", Age = 21, Height = 30.4543f });
            testObj.Add(new TestObj() { Name = "test7", Age = 18, Height = 20.454f });
            testObj.Add(new TestObj() { Name = "test8", Age = 24, Height = 230.453f });
            testObj.Add(new TestObj() { Name = "test9", Age = 11, Height = 20.443f });
            string obj = null;
            for(int i = 0; i < 10000; i++)
                obj = (new JsonSerializer(testObj)).Serialize();
            Assert.IsTrue(obj != null);
        }

        [Test]
        public void TestCircularReference()
        {
            TestObj testObj = new TestObj();
            TestObj testObj2 = new TestObj();
            testObj.Obj = testObj2;
            testObj2.Obj = testObj;
            try
            {
                string obj = (new JsonSerializer(testObj)).Serialize();
            }
            catch (Exception ex)
            {
                Assert.AreEqual(ex.Message, "循环引用");
                return;
            }
            throw new Exception("没有按照预期抛出 “循环引用” 异常");

        }

        [Test]
        public void TestDictionary()
        {
            Dictionary<string, object> testObj = new Dictionary<string, object>();
            testObj.Add("test1", new { Name = "test1", Age = 18 });
            testObj.Add("test2", new { Name = "test2", IsTeacher = true});
            string obj = (new JsonSerializer(testObj)).Serialize();
            Assert.IsTrue(obj != null);
        }
        [Test]
        public void TestDateTime()
        {
            DateTime testObj = new DateTime(2020, 7, 18);
            IJsonFormat[] formats = { new DateTimeJsonFormat("yyyy-MM-dd") };
            string obj = (new JsonSerializer(testObj, formats)).Serialize();
            Assert.AreEqual(obj, "\"2020-07-18\"");

            formats[0] = new DateTimeJsonFormat((n)=>"test123");
            obj = (new JsonSerializer(testObj, formats)).Serialize();
            Assert.AreEqual(obj, "\"test123\"");

        }
        [Test]
        public void TestString()
        {
            string testObj = "test\\\"\\\\tal";
            
            string obj = (new JsonSerializer(testObj)).Serialize();
            Assert.IsTrue("\"test\\\\\\\"\\\\\\\\tal\"" == obj);

            testObj = null;
            obj = (new JsonSerializer(testObj)).Serialize();
            Assert.AreEqual(obj, "null");


            testObj = "";
            obj = (new JsonSerializer(testObj)).Serialize();
            Assert.AreEqual(obj, "\"\"");

        }

        [Test]
        public void TestNullable()
        {
            int? testObj = null;
            string obj = (new JsonSerializer(testObj)).Serialize();
            Assert.AreEqual(obj, "null");

            testObj = 12;
            obj = (new JsonSerializer(testObj)).Serialize();
            Assert.AreEqual(obj, "12");
        }

        [Test]
        public void TestEmptyObject()
        {
            object obj = new object();
            string objson = (new JsonSerializer(obj)).Serialize();
            Assert.AreEqual("{}", objson);
        }


        [Test]
        public void TestEmptyArray()
        {
            List<object> list = new List<object>();
            string listjson = (new JsonSerializer(list)).Serialize();
            Assert.AreEqual("[]", listjson);

            listjson = (new JsonSerializer(new int[0])).Serialize();
            Assert.AreEqual("[]", listjson);
        }

        [Test]
        public void TestCommonExtend()
        {
            var jsonStr = new { Name = "test", Age = 20 }.ToJson();
            Assert.AreEqual(jsonStr, "{\"Name\":\"test\",\"Age\":20}");
        }

        [Test]
        public void TestStruct()
        {
            TestStructObj structObj = new TestStructObj() { Name="test2", Age = 10, Height= 170, Obj = null };
            string structjson = (new JsonSerializer(structObj)).Serialize();
            Assert.AreEqual(structjson, "{\"Name\":\"test2\",\"Age\":10,\"Height\":170,\"Obj\":null}");
        }


        public class TestObj
        {
            public string Name { get; set; }
            public int Age { get; set; }
            public float Height { get; set; }
            public Object Obj { get; set; }
        }

        public struct TestStructObj
        {
            public string Name { get; set; }
            public int Age { get; set; }
            public float Height { get; set; }
            public TestObj Obj { get; set; }
        }
    }
}
