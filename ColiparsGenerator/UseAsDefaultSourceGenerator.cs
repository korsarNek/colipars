using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Colipars.Generator;

[Generator(LanguageNames.CSharp)]
class UseAsDefaultSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        /*List<(string, string)> parameterVariations = [
            ("", "Action"),
            ("", "Func<int>"),
            ("", "Func<bool>"),
            ("", "Func<Task>"),
            ("", "Func<Task<int>>"),
            ("", "Func<Task<bool>>"),
        ];

        for (int i = 1; i <= 16; i++)
        {
            var t = string.Join(",", Enumerable.Range(1, i).Select(n => $"T{n}"));
            parameterVariations.AddRange([
                ($"<{t}>", $"Action<{t}>"),
                ($"<{t}>", $"Func<{t}, int>"),
                ($"<{t}>", $"Func<{t}, bool>"),
                ($"<{t}>", $"Func<{t}, Task>"),
                ($"<{t}>", $"Func<{t}, Task<int>>"),
                ($"<{t}>", $"Func<{t}, Task<bool>>")
            ]);
        }

        var code = new StringBuilder($@"
            namespace Colipars.Attribute.Method;

            using System;
            using System.Reflection;
            using System.Threading.Tasks;
            using System.CodeDom.Compiler;

            public partial class AttributeConfiguration
            {{
                {string.Join("\n", parameterVariations.Select((entry) => $@"
                [GeneratedCode(""ColiparsGenerator"", ""0.1"")]
                public void UseAsDefault{entry.Item1}({entry.Item2} method)
                {{
                    _defaultMethod = method.GetMethodInfo();
                }}
                "))}
            }}
        ");

        context.RegisterPostInitializationOutput(ctx => ctx.AddSource("UseAsDefault.g.cs", SourceText.From(code.ToString(), Encoding.UTF8)));*/
    }
}