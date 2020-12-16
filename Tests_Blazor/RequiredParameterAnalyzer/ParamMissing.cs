using Tests_Blazor.Helpers;
using Xunit;

namespace Tests_Blazor
{
    public partial class RequiredParameterAnalyzerTests
    {
        [Fact]
        public void ParamMissing()
        {
            var userSource = @"
namespace Foo
{
    public class Bar
    {
        public void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenComponent<Bar>(0);
            builder.CloseComponent();
        }
        [Required][Parameter] public object Value { get; set; }
    }
}";
            RunGenerator(userSource, out var generatorDiagnostics, out _);
            generatorDiagnostics.Verify(new DiagnosticResult("BB0004", "builder.OpenComponent<Bar>(0)", Microsoft.CodeAnalysis.DiagnosticSeverity.Warning).WithLocation(8, 13));
        }
    }
}