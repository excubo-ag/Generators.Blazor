using Excubo.Generators.Blazor;
using Tests_Blazor.Helpers;
using Xunit.Abstractions;

namespace Tests_Blazor
{
    public partial class RequiredParameterAnalyzerTests : TestBase<RequiredParameterAnalyzer>
    {
        public RequiredParameterAnalyzerTests(ITestOutputHelper output_helper) : base(output_helper)
        {
        }
        // for the actual test cases, see folder "RequiredParameterAnalyzer"
    }
}