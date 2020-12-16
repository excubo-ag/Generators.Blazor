using Tests_Blazor.Helpers;
using Xunit;

namespace Tests_Blazor
{
    public partial class KeyAnalyzerTests
    {
        [Fact]
        public void RenderTreeWithForeach_Component_Nested_Key()
        {
            var userSource = @"
namespace Foo
{
public class Bar
{
    public void BuildRenderTree(global::Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
    {
        builder.OpenComponent(0, typeof(Foo));
        builder.AddAttribute(1, ""ChildContent"", (builder2) =>
        {
            foreach (var element in items)
            {
                builder2.OpenComponent<TComponent>(2);
                builder2.SetKey(element);
                builder2.CloseComponent();
            }
        }
        builder.CloseComponent();
    }
}
}
";
            RunGenerator(userSource, out var generatorDiagnostics, out _);
            generatorDiagnostics.Verify();
        }
    }
}