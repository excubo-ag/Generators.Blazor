using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Excubo.Generators.BetterBlazor
{
    internal static class StringExtension
    {
        public static string NormalizeWhitespace(this string code)
        {
            return CSharpSyntaxTree.ParseText(code).GetRoot().NormalizeWhitespace().ToFullString();
        }
    }
}