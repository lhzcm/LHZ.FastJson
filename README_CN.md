# LHZ.FastJson
轻巧便利的Json序列化和反序列化工具 

[English](README.md) | [更新日志](CHANGELOG_CN.md)

# 如何安装 
### 下面展示不同的安装方法，以安装[LHZ.FastJson 2.0.0](https://www.nuget.org/packages/LHZ.FastJson/2.0.0)版本为例
### Package Manager
``` bash
Install-Package LHZ.FastJson -Version 2.0.0
```
### .NET CLI
``` bash
dotnet add package LHZ.FastJson --version 2.0.0
```

### package-reference
``` xml
<PackageReference Include="LHZ.FastJson" Version="2.0.0" />
```

### Paket CLI
``` bash
paket add LHZ.FastJson --version 2.0.0
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

## 序列化性能

LHZ.FastJson 重构了序列化方法，拥有极高的序列化性能。下表为 LHZ.FastJson、Newtonsoft.Json 和 System.Text.Json 的序列化性能测试（单位：ns，数值越低越好）：

``` ini
BenchmarkDotNet=v0.15.8, OS=Windows 10 (10.0.19045.6466/22H2/2022Update)
Intel Core i7-9700K CPU 3.60GHz (Coffee Lake), 1 CPU, 8 logical and 8 physical cores
  [Host]     : .NET Framework 4.8.1 (4.8.9310.0), X64 RyuJIT VectorSize=256
  DefaultJob : .NET Framework 4.8.1 (4.8.9310.0), X64 RyuJIT VectorSize=256
```

|                 场景 | LHZ.FastJson | Newtonsoft.Json | System.Text.Json |
|--------------------- |-------------:|----------------:|-----------------:|
|              小对象   |     766.0 ns |        788.1 ns |        689.5 ns |
|             中等对象  |   3,671.4 ns |      3,829.3 ns |      3,512.5 ns |
|       大列表（100 项）|  69,502.8 ns |     57,050.7 ns |     56,036.1 ns |
|        字典（10 项）  |   1,503.1 ns |      1,428.7 ns |      1,307.1 ns |
|    可空类型（全 null）|     412.7 ns |        782.3 ns |        613.7 ns |
|      可空类型（有值） |   1,490.4 ns |      1,425.9 ns |      1,227.5 ns |
|              枚举     |     368.6 ns |        617.2 ns |        516.1 ns |
|          转义字符串   |     457.9 ns |        901.5 ns |        839.4 ns |

## 反序列化性能

下表为 LHZ.FastJson、Newtonsoft.Json 和 System.Text.Json 的反序列化性能测试（单位：ns，数值越低越好）：

``` ini
BenchmarkDotNet=v0.15.8, OS=Windows 10 (10.0.19045.6466/22H2/2022Update)
Intel Core i7-9700K CPU 3.60GHz (Coffee Lake), 1 CPU, 8 logical and 8 physical cores
  [Host]     : .NET Framework 4.8.1 (4.8.9310.0), X64 RyuJIT VectorSize=256
  DefaultJob : .NET Framework 4.8.1 (4.8.9310.0), X64 RyuJIT VectorSize=256
```

|                 场景 | LHZ.FastJson | Newtonsoft.Json | System.Text.Json |
|--------------------- |-------------:|----------------:|-----------------:|
|              小对象   |   1,086.4 ns |      1,296.7 ns |        982.2 ns |
|             中等对象  |   4,359.8 ns |      5,502.5 ns |      4,945.0 ns |
|       大列表（100 项）| 108,993.4 ns |     96,139.6 ns |     97,247.1 ns |
|        字典（10 项）  |   1,976.6 ns |      2,680.6 ns |      3,206.0 ns |
|    可空类型（全 null）|     469.3 ns |      1,272.3 ns |        990.5 ns |
|      可空类型（有值） |   1,345.8 ns |      1,874.2 ns |      1,406.9 ns |
|              枚举     |     539.6 ns |      1,215.9 ns |        712.4 ns |
|          转义字符串   |     498.4 ns |      1,012.4 ns |        949.4 ns |
