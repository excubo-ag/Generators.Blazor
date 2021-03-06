﻿using System.Linq;
using Tests_Blazor.Helpers;
using Xunit;

namespace Tests_Blazor
{
    public partial class SetParametersAsyncGeneratorTests
    {
        [Fact]
        public void Inheritance()
        {
            var userSource = @"
using Excubo.Generators.Blazor;
using System;

namespace Testing.Positive.Inheritance
{
    [GenerateSetParametersAsyncAttribute]
    public partial class Component : MyBase
    {
    }
}
    public partial class MyBase
    {
         [Parameter] public string Parameter1 { get; set; }
         [Parameter] public System.Object Parameter2 { get; set; }
         [CascadingParameter] public GenerateSetParametersAsyncAttribute Parameter3 { get; set; }
         [CascadingParameter] private object Foo { get; set; }
    }
}
";
            RunGenerator(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            Assert.Equal(2, generated.Length);

            generated.ContainsFileWithContent("Testing.Positive.Inheritance.Component_override.cs", @"
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

#pragma warning disable CS0162
#pragma warning disable CS8632
namespace Testing.Positive.Inheritance
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
            generated.ContainsFileWithContent("Testing.Positive.Inheritance.Component_implementation.cs", @"
using System;

#pragma warning disable CS0162
#pragma warning disable CS8632
namespace Testing.Positive.Inheritance
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
                case ""Parameter3"":
                    this.Parameter3 = (Excubo.Generators.Blazor.GenerateSetParametersAsyncAttribute)value;
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
                        case ""parameter3"":
                            this.Parameter3 = (Excubo.Generators.Blazor.GenerateSetParametersAsyncAttribute)value;
                            break;
                        default:
                            throw new ArgumentException($""Unknown parameter: {name}"");
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