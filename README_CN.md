# LHZ.FastJson
轻巧便利的Json序列化和反序列化工具 

[English](README.md) | [更新日志](CHANGELOG_CN.md)

# 如何安装 
### 下面展示不同的安装方法，以安装[LHZ.FastJson 2.1.2](https://www.nuget.org/packages/LHZ.FastJson/2.1.2)版本为例
### Package Manager
``` bash
Install-Package LHZ.FastJson -Version 2.1.2
```
### .NET CLI
``` bash
dotnet add package LHZ.FastJson --version 2.1.2
```

### package-reference
``` xml
<PackageReference Include="LHZ.FastJson" Version="2.1.2" />
```

### Paket CLI
``` bash
paket add LHZ.FastJson --version 2.1.2
```

# 如何使用
## 使用LHZ.FastJson进行序列化
### 序列化代码示例
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
### 运行结果
``` bash
PS C:\Users\admin\source\repos\LHZ.FastJson\LHZ.FastJson.Test> dotnet run
{"NO":1,"studentName":"lhz","Age":18,"Brithday":"2000/1/1 0:00:00"}
```

## 使用LHZ.FastJson进行反序列化
### 反序列化代码示例
``` csharp
string str = "{\"NO\":1,\"studentName\":\"lhz\",\"Age\":18,\"Brithday\":\"2000/1/1 0:00:00\"}";

Student student = JsonConvert.Deserialize<Student>(str);

Console.WriteLine("NO:{0},Name:{1},Age:{2},Brithday:{3}", student.NO, student.Name, student.Age, student.Brithday.ToString("yyyy-MM-dd"));
```

### 运行结果
``` bash
PS C:\Users\admin\source\repos\LHZ.FastJson\LHZ.FastJson.Test> dotnet run
NO:1,Name:lhz,Age:18,Brithday:2000-1-1
```

## 命名风格（`CaseStyle`）
默认情况下属性名按声明名原样输出（`CaseStyle.PascalCase`）。可以在每次调用时通过 `JsonConvertOptions.CaseStyle` 选择命名风格：

| `CaseStyle` | `UserName` | `URLValue` |
|-------------|------------|------------|
| `PascalCase`（默认，保持声明名） | `UserName` | `URLValue` |
| `CamelCase` | `userName` | `urlValue` |
| `SnakeCase` | `user_name` | `url_value` |
| `KebabCase` | `user-name` | `url-value` |

- 序列化时属性名会转换为所选风格，反序列化时自动匹配该风格的属性名；
- 带有 `[JsonProperty("xxx")]` 特性的属性始终优先使用特性指定的名称；
- 只转换对象的属性名，字典的 key 不受影响；
- 选项只对传入它的那次调用生效，不存在全局状态，不同调用方可以同时使用不同的命名风格；
- `JsonConvertOptions.MaxDepth`（默认 64）用于限制读取器接受的最大嵌套深度。
- `JsonConvertOptions.DateTimeFormat`（默认 `null`）用于自定义 `DateTime` 格式。默认按 ISO 8601 往返格式输出（`"2024-01-02T03:04:05.678Z"`）；读取时始终兼容 ISO 8601 与 `yyyy-MM-dd HH:mm:ss`。

### 命名风格示例
``` csharp
public class UserInfo
{
    public string UserName { get; set; }
    public int UserAge { get; set; }
}

var options = new JsonConvertOptions { CaseStyle = CaseStyle.SnakeCase };

//序列化：属性名自动转换为 snake_case
string jsonStr = JsonConvert.Serialize(new UserInfo { UserName = "lhz", UserAge = 18 }, options);
//输出：{"user_name":"lhz","user_age":18}

//反序列化：自动匹配 snake_case 属性名
UserInfo user = JsonConvert.Deserialize<UserInfo>("{\"user_name\":\"lhz\",\"user_age\":18}", options);
```

### 手动构建 JSON 树

除了从字符串解析 JSON，还可以直接使用 `JsonContent`、`JsonArray`、`JsonString`、`JsonNumber`、`JsonBoolean` 和 `JsonNull` 类手动构建 JSON 树：

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
输出
``` powershell
{"Name":"LHZ.FastJson","Size":1024,"IsRelease":true,"Exat":null,"Versions":["2.0.0","1.8.5","1.8.4","1.8.3"]}
```

## 使用 `JsonReader` 解析 JSON

`JsonReader`它将 JSON 字符串解析为 `IJsonObject` 树，支持动态遍历。

### 基本解析

``` csharp
using LHZ.FastJson;

string json = @"{""name"":""Alice"",""age"":25,""items"":[1,2,3]}";

var reader = new JsonReader(json);
IJsonObject obj = reader.JsonRead();

// 访问解析后的数据
Console.WriteLine(obj["name"].Value);    // Alice
Console.WriteLine(obj["age"].Value);     // 25
Console.WriteLine(obj["items"][0].Value); // 1
```

### 验证 JSON 有效性

``` csharp
string json = @"{""key"":""value""}";

var reader = new JsonReader(json);
bool isValid = reader.IsValidJson;  // true

string invalidJson = @"{""key"":}";
var reader2 = new JsonReader(invalidJson);
bool isValid2 = reader2.IsValidJson; // false
```

### 静态验证

``` csharp
bool isJson = JsonReader.IsJsonString(jsonString, out Exception exception);
if (!isJson)
{
    Console.WriteLine($"JSON 无效: {exception.Message}");
}
```

### 解析结果类型映射

| JSON 类型 | `IJsonObject` 类型 | `.Value` 类型 |
|------------|--------------------|---------------|
| `{"a":1}` | `JsonContent` | `Dictionary<JsonPropertyName, IJsonObject>` |
| `[1,2]` | `JsonArray` | `List<IJsonObject>` |
| `"text"` | `JsonString` | `string` |
| `123` | `JsonNumber` | `IConvertible` (StringView) |
| `true` / `false` | `JsonBoolean` | `bool` |
| `null` | `JsonNull` | `null` |

## 性能

基准测试使用 [BenchmarkDotNet](https://benchmarkdotnet.org/)，将 LHZ.FastJson 与 Newtonsoft.Json 13.0.3、System.Text.Json（.NET 10 内置）进行对比。测试源码位于 [`src/LHZ.FastJson.Benchmark`](src/LHZ.FastJson.Benchmark)，运行方式：

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

表格中每个单元格为平均耗时，括号内是相对 LHZ.FastJson 的倍数（小于 `1.00` 表示比 LHZ.FastJson 更快）。测试数据：`SmallObject` / `MediumObject` 为普通对象，`LargeList` 为 `List<SmallModel>`，`Dictionary` 为 `Dictionary<string, int>`，`StringEscapes` 为包含需要转义字符的字符串，`Nullable_*` / `Enum` 为包含可空类型和枚举属性的模型。

> **关于数据的说明：** 在桌面环境下，多次运行的结果会有大约 10%-20% 的波动，与 System.Text.Json 的差距在此范围内时可视为持平。稳定的结论是：各场景下 LHZ.FastJson 都比 Newtonsoft.Json 快约 **1.2 到 3 倍**，但内存分配量比 System.Text.Json 更多。

### 序列化

| 场景                | LHZ.FastJson | Newtonsoft.Json   | System.Text.Json  |
|---------------------|-------------:|------------------:|------------------:|
| SmallObject         |    108.4 ns  |  225.9 ns (2.08)  |  134.3 ns (1.24)  |
| MediumObject        |    537.4 ns  |  776.0 ns (1.44)  |  426.5 ns (0.79)  |
| LargeList           |     6.00 μs  |  12.40 μs (2.08)  |   6.22 μs (1.04)  |
| Dictionary          |    276.1 ns  |  397.7 ns (1.44)  |  204.4 ns (0.74)  |
| StringEscapes       |    120.8 ns  |  258.0 ns (2.14)  |  187.2 ns (1.55)  |
| Nullable_WithValues |    197.8 ns  |  335.3 ns (1.71)  |  176.9 ns (0.90)  |
| Nullable_WithNulls  |     61.6 ns  |  181.7 ns (2.95)  |   70.8 ns (1.15)  |
| Enum                |     65.8 ns  |  147.5 ns (2.27)  |   85.2 ns (1.31)  |

单次操作的内存分配：

| 场景                | LHZ.FastJson | Newtonsoft.Json | System.Text.Json |
|---------------------|-------------:|----------------:|-----------------:|
| SmallObject         |        744 B |          1512 B |            168 B |
| MediumObject        |       2464 B |          3208 B |           1160 B |
| LargeList           |      29368 B |         34712 B |          12504 B |
| Dictionary          |       1000 B |          1816 B |            224 B |
| StringEscapes       |        680 B |          1496 B |            216 B |
| Nullable_WithValues |        856 B |          1576 B |            176 B |
| Nullable_WithNulls  |        624 B |          1344 B |            144 B |
| Enum                |        592 B |          1408 B |             88 B |

### 反序列化

| 场景                | LHZ.FastJson | Newtonsoft.Json   | System.Text.Json  |
|---------------------|-------------:|------------------:|------------------:|
| SmallObject         |    143.3 ns  |  286.8 ns (2.00)  |  119.9 ns (0.84)  |
| MediumObject        |    858.3 ns  | 1668.8 ns (1.95)  |  845.6 ns (0.99)  |
| LargeList           |    13.57 μs  |  25.32 μs (1.87)  |  15.21 μs (1.12)  |
| Dictionary          |    512.3 ns  |  598.0 ns (1.17)  |  533.6 ns (1.04)  |
| StringEscapes       |    185.6 ns  |  359.6 ns (1.94)  |  235.2 ns (1.27)  |
| Nullable_WithValues |    200.3 ns  |  438.1 ns (2.19)  |  206.0 ns (1.03)  |
| Nullable_WithNulls  |    120.3 ns  |  331.0 ns (2.76)  |  142.7 ns (1.19)  |
| Enum                |     95.2 ns  |  292.4 ns (3.07)  |  115.3 ns (1.21)  |

单次操作的内存分配：

| 场景                | LHZ.FastJson | Newtonsoft.Json | System.Text.Json |
|---------------------|-------------:|----------------:|-----------------:|
| SmallObject         |        248 B |          2760 B |            104 B |
| MediumObject        |       1216 B |          3880 B |           1320 B |
| LargeList           |      19808 B |         19936 B |          10664 B |
| Dictionary          |       1920 B |          3952 B |           1320 B |
| StringEscapes       |        632 B |          4832 B |            176 B |
| Nullable_WithValues |        280 B |          2888 B |            104 B |
| Nullable_WithNulls  |        176 B |          2648 B |             64 B |
| Enum                |        256 B |          2704 B |             72 B |

### 历史版本对比

LHZ.FastJson 在 1.6.0 重构了反序列化方法，相比之前的版本有接近 2 倍的性能提升。以下数据在旧硬件和 .NET 6 上测得，不能与上面的表格直接比较：

``` ini
BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19044.1889 (21H2)
Intel Core i7-9700K CPU 3.60GHz (Coffee Lake), 1 CPU, 8 logical and 8 physical cores
.NET SDK=6.0.302
  [Host]     : .NET 6.0.7 (6.0.722.32202), X64 RyuJIT
  DefaultJob : .NET 6.0.7 (6.0.722.32202), X64 RyuJIT
```

|             Method |     Mean |    Error |   StdDev |
|------------------- |---------:|---------:|---------:|
|    LHZFastJson v1.6.0 | 43.91 ms | 0.181 ms | 0.170 ms |
|    LHZFastJson v1.5.2 | 85.18 ms | 0.301 ms | 0.282 ms |
