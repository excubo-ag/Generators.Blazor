using System.Linq;
using Tests_BetterBlazor.Helpers;
using Xunit;

namespace Tests_BetterBlazor
{
    public partial class GeneratorTests
    {
        [Fact]
        public void PositiveReadonly()
        {
            var userSource = @"
using Excubo.Generators.BetterBlazor;
using System;

namespace Testing.PositiveReadonly
{
    [GenerateSetParametersAsyncAttribute]
    public partial class Component
    {
         [Parameter] public string Parameter1 { get; }
         [Parameter] public System.Object Parameter2 { get; set; }
    }
}
";
            RunGenerator(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            Assert.Equal(3, generated.Length);
            Assert.True(generated.Any(g => g.Filename.EndsWith("GenerateSetParametersAsyncAttribute.cs")));
            generated.ContainsFileWithContent("Testing.PositiveReadonly.Component_override.cs", @"
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Testing.PositiveReadonly
{
    public partial class Component
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
            generated.ContainsFileWithContent("Testing.PositiveReadonly.Component_implementation.cs", @"
using System;

namespace Testing.PositiveReadonly
{
    public partial class Component
    {
        private void BetterBlazorImplementation__WriteSingleParameter(string name, object value)
        {
            switch (name.ToLowerInvariant())
            {
                case ""parameter2"":
                    this.Parameter2 = (object)value;
                    break;
                default:
                    throw new ArgumentException($""Unknown parameter: {name}"");
        }
    }
    }
}
");
        }
    }
}
