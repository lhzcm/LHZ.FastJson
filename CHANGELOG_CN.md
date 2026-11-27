# 更新日志

[English](CHANGELOG.md)

本文档记录 LHZ.FastJson 的重要变更。

## 未发布

### 不兼容变更

- 异常消息改为英文（原来是中文），并统一格式：问题描述 + 位置，例如 `Unterminated string at position 4.`。消息中的控制字符会被转义。依赖消息文本做判断的代码需要调整。
- 序列化失败（循环引用，或两个属性映射到同一个 JSON 属性名）现在抛出新增的 `JsonSerializationException`，不再是裸的 `System.Exception`。它继承自 `Exception`，所以 `catch (Exception)` 仍然有效，出错的类型可通过 `SerializedType` 获取。
- `JsonDirectDeserializationException` 现在继承自 `JsonDeserializationException`，后者继承自 `JsonReadException`。一个 `catch (JsonDeserializationException)` 就能处理所有"JSON 合法但类型不对"的错误，原有的 `catch` 写法不受影响。`JsonDeserializationException` 新增了接受位置参数的构造函数，不再依赖已解析的节点。
- 转换值时的失败统一报告为 `JsonDeserializationException`（实际抛出 `JsonDirectDeserializationException`），带有位置、JSON 类型和目标类型，原始异常保存在 `InnerException`。以前这些情况会漏出 `OverflowException`、`FormatException`、`InvalidCastException` 或 `ArgumentException`：数字超出目标类型范围或格式不合法、枚举名或枚举值无效、Guid 无效、字典键无法转换、字典键重复，以及根本无法反序列化的类型（例如 `ReadOnlyCollection<T>`）。
- `JsonContent.AddJsonProperty` 遇到重复属性时抛出带参数名的 `ArgumentException`；无类型解析（`JsonConvert.Deserialize(string)`）遇到输入中的重复属性名时抛出带位置的 `JsonReadException`。两者以前都是抛出裸的 `System.Exception`。

### 修复

- 带类型的反序列化（`JsonConvert.Deserialize<T>`）现在接受标量值（数字、字符串、布尔、`null`、`DateTime`）后面的空白，空白之后可以是 `,`、`}` 或 `]`，缩进（格式化）的 JSON 因此可以被读取。以前只接受值前面的空白，`{"a":1 ,"b":2}`、`{\n  "a": 1\n}` 这类输入会抛出 `JsonReadException`。缺少逗号（`[1 2]`）以及以空白结尾的未完成输入仍然会被拒绝。
- 输入在某个 token 中间结束时，现在一律抛出 `JsonReadException`，不再抛出 `IndexOutOfRangeException`：输入以属性名结尾（`{"a"`）、属性名之前的逗号后面直接结束（`{"a":1,`）、字符串在转义序列中间被截断（`"abc\` 或 `"\u12`）。

- 数值类型的字典键使用固定区域设置（Invariant）转换，因此在用逗号作小数点的区域设置下，`Dictionary<double, T>` 也能正确读取 `{"1.5":1}`。
- `JsonReader` 收到 null 或空字符串时，抛出的 `ArgumentNullException` 现在有正确的参数名和消息（以前是把消息当成参数名传入）。
- 对象的值之后出现意外字符时，原来的错误提示写的是期望 `:` 或 `}`，现在改为 `,` 或 `}`。
- NuGet 包的描述和发布说明改为英文。

### 测试

- 新增异常契约测试（层次、位置、被包装的转换错误、英文消息），以及一个守卫测试：库源码中出现中文时测试失败。
- 新增序列化、反序列化、往返、空白处理和截断输入测试（有效文档的每个真前缀都必须抛出 `JsonReadException`）。已确认但尚未修复的问题以被忽略的测试形式记录在 `KnownIssueTests.cs` 中。

## 3.0.0 - 2026-11-26

本版本移除自定义转换器以保持库的小巧，并为每种类型编译强类型的序列化委托。

### 不兼容变更

- 移除自定义转换器功能，保持库的小巧和转换路径的精简。`IJsonCustomConverter`、`JsonCustomConvert<T>`、`JsonCustomConvertItem` 和 `JsonCustomConverterException` 已删除，接受转换器的重载也一并删除：`JsonConvert.Deserialize<T>(string, params IJsonCustomConverter[])`、`JsonConvert.Deserialize<T>(string, JsonConvertOptions, params IJsonCustomConverter[])` 和 `JsonSerializer(object, params IJsonCustomConverter[])`。构造函数 `JsonSerializer(object, JsonConvertOptions, params IJsonCustomConverter[])` 改为 `JsonSerializer(object, JsonConvertOptions)`，`JsonDeserializer<T>.Deserialize()` 不再接受转换器参数。编译后的反序列化委托不再在每一层嵌套调用中传递转换器表。如需控制某个值的输出，可以先把它转换为受支持的类型（例如 `string` 属性），或用 `JsonContent` / `JsonArray` 节点构建并赋给 `object` 类型的属性，序列化时会原样输出。
- `CaseStyle` 从 `LHZ.FastJson` 命名空间移到 `LHZ.FastJson.Enum`，与其他枚举放在一起。使用 `CaseStyle` 的地方需要添加 `using LHZ.FastJson.Enum;`，例如 `new JsonConvertOptions { CaseStyle = CaseStyle.CamelCase }`。
- 序列化包含自身的集合（数组、列表或其他 `IEnumerable`）时，现在抛出 `System.Exception`（"循环引用"），与对象或字典之间循环引用时抛出的异常相同，不再是 `InvalidOperationException`。原来捕获 `InvalidOperationException` 处理这种情况的代码需要改为捕获 `Exception`。

### 性能

- 序列化按类型和命名风格编译强类型委托。值类型和 sealed 类型的属性直接调用该委托，可空值类型属性不再装箱：在基准测试中，序列化包含三个有值可空属性的对象，耗时减少约 20%，内存分配减少 80 B。

## 2.1.2 - 2026-11-24

本版本修复泛型字典接口属性的序列化，并让接口类型的集合与字典可以被反序列化。

### 修复

- 声明为 `IDictionary<TKey,TValue>` 或 `IReadOnlyDictionary<TKey,TValue>` 的字典属性现在写成 JSON 对象（`{"k":1}`），不再写成由 `{"Key":"k","Value":1}` 组成的数组。类型本身或其实现中带有 `IDictionary<,>` / `IReadOnlyDictionary<,>` 的，都按字典处理，key/value 类型从该接口解析，泛型字典子类也不会再在构建序列化表达式时抛出 `ArgumentException`。
- 接口类型的集合与字典现在可以反序列化：`IDictionary<TKey,TValue>`、`IReadOnlyDictionary<TKey,TValue>`、`IList<T>`、`ICollection<T>`、`IEnumerable<T>`、`IReadOnlyList<T>`、`IReadOnlyCollection<T>`，以及非泛型的 `IDictionary`、`IList`、`ICollection`、`IEnumerable`。它们会落到 `Dictionary<TKey, TValue>` / `List<T>`（非泛型接口为 `Dictionary<string, object>` / `List<object>`）。此前会抛出 `JsonDirectDeserializationException`（“类型为接口或者是抽象接口，无法创建该类型对象”）。
- 反序列化时字典键会按声明的键类型转换，`Dictionary<int, string>` 不再因为键是字符串而失败。
- 序列化器直接输出 `IJsonObject` 节点的原文，因此 `object` 类型属性与非泛型集合（反序列化得到 `JsonObject` 节点）可以原样写回相同的 JSON。

## 2.1.1 - 2026-11-20

本版本将 `DateTime` 改为 ISO 8601 往返格式并提升读写性能，同时让自定义序列化转换器作用于基础类型属性。

### 不兼容变更

- `DateTime` 默认改为按 ISO 8601 往返格式序列化（与 System.Text.Json / Newtonsoft.Json 一致），不再使用 `yyyy-MM-dd HH:mm:ss`。保留小数秒（最多 7 位，省略末尾 0）和 `DateTimeKind`：`"2024-01-02T03:04:05.678Z"`（Utc）、`"2024-01-02T03:04:05.678+08:00"`（Local）、`"2024-01-02T03:04:05.678"`（Unspecified）。如需保持旧输出，设置 `JsonConvertOptions.DateTimeFormat = "yyyy-MM-dd HH:mm:ss"`。

### 功能

- 新增 `JsonConvertOptions.DateTimeFormat`，可用自定义格式（InvariantCulture）序列化 `DateTime`，反序列化时同样接受该格式。

### 修复

- `DateTime` 反序列化不再依赖当前区域设置。ISO 8601（`T` 或空格分隔、可选小数秒、`Z` 或 `±hh:mm` 偏移）及旧的 `yyyy-MM-dd HH:mm:ss` 由专用解析器处理，其他字符串回退到 InvariantCulture 解析。`Z` 解析为 `Utc`，带偏移解析为 `Local` 时间，无偏移为 `Unspecified`。
- 非法日期字符串现在抛出带位置和原始文本的 `JsonDirectDeserializationException`。
- 自定义序列化转换器现在会作用于基础类型的对象属性（数字、`bool`、`char`、`string`、`DateTime`、`Guid`、枚举、字典、集合），此前这些属性会忽略转换器。不定制序列化的转换器仍走内置快速路径。
- 修复枚举作为根值、集合元素、字典值或 `object` 类型属性时，序列化抛出 `ArgumentException` 的问题。

### 性能

- `DateTime` 读写改为无额外分配的实现。1000 项 `List<DateTime>`：序列化提速约 23%、内存减少约 21%；反序列化提速约 3.8 倍、内存减少约 80%。

## 2.0.1 - 2026-11-17

本版本新增按调用传入的命名风格（camelCase / snake_case / kebab-case）与嵌套深度选项，并移除旧的反序列化引擎。

### 功能

- 新增 `JsonConvertOptions` 选项类，可按调用传入 `JsonConvert.Serialize` / `Deserialize` / `TryDeserialize`（以及 `JsonSerializer`、`JsonDeserializer<T>`、`JsonReader`），不再有全局可变配置。
- 新增 `CaseStyle` 枚举（`PascalCase`、`CamelCase`、`SnakeCase`、`KebabCase`）与 `JsonConvertOptions.CaseStyle`：序列化时属性名自动转换为所选风格（如 `UserName` -> `userName` / `user_name` / `user-name`，`URLValue` -> `urlValue` / `url_value` / `url-value`），反序列化时自动匹配该风格的属性名。默认的 `PascalCase` 保持声明名不变；字典的 key 不做转换。
- 新增 `JsonConvertOptions.ConvertName(name, style)`，可将名称转换为指定风格。
- `JsonProperty` 特性指定的名称始终优先于命名风格转换。
- 已编译的序列化/反序列化表达式按命名策略分别缓存，不同选项可并发使用且无需重新编译。
- 新增 `JsonConvertOptions.MaxDepth`（默认 64），限制 `JsonReader` 与直读反序列化器接受的最大嵌套深度。
- 移除旧的基于 `IJsonObject` 的反序列化引擎（`JsonDeserialzerExpression`），默认的 `JsonCustomConvert<T>` 反序列化改为走直读引擎。
- 已知限制：默认的 `JsonCustomConvert<T>` 转换器（未提供自定义委托）使用默认选项，不继承外层调用传入的选项。
- 修复直读反序列化引擎（`JsonDirectDeserialzerExpression`）忽略 `JsonProperty` 特性的缺陷，现在与序列化端行为一致。

## 2.0.0 - 2026-08-13

本版本引入全新的直读流反序列化引擎，直接从 JSON 字符串读取，无需构建中间对象树，带来显著的性能提升并支持部分 JSON 输入。

### 功能

- 新增基于 `JsonDirectReader` 和 `JsonDirectDeserialzerExpression` 的直读反序列化，取代原先“先解析后转换”的两阶段流程。`JsonDeserializer<T>` 现在直接从原始 JSON 字符串反序列化。
- 反序列化支持部分 JSON 输入。

### 破坏性变更

- 移除 `JsonDeserializer<T>` 的 `IJsonObject` 和 `JsonReader` 构造函数，仅保留 `string` 构造函数。
- 移除 `JsonCommonExtend.ToObject<T>()` 扩展方法。
- 新增 `JsonDirectDeserializationException`（继承自 `JsonReadException`），用于直读反序列化错误，暴露 `JsonType` 和 `TargetType`。

### 改进

- `JsonPropertyName` 的哈希码现在在构造函数中即时计算，消除字典查找时的延迟哈希计算。
- 直读反序列化现在能正确处理哈希冲突。

## 1.9.1 - 2026-08-04

本版本移除废弃的 JSON 格式化系统并进行多项性能优化。

### 移除

- 移除已废弃的 `IJsonFormat` 接口、`JsonFormatter` 类、`DateTimeJsonFormat` 类以及 `JsonFormatterException` 异常。请改用 `IJsonCustomConverter` 进行自定义格式化。
- 移除 `JsonSerializer` 中已废弃的构造函数和 `_formater` 字段。
- 移除 `IJsonCustomConverter.cs` 中已注释的泛型接口代码。

### 改进

- **StringBuilder.Append(char) 优化**：将 `JsonArray` 和 `JsonContent` 中的 `Append("[")` 替换为 `Append('[')`，避免字符串分配开销。
- **空数组缓存**：将 `JsonDeserialzerExpression` 中的 `new Type[0]` / `new Type[] { }` 替换为 `EmptyArray<Type>.Value`，消除重复的空数组分配。
- **扩展 AggressiveInlining**：`StringView` 现为 `NETSTANDARD2_0_OR_GREATER` 和 `NETCOREAPP2_0_OR_GREATER` 也应用 `[MethodImpl(MethodImplOptions.AggressiveInlining)]`。
- 简化 `SerializeDateTime`，统一使用标准 `yyyy-MM-dd HH:mm:ss` 格式。
- 将 `JsonSerializer` 中的 `GetObjectType` 方法改为静态方法。
- 移除 `JsonCustomConverter` 中不必要的字段初始化器（`_serializeValidate = false`）。
- 为 `JsonDeserialzerExpression` 中的 `DateTime.Parse` 调用添加 `CultureInfo.CurrentCulture`。

### 新增

- 新增 `Utils/EmptyArray.cs` 工具类，提供缓存的空类型数组。

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
