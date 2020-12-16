using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using System.Collections.Generic;
using System.Linq;

namespace IntegrationTest
{
    [Excubo.Generators.Blazor.GenerateSetParametersAsync]
    public partial class Component3 : ComponentBase
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            foreach (var element in Enumerable.Range(0, 1))
            {
                builder.OpenElement(0, "div");
                builder.AddMultipleAttributes(1, Additional);
                builder.AddEventPreventDefaultAttribute(2, "onclick", true);
                builder.CloseElement();
            }
        }
        private void Foo()
        {
            BlazorImplementation__WriteSingleParameter(null, null);
        }
        [Parameter] public string Parameter1 { get; set; }
        [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object> Additional { get; set; }
    }
}