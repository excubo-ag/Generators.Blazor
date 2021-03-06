﻿using Tests_Blazor.Helpers;
using Xunit;

namespace Tests_Blazor
{
    public partial class KeyAnalyzerTests
    {
        [Fact]
        public void RenderTreeWithForeach_GenericComponent_Key()
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
            Foo.Bar.TypeInference.CreateListItem_0(builder, 0);
        }
    }
    internal static class TypeInference
    {
        public static void CreateListItem_0<T>(global::Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder __builder, int seq, T element)
        {
            __builder.OpenComponent<global::Excubo.Blazor.TreeViews.__Internal.ListItem<T>>(seq);
            __builder.SetKey(element);
            __builder.CloseComponent();
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