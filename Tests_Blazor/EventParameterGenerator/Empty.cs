using Tests_Blazor.Helpers;
using Xunit;

namespace Tests_Blazor
{
    public partial class EventParameterGeneratorTests
    {
        [Fact]
        public void Empty()
        {
            var userSource = "";
            RunGenerator(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            Assert.Single(generated);
            generated.ContainsFileWithContent("GenerateEventsAttribute.cs", @"
using System;
namespace Excubo.Generators.Blazor
{
    public enum HtmlEvent
    {
        Click = 1
    }
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    sealed class GenerateEventsAttribute : Attribute
    {
        public GenerateEventsAttribute(HtmlEvent events)
        {
        }
    }
}");
        }
        [Fact]
        public void OneEvent()
        {
            var userSource = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Excubo.Generators.Blazor;

namespace N.S
{
    [GenerateEvents(HtmlEvent.Click)]
    public partial class Component : Microsoft.AspNetCore.Components.ComponentBase
    {
        protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder __builder)
        {
            __builder.OpenElement(0, ""div"");
            __builder.AddAttribute(1, ""onclick"", Microsoft.AspNetCore.Components.EventCallback.Factory.Create<Microsoft.AspNetCore.Components.Web.MouseEventArgs>(this, OnClick));
            __builder.CloseElement();
        }
}";
            RunGenerator(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            Assert.Equal(2, generated.Length);
            generated.ContainsFileWithContent("_parameters.cs", @"
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace N.S
{
    public partial class Component
    {
        [Parameter] public EventCallback<Microsoft.AspNetCore.Components.Web.MouseEventArgs> OnClick { get; set; }
    }
}");
        }
    }
}
