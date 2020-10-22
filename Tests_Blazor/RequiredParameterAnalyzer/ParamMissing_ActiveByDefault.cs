using Tests_Blazor.Helpers;
using Xunit;

namespace Tests_Blazor
{
    public partial class RequiredParameterAnalyzerTests
    {
        [Fact]
        public void ParamMissing_ActiveByDefault()
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
            builder.CloseComponent();
        }
        [Parameter] public object Value { get; set; }
    }
}";
            RunGenerator(userSource, out var generatorDiagnostics, out _);
            generatorDiagnostics.Verify(new DiagnosticResult("BB0004", "builder.OpenComponent<Bar>(0)", Microsoft.CodeAnalysis.DiagnosticSeverity.Warning).WithLocation(9, 13));
        }
    }
}
