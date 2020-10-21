using Tests_Blazor.Helpers;
using Xunit;

namespace Tests_Blazor
{
    public partial class KeyAnalyzerTests
    {
        [Fact]
        public void RenderTreeWithoutLoop()
        {
            var userSource = @"
namespace Foo
{
    public class Bar
    {
        public void BuildRenderTree(RenderTreBuilder builder)
        {
    
        }
    }
}
";
            RunGenerator(userSource, out var generatorDiagnostics, out _);
            generatorDiagnostics.Verify();
        }
    }
}
