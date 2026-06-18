using System;
using System.Collections.Generic;
using System.Linq;

namespace LHZ.FastJson.Benchmark
{
    /// <summary>
    /// 小对象模型 — 基本值类型属性。
    /// </summary>
    public class SmallModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// 中等对象模型 — 十属性，含嵌套 SmallModel 和集合。
    /// </summary>
    public class MediumModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsActive { get; set; }
        public double Score { get; set; }
        public decimal Price { get; set; }
        public SmallModel Child { get; set; }
        public List<string> Tags { get; set; }
    }

    /// <summary>
    /// 含可空属性的对象模型。
    /// </summary>
    public class NullableModel
    {
        public int? Count { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public double? Rating { get; set; }
        public string Name { get; set; }
    }

    /// <summary>
    /// 含需转义字符的字符串模型。
    /// </summary>
    public class StringEscapeModel
    {
        public string Content { get; set; }
    }

    /// <summary>
    /// 枚举模型。
    /// </summary>
    public enum TestEnum
    {
        Alpha,
        Beta,
        Gamma,
        Delta
    }

    /// <summary>
    /// 含枚举属性的对象模型。
    /// </summary>
    public class EnumModel
    {
        public TestEnum Value { get; set; }
        public string Label { get; set; }
    }

    /// <summary>
    /// 构建基准测试中使用的测试数据。
    /// </summary>
    internal static class BenchmarkData
    {
        internal static SmallModel CreateSmallModel() => new SmallModel
        {
            Id = 42,
            Name = "Hello LHZ.FastJson",
            CreatedAt = new DateTime(2025, 6, 18, 10, 30, 0)
        };

        internal static MediumModel CreateMediumModel() => new MediumModel
        {
            Id = 100,
            Title = "Benchmark Test Object",
            Description = "A medium complexity object used for benchmarking JSON serialization and deserialization.",
            CreatedAt = new DateTime(2024, 1, 15, 8, 0, 0),
            UpdatedAt = new DateTime(2025, 6, 18, 12, 0, 0),
            IsActive = true,
            Score = 98.7654321,
            Price = 199.99m,
            Child = CreateSmallModel(),
            Tags = new List<string> { "benchmark", "json", "performance", "dotnet", "serialization" }
        };

        internal static NullableModel CreateNullableModel_WithValues() => new NullableModel
        {
            Count = 99,
            ExpiryDate = new DateTime(2026, 12, 31),
            Rating = 4.5,
            Name = "NotNull"
        };

        internal static NullableModel CreateNullableModel_WithNulls() => new NullableModel
        {
            Count = null,
            ExpiryDate = null,
            Rating = null,
            Name = null
        };

        internal static List<SmallModel> CreateLargeList() =>
            Enumerable.Range(1, 100).Select(i => new SmallModel
            {
                Id = i,
                Name = $"Item_{i}",
                CreatedAt = new DateTime(2025, 1, 1).AddDays(i)
            }).ToList();

        internal static Dictionary<string, int> CreateDictionary() => new Dictionary<string, int>
        {
            { "alpha", 1 }, { "beta", 2 }, { "gamma", 3 },
            { "delta", 4 }, { "epsilon", 5 }, { "zeta", 6 },
            { "eta", 7 }, { "theta", 8 }, { "iota", 9 }, { "kappa", 10 }
        };

        internal static StringEscapeModel CreateStringWithEscapes() => new StringEscapeModel
        {
            Content = "Line1\tColumn1\r\nLine2 \"quoted\" value \\ path\\to\\file\r\nLine3\tTabbed"
        };

        internal static EnumModel CreateEnumModel() => new EnumModel
        {
            Value = TestEnum.Gamma,
            Label = "TestEnum"
        };
    }
}
