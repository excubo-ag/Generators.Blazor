using Tests_Blazor.Helpers;
using Xunit;

namespace Tests_Blazor
{
    public partial class KeyAnalyzerTests
    {
        [Fact]
        public void RenderTreeWithForeach_Component_NoKey()
        {
            var userSource = @"
namespace Foo
{
    public class Bar
    {
        public void BuildRenderTree(RenderTreBuilder builder)
        {
            foreach (var element in items)
            {
                builder.OpenComponent<TComponent>(0);
                builder.CloseComponent();
            }
        }
    }
}
";
            RunGenerator(userSource, out var generatorDiagnostics, out _);
            generatorDiagnostics.Verify(new DiagnosticResult("BB0003", "foreach", Microsoft.CodeAnalysis.DiagnosticSeverity.Warning).WithLocation(8, 13));
        }
    }
}
