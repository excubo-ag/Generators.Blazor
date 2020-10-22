using Tests_Blazor.Helpers;
using Xunit;

namespace Tests_Blazor
{
    public partial class KeyAnalyzerTests
    {
        [Fact]
        public void RenderTreeWithForeach_Component_Key()
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
            builder.OpenComponent<TComponent>(0);
            builder.SetKey(1, ""foo"");
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
