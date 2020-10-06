using Excubo.Generators.BetterBlazor;
using System.Linq;
using Tests_BetterBlazor.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace Tests_BetterBlazor
{
    public class GeneratorTests : TestBase<SetParametersAsyncGenerator>
    {
        public GeneratorTests(ITestOutputHelper output_helper) : base(output_helper)
        {
        }

        [Fact]
        public void Empty()
        {
            var userSource = "";
            RunGenerator(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            Assert.Single(generated);
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
        }

        [Fact]
        public void Positive()
        {
            var userSource = @"
using Excubo.Generators.BetterBlazor;
using System;

namespace Testing.Positive
{
    [GenerateSetParametersAsyncAttribute]
    public partial class Component
    {
         [Parameter] public string Parameter1 { get; set; }
         [Parameter] public System.Object Parameter2 { get; set; }
         [Parameter] public GenerateSetParametersAsyncAttribute Parameter3 { get; set; }
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

namespace Testing.Positive
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
            generated.ContainsFileWithContent("Testing.Positive.Component_implementation.cs", @"
using System;

namespace Testing.Positive
{
    public partial class Component
    {
        private void BetterBlazorImplementation__WriteSingleParameter(string name, object value)
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
                    this.Parameter3 = (Excubo.Generators.BetterBlazor.GenerateSetParametersAsyncAttribute)value;
                    break;
                default:
                    throw new ArgumentException($""Unknown parameter: {name}"");
        }
    }
    }
}
");
        }


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
            switch (name)
            {
                case ""Parameter2"":
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
                    throw new ArgumentException($""Unknown parameter: {name}"");
        }
    }
    }
}
");
        }

        [Fact]
        public void PositiveAndNegative()
        {
            var userSource = @"
using Excubo.Generators.BetterBlazor;
using System;

namespace Testing.Positive
{
    [GenerateSetParametersAsyncAttribute]
    [DoNotGenerateSetParametersAsyncAttribute]
    public partial class Component
    {
         [Parameter] public string Parameter1 { get; set; }
    }
}
";
            RunGenerator(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            Assert.Equal(1, generated.Length);
            Assert.True(generated.Any(g => g.Filename.EndsWith("GenerateSetParametersAsyncAttribute.cs")));
        }

        [Fact]
        public void Negative()
        {
            var userSource = @"
using Excubo.Generators.BetterBlazor;
using System;

namespace Testing.Positive
{
    [GenerateSetParametersAsyncAttribute]
    [DoNotGenerateSetParametersAsyncAttribute]
    public partial class Component
    {
         [Parameter] public string Parameter1 { get; set; }
    }
}
";
            RunGenerator(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            Assert.Equal(1, generated.Length);
            Assert.True(generated.Any(g => g.Filename.EndsWith("GenerateSetParametersAsyncAttribute.cs")));
        }
    }
}
