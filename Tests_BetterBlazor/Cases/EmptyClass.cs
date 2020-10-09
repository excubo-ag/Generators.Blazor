using Tests_BetterBlazor.Helpers;
using Xunit;

namespace Tests_BetterBlazor
{
    public partial class GeneratorTests
    {
        [Fact]
        public void EmptyClass()
        {
            var userSource = @"
using Excubo.Generators.BetterBlazor;
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
            generated.ContainsFileWithContent("GenerateSetParametersAsyncAttribute.cs", @"
using System;
namespace Excubo.Generators.BetterBlazor
    {
        [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
        sealed class GenerateSetParametersAsyncAttribute : Attribute
        {
        }
        [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
        sealed class DoNotGenerateSetParametersAsyncAttribute : Attribute
        {
        }
    }
");
            generated.ContainsFileWithContent("EmptyClass.NothingToSee_override.cs", @"
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace EmptyClass
{
    public partial class NothingToSee
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
            generated.ContainsFileWithContent("EmptyClass.NothingToSee_implementation.cs", @"
using System;

namespace EmptyClass
{
    public partial class NothingToSee
    {
        private void BetterBlazorImplementation__WriteSingleParameter(string name, object value)
        {
            switch (name.ToLowerInvariant())
            {
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
