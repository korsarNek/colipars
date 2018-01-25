﻿using System;
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
            Assert.AreEqual(0, Parsers.Setup.MethodAttributes<Container>((c) => c.UseAsDefault<Container>(nameof(Container.NumbersTest))).Parse("--numbers 10 20 -30".Split()).Execute());
        }

        [TestMethod]
        public void ExceptionOnTryExecute()
        {
            var success = Parsers.Setup.MethodAttributes<Container>().Parse("exception".Split()).TryExecute(out int exitCode);
            Assert.IsFalse(success);
            Assert.AreEqual(1, exitCode);
        }

        [TestMethod]
        public void ErrorOnMissingVerb()
        {
            var errors = Parsers.Setup.MethodAttributes<Container>().Parse("".Split()).Errors;
            Assert.AreEqual(1, errors.Count());
            Assert.IsInstanceOfType(errors.First(), typeof(VerbIsMissingError));
        }

        [TestMethod]
        public void ParseNamedOptionAfterNamedCollectionOption()
        {
            Assert.AreEqual(0, Parsers.Setup.MethodAttributes<Container>().Parse("NamedAfterNamedCollection --files testfile1.png testfile2.png --output outputfile.png".Split()).Execute());
        }

        [TestMethod]
        public void ParsePositionalOptionAfterNamedCollectionOption()
        {
            //The case of a positionalOption after a NamedCollection does not work, at least in the provided string collection, se we have to specify the positional one before.
            Assert.AreEqual(0, Parsers.Setup.MethodAttributes<Container>().Parse("PositionalAfterNamedCollection outputfile.png --files testfile1.png testfile2.png".Split()).Execute());
        }

        class Container
        {
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

        }
    }
}
