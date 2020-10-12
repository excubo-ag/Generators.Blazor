using Excubo.Generators.Blazor;
using FluentAssertions;
using FluentAssertions.Primitives;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;
using System.Linq;
using Xunit;

namespace Tests_Blazor.Helpers
{
    public static class AssertionExtensions
    {
        public static AndConstraint<StringAssertions> BeIgnoringLineEndings(this StringAssertions stringAssertions, string expected)
        {
            return stringAssertions.Subject.Should().Be(expected.NormalizeWhitespace());
        }
        public static void ContainsFileWithContent(this ImmutableArray<(string Filename, string Content)> collection, string filename, string content)
        {
            var match = collection.FirstOrDefault(e => e.Filename.EndsWith(filename));
            if (match == default)
            {
                Assert.False(true, $"file {filename} not found. Available files: {string.Join(", ", collection.Select(e => e.Filename))}");
            }
            match.Content.Should().BeIgnoringLineEndings(content);
        }
    }
}