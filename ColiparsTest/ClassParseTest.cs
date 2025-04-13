using Colipars;
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
    public class ClassParseTest
    {
        [TestMethod]
        public void ParseNamed()
        {
            var exitCode = Parsers.Setup.ClassAttributes<Command>().Parse("test --value test -n 0.42".Split()).Map((Command command) => command.Execute(), (errors) => 1);

            Assert.AreEqual(40, exitCode);
        }

        [TestMethod]
        public void ShowGeneralHelp()
        {
            var text = new StringBuilder();

            Parsers.Setup.ClassAttributes<Command>((c) => ((ServiceProvider)c.Services).Register<IHelpPresenter>(new HelpPresenter(text))).ShowHelp();

            Assert.AreEqual("Showed general help.", text.ToString());
        }

        [TestMethod]
        public void ShowSpecificHelp()
        {
            var text = new StringBuilder();

            Parsers.Setup.ClassAttributes<Command>((c) => ((ServiceProvider)c.Services).Register<IHelpPresenter>(new HelpPresenter(text))).ShowHelp(Attribute.Class.AttributeConfiguration.GetVerbFromType(typeof(Command)));

            Assert.AreEqual(@"Showed help for ""test""", text.ToString());
        }

        [TestMethod]
        public void ParsePositional()
        {
            var command = Parsers.Setup.ClassAttributes<SetPositionCommand>().Parse("setPosition 0.4 -220.4".Split()).GetCustomObject();

            Assert.AreEqual(0.4f, command.X);
            Assert.AreEqual(-220.4f, command.Y);
        }

        [TestMethod]
        public void ParseMixed()
        {
            var command = Parsers.Setup.ClassAttributes<PositionalAndNamedCommand>((c) => c.UseAsDefault<PositionalAndNamedCommand>()).Parse("-f true false".Split()).GetCustomObject();

            Assert.AreEqual(true, command.IsFlagged);
            Assert.AreEqual(false, command.AnotherFlag);
        }

        [TestMethod]
        public void ParseFlag()
        {
            var command = Parsers.Setup.ClassAttributes<FlagCommand>((c) => c.UseAsDefault<FlagCommand>()).Parse("-v".Split()).GetCustomObject();

            Assert.AreEqual(true, command.Verbose);
        }

        [TestMethod]
        public void ParseCustomFlag()
        {
            var command = Parsers.Setup.ClassAttributes<VerbosityCommand>((c) => c.UseAsDefault<VerbosityCommand>()).Parse("-vv".Split()).GetCustomObject();

            Assert.AreEqual(VerbosityLevel.Extensive, command.Verbosity);
        }

        [TestMethod]
        public void MultipleCommands()
        {
            var verbObj = Parsers.Setup.ClassAttributes<FlagCommand, VerbosityCommand>().Parse("verbosity -vvv".Split()).GetCustomObject();

            Assert.IsInstanceOfType(verbObj, typeof(VerbosityCommand));
            Assert.AreEqual(VerbosityLevel.Debug, ((VerbosityCommand)verbObj).Verbosity);
        }

        [TestMethod]
        public void ParseNegativeNumber()
        {
            var command = Parsers.Setup.ClassAttributes<Command>().Parse("test -n -4.0".Split()).GetCustomObject();

            Assert.AreEqual(command.Number, -4.0f);
        }

        [TestMethod]
        public void ParseListArgument()
        {
            var result = Parsers.Setup.ClassAttributes<ListCommand>((c) => c.UseAsDefault<ListCommand>()).Parse("-n 10 -20 4".Split());
            var listCommand = result.GetCustomObject();

            Assert.AreEqual(3, listCommand.Numbers.Count);
            CollectionAssert.AreEqual(new[] { 10, -20, 4 }, listCommand.Numbers.ToArray());
        }

        [TestMethod]
        public void ParseListArgumentWithFlagParameterAfterwards()
        {
            var result = Parsers.Setup.ClassAttributes<ListCommand>((c) => c.UseAsDefault<ListCommand>()).Parse("-n 4 -4 4 -f".Split());
            var listCommand = result.GetCustomObject();

            Assert.AreEqual(3, listCommand.Numbers.Count);
            CollectionAssert.AreEqual(new[] { 4, -4, 4 }, listCommand.Numbers.ToArray());

            Assert.IsTrue(listCommand.FlagOption);
        }

        [TestMethod]
        public void ParseListArgumentWithCustomConverter()
        {
            var result = Parsers.Setup.ClassAttributes<CustomListCommand>((c) => c.UseAsDefault<CustomListCommand>()).Parse("-n 10 20".Split());
            var listCommand = result.GetCustomObject();

            Assert.AreEqual(2, listCommand.Numbers.Count);
            CollectionAssert.AreEqual(new[] { new Wrapper() { number = 10 }, new Wrapper() { number = 20 } }, listCommand.Numbers.ToArray());
        }

        [Verb("test", Description = "Example description")]
        class Command
        {
            [NamedOption("value", Description = "Any random value")]
            public string Value { get; set; } = "";

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
            public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
            {
                if (sourceType == typeof(string))
                    return true;

                return base.CanConvertFrom(context, sourceType);
            }

            public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
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

            public void Present(IVerb verb)
            {
                StringBuilder.Append($"Showed help for \"{verb.Name}\"");
            }

            public void Present()
            {
                StringBuilder.Append("Showed general help.");
            }
        }
    }
}
