﻿using Tests_Blazor.Helpers;
using Xunit;

namespace Tests_Blazor
{
    public partial class KeyAnalyzerTests
    {
        [Fact]
        public void RenderTreeWithForeach_Element_Key()
        {
            var userSource = @"
namespace Foo
{
public class Bar
{
    public void BuildRenderTree(global::Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
    {
        foreach (var element in items)
        {
            builder.OpenElement(0, ""div"");
            builder.SetKey(element);
            builder.CloseElement();
        }
    }
}
}
";
            RunGenerator(userSource, out var generatorDiagnostics, out _);
            generatorDiagnostics.Verify();
        }
    }
}