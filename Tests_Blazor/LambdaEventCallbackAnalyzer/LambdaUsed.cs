using Tests_Blazor.Helpers;
using Xunit;

namespace Tests_Blazor
{
    public partial class LambdaEventCallbackAnalyzerTests
    {
        [Fact]
        public void LambdaUsed()
        {
            var userSource = @"
namespace Foo
{
    public class Bar
    {
        public void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenComponent<Bar>(0);
builder.AddAttribute(1, ""onclick"", Microsoft.AspNetCore.Components.EventCallback.Factory.Create<Microsoft.AspNetCore.Components.Web.MouseEventArgs>(this, (_) => {}));
            builder.CloseComponent();
        }
        [Parameter] public object Value { get; set; }
    }
}";
            RunGenerator(userSource, out var generatorDiagnostics, out _);
            generatorDiagnostics.Verify(new DiagnosticResult("BB0009", "(_) => {}", Microsoft.CodeAnalysis.DiagnosticSeverity.Warning).WithLocation(9, 155));
        }
    }
}