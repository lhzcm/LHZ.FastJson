using LHZ.FastJson.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

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
            Assert.IsTrue(obj == new DateTime(2020,07,09,12,0,0));
        }


        [Test]
        public void TestArray()
        {
            string testStr = "[1,2,3,4,null,6,7,8,9,10]";
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
        }

        [Test]
        public void TestDictionary()
        {
            string testStr = "{\"one\":1,\"two\":2,\"three\":3}";
            Dictionary<string,int> obj = (new JsonDeserializer<Dictionary<string, int>>(testStr)).Deserialize();

            int result;
            obj.TryGetValue("three", out result);
            Assert.IsTrue(result == 3);
        }

        [Test]
        public void TestObject()
        {
            string testStr = "{}";
            object obj = (new JsonDeserializer<object>(testStr)).Deserialize();
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
    }
}
