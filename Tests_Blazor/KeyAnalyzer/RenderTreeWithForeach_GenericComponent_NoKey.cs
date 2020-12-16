using Tests_Blazor.Helpers;
using Xunit;

namespace Tests_Blazor
{
    public partial class KeyAnalyzerTests
    {
        [Fact]
        public void RenderTreeWithForeach_GenericComponent_NoKey()
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
                Foo.Bar.TypeInference.CreateListItem_0<object>(builder, 0);
            }
        }
        internal static class TypeInference
        {
            public static void CreateListItem_0<T>(global::Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder __builder, int seq)
            {
                __builder.OpenComponent<global::Excubo.Blazor.TreeViews.__Internal.ListItem<T>>(seq);
                __builder.CloseComponent();
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