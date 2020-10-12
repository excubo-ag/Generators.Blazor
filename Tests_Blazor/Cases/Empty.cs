using Tests_Blazor.Helpers;
using Xunit;

namespace Tests_Blazor
{
    public partial class GeneratorTests
    {
        [Fact]
        public void Empty()
        {
            var userSource = "";
            RunGenerator(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            Assert.Single(generated);
            generated.ContainsFileWithContent("GenerateSetParametersAsyncAttribute.cs", @"
using System;
namespace Excubo.Generators.Blazor
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
    }
}
