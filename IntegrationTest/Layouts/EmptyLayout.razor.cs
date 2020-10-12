using Excubo.Generators.Blazor;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationTest.Layouts
{
    [GenerateSetParametersAsync]
    public partial class EmptyLayout : LayoutComponentBase
    {
        public void Foo()
        {
            BlazorImplementation__WriteSingleParameter("", new object());
        }
#nullable disable
        private void BlazorImplementation2__WriteSingleParameter(string name, object value)
        {
            switch (name)
            {
                case "Body":
                    this.Body = (RenderFragment)value; break;
                default:
                    throw new ArgumentException($"Unknown parameter: {name}");
            }
        }
    }
#nullable restore
}
