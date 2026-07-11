# 更新日志

[English](CHANGELOG.md)

本文档记录 LHZ.FastJson 的重要变更。

## 1.9.0 - 2026-07-11

本版本引入双层 JsonClass 架构、零分配字符串解析以及全面的英文文档。

### 功能

- 新增 `JsonClass.Internal` 命名空间，包含内部子类（`Internal.JsonArray`、`Internal.JsonBoolean`、`Internal.JsonContent`、`Internal.JsonNull`、`Internal.JsonNumber`、`Internal.JsonString`），携带位置元数据并支持延迟转义解析。
- `StringView` 现实现 `System.IConvertible` 接口，支持延迟数字解析 — `JsonNumber` 存储 `StringView`，仅在请求具体数值转换时才进行解析。

### 改进

- **零分配字符串解析**：`JsonReader.ReadStringLiteral` 现在直接返回 `StringView`，不再构建 `StringBuilder`，消除 JSON 字符串解析过程中的堆内存分配。
- **更快的布尔/null 解析**：`GetJsonBoolean` 和 `GetJsonNull` 现通过指针运算直接比较字符，不再创建中间 `StringView` 对象。
- **延迟转义解析**：转义序列（`\n`、`\t`、`\uXXXX` 等）不再在解析时处理，而是延迟到 `Internal.JsonString.Value` 首次访问时才解析。
- **JsonPropertyName 哈希缓存**：预计算的 DJB2 哈希值直接传入 `JsonPropertyName` 构造函数，避免字典查找时重复计算哈希。
- **NET5+ 优化**：`JsonContent.AddJsonProperty` 在 .NET 5+ 上使用 `Dictionary.TryAdd` 实现无锁快速路径。
- **线程安全表达式缓存**：`JsonDeserialzerExpression` 现使用 `Interlocked` 进行并发字典访问，减少首次反序列化时的竞态风险。
- `StringView` 从全局命名空间移至 `LHZ.FastJson.JsonClass`，实现正确的命名空间作用域。

### 破坏性变更

- `JsonBoolean` 和 `JsonNull` 的构造函数现为 `internal`。请改用 `JsonBoolean.True`、`JsonBoolean.False` 或 `JsonNull.Null` 静态属性。
- 移除 `JsonString` 中已弃用的 `GetValue()` 方法，请使用 `.Value` 或 `.ToString()`。
- `JsonString.ToJsonStringBuilder()` 重命名为 `ToStringBuilder()`，与 `IJsonObject` 接口约定保持一致。

### 文档

- 整个库的 XML 文档注释从中文翻译为英文。
- 为之前未文档化的成员添加了 XML 文档注释（`JsonReader` 构造函数、`StructConvertResult`、`StringView` 等）。
- 在 `.csproj` 中启用 `GenerateDocumentationFile`，构建时生成 XML 文档文件。
- 新增 `wiki.md`，提供全面的中英双语使用文档。

### 测试

- 新增 `RegressionTests` 类，覆盖边界情况：尾部多余字符、非法数字格式、Unicode 转义、默认 `DateTime` 格式、可空属性、集合 null 元素以及重复 `[JsonProperty]` 名称检测。
- 增强 `TestJsonDeserizlizer`、`TestJsonReder` 和 `TestJsonSerizlizer` 中的现有测试覆盖。

## 1.8.5 - 2026-06-29

本版本重构了 Json 解析与 JsonClass 基础设施，引入 `StringView` 和 `JsonPropertyName`，提升性能并优化代码结构。

### 功能

- 新增 `StringView` 结构体，优化字符串视图操作，减少 JSON 解析过程中的内存分配。
- 新增 `JsonPropertyName` 类，规范化 JsonClass 类型中的属性名称处理。

### 改进

- 重构 `JsonReader`，改进 JSON 解析性能和代码结构。
- 优化所有 JsonClass 类型（`JsonArray`、`JsonBoolean`、`JsonContent`、`JsonNull`、`JsonNumber`、`JsonObject`、`JsonString`），实现更清晰的代码。
- 改进 `JsonDeserialzerExpression` 表达式树生成逻辑。
- 更新 `IJsonObject` 接口，增强可扩展性。

### 测试

- 完善重构组件的单元测试覆盖。

## 1.8.4 - 2026-06-24

本版本新增了 `Guid` 类型的序列化和反序列化支持。

### 功能

- 在 `ObjectType` 枚举中添加 `Guid` 类型，实现 GUID 值的正确类型路由。
- 在 `JsonSerializer` 中添加 `Guid` 序列化支持，以标准连字符格式输出 GUID。
- 在 `JsonDeserialzerExpression` 中通过专用的 `ConvertToGuid` 方法添加 `Guid` 反序列化支持。

### 测试

- 在 `TestJsonSerizlizer` 中添加 Guid 序列化/反序列化往返测试。
- 在 `TestJsonDeserizlizer` 中添加 Guid 反序列化测试。

## 1.8.3 - 2026-06-24

本版本优化了 IJsonObject 反序列化，并完善了项目国际化文档。

### 改进

- 优化 IJsonObject 类型反序列化：当目标类型可从 `IJsonObject` 指派时（如 `System.Object`），直接返回原始 `IJsonObject` 对象，避免不必要的转换，提升性能与正确性。
- 重新整理 `JsonDeserialzerExpression.cs` 中的 `using` 语句，遵循标准规范（System 命名空间优先）。

### 文档

- 新增 README 和更新日志的英文版本，中文版本分别重命名为 `README_CN.md` 和 `CHANGELOG_CN.md`。
- 添加中英文文档之间的语言切换链接。

## 1.8.2 - 2026-06-22

本版本聚焦 Enum 序列化修复、IJsonObject 反序列化类型正确性，并新增性能基准测试。

### 修复

- 修复 Enum 序列化问题，将 `SerializeEnum(object, Type)` 重构为 `SerializeEnum(System.Enum)`，简化调用并修复潜在的类型转换错误。
- 修复反序列化为 `IJsonObject` 及其子类（如 `JsonContent`）时赋值类型不正确的问题，改用 `Expression.Convert` 确保类型安全。

### 改进

- 为 `JsonCustomConvertItem` 枚举添加 `[Flags]` 特性，支持位标志组合。
- 更新 `JsonConvert.Serialize(object, IJsonFormat[])` 的 `[Obsolete]` 消息，提供更明确的替代方法指引。

### 测试

- 新增性能基准测试项目 `LHZ.FastJson.Benchmark`，覆盖序列化、反序列化及 JsonReader 性能测试。
- 新增回归测试，覆盖 Enum 序列化、IJsonObject 反序列化等场景。

## 1.8.1 - 2026-06-10

本版本聚焦 JSON 规范兼容性、空值场景稳定性、自定义转换器传递和并发安全。

### 修复

- 强化 `JsonReader` 校验：解析完成后检查尾部多余字符，支持标准负数、指数数字和 `\uXXXX` Unicode 转义。
- 拒绝无效 JSON 输入，包括前导零数字、小数点后缺少数字、数组尾随逗号等场景。
- 修复默认 `DateTime` 序列化时未配置旧版格式化器导致的空引用异常。
- 修复 `Nullable<T>` 属性、集合元素和字典值为 `null` 时的序列化问题。
- 修复属性名、字典键和 `JsonString.ToJsonString()` 的字符串转义，避免生成非法 JSON。
- 修复重复 `JsonProperty` 名称检测失效的问题。
- 修复反序列化时 `[JsonIgnored(JsonMethods.Deserialize)]` 会跳过后续属性的问题。
- 修复自定义转换器未传递到数组、列表、字典和 `Nullable<T>` 内部元素的问题。
- 数字反序列化改用 `InvariantCulture`，避免受系统区域设置影响。
- 修复自定义序列化开启校验时会调用自定义函数两次的问题。
- 将序列化表达式缓存改为线程安全缓存，降低并发首次序列化时的竞态风险。

### 测试

- 新增回归测试，覆盖严格 JSON 读取、默认 `DateTime` 序列化、`Nullable<T>`、集合/字典 `null`、重复属性名、忽略属性和嵌套自定义转换器。
