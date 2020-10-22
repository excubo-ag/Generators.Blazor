using Tests_Blazor.Helpers;
using Xunit;

namespace Tests_Blazor
{
    public partial class RequiredParameterAnalyzerTests
    {
        [Fact]
        public void Empty()
        {
            var userSource = @"";
            RunGenerator(userSource, out var generatorDiagnostics, out _);
            generatorDiagnostics.Verify();
        }
    }
    public partial class RequiredParameterAnalyzerTests
    {
        [Fact]
        public void ParamMissing()
        {
            var userSource = @"
namespace Foo
{
    public class Bar
    {
        public void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenComponent<Bar>(0);
            builder.CloseComponent();
        }
        [Required][Parameter] public object Value { get; set; }
    }
}";
            RunGenerator(userSource, out var generatorDiagnostics, out _);
            generatorDiagnostics.Verify(new DiagnosticResult("BB0004", "builder.OpenComponent<Bar>(0)", Microsoft.CodeAnalysis.DiagnosticSeverity.Warning).WithLocation(8, 13));
        }
    }
    public partial class RequiredParameterAnalyzerTests
    {
        [Fact]
        public void ParamMissing_ActiveByDefault()
        {
            var userSource = @"
namespace Foo
{
    [ParametersAreRequiredByDefault]
    public class Bar
    {
        public void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenComponent<Bar>(0);
            builder.CloseComponent();
        }
        [Parameter] public object Value { get; set; }
    }
}";
            RunGenerator(userSource, out var generatorDiagnostics, out _);
            generatorDiagnostics.Verify(new DiagnosticResult("BB0004", "builder.OpenComponent<Bar>(0)", Microsoft.CodeAnalysis.DiagnosticSeverity.Warning).WithLocation(9, 13));
        }
    }
    public partial class RequiredParameterAnalyzerTests
    {
        [Fact]
        public void ParamNotMissing_ActiveByDefault()
        {
            var userSource = @"
namespace Foo
{
    [ParametersAreRequiredByDefault]
    public class Bar
    {
        public void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenComponent<Bar>(0);
            builder.AddAttribute(1, ""Value"", null);
            builder.CloseComponent();
        }
        [Parameter] public object Value { get; set; }
    }
}";
            RunGenerator(userSource, out var generatorDiagnostics, out _);
            generatorDiagnostics.Verify();
        }
    }
    public partial class RequiredParameterAnalyzerTests
    {
        [Fact]
        public void ParamNotMissing()
        {
            var userSource = @"
namespace Foo
{
    [ParametersAreRequiredByDefault]
    public class Bar
    {
        public void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenComponent<Bar>(0);
            builder.AddAttribute(1, ""Value"", null);
            builder.CloseComponent();
        }
        [Required][Parameter] public object Value { get; set; }
    }
}";
            RunGenerator(userSource, out var generatorDiagnostics, out _);
            generatorDiagnostics.Verify();
        }
    }
    public partial class RequiredParameterAnalyzerTests
    {
        [Fact]
        public void ParamNotMissing_ActiveByDefault_Nameof()
        {
            var userSource = @"
namespace Foo
{
    [ParametersAreRequiredByDefault]
    public class Bar
    {
        public void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenComponent<Bar>(0);
            builder.AddAttribute(1, nameof(Foo.Bar.Value), null);
            builder.CloseComponent();
        }
        [Parameter] public object Value { get; set; }
    }
}";
            RunGenerator(userSource, out var generatorDiagnostics, out _);
            generatorDiagnostics.Verify();
        }
    }
    public partial class RequiredParameterAnalyzerTests
    {
        [Fact]
        public void ParamNotMissing_Nameof()
        {
            var userSource = @"
namespace Foo
{
    [ParametersAreRequiredByDefault]
    public class Bar
    {
        public void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenComponent<Bar>(0);
            builder.AddAttribute(1, nameof(Foo.Bar.Value), null);
            builder.CloseComponent();
        }
        [Required][Parameter] public object Value { get; set; }
    }
}";
            RunGenerator(userSource, out var generatorDiagnostics, out _);
            generatorDiagnostics.Verify();
        }
    }
}
