using System;
namespace Excubo.Generators.Blazor
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public sealed class RequiredAttribute : Attribute
    {
    }
}