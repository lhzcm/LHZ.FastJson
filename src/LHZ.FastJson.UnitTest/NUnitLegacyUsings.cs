// NUnit 4 moved the classic assertion API (Assert.AreEqual, Assert.IsTrue, ...)
// into the NUnit.Framework.Legacy namespace. Keep the existing tests source-compatible
// while they are migrated to Assert.That incrementally.
global using NUnit.Framework.Legacy;
