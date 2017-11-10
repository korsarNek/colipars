﻿using Colipars;
using Colipars.Attribute;
using Colipars.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colipars.Test
{
    [TestClass]
    public class ParseTest
    {
        [TestMethod]
        public void ParseNamed()
        {
            var exitCode = Parsers.Setup.Attributes<Command>().Parse("test --value test -n 0.42".Split()).Map((Command command) => command.Execute(), (IEnumerable<IError> errors) => 1);

            Assert.AreEqual(40, exitCode);
        }

        [TestMethod]
        public void ShowGeneralHelp()
        {
            var text = new StringBuilder();

            Parsers.Setup.Attributes<Command>((c) => ((ServiceProvider)c.Services).Register<IHelpPresenter>(new HelpPresenter(text))).ShowHelp();

            Assert.AreEqual("Showed general help.", text.ToString());
        }

        [TestMethod]
        public void ShowSpecificHelp()
        {
            var text = new StringBuilder();

            Parsers.Setup.Attributes<Command>((c) => ((ServiceProvider)c.Services).Register<IHelpPresenter>(new HelpPresenter(text))).ShowHelp<Command>();

            Assert.AreEqual(@"Showed help for ""test""", text.ToString());
        }

        [TestMethod]
        public void ParsePositional()
        {
            var command = Parsers.Setup.Attributes<SetPositionCommand>().Parse("setPosition 0.4 -220.4".Split()).GetVerbObject<SetPositionCommand>();

            Assert.AreEqual(0.4f, command.X);
            Assert.AreEqual(-220.4f, command.Y);
        }

        [TestMethod]
        public void ParseMixed()
        {
            var command = Parsers.Setup.Attributes<PositionalAndNamedCommand>((c) => c.UseAsDefault<PositionalAndNamedCommand>()).Parse("-f true false".Split()).GetVerbObject<PositionalAndNamedCommand>();

            Assert.AreEqual(true, command.IsFlagged);
            Assert.AreEqual(false, command.AnotherFlag);
        }

        [TestMethod]
        public void ParseFlag()
        {
            var command = Parsers.Setup.Attributes<FlagCommand>((c) => c.UseAsDefault<FlagCommand>()).Parse("-v".Split()).GetVerbObject<FlagCommand>();

            Assert.AreEqual(true, command.Verbose);
        }

        [TestMethod]
        public void ParseCustomFlag()
        {
            var command = Parsers.Setup.Attributes<VerbosityCommand>((c) => c.UseAsDefault<VerbosityCommand>()).Parse("-vv".Split()).GetVerbObject<VerbosityCommand>();

            Assert.AreEqual(VerbosityLevel.Extensive, command.Verbosity);
        }

        [TestMethod]
        public void MultipleCommands()
        {
            var verbObj = Parsers.Setup.Attributes<FlagCommand, VerbosityCommand>().Parse("verbosity -vvv".Split()).GetVerbObject();

            Assert.IsInstanceOfType(verbObj, typeof(VerbosityCommand));
            Assert.AreEqual(VerbosityLevel.Debug, ((VerbosityCommand)verbObj).Verbosity);
        }

        [TestMethod]
        public void ParseNegativeNumber()
        {
            var command = Parsers.Setup.Attributes<Command>().Parse("test -n -4.0".Split()).GetVerbObject<Command>();

            Assert.AreEqual(command.Number, -4.0f);
        }

        [TestMethod]
        public void ParseListArgument()
        {
            var result = Parsers.Setup.Attributes<ListCommand>((c) => c.UseAsDefault<ListCommand>()).Parse("-n 10 -20 4".Split());
            var listCommand = result.GetVerbObject<ListCommand>();

            Assert.AreEqual(listCommand.Numbers.Count, 3);
            CollectionAssert.AreEqual(listCommand.Numbers.ToArray(), new[] { 10, -20, 4 });
        }

        [TestMethod]
        public void ParseListArgumentWithFlagParameterAfterwards()
        {
            var result = Parsers.Setup.Attributes<ListCommand>((c) => c.UseAsDefault<ListCommand>()).Parse("-n 4 -4 4 -f".Split());
            var listCommand = result.GetVerbObject<ListCommand>();

            Assert.AreEqual(listCommand.Numbers.Count, 3);
            CollectionAssert.AreEqual(listCommand.Numbers.ToArray(), new[] { 4, -4, 4 });

            Assert.IsTrue(listCommand.FlagOption);
        }

        [TestMethod]
        public void ParseListArgumentWithCustomConverter()
        {
            var result = Parsers.Setup.Attributes<CustomListCommand>((c) => c.UseAsDefault<CustomListCommand>()).Parse("-n 10 20".Split());
            var listCommand = result.GetVerbObject<CustomListCommand>();

            Assert.AreEqual(listCommand.Numbers.Count, 2);
            CollectionAssert.AreEqual(listCommand.Numbers.ToArray(), new[] { new Wrapper() { number = 10 }, new Wrapper() { number = 20 } });
        }

        [Verb("test")]
        class Command
        {
            [NamedOption("value", Description = "Any random value")]
            public string Value { get; set; }

            [NamedOption(nameof(Number), Description = "A floating point value.", Alias = "n")]
            public double Number { get; set; }

            public int Execute()
            {
                return 40;
            }
        }

        [Verb("setPosition")]
        class SetPositionCommand
        {
            [PositionalOption(0, Description = "X coordinate")]
            public float X { get; set; }

            [PositionalOption(1, Description = "Y coordinate")]
            public float Y { get; set; }
        }

        [Verb("test")]
        class PositionalAndNamedCommand
        {
            [NamedOption("isFlagged", Alias = "f", Description = "some boolean flag", Required = true)]
            public bool IsFlagged { get; set; }

            [PositionalOption(0, Description = "some other flag")]
            public bool AnotherFlag { get; set; }
        }

        [Verb("flag")]
        class FlagCommand
        {
            [FlagOption("Verbose", ShortHand = "v", Description = "Should the output be verbose")]
            public bool Verbose { get; set; }
        }

        [Verb("verbosity")]
        class VerbosityCommand
        {
            [FlagOption("Verbosity", ShortHand = "v")]
            [TypeConverter(typeof(VerbosityLevelConverter))]
            public VerbosityLevel Verbosity { get; set; }
        }

        class ListCommand
        {
            [NamedCollectionOption("numbers", Alias = "n")]
            public ICollection<int> Numbers { get; } = new List<int>();

            [FlagOption("flag", ShortHand = "f")]
            public bool FlagOption { get; set; }
        }

        class CustomListCommand
        {
            [NamedCollectionOption("numbers", Alias = "n")]
            [CollectionTypeConverter(typeof(WrapperConverter))]
            public ICollection<Wrapper> Numbers { get; } = new List<Wrapper>();
        }

        enum VerbosityLevel
        {
            Normal,
            Extensive,
            Debug,
        }

        class VerbosityLevelConverter : TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                if (sourceType == typeof(string))
                    return true;

                return base.CanConvertFrom(context, sourceType);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                if (value is string text)
                {
                    switch (text)
                    {
                        case "v":
                            return VerbosityLevel.Normal;
                        case "vv":
                            return VerbosityLevel.Extensive;
                        case "vvv":
                            return VerbosityLevel.Debug;
                        default:
                            throw new ArgumentException($"Can't parse \"{text}\" to a verbosity level");
                    }
                }
                else
                    return base.ConvertFrom(context, culture, value);
            }
        }

        class HelpPresenter : IHelpPresenter
        {
            public StringBuilder StringBuilder { get; }

            public HelpPresenter(StringBuilder stringBuilder)
            {
                StringBuilder = stringBuilder;
            }

            public void Present(string verb)
            {
                StringBuilder.Append($"Showed help for \"{verb}\"");
            }

            public void Present()
            {
                StringBuilder.Append("Showed general help.");
            }
        }
    }
}