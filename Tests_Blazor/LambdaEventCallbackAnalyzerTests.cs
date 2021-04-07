using Excubo.Generators.Blazor;
using Tests_Blazor.Helpers;
using Xunit.Abstractions;

namespace Tests_Blazor
{
    public partial class LambdaEventCallbackAnalyzerTests : TestBase<LambdaEventCallbackAnalyzer>
    {
        public LambdaEventCallbackAnalyzerTests(ITestOutputHelper output_helper) : base(output_helper)
        {
        }
        // for the actual test cases, see folder "LambdaEventCallbackAnalyzer"
    }
}