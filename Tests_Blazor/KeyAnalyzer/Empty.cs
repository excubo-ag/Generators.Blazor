using Tests_Blazor.Helpers;
using Xunit;

namespace Tests_Blazor
{
    public partial class KeyAnalyzerTests
    {
        [Fact]
        public void Empty()
        {
            var userSource = @"";
            RunGenerator(userSource, out var generatorDiagnostics, out _);
            generatorDiagnostics.Verify();
        }
    }
}
