using System;
namespace Excubo.Generators.Blazor
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class ParametersAreRequiredByDefaultAttribute : Attribute
    {
    }
}