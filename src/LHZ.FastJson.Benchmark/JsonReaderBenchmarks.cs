using BenchmarkDotNet.Attributes;
using LHZ.FastJson.JsonClass;

namespace LHZ.FastJson.Benchmark
{
    /// <summary>
    /// JsonReader 解析性能基准 —— 直接测试 ReadStringLiteral 批量追加优化的效果。
    /// </summary>
    [MemoryDiagnoser]
    [RankColumn]
    public class JsonReaderBenchmarks
    {
        private string _simpleStringJson;
        private string _escapedStringJson;
        private string _smallObjectJson;
        private string _largeStringArrayJson;

        [GlobalSetup]
        public void Setup()
        {
            // 简单字符串 JSON token
            _simpleStringJson = "\"Hello World, this is a simple string without any escape sequences\"";

            // 含大量转义字符的字符串
            _escapedStringJson = "\"Line1\\tColumn1\\r\\nLine2\\tColumn2\\r\\nLine3\\tColumn3\\r\\nLine4 \\\"quoted\\\" value \\\\ path\\\\to\\\\file\\r\\nLine5\\tTabbed\"";

            // 小对象 JSON（包含属性名 + 字符串值，属性名和字符串值都走 ReadStringLiteral）
            _smallObjectJson = "{\"Id\":42,\"Name\":\"Hello LHZ.FastJson\",\"Description\":\"A simple object with a few string properties\"}";

            // 大字符串数组 —— 100个含转义字符的字符串元素
            var items = new System.Text.StringBuilder("[");
            for (int i = 0; i < 100; i++)
            {
                if (i > 0) items.Append(',');
                items.Append("\"Item_").Append(i).Append("\\tValue_").Append(i).Append("\\r\\n\"");
            }
            items.Append(']');
            _largeStringArrayJson = items.ToString();
        }

        /// <summary>
        /// 解析简单字符串 —— 无转义字符，批量追加收益最大。
        /// </summary>
        [BenchmarkCategory("ReadString"), Benchmark(Baseline = true)]
        public JsonObject ReadSimpleString()
        {
            var reader = new JsonReader(_simpleStringJson);
            return reader.JsonRead();
        }

        /// <summary>
        /// 解析含转义字符的字符串 —— 转义中断批量追加，收益降低。
        /// </summary>
        [BenchmarkCategory("ReadString"), Benchmark]
        public JsonObject ReadEscapedString()
        {
            var reader = new JsonReader(_escapedStringJson);
            return reader.JsonRead();
        }

        /// <summary>
        /// 解析小对象 —— 属性名和字符串值都经过 ReadStringLiteral。
        /// </summary>
        [BenchmarkCategory("ReadObject"), Benchmark]
        public JsonObject ReadSmallObject()
        {
            var reader = new JsonReader(_smallObjectJson);
            return reader.JsonRead();
        }

        /// <summary>
        /// 解析含 100 个转义字符串的数组 —— 大量 ReadStringLiteral 调用。
        /// </summary>
        [BenchmarkCategory("ReadArray"), Benchmark]
        public JsonObject ReadLargeStringArray()
        {
            var reader = new JsonReader(_largeStringArrayJson);
            return reader.JsonRead();
        }
    }
}
