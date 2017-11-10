using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Colipars;
using System.Linq;
using System.ComponentModel;
using System.Globalization;
using Colipars.Attribute;
using System.Collections.Generic;

namespace Colipars.Test
{
    [TestClass]
    public class ErrorTest
    {
        [TestMethod]
        public void ShowHelpIfVerbIsMissing()
        {
            var errors = Parsers.Setup.Attributes<RequiredOptionCommand>().Parse(new string[0]).Errors;

            Assert.AreEqual(errors.Count(), 1);
            Assert.IsInstanceOfType(errors.First(), typeof(VerbIsMissingError));
        }

        [TestMethod]
        public void ShowHelpIfRequiredOptionIsMissing()
        {
            var errors = Parsers.Setup.Attributes<RequiredOptionCommand>((c) => c.UseAsDefault<RequiredOptionCommand>()).Parse("BoolValue true".Split()).Errors;

            Assert.AreEqual(errors.Count(), 1);
            Assert.IsInstanceOfType(errors.First(), typeof(RequiredParameterMissingError));
        }

        [TestMethod]
        public void LessThanMinimumCountError()
        {
            var errors = Parsers.Setup.Attributes<ListCommand>((c) => c.UseAsDefault<ListCommand>()).Parse("-n 10 20".Split()).Errors;

            Assert.AreEqual(errors.Count(), 1);
            Assert.IsInstanceOfType(errors.First(), typeof(NotEnoughElementsError));
        }

        [TestMethod]
        public void DontMapOnError()
        {
            //MinimumCount is 3
            Parsers.Setup.Attributes<ListCommand>().Parse("ListCommand -n 10".Split()).Map((ListCommand c) => { Assert.Fail("Called map even though there is an error"); return 1; });
        }

        [Verb("required")]
        class RequiredOptionCommand
        {
            [NamedOption("BoolValue", Required = true)]
            public bool BoolValue { get; set; }

            [NamedOption("IntValue", Required = true)]
            public int IntValue { get; set; }
        }

        [Verb]
        class ListCommand
        {
            [NamedCollectionOption("numbers", Alias = "n", MinimumCount = 3)]
            public List<int> Numbers { get; } = new List<int>();
        }
    }
}
