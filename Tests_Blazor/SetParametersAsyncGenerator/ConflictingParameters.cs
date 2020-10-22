using System.Linq;
using Tests_Blazor.Helpers;
using Xunit;

namespace Tests_Blazor
{
    public partial class SetParametersAsyncGeneratorTests
    {
        [Fact]
        public void ConflictingParameters()
        {
            var userSource = @"
using Excubo.Generators.Blazor;
using System;

namespace Testing.ConflictingParameters
{
    [GenerateSetParametersAsyncAttribute]
    public partial class Component
    {
         [Parameter] public string Parameter { get; set; }
         [Parameter] public System.Object parameter { get; set; }
    }
}
";
            RunGenerator(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                new DiagnosticResult("BB0001", "Parameter", Microsoft.CodeAnalysis.DiagnosticSeverity.Error).WithLocation(10, 36),
                new DiagnosticResult("BB0001", "parameter", Microsoft.CodeAnalysis.DiagnosticSeverity.Error).WithLocation(11, 43));
            Assert.Equal(3, generated.Length);
            Assert.True(generated.Any(g => g.Filename.EndsWith("GenerateSetParametersAsyncAttribute.cs")));
            generated.ContainsFileWithContent("Testing.ConflictingParameters.Component_override.cs", @"
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

#pragma warning disable CS0162
#pragma warning disable CS8632
namespace Testing.ConflictingParameters
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
            generated.ContainsFileWithContent("Testing.ConflictingParameters.Component_implementation.cs", @"
using System;

#pragma warning disable CS0162
#pragma warning disable CS8632
namespace Testing.ConflictingParameters
{
    public partial class Component
    {
        private void BlazorImplementation__WriteSingleParameter(string name, object value)
        {
            switch (name)
            {
                case ""Parameter"":
                    this.Parameter = (string)value;
                    break;
                case ""parameter"":
                    this.parameter = (object)value;
                    break;
                default:
                {
                    switch (name.ToLowerInvariant())
                    {
                        case ""parameter"":
                            this.Parameter = (string)value;
                            break;
                        case ""parameter"":
                            this.parameter = (object)value;
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
