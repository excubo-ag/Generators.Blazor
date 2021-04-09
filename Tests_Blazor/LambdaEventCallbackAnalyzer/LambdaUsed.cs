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
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Rendering;
using System.Linq;
using System.Collections.Generic;

namespace Foo
{
    public class Bar
    {
        public void BuildRenderTree(RenderTreeBuilder builder)
        {
            foreach (var element in Enumerable.Empty<object>())
            {
                builder.OpenComponent<Bar>(0);
                builder.AddAttribute(1, ""onclick"", Microsoft.AspNetCore.Components.EventCallback.Factory.Create<Microsoft.AspNetCore.Components.Web.MouseEventArgs>(this, (_) => SetItem(element)));
                builder.CloseComponent();
            }
        }
        [Parameter] public object Value { get; set; }
        private void SetItem(object)
        {
        }
    }
}";
            RunGenerator(userSource, out var generatorDiagnostics, out _);
            generatorDiagnostics.Verify(new DiagnosticResult("BB0009", "(_) => SetItem(element)", Microsoft.CodeAnalysis.DiagnosticSeverity.Warning).WithLocation(17, 171));
        }
    }
}