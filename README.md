
# Excubo.Generators.Blazor

[![Nuget](https://img.shields.io/nuget/v/Excubo.Generators.Blazor)](https://www.nuget.org/packages/Excubo.Generators.Blazor/)
[![Nuget](https://img.shields.io/nuget/dt/Excubo.Generators.Blazor)](https://www.nuget.org/packages/Excubo.Generators.Blazor/)
[![GitHub](https://img.shields.io/github/license/excubo-ag/Generators.Blazor)](https://github.com/excubo-ag/Generators.Blazor)

This project improves the performance of Blazor components using source generators and provides helpful diagnostics.

## Installation

### Nuget

Excubo.Generators.Blazor is distributed [via nuget.org](https://www.nuget.org/packages/Excubo.Generators.Blazor/).
[![Nuget](https://img.shields.io/nuget/v/Excubo.Generators.Blazor)](https://www.nuget.org/packages/Excubo.Generators.Blazor/)

#### Package Manager:
```ps
Install-Package Excubo.Generators.Blazor
```

#### .NET Cli:
```cmd
dotnet add package Excubo.Generators.Blazor
```

#### Package Reference
```xml
<PackageReference Include="Excubo.Generators.Blazor" />
```

### Project settings

Your project needs to use C# 9.0, therefore `<LangVersion>latest</LangVersion>` or `<LangVersion>preview</LangVersion>` must be specified in the project file.

## SetParametersAsync Source Generator

### How does it work

Blazor uses C#-Reflection to handle the setting of component's `[Parameter]`s which is slower than a compile-time approach.
The `SetParametersAsync` generator overrides the default reflection-based implementation of `Task SetParametersAsync(ParameterView parameters)` following this 
[recommendation by MS](https://github.com/dotnet/AspNetCore.Docs/blob/1e199f340780f407a685695e6c4d953f173fa891/aspnetcore/blazor/webassembly-performance-best-practices.md#implement-setparametersasync-manually).

This increases the performance of setting parameters of components up to 6x.

### How to enable

Add `@attribute [Excubo.Generators.Blazor.GenerateSetParametersAsync]` to your `_Imports.razor` file. This enables the source generator on _all_ components.
As sometimes you might want to override the method yourself, you can opt-out of the source generator by adding `@attribute [Excubo.Generators.Blazor.DoNotGenerateSetParametersAsync]` to a component.

You can use `[GenerateSetParametersAsync(RequireExactMatch = true)]`, if you do not require parameters to match when they differ in case.

### Implementation details

If you write the code

```cs
using Excubo.Generators.Blazor;
using Microsoft.AspNetCore.Components;

namespace IntegrationTest
{
    [GenerateSetParametersAsync]
    public partial class Component : ComponentBase
    {
        [Parameter] public string Parameter1 { get; set; }
        // usually you have more parameters
    }
}
```

the source generator generates

```cs
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;
using System;

namespace IntegrationTest
{
    public partial class Component
    {
        public override Task SetParametersAsync(ParameterView parameters)
        {
            foreach (var parameter in parameters)
            {
                BlazorImplementation__WriteSingleParameter(parameter.Name, parameter.Value);
            }

            // Run the normal lifecycle methods, but without assigning parameters again
            return base.SetParametersAsync(ParameterView.Empty);
        }

        private void BlazorImplementation__WriteSingleParameter(string name, object value)
        {
            switch (name) // parameter properties are actually case insensitive. This is ignored here for performance, but handled later for correctness
            {
                case "Parameter1":
                    this.Parameter1 = (string)value;
                    break;
                // more parameters would create more cases
                default:
                {
                    switch (name.ToLowerInvariant()) // parameter properties are actually case insensitive.
                    {
                        case "parameter1":
                            this.Parameter1 = (string)value;
                            break;
                        // more parameters would create more cases
                        default:
                            throw new ArgumentException($"Unknown parameter: {name}");
                    }
                    break;
                }
            }
        }
    }
}
```
## Diagnostic for missing `@key` in loops

There is a common source of issues when loops are used in Blazor without assigning `@key`s to the contained elements/components. This leads to performance issues, and can also lead to issues with correctness (e.g. when it is important which component gets disposed).
By installing this nuget package, you get warnings when you forget to set `@key`:

```html
@foreach (var element in items)
 ~~~~~~~
 Warning: A key must be used when rendering loops in Blazor
{
    <div class="my-component">
        @element
    </div>
}
```

## Experimental diagnostic: required parameters

In some situations it's an advantage to be able to mark parameters as required. With this package you have two ways to say that:

1. explicitly required

```cs
@code {
    [Required][Parameter] public T Value { get; set; }
}
```

2. all parameters are required

```cs
// either in your Component.razor or in _Imports.razor
@attribute [Excubo.Generators.Blazor.ParametersAreRequiredByDefault]
```