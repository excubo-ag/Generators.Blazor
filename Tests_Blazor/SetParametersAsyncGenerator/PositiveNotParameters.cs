using System.Linq;
using Tests_Blazor.Helpers;
using Xunit;

namespace Tests_Blazor
{
    public partial class SetParametersAsyncGeneratorTests
    {
        [Fact]
        public void PositiveNotParameters()
        {
            var userSource = @"
using Excubo.Generators.Blazor;
using System;

namespace Testing.PositiveNotParameters
{
    [GenerateSetParametersAsyncAttribute]
    public partial class Component
    {
         public string Parameter1 { get; }
         public System.Object Parameter2 { get; set; }
    }
}
";
            RunGenerator(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            Assert.Equal(2, generated.Length);
            
            generated.ContainsFileWithContent("Testing.PositiveNotParameters.Component_override.cs", @"
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

#pragma warning disable CS0162
#pragma warning disable CS8632
namespace Testing.PositiveNotParameters
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
            generated.ContainsFileWithContent("Testing.PositiveNotParameters.Component_implementation.cs", @"
using System;

#pragma warning disable CS0162
#pragma warning disable CS8632
namespace Testing.PositiveNotParameters
{
    public partial class Component
    {
        private void BlazorImplementation__WriteSingleParameter(string name, object value)
        {
            switch (name)
            {
                default:
                {
                    switch (name.ToLowerInvariant())
                    {
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
