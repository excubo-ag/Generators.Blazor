using Tests_Blazor.Helpers;
using Xunit;

namespace Tests_Blazor
{
    public partial class EventParameterGeneratorTests
    {
        [Fact]
        public void Empty()
        {
            var userSource = @"";
            RunGenerator(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            Assert.Empty(generated);
        }
    }
}