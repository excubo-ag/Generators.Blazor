using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace Excubo.Generators.Blazor
{
    internal static class SourceGeneratorContextExtension
    {
        public static void AddCode(this SourceProductionContext context, string hint_name, string code)
        {
            context.AddSource(hint_name.Replace("<", "_").Replace(">", "_"), SourceText.From(code, Encoding.UTF8));
        }
    }
}