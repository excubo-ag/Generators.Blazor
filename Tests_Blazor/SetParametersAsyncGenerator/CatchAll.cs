using System.Linq;
using Tests_Blazor.Helpers;
using Xunit;

namespace Tests_Blazor
{
    public partial class SetParametersAsyncGeneratorTests
    {
        [Fact]
        public void CatchAll()
        {
            var userSource = @"
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Excubo.Generators.Blazor;
using System;

namespace Testing.Positive
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
    [GenerateSetParametersAsyncAttribute]
    public partial class Component
    {
         [Parameter] public string Parameter1 { get; set; }
         [Parameter] public System.Object Parameter2 { get; set; }
         [Parameter(CaptureUnmatchedValues = true)] public IEnumerable<KeyValuePair<string, object>> Parameter3 { get; set; }
    }
}
";
            RunGenerator(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            Assert.Equal(3, generated.Length);
            Assert.True(generated.Any(g => g.Filename.EndsWith("GenerateSetParametersAsyncAttribute.cs")));
            generated.ContainsFileWithContent("Testing.Positive.Component_override.cs", @"
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

#pragma warning disable CS0162
#pragma warning disable CS8632
namespace Testing.Positive
{
    public partial class Component
    {
        public override Task SetParametersAsync(ParameterView parameters)
        {
            foreach (var parameter in parameters)
            {
                BlazorImplementation__WriteSingleParameter(parameter.Name, parameter.Value);
            }

            // Run the normal lifecycle methods, but without assigning parameters again
            return base.SetParametersAsync(ParameterView.Empty);
        }
    }
}
#pragma warning restore CS8632
#pragma warning restore CS0162
");
            generated.ContainsFileWithContent("Testing.Positive.Component_implementation.cs", @"
using System;

#pragma warning disable CS0162
#pragma warning disable CS8632
namespace Testing.Positive
{
    public partial class Component
    {
        private void BlazorImplementation__WriteSingleParameter(string name, object value)
        {
            switch (name)
            {
                case ""Parameter1"":
                    this.Parameter1 = (string)value;
                    break;
                case ""Parameter2"":
                    this.Parameter2 = (object)value;
                    break;
                default:
                {
                    switch (name.ToLowerInvariant())
                    {
                        case ""parameter1"":
                            this.Parameter1 = (string)value;
                            break;
                        case ""parameter2"":
                            this.Parameter2 = (object)value;
                            break;
                        default:
                            {
                                this.Parameter3 ??= new System.Collections.Generic.Dictionary<string, object>();
                                if (!this.Parameter3.ContainsKey(name))
                                {
                                    this.Parameter3.Add(name, value);
                                }
                                else
                                {
                                    this.Parameter3[name] = value;
                                }
                                break;
                            }
                    }
                    break;
                }
            }
        }
    }
}
#pragma warning restore CS8632
#pragma warning restore CS0162
");
        }
    }
}
