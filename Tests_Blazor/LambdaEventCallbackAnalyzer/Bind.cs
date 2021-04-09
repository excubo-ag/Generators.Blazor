using Tests_Blazor.Helpers;
using Xunit;

namespace Tests_Blazor
{
    public partial class LambdaEventCallbackAnalyzerTests
    {
        [Fact]
        public void Bind()
        {
            var userSource = @"

namespace IntegrationTest
{
    
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Web;

    public partial class Component2 : Microsoft.AspNetCore.Components.ComponentBase
    {
        
        protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder __builder)
        {
             foreach (var element in Enumerable.Range(0, 1))
            {
                __builder.OpenElement(8, ""input"");
                __builder.AddAttribute(9, ""type"", ""text"");
                __builder.AddAttribute(10, ""value"", Microsoft.AspNetCore.Components.BindConverter.FormatValue(text));
                __builder.AddAttribute(11, ""onchange"", Microsoft.AspNetCore.Components.EventCallback.Factory.CreateBinder(this, __value => text = __value, text));
                __builder.SetUpdatesAttributeName(""value"");
                __builder.CloseElement();
            }
        }
        private string text;
        private void Callback(object value)
        {
        }
        [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object> Additional { get; set; }
    }
}

";
            RunGenerator(userSource, out var generatorDiagnostics, out _);
            generatorDiagnostics.Verify(new DiagnosticResult("BB0009", "__value => text = __value", Microsoft.CodeAnalysis.DiagnosticSeverity.Warning).WithLocation(23, 129));
        }
    }
}