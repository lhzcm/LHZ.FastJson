using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;

namespace LHZ.FastJson.Benchmark
{
    /// <summary>
    /// DateTime 序列化 / 反序列化性能基准测试。
    /// </summary>
    [MemoryDiagnoser]
    public class DateTimeBenchmarks
    {
        private List<DateTime> _dates;
        private string _isoJson;
        private string _legacyJson;

        [GlobalSetup]
        public void Setup()
        {
            _dates = new List<DateTime>(1000);
            var start = new DateTime(2025, 1, 1, 8, 30, 0, DateTimeKind.Utc);
            for (int i = 0; i < 1000; i++)
            {
                _dates.Add(start.AddMinutes(i * 37).AddMilliseconds(i));
            }
            // ISO 8601（含毫秒和 Z），由 Newtonsoft.Json 生成
            _isoJson = Newtonsoft.Json.JsonConvert.SerializeObject(_dates);
            // 旧版 LHZ.FastJson 默认格式 "yyyy-MM-dd HH:mm:ss"
            _legacyJson = Newtonsoft.Json.JsonConvert.SerializeObject(_dates, new Newtonsoft.Json.JsonSerializerSettings { DateFormatString = "yyyy-MM-dd HH:mm:ss" });
        }

        [Benchmark]
        public string LHZ_Serialize_DateTimeList()
        {
            return JsonConvert.Serialize(_dates);
        }

        [Benchmark]
        public List<DateTime> LHZ_Deserialize_IsoDateTimeList()
        {
            return JsonConvert.Deserialize<List<DateTime>>(_isoJson);
        }

        [Benchmark]
        public List<DateTime> LHZ_Deserialize_LegacyDateTimeList()
        {
            return JsonConvert.Deserialize<List<DateTime>>(_legacyJson);
        }
    }
}
