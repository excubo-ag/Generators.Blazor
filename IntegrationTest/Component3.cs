using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace IntegrationTest
{
    [Excubo.Generators.Blazor.GenerateSetParametersAsync]
    public partial class Component3 : ComponentBase
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "div");
            builder.CloseElement();
        }
        [Parameter] public string Parameter1 { get; set; }
        private void Foo()
        {
            BlazorImplementation__WriteSingleParameter("foo", new object());
        }
    }
}
