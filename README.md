# LHZ.FastJson

A lightweight and high-performance JSON serialization and deserialization library for .NET.

[中文](README_CN.md) | [Changelog](CHANGELOG.md)

## Serialization Performance

LHZ.FastJson has been re-architected with a highly optimized serialization engine. The table below compares serialization performance across LHZ.FastJson, Newtonsoft.Json, and System.Text.Json:

``` ini
BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19044.1706 (21H2)
Intel Core i7-9700K CPU 3.60GHz (Coffee Lake), 1 CPU, 8 logical and 8 physical cores
.NET SDK=6.0.201
  [Host]     : .NET 6.0.3 (6.0.322.12309), X64 RyuJIT
  DefaultJob : .NET 6.0.3 (6.0.322.12309), X64 RyuJIT
```

|             Method  |      Mean |     Error |    StdDev |
|-------------------- |----------:|----------:|----------:|
|       LHZ.FastJson  |  7.014 μs | 0.0298 μs | 0.0279 μs |
|       Newtonsoft.Json | 18.943 μs | 0.1786 μs | 0.1671 μs |
|    System.Text.Json | 10.410 μs | 0.0499 μs | 0.0467 μs |

## Deserialization Performance

LHZ.FastJson has also been re-architected for deserialization, achieving nearly **2x** performance improvement over versions prior to 1.6.0:

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

## Installation

The following examples show different installation methods using [LHZ.FastJson 1.8.4](https://www.nuget.org/packages/LHZ.FastJson/1.8.4) as an example:

### Package Manager

``` bash
Install-Package LHZ.FastJson -Version 1.8.4
```

### .NET CLI

``` bash
dotnet add package LHZ.FastJson --Version 1.8.4
```

### Package Reference

``` xml
<PackageReference Include="LHZ.FastJson" Version="1.8.4" />
```

### Paket CLI

``` bash
paket add LHZ.FastJson --version 1.8.4
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
