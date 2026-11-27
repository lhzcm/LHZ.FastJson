using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using LHZ.FastJson.Enum;
using LHZ.FastJson.Exceptions;
using NUnit.Framework;

namespace LHZ.FastJson.UnitTest
{
    /// <summary>
    /// The exception contract: which exception types the library throws, what they carry, and that their messages are English.
    /// <list type="bullet">
    /// <item><description><see cref="JsonReadException"/>: the JSON text is malformed or ends unexpectedly.</description></item>
    /// <item><description><see cref="JsonDeserializationException"/> (derives from it): valid JSON that cannot be converted to the requested type.</description></item>
    /// <item><description><see cref="JsonSerializationException"/>: an object cannot be serialized.</description></item>
    /// </list>
    /// </summary>
    public class ExceptionContractTests
    {
        public enum ExColor { Red = 0, Green = 1 }

        public class ExModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public ExModel Child { get; set; }
        }

        private static T De<T>(string json)
        {
            return JsonConvert.Deserialize<T>(json.Replace('\'', '"'));
        }

        /// <summary>
        /// Deserialize to a type chosen at run time and return the exception that was thrown (null when none).
        /// </summary>
        private static Exception Catch(Type target, string json)
        {
            var method = typeof(JsonConvert).GetMethods().First(m => m.Name == "Deserialize" && m.IsGenericMethodDefinition && m.GetParameters().Length == 1);
            try
            {
                method.MakeGenericMethod(target).Invoke(null, new object[] { json.Replace('\'', '"') });
                return null;
            }
            catch (TargetInvocationException e)
            {
                return e.InnerException;
            }
        }

        private static bool IsAscii(string text)
        {
            return text.All(c => c < 128);
        }

        #region Hierarchy

        [Test]
        public void ExceptionHierarchy()
        {
            Assert.That(typeof(JsonReadException).BaseType, Is.EqualTo(typeof(Exception)));
            Assert.That(typeof(JsonDeserializationException).BaseType, Is.EqualTo(typeof(JsonReadException)));
            Assert.That(typeof(JsonDirectDeserializationException).BaseType, Is.EqualTo(typeof(JsonDeserializationException)));
            Assert.That(typeof(JsonSerializationException).BaseType, Is.EqualTo(typeof(Exception)));
        }

        [Test]
        public void TypeMismatchCanBeCaughtAsDeserializationOrReadException()
        {
            Assert.That(() => De<int>("'x'"), Throws.InstanceOf<JsonDeserializationException>());
            Assert.That(() => De<int>("'x'"), Throws.InstanceOf<JsonReadException>());
            Assert.That(() => De<int>("'x'"), Throws.TypeOf<JsonDirectDeserializationException>());
        }

        [Test]
        public void MalformedJsonIsAReadExceptionButNotADeserializationException()
        {
            var ex = Assert.Throws<JsonReadException>(() => De<ExModel>("{'Id':1,}"));
            Assert.That(ex, Is.Not.InstanceOf<JsonDeserializationException>());
        }

        #endregion

        #region What the exceptions carry

        [Test]
        public void TypeMismatchCarriesPositionJsonTypeAndTargetType()
        {
            var ex = Assert.Catch<JsonDeserializationException>(() => De<ExModel>("{'Id':'x'}"));
            Assert.That(ex.Position, Is.EqualTo(6));
            Assert.That(ex.JsonType, Is.EqualTo(JsonType.String));
            Assert.That(ex.TargetType, Is.EqualTo(typeof(int)));
            Assert.That(ex.Message, Does.Contain("string").And.Contain("Int32").And.Contain("number").And.Contain("position 6"));
        }

        [TestCase("[1]", typeof(Dictionary<string, int>), JsonType.Array)]
        [TestCase("{'a':1}", typeof(int[]), JsonType.Content)]
        [TestCase("5", typeof(List<int>), JsonType.Number)]
        [TestCase("true", typeof(ExModel), JsonType.Boolean)]
        public void CollectionAndObjectMismatchCarryTheActualJsonType(string json, Type target, JsonType actual)
        {
            var ex = (JsonDeserializationException)Catch(target, json);
            Assert.That(ex, Is.Not.Null);
            Assert.That(ex.JsonType, Is.EqualTo(actual));
            Assert.That(ex.TargetType, Is.EqualTo(target));
            Assert.That(ex.Position, Is.EqualTo(0));
        }

        [Test]
        public void ReadExceptionCarriesThePositionOfTheProblem()
        {
            Assert.That(Assert.Throws<JsonReadException>(() => De<ExModel>("{'Id':1,}")).Position, Is.EqualTo(8));
            Assert.That(Assert.Throws<JsonReadException>(() => De<int[]>("[1,2,x]")).Position, Is.EqualTo(5));
            Assert.That(Assert.Throws<JsonReadException>(() => De<string>("'abc")).Position, Is.EqualTo(4));
        }

        [Test]
        public void MessageEndsWithThePosition()
        {
            var ex = Assert.Throws<JsonReadException>(() => De<string>("'abc"));
            Assert.That(ex.Message, Is.EqualTo("Unterminated string at position 4."));
        }

        [Test]
        public void ControlCharactersInMessagesAreEscaped()
        {
            var ex = Assert.Throws<JsonReadException>(() => De<int[]>("[1\u0001]"));
            Assert.That(ex.Message, Does.Contain("'\\u0001'"));
            Assert.That(ex.Message.Any(char.IsControl), Is.False);
        }

        #endregion

        #region Conversion failures are wrapped instead of leaking BCL exceptions

        [TestCase(typeof(int), "3000000000")]
        [TestCase(typeof(int), "-2147483649")]
        [TestCase(typeof(int), "1.5")]
        [TestCase(typeof(int), "1e2")]
        [TestCase(typeof(byte), "256")]
        [TestCase(typeof(byte), "-1")]
        [TestCase(typeof(short), "32768")]
        [TestCase(typeof(ushort), "-1")]
        [TestCase(typeof(uint), "-1")]
        [TestCase(typeof(long), "9223372036854775808")]
        [TestCase(typeof(ulong), "18446744073709551616")]
        [TestCase(typeof(decimal), "79228162514264337593543950336")]
        public void NumbersThatDoNotFitAreWrapped(Type target, string json)
        {
            var ex = Catch(target, json);
            Assert.That(ex, Is.InstanceOf<JsonDeserializationException>(), target.Name + " <- " + json);
            var deserializationException = (JsonDeserializationException)ex;
            Assert.That(deserializationException.TargetType, Is.EqualTo(target));
            Assert.That(deserializationException.Position, Is.EqualTo(0));
            //The original exception stays available for diagnostics, and the message names the offending number
            Assert.That(ex.InnerException, Is.Not.Null);
            Assert.That(ex.Message, Does.Contain("'" + json + "'"));
        }

        [Test]
        public void ConversionFailureInsideAnObjectReportsThePositionOfTheValue()
        {
            var ex = Assert.Catch<JsonDeserializationException>(() => De<ExModel>("{'Name':'n','Id':3000000000}"));
            Assert.That(ex.Position, Is.EqualTo(17));
            Assert.That(ex.JsonType, Is.EqualTo(JsonType.Number));
            Assert.That(ex.TargetType, Is.EqualTo(typeof(int)));
            Assert.That(ex.InnerException, Is.InstanceOf<OverflowException>());
        }

        [Test]
        public void OverflowAndFormatErrorsKeepTheirOriginalExceptionAsInner()
        {
            Assert.That(Catch(typeof(int), "3000000000").InnerException, Is.InstanceOf<OverflowException>());
            Assert.That(Catch(typeof(int), "1.5").InnerException, Is.InstanceOf<FormatException>());
        }

        [Test]
        public void InvalidEnumGuidAndCharValuesAreWrapped()
        {
            var name = (JsonDeserializationException)Catch(typeof(ExColor), "'Purple'");
            Assert.That(name.TargetType, Is.EqualTo(typeof(ExColor)));
            Assert.That(name.Message, Does.Contain("Purple").And.Contain("ExColor"));
            Assert.That(name.InnerException, Is.Not.Null);

            var undefined = (JsonDeserializationException)Catch(typeof(ExColor), "99");
            Assert.That(undefined.Message, Does.Contain("99").And.Contain("not defined"));

            var guid = (JsonDeserializationException)Catch(typeof(Guid), "'nope'");
            Assert.That(guid.TargetType, Is.EqualTo(typeof(Guid)));
            Assert.That(guid.Message, Does.Contain("nope").And.Contain("GUID"));
            Assert.That(guid.InnerException, Is.InstanceOf<FormatException>());

            var character = (JsonDeserializationException)Catch(typeof(char), "'ab'");
            Assert.That(character.TargetType, Is.EqualTo(typeof(char)));
            Assert.That(character.Message, Does.Contain("exactly one character"));
        }

        public enum ExByteEnum : byte { A, B }

        [Test]
        public void EnumOfAnotherUnderlyingTypeIsReportedAsDeserializationError()
        {
            //Not supported yet (see KnownIssueTests), but it must fail with the library's exception, not a raw ArgumentException
            var ex = Catch(typeof(ExByteEnum), "1");
            Assert.That(ex, Is.InstanceOf<JsonDeserializationException>());
            Assert.That(((JsonDeserializationException)ex).TargetType, Is.EqualTo(typeof(ExByteEnum)));
            Assert.That(ex.InnerException, Is.InstanceOf<ArgumentException>());
        }

        [Test]
        public void InvalidDictionaryKeysAndDuplicatesAreWrapped()
        {
            var badKey = (JsonDeserializationException)Catch(typeof(Dictionary<int, string>), "{'x':'a'}");
            Assert.That(badKey.Message, Does.Contain("'x'").And.Contain("Int32"));
            Assert.That(badKey.InnerException, Is.InstanceOf<FormatException>());

            var duplicate = (JsonDeserializationException)Catch(typeof(Dictionary<string, int>), "{'a':1,'a':2}");
            Assert.That(duplicate.Message, Does.Contain("already contains the key 'a'"));
            Assert.That(duplicate.InnerException, Is.InstanceOf<ArgumentException>());
        }

        [Test]
        public void TypesThatCannotBeDeserializedAreReportedWithTheTypeName()
        {
            var ex = (JsonDeserializationException)Catch(typeof(ReadOnlyCollection<int>), "[1]");
            Assert.That(ex.TargetType, Is.EqualTo(typeof(ReadOnlyCollection<int>)));
            Assert.That(ex.Message, Does.Contain("ReadOnlyCollection<Int32>").And.Contain("not supported"));
            Assert.That(ex.InnerException, Is.Not.Null);
        }

        [Test]
        public void ExceptionsForUnconstructableTypesNameTheTypeAndThePosition()
        {
            var noCtor = (JsonDeserializationException)Catch(typeof(NoParameterlessConstructor), "{'V':1}");
            Assert.That(noCtor.Message, Does.Contain("NoParameterlessConstructor").And.Contain("parameterless constructor"));

            var abstractType = (JsonDeserializationException)Catch(typeof(IDisposable), "{}");
            Assert.That(abstractType.Message, Does.Contain("IDisposable").And.Contain("interface or an abstract type"));
        }

        public class NoParameterlessConstructor
        {
            public NoParameterlessConstructor(int v) { V = v; }
            public int V { get; set; }
        }

        public struct ExPoint { public int X { get; set; } }

        [Test]
        public void NullForAValueTypeIsReportedWithTheTypeName()
        {
            var ex = Assert.Catch<JsonDeserializationException>(() => De<ExPoint>("null"));
            Assert.That(ex.JsonType, Is.EqualTo(JsonType.Null));
            Assert.That(ex.Message, Does.Contain("null").And.Contain("value type").And.Contain("ExPoint"));
        }

        #endregion

        #region Messages are English

        private static readonly string[] MalformedInputs =
        {
            "", "   ", "{", "}", "[", "]", "{'a'", "{'a' ", "{'a':", "{'a':1", "{'a':1,", "{'a':1,}", "{'a' 1}", "{a:1}", "{1:2}",
            "[1,", "[1,]", "[,1]", "[1 2]", "[1,,2]", "['a", "['a\\", "['a\\u12", "['a\\q']", "['\\u12G4']", "['a\nb']",
            "tru", "fals", "nul", "TRUE", "-", "01", "1.", "1e", ".5", "+1", "--1", "NaN", "{'a':tru}", "{'a':nul}",
            "'abc", "'abc\\", "x", "{'a':1}}x", "{'a':{'b':", "[[[", "{'a':[1,{'b':2}",
        };

        [Test]
        public void MessagesOfMalformedInputAreEnglishAndHaveAPosition()
        {
            int checkedCount = 0;
            foreach (var raw in MalformedInputs)
            {
                string json = raw.Replace('\'', '"');
                foreach (var target in new[] { typeof(ExModel), typeof(int[]), typeof(Dictionary<string, int>), typeof(string), typeof(object) })
                {
                    var ex = Catch(target, json);
                    if (ex == null)
                    {
                        continue; //trailing content after a complete value is tolerated, see CHANGELOG 2.0.0
                    }
                    Assert.That(ex, Is.InstanceOf<JsonReadException>(), target.Name + " <- " + raw);
                    Assert.That(IsAscii(ex.Message), Is.True, "Message is not ASCII: " + ex.Message);
                    Assert.That(ex.Message, Does.EndWith("."), ex.Message);
                    checkedCount++;
                }
            }
            Assert.That(checkedCount, Is.GreaterThan(150));
        }

        [Test]
        public void UntypedParsingReportsTheSameKindOfErrors()
        {
            foreach (var raw in MalformedInputs.Where(s => s.Trim().Length > 0))
            {
                Exception ex = null;
                try
                {
                    JsonConvert.Deserialize(raw.Replace('\'', '"'));
                }
                catch (Exception e)
                {
                    ex = e;
                }
                if (ex == null)
                {
                    continue;
                }
                Assert.That(ex, Is.InstanceOf<JsonReadException>(), raw);
                Assert.That(IsAscii(ex.Message), Is.True, ex.Message);
                Assert.That(ex.Message, Does.Contain("position"), ex.Message);
            }
        }

        [Test]
        public void SerializationMessagesAreEnglish()
        {
            var node = new ExModel();
            node.Child = node;
            var ex = Assert.Throws<JsonSerializationException>(() => JsonConvert.Serialize(node));
            Assert.That(ex.Message, Is.EqualTo("A circular reference was detected while serializing an object of type 'ExModel'."));
            Assert.That(ex.SerializedType, Is.EqualTo(typeof(ExModel)));
        }

        [Test]
        public void ArgumentExceptionsHaveEnglishMessagesAndParameterNames()
        {
            var empty = Assert.Throws<ArgumentNullException>(() => new JsonReader("").JsonRead());
            Assert.That(empty.ParamName, Is.EqualTo("jsonString"));
            Assert.That(empty.Message, Does.Contain("must not be null or empty"));

            var duplicate = Assert.Throws<ArgumentException>(() =>
            {
                var content = new JsonClass.JsonContent();
                content.AddJsonProperty("a", JsonClass.JsonNull.Null);
                content.AddJsonProperty("a", JsonClass.JsonNull.Null);
            });
            Assert.That(duplicate.ParamName, Is.EqualTo("jsonPropertyName"));
            Assert.That(duplicate.Message, Does.Contain("already exists").And.Contain("'a'"));

            var content2 = new JsonClass.JsonContent();
            var range = Assert.Throws<ArgumentOutOfRangeException>(() => { var x = content2[5]; });
            Assert.That(range.ParamName, Is.EqualTo("index"));
        }

        [Test]
        public void IndexerMisuseHasEnglishMessages()
        {
            var array = JsonConvert.Deserialize("[1]");
            var byString = Assert.Throws<InvalidOperationException>(() => { var x = array["a"]; });
            Assert.That(byString.Message, Does.Contain("string indexer").And.Contain("Content").And.Contain("Array"));

            var number = JsonConvert.Deserialize("5");
            var byIndex = Assert.Throws<InvalidOperationException>(() => { var x = number[0]; });
            Assert.That(byIndex.Message, Does.Contain("numeric indexer").And.Contain("Number"));
        }

        [Test]
        public void UntypedDuplicatePropertyIsAReadExceptionWithAPosition()
        {
            var ex = Assert.Throws<JsonReadException>(() => JsonConvert.Deserialize("{\"a\":1,\"a\":2}"));
            Assert.That(ex.Position, Is.EqualTo(7));
            Assert.That(ex.Message, Is.EqualTo("Duplicate property name 'a' in a JSON object at position 7."));
            Assert.That(ex.InnerException, Is.InstanceOf<ArgumentException>());
        }

        #endregion

        #region Source guard

        /// <summary>
        /// The library is meant for an international audience: exception messages, comments and package metadata
        /// must not contain Chinese text. (Localized documentation lives in the README_CN / wiki / CHANGELOG_CN files.)
        /// </summary>
        [Test]
        public void LibrarySourceContainsNoChineseText()
        {
            string sourceDirectory = FindLibrarySourceDirectory();
            if (sourceDirectory == null)
            {
                Assert.Ignore("The library source directory was not found next to the test assembly.");
            }

            var offenders = new List<string>();
            foreach (var file in Directory.EnumerateFiles(sourceDirectory, "*.*", SearchOption.AllDirectories)
                .Where(f => f.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) || f.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
                .Where(f => f.IndexOf(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) < 0
                         && f.IndexOf(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) < 0))
            {
                int lineNumber = 0;
                foreach (var line in File.ReadLines(file))
                {
                    lineNumber++;
                    if (line.Any(IsChineseOrFullWidth))
                    {
                        offenders.Add(Path.GetFileName(file) + ":" + lineNumber);
                    }
                }
            }
            Assert.That(offenders, Is.Empty, "Chinese text found in: " + string.Join(", ", offenders.Take(20)));
        }

        private static bool IsChineseOrFullWidth(char c)
        {
            return (c >= 0x4E00 && c <= 0x9FFF)      //CJK unified ideographs
                || (c >= 0x3000 && c <= 0x303F)      //CJK punctuation
                || (c >= 0xFF00 && c <= 0xFFEF)      //full-width forms
                || (c >= 0x2018 && c <= 0x201D);     //curly quotes used in the old Chinese messages
        }

        private static string FindLibrarySourceDirectory()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null)
            {
                string candidate = Path.Combine(directory.FullName, "LHZ.FastJson");
                if (File.Exists(Path.Combine(candidate, "LHZ.FastJson.csproj")))
                {
                    return candidate;
                }
                directory = directory.Parent;
            }
            return null;
        }

        #endregion
    }
}
