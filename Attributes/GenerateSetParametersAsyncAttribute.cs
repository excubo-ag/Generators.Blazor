using System;
namespace Excubo.Generators.Blazor
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class GenerateSetParametersAsyncAttribute : Attribute
    {
        public bool RequireExactMatch { get; set; }
    }
}