using System;
using System.Collections.Generic;
using System.Linq;
using Colipars.Attribute;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Colipars.Test
{
    [TestClass]
    public class MethodParseTest
    {
        [TestMethod]
        public void ParseNumbers()
        {
            Assert.AreEqual(0, Cli.Setup.MethodAttributes<Container>((c) => c.UseAsDefault<Container>(nameof(Container.NumbersTest))).Parse("--numbers 10 20 -30".Split()).Execute());
        }

        [TestMethod]
        public void ExceptionOnTryExecute()
        {
            var success = Cli.Setup.MethodAttributes<Container>().Parse("exception".Split()).TryExecute(out int exitCode);
            Assert.IsFalse(success);
            Assert.AreEqual(1, exitCode);
        }

        [TestMethod]
        public void ErrorOnMissingVerb()
        {
            var errors = Cli.Setup.MethodAttributes<Container>().Parse("".Split()).Errors;
            Assert.AreEqual(1, errors.Count());
            Assert.IsInstanceOfType(errors.First(), typeof(VerbIsMissingError));
        }

        [TestMethod]
        public void ParseNamedOptionAfterNamedCollectionOption()
        {
            Assert.AreEqual(0, Cli.Setup.MethodAttributes<Container>().Parse("NamedAfterNamedCollection --files testfile1.png testfile2.png --output outputfile.png".Split()).Execute());
        }

        [TestMethod]
        public void ParsePositionalOptionAfterNamedCollectionOption()
        {
            //The case of a positionalOption after a NamedCollection does not work, at least in the provided string collection, se we have to specify the positional one before.
            Assert.AreEqual(0, Cli.Setup.MethodAttributes<Container>().Parse("PositionalAfterNamedCollection outputfile.png --files testfile1.png testfile2.png".Split()).Execute());
        }

        [TestMethod]
        public void ExecuteAVerbWithDefaultParameters()
        {
            Assert.AreEqual(0, Cli.Setup.MethodAttributes<Container>().Parse("DefaultParameter --output outputfile.png".Split()).Execute());
        }

        [TestMethod]
        public void StaticMethodWithVerb()
        {
            Assert.AreEqual(2, Cli.Setup.MethodAttributes<Container>().Parse("StaticMethod outputfile.png".Split()).Execute());
        }

        [TestMethod]
        public void VerbOnInstance()
        {
            var container = new Container
            {
                Field = 10
            };
            Assert.AreEqual(2, Cli.Setup.MethodAttributes(instance: container).Parse("Instance".Split()).Execute());
        }

        [TestMethod]
        public void StaticDefaultWithoutVerb()
        {
            Cli.Setup.MethodAttributes(cfg => cfg.UseAsDefault<Container>(nameof(Container.WithoutVerb))).Parse("static".Split()).Execute();
        }

        [TestMethod]
        public void BooleanTrueResult()
        {
            Assert.AreEqual(0, Cli.Setup.MethodAttributes(cfg => cfg.UseAsDefault<Container>(nameof(Container.Boolean))).Parse("--result true".Split()).Execute());
            Assert.AreEqual(1, Cli.Setup.MethodAttributes(cfg => cfg.UseAsDefault<Container>(nameof(Container.Boolean))).Parse("--result false".Split()).Execute());
            Assert.AreEqual(1, Cli.Setup.MethodAttributes(cfg => cfg.UseAsDefault<Container>(nameof(Container.Boolean))).Parse([]).Execute());
        }

        [TestMethod]
        public async Task ExecuteAsync()
        {
            Assert.AreEqual(0, await Cli.Setup.MethodAttributes(cfg => cfg.UseAsDefault<Container>(nameof(Container.Async))).Parse([]).ExecuteAsync());
        }

        [TestMethod]
        public void DefaultWithoutArgs()
        {
            Assert.AreEqual(0, Cli.Setup.MethodAttributes(cfg => cfg.UseAsDefault<Container>(nameof(Container.Empty))).Parse([]).Execute());
        }

        [TestMethod]
        public void NamedBooleanUsedAsFlag()
        {
            Assert.AreEqual(101, Cli.Setup.MethodAttributes(cfg => cfg.UseAsDefault<Container>(nameof(Container.Boolean))).Parse("--result".Split()).Execute());
        }

        [TestMethod]
        public void DirectFunc()
        {
            Cli.Setup.MethodAttributes(cfg => cfg.UseAsDefault(Container.Func)).Parse([]).Execute();
        }

        class Container
        {
            public int Field = 0;

            [Verb("numbers")]
            public int NumbersTest([NamedCollectionOption("numbers")] IEnumerable<int> numbers)
            {
                CollectionAssert.AreEqual(new[] { 10, 20, -30 }, numbers.ToArray());
                return 0;
            }

            [Verb("exception")]
            public void ThrowException()
            {
                throw new Exception();
            }

            [Verb("NamedAfterNamedCollection")]
            public void NamedAfterNamedCollection([NamedCollectionOption("files")] IEnumerable<string> files, [NamedOption("output")] string output)
            {
                CollectionAssert.AreEqual(new[] { "testfile1.png", "testfile2.png" }, files.ToArray());
                Assert.AreEqual("outputfile.png", output);
            }

            [Verb("PositionalAfterNamedCollection")]
            public void PositionalAfterNamedCollection([NamedCollectionOption("files")] IEnumerable<string> files, [PositionalOption(0)] string output)
            {
                CollectionAssert.AreEqual(new[] { "testfile1.png", "testfile2.png" }, files.ToArray());
                Assert.AreEqual("outputfile.png", output);
            }

            [Verb("DefaultParameter")]
            public void DefaultValue([NamedOption("output")] string output, int myNumber = 32)
            {
                Assert.AreEqual(32, myNumber);
                Assert.AreEqual("outputfile.png", output);
            }

            [Verb("StaticMethod")]
            public static int StaticMethod([PositionalOption(0)] string output)
            {
                Assert.AreEqual("outputfile.png", output);
                return 2;
            }

            [Verb("Instance")]
            public int Instance()
            {
                Assert.AreEqual(10, Field);
                return 2;
            }

            public static void WithoutVerb([PositionalOption(0)] string output)
            {
                Assert.AreEqual("static", output);
            }

            [Verb("Async")]
            public static Task<bool> Async()
            {
                return Task.FromResult(true);
            }

            public static bool Boolean([NamedOption("result")] bool result)
            {
                return result;
            }

            public static int Empty()
            {
                return 0;
            }

            public static int Func()
            {
                return 0;
            }
        }
    }
}
