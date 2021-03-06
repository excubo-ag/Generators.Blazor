﻿using System.Linq;
using Tests_Blazor.Helpers;
using Xunit;

namespace Tests_Blazor
{
    public partial class SetParametersAsyncGeneratorTests
    {
        [Fact]
        public void Negative()
        {
            var userSource = @"
using Excubo.Generators.Blazor;
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
            Assert.Empty(generated);
        }
    }
}