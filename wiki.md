# LHZ.FastJson Wiki

> 轻量级、高性能的 .NET JSON 序列化与反序列化库 | Lightweight, high-performance JSON serialization/deserialization library for .NET

---

## 目录 | Table of Contents

- [概述 | Overview](#概述--overview)
- [安装 | Installation](#安装--installation)
- [快速入门 | Quick Start](#快速入门--quick-start)
- [序列化 | Serialization](#序列化--serialization)
- [反序列化 | Deserialization](#反序列化--deserialization)
- [API 参考 | API Reference](#api-参考--api-reference)
- [JSON 类型系统 | JSON Type System](#json-类型系统--json-type-system)
- [特性注解 | Attributes](#特性注解--attributes)
- [自定义转换器 | Custom Converters](#自定义转换器--custom-converters)
- [扩展方法 | Extension Methods](#扩展方法--extension-methods)
- [异常处理 | Exceptions](#异常处理--exceptions)
- [性能 | Performance](#性能--performance)
- [支持类型 | Supported Types](#支持类型--supported-types)
- [架构设计 | Architecture](#架构设计--architecture)

---

## 概述 | Overview

LHZ.FastJson 是一个针对 .NET 平台（net40~net6）的轻量级 JSON 库，专注于高性能序列化与反序列化。其核心性能策略包括：

- **不安全代码 (`unsafe`)**：使用 `char*` 指针实现零分配的 JSON 解析
- **表达式树编译**：通过 `Expression<T>` 编译为委托，消除运行时的反射开销
- **循环引用检测**：防止序列化时的无限递归

### 核心特性

| 特性 | 说明 |
|------|------|
| 🚀 高性能 | 基于 `unsafe` 指针解析 + Expression Tree 编译 |
| 🔌 可扩展 | 支持自定义转换器 (`IJsonCustomConverter`) |
| 🏷️ 注解支持 | `JsonPropertyAttribute` / `JsonIgnoredAttribute` |
| 🔄 循环引用检测 | 自动检测并跳过循环引用 |
| 📦 多目标框架 | net40 / net45 / net46 / netstandard2.0 / net5 / net6 |
| 🧵 线程安全 | 序列化委托使用 `ConcurrentDictionary` 缓存 |

---

## 安装 | Installation

### NuGet 安装

```bash
# Package Manager
Install-Package LHZ.FastJson -Version 1.8.4

# .NET CLI
dotnet add package LHZ.FastJson --version 1.8.4

# Paket CLI
paket add LHZ.FastJson --version 1.8.4
```

### csproj 引用

```xml
<PackageReference Include="LHZ.FastJson" Version="1.8.4" />
```

---

## 快速入门 | Quick Start

### 序列化对象

```csharp
using LHZ.FastJson;

var student = new Student
{
    NO = 1,
    Name = "张三",
    Age = 20,
    Birthday = new DateTime(2002, 1, 1)
};

string json = JsonConvert.Serialize(student);
// 输出: {"NO":1,"Name":"张三","Age":20,"Birthday":"2002-01-01 00:00:00"}
```

### 反序列化对象

```csharp
string json = @"{""NO"":1,""Name"":""张三"",""Age"":20}";

Student student = JsonConvert.Deserialize<Student>(json);
Console.WriteLine(student.Name); // 输出: 张三
```

### 解析为动态 JSON 树

```csharp
string json = @"{""name"":""张三"",""items"":[1,2,3]}";

IJsonObject obj = JsonConvert.Deserialize(json);
Console.WriteLine(obj["name"].Value);       // 张三
Console.WriteLine(obj["items"][0].Value);   // 1
```

---

## 序列化 | Serialization

### 基本序列化

```csharp
string json = JsonConvert.Serialize(anyObject);
```

> 如果 `anyObject` 为 `null`，返回 `"null"`。

### 支持的类型

| 类别 | .NET 类型 | JSON 输出 |
|------|-----------|-----------|
| 布尔 | `bool` | `true` / `false` |
| 整数 | `byte`, `short`, `ushort`, `int`, `uint`, `long`, `ulong` | 数字 |
| 浮点 | `float`, `double`, `decimal` | 数字 |
| 字符 | `char` | 字符串 `"c"` |
| 字符串 | `string` | `"value"` （自动转义） |
| 日期 | `DateTime` | `"yyyy-MM-dd HH:mm:ss"` |
| 枚举 | `Enum` | 数字字符串 `"1"` |
| GUID | `Guid` | 字符串 `"xxxx-xxxx..."` |
| 字典 | `IDictionary` | `{"key":value,...}` |
| 集合 | `IEnumerable` / `IList` / `Array` | `[item,item,...]` |
| 对象 | 任意 class / struct | `{"Property":value,...}` |

### 特殊字符转义

序列化时自动转义以下字符：

| 字符 | JSON 转义 |
|------|-----------|
| `"` | `\"` |
| `\` | `\\` |
| 换行 `\n` | `\\n` |
| 制表 `\t` | `\\t` |
| 退格 `\b` | `\\b` |
| 换页 `\f` | `\\f` |
| 回车 `\r` | `\\r` |
| 控制字符 | `\\uXXXX` |

### 循环引用检测

LHZ.FastJson 会自动检测序列化过程中的循环引用，防止 StackOverflowException：

```csharp
var parent = new Node { Name = "Parent" };
var child = new Node { Name = "Child", Parent = parent };
parent.Child = child;

string json = JsonConvert.Serialize(parent);
// 循环引用被安全跳过
```

---

## 反序列化 | Deserialization

### 基本反序列化

```csharp
// 泛型反序列化
T obj = JsonConvert.Deserialize<T>(jsonString);

// 动态解析
IJsonObject obj = JsonConvert.Deserialize(jsonString);
```

### 安全反序列化

```csharp
// 不会抛出异常
if (JsonConvert.TryDeserialize<Student>(jsonString, out Student student))
{
    // 反序列化成功
}
else
{
    // 处理失败情况
}
```

### 类型转换规则

| JSON 类型 | 可转换为 |
|-----------|----------|
| `Boolean` | `bool` |
| `Number` / `Long` | `byte`, `short`, `ushort`, `int`, `uint`, `long`, `ulong` |
| `Number` / `Double` | `float`, `double`, `decimal` |
| `String` | `string`, `char`（长度=1）, `DateTime`, `Guid`, `Enum`（按名称） |
| `Number` / `Long` | `Enum`（按数值） |
| `Null` | 引用类型 → `null`，值类型 → 抛出异常 |

---

## API 参考 | API Reference

### `JsonConvert` (静态类)

主要的公共 API 入口点。

#### 序列化方法

```csharp
// 将对象序列化为 JSON 字符串
public static string Serialize(object obj)

// [已废弃] 带格式化选项
[Obsolete]
public static string Serialize(object obj, params IJsonFormat[] formats)
```

#### 反序列化方法

```csharp
// 解析为 IJsonObject 树
public static IJsonObject Deserialize(string jsonString)

// 安全解析（不抛异常）
public static bool TryDeserialize(string jsonString, out IJsonObject dist)

// 泛型反序列化
public static T Deserialize<T>(string jsonString)

// 带自定义转换器的泛型反序列化
public static T Deserialize<T>(string jsonString, params IJsonCustomConverter[] jsonCustomConverters)

// 安全泛型反序列化
public static bool TryDeserialize<T>(string jsonString, out T dist)
```

### `IJsonObject` 接口

```csharp
public interface IJsonObject
{
    JsonType Type { get; }                          // JSON 类型
    object Value { get; }                           // 原始值
    IJsonObject this[string index] { get; }         // 按键（属性名）访问
    IJsonObject this[int index] { get; }            // 按索引访问（数组/对象）
    bool HasChildrenNode(string name);              // 判断是否存在子节点
    string ToJsonString();                          // 转回 JSON 字符串
    int Position { get; }                           // 在原始字符串中的位置
}
```

### `JsonType` 枚举

| 值 | 说明 | 对应类 |
|----|------|--------|
| `Content` | JSON 对象 `{}` | `JsonContent` |
| `Array` | JSON 数组 `[]` | `JsonArray` |
| `String` | 字符串 | `JsonString` |
| `Number` | 数字 | `JsonNumber` |
| `Boolean` | 布尔值 | `JsonBoolean` |
| `Null` | null | `JsonNull` |

---

## JSON 类型系统 | JSON Type System

### `JsonObject` 及其子类

```
JsonObject (abstract)
├── JsonContent  →  JSON 对象 {"key":"value"}
├── JsonArray    →  JSON 数组 [item1, item2]
├── JsonString   →  JSON 字符串 "text"
├── JsonNumber   →  JSON 数字 123 / 1.5
├── JsonBoolean  →  JSON 布尔 true/false
└── JsonNull     →  JSON null
```

### `JsonContent` — JSON 对象

```csharp
var obj = JsonConvert.Deserialize(@"{""name"":""张三"",""age"":20}");
var content = obj as JsonContent;

// 按键访问
Console.WriteLine(content["name"].Value);  // 张三
Console.WriteLine(content["age"].Value);   // 20

// 遍历属性
foreach (var kv in content)
{
    Console.WriteLine($"{kv.Key} = {kv.Value}");
}

// 检查属性是否存在
if (content.HasChildrenNode("name")) { ... }

// 转为 JSON 字符串
string json = content.ToJsonString();  // {"name":"张三","age":20}
```

### `JsonArray` — JSON 数组

```csharp
var arr = JsonConvert.Deserialize(@"[1, ""hello"", true]");
var array = arr as JsonArray;

Console.WriteLine(array.Length);         // 3
Console.WriteLine(array[0].Value);       // 1
Console.WriteLine(array[1].Value);       // hello

// 遍历
foreach (var item in array) { ... }
```

### `JsonString` — JSON 字符串

```csharp
var str = JsonConvert.Deserialize(@"""hello world""") as JsonString;
Console.WriteLine(str.Value);         // hello world
Console.WriteLine(str.ToJsonString()); // "hello world"
```

### `JsonNumber` — JSON 数字

```csharp
var num = JsonConvert.Deserialize(@"123") as JsonNumber;
Console.WriteLine(num.NumberType);    // Long
Console.WriteLine(num.Value);         // 123

var dec = JsonConvert.Deserialize(@"3.14") as JsonNumber;
Console.WriteLine(dec.NumberType);    // Double
```

### `JsonBoolean` — JSON 布尔

```csharp
var b = JsonConvert.Deserialize(@"true") as JsonBoolean;
Console.WriteLine(b.BooleanType);     // True
```

### `JsonNull` — JSON null

```csharp
var n = JsonConvert.Deserialize(@"null") as JsonNull;
Console.WriteLine(n.ToJsonString());  // null
```

---

## 特性注解 | Attributes

### `[JsonProperty(string name)]`

指定 JSON 属性名与 C# 属性名的映射关系：

```csharp
public class Student
{
    [JsonProperty("studentId")]
    public int Id { get; set; }

    [JsonProperty("studentName")]
    public string Name { get; set; }
}

// 序列化输出: {"studentId":1,"studentName":"张三"}
// 反序列化时自动映射 studentId → Id, studentName → Name
```

> ⚠️ 如果多个属性映射到相同的 JSON 名称，序列化时将抛出 `Exception`。

### `[JsonIgnored]` / `[JsonIgnored(JsonMethods method)]`

忽略指定属性，使其不参与序列化/反序列化：

```csharp
public class User
{
    public string Name { get; set; }

    [JsonIgnored]                    // 序列化和反序列化均忽略
    public string Password { get; set; }

    [JsonIgnored(JsonMethods.Serialize)]   // 仅序列化时忽略
    public string InternalId { get; set; }

    [JsonIgnored(JsonMethods.Deserialize)] // 仅反序列化时忽略
    public string ComputedField { get; set; }
}
```

`JsonMethods` 枚举值：

| 值 | 说明 |
|----|------|
| `Serialize` | 仅序列化时忽略 |
| `Deserialize` | 仅反序列化时忽略 |
| `All` | 序列化和反序列化均忽略 |

---

## 自定义转换器 | Custom Converters

### 实现 `IJsonCustomConverter` 接口

```csharp
using LHZ.FastJson.Interface;
using LHZ.FastJson.Enum.CustomConverter;

public class DateTimeCustomConverter : IJsonCustomConverter
{
    public Type ConvertType => typeof(DateTime);

    public JsonCustomConvertItem CustomItem => JsonCustomConvertItem.All;

    public string Serialize(object dist)
    {
        var dt = (DateTime)dist;
        return $"\"{dt:yyyy/MM/dd}\"";
    }

    public object Deserialize(IJsonObject jsonObject)
    {
        return DateTime.ParseExact(
            jsonObject.Value.ToString(),
            "yyyy/MM/dd",
            System.Globalization.CultureInfo.InvariantCulture
        );
    }
}
```

### 使用 `JsonCustomConvert<T>` 便捷类

```csharp
// 仅自定义序列化
var converter = new JsonCustomConvert<DateTime>(
    serializeFunc: dt => $"\"{dt:yyyy/MM/dd}\""
);

// 仅自定义反序列化
var converter = new JsonCustomConvert<DateTime>(
    deserializeFunc: jsonObj =>
        DateTime.Parse(jsonObj.Value.ToString())
);

// 同时自定义序列化和反序列化
var converter = new JsonCustomConvert<DateTime>(
    serializeFunc: dt => $"\"{dt:yyyy/MM/dd}\"",
    deserializeFunc: jsonObj =>
        DateTime.Parse(jsonObj.Value.ToString())
);
```

### 在反序列化中使用自定义转换器

```csharp
var converters = new IJsonCustomConverter[]
{
    new DateTimeCustomConverter(),
    new MyTypeConverter()
};

// 方式 1：通过 JsonConvert
var obj = JsonConvert.Deserialize<MyClass>(jsonString, converters);

// 方式 2：通过 JsonDeserializer
var deserializer = new JsonDeserializer<MyClass>(jsonString);
var obj = deserializer.Deserialize(converters);
```

### `JsonCustomConvertItem` 标志枚举

| 值 | 说明 |
|----|------|
| `None` | 不自定义 |
| `CustomSerialize` | 自定义序列化 |
| `CustomDeSerialize` | 自定义反序列化 |
| `All` | 全部自定义 |

---

## 扩展方法 | Extension Methods

LHZ.FastJson 提供了三个便捷扩展方法：

```csharp
using LHZ.FastJson.Extend;  // ⚠️ 需要手动 using

// 对象 → JSON 字符串
string json = myObject.ToJson();

// JSON 字符串 → 类型对象
Student student = jsonString.FromJson<Student>();

// IJsonObject → 类型对象
IJsonObject jsonObj = JsonConvert.Deserialize(jsonString);
Student student = jsonObj.ToObject<Student>();
```

---

## 异常处理 | Exceptions

| 异常类 | 基类 | 说明 |
|--------|------|------|
| `JsonReadException` | `Exception` | JSON 解析错误，包含 `Position` 属性指示错误位置 |
| `JsonDeserializationException` | `JsonReadException` | 反序列化类型错误，包含 `JsonType` 和 `TargetType` |
| `JsonFormatterException` | `Exception` | 格式化错误（已废弃） |
| `JsonCustomConverterException` | `Exception` | 自定义转换器错误 |

### 验证 JSON 有效性

```csharp
// 直接使用 JsonReader 验证
bool isValid = JsonReader.IsJsonString(jsonString, out Exception error);
if (!isValid)
{
    Console.WriteLine($"JSON 无效: {error.Message}");
}

// 使用 TryDeserialize
if (!JsonConvert.TryDeserialize(jsonString, out IJsonObject obj))
{
    // 解析失败
}
```

---

## 性能 | Performance

### 序列化性能对比

基于 BenchmarkDotNet 测试（.NET 6, Intel i7-9700K）：

| 库 | 平均耗时 (μs) |
|----|---------------|
| **LHZ.FastJson** | **7.014** |
| System.Text.Json | 10.410 |
| Newtonsoft.Json | 18.943 |

> LHZ.FastJson 的序列化性能比 Newtonsoft.Json 快约 **2.7 倍**，比 System.Text.Json 快约 **1.5 倍**。

### 性能原理

1. **零分配解析**：`JsonReader` 使用 `unsafe char*` 指针直接在 JSON 字符串上遍历，避免字符拷贝
2. **表达式树缓存**：每个类型的序列化/反序列化委托只在首次编译一次，之后直接调用 `ConcurrentDictionary` 中缓存的委托
3. **无反射**：运行时零反射开销，所有属性访问在 Expression 编译阶段就已完成

---

## 支持类型 | Supported Types

### 序列化支持

| 类型 | 支持 | 备注 |
|------|------|------|
| `bool` | ✅ | |
| `byte` | ✅ | |
| `char` | ✅ | 序列化为单字符字符串 |
| `short` / `ushort` | ✅ | |
| `int` / `uint` | ✅ | |
| `long` / `ulong` | ✅ | |
| `float` | ✅ | |
| `double` | ✅ | |
| `decimal` | ✅ | |
| `DateTime` | ✅ | 格式 `yyyy-MM-dd HH:mm:ss` |
| `Guid` | ✅ | |
| `string` | ✅ | |
| `Enum` | ✅ | 序列化为数值字符串 |
| `IDictionary` | ✅ | 序列化为 JSON 对象 |
| `IEnumerable` / `IList` / `Array` | ✅ | 序列化为 JSON 数组 |
| 自定义 class / struct | ✅ | 按公共属性序列化 |
| `Nullable<T>` | ✅ | `null` → `"null"` |

### 反序列化支持

| 目标类型 | JSON 类型要求 |
|----------|---------------|
| `bool` | `Boolean` |
| `byte` / `short` / `ushort` / `int` / `uint` / `long` / `ulong` | `Number` + `Long` |
| `float` / `double` / `decimal` | `Number`（任意） |
| `char` | `String` 且长度为 1 |
| `string` | `String` |
| `DateTime` | `String` |
| `Guid` | `String` |
| `Enum` | `String`（按名称）或 `Number`（按数值） |
| 自定义对象 | `Content`（属性名匹配） |
| 数组 | `Array` |
| `IList<T>` | `Array` |
| `IDictionary<K,V>` | `Content` |
| `Nullable<T>` | 对应类型或 `Null` |

---

## 架构设计 | Architecture

```
                            ┌──────────────────────────┐
                            │      JsonConvert          │
                            │     (用户 API 入口)         │
                            └──────────┬───────────────┘
                                       │
              ┌────────────────────────┼────────────────────────┐
              ▼                        │                        ▼
 ┌──────────────────────┐              │          ┌──────────────────────────┐
 │    JsonSerializer     │              │          │  JsonDeserializer<T>      │
 │  (Expression Tree     │              │          │  → JsonDeserialzerExpression<T>
 │   动态编译序列化)       │              │          │  (Expression Tree 动态反序列化) │
 └──────────────────────┘              │          └──────────────────────────┘
                                       │
                                       ▼
                          ┌──────────────────────┐
                          │     JsonReader        │
                          │  (unsafe char* 解析)    │
                          └──────────┬───────────┘
                                     │
                                     ▼
                          ┌──────────────────────┐
                          │   IJsonObject 树       │
                          │  ├─ JsonContent {}    │
                          │  ├─ JsonArray []      │
                          │  ├─ JsonString ""     │
                          │  ├─ JsonNumber 123    │
                          │  ├─ JsonBoolean t/f   │
                          │  └─ JsonNull null     │
                          └──────────────────────┘
```

### 关键设计决策

1. **统一入口**：所有公共 API 通过 `JsonConvert` 静态类暴露，降低使用门槛
2. **延迟编译**：每个类型首次序列化/反序列化时编译 Expression Tree，后续直接使用缓存委托
3. **不安全代码隔离**：`unsafe` 代码仅限 `JsonReader` 类，不影响外部 API
4. **树形中间表示**：`IJsonObject` 提供与具体类型无关的 JSON 数据访问层，适合动态 JSON 处理场景

---

## 许可证 | License

[MIT License](LICENSE)

## 链接 | Links

- [NuGet 包](https://www.nuget.org/packages/LHZ.FastJson/)
- [GitHub 仓库](https://github.com/lhzcm/LHZ.FastJson)
- [更新日志 | Changelog](CHANGELOG.md)
