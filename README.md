# LHZ.FastJson

A lightweight and high-performance JSON serialization and deserialization library for .NET.

[中文](README_CN.md) | [Changelog](CHANGELOG.md)

## Installation

The following examples show different installation methods using [LHZ.FastJson 3.0.0](https://www.nuget.org/packages/LHZ.FastJson/3.0.0) as an example:

### Package Manager

``` bash
Install-Package LHZ.FastJson -Version 3.0.0
```

### .NET CLI

``` bash
dotnet add package LHZ.FastJson --version 3.0.0
```

### Package Reference

``` xml
<PackageReference Include="LHZ.FastJson" Version="3.0.0" />
```

### Paket CLI

``` bash
paket add LHZ.FastJson --version 3.0.0
```

## Usage

### Serialization with LHZ.FastJson

#### Example

``` csharp
Student student = new Student
{
    NO = 1,
    [JsonProperty("studentName")]
    Name = "lhz",
    Age = 18,
    Brithday = new DateTime(2002, 1, 1)
};

string jsonStr = LHZ.FastJson.JsonConvert.Serialize(student);

Console.WriteLine(jsonStr);
```

#### Output

``` bash
PS C:\Users\admin\source\repos\LHZ.FastJson\LHZ.FastJson.Test> dotnet run
{"NO":1,"studentName":"lhz","Age":18,"Brithday":"2000/1/1 0:00:00"}
```

### Deserialization with LHZ.FastJson

#### Example

``` csharp
string str = "{\"NO\":1,\"studentName\":\"lhz\",\"Age\":18,\"Brithday\":\"2000/1/1 0:00:00\"}";

Student student = JsonConvert.Deserialize<Student>(str);

Console.WriteLine("NO:{0},Name:{1},Age:{2},Brithday:{3}", student.NO, student.Name, student.Age, student.Brithday.ToString("yyyy-MM-dd"));
```

#### Output

``` bash
PS C:\Users\admin\source\repos\LHZ.FastJson\LHZ.FastJson.Test> dotnet run
NO:1,Name:lhz,Age:18,Brithday:2000-1-1
```

### Naming Style (`CaseStyle`)

By default, property names are serialized exactly as declared (`CaseStyle.PascalCase`). You can choose a naming style per call through `JsonConvertOptions.CaseStyle`:

| `CaseStyle` | `UserName` | `URLValue` |
|-------------|------------|------------|
| `PascalCase` (default, names as declared) | `UserName` | `URLValue` |
| `CamelCase` | `userName` | `urlValue` |
| `SnakeCase` | `user_name` | `url_value` |
| `KebabCase` | `user-name` | `url-value` |

- During serialization, property names are converted to the selected style; during deserialization, names in that style are matched automatically;
- Properties marked with the `[JsonProperty("xxx")]` attribute always keep the explicitly configured name;
- Only object property names are converted; dictionary keys are left untouched;
- The options only affect the call they are passed to; there is no global state, so different callers can use different naming styles at the same time;
- `JsonConvertOptions.MaxDepth` (default 64) limits the nesting depth accepted by the readers.
- `JsonConvertOptions.DateTimeFormat` (default `null`) sets a custom `DateTime` format. By default `DateTime` is written in the ISO 8601 round-trip form (`"2024-01-02T03:04:05.678Z"`); ISO 8601 and `yyyy-MM-dd HH:mm:ss` are always accepted when reading.

#### Example

``` csharp
public class UserInfo
{
    public string UserName { get; set; }
    public int UserAge { get; set; }
}

var options = new JsonConvertOptions { CaseStyle = CaseStyle.SnakeCase };

// Serialization: property names are converted to snake_case
string jsonStr = JsonConvert.Serialize(new UserInfo { UserName = "lhz", UserAge = 18 }, options);
// Output: {"user_name":"lhz","user_age":18}

// Deserialization: snake_case property names are matched automatically
UserInfo user = JsonConvert.Deserialize<UserInfo>("{\"user_name\":\"lhz\",\"user_age\":18}", options);
```

### Building JSON Trees Programmatically

In addition to parsing JSON from strings, you can construct JSON trees directly using the `JsonContent`, `JsonArray`, `JsonString`, `JsonNumber`, `JsonBoolean`, and `JsonNull` classes:

``` csharp
//Object
var jsonContent = new JsonContent();
//Add String
jsonContent.AddJsonProperty("Name", new JsonString("LHZ.FastJson"));
//Add Number
jsonContent.AddJsonProperty("Size", new JsonNumber(1024));
//Add Boolean
jsonContent.AddJsonProperty("IsRelease", JsonBoolean.True);
//Add Null
jsonContent.AddJsonProperty("Exat", JsonNull.Null);
//Add Array
var jsonVersionArray = new JsonArray();
jsonVersionArray.AddJsonObject(new JsonString("2.0.0"));
jsonVersionArray.AddJsonObject(new JsonString("1.8.5"));
jsonVersionArray.AddJsonObject(new JsonString("1.8.4"));
jsonVersionArray.AddJsonObject(new JsonString("1.8.3"));
jsonContent.AddJsonProperty("Versions", jsonVersionArray);
//To Json String
var json = jsonContent.ToString();
```
OutPut
``` powershell
{"Name":"LHZ.FastJson","Size":1024,"IsRelease":true,"Exat":null,"Versions":["2.0.0","1.8.5","1.8.4","1.8.3"]}
```

### JSON Parsing with `JsonReader`

`JsonReader` parses a JSON string into an `IJsonObject` tree that you can traverse dynamically.

#### Basic Parsing

``` csharp
using LHZ.FastJson;

string json = @"{""name"":""Alice"",""age"":25,""items"":[1,2,3]}";

var reader = new JsonReader(json);
IJsonObject obj = reader.JsonRead();

// Access parsed data
Console.WriteLine(obj["name"].Value);    // Alice
Console.WriteLine(obj["age"].Value);     // 25
Console.WriteLine(obj["items"][0].Value); // 1
```

#### Validate JSON

``` csharp
string json = @"{""key"":""value""}";

var reader = new JsonReader(json);
bool isValid = reader.IsValidJson;  // true

string invalidJson = @"{""key"":}";
var reader2 = new JsonReader(invalidJson);
bool isValid2 = reader2.IsValidJson; // false
```

#### Static Validation

``` csharp
bool isJson = JsonReader.IsJsonString(jsonString, out Exception exception);
if (!isJson)
{
    Console.WriteLine($"Invalid JSON: {exception.Message}");
}
```

#### Parsed JSON Type Tree

| JSON Value | `IJsonObject` Type | `.Value` Type |
|------------|--------------------|---------------|
| `{"a":1}` | `JsonContent` | `Dictionary<JsonPropertyName, IJsonObject>` |
| `[1,2]` | `JsonArray` | `List<IJsonObject>` |
| `"text"` | `JsonString` | `string` |
| `123` | `JsonNumber` | `IConvertible` (StringView) |
| `true` / `false` | `JsonBoolean` | `bool` |
| `null` | `JsonNull` | `null` |

## Performance

Benchmarks compare LHZ.FastJson with Newtonsoft.Json 13.0.3 and System.Text.Json (built into .NET 10) using [BenchmarkDotNet](https://benchmarkdotnet.org/). The source is in [`src/LHZ.FastJson.Benchmark`](src/LHZ.FastJson.Benchmark); run it with:

``` shell
cd src/LHZ.FastJson.Benchmark
dotnet run -c Release -- --filter "*"
```

``` ini
BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.9974/25H2/2025Update/HudsonValley2)
Intel Core Ultra 9 285H 2.90GHz, 1 CPU, 16 logical and 16 physical cores
.NET SDK 10.0.404
  [Host]     : .NET 10.0.15 (10.0.15, 10.0.1526.53008), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.15 (10.0.15, 10.0.1526.53008), X64 RyuJIT x86-64-v3
```

Each cell shows the mean time with the ratio to LHZ.FastJson in parentheses (below `1.00` is faster than LHZ.FastJson). Test data: `SmallObject` / `MediumObject` are plain POCOs, `LargeList` is a `List<SmallModel>`, `Dictionary` is `Dictionary<string, int>`, `StringEscapes` is a string containing characters that must be escaped, and `Nullable_*` / `Enum` are models with nullable and enum properties.

> **Reading the numbers:** results fluctuate by roughly 10-20% between runs on a desktop machine. Treat differences against System.Text.Json within that range as a tie. The consistent findings are that LHZ.FastJson is about **1.2x-3x faster than Newtonsoft.Json** in every scenario, and allocates more than System.Text.Json.

### Serialization

| Scenario            | LHZ.FastJson | Newtonsoft.Json   | System.Text.Json  |
|---------------------|-------------:|------------------:|------------------:|
| SmallObject         |    108.4 ns  |  225.9 ns (2.08)  |  134.3 ns (1.24)  |
| MediumObject        |    537.4 ns  |  776.0 ns (1.44)  |  426.5 ns (0.79)  |
| LargeList           |     6.00 μs  |  12.40 μs (2.08)  |   6.22 μs (1.04)  |
| Dictionary          |    276.1 ns  |  397.7 ns (1.44)  |  204.4 ns (0.74)  |
| StringEscapes       |    120.8 ns  |  258.0 ns (2.14)  |  187.2 ns (1.55)  |
| Nullable_WithValues |    197.8 ns  |  335.3 ns (1.71)  |  176.9 ns (0.90)  |
| Nullable_WithNulls  |     61.6 ns  |  181.7 ns (2.95)  |   70.8 ns (1.15)  |
| Enum                |     65.8 ns  |  147.5 ns (2.27)  |   85.2 ns (1.31)  |

Allocated per operation:

| Scenario            | LHZ.FastJson | Newtonsoft.Json | System.Text.Json |
|---------------------|-------------:|----------------:|-----------------:|
| SmallObject         |        744 B |          1512 B |            168 B |
| MediumObject        |       2464 B |          3208 B |           1160 B |
| LargeList           |      29368 B |         34712 B |          12504 B |
| Dictionary          |       1000 B |          1816 B |            224 B |
| StringEscapes       |        680 B |          1496 B |            216 B |
| Nullable_WithValues |        856 B |          1576 B |            176 B |
| Nullable_WithNulls  |        624 B |          1344 B |            144 B |
| Enum                |        592 B |          1408 B |             88 B |

### Deserialization

| Scenario            | LHZ.FastJson | Newtonsoft.Json   | System.Text.Json  |
|---------------------|-------------:|------------------:|------------------:|
| SmallObject         |    143.3 ns  |  286.8 ns (2.00)  |  119.9 ns (0.84)  |
| MediumObject        |    858.3 ns  | 1668.8 ns (1.95)  |  845.6 ns (0.99)  |
| LargeList           |    13.57 μs  |  25.32 μs (1.87)  |  15.21 μs (1.12)  |
| Dictionary          |    512.3 ns  |  598.0 ns (1.17)  |  533.6 ns (1.04)  |
| StringEscapes       |    185.6 ns  |  359.6 ns (1.94)  |  235.2 ns (1.27)  |
| Nullable_WithValues |    200.3 ns  |  438.1 ns (2.19)  |  206.0 ns (1.03)  |
| Nullable_WithNulls  |    120.3 ns  |  331.0 ns (2.76)  |  142.7 ns (1.19)  |
| Enum                |     95.2 ns  |  292.4 ns (3.07)  |  115.3 ns (1.21)  |

Allocated per operation:

| Scenario            | LHZ.FastJson | Newtonsoft.Json | System.Text.Json |
|---------------------|-------------:|----------------:|-----------------:|
| SmallObject         |        248 B |          2760 B |            104 B |
| MediumObject        |       1216 B |          3880 B |           1320 B |
| LargeList           |      19808 B |         19936 B |          10664 B |
| Dictionary          |       1920 B |          3952 B |           1320 B |
| StringEscapes       |        632 B |          4832 B |            176 B |
| Nullable_WithValues |        280 B |          2888 B |            104 B |
| Nullable_WithNulls  |        176 B |          2648 B |             64 B |
| Enum                |        256 B |          2704 B |             72 B |

### Version history

LHZ.FastJson was re-architected in 1.6.0, achieving nearly **2x** deserialization performance compared with earlier versions. These numbers were measured on older hardware and .NET 6 and are not comparable with the tables above:

``` ini
BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19044.1889 (21H2)
Intel Core i7-9700K CPU 3.60GHz (Coffee Lake), 1 CPU, 8 logical and 8 physical cores
.NET SDK=6.0.302
  [Host]     : .NET 6.0.7 (6.0.722.32202), X64 RyuJIT
  DefaultJob : .NET 6.0.7 (6.0.722.32202), X64 RyuJIT
```

|              Method |     Mean |    Error |   StdDev |
|-------------------- |---------:|---------:|---------:|
| LHZFastJson v1.6.0  | 43.91 ms | 0.181 ms | 0.170 ms |
| LHZFastJson v1.5.2  | 85.18 ms | 0.301 ms | 0.282 ms |
