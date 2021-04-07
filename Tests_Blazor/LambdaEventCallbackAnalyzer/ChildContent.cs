using Tests_Blazor.Helpers;
using Xunit;

namespace Tests_Blazor
{
    public partial class LambdaEventCallbackAnalyzerTests
    {
        [Fact]
        public void ChildContent()
        {
            var userSource = @"
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
namespace Foo
{
    public class Bar
    {
        public void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenComponent<Bar>(0);
            builder.AddAttribute(1, ""ChildContent"", (RenderFragment)((builder2) => 
            {
                builder2.OpenElement(0, ""div"");
                builder2.CloseElement();
            }));
            builder.CloseComponent();
        }
        [Parameter] public object Value { get; set; }
    }
}";
            RunGenerator(userSource, out var generatorDiagnostics, out _);
            generatorDiagnostics.Verify();
        }
    }
}