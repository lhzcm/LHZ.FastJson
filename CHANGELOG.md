# Changelog

[中文](CHANGELOG_CN.md)

This document records important changes to LHZ.FastJson.

## 1.8.2 - 2026-06-22

This release focuses on Enum serialization fixes, correct type handling for IJsonObject deserialization, and adds performance benchmarks.

### Fixes

- Fixed Enum serialization by refactoring `SerializeEnum(object, Type)` to `SerializeEnum(System.Enum)`, simplifying the call and resolving potential type conversion errors.
- Fixed incorrect type assignment when deserializing to `IJsonObject` and its subclasses (such as `JsonContent`) by using `Expression.Convert` to ensure type safety.

### Improvements

- Added `[Flags]` attribute to the `JsonCustomConvertItem` enum, enabling bit flag combinations.
- Updated the `[Obsolete]` message for `JsonConvert.Serialize(object, IJsonFormat[])` to provide clearer guidance on alternatives.

### Tests

- Added a new performance benchmark project `LHZ.FastJson.Benchmark`, covering serialization, deserialization, and JsonReader performance tests.
- Added regression tests covering Enum serialization, IJsonObject deserialization, and other scenarios.

## 1.8.1 - 2026-06-10

This release focuses on JSON specification compliance, null-value stability, custom converter propagation, and thread safety.

### Fixes

- Strengthened `JsonReader` validation: added trailing character checks after parsing, and support for standard negative numbers, exponential numbers, and `\uXXXX` Unicode escape sequences.
- Rejected invalid JSON input, including leading-zero numbers, missing digits after decimal points, and trailing commas in arrays.
- Fixed a null-reference exception when serializing default `DateTime` values without a legacy formatter configured.
- Fixed serialization issues for `Nullable<T>` properties, collection elements, and dictionary values when they are `null`.
- Fixed string escaping for property names, dictionary keys, and `JsonString.ToJsonString()` to prevent generating invalid JSON.
- Fixed an issue where duplicate `JsonProperty` name detection was not working.
- Fixed an issue where `[JsonIgnored(JsonMethods.Deserialize)]` would skip subsequent properties during deserialization.
- Fixed an issue where custom converters were not propagated to inner elements of arrays, lists, dictionaries, and `Nullable<T>`.
- Switched numeric deserialization to use `InvariantCulture` to avoid being affected by system locale settings.
- Fixed an issue where custom serialization with validation enabled would invoke the custom function twice.
- Changed the serialization expression cache to a thread-safe cache, reducing race conditions during concurrent first-time serialization.

### Tests

- Added regression tests covering strict JSON reading, default `DateTime` serialization, `Nullable<T>`, collection/dictionary `null` values, duplicate property names, ignored properties, and nested custom converters.
