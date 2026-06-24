# 更新日志

[English](CHANGELOG.md)

本文档记录 LHZ.FastJson 的重要变更。

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
