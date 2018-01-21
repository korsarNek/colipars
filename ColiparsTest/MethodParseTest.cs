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
            Assert.AreEqual(0, Parsers.Setup.MethodAttributes<Container>((c) => c.UseAsDefault<Container>(nameof(Container.NumbersTest))).Parse("--numbers 10 20 -30".Split()).Execute());
        }

        class Container
        {
            [Verb("numbers")]
            public int NumbersTest([NamedCollectionOption("numbers")] IEnumerable<int> numbers)
            {
                CollectionAssert.AreEqual(new[] { 10, 20, -30 }, numbers.ToArray());
                return 0;
            }
        }
    }
}
