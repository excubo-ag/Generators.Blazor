using Tests_Blazor.Helpers;
using Xunit;

namespace Tests_Blazor
{
    public partial class RequiredParameterAnalyzerTests
    {
        [Fact]
        public void CatchAllNotUsed()
        {
            var userSource = @"
using System;
using System.Collections.Generic;
namespace Foo
{
    //
    // Summary:
    //     Denotes the target member as a component parameter.
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class ParameterAttribute : Attribute
    {
        public ParameterAttribute();

        //
        // Summary:
        //     Gets or sets a value that determines whether the parameter will capture values
        //     that don't match any other parameter.
        //
        // Remarks:
        //     Microsoft.AspNetCore.Components.ParameterAttribute.CaptureUnmatchedValues allows
        //     a component to accept arbitrary additional attributes, and pass them to another
        //     component, or some element of the underlying markup.
        //     Microsoft.AspNetCore.Components.ParameterAttribute.CaptureUnmatchedValues can
        //     be used on at most one parameter per component.
        //     Microsoft.AspNetCore.Components.ParameterAttribute.CaptureUnmatchedValues should
        //     only be applied to parameters of a type that can be used with Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder.AddMultipleAttributes(System.Int32,System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{System.String,System.Object}})
        //     such as System.Collections.Generic.Dictionary`2.
        public bool CaptureUnmatchedValues { get; set; }
    }
    [ParametersAreRequiredByDefault]
    public class Bar
    {
        public void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenComponent<Bar>(0);
            builder.CloseComponent();
        }
        [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object> CatchEmAll {get;set;}
    }
}";
            RunGenerator(userSource, out var generatorDiagnostics, out _);
            generatorDiagnostics.Verify();
        }
    }
}