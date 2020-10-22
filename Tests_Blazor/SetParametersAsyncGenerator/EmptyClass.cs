using Tests_Blazor.Helpers;
using Xunit;

namespace Tests_Blazor
{
    public partial class SetParametersAsyncGeneratorTests
    {
        [Fact]
        public void EmptyClass()
        {
            var userSource = @"
using Excubo.Generators.Blazor;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;
namespace EmptyClass
{
    [GenerateSetParametersAsyncAttribute]
    public partial class NothingToSee : LayoutComponentBase
    {
    }
}";
            RunGenerator(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            Assert.Equal(3, generated.Length);
            generated.ContainsFileWithContent("EmptyClass.NothingToSee_override.cs", @"
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

#pragma warning disable CS0162
#pragma warning disable CS8632
namespace EmptyClass
{
    public partial class NothingToSee
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
            generated.ContainsFileWithContent("EmptyClass.NothingToSee_implementation.cs", @"
using System;

#pragma warning disable CS0162
#pragma warning disable CS8632
namespace EmptyClass
{
    public partial class NothingToSee
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
