using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace LHZ.FastJson.UnitTest
{
    /// <summary>
    /// 往返测试：序列化后再反序列化应该得到相同的数据。
    /// 随机测试使用固定种子，保证结果可重复。
    /// </summary>
    public class RoundTripTests
    {
        #region 模型

        public enum Status { Unknown = 0, Active = 1, Closed = 2 }

        public class Address
        {
            public string City { get; set; }
            public string Street { get; set; }
            public int? Zip { get; set; }
        }

        public class Customer
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public DateTime Created { get; set; }
            public decimal Balance { get; set; }
            public double Rating { get; set; }
            public float Ratio { get; set; }
            public bool Active { get; set; }
            public Guid Key { get; set; }
            public Status Status { get; set; }
            public long Big { get; set; }
            public ulong Huge { get; set; }
            public char Initial { get; set; }
            public Address Home { get; set; }
            public Address Work { get; set; }
            public List<string> Tags { get; set; }
            public string[] Aliases { get; set; }
            public int[] Numbers { get; set; }
            public int? Optional { get; set; }
            public List<Address> History { get; set; }
            public Dictionary<string, int> Scores { get; set; }
            public Dictionary<string, List<int>> Series { get; set; }
            public Dictionary<int, Address> ByNumber { get; set; }
        }

        public class Node
        {
            public int Level { get; set; }
            public Node Next { get; set; }
        }

        public struct Coordinate
        {
            public double Lat { get; set; }
            public double Lng { get; set; }
        }

        public class Place
        {
            public string Name { get; set; }
            public Coordinate Position { get; set; }
            public Coordinate? Marker { get; set; }
        }

        #endregion

        private static string Ser(object value)
        {
            return JsonConvert.Serialize(value);
        }

        private static T RoundTrip<T>(T value)
        {
            return JsonConvert.Deserialize<T>(JsonConvert.Serialize(value));
        }

        private static Customer CreateCustomer()
        {
            return new Customer
            {
                Id = 42,
                Name = "张三 \"Zhang\" \\ / \n\t😀",
                Created = new DateTime(2024, 5, 6, 7, 8, 9, 123, DateTimeKind.Utc),
                Balance = 1234567.89m,
                Rating = 4.75,
                Ratio = 0.1f,
                Active = true,
                Key = Guid.Parse("11111111-2222-3333-4444-555555555555"),
                Status = Status.Closed,
                Big = long.MinValue,
                Huge = ulong.MaxValue,
                Initial = '中',
                Home = new Address { City = "Shenzhen", Street = "a\\b\"c", Zip = 518000 },
                Work = null,
                Tags = new List<string> { "a", "", "中文", null },
                Aliases = new[] { "x", "y" },
                Numbers = new[] { -1, 0, int.MaxValue },
                Optional = null,
                History = new List<Address> { new Address { City = "A" }, null, new Address { City = "B", Zip = 1 } },
                Scores = new Dictionary<string, int> { ["math"] = 90, ["中文"] = 85, [""] = 0 },
                Series = new Dictionary<string, List<int>> { ["s1"] = new List<int> { 1, 2, 3 }, ["empty"] = new List<int>() },
                ByNumber = new Dictionary<int, Address> { [1] = new Address { City = "One" }, [-2] = null }
            };
        }

        #region 复杂对象

        [Test]
        public void ComplexObjectRoundTripsFieldByField()
        {
            var original = CreateCustomer();
            var back = RoundTrip(original);

            Assert.That(back.Id, Is.EqualTo(original.Id));
            Assert.That(back.Name, Is.EqualTo(original.Name));
            Assert.That(back.Created, Is.EqualTo(original.Created));
            Assert.That(back.Created.Kind, Is.EqualTo(DateTimeKind.Utc));
            Assert.That(back.Balance, Is.EqualTo(original.Balance));
            Assert.That(back.Rating, Is.EqualTo(original.Rating));
            Assert.That(back.Ratio, Is.EqualTo(original.Ratio));
            Assert.That(back.Active, Is.True);
            Assert.That(back.Key, Is.EqualTo(original.Key));
            Assert.That(back.Status, Is.EqualTo(Status.Closed));
            Assert.That(back.Big, Is.EqualTo(long.MinValue));
            Assert.That(back.Huge, Is.EqualTo(ulong.MaxValue));
            Assert.That(back.Initial, Is.EqualTo('中'));
            Assert.That(back.Home.City, Is.EqualTo("Shenzhen"));
            Assert.That(back.Home.Street, Is.EqualTo("a\\b\"c"));
            Assert.That(back.Home.Zip, Is.EqualTo(518000));
            Assert.That(back.Work, Is.Null);
            Assert.That(back.Tags, Is.EqualTo(original.Tags));
            Assert.That(back.Aliases, Is.EqualTo(original.Aliases));
            Assert.That(back.Numbers, Is.EqualTo(original.Numbers));
            Assert.That(back.Optional, Is.Null);
            Assert.That(back.History.Count, Is.EqualTo(3));
            Assert.That(back.History[1], Is.Null);
            Assert.That(back.History[2].Zip, Is.EqualTo(1));
            Assert.That(back.Scores, Is.EquivalentTo(original.Scores));
            Assert.That(back.Series["s1"], Is.EqualTo(new[] { 1, 2, 3 }));
            Assert.That(back.Series["empty"], Is.Empty);
            Assert.That(back.ByNumber[1].City, Is.EqualTo("One"));
            Assert.That(back.ByNumber[-2], Is.Null);
        }

        [Test]
        public void SecondSerializationIsIdenticalToTheFirst()
        {
            string first = Ser(CreateCustomer());
            string second = Ser(JsonConvert.Deserialize<Customer>(first));
            Assert.That(second, Is.EqualTo(first));
        }

        [Test]
        public void PropertyOrderInTheJsonDoesNotMatter()
        {
            var forward = JsonConvert.Deserialize<Address>("{\"City\":\"c\",\"Street\":\"s\",\"Zip\":1}");
            var reversed = JsonConvert.Deserialize<Address>("{\"Zip\":1,\"Street\":\"s\",\"City\":\"c\"}");
            Assert.That(Ser(reversed), Is.EqualTo(Ser(forward)));
        }

        [Test]
        public void StructsRoundTrip()
        {
            var back = RoundTrip(new Place { Name = "p", Position = new Coordinate { Lat = 22.5431, Lng = 114.0579 }, Marker = new Coordinate { Lat = -1.5, Lng = 2.5 } });
            Assert.That(back.Name, Is.EqualTo("p"));
            Assert.That(back.Position.Lat, Is.EqualTo(22.5431));
            Assert.That(back.Position.Lng, Is.EqualTo(114.0579));
            Assert.That(back.Marker.Value.Lat, Is.EqualTo(-1.5));

            Assert.That(RoundTrip(new Place { Name = "q" }).Marker, Is.Null);
        }

        [Test]
        public void EmptyCollectionsStayEmptyAndNullsStayNull()
        {
            var back = RoundTrip(new Customer { Tags = new List<string>(), Numbers = new int[0], Scores = new Dictionary<string, int>() });
            Assert.That(back.Tags, Is.Not.Null.And.Empty);
            Assert.That(back.Numbers, Is.Not.Null.And.Empty);
            Assert.That(back.Scores, Is.Not.Null.And.Empty);
            Assert.That(back.Aliases, Is.Null);
            Assert.That(back.History, Is.Null);
            Assert.That(back.Series, Is.Null);
        }

        [Test]
        public void UnicodeDictionaryKeysRoundTrip()
        {
            var original = new Dictionary<string, int> { ["中文"] = 1, ["é"] = 2, ["😀"] = 3, ["a b"] = 4, ["a/b"] = 5 };
            Assert.That(RoundTrip(original), Is.EquivalentTo(original));
        }

        #endregion

        #region 字符串

        /// <summary>
        /// 验证基本多文种平面内（除代理项外）的每一个字符都能往返。
        /// </summary>
        [Test]
        public void EveryBmpCharacterRoundTrips()
        {
            var builder = new StringBuilder(65536);
            for (int c = 0; c <= 0xFFFF; c++)
            {
                if (c < 0xD800 || c > 0xDFFF)
                {
                    builder.Append((char)c);
                }
            }
            string all = builder.ToString();
            Assert.That(RoundTrip(all), Is.EqualTo(all));
        }

        [Test]
        public void SurrogatePairsRoundTrip()
        {
            string emoji = "😀🎉👨‍👩‍👧‍👦𠮷";
            Assert.That(RoundTrip(emoji), Is.EqualTo(emoji));
            Assert.That(RoundTrip(new Address { City = emoji }).City, Is.EqualTo(emoji));
        }

        [Test]
        public void RandomStringsRoundTrip()
        {
            var random = new Random(20240101);
            for (int i = 0; i < 500; i++)
            {
                var builder = new StringBuilder();
                int length = random.Next(0, 60);
                for (int j = 0; j < length; j++)
                {
                    switch (random.Next(4))
                    {
                        case 0: builder.Append((char)random.Next(0, 0x80)); break;
                        case 1: builder.Append((char)random.Next(0x80, 0xD800)); break;
                        case 2: builder.Append("\"\\/\b\f\n\r\t".Substring(random.Next(0, 8), 1)); break;
                        default: builder.Append(char.ConvertFromUtf32(random.Next(0x10000, 0x10FFFF))); break;
                    }
                }
                string value = builder.ToString();
                Assert.That(RoundTrip(value), Is.EqualTo(value), "第 " + i + " 个随机字符串");
            }
        }

        [Test]
        public void LongStringRoundTrips()
        {
            string value = new string('x', 200000) + "\"" + new string('中', 50000);
            Assert.That(RoundTrip(value), Is.EqualTo(value));
        }

        #endregion

        #region 随机数值

        [Test]
        public void RandomDoublesRoundTripExactly()
        {
            var random = new Random(1);
            var bytes = new byte[8];
            int checkedCount = 0;
            for (int i = 0; i < 3000; i++)
            {
                random.NextBytes(bytes);
                double value = BitConverter.ToDouble(bytes, 0);
                if (double.IsNaN(value) || double.IsInfinity(value))
                {
                    continue;
                }
                double back = RoundTrip(value);
                Assert.That(BitConverter.DoubleToInt64Bits(back), Is.EqualTo(BitConverter.DoubleToInt64Bits(value)), "double " + value.ToString("R"));
                checkedCount++;
            }
            Assert.That(checkedCount, Is.GreaterThan(2900));
        }

        [Test]
        public void HandPickedDoublesRoundTripExactly()
        {
            foreach (double value in new[] { 0.0, -0.0, 0.1, 0.2 + 0.1, 1.0 / 3, 1e-300, 1e300, 123456789.123456789, double.Epsilon, double.MaxValue, double.MinValue, 4.9e-324, 2.2250738585072014E-308 })
            {
                Assert.That(BitConverter.DoubleToInt64Bits(RoundTrip(value)), Is.EqualTo(BitConverter.DoubleToInt64Bits(value)), value.ToString("R"));
            }
        }

        [Test]
        public void RandomFloatsRoundTripExactly()
        {
            var random = new Random(2);
            var bytes = new byte[4];
            for (int i = 0; i < 3000; i++)
            {
                random.NextBytes(bytes);
                float value = BitConverter.ToSingle(bytes, 0);
                if (float.IsNaN(value) || float.IsInfinity(value))
                {
                    continue;
                }
                float back = RoundTrip(value);
                Assert.That(BitConverter.ToInt32(BitConverter.GetBytes(back), 0), Is.EqualTo(BitConverter.ToInt32(BitConverter.GetBytes(value), 0)), "float " + value.ToString("R"));
            }
        }

        [Test]
        public void RandomDecimalsRoundTripExactly()
        {
            var random = new Random(3);
            for (int i = 0; i < 2000; i++)
            {
                var value = new decimal(random.Next(int.MinValue, int.MaxValue), random.Next(int.MinValue, int.MaxValue), random.Next(0, int.MaxValue), random.Next(2) == 0, (byte)random.Next(0, 29));
                decimal back = RoundTrip(value);
                Assert.That(back, Is.EqualTo(value), value.ToString());
                Assert.That(back.ToString(), Is.EqualTo(value.ToString()), "小数位数应保持不变：" + value);
            }
        }

        [Test]
        public void RandomIntegersRoundTrip()
        {
            var random = new Random(4);
            var bytes = new byte[8];
            for (int i = 0; i < 2000; i++)
            {
                random.NextBytes(bytes);
                long signed = BitConverter.ToInt64(bytes, 0);
                ulong unsigned = BitConverter.ToUInt64(bytes, 0);
                Assert.That(RoundTrip(signed), Is.EqualTo(signed));
                Assert.That(RoundTrip(unsigned), Is.EqualTo(unsigned));
                Assert.That(RoundTrip((int)signed), Is.EqualTo((int)signed));
                Assert.That(RoundTrip((uint)unsigned), Is.EqualTo((uint)unsigned));
                Assert.That(RoundTrip((short)signed), Is.EqualTo((short)signed));
                Assert.That(RoundTrip((ushort)unsigned), Is.EqualTo((ushort)unsigned));
                Assert.That(RoundTrip((byte)unsigned), Is.EqualTo((byte)unsigned));
            }
        }

        [Test]
        public void RandomDateTimesRoundTripWithTicksAndKind()
        {
            var random = new Random(5);
            var kinds = new[] { DateTimeKind.Utc, DateTimeKind.Local, DateTimeKind.Unspecified };
            for (int i = 0; i < 500; i++)
            {
                long ticks = (long)(random.NextDouble() * (DateTime.MaxValue.Ticks - 1));
                var value = new DateTime(ticks, kinds[i % 3]);
                var back = RoundTrip(value);
                Assert.That(back.Ticks, Is.EqualTo(value.Ticks), value.ToString("o"));
                Assert.That(back.Kind, Is.EqualTo(value.Kind), value.ToString("o"));
            }
        }

        #endregion

        #region 规模与深度

        [Test]
        public void LargeListOfObjectsRoundTrips()
        {
            var list = Enumerable.Range(0, 10000)
                .Select(i => new Address { City = "city" + i, Street = i % 7 == 0 ? null : "s\"" + i, Zip = i % 3 == 0 ? (int?)null : i })
                .ToList();
            var back = RoundTrip(list);
            Assert.That(back.Count, Is.EqualTo(10000));
            Assert.That(back[0].City, Is.EqualTo("city0"));
            Assert.That(back[0].Street, Is.Null);
            Assert.That(back[9999].City, Is.EqualTo("city9999"));
            Assert.That(back[9999].Zip, Is.Null);
            Assert.That(back[9998].Zip, Is.EqualTo(9998));
            Assert.That(Ser(back), Is.EqualTo(Ser(list)));
        }

        [Test]
        public void LargeDictionaryRoundTrips()
        {
            var map = Enumerable.Range(0, 5000).ToDictionary(i => "key" + i, i => i);
            var back = RoundTrip(map);
            Assert.That(back.Count, Is.EqualTo(5000));
            Assert.That(back["key4999"], Is.EqualTo(4999));
        }

        [Test]
        public void ObjectChainWithinTheDefaultDepthLimitRoundTrips()
        {
            Node head = null;
            for (int level = 50; level >= 1; level--)
            {
                head = new Node { Level = level, Next = head };
            }
            var back = RoundTrip(head);
            int count = 0;
            for (var node = back; node != null; node = node.Next)
            {
                count++;
                Assert.That(node.Level, Is.EqualTo(count));
            }
            Assert.That(count, Is.EqualTo(50));
        }

        [Test]
        public void DeeperChainRoundTripsWhenTheDepthLimitIsRaised()
        {
            Node head = null;
            for (int level = 100; level >= 1; level--)
            {
                head = new Node { Level = level, Next = head };
            }
            var options = new JsonConvertOptions { MaxDepth = 150 };
            var back = JsonConvert.Deserialize<Node>(JsonConvert.Serialize(head, options), options);
            int count = 0;
            for (var node = back; node != null; node = node.Next)
            {
                count++;
            }
            Assert.That(count, Is.EqualTo(100));
        }

        #endregion

        #region JSON 节点树

        [Test]
        public void ParsedTreeIsWrittenBackVerbatim()
        {
            string json = "{\"a\":[1,2.5,-3e2,\"x\\\"y\\\\z\\n\",true,false,null,{\"b\":{}}],\"c\":\"\\u4e2d\",\"d\":[]}";
            var tree = JsonConvert.Deserialize(json);
            string written = Ser(tree);
            var reparsed = JsonConvert.Deserialize(written);
            Assert.That(Ser(reparsed), Is.EqualTo(written));
            Assert.That(tree.ToString(), Is.EqualTo(written));
        }

        [Test]
        public void TreeFromTypedOutputMatchesTheTypedOutput()
        {
            string typedJson = Ser(CreateCustomer());
            Assert.That(JsonConvert.Deserialize(typedJson).ToString(), Is.EqualTo(typedJson));
        }

        #endregion
    }
}
