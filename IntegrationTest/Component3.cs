using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Collections.Generic;
using System.Linq;

namespace IntegrationTest
{
    [Excubo.Generators.Blazor.GenerateSetParametersAsync]
    public partial class Component3 : ComponentBase
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            foreach (var element in Enumerable.Empty<string>())
            {
                builder.OpenElement(0, "div");
                builder.CloseElement();
            }
        }
        private void Foo()
        {
            BlazorImplementation__WriteSingleParameter(null, null);
        }
        [Parameter] public string Parameter1 { get; set; }
        [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object> Additional { get; set; }
    }
}
