﻿using System.Linq;
using Tests_BetterBlazor.Helpers;
using Xunit;

namespace Tests_BetterBlazor
{
    public partial class GeneratorTests
    {
        [Fact]
        public void PositiveT()
        {
            var userSource = @"
using Excubo.Generators.BetterBlazor;
using System;

namespace Testing.Positive
{
    [GenerateSetParametersAsyncAttribute]
    public partial class Component<T>
    {
         [Parameter] public string Parameter1 { get; set; }
         [Parameter] public T Parameter2 { get; set; }
         [Parameter] public GenerateSetParametersAsyncAttribute Parameter3 { get; set; }
    }
}
";
            RunGenerator(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            Assert.Equal(3, generated.Length);
            Assert.True(generated.Any(g => g.Filename.EndsWith("GenerateSetParametersAsyncAttribute.cs")));
            generated.ContainsFileWithContent("Testing.Positive.Component_T__override.cs", @"
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Testing.Positive
{
    public partial class Component<T>
    {
        public override Task SetParametersAsync(ParameterView parameters)
        {
            foreach (var parameter in parameters)
            {
                BetterBlazorImplementation__WriteSingleParameter(parameter.Name, parameter.Value);
            }

            // Run the normal lifecycle methods, but without assigning parameters again
            return base.SetParametersAsync(ParameterView.Empty);
        }
    }
}
");
            generated.ContainsFileWithContent("Testing.Positive.Component_T__implementation.cs", @"
using System;

namespace Testing.Positive
{
    public partial class Component<T>
    {
        private void BetterBlazorImplementation__WriteSingleParameter(string name, object value)
        {
            switch (name)
            {
                case ""Parameter1"":
                    this.Parameter1 = (string)value;
                    break;
                case ""Parameter2"":
                    this.Parameter2 = (T)value;
                    break;
                case ""Parameter3"":
                    this.Parameter3 = (Excubo.Generators.BetterBlazor.GenerateSetParametersAsyncAttribute)value;
                    break;
                default:
                {
                    switch (name.ToLowerInvariant())
                    {
                        case ""parameter1"":
                            this.Parameter1 = (string)value;
                            break;
                        case ""parameter2"":
                            this.Parameter2 = (T)value;
                            break;
                        case ""parameter3"":
                            this.Parameter3 = (Excubo.Generators.BetterBlazor.GenerateSetParametersAsyncAttribute)value;
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
");
        }
    }
}
