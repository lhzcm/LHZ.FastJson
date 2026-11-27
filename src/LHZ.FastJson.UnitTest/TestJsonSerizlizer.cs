using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using LHZ.FastJson.Enum;
using LHZ.FastJson.Exceptions;
using LHZ.FastJson.Json;
using LHZ.FastJson.Json.Attributes;
using LHZ.FastJson.JsonClass;
using NUnit.Framework;

namespace LHZ.FastJson.UnitTest
{
    /// <summary>
    /// 验证 JSON 序列化的核心场景。
    /// </summary>
    class TestJsonSerizlizer
    {
        /// <summary>
        /// 验证 null 会序列化为 JSON null。
        /// </summary>
        [Test]
        public void TestNull()
        {
            object testObj = null;
            string obj = (new JsonSerializer(testObj)).Serialize();
            ClassicAssert.IsTrue(obj == "null");

            var _nullableWithNullsJson = JsonConvert.Serialize(new NullableModel());
            ClassicAssert.AreEqual("{\"Count\":null,\"ExpiryDate\":null,\"Rating\":null,\"Name\":null}", _nullableWithNullsJson);
        }

        /// <summary>
        /// 验证集合对象可以连续序列化。
        /// </summary>
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
            for (int i = 0; i < 10000; i++)
                obj = (new JsonSerializer(testObj)).Serialize();
            ClassicAssert.IsTrue(obj != null);
        }

        /// <summary>
        /// 验证循环引用会抛出预期异常。
        /// </summary>
        [Test]
        public void TestCircularReference()
        {
            TestObj testObj = new TestObj();
            TestObj testObj2 = new TestObj();
            testObj.Obj = testObj2;
            testObj2.Obj = testObj;
            var ex = Assert.Throws<JsonSerializationException>(() => (new JsonSerializer(testObj)).Serialize());
            Assert.That(ex.Message, Does.Contain("circular reference"));
            Assert.That(ex.SerializedType, Is.EqualTo(typeof(TestObj)));
        }

        /// <summary>
        /// 验证字典对象可以序列化。
        /// </summary>
        [Test]
        public void TestDictionary()
        {
            Dictionary<string, object> testObj = new Dictionary<string, object>();
            testObj.Add("test1", new { Name = "test1", Age = 18 });
            testObj.Add("test2", new { Name = "test2", IsTeacher = true });
            string obj = (new JsonSerializer(testObj)).Serialize();
            ClassicAssert.IsTrue(obj != null);
        }
        /// <summary>
        /// 验证字符串转义、空值和空串的序列化。
        /// </summary>
        [Test]
        public void TestString()
        {
            string testObj = "test\\\"\\\\tal";

            string obj = (new JsonSerializer(testObj)).Serialize();
            ClassicAssert.IsTrue("\"test\\\\\\\"\\\\\\\\tal\"" == obj);

            testObj = null;
            obj = (new JsonSerializer(testObj)).Serialize();
            ClassicAssert.AreEqual(obj, "null");


            testObj = "";
            obj = (new JsonSerializer(testObj)).Serialize();
            ClassicAssert.AreEqual(obj, "\"\"");

        }

        /// <summary>
        /// 验证可空值类型的序列化。
        /// </summary>
        [Test]
        public void TestNullable()
        {
            int? testObj = null;
            string obj = (new JsonSerializer(testObj)).Serialize();
            ClassicAssert.AreEqual(obj, "null");

            testObj = 12;
            obj = (new JsonSerializer(testObj)).Serialize();
            ClassicAssert.AreEqual(obj, "12");
        }

        /// <summary>
        /// 验证空对象会序列化为空 JSON 对象。
        /// </summary>
        [Test]
        public void TestEmptyObject()
        {
            object obj = new object();
            string objson = (new JsonSerializer(obj)).Serialize();
            ClassicAssert.AreEqual("{}", objson);
        }


        /// <summary>
        /// 验证空集合和空数组的序列化。
        /// </summary>
        [Test]
        public void TestEmptyArray()
        {
            List<object> list = new List<object>();
            string listjson = (new JsonSerializer(list)).Serialize();
            ClassicAssert.AreEqual("[]", listjson);

            listjson = (new JsonSerializer(new int[0])).Serialize();
            ClassicAssert.AreEqual("[]", listjson);
        }

        /// <summary>
        /// 验证 ToJson 扩展方法的序列化结果。
        /// </summary>
        [Test]
        public void TestCommonExtend()
        {
            var jsonStr = new { Name = "test", Age = 20 }.ToJson();
            ClassicAssert.AreEqual(jsonStr, "{\"Name\":\"test\",\"Age\":20}");
        }

        /// <summary>
        /// 验证结构体对象的序列化。
        /// </summary>
        [Test]
        public void TestStruct()
        {
            TestStructObj structObj = new TestStructObj() { Name = "test2", Age = 10, Height = 170, Obj = null };
            string structjson = (new JsonSerializer(structObj)).Serialize();
            ClassicAssert.AreEqual(structjson, "{\"Name\":\"test2\",\"Age\":10,\"Height\":170,\"Obj\":null}");
        }
        /// <summary>
        /// 验证Guid类型的序列化
        /// </summary>
        [Test]
        public void TestGuid()
        {
            var guid = Guid.NewGuid();
            var jsonStr = JsonConvert.Serialize(guid);
            var guid2 = JsonConvert.Deserialize<Guid>(jsonStr);
            ClassicAssert.AreEqual(guid, guid2);
        }

        /// <summary>
        /// 验证 Boolean 序列化。
        /// </summary>
        [Test]
        public void TestBoolean()
        {
            ClassicAssert.AreEqual("true", JsonConvert.Serialize(true));
            ClassicAssert.AreEqual("false", JsonConvert.Serialize(false));
        }

        /// <summary>
        /// 验证 Byte/SByte 序列化。
        /// </summary>
        [Test]
        public void TestByte()
        {
            ClassicAssert.AreEqual("255", JsonConvert.Serialize((byte)255));
            ClassicAssert.AreEqual("0", JsonConvert.Serialize((byte)0));
        }

        /// <summary>
        /// 验证 Int16/UInt16 序列化。
        /// </summary>
        [Test]
        public void TestInt16()
        {
            ClassicAssert.AreEqual("-32768", JsonConvert.Serialize((short)(-32768)));
            ClassicAssert.AreEqual("65535", JsonConvert.Serialize((ushort)65535));
        }

        /// <summary>
        /// 验证 UInt32/UInt64 序列化。
        /// </summary>
        [Test]
        public void TestUInt()
        {
            ClassicAssert.AreEqual("4294967295", JsonConvert.Serialize(4294967295u));
            ClassicAssert.AreEqual("18446744073709551615", JsonConvert.Serialize(18446744073709551615ul));
        }

        /// <summary>
        /// 验证 Char 序列化。
        /// </summary>
        [Test]
        public void TestChar()
        {
            ClassicAssert.AreEqual("\"A\"", JsonConvert.Serialize('A'));
        }

        /// <summary>
        /// 验证负数序列化。
        /// </summary>
        [Test]
        public void TestNegativeNumbers()
        {
            ClassicAssert.AreEqual("-42", JsonConvert.Serialize(-42));
            ClassicAssert.AreEqual("-3.14", JsonConvert.Serialize(-3.14d));
            ClassicAssert.AreEqual("-2.5", JsonConvert.Serialize(-2.5f));
        }

        /// <summary>
        /// 验证嵌套对象序列化。
        /// </summary>
        [Test]
        public void TestNestedObject()
        {
            var obj = new NestedSerializeObj
            {
                Name = "parent",
                Child = new NestedSerializeChild { Name = "child", Age = 10 }
            };
            var json = JsonConvert.Serialize(obj);
            ClassicAssert.AreEqual("{\"Name\":\"parent\",\"Child\":{\"Name\":\"child\",\"Age\":10}}", json);
        }

        /// <summary>
        /// 验证 JsonIgnored(All) 序列化时会忽略该属性。
        /// </summary>
        [Test]
        public void TestJsonIgnoredAll()
        {
            var obj = new JsonIgnoredAllSerializeClass
            {
                Name = "test",
                Age = 10,
                Height = 170
            };
            var json = JsonConvert.Serialize(obj);
            ClassicAssert.AreEqual("{\"Name\":\"test\",\"Age\":10}", json);
        }

        /// <summary>
        /// 验证枚举作为根对象、集合元素、object 类型属性和字典值时都序列化为数值。
        /// </summary>
        [Test]
        public void TestEnumSerializeInContainers()
        {
            ClassicAssert.AreEqual("1", new JsonSerializer(SerializeColor.Green).Serialize());
            ClassicAssert.AreEqual("[0,1]", new JsonSerializer(new List<SerializeColor> { SerializeColor.Red, SerializeColor.Green }).Serialize());
            ClassicAssert.AreEqual("{\"Value\":1}", new JsonSerializer(new SerializeObjectHolder { Value = SerializeColor.Green }).Serialize());
            ClassicAssert.AreEqual("{\"a\":1}", new JsonSerializer(new Dictionary<string, SerializeColor> { { "a", SerializeColor.Green } }).Serialize());
        }

        /// <summary>
        /// 验证手动构建的 JSON 节点放在 object 类型属性中时会原样输出（替代自定义转换器的写法）。
        /// </summary>
        [Test]
        public void TestJsonNodeInObjectPropertySerialize()
        {
            var money = new JsonContent();
            money.AddJsonProperty("amount", new JsonNumber(12.5));
            money.AddJsonProperty("currency", new JsonString("CNY"));

            ClassicAssert.AreEqual("{\"Value\":{\"amount\":12.5,\"currency\":\"CNY\"}}", new JsonSerializer(new SerializeObjectHolder { Value = money }).Serialize());
        }

        /// <summary>
        /// 验证序列化时会忽略指定属性。
        /// </summary>
        [Test]
        public void TestJsonIgnored()
        {
            TestJsonTgnoredClass testJsonTgnoredClass = new TestJsonTgnoredClass()
            {
                Name = "test",
                Age = 10,
                Height = 170
            };
            var testStr = LHZ.FastJson.JsonConvert.Serialize(testJsonTgnoredClass);
            ClassicAssert.AreEqual(testStr, @"{""Name"":""test"",""Age"":10}");
        }

        /// <summary>
        /// 验证 JsonProperty 特性会改写属性名。
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
            var jsonObj = (new JsonReader(jsonStr)).JsonRead();
            ClassicAssert.AreEqual(jsonObj["studentName"].Value, name);
        }
        [Test]
        public void TestEnumSerialize()
        {
            var obj = new TestEnumClass() { ObjectType = ObjectType.Enum, Name = "枚举类型" };
            var jsonStr = JsonConvert.Serialize(obj);
            ClassicAssert.AreEqual(jsonStr, "{\"ObjectType\":"+ ((int)ObjectType.Enum).ToString() + ",\"Name\":\"枚举类型\"}");
        }
        [Test]
        public void TestDitionarySerialize()
        {
            var obj = new Dictionary<string, SimpleModel>();
            obj.Add("test1", new SimpleModel()
            {
                Id = 1,
                Name = "test1",
                Price = 10.0,
                IsActive = true,
                CreateTime = DateTime.Parse("2026-06-24 23:32:03"),
                Uid = Guid.Parse("5789473f-32c1-43f4-b723-834e05b07afd")
            });
            obj.Add("test2", new SimpleModel()
            {
                Id = 2,
                Name = "test2",
                Price = 20.0,
                IsActive = false,
                CreateTime = DateTime.Parse("2026-06-24 23:32:03"),
                Uid = Guid.Parse("0d6c8fd4-b18d-47d2-95c2-b07c9a5d8312")
            });
            var jsonStr = JsonConvert.Serialize(obj);
            ClassicAssert.AreEqual(jsonStr, "{\"test1\":{\"Id\":1,\"Name\":\"test1\",\"Price\":10,\"IsActive\":true,\"CreateTime\":\"2026-06-24T23:32:03\",\"Uid\":\"5789473f-32c1-43f4-b723-834e05b07afd\"},\"test2\":{\"Id\":2,\"Name\":\"test2\",\"Price\":20,\"IsActive\":false,\"CreateTime\":\"2026-06-24T23:32:03\",\"Uid\":\"0d6c8fd4-b18d-47d2-95c2-b07c9a5d8312\"}}");
        }
        public class JsonProptryNameAttributeTest
        {
            [JsonProperty("studentName")]
            public string Name { get; set; }
            public int Age { get; set; }
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

        public class TestJsonTgnoredClass
        {
            public string Name { get; set; }
            public int Age { get; set; }
            [Json.Attributes.JsonIgnored(Enum.JsonMethods.Serialize)]
            public float Height { get; set; }
        }
        internal class TestEnumClass
        {
            public ObjectType ObjectType { get; set; }
            public string Name { get; set; }
        }
        public class SimpleModel
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public double Price { get; set; }
            public bool IsActive { get; set; }
            public DateTime CreateTime { get; set; }
            public Guid Uid { get; set; }
        }
        public class NullableModel
        {
            public int? Count { get; set; }
            public DateTime? ExpiryDate { get; set; }
            public double? Rating { get; set; }
            public string Name { get; set; }
        }

        public class NestedSerializeObj
        {
            public string Name { get; set; }
            public NestedSerializeChild Child { get; set; }
        }

        public class NestedSerializeChild
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }

        public class JsonIgnoredAllSerializeClass
        {
            public string Name { get; set; }
            public int Age { get; set; }
            [Json.Attributes.JsonIgnored(Enum.JsonMethods.All)]
            public float Height { get; set; }
        }

        public enum SerializeColor
        {
            Red = 0,
            Green = 1
        }

        public class SerializeObjectHolder
        {
            public object Value { get; set; }
        }
    }
}
