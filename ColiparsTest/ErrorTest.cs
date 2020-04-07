using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Colipars;
using System.Linq;
using System.ComponentModel;
using System.Globalization;
using Colipars.Attribute;
using System.Collections.Generic;
using Colipars.Internal;
using Moq;

namespace Colipars.Test
{
    [TestClass]
    public class ErrorTest
    {
        [TestMethod]
        public void MissingVerbErrorIfNoneIsProvided()
        {
            var errors = Parsers.Setup.ClassAttributes<RequiredOptionCommand>().Parse(new string[0]).Errors;

            Assert.AreEqual(1, errors.Count());
            Assert.IsInstanceOfType(errors.First(), typeof(VerbIsMissingError));
        }

        [TestMethod]
        public void ShowHelpIfVerbIsMissingAndAutoHelpIsEnabled()
        {
            Mock<IHelpPresenter> mock = new Mock<IHelpPresenter>();
            mock.Setup(a => a.Present());

            var errors = Parsers.Setup.ClassAttributes<RequiredOptionCommand>((c) => {
                c.ShowHelpOnMissingVerb = true;
                ((ServiceProvider)c.Services).Register(mock.Object);
            }).Parse(new string[0]).Errors;

            Assert.AreEqual(0, errors.Count());
            mock.VerifyAll();
        }

        [TestMethod]
        public void ErrorIfRequiredOptionIsMissing()
        {
            var errors = Parsers.Setup.ClassAttributes<RequiredOptionCommand>((c) => c.UseAsDefault<RequiredOptionCommand>()).Parse("BoolValue true".Split()).Errors;

            Assert.AreEqual(1, errors.Count());
            Assert.IsInstanceOfType(errors.First(), typeof(RequiredParameterMissingError));
        }

        [TestMethod]
        public void LessThanMinimumCountError()
        {
            var errors = Parsers.Setup.ClassAttributes<ListCommand>((c) => c.UseAsDefault<ListCommand>()).Parse("-n 10 20".Split()).Errors;

            Assert.AreEqual(errors.Count(), 1);
            Assert.IsInstanceOfType(errors.First(), typeof(NotEnoughElementsError));
        }

        [TestMethod]
        public void DontMapOnError()
        {
            //MinimumCount is 3
            Parsers.Setup.ClassAttributes<ListCommand>().Parse("ListCommand -n 10".Split()).Map((ListCommand c) => { Assert.Fail("Called map even though there is an error"); return 1; });
        }

        [TestMethod]
        public void WriteErrorToConsoleIfRequiredOptionIsMissing()
        {
            Mock<ErrorHandler> mock = new Mock<ErrorHandler>();
            mock.Setup(a => a(It.Is<IError[]>((errors) => errors.Length == 1 && errors.First().GetType() == typeof(RequiredParameterMissingError)))).Returns(1);

            Parsers.Setup.ClassAttributes<RequiredOptionCommand>((c) =>
            {
                c.UseAsDefault<RequiredOptionCommand>();
                ((ServiceProvider)c.Services).Register(mock.Object);
            }).Parse("BoolValue true".Split()).Map<RequiredOptionCommand>((option) => { Assert.Fail("Called map even though there is an error"); return 1; });

            mock.VerifyAll();
        }

        [TestMethod]
        public void TryMapWithoutError()
        {
            Parsers.Setup.ClassAttributes<RequiredOptionCommand>().Parse("required --BoolValue true --IntValue 2".Split()).TryMap((RequiredOptionCommand x) => 12, out int exitCode);

            Assert.AreEqual(12, exitCode);
        }

        [TestMethod]
        public void TryMapWithException()
        {
            Mock<ErrorHandler> mock = new Mock<ErrorHandler>();
            mock.Setup(a => a(It.IsAny<IError[]>())).Returns(1);

            Parsers.Setup.ClassAttributes<RequiredOptionCommand>((c) => ((ServiceProvider)c.Services).Register<ErrorHandler>(mock.Object)).Parse("required --BoolValue true --IntValue 2".Split()).TryMap((RequiredOptionCommand x) => throw new Exception(), out int exitCode);

            Assert.AreEqual(1, exitCode);
            mock.VerifyAll();
        }

        [Verb("required")]
        class RequiredOptionCommand
        {
            [NamedOption("BoolValue", Required = true)]
            public bool BoolValue { get; set; }

            [NamedOption("IntValue", Required = true)]
            public int IntValue { get; set; }
        }

        [Verb("list")]
        class ListCommand
        {
            [NamedCollectionOption("numbers", Alias = "n", MinimumCount = 3)]
            public List<int> Numbers { get; } = new List<int>();
        }
    }
}
