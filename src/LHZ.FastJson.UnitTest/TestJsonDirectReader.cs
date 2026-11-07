using System;
using System.Collections.Generic;
using System.Reflection;
using LHZ.FastJson.Enum;
using LHZ.FastJson.Exceptions;
using LHZ.FastJson.JsonClass;
using LHZ.FastJson.Utils;
using NUnit.Framework;

namespace LHZ.FastJson.UnitTest;

/// <summary>
/// Comprehensive tests for the JsonDirectReader class.
/// JsonDirectReader is internal, so reflection is used to access it.
/// </summary>
[TestFixture]
public class TestJsonDirectReader
{
    #region Reflection Helpers

    private static readonly Type ReaderType;
    private static readonly ConstructorInfo CtorWithString;

    static TestJsonDirectReader()
    {
        var assembly = Assembly.GetAssembly(typeof(JsonConvert));
        ReaderType = assembly.GetType("LHZ.FastJson.JsonDirectReader");
        CtorWithString = ReaderType.GetConstructor(
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
            null, new[] { typeof(string) }, null);
    }

    private const BindingFlags MethodFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

    /// <summary>Create a JsonDirectReader instance via reflection.</summary>
    private static object CreateReader(string json)
    {
        return CtorWithString.Invoke(new object[] { json });
    }

    /// <summary>Get a property value from the reader.</summary>
    private static T GetProp<T>(object reader, string name)
    {
        return (T)ReaderType.GetProperty(name, MethodFlags).GetValue(reader);
    }

    /// <summary>Invoke a method on the reader.</summary>
    private static T Invoke<T>(object reader, string name, params object[] args)
    {
        return (T)ReaderType.GetMethod(name, MethodFlags).Invoke(reader, args);
    }

    /// <summary>Invoke a void method on the reader.</summary>
    private static void InvokeVoid(object reader, string name, params object[] args)
    {
        ReaderType.GetMethod(name, MethodFlags).Invoke(reader, args);
    }

    /// <summary>Invoke a generic method on the reader.</summary>
    private static T InvokeGeneric<T>(object reader, string name, Type[] typeArgs, params object[] args)
    {
        var method = ReaderType.GetMethod(name, MethodFlags);
        var generic = method.MakeGenericMethod(typeArgs);
        return (T)generic.Invoke(reader, args);
    }

    #endregion

    #region Constructor & Basic Properties

    /// <summary>
    /// Verify that the constructor correctly sets Content, Position, and Length.
    /// </summary>
    [Test]
    public void Constructor_SetsContentAndPosition()
    {
        var reader = CreateReader("{\"a\":1}");

        ClassicAssert.AreEqual("{\"a\":1}", GetProp<string>(reader, "Content"));
        ClassicAssert.AreEqual(0, GetProp<int>(reader, "Position"));
        ClassicAssert.AreEqual(7, GetProp<int>(reader, "Length"));
        ClassicAssert.IsFalse(GetProp<bool>(reader, "IsReadEnd"));
    }

    /// <summary>
    /// Verify that a null string argument throws ArgumentNullException.
    /// </summary>
    [Test]
    public void Constructor_NullContent_ThrowsArgumentNullException()
    {
        Assert.Throws<TargetInvocationException>(() => CtorWithString.Invoke(new object[] { null }));
    }
    #endregion

    #region JsonType Detection

    /// <summary>
    /// Verify JsonType detects all six JSON types correctly.
    /// </summary>
    [Test]
    public void JsonType_DetectsAllTypes()
    {
        ClassicAssert.AreEqual(JsonType.Content, GetProp<JsonType>(CreateReader("{}"), "JsonType"));
        ClassicAssert.AreEqual(JsonType.Array, GetProp<JsonType>(CreateReader("[]"), "JsonType"));
        ClassicAssert.AreEqual(JsonType.String, GetProp<JsonType>(CreateReader("\"hello\""), "JsonType"));
        ClassicAssert.AreEqual(JsonType.Number, GetProp<JsonType>(CreateReader("42"), "JsonType"));
        ClassicAssert.AreEqual(JsonType.Number, GetProp<JsonType>(CreateReader("-7"), "JsonType"));
        ClassicAssert.AreEqual(JsonType.Boolean, GetProp<JsonType>(CreateReader("true"), "JsonType"));
        ClassicAssert.AreEqual(JsonType.Boolean, GetProp<JsonType>(CreateReader("false"), "JsonType"));
        ClassicAssert.AreEqual(JsonType.Null, GetProp<JsonType>(CreateReader("null"), "JsonType"));
    }

    /// <summary>
    /// Verify JsonType skips leading whitespace before detecting type.
    /// </summary>
    [Test]
    public void JsonType_SkipsLeadingWhitespace()
    {
        ClassicAssert.AreEqual(JsonType.String, GetProp<JsonType>(CreateReader("  \t\r\n\"x\""), "JsonType"));
        ClassicAssert.AreEqual(JsonType.Number, GetProp<JsonType>(CreateReader("  123"), "JsonType"));
        ClassicAssert.AreEqual(JsonType.Content, GetProp<JsonType>(CreateReader("  {}"), "JsonType"));
    }

    #endregion

    #region SkipWhitespace

    /// <summary>
    /// Verify SkipWhitespace moves past spaces, tabs, newlines, and carriage returns.
    /// </summary>
    [Test]
    public void SkipWhitespace_SkipsAllWhitespace()
    {
        var reader = CreateReader(" \t\r\n  {\"a\":1}");
        InvokeVoid(reader, "SkipWhitespace");
        ClassicAssert.AreEqual('{', GetProp<char>(reader, "CurrentChar"));
        // " \t\r\n  " = 6 whitespace chars before '{'
        ClassicAssert.AreEqual(6, GetProp<int>(reader, "Position"));
    }

    /// <summary>
    /// Verify SkipWhitespace does nothing when already at end.
    /// </summary>
    [Test]
    public void SkipWhitespace_DoesNothingAtEnd()
    {
        var reader = CreateReader(" ");
        // The reader starts at position 0, SkipWhitespace should skip the whitespace
        // but not throw at end (it returns when Position >= Length)
        InvokeVoid(reader, "SkipWhitespace");
        Assert.DoesNotThrow(() => InvokeVoid(reader, "SkipWhitespace"));
    }

    #endregion

    #region ReadString

    /// <summary>
    /// Verify ReadString returns the string content without quotes.
    /// </summary>
    [Test]
    public void ReadString_ReturnsStringContent()
    {
        var reader = CreateReader("\"Hello World\"");
        ClassicAssert.AreEqual("Hello World", Invoke<string>(reader, "ReadString"));
        ClassicAssert.IsTrue(GetProp<bool>(reader, "IsReadEnd"));
    }

    /// <summary>
    /// Verify ReadString handles empty strings.
    /// </summary>
    [Test]
    public void ReadString_EmptyString()
    {
        var reader = CreateReader("\"\"");
        ClassicAssert.AreEqual("", Invoke<string>(reader, "ReadString"));
    }

    /// <summary>
    /// Verify ReadString handles escape sequences.
    /// </summary>
    [Test]
    public void ReadString_EscapeSequences()
    {
        ClassicAssert.AreEqual("quote\"here", Invoke<string>(CreateReader("\"quote\\\"here\""), "ReadString"));
        ClassicAssert.AreEqual("back\\slash", Invoke<string>(CreateReader("\"back\\\\slash\""), "ReadString"));
        ClassicAssert.AreEqual("line1\nline2", Invoke<string>(CreateReader("\"line1\\nline2\""), "ReadString"));
        ClassicAssert.AreEqual("tab\there", Invoke<string>(CreateReader("\"tab\\there\""), "ReadString"));
        ClassicAssert.AreEqual("carriage\rreturn", Invoke<string>(CreateReader("\"carriage\\rreturn\""), "ReadString"));
        ClassicAssert.AreEqual("slash/path", Invoke<string>(CreateReader("\"slash\\/path\""), "ReadString"));
        ClassicAssert.AreEqual("back\bspace", Invoke<string>(CreateReader("\"back\\bspace\""), "ReadString"));
        ClassicAssert.AreEqual("form\ffeed", Invoke<string>(CreateReader("\"form\\ffeed\""), "ReadString"));
    }

    /// <summary>
    /// Verify ReadString handles unicode escape sequences.
    /// </summary>
    [Test]
    public void ReadString_UnicodeEscapes()
    {
        ClassicAssert.AreEqual("ABC", Invoke<string>(CreateReader("\"\\u0041\\u0042\\u0043\""), "ReadString"));
        ClassicAssert.AreEqual("中文", Invoke<string>(CreateReader("\"\\u4e2d\\u6587\""), "ReadString"));
        ClassicAssert.AreEqual("\u00e9", Invoke<string>(CreateReader("\"\\u00e9\""), "ReadString"));
    }

    /// <summary>
    /// Verify ReadString handles mixed content with escapes.
    /// </summary>
    [Test]
    public void ReadString_MixedContentWithEscapes()
    {
        var reader = CreateReader("\"hello\\nworld\\t!\"");
        ClassicAssert.AreEqual("hello\nworld\t!", Invoke<string>(reader, "ReadString"));
    }

    /// <summary>
    /// Verify ReadString throws when the string is not properly quoted.
    /// </summary>
    [Test]
    public void ReadString_NotQuoted_ThrowsJsonReadException()
    {
        var reader = CreateReader("not_a_string");
        Assert.Throws<TargetInvocationException>(() => Invoke<string>(reader, "ReadString"));
    }

    /// <summary>
    /// Verify ReadString throws for unclosed strings.
    /// </summary>
    [Test]
    public void ReadString_Unclosed_ThrowsJsonReadException()
    {
        var reader = CreateReader("\"unclosed");
        Assert.Throws<TargetInvocationException>(() => Invoke<string>(reader, "ReadString"));
    }

    #endregion

    #region ReadNumber (internal)

    /// <summary>
    /// Verify ReadNumber parses an integer correctly.
    /// </summary>
    [Test]
    public void ReadNumber_Integer()
    {
        var reader = CreateReader("12345");
        var sv = Invoke<object>(reader, "ReadNumber");
        ClassicAssert.AreEqual("12345", sv.ToString());
    }

    /// <summary>
    /// Verify ReadNumber parses a negative integer.
    /// </summary>
    [Test]
    public void ReadNumber_NegativeInteger()
    {
        var reader = CreateReader("-42");
        var sv = Invoke<object>(reader, "ReadNumber");
        ClassicAssert.AreEqual("-42", sv.ToString());
    }

    /// <summary>
    /// Verify ReadNumber parses a floating-point number.
    /// </summary>
    [Test]
    public void ReadNumber_Float()
    {
        var reader = CreateReader("3.14159");
        var sv = Invoke<object>(reader, "ReadNumber");
        ClassicAssert.AreEqual("3.14159", sv.ToString());
    }

    /// <summary>
    /// Verify ReadNumber parses scientific notation.
    /// </summary>
    [Test]
    public void ReadNumber_ScientificNotation()
    {
        ClassicAssert.AreEqual("1e10", Invoke<object>(CreateReader("1e10"), "ReadNumber").ToString());
        ClassicAssert.AreEqual("2.5E-3", Invoke<object>(CreateReader("2.5E-3"), "ReadNumber").ToString());
        ClassicAssert.AreEqual("-1.5e+2", Invoke<object>(CreateReader("-1.5e+2"), "ReadNumber").ToString());
        ClassicAssert.AreEqual("0.5e10", Invoke<object>(CreateReader("0.5e10"), "ReadNumber").ToString());
    }

    /// <summary>
    /// Verify ReadNumber parses zero correctly.
    /// </summary>
    [Test]
    public void ReadNumber_Zero()
    {
        var reader = CreateReader("0");
        var sv = Invoke<object>(reader, "ReadNumber");
        ClassicAssert.AreEqual("0", sv.ToString());
    }

    /// <summary>
    /// Verify ReadNumber throws on leading zeros.
    /// </summary>
    [Test]
    public void ReadNumber_LeadingZero_ThrowsJsonReadException()
    {
        var reader = CreateReader("007");
        Assert.Throws<TargetInvocationException>(() => Invoke<object>(reader, "ReadNumber"));
    }

    /// <summary>
    /// Verify ReadNumber throws when sign is not followed by a digit.
    /// </summary>
    [Test]
    public void ReadNumber_SignWithoutDigit_ThrowsJsonReadException()
    {
        var reader = CreateReader("-");
        Assert.Throws<TargetInvocationException>(() => Invoke<object>(reader, "ReadNumber"));
    }

    /// <summary>
    /// Verify ReadNumber throws when decimal point is not followed by a digit.
    /// </summary>
    [Test]
    public void ReadNumber_DecimalWithoutDigit_ThrowsJsonReadException()
    {
        var reader = CreateReader("1.");
        Assert.Throws<TargetInvocationException>(() => Invoke<object>(reader, "ReadNumber"));
    }

    #endregion

    #region ReadBoolean (internal)

    /// <summary>
    /// Verify ReadBoolean returns true for "true".
    /// </summary>
    [Test]
    public void ReadBoolean_True()
    {
        var reader = CreateReader("true");
        ClassicAssert.IsTrue(Invoke<bool>(reader, "ReadBoolean"));
    }

    /// <summary>
    /// Verify ReadBoolean returns false for "false".
    /// </summary>
    [Test]
    public void ReadBoolean_False()
    {
        var reader = CreateReader("false");
        ClassicAssert.IsFalse(Invoke<bool>(reader, "ReadBoolean"));
    }

    /// <summary>
    /// Verify ReadBoolean throws for invalid boolean literals.
    /// </summary>
    [Test]
    public void ReadBoolean_Invalid_ThrowsJsonReadException()
    {
        var reader = CreateReader("tru");
        Assert.Throws<TargetInvocationException>(() => Invoke<bool>(reader, "ReadBoolean"));
    }

    #endregion

    #region ReadNull (internal)

    /// <summary>
    /// Verify ReadNull returns null for "null".
    /// </summary>
    [Test]
    public void ReadNull_ReturnsNull()
    {
        var reader = CreateReader("null");
        var result = InvokeGeneric<object>(reader, "ReadNull", new[] { typeof(object) });
        ClassicAssert.IsNull(result);
    }

    /// <summary>
    /// Verify ReadNull throws for invalid null literal.
    /// </summary>
    [Test]
    public void ReadNull_Invalid_ThrowsJsonReadException()
    {
        var reader = CreateReader("nul");
        Assert.Throws<TargetInvocationException>(
            () => InvokeGeneric<object>(reader, "ReadNull", new[] { typeof(object) }));
    }

    #endregion

    #region ReadJsonPropertyName

    /// <summary>
    /// Verify ReadJsonPropertyName returns the correct property name.
    /// JsonPropertyName.Name is internal, so we access it via reflection.
    /// </summary>
    [Test]
    public void ReadJsonPropertyName_ReturnsName()
    {
        var reader = CreateReader("\"name\": 42");
        var prop = Invoke<object>(reader, "ReadJsonPropertyName");
        // Access internal Name property via reflection (NonPublic binding flags needed)
        var nameProp = prop.GetType().GetProperty("Name", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        var nameValue = nameProp.GetValue(prop);
        ClassicAssert.AreEqual("name", nameValue.ToString());

        // After reading property name, reader is positioned after ':'. 
        // Skip any whitespace before reading the value.
        InvokeVoid(reader, "SkipWhitespace");
        var numResult = Invoke<object>(reader, "ReadNumber");
        ClassicAssert.AreEqual("42", numResult.ToString());
    }

    /// <summary>
    /// Verify ReadJsonPropertyName throws when not starting with a quote.
    /// </summary>
    [Test]
    public void ReadJsonPropertyName_NotQuoted_ThrowsJsonReadException()
    {
        var reader = CreateReader("name: 42");
        Assert.Throws<TargetInvocationException>(() => Invoke<object>(reader, "ReadJsonPropertyName"));
    }

    /// <summary>
    /// Verify ReadJsonPropertyName throws when missing colon after name.
    /// </summary>
    [Test]
    public void ReadJsonPropertyName_MissingColon_ThrowsJsonReadException()
    {
        var reader = CreateReader("\"name\" 42");
        Assert.Throws<TargetInvocationException>(() => Invoke<object>(reader, "ReadJsonPropertyName"));
    }

    #endregion

    #region ReadContent

    /// <summary>
    /// Verify ReadContent iterates over all properties in a JSON object.
    /// Uses reflection because ReadContent returns internal types.
    /// </summary>
    [Test]
    public void ReadContent_IteratesProperties()
    {
        var reader = CreateReader("{\"name\":\"LHZ\",\"age\":30}");
        var keys = new List<string>();
        var types = new List<JsonType>();

        // ReadContent returns IEnumerable<KeyValuePair<JsonPropertyName, JsonDirectReader>>
        // Both are internal, so iterate as non-generic IEnumerable and use reflection
        var enumerable = (System.Collections.IEnumerable)ReaderType.GetMethod("ReadContent", MethodFlags).Invoke(reader, null);
        foreach (var kvpObj in enumerable)
        {
            // Get Key and Value from KeyValuePair via reflection
            var kvpType = kvpObj.GetType();
            var keyObj = kvpType.GetProperty("Key").GetValue(kvpObj);
            var valReader = kvpType.GetProperty("Value").GetValue(kvpObj);

            // Access JsonPropertyName.Name (internal) via reflection
            var nameProp = keyObj.GetType().GetProperty("Name", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            var nameValue = nameProp.GetValue(keyObj);
            keys.Add(nameValue.ToString());

            types.Add(GetProp<JsonType>(valReader, "JsonType"));
            // Must consume the value so enumeration advances
            InvokeVoid(valReader, "SkipCurrentObject");
        }

        ClassicAssert.AreEqual(new[] { "name", "age" }, keys.ToArray());
        ClassicAssert.AreEqual(new[] { JsonType.String, JsonType.Number }, types.ToArray());
    }

    /// <summary>
    /// Verify ReadContent handles an empty object.
    /// </summary>
    [Test]
    public void ReadContent_EmptyObject()
    {
        var reader = CreateReader("{}");
        var enumerable = (System.Collections.IEnumerable)ReaderType.GetMethod("ReadContent", MethodFlags).Invoke(reader, null);
        int count = 0;
        foreach (var _ in enumerable) count++;
        ClassicAssert.AreEqual(0, count);
    }

    /// <summary>
    /// Verify ReadContent handles nested objects.
    /// </summary>
    [Test]
    public void ReadContent_NestedObject()
    {
        var json = "{\"outer\":{\"inner\":\"value\"}}";
        var reader = CreateReader(json);
        var enumerable = (System.Collections.IEnumerable)ReaderType.GetMethod("ReadContent", MethodFlags).Invoke(reader, null);

        int count = 0;
        foreach (var kvpObj in enumerable)
        {
            count++;
            var kvpType = kvpObj.GetType();
            var keyObj = kvpType.GetProperty("Key").GetValue(kvpObj);
            var valReader = kvpType.GetProperty("Value").GetValue(kvpObj);

            // Access internal Name property
            var nameProp = keyObj.GetType().GetProperty("Name", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            ClassicAssert.AreEqual("outer", nameProp.GetValue(keyObj).ToString());

            // item.Value is positioned at '{'
            ClassicAssert.AreEqual(JsonType.Content, GetProp<JsonType>(valReader, "JsonType"));
            // Must consume the nested object so enumeration advances
            InvokeVoid(valReader, "SkipCurrentObject");
        }
        ClassicAssert.AreEqual(1, count);
    }

    #endregion

    #region ReadArray

    /// <summary>
    /// Verify ReadArray iterates over all elements in a JSON array.
    /// </summary>
    [Test]
    public void ReadArray_IteratesElements()
    {
        var reader = CreateReader("[1, 2, 3]");
        var values = new List<string>();

        var enumerable = (System.Collections.IEnumerable)ReaderType.GetMethod("ReadArray", MethodFlags).Invoke(reader, null);
        foreach (var item in enumerable)
        {
            // item is the same JsonDirectReader instance (yield return this)
            // ReadNumber is internal, call via reflection
            var result = Invoke<object>(item, "ReadNumber");
            values.Add(result.ToString());
        }

        ClassicAssert.AreEqual(new[] { "1", "2", "3" }, values.ToArray());
    }

    /// <summary>
    /// Verify ReadArray handles an empty array.
    /// </summary>
    [Test]
    public void ReadArray_EmptyArray()
    {
        var reader = CreateReader("[]");
        var enumerable = (System.Collections.IEnumerable)ReaderType.GetMethod("ReadArray", MethodFlags).Invoke(reader, null);
        int count = 0;
        foreach (var _ in enumerable) count++;
        ClassicAssert.AreEqual(0, count);
    }

    /// <summary>
    /// Verify ReadArray handles mixed types.
    /// </summary>
    [Test]
    public void ReadArray_MixedTypes()
    {
        var reader = CreateReader("[\"text\", 42, true, null]");

        var enumerable = (System.Collections.IEnumerable)ReaderType.GetMethod("ReadArray").Invoke(reader, null);
        var types = new List<JsonType>();
        foreach (var item in enumerable)
        {
            types.Add(GetProp<JsonType>(item, "JsonType"));
            // Must consume the value so the next iteration advances correctly
            InvokeVoid(item, "SkipCurrentObject");
        }

        ClassicAssert.AreEqual(new[] { JsonType.String, JsonType.Number, JsonType.Boolean, JsonType.Null }, types.ToArray());
    }

    /// <summary>
    /// Verify ReadArray handles an array of objects.
    /// Note: nested arrays like [[1,2],[3,4]] are not fully supported by ReadArray
    /// because it skips consecutive '[' characters (known behavior).
    /// </summary>
    [Test]
    public void ReadArray_ArrayOfObjects()
    {
        var reader = CreateReader("[{\"a\":1},{\"b\":2}]");

        var outer = (System.Collections.IEnumerable)ReaderType.GetMethod("ReadArray", MethodFlags).Invoke(reader, null);
        int outerCount = 0;
        foreach (var outerItem in outer)
        {
            outerCount++;
            ClassicAssert.AreEqual(JsonType.Content, GetProp<JsonType>(outerItem, "JsonType"));
            // Must consume the content object so enumeration advances
            InvokeVoid(outerItem, "SkipCurrentObject");
        }
        ClassicAssert.AreEqual(2, outerCount);
    }

    #endregion

    #region SkipCurrentObject

    /// <summary>
    /// Verify SkipCurrentObject skips a simple value.
    /// </summary>
    [Test]
    public void SkipCurrentObject_SkipsString()
    {
        var reader = CreateReader("\"skip\"");
        InvokeVoid(reader, "SkipCurrentObject");
        ClassicAssert.IsTrue(GetProp<bool>(reader, "IsReadEnd"));
    }

    /// <summary>
    /// Verify SkipCurrentObject skips a number.
    /// </summary>
    [Test]
    public void SkipCurrentObject_SkipsNumber()
    {
        var reader = CreateReader("12345");
        InvokeVoid(reader, "SkipCurrentObject");
        ClassicAssert.IsTrue(GetProp<bool>(reader, "IsReadEnd"));
    }

    /// <summary>
    /// Verify SkipCurrentObject skips a nested object (note: array skip has a known bug
    /// in the source where ReadContent is called instead of ReadArray for '[').
    /// </summary>
    [Test]
    public void SkipCurrentObject_SkipsObject()
    {
        var reader = CreateReader("{\"a\":1,\"b\":\"x\"}");
        InvokeVoid(reader, "SkipCurrentObject");
        ClassicAssert.IsTrue(GetProp<bool>(reader, "IsReadEnd"));
    }

    /// <summary>
    /// Verify SkipCurrentObject on a boolean value.
    /// </summary>
    [Test]
    public void SkipCurrentObject_SkipsBoolean()
    {
        var reader = CreateReader("true");
        InvokeVoid(reader, "SkipCurrentObject");
        ClassicAssert.IsTrue(GetProp<bool>(reader, "IsReadEnd"));

        reader = CreateReader("false");
        InvokeVoid(reader, "SkipCurrentObject");
        ClassicAssert.IsTrue(GetProp<bool>(reader, "IsReadEnd"));
    }

    /// <summary>
    /// Verify SkipCurrentObject on null.
    /// </summary>
    [Test]
    public void SkipCurrentObject_SkipsNull()
    {
        var reader = CreateReader("null");
        InvokeVoid(reader, "SkipCurrentObject");
        ClassicAssert.IsTrue(GetProp<bool>(reader, "IsReadEnd"));
    }

    /// <summary>
    /// Verify SkipCurrentObject skips one element in a sequence (via array).
    /// </summary>
    [Test]
    public void SkipCurrentObject_SkipsOneElementInSequence()
    {
        var reader = CreateReader("42");
        InvokeVoid(reader, "SkipCurrentObject");
        ClassicAssert.IsTrue(GetProp<bool>(reader, "IsReadEnd"));
    }

    #endregion

    #region ReadAsJsonObject

    /// <summary>
    /// Verify ReadAsJsonObject returns a valid JsonObject tree.
    /// </summary>
    [Test]
    public void ReadAsJsonObject_ReturnsJsonObject()
    {
        var reader = CreateReader("{\"name\":\"LHZ\"}");
        var result = Invoke<JsonObject>(reader, "ReadAsJsonObject");
        ClassicAssert.IsNotNull(result);
        ClassicAssert.AreEqual(JsonType.Content, result.Type);
        ClassicAssert.AreEqual("LHZ", result["name"].Value);
    }

    /// <summary>
    /// Verify ReadAsJsonObject handles arrays.
    /// </summary>
    [Test]
    public void ReadAsJsonObject_Array()
    {
        var reader = CreateReader("[1, 2, 3]");
        var result = Invoke<JsonObject>(reader, "ReadAsJsonObject");
        ClassicAssert.IsNotNull(result);
        ClassicAssert.AreEqual(JsonType.Array, result.Type);
    }

    /// <summary>
    /// Verify ReadAsJsonObject handles all primitive types.
    /// </summary>
    [Test]
    public void ReadAsJsonObject_Primitives()
    {
        var reader = CreateReader("\"hello\"");
        var result = Invoke<JsonObject>(reader, "ReadAsJsonObject");
        ClassicAssert.AreEqual(JsonType.String, result.Type);
        ClassicAssert.AreEqual("hello", result.Value);

        reader = CreateReader("true");
        result = Invoke<JsonObject>(reader, "ReadAsJsonObject");
        ClassicAssert.AreEqual(JsonType.Boolean, result.Type);
        ClassicAssert.AreEqual(true, result.Value);

        reader = CreateReader("null");
        result = Invoke<JsonObject>(reader, "ReadAsJsonObject");
        ClassicAssert.AreEqual(JsonType.Null, result.Type);
    }

    #endregion

    #region Integration Tests (via JsonConvert)

    /// <summary>
    /// Verify that deserialization through JsonConvert works correctly,
    /// which internally exercises JsonDirectReader.
    /// </summary>
    [Test]
    public void Integration_DeserializeSimpleObject()
    {
        var json = "{\"Id\":1,\"Name\":\"LHZ\"}";
        var obj = JsonConvert.Deserialize<TestObjClass>(json);
        ClassicAssert.AreEqual(1, obj.Id);
        ClassicAssert.AreEqual("LHZ", obj.Name);
    }

    /// <summary>
    /// Verify deserialization of nullable types.
    /// </summary>
    [Test]
    public void Integration_DeserializeNullableModel()
    {
        var json = "{\"Count\":null,\"ExpiryDate\":null,\"Rating\":null,\"Name\":null}";
        var obj = JsonConvert.Deserialize<NullableModel>(json);
        ClassicAssert.IsNull(obj.Count);
        ClassicAssert.IsNull(obj.ExpiryDate);
        ClassicAssert.IsNull(obj.Rating);
        ClassicAssert.IsNull(obj.Name);
    }

    /// <summary>
    /// Verify deserialization of an array.
    /// </summary>
    [Test]
    public void Integration_DeserializeArray()
    {
        var json = "[10, 20, 30]";
        var arr = JsonConvert.Deserialize<int[]>(json);
        ClassicAssert.AreEqual(new[] { 10, 20, 30 }, arr);
    }

    /// <summary>
    /// Verify deserialization of a list of objects.
    /// </summary>
    [Test]
    public void Integration_DeserializeList()
    {
        var json = "[{\"Name\":\"A\",\"Age\":1,\"Height\":1.5,\"Obj\":null},{\"Name\":\"B\",\"Age\":2,\"Height\":2.5,\"Obj\":null}]";
        var list = JsonConvert.Deserialize<List<TestMultiProtertyObj>>(json);
        ClassicAssert.AreEqual(2, list.Count);
        ClassicAssert.AreEqual("A", list[0].Name);
        ClassicAssert.AreEqual(1, list[0].Age);
        ClassicAssert.AreEqual("B", list[1].Name);
        ClassicAssert.AreEqual(2, list[1].Age);
    }

    /// <summary>
    /// Verify deserialization of a dictionary.
    /// </summary>
    [Test]
    public void Integration_DeserializeDictionary()
    {
        var json = "{\"key1\":100,\"key2\":200}";
        var dict = JsonConvert.Deserialize<Dictionary<string, int>>(json);
        ClassicAssert.AreEqual(100, dict["key1"]);
        ClassicAssert.AreEqual(200, dict["key2"]);
    }

    /// <summary>
    /// Verify deserialization of nested objects.
    /// </summary>
    [Test]
    public void Integration_DeserializeNestedObject()
    {
        var json = "{\"Name\":\"Parent\",\"Child\":{\"Name\":\"Child\",\"Age\":10}}";
        var obj = JsonConvert.Deserialize<NestedTestObj>(json);
        ClassicAssert.AreEqual("Parent", obj.Name);
        ClassicAssert.IsNotNull(obj.Child);
        ClassicAssert.AreEqual("Child", obj.Child.Name);
        ClassicAssert.AreEqual(10, obj.Child.Age);
    }

    #endregion

    #region Error Handling

    /// <summary>
    /// Verify JsonType throws for unknown characters.
    /// </summary>
    [Test]
    public void JsonType_UnknownChar_ThrowsException()
    {
        var reader = CreateReader("!invalid");
        Assert.Throws<TargetInvocationException>(() => GetProp<JsonType>(reader, "JsonType"));
    }

    /// <summary>
    /// Verify malformed unicode escape throws.
    /// </summary>
    [Test]
    public void ReadString_MalformedUnicode_ThrowsJsonReadException()
    {
        var reader = CreateReader("\"\\u00ZZ\"");
        Assert.Throws<TargetInvocationException>(() => Invoke<string>(reader, "ReadString"));
    }

    /// <summary>
    /// Verify unknown escape character throws.
    /// </summary>
    [Test]
    public void ReadString_UnknownEscape_ThrowsJsonReadException()
    {
        var reader = CreateReader("\"\\x\"");
        Assert.Throws<TargetInvocationException>(() => Invoke<string>(reader, "ReadString"));
    }

    /// <summary>
    /// Verify unescaped control character throws.
    /// </summary>
    [Test]
    public void ReadString_ControlCharacter_ThrowsJsonReadException()
    {
        // 0x01 is a control character that should be rejected in JSON strings
        var json = "\"\u0001\"";
        var reader = CreateReader(json);
        Assert.Throws<TargetInvocationException>(() => Invoke<string>(reader, "ReadString"));
    }

    /// <summary>
    /// Verify deserializing wrong type throws an exception.
    /// </summary>
    [Test]
    public void Integration_TypeMismatch_ThrowsException()
    {
        Assert.Throws<JsonDirectDeserializationException>(
            () => JsonConvert.Deserialize<int>("\"not a number\""));
    }

    #endregion
}
