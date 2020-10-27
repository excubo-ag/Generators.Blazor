using System;

namespace Excubo.Generators.Blazor
{
    public class ExperimentalDoNotUseYet
    {
        public enum HtmlEvent
        {
            Click = 1
        }
        [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
        public sealed class GenerateEventsAttribute : Attribute
        {
            public GenerateEventsAttribute(HtmlEvent events)
            {
            }
        }
    }
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class RequiredAttribute : Attribute
    {
    }
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class ParametersAreRequiredByDefaultAttribute : Attribute
    {
    }
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class GenerateSetParametersAsyncAttribute : Attribute
    {
        public bool RequireExactMatch { get; set; }
    }
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class DoNotGenerateSetParametersAsyncAttribute : Attribute
    {
    }
}
