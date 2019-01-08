using System;
using System.IO;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.DataAccess.Poco.Tests
{
    [TestFixture]
    internal static class PocoViewGeneratorTests
    {
        [Test]
        public static void Ctor_GivenNullNameTranslator_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new PocoViewGenerator(null, "testns"));
        }

        [Test]
        public static void Ctor_GivenNullNamespace_ThrowsArgumentNullException()
        {
            var nameTranslator = new VerbatimNameTranslator();
            Assert.Throws<ArgumentNullException>(() => new PocoViewGenerator(nameTranslator, null));
        }

        [Test]
        public static void Ctor_GivenEmptyNamespace_ThrowsArgumentNullException()
        {
            var nameTranslator = new VerbatimNameTranslator();
            Assert.Throws<ArgumentNullException>(() => new PocoViewGenerator(nameTranslator, string.Empty));
        }

        [Test]
        public static void Ctor_GivenWhiteSpaceNamespace_ThrowsArgumentNullException()
        {
            var nameTranslator = new VerbatimNameTranslator();
            Assert.Throws<ArgumentNullException>(() => new PocoViewGenerator(nameTranslator, "   "));
        }

        [Test]
        public static void Ctor_GivenNullIndent_ThrowsArgumentNullException()
        {
            var nameTranslator = new VerbatimNameTranslator();
            Assert.Throws<ArgumentNullException>(() => new PocoViewGenerator(nameTranslator, "testns", null));
        }

        [Test]
        public static void GetFilePath_GivenNullObjectName_ThrowsArgumentNullException()
        {
            var nameTranslator = new VerbatimNameTranslator();
            const string testNs = "SJP.Schematic.Test";
            var generator = new PocoViewGenerator(nameTranslator, testNs);
            var baseDir = new DirectoryInfo(Environment.CurrentDirectory);

            Assert.Throws<ArgumentNullException>(() => generator.GetFilePath(baseDir, null));
        }

        [Test]
        public static void GetFilePath_GivenNameWithOnlyLocalName_ReturnsExpectedPath()
        {
            var nameTranslator = new VerbatimNameTranslator();
            const string testNs = "SJP.Schematic.Test";
            var generator = new PocoViewGenerator(nameTranslator, testNs);
            var baseDir = new DirectoryInfo(Environment.CurrentDirectory);
            const string testViewName = "view_name";
            var expectedPath = Path.Combine(Environment.CurrentDirectory, "Views", testViewName + ".cs");

            var filePath = generator.GetFilePath(baseDir, testViewName);

            Assert.AreEqual(expectedPath, filePath.FullName);
        }

        [Test]
        public static void GetFilePath_GivenNameWithSchemaAndLocalName_ReturnsExpectedPath()
        {
            var nameTranslator = new VerbatimNameTranslator();
            const string testNs = "SJP.Schematic.Test";
            var generator = new PocoViewGenerator(nameTranslator, testNs);
            var baseDir = new DirectoryInfo(Environment.CurrentDirectory);
            const string testViewSchema = "view_schema";
            const string testViewName = "view_name";
            var expectedPath = Path.Combine(Environment.CurrentDirectory, "Views", testViewSchema, testViewName + ".cs");

            var filePath = generator.GetFilePath(baseDir, new Identifier(testViewSchema, testViewName));

            Assert.AreEqual(expectedPath, filePath.FullName);
        }

        [Test]
        public static void Generate_GivenNullView_ThrowsArgumentNullException()
        {
            var nameTranslator = new VerbatimNameTranslator();
            const string testNs = "SJP.Schematic.Test";
            var generator = new PocoViewGenerator(nameTranslator, testNs);

            Assert.Throws<ArgumentNullException>(() => generator.Generate(null));
        }
    }
}
