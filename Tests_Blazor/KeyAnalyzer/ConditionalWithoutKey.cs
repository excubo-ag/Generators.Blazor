using Tests_Blazor.Helpers;
using Xunit;


namespace Tests_Blazor
{
    public partial class KeyAnalyzerTests
    {
        [Fact]
        public void ConditionalWithoutKey()
        {
            var userSource = @"
namespace TestProject_Components.Pages
{
    public partial class AutoLayout : Microsoft.AspNetCore.Components.ComponentBase
    {
        protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder __builder)
        {
            __builder.AddAttribute(46, ""ChildContent"", (Microsoft.AspNetCore.Components.RenderFragment)((__builder2) =>
            {
                __builder2.AddAttribute(48, ""ChildContent"", (Microsoft.AspNetCore.Components.RenderFragment)((__builder3) =>
                {
                    bool odd = false;
                    foreach (var node in nodes)
                    {
                        if (odd)
                        {
                            __builder3.OpenComponent<Excubo.Blazor.Diagrams.RectangleNode>(49);
                            __builder3.CloseComponent();
                        }
                        else
                        {
                            __builder3.OpenComponent<Excubo.Blazor.Diagrams.EllipseNode>(59);
                            __builder3.SetKey(
                                                node.Id
                            );
                            __builder3.CloseComponent();
                        }
                        odd = !odd;
                    }
                }
                ));
            }));
        }
    }
}
";
            RunGenerator(userSource, out var generatorDiagnostics, out _);
            generatorDiagnostics.Verify(new DiagnosticResult("BB0003", "foreach", Microsoft.CodeAnalysis.DiagnosticSeverity.Warning).WithLocation(13, 21));
        }
    }
}