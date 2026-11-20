using System;
using System.Collections.Generic;
using System.Globalization;
using LHZ.FastJson.Exceptions;
using NUnit.Framework;

namespace LHZ.FastJson.UnitTest
{
    /// <summary>
    /// DateTime 序列化（ISO 8601 往返格式）与反序列化测试。
    /// </summary>
    public class TestJsonDateTime
    {
        public class DateModel
        {
            public DateTime D { get; set; }
            public DateTime? N { get; set; }
        }

        /// <summary>
        /// 验证不同 DateTimeKind 的默认输出格式。
        /// </summary>
        [Test]
        public void SerializeUsesIso8601RoundTripFormat()
        {
            ClassicAssert.AreEqual("\"2024-01-02T03:04:05\"", JsonConvert.Serialize(new DateTime(2024, 1, 2, 3, 4, 5)));
            ClassicAssert.AreEqual("\"2024-01-02T03:04:05.678Z\"", JsonConvert.Serialize(new DateTime(2024, 1, 2, 3, 4, 5, 678, DateTimeKind.Utc)));
            ClassicAssert.AreEqual("\"2024-01-02T03:04:05.1234567\"", JsonConvert.Serialize(new DateTime(2024, 1, 2, 3, 4, 5).AddTicks(1234567)));
            ClassicAssert.AreEqual("\"2024-01-02T03:04:05.00001\"", JsonConvert.Serialize(new DateTime(2024, 1, 2, 3, 4, 5).AddTicks(100)));
            ClassicAssert.AreEqual("\"0001-01-01T00:00:00\"", JsonConvert.Serialize(DateTime.MinValue));
            ClassicAssert.AreEqual("\"9999-12-31T23:59:59.9999999\"", JsonConvert.Serialize(DateTime.MaxValue));
        }

        /// <summary>
        /// 验证 Local 时间输出与 .NET "o" 格式一致（含本地时区偏移）。
        /// </summary>
        [Test]
        public void SerializeLocalMatchesRoundTripSpecifier()
        {
            var local = new DateTime(2024, 7, 1, 12, 30, 0, 250, DateTimeKind.Local);
            string expected = local.ToString("yyyy-MM-dd'T'HH:mm:ss.FFFFFFFK", CultureInfo.InvariantCulture);
            ClassicAssert.AreEqual("\"" + expected + "\"", JsonConvert.Serialize(local));
        }

        /// <summary>
        /// 验证序列化后再反序列化，值和 Kind 都不丢失。
        /// </summary>
        [Test]
        public void RoundTripKeepsTicksAndKind()
        {
            var values = new[]
            {
                new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Unspecified).AddTicks(1234567),
                new DateTime(2024, 1, 2, 3, 4, 5, 678, DateTimeKind.Utc),
                new DateTime(2024, 7, 1, 12, 30, 0, 250, DateTimeKind.Local),
                DateTime.MinValue,
                DateTime.MaxValue,
            };
            foreach (var value in values)
            {
                var back = JsonConvert.Deserialize<DateTime>(JsonConvert.Serialize(value));
                ClassicAssert.AreEqual(value.Ticks, back.Ticks, value.ToString("o"));
                ClassicAssert.AreEqual(value.Kind, back.Kind, value.ToString("o"));
            }
        }

        /// <summary>
        /// 验证对象属性和可空属性的往返。
        /// </summary>
        [Test]
        public void RoundTripProperties()
        {
            var model = new DateModel { D = new DateTime(2024, 1, 2, 3, 4, 5, 6, DateTimeKind.Utc), N = new DateTime(2020, 2, 29) };
            var json = JsonConvert.Serialize(model);
            ClassicAssert.AreEqual("{\"D\":\"2024-01-02T03:04:05.006Z\",\"N\":\"2020-02-29T00:00:00\"}", json);
            var back = JsonConvert.Deserialize<DateModel>(json);
            ClassicAssert.AreEqual(model.D, back.D);
            ClassicAssert.AreEqual(DateTimeKind.Utc, back.D.Kind);
            ClassicAssert.AreEqual(model.N, back.N);
            ClassicAssert.IsNull(JsonConvert.Deserialize<DateModel>("{\"D\":\"2024-01-02\",\"N\":null}").N);
        }

        /// <summary>
        /// 验证兼容旧格式和常见 ISO 8601 变体。
        /// </summary>
        [Test]
        public void DeserializeAcceptsCommonFormats()
        {
            ClassicAssert.AreEqual(new DateTime(2020, 7, 9, 12, 0, 0), JsonConvert.Deserialize<DateTime>("\"2020-07-09 12:00:00\""));
            ClassicAssert.AreEqual(new DateTime(2020, 7, 9), JsonConvert.Deserialize<DateTime>("\"2020-07-09\""));
            ClassicAssert.AreEqual(new DateTime(2020, 7, 9, 12, 30, 0), JsonConvert.Deserialize<DateTime>("\"2020-07-09T12:30\""));
            //More than 7 fractional digits are truncated
            ClassicAssert.AreEqual(new DateTime(2020, 7, 9).AddTicks(1234567), JsonConvert.Deserialize<DateTime>("\"2020-07-09T00:00:00.123456789\""));

            var utc = new DateTime(2020, 7, 9, 4, 0, 0, DateTimeKind.Utc);
            foreach (var text in new[] { "2020-07-09T12:00:00+08:00", "2020-07-09T12:00:00+0800", "2020-07-09T12:00:00+08", "2020-07-09T05:30:00+01:30", "2020-07-09T00:00:00-04:00" })
            {
                var parsed = JsonConvert.Deserialize<DateTime>("\"" + text + "\"");
                ClassicAssert.AreEqual(DateTimeKind.Local, parsed.Kind, text);
                ClassicAssert.AreEqual(utc, parsed.ToUniversalTime(), text);
            }
            ClassicAssert.AreEqual(DateTimeKind.Utc, JsonConvert.Deserialize<DateTime>("\"2020-07-09T04:00:00z\"").Kind);

            //Escaped characters go through the decoding path
            ClassicAssert.AreEqual(new DateTime(2020, 7, 9), JsonConvert.Deserialize<DateTime>("\"2020\\u002d07-09\""));
            //Non-ISO strings fall back to the invariant-culture parser
            ClassicAssert.AreEqual(new DateTime(2020, 7, 9), JsonConvert.Deserialize<DateTime>("\"07/09/2020\""));
        }

        /// <summary>
        /// 验证非法日期字符串抛出 JsonDirectDeserializationException。
        /// </summary>
        [Test]
        public void DeserializeRejectsInvalidDates()
        {
            foreach (var text in new[] { "", "abc", "2020-13-01", "2020-02-30", "2020-07-09T24:00:00", "2020-07-09T12:00:00+15:00", "2020-07-09T12:00:00.", "2020-07-09T12:00:00Zx" })
            {
                Assert.Throws<JsonDirectDeserializationException>(() => JsonConvert.Deserialize<DateTime>("\"" + text + "\""), text);
            }
            Assert.Throws<JsonDirectDeserializationException>(() => JsonConvert.Deserialize<DateTime>("123"));
            Assert.Throws<JsonReadException>(() => JsonConvert.Deserialize<DateTime>("\"2020-07-09"));
        }

        /// <summary>
        /// 验证 DateTimeFormat 选项同时作用于序列化和反序列化。
        /// </summary>
        [Test]
        public void CustomDateTimeFormatOption()
        {
            var options = new JsonConvertOptions { DateTimeFormat = "yyyy/MM/dd HH:mm" };
            var model = new DateModel { D = new DateTime(2024, 1, 2, 3, 4, 0), N = null };
            var json = JsonConvert.Serialize(model, options);
            ClassicAssert.AreEqual("{\"D\":\"2024/01/02 03:04\",\"N\":null}", json);
            ClassicAssert.AreEqual(model.D, JsonConvert.Deserialize<DateModel>(json, options).D);

            //Legacy format via option
            var legacy = new JsonConvertOptions { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" };
            ClassicAssert.AreEqual("\"2024-01-02 03:04:05\"", JsonConvert.Serialize(new DateTime(2024, 1, 2, 3, 4, 5, 999), legacy));

            //A format that the fallback parser would read differently (dd/MM)
            var dayFirst = new JsonConvertOptions { DateTimeFormat = "dd/MM/yyyy" };
            ClassicAssert.AreEqual(new DateTime(2020, 9, 7), JsonConvert.Deserialize<DateTime>("\"07/09/2020\"", dayFirst));
        }

        /// <summary>
        /// 验证结果与当前线程区域设置无关。
        /// </summary>
        [Test]
        public void IndependentOfCurrentCulture()
        {
            var saved = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("ar-SA");
                var value = new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc);
                var json = JsonConvert.Serialize(value);
                ClassicAssert.AreEqual("\"2024-01-02T03:04:05Z\"", json);
                ClassicAssert.AreEqual(value, JsonConvert.Deserialize<DateTime>(json));
                ClassicAssert.AreEqual(new DateTime(2020, 7, 9), JsonConvert.Deserialize<DateTime>("\"07/09/2020\""));
            }
            finally
            {
                CultureInfo.CurrentCulture = saved;
            }
        }

        /// <summary>
        /// 验证 DateTime 列表批量往返。
        /// </summary>
        [Test]
        public void RoundTripList()
        {
            var list = new List<DateTime>();
            var start = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            for (int i = 0; i < 500; i++)
            {
                list.Add(start.AddMinutes(i * 37).AddTicks(i * 7919));
            }
            var back = JsonConvert.Deserialize<List<DateTime>>(JsonConvert.Serialize(list));
            CollectionAssert.AreEqual(list, back);
        }
    }
}
