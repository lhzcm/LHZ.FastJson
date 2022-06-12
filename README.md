# LHZ.FastJson
轻巧便利的Json序列化和反序列化工具 <br/>
Lightweight and convenient tool for JSON serialization and deserialization

## 序列化性能
LHZ.FastJson重构了序列化方法，拥有极高的序列化性能，下表为LHZ.FastJson、NewtonJson和System.Text.Json的序列化性能测试
``` ini

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19044.1706 (21H2)
Intel Core i7-9700K CPU 3.60GHz (Coffee Lake), 1 CPU, 8 logical and 8 physical cores
.NET SDK=6.0.201
  [Host]     : .NET 6.0.3 (6.0.322.12309), X64 RyuJIT
  DefaultJob : .NET 6.0.3 (6.0.322.12309), X64 RyuJIT


```
|             Method |      Mean |     Error |    StdDev |
|------------------- |----------:|----------:|----------:|
|    LHZ.FastJson |  7.014 μs | 0.0298 μs | 0.0279 μs |
|     NewtonJson | 18.943 μs | 0.1786 μs | 0.1671 μs |
| System.Text.Json | 10.410 μs | 0.0499 μs | 0.0467 μs |

# 如何安装 
### 下面展示不同的安装方法，以安装[LHZ.FastJson 1.3.3](https://www.nuget.org/packages/LHZ.FastJson/1.5.0)版本为例
### Package Manager
``` bash
Install-Package LHZ.FastJson -Version 1.5.0
```
### .NET CLI
``` bash
dotnet add package LHZ.FastJson --Version 1.5.0
```

### package-reference
``` xml
<PackageReference Include="LHZ.FastJson" Version="1.5.0" />
```

### Paket CLI
``` bash
paket add LHZ.FastJson --version 1.5.0
```

# 如何使用
## 使用LHZ.FastJson进行序列化
### 序列化代码示例
``` cshap
Student student = new Student
{
    NO = 1,
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
{"NO":1,"Name":"lhz","Age":18,"Brithday":"2000/1/1 0:00:00"}
```

## 使用LHZ.FastJson进行反序列化
### 反序列化代码示例
``` cshap
string str = "{\"NO\":1,\"Name\":\"lhz\",\"Age\":18,\"Brithday\":\"2000/1/1 0:00:00\"}";

Student student = JsonConvert.Deserialize<Student>(str);

Console.WriteLine("NO:{0},Name:{1},Age:{2},Brithday:{3}", student.NO, student.Name, student.Age, student.Brithday.ToString("yyyy-MM-dd"));
```

### 运行结果
``` bash
PS C:\Users\admin\source\repos\LHZ.FastJson\LHZ.FastJson.Test> dotnet run
NO:1,Name:lhz,Age:18,Brithday:2000-1-1
```
