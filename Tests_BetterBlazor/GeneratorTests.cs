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
        public void OwnTypeDefinitions()
        {
            var userSource = @"using Excubo.Generators.BetterBlazor;
using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Components
{
    //
    // Summary:
    //     Denotes the target member as a component parameter.
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class ParameterAttribute : Attribute
    {

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
    //
    // Summary:
    //     Represents a single parameter supplied to an Microsoft.AspNetCore.Components.IComponent
    //     by its parent in the render tree.
    public readonly struct ParameterValue
    {
        //
        // Summary:
        //     Gets the name of the parameter.
        public string Name { get; }
        //
        // Summary:
        //     Gets the value being supplied for the parameter.
        public object Value { get; }
        //
        // Summary:
        //     Gets a value to indicate whether the parameter is cascading, meaning that it
        //     was supplied by a Microsoft.AspNetCore.Components.CascadingValue`1.
        public bool Cascading { get; }
    }
    //
    // Summary:
    //     Represents a collection of parameters supplied to an Microsoft.AspNetCore.Components.IComponent
    //     by its parent in the render tree.
    public readonly struct ParameterView
    {
        //
        // Summary:
        //     Gets an empty Microsoft.AspNetCore.Components.ParameterView.
        public static ParameterView Empty { get; }
        //
        // Summary:
        //     Returns an enumerator that iterates through the Microsoft.AspNetCore.Components.ParameterView.
        //
        // Returns:
        //     The enumerator.
        public Enumerator GetEnumerator() { return new Enumerator(); }

        //
        // Summary:
        //     An enumerator that iterates through a Microsoft.AspNetCore.Components.ParameterView.
        public struct Enumerator
        {
            //
            // Summary:
            //     Gets the current value of the enumerator.
            public ParameterValue Current { get; }

            //
            // Summary:
            //     Instructs the enumerator to move to the next value in the sequence.
            //
            // Returns:
            //     A flag to indicate whether or not there is a next value.
            public bool MoveNext() { return false; }
        }
    }
    public abstract class ComponentBase
    {
        public virtual Task SetParametersAsync(ParameterView parameters) { return Task.CompletedTask; }
    }
}

namespace IntegrationConsoleTest
{
    partial class Program
    {
        static void Main(string[] args)
        {
        }
    }
    [GenerateSetParametersAsync]
    partial class Baz : ComponentBase
    {
        [Parameter] public object Bar1 { get; set; }
        [Parameter] public object Baz2 { get; set; }
        void Foo(string[] args)
        {
            BetterBlazorImplementation__WriteSingleParameter(""foo"", null);
        }
    }
}

";

            RunGenerator(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            Assert.Equal(3, generated.Length);
            Assert.True(generated.Any(g => g.Filename.EndsWith("GenerateSetParametersAsyncAttribute.cs")));
            generated.ContainsFileWithContent("IntegrationConsoleTest.Baz_override.cs", @"
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace IntegrationConsoleTest
{
    public partial class Baz
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
            generated.ContainsFileWithContent("IntegrationConsoleTest.Baz_implementation.cs", @"
using System;

namespace IntegrationConsoleTest
{
    public partial class Baz
    {
        private void BetterBlazorImplementation__WriteSingleParameter(string name, object value)
        {
            switch (name)
            {
                case ""Bar1"":
                    this.Bar1 = (object)value;
                    break;
                case ""Baz2"":
                    this.Baz2 = (object)value;
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
        public void Cascading()
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
         [CascadingParameter] public GenerateSetParametersAsyncAttribute Parameter3 { get; set; }
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
        public void PositiveMultipleParts()
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
    }
    [GenerateSetParametersAsyncAttribute]
    public partial class Component
    {
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
        public void PositiveNotParameters()
        {
            var userSource = @"
using Excubo.Generators.BetterBlazor;
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
            Assert.Equal(3, generated.Length);
            Assert.True(generated.Any(g => g.Filename.EndsWith("GenerateSetParametersAsyncAttribute.cs")));
            generated.ContainsFileWithContent("Testing.PositiveNotParameters.Component_override.cs", @"
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Testing.PositiveNotParameters
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
            generated.ContainsFileWithContent("Testing.PositiveNotParameters.Component_implementation.cs", @"
using System;

namespace Testing.PositiveNotParameters
{
    public partial class Component
    {
        private void BetterBlazorImplementation__WriteSingleParameter(string name, object value)
        {
            switch (name)
            {
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
