# LHZ.FastJson
轻巧便利的Json序列化和反序列化工具 <br/>
Lightweight and convenient tool for JSON serialization and deserialization

# 如何安装 
### 下面展示不同的安装方法，以安装[LHZ.FastJson 1.3.3](https://www.nuget.org/packages/LHZ.FastJson/1.3.3)版本为例
### Package Manager
``` bash
Install-Package LHZ.FastJson -Version 1.3.3
```
### .NET CLI
``` bash
dotnet add package LHZ.FastJson --Version 1.3.3
```

### package-reference
``` xml
<PackageReference Include="LHZ.FastJson" Version="1.3.3" />
```

### Paket CLI
``` bash
paket add LHZ.FastJson --version 1.3.3
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
