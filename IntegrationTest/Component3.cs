using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Linq;

namespace IntegrationTest
{
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
        [Parameter] public string Parameter1 { get; set; }
    }
}
