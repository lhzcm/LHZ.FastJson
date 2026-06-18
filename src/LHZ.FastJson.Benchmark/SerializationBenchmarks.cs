using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using LHZ.FastJson.Json;

namespace LHZ.FastJson.Benchmark
{
    /// <summary>
    /// 序列化性能基准测试 — 三引擎对比 (LHZ.FastJson / Newtonsoft.Json / System.Text.Json)。
    /// </summary>
    [MemoryDiagnoser]
    [RankColumn]
    public class SerializationBenchmarks
    {
        private SmallModel _smallModel;
        private MediumModel _mediumModel;
        private List<SmallModel> _largeList;
        private Dictionary<string, int> _dictionary;
        private StringEscapeModel _stringWithEscapes;
        private NullableModel _nullableWithValues;
        private NullableModel _nullableWithNulls;
        private EnumModel _enumModel;

        [GlobalSetup]
        public void Setup()
        {
            _smallModel = BenchmarkData.CreateSmallModel();
            _mediumModel = BenchmarkData.CreateMediumModel();
            _largeList = BenchmarkData.CreateLargeList();
            _dictionary = BenchmarkData.CreateDictionary();
            _stringWithEscapes = BenchmarkData.CreateStringWithEscapes();
            _nullableWithValues = BenchmarkData.CreateNullableModel_WithValues();
            _nullableWithNulls = BenchmarkData.CreateNullableModel_WithNulls();
            _enumModel = BenchmarkData.CreateEnumModel();
        }

        // ──────────────── LHZ.FastJson ────────────────

        [BenchmarkCategory("SmallObject"), Benchmark(Baseline = true)]
        public string LHZ_FastJson_Serialize_SmallObject()
        {
            return JsonConvert.Serialize(_smallModel);
        }

        [BenchmarkCategory("MediumObject"), Benchmark]
        public string LHZ_FastJson_Serialize_MediumObject()
        {
            return JsonConvert.Serialize(_mediumModel);
        }

        [BenchmarkCategory("LargeList"), Benchmark]
        public string LHZ_FastJson_Serialize_LargeList()
        {
            return JsonConvert.Serialize(_largeList);
        }

        [BenchmarkCategory("Dictionary"), Benchmark]
        public string LHZ_FastJson_Serialize_Dictionary()
        {
            return JsonConvert.Serialize(_dictionary);
        }

        [BenchmarkCategory("StringEscapes"), Benchmark]
        public string LHZ_FastJson_Serialize_StringWithEscapes()
        {
            return JsonConvert.Serialize(_stringWithEscapes);
        }

        [BenchmarkCategory("Nullable_WithValues"), Benchmark]
        public string LHZ_FastJson_Serialize_NullableWithValues()
        {
            return JsonConvert.Serialize(_nullableWithValues);
        }

        [BenchmarkCategory("Nullable_WithNulls"), Benchmark]
        public string LHZ_FastJson_Serialize_NullableWithNulls()
        {
            return JsonConvert.Serialize(_nullableWithNulls);
        }

        [BenchmarkCategory("Enum"), Benchmark]
        public string LHZ_FastJson_Serialize_Enum()
        {
            return JsonConvert.Serialize(_enumModel);
        }

        // ──────────────── Newtonsoft.Json ────────────────

        [BenchmarkCategory("SmallObject"), Benchmark]
        public string Newtonsoft_Serialize_SmallObject()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(_smallModel);
        }

        [BenchmarkCategory("MediumObject"), Benchmark]
        public string Newtonsoft_Serialize_MediumObject()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(_mediumModel);
        }

        [BenchmarkCategory("LargeList"), Benchmark]
        public string Newtonsoft_Serialize_LargeList()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(_largeList);
        }

        [BenchmarkCategory("Dictionary"), Benchmark]
        public string Newtonsoft_Serialize_Dictionary()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(_dictionary);
        }

        [BenchmarkCategory("StringEscapes"), Benchmark]
        public string Newtonsoft_Serialize_StringWithEscapes()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(_stringWithEscapes);
        }

        [BenchmarkCategory("Nullable_WithValues"), Benchmark]
        public string Newtonsoft_Serialize_NullableWithValues()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(_nullableWithValues);
        }

        [BenchmarkCategory("Nullable_WithNulls"), Benchmark]
        public string Newtonsoft_Serialize_NullableWithNulls()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(_nullableWithNulls);
        }

        [BenchmarkCategory("Enum"), Benchmark]
        public string Newtonsoft_Serialize_Enum()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(_enumModel);
        }

        // ──────────────── System.Text.Json ────────────────

        [BenchmarkCategory("SmallObject"), Benchmark]
        public string SystemTextJson_Serialize_SmallObject()
        {
            return System.Text.Json.JsonSerializer.Serialize(_smallModel);
        }

        [BenchmarkCategory("MediumObject"), Benchmark]
        public string SystemTextJson_Serialize_MediumObject()
        {
            return System.Text.Json.JsonSerializer.Serialize(_mediumModel);
        }

        [BenchmarkCategory("LargeList"), Benchmark]
        public string SystemTextJson_Serialize_LargeList()
        {
            return System.Text.Json.JsonSerializer.Serialize(_largeList);
        }

        [BenchmarkCategory("Dictionary"), Benchmark]
        public string SystemTextJson_Serialize_Dictionary()
        {
            return System.Text.Json.JsonSerializer.Serialize(_dictionary);
        }

        [BenchmarkCategory("StringEscapes"), Benchmark]
        public string SystemTextJson_Serialize_StringWithEscapes()
        {
            return System.Text.Json.JsonSerializer.Serialize(_stringWithEscapes);
        }

        [BenchmarkCategory("Nullable_WithValues"), Benchmark]
        public string SystemTextJson_Serialize_NullableWithValues()
        {
            return System.Text.Json.JsonSerializer.Serialize(_nullableWithValues);
        }

        [BenchmarkCategory("Nullable_WithNulls"), Benchmark]
        public string SystemTextJson_Serialize_NullableWithNulls()
        {
            return System.Text.Json.JsonSerializer.Serialize(_nullableWithNulls);
        }

        [BenchmarkCategory("Enum"), Benchmark]
        public string SystemTextJson_Serialize_Enum()
        {
            return System.Text.Json.JsonSerializer.Serialize(_enumModel);
        }
    }
}
