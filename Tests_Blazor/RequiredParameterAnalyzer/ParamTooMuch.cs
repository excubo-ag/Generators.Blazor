using Tests_Blazor.Helpers;
using Xunit;

namespace Tests_Blazor
{
    public partial class RequiredParameterAnalyzerTests
    {
        [Fact]
        public void ParamTooMuch()
        {
            var userSource = @"
namespace Foo
{
    [ParametersAreRequiredByDefault]
    public class Bar
    {
        public void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenComponent<Bar>(0);
            builder.AddAttribute(1, ""Value"", null);
            builder.CloseComponent();
        }
    }
}";
            RunGenerator(userSource, out var generatorDiagnostics, out _);
            generatorDiagnostics.Verify(new DiagnosticResult("BB0008", @"builder.AddAttribute(1, ""Value"", null)", Microsoft.CodeAnalysis.DiagnosticSeverity.Warning).WithLocation(10, 13));
        }
    }
}