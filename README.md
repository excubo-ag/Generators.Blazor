
# Excubo.Generators.Blazor

[![Nuget](https://img.shields.io/nuget/v/Excubo.Generators.Blazor)](https://www.nuget.org/packages/Excubo.Generators.Blazor/)
[![Nuget](https://img.shields.io/nuget/dt/Excubo.Generators.Blazor)](https://www.nuget.org/packages/Excubo.Generators.Blazor/)
[![GitHub](https://img.shields.io/github/license/excubo-ag/Generators.Blazor)](https://github.com/excubo-ag/Generators.Blazor)

This project improves the performance of Blazor components using source generators.

## How does it work

Blazor uses C#-Reflection to handle the setting of component's `[Parameter]`s which is slower than a compile-time approach.
The `SetParametersAsync` generator overrides the default reflection-based implementation of `Task SetParametersAsync(ParameterView parameters)` following this 
[recommendation by MS](https://github.com/dotnet/AspNetCore.Docs/blob/1e199f340780f407a685695e6c4d953f173fa891/aspnetcore/blazor/webassembly-performance-best-practices.md#implement-setparametersasync-manually).

This increases the performance of setting parameters of components up to 6x.

## How to use

### 1. Install the nuget package Excubo.Generators.Blazor

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

### 2. Enable `GenerateSetParametersAsync` for all components

Add `@attribute [Excubo.Generators.Blazor.GenerateSetParametersAsync]` to your `_Imports.razor` file. This enables the source generator on _all_ components.
As sometimes you might want to override the method yourself, you can opt-out of the source generator by adding `@attribute [Excubo.Generators.Blazor.DoNotGenerateSetParametersAsync]` to a component.

## Implementation

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
