using System.Collections.Generic;
using BenchmarkDotNet.Attributes;

namespace LHZ.FastJson.Benchmark
{
    /// <summary>
    /// 反序列化性能基准测试 — 三引擎对比 (LHZ.FastJson / Newtonsoft.Json / System.Text.Json)。
    /// </summary>
    [MemoryDiagnoser]
    [RankColumn]
    public class DeserializationBenchmarks
    {
        private string _smallModelJson;
        private string _mediumModelJson;
        private string _largeListJson;
        private string _dictionaryJson;
        private string _stringWithEscapesJson;
        private string _nullableWithValuesJson;
        private string _nullableWithNullsJson;
        private string _enumModelJson;

        [GlobalSetup]
        public void Setup()
        {
            // 使用 Newtonsoft.Json 生成 JSON（ISO 8601 DateTime 格式），确保三个库都能解析
            _smallModelJson = Newtonsoft.Json.JsonConvert.SerializeObject(BenchmarkData.CreateSmallModel());
            _mediumModelJson = Newtonsoft.Json.JsonConvert.SerializeObject(BenchmarkData.CreateMediumModel());
            _largeListJson = Newtonsoft.Json.JsonConvert.SerializeObject(BenchmarkData.CreateLargeList());
            _dictionaryJson = Newtonsoft.Json.JsonConvert.SerializeObject(BenchmarkData.CreateDictionary());
            _stringWithEscapesJson = Newtonsoft.Json.JsonConvert.SerializeObject(BenchmarkData.CreateStringWithEscapes());
            _nullableWithValuesJson = Newtonsoft.Json.JsonConvert.SerializeObject(BenchmarkData.CreateNullableModel_WithValues());
            _nullableWithNullsJson = Newtonsoft.Json.JsonConvert.SerializeObject(BenchmarkData.CreateNullableModel_WithNulls());
            _enumModelJson = Newtonsoft.Json.JsonConvert.SerializeObject(BenchmarkData.CreateEnumModel());
        }

        // ──────────────── LHZ.FastJson ────────────────

        [BenchmarkCategory("SmallObject"), Benchmark(Baseline = true)]
        public SmallModel LHZ_FastJson_Deserialize_SmallObject()
        {
            return JsonConvert.Deserialize<SmallModel>(_smallModelJson);
        }

        [BenchmarkCategory("MediumObject"), Benchmark]
        public MediumModel LHZ_FastJson_Deserialize_MediumObject()
        {
            return JsonConvert.Deserialize<MediumModel>(_mediumModelJson);
        }

        [BenchmarkCategory("LargeList"), Benchmark]
        public List<SmallModel> LHZ_FastJson_Deserialize_LargeList()
        {
            return JsonConvert.Deserialize<List<SmallModel>>(_largeListJson);
        }

        [BenchmarkCategory("Dictionary"), Benchmark]
        public Dictionary<string, int> LHZ_FastJson_Deserialize_Dictionary()
        {
            return JsonConvert.Deserialize<Dictionary<string, int>>(_dictionaryJson);
        }

        [BenchmarkCategory("StringEscapes"), Benchmark]
        public StringEscapeModel LHZ_FastJson_Deserialize_StringWithEscapes()
        {
            return JsonConvert.Deserialize<StringEscapeModel>(_stringWithEscapesJson);
        }

        [BenchmarkCategory("Nullable_WithValues"), Benchmark]
        public NullableModel LHZ_FastJson_Deserialize_NullableWithValues()
        {
            return JsonConvert.Deserialize<NullableModel>(_nullableWithValuesJson);
        }

        [BenchmarkCategory("Nullable_WithNulls"), Benchmark]
        public NullableModel LHZ_FastJson_Deserialize_NullableWithNulls()
        {
            return JsonConvert.Deserialize<NullableModel>(_nullableWithNullsJson);
        }

        [BenchmarkCategory("Enum"), Benchmark]
        public EnumModel LHZ_FastJson_Deserialize_Enum()
        {
            return JsonConvert.Deserialize<EnumModel>(_enumModelJson);
        }

        // ──────────────── Newtonsoft.Json ────────────────

        [BenchmarkCategory("SmallObject"), Benchmark]
        public SmallModel Newtonsoft_Deserialize_SmallObject()
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<SmallModel>(_smallModelJson);
        }

        [BenchmarkCategory("MediumObject"), Benchmark]
        public MediumModel Newtonsoft_Deserialize_MediumObject()
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<MediumModel>(_mediumModelJson);
        }

        [BenchmarkCategory("LargeList"), Benchmark]
        public List<SmallModel> Newtonsoft_Deserialize_LargeList()
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<List<SmallModel>>(_largeListJson);
        }

        [BenchmarkCategory("Dictionary"), Benchmark]
        public Dictionary<string, int> Newtonsoft_Deserialize_Dictionary()
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, int>>(_dictionaryJson);
        }

        [BenchmarkCategory("StringEscapes"), Benchmark]
        public StringEscapeModel Newtonsoft_Deserialize_StringWithEscapes()
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<StringEscapeModel>(_stringWithEscapesJson);
        }

        [BenchmarkCategory("Nullable_WithValues"), Benchmark]
        public NullableModel Newtonsoft_Deserialize_NullableWithValues()
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<NullableModel>(_nullableWithValuesJson);
        }

        [BenchmarkCategory("Nullable_WithNulls"), Benchmark]
        public NullableModel Newtonsoft_Deserialize_NullableWithNulls()
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<NullableModel>(_nullableWithNullsJson);
        }

        [BenchmarkCategory("Enum"), Benchmark]
        public EnumModel Newtonsoft_Deserialize_Enum()
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<EnumModel>(_enumModelJson);
        }

        // ──────────────── System.Text.Json ────────────────

        [BenchmarkCategory("SmallObject"), Benchmark]
        public SmallModel SystemTextJson_Deserialize_SmallObject()
        {
            return System.Text.Json.JsonSerializer.Deserialize<SmallModel>(_smallModelJson);
        }

        [BenchmarkCategory("MediumObject"), Benchmark]
        public MediumModel SystemTextJson_Deserialize_MediumObject()
        {
            return System.Text.Json.JsonSerializer.Deserialize<MediumModel>(_mediumModelJson);
        }

        [BenchmarkCategory("LargeList"), Benchmark]
        public List<SmallModel> SystemTextJson_Deserialize_LargeList()
        {
            return System.Text.Json.JsonSerializer.Deserialize<List<SmallModel>>(_largeListJson);
        }

        [BenchmarkCategory("Dictionary"), Benchmark]
        public Dictionary<string, int> SystemTextJson_Deserialize_Dictionary()
        {
            return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(_dictionaryJson);
        }

        [BenchmarkCategory("StringEscapes"), Benchmark]
        public StringEscapeModel SystemTextJson_Deserialize_StringWithEscapes()
        {
            return System.Text.Json.JsonSerializer.Deserialize<StringEscapeModel>(_stringWithEscapesJson);
        }

        [BenchmarkCategory("Nullable_WithValues"), Benchmark]
        public NullableModel SystemTextJson_Deserialize_NullableWithValues()
        {
            return System.Text.Json.JsonSerializer.Deserialize<NullableModel>(_nullableWithValuesJson);
        }

        [BenchmarkCategory("Nullable_WithNulls"), Benchmark]
        public NullableModel SystemTextJson_Deserialize_NullableWithNulls()
        {
            return System.Text.Json.JsonSerializer.Deserialize<NullableModel>(_nullableWithNullsJson);
        }

        [BenchmarkCategory("Enum"), Benchmark]
        public EnumModel SystemTextJson_Deserialize_Enum()
        {
            return System.Text.Json.JsonSerializer.Deserialize<EnumModel>(_enumModelJson);
        }
    }
}
