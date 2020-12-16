using Tests_Blazor.Helpers;
using Xunit;


namespace Tests_Blazor
{
    public partial class RequiredParameterAnalyzerTests
    {
        [Fact]
        public void ConditionalWithSet()
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
                builder.AddAttribute(1, ""Foo"", null);
                builder.CloseComponent();
            }
        }
    }
}
";
            RunGenerator(userSource, out var generatorDiagnostics, out _);
            generatorDiagnostics.Verify();
        }
    }
}