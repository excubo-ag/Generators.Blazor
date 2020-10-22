using Tests_Blazor.Helpers;
using Xunit;

namespace Tests_Blazor
{
    public partial class KeyAnalyzerTests
    {
        [Fact]
        public void RenderTreeWithForeach_Element_NoKey()
        {
            var userSource = @"
namespace Foo
{
    public class Bar
    {
        public void BuildRenderTree(global::Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
        {
            foreach (var element in items)
            {
                builder.OpenElement(0, ""div"");
                builder.CloseElement();
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
