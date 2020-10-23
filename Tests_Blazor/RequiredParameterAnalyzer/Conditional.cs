using Tests_Blazor.Helpers;
using Xunit;


namespace Tests_Blazor
{
    public partial class RequiredParameterAnalyzerTests
    {
        [Fact]
        public void Conditional()
        {
            var userSource = @"
namespace TestProject_Components.Pages
{
    public partial class Conditional : Microsoft.AspNetCore.Components.ComponentBase
    {
        [Required][Parameter] public object Foo { get; set; }
        protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
        {
            if (true)
            {
                builder.OpenComponent<Conditional>(0);
                builder.CloseComponent();
            }
        }
    }
}
";
            RunGenerator(userSource, out var generatorDiagnostics, out _);
            generatorDiagnostics.Verify(new DiagnosticResult("BB0004", "builder.OpenComponent<Conditional>(0)", Microsoft.CodeAnalysis.DiagnosticSeverity.Warning).WithLocation(11, 17));
        }
    }
}
