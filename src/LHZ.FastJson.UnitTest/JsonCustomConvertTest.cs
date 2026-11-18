using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using LHZ.FastJson.Enum;
using LHZ.FastJson.Enum.CustomConverter;
using LHZ.FastJson.Exceptions;
using LHZ.FastJson.Interface;
using LHZ.FastJson.Json;
using LHZ.FastJson.Json.CustomConverter;
using NUnit.Framework;

namespace LHZ.FastJson.UnitTest
{
    /// <summary>
    /// 验证 JsonCustomConvert 的构造、元数据、序列化 / 反序列化以及与 JsonConvert 的集成行为。
    /// </summary>
    public class JsonCustomConvertTest
    {
        #region 构造与元数据

        /// <summary>
        /// 验证各个构造函数对应的 CustomItem 和 ConvertType。
        /// </summary>
        [Test]
        public void ConstructorsSetCustomItemAndConvertType()
        {
            var none = new JsonCustomConvert<int>();
            var serializeOnly = new JsonCustomConvert<int>(n => n.ToString());
            var deserializeOnly = new JsonCustomConvert<int>(j => 1);
            var both = new JsonCustomConvert<int>(n => n.ToString(), j => 1);

            ClassicAssert.AreEqual(JsonCustomConvertItem.None, none.CustomItem);
            ClassicAssert.AreEqual(JsonCustomConvertItem.CustomSerialize, serializeOnly.CustomItem);
            ClassicAssert.AreEqual(JsonCustomConvertItem.CustomDeSerialize, deserializeOnly.CustomItem);
            ClassicAssert.AreEqual(JsonCustomConvertItem.All, both.CustomItem);

            foreach (IJsonCustomConverter converter in new IJsonCustomConverter[] { none, serializeOnly, deserializeOnly, both })
            {
                ClassicAssert.AreEqual(typeof(int), converter.ConvertType);
            }
            ClassicAssert.AreEqual(typeof(Money), new JsonCustomConvert<Money>().ConvertType);
        }

        /// <summary>
        /// 验证委托为 null 时构造函数抛出 ArgumentNullException。
        /// </summary>
        [Test]
        public void ConstructorsRejectNullDelegates()
        {
            Assert.Throws<ArgumentNullException>(() => new JsonCustomConvert<int>((Func<int, string>)null));
            Assert.Throws<ArgumentNullException>(() => new JsonCustomConvert<int>((Func<IJsonObject, int>)null));
            Assert.Throws<ArgumentNullException>(() => new JsonCustomConvert<int>(null, j => 1));
            Assert.Throws<ArgumentNullException>(() => new JsonCustomConvert<int>(n => n.ToString(), null));
        }

        #endregion

        #region Serialize / Deserialize 方法本身

        /// <summary>
        /// 验证 Serialize 会调用自定义委托，并支持通过非泛型接口调用。
        /// </summary>
        [Test]
        public void SerializeInvokesCustomDelegate()
        {
            var converter = new JsonCustomConvert<int>(n => "\"v" + n + "\"");

            ClassicAssert.AreEqual("\"v5\"", converter.Serialize(5));
            ClassicAssert.AreEqual("\"v6\"", ((IJsonCustomConverter)converter).Serialize(6));
        }

        /// <summary>
        /// 验证 Deserialize 会把解析后的 IJsonObject 交给自定义委托，并支持通过非泛型接口调用。
        /// </summary>
        [Test]
        public void DeserializeInvokesCustomDelegate()
        {
            var converter = new JsonCustomConvert<int>(j => int.Parse(j.Value.ToString()) + 1);
            var tree = JsonConvert.Deserialize("41");

            ClassicAssert.AreEqual(42, converter.Deserialize(tree));
            ClassicAssert.AreEqual(42, ((IJsonCustomConverter)converter).Deserialize(tree));
        }

        /// <summary>
        /// 验证开启 serializeValidate 后，自定义序列化返回非法 JSON 会抛出 JsonCustomConverterException。
        /// </summary>
        [Test]
        public void SerializeValidateRejectsInvalidJson()
        {
            var converter = new JsonCustomConvert<int>(n => "{not json", true);

            var ex = Assert.Throws<JsonCustomConverterException>(() => converter.Serialize(1));
            ClassicAssert.AreSame(converter, ex.JsonCustomConverter);
            ClassicAssert.IsNotNull(ex.InnerException);

            //通过序列化器调用同样会抛出
            Assert.Throws<JsonCustomConverterException>(() => new JsonSerializer(1, converter).Serialize());
        }

        /// <summary>
        /// 验证开启 serializeValidate 且返回合法 JSON 时不会抛异常，并且委托只被调用一次。
        /// </summary>
        [Test]
        public void SerializeValidateAcceptsValidJsonAndCallsDelegateOnce()
        {
            int calls = 0;
            var converter = new JsonCustomConvert<int>(n => { calls++; return "{\"v\":" + n + "}"; }, true);

            ClassicAssert.AreEqual("{\"v\":3}", converter.Serialize(3));
            ClassicAssert.AreEqual(1, calls);
        }

        /// <summary>
        /// 验证未开启 serializeValidate 时不校验自定义序列化结果。
        /// </summary>
        [Test]
        public void SerializeWithoutValidateDoesNotCheckResult()
        {
            var converter = new JsonCustomConvert<int>(n => "{not json");

            ClassicAssert.AreEqual("{not json", converter.Serialize(1));
        }

        /// <summary>
        /// 验证默认构造的转换器使用内置序列化 / 反序列化。
        /// </summary>
        [Test]
        public void DefaultConverterUsesBuiltInConversion()
        {
            var converter = new JsonCustomConvert<Money>();

            ClassicAssert.AreEqual("{\"Amount\":12.5,\"Currency\":\"USD\"}", converter.Serialize(new Money { Amount = 12.5m, Currency = "USD" }));

            var money = converter.Deserialize(JsonConvert.Deserialize("{\"Amount\":12.5,\"Currency\":\"USD\"}"));
            ClassicAssert.AreEqual(12.5m, money.Amount);
            ClassicAssert.AreEqual("USD", money.Currency);

            var list = new JsonCustomConvert<List<int>>().Deserialize(JsonConvert.Deserialize("[1,2,3]"));
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, list);
        }

        #endregion

        #region 序列化集成

        /// <summary>
        /// 验证自定义序列化会作用于对象中自定义类型的属性。
        /// </summary>
        [Test]
        public void CustomSerializeAppliesToObjectProperties()
        {
            var converter = new JsonCustomConvert<Money>(m => "\"" + m.Amount.ToString(CultureInfo.InvariantCulture) + " " + m.Currency + "\"");
            var order = new Order { Price = new Money { Amount = 9.5m, Currency = "EUR" }, Color = Color.Green };

            ClassicAssert.AreEqual("{\"Price\":\"9.5 EUR\",\"Color\":1}", new JsonSerializer(order, converter).Serialize());
        }

        /// <summary>
        /// 验证自定义序列化会作用于基础类型（DateTime、int、string、枚举等）的属性。
        /// </summary>
        [Test]
        public void CustomSerializeAppliesToPrimitiveTypedProperties()
        {
            var dateConverter = new JsonCustomConvert<DateTime>(d => "\"" + d.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture) + "\"");
            var person = new Person { Id = 1, Name = "tom", Birthday = new DateTime(2020, 1, 2) };
            ClassicAssert.AreEqual("{\"Id\":1,\"Name\":\"tom\",\"Birthday\":\"2020/01/02\"}", new JsonSerializer(person, dateConverter).Serialize());

            var intConverter = new JsonCustomConvert<int>(n => (n * 10).ToString());
            ClassicAssert.AreEqual("{\"Id\":10,\"Name\":\"tom\",\"Birthday\":\"2020-01-02 00:00:00\"}", new JsonSerializer(person, intConverter).Serialize());

            var stringConverter = new JsonCustomConvert<string>((string t) => "\"<" + t + ">\"");
            ClassicAssert.AreEqual("{\"Id\":1,\"Name\":\"<tom>\",\"Birthday\":\"2020-01-02 00:00:00\"}", new JsonSerializer(person, stringConverter).Serialize());

            var colorConverter = new JsonCustomConvert<Color>(c => "\"" + c.ToString().ToLowerInvariant() + "\"");
            var order = new Order { Price = null, Color = Color.Green };
            ClassicAssert.AreEqual("{\"Price\":null,\"Color\":\"green\"}", new JsonSerializer(order, colorConverter).Serialize());
        }

        /// <summary>
        /// 验证没有转换器时基础类型属性仍使用内置序列化，且同一类型可交替使用有 / 无转换器的序列化器（编译缓存共享）。
        /// </summary>
        [Test]
        public void PrimitiveTypedPropertiesAlternateBetweenConvertersAndBuiltIn()
        {
            var person = new Person { Id = 1, Name = "tom", Birthday = new DateTime(2020, 1, 2) };
            var converter = new JsonCustomConvert<int>(n => "0");

            for (int i = 0; i < 3; i++)
            {
                ClassicAssert.AreEqual("{\"Id\":1,\"Name\":\"tom\",\"Birthday\":\"2020-01-02 00:00:00\"}", new JsonSerializer(person).Serialize());
                ClassicAssert.AreEqual("{\"Id\":0,\"Name\":\"tom\",\"Birthday\":\"2020-01-02 00:00:00\"}", new JsonSerializer(person, converter).Serialize());
            }
        }

        /// <summary>
        /// 验证枚举作为根对象或集合元素时可以序列化（内置和自定义转换器均可）。
        /// </summary>
        [Test]
        public void EnumSerializesAtRootAndInsideCollections()
        {
            ClassicAssert.AreEqual("1", new JsonSerializer(Color.Green).Serialize());
            ClassicAssert.AreEqual("[0,1]", new JsonSerializer(new List<Color> { Color.Red, Color.Green }).Serialize());

            var converter = new JsonCustomConvert<Color>(c => "\"" + c.ToString().ToLowerInvariant() + "\"");
            ClassicAssert.AreEqual("\"green\"", new JsonSerializer(Color.Green, converter).Serialize());
            ClassicAssert.AreEqual("[\"red\",\"green\"]", new JsonSerializer(new List<Color> { Color.Red, Color.Green }, converter).Serialize());
        }

        /// <summary>
        /// 验证值为 null 的引用类型属性输出 null，且不会调用该类型的转换器。
        /// </summary>
        [Test]
        public void NullPropertyDoesNotInvokeConverter()
        {
            int calls = 0;
            var converter = new JsonCustomConvert<string>((string t) => { calls++; return "\"x\""; });

            ClassicAssert.AreEqual("{\"Id\":0,\"Name\":null,\"Birthday\":\"0001-01-01 00:00:00\"}", new JsonSerializer(new Person(), converter).Serialize());
            ClassicAssert.AreEqual(0, calls);
        }

        /// <summary>
        /// 验证声明为接口类型（IEnumerable）且实际为 LINQ 迭代器的属性，在有 / 无转换器时都能正确序列化。
        /// </summary>
        [Test]
        public void InterfaceTypedEnumerablePropertySerializesWithConverters()
        {
            var holder = new LazyHolder { Items = new[] { 1, 2 }.Select(x => x * 2) };

            ClassicAssert.AreEqual("{\"Items\":[2,4]}", new JsonSerializer(holder).Serialize());
            ClassicAssert.AreEqual("{\"Items\":[2,4]}", new JsonSerializer(holder, new JsonCustomConvert<Guid>((Guid g) => "\"g\"")).Serialize());
            ClassicAssert.AreEqual("{\"Items\":[20,40]}", new JsonSerializer(holder, new JsonCustomConvert<int>((int n) => (n * 10).ToString())).Serialize());
        }

        /// <summary>
        /// 验证与序列化无关的转换器（只自定义反序列化 / 默认转换器）不改变输出。
        /// </summary>
        [Test]
        public void NonSerializeConvertersKeepBuiltInOutput()
        {
            var person = new Person { Id = 1, Name = "tom", Birthday = new DateTime(2020, 1, 2) };
            const string expected = "{\"Id\":1,\"Name\":\"tom\",\"Birthday\":\"2020-01-02 00:00:00\"}";

            ClassicAssert.AreEqual(expected, new JsonSerializer(person, new JsonCustomConvert<int>(j => 99)).Serialize());
            ClassicAssert.AreEqual(expected, new JsonSerializer(person, new JsonCustomConvert<int>()).Serialize());
        }

        /// <summary>
        /// 验证枚举在 object 类型属性、字典值中也能序列化（不依赖转换器）。
        /// </summary>
        [Test]
        public void EnumSerializesInObjectPropertyAndDictionaryValue()
        {
            ClassicAssert.AreEqual("{\"Value\":1}", new JsonSerializer(new ObjectHolder { Value = Color.Green }).Serialize());
            ClassicAssert.AreEqual("{\"a\":1}", new JsonSerializer(new Dictionary<string, Color> { { "a", Color.Green } }).Serialize());
        }

        /// <summary>
        /// 验证集合 / 字典类型本身的转换器也会作用于对象中该类型的属性。
        /// </summary>
        [Test]
        public void CustomSerializeAppliesToCollectionTypedProperties()
        {
            var listConverter = new JsonCustomConvert<List<int>>(list => "\"count=" + list.Count + "\"");
            var holder = new ContainerHolder { Numbers = new List<int> { 1, 2 }, Optional = 3 };

            ClassicAssert.AreEqual("{\"Numbers\":\"count=2\",\"Optional\":3,\"Missing\":null}", new JsonSerializer(holder, listConverter).Serialize());
        }

        /// <summary>
        /// 验证自定义序列化会作用于集合、字典值和 Nullable 的内部元素。
        /// </summary>
        [Test]
        public void CustomSerializeAppliesInsideContainers()
        {
            var converter = new JsonCustomConvert<int>(n => (n * 10).ToString());

            ClassicAssert.AreEqual("[10,20]", new JsonSerializer(new List<int> { 1, 2 }, converter).Serialize());
            ClassicAssert.AreEqual("{\"a\":10}", new JsonSerializer(new Dictionary<string, int> { { "a", 1 } }, converter).Serialize());

            var holder = new ContainerHolder { Numbers = new List<int> { 1, 2 }, Optional = 3, Missing = null };
            ClassicAssert.AreEqual("{\"Numbers\":[10,20],\"Optional\":30,\"Missing\":null}", new JsonSerializer(holder, converter).Serialize());
        }

        /// <summary>
        /// 验证自定义序列化可以作用于集合 / 字典 / 根节点中的自定义类型。
        /// </summary>
        [Test]
        public void CustomSerializeAppliesToObjectTypeEverywhere()
        {
            var converter = new JsonCustomConvert<Money>(m => "\"" + m.Currency + "\"");
            var money = new Money { Amount = 1m, Currency = "USD" };

            ClassicAssert.AreEqual("\"USD\"", new JsonSerializer(money, converter).Serialize());
            ClassicAssert.AreEqual("[\"USD\"]", new JsonSerializer(new List<Money> { money }, converter).Serialize());
            ClassicAssert.AreEqual("{\"k\":\"USD\"}", new JsonSerializer(new Dictionary<string, Money> { { "k", money } }, converter).Serialize());
        }

        /// <summary>
        /// 验证只自定义反序列化的转换器不会影响序列化。
        /// </summary>
        [Test]
        public void DeserializeOnlyConverterDoesNotAffectSerialize()
        {
            var converter = new JsonCustomConvert<int>(j => 99);

            ClassicAssert.AreEqual("5", new JsonSerializer(5, converter).Serialize());
        }

        /// <summary>
        /// 验证默认转换器（CustomItem 为 None）不会影响序列化。
        /// </summary>
        [Test]
        public void DefaultConverterDoesNotAffectSerialize()
        {
            ClassicAssert.AreEqual("5", new JsonSerializer(5, new JsonCustomConvert<int>()).Serialize());
        }

        /// <summary>
        /// 验证同一类型注册多个转换器时，后注册的生效。
        /// </summary>
        [Test]
        public void LastSerializeConverterWinsForSameType()
        {
            var first = new JsonCustomConvert<int>(n => "1");
            var second = new JsonCustomConvert<int>(n => "2");

            ClassicAssert.AreEqual("2", new JsonSerializer(7, first, second).Serialize());
        }

        /// <summary>
        /// 验证转换器只作用于自己的类型，其他类型不受影响。
        /// </summary>
        [Test]
        public void ConverterOnlyAppliesToItsOwnType()
        {
            var converter = new JsonCustomConvert<Money>(m => "0");
            var order = new Order { Price = new Money { Amount = 9.5m, Currency = "EUR" }, Color = Color.Green };
            var person = new Person { Id = 1, Name = "tom", Birthday = new DateTime(2020, 1, 2) };

            ClassicAssert.AreEqual("{\"Price\":0,\"Color\":1}", new JsonSerializer(order, converter).Serialize());
            ClassicAssert.AreEqual("{\"Id\":1,\"Name\":\"tom\",\"Birthday\":\"2020-01-02 00:00:00\"}", new JsonSerializer(person, converter).Serialize());
        }

        #endregion

        #region 反序列化集成

        /// <summary>
        /// 验证自定义反序列化会作用于对象的属性（DateTime 自定义格式）。
        /// </summary>
        [Test]
        public void CustomDeserializeAppliesToProperties()
        {
            var converter = new JsonCustomConvert<DateTime>(j => DateTime.ParseExact(j.Value.ToString(), "yyyy/MM/dd", CultureInfo.InvariantCulture));

            var person = JsonConvert.Deserialize<Person>("{\"Id\":1,\"Name\":\"tom\",\"Birthday\":\"2020/01/02\"}", converter);

            ClassicAssert.AreEqual(1, person.Id);
            ClassicAssert.AreEqual("tom", person.Name);
            ClassicAssert.AreEqual(new DateTime(2020, 1, 2), person.Birthday);
        }

        /// <summary>
        /// 验证自定义反序列化可以读取对象节点，并转换成自定义类型。
        /// </summary>
        [Test]
        public void CustomDeserializeReceivesObjectNode()
        {
            IJsonObject received = null;
            var converter = new JsonCustomConvert<Money>(j =>
            {
                received = j;
                return new Money { Amount = decimal.Parse(j["amount"].Value.ToString(), CultureInfo.InvariantCulture), Currency = "CNY" };
            });

            var order = JsonConvert.Deserialize<Order>("{\"Price\":{\"amount\":3.25},\"Color\":1}", converter);

            ClassicAssert.IsNotNull(received);
            ClassicAssert.AreEqual(JsonType.Content, received.Type);
            ClassicAssert.AreEqual(3.25m, order.Price.Amount);
            ClassicAssert.AreEqual("CNY", order.Price.Currency);
            ClassicAssert.AreEqual(Color.Green, order.Color);
        }

        /// <summary>
        /// 验证自定义反序列化会作用于根节点、集合、字典和 Nullable 的内部元素。
        /// </summary>
        [Test]
        public void CustomDeserializeAppliesInsideContainers()
        {
            var converter = new JsonCustomConvert<int>(j => int.Parse(j.Value.ToString()) * 10);

            ClassicAssert.AreEqual(50, JsonConvert.Deserialize<int>("5", converter));
            CollectionAssert.AreEqual(new[] { 10, 20 }, JsonConvert.Deserialize<List<int>>("[1,2]", converter));
            CollectionAssert.AreEqual(new[] { 10, 20 }, JsonConvert.Deserialize<int[]>("[1,2]", converter));
            ClassicAssert.AreEqual(10, JsonConvert.Deserialize<Dictionary<string, int>>("{\"a\":1}", converter)["a"]);
            ClassicAssert.AreEqual(30, JsonConvert.Deserialize<int?>("3", converter).Value);

            var holder = JsonConvert.Deserialize<ContainerHolder>("{\"Numbers\":[1,2],\"Optional\":3,\"Missing\":null}", converter);
            CollectionAssert.AreEqual(new[] { 10, 20 }, holder.Numbers);
            ClassicAssert.AreEqual(30, holder.Optional);
        }

        /// <summary>
        /// 验证只自定义序列化的转换器不会影响反序列化。
        /// </summary>
        [Test]
        public void SerializeOnlyConverterDoesNotAffectDeserialize()
        {
            var converter = new JsonCustomConvert<int>(n => "0");

            ClassicAssert.AreEqual(5, JsonConvert.Deserialize<int>("5", converter));
        }

        /// <summary>
        /// 验证同一类型注册多个转换器时，后注册的生效。
        /// </summary>
        [Test]
        public void LastDeserializeConverterWinsForSameType()
        {
            var first = new JsonCustomConvert<int>(j => 1);
            var second = new JsonCustomConvert<int>(j => 2);

            ClassicAssert.AreEqual(2, JsonConvert.Deserialize<int>("7", first, second));
        }

        /// <summary>
        /// 验证未传转换器或传入 null / 空数组时走内置反序列化。
        /// </summary>
        [Test]
        public void NoConvertersFallBackToBuiltIn()
        {
            ClassicAssert.AreEqual(7, JsonConvert.Deserialize<int>("7"));
            ClassicAssert.AreEqual(7, JsonConvert.Deserialize<int>("7", (IJsonCustomConverter[])null));
            ClassicAssert.AreEqual(7, JsonConvert.Deserialize<int>("7", new IJsonCustomConverter[0]));
        }

        /// <summary>
        /// 验证转换器可以与 JsonConvertOptions 同时使用。
        /// </summary>
        [Test]
        public void ConvertersWorkTogetherWithOptions()
        {
            var options = new JsonConvertOptions { CaseStyle = CaseStyle.SnakeCase };
            var converter = new JsonCustomConvert<int>(j => int.Parse(j.Value.ToString()) + 100);

            var holder = JsonConvert.Deserialize<ContainerHolder>("{\"numbers\":[1],\"optional\":2}", options, converter);

            CollectionAssert.AreEqual(new[] { 101 }, holder.Numbers);
            ClassicAssert.AreEqual(102, holder.Optional);
        }

        #endregion

        #region 往返

        /// <summary>
        /// 验证同时自定义序列化和反序列化时，对象可以往返。
        /// </summary>
        [Test]
        public void CustomConverterRoundTrip()
        {
            var converter = new JsonCustomConvert<Money>(
                m => "\"" + m.Amount.ToString(CultureInfo.InvariantCulture) + "|" + m.Currency + "\"",
                j =>
                {
                    var parts = j.Value.ToString().Split('|');
                    return new Money { Amount = decimal.Parse(parts[0], CultureInfo.InvariantCulture), Currency = parts[1] };
                });
            var order = new Order { Price = new Money { Amount = 19.99m, Currency = "USD" }, Color = Color.Green };

            var json = new JsonSerializer(order, converter).Serialize();
            ClassicAssert.AreEqual("{\"Price\":\"19.99|USD\",\"Color\":1}", json);

            var back = JsonConvert.Deserialize<Order>(json, converter);
            ClassicAssert.AreEqual(19.99m, back.Price.Amount);
            ClassicAssert.AreEqual("USD", back.Price.Currency);
            ClassicAssert.AreEqual(Color.Green, back.Color);
        }

        #endregion

        #region 测试模型

        public enum Color
        {
            Red = 0,
            Green = 1
        }

        public class Money
        {
            public decimal Amount { get; set; }
            public string Currency { get; set; }
        }

        public class Person
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public DateTime Birthday { get; set; }
        }

        public class Order
        {
            public Money Price { get; set; }
            public Color Color { get; set; }
        }

        public class LazyHolder
        {
            public IEnumerable<int> Items { get; set; }
        }

        public class ObjectHolder
        {
            public object Value { get; set; }
        }

        public class ContainerHolder
        {
            public List<int> Numbers { get; set; }
            public int? Optional { get; set; }
            public int? Missing { get; set; }
        }

        #endregion
    }
}
