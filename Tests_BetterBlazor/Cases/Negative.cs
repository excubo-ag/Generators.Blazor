﻿using System.Linq;
using Tests_BetterBlazor.Helpers;
using Xunit;

namespace Tests_BetterBlazor
{
    public partial class GeneratorTests
    {
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
