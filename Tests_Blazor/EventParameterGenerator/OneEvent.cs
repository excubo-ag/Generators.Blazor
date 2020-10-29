using Tests_Blazor.Helpers;
using Xunit;

namespace Tests_Blazor
{
    public partial class EventParameterGeneratorTests
    {
        [Fact]
        public void OneEventFromOneCategoryForgottenCallback()
        {
            var userSource = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Excubo.Generators.Blazor.ExperimentalDoNotUseYet;

namespace N.S
{
    [GenerateEvents(Error.onerror)]
    public partial class Component : Microsoft.AspNetCore.Components.ComponentBase
    {
        protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder __builder)
        {
            __builder.OpenElement(0, ""div"");
            __builder.AddEventPreventDefaultAttribute(1, ""onerror"", onerrorPreventDefault);
            __builder.AddEventStopPropagationAttribute(2, ""onerror"", onerrorStopPropagation);
            __builder.CloseElement();
        }
}";
            RunGenerator(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(new DiagnosticResult("BB0002", "Component", Microsoft.CodeAnalysis.DiagnosticSeverity.Warning).WithLocation(13, 26));
            Assert.Single(generated);
            generated.ContainsFileWithContent("_parameters.cs", @"
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace N.S
{
    public partial class Component
    {
        [Parameter] public EventCallback<Microsoft.AspNetCore.Components.Web.ErrorEventArgs> onerror { get; set; }
        [Parameter] public bool onerrorStopPropagation { get; set; }
        [Parameter] public bool onerrorPreventDefault { get; set; }
    }
}");
        }
    }
    public partial class EventParameterGeneratorTests
    {
        [Fact]
        public void OneEventFromOneCategoryForgottenPreventDefault()
        {
            var userSource = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Excubo.Generators.Blazor.ExperimentalDoNotUseYet;

namespace N.S
{
    [GenerateEvents(Error.onerror)]
    public partial class Component : Microsoft.AspNetCore.Components.ComponentBase
    {
        protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder __builder)
        {
            __builder.OpenElement(0, ""div"");
            __builder.AddAttribute(1, ""onerror"", Microsoft.AspNetCore.Components.EventCallback.Factory.Create<Microsoft.AspNetCore.Components.Web.ErrorEventArgs>(this, onerror));
            __builder.AddEventStopPropagationAttribute(21, ""onerror"", onerrorStopPropagation);
            __builder.CloseElement();
        }
}";
            RunGenerator(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(new DiagnosticResult("BB0007", "Component", Microsoft.CodeAnalysis.DiagnosticSeverity.Warning).WithLocation(13, 26));
            Assert.Single(generated);
            generated.ContainsFileWithContent("_parameters.cs", @"
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace N.S
{
    public partial class Component
    {
        [Parameter] public EventCallback<Microsoft.AspNetCore.Components.Web.ErrorEventArgs> onerror { get; set; }
        [Parameter] public bool onerrorStopPropagation { get; set; }
        [Parameter] public bool onerrorPreventDefault { get; set; }
    }
}");
        }
    }
    public partial class EventParameterGeneratorTests
    {
        [Fact]
        public void OneEventFromOneCategoryForgottenStopPropagation()
        {
            var userSource = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Excubo.Generators.Blazor.ExperimentalDoNotUseYet;

namespace N.S
{
    [GenerateEvents(Error.onerror)]
    public partial class Component : Microsoft.AspNetCore.Components.ComponentBase
    {
        protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder __builder)
        {
            __builder.OpenElement(0, ""div"");
            __builder.AddAttribute(1, ""onerror"", Microsoft.AspNetCore.Components.EventCallback.Factory.Create<Microsoft.AspNetCore.Components.Web.ErrorEventArgs>(this, onerror));
            __builder.AddEventPreventDefaultAttribute(21, ""onerror"", onerrorPreventDefault);
            __builder.CloseElement();
        }
}";
            RunGenerator(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(new DiagnosticResult("BB0006", "Component", Microsoft.CodeAnalysis.DiagnosticSeverity.Warning).WithLocation(13, 26));
            Assert.Single(generated);
            generated.ContainsFileWithContent("_parameters.cs", @"
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace N.S
{
    public partial class Component
    {
        [Parameter] public EventCallback<Microsoft.AspNetCore.Components.Web.ErrorEventArgs> onerror { get; set; }
        [Parameter] public bool onerrorStopPropagation { get; set; }
        [Parameter] public bool onerrorPreventDefault { get; set; }
    }
}");
        }
    }
    public partial class EventParameterGeneratorTests
    {
        [Fact]
        public void OneEventFromOneCategory()
        {
            var userSource = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Excubo.Generators.Blazor.ExperimentalDoNotUseYet;

namespace N.S
{
    [GenerateEvents(Error.onerror)]
    public partial class Component : Microsoft.AspNetCore.Components.ComponentBase
    {
        protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder __builder)
        {
            __builder.OpenElement(0, ""div"");
            __builder.AddAttribute(1, ""onerror"", Microsoft.AspNetCore.Components.EventCallback.Factory.Create<Microsoft.AspNetCore.Components.Web.ErrorEventArgs>(this, onerror));
            __builder.AddEventStopPropagationAttribute(11, ""onerror"", onerrorStopPropagation);
            __builder.AddEventPreventDefaultAttribute(21, ""onerror"", onerrorPreventDefault);
            __builder.CloseElement();
        }
}";
            RunGenerator(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            Assert.Single(generated);
            generated.ContainsFileWithContent("_parameters.cs", @"
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace N.S
{
    public partial class Component
    {
        [Parameter] public EventCallback<Microsoft.AspNetCore.Components.Web.ErrorEventArgs> onerror { get; set; }
        [Parameter] public bool onerrorStopPropagation { get; set; }
        [Parameter] public bool onerrorPreventDefault { get; set; }
    }
}");
        }
    }
    public partial class EventParameterGeneratorTests
    {
        [Fact]
        public void OneEventFromTwoCategories()
        {
            var userSource = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Excubo.Generators.Blazor.ExperimentalDoNotUseYet;

namespace N.S
{
    [GenerateEvents(Error.onerror)]
    [GenerateEvents(General.onactivate)]
    public partial class Component : Microsoft.AspNetCore.Components.ComponentBase
    {
        protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder __builder)
        {
            __builder.OpenElement(0, ""div"");
            __builder.AddAttribute(1, ""onerror"", Microsoft.AspNetCore.Components.EventCallback.Factory.Create<Microsoft.AspNetCore.Components.Web.ErrorEventArgs>(this, onerror));
            __builder.AddAttribute(2, ""onactivate"", Microsoft.AspNetCore.Components.EventCallback.Factory.Create<System.EventArgs>(this, onactivate));
            __builder.AddEventStopPropagationAttribute(11, ""onerror"", onerrorStopPropagation);
            __builder.AddEventStopPropagationAttribute(12, ""onactivate"", onactivateStopPropagation);
            __builder.AddEventPreventDefaultAttribute(21, ""onerror"", onerrorPreventDefault);
            __builder.AddEventPreventDefaultAttribute(22, ""onactivate"", onactivatePreventDefault);
            __builder.CloseElement();
        }
}";
            RunGenerator(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            Assert.Single(generated);
            generated.ContainsFileWithContent("_parameters.cs", @"
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace N.S
{
    public partial class Component
    {
        [Parameter] public EventCallback<Microsoft.AspNetCore.Components.Web.ErrorEventArgs> onerror { get; set; }
        [Parameter] public bool onerrorStopPropagation { get; set; }
        [Parameter] public bool onerrorPreventDefault { get; set; }
        [Parameter] public EventCallback<System.EventArgs> onactivate { get; set; }
        [Parameter] public bool onactivateStopPropagation { get; set; }
        [Parameter] public bool onactivatePreventDefault { get; set; }
    }
}");
        }
    }
    public partial class EventParameterGeneratorTests
    {
        [Fact]
        public void AllEventsFromOneCategory()
        {
            var userSource = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Excubo.Generators.Blazor.ExperimentalDoNotUseYet;

namespace N.S
{
    [GenerateEvents(Progress.all)]
    public partial class Component : Microsoft.AspNetCore.Components.ComponentBase
    {
        protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder __builder)
        {
            __builder.OpenElement(0, ""div"");
            __builder.AddAttribute(1, ""onabort"", Microsoft.AspNetCore.Components.EventCallback.Factory.Create<Microsoft.AspNetCore.Components.Web.ProgressEventArgs>(this, onabort));
            __builder.AddAttribute(2, ""onload"", Microsoft.AspNetCore.Components.EventCallback.Factory.Create<Microsoft.AspNetCore.Components.Web.ProgressEventArgs>(this, onload));
            __builder.AddAttribute(3, ""onloadend"", Microsoft.AspNetCore.Components.EventCallback.Factory.Create<Microsoft.AspNetCore.Components.Web.ProgressEventArgs>(this, onloadend));
            __builder.AddAttribute(4, ""onloadstart"", Microsoft.AspNetCore.Components.EventCallback.Factory.Create<Microsoft.AspNetCore.Components.Web.ProgressEventArgs>(this, onloadstart));
            __builder.AddAttribute(5, ""onprogress"", Microsoft.AspNetCore.Components.EventCallback.Factory.Create<Microsoft.AspNetCore.Components.Web.ProgressEventArgs>(this, onprogress));
            __builder.AddAttribute(6, ""ontimeout"", Microsoft.AspNetCore.Components.EventCallback.Factory.Create<Microsoft.AspNetCore.Components.Web.ProgressEventArgs>(this, ontimeout));
            __builder.AddEventStopPropagationAttribute(11, ""onabort"", onabortStopPropagation);
            __builder.AddEventStopPropagationAttribute(12, ""onload"", onloadStopPropagation);
            __builder.AddEventStopPropagationAttribute(13, ""onloadend"", onloadendStopPropagation);
            __builder.AddEventStopPropagationAttribute(14, ""onloadstart"", onloadstartStopPropagation);
            __builder.AddEventStopPropagationAttribute(15, ""onprogress"", onprogressStopPropagation);
            __builder.AddEventStopPropagationAttribute(16, ""ontimeout"", ontimeoutStopPropagation);
            __builder.AddEventPreventDefaultAttribute(21, ""onabort"", onabortPreventDefault);
            __builder.AddEventPreventDefaultAttribute(22, ""onload"", onloadPreventDefault);
            __builder.AddEventPreventDefaultAttribute(23, ""onloadend"", onloadendPreventDefault);
            __builder.AddEventPreventDefaultAttribute(24, ""onloadstart"", onloadstartPreventDefault);
            __builder.AddEventPreventDefaultAttribute(25, ""onprogress"", onprogressPreventDefault);
            __builder.AddEventPreventDefaultAttribute(26, ""ontimeout"", ontimeoutPreventDefault);
            __builder.CloseElement();
        }
}";
            RunGenerator(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            Assert.Single(generated);
            generated.ContainsFileWithContent("_parameters.cs", @"
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace N.S
{
    public partial class Component
    {
        [Parameter] public EventCallback<Microsoft.AspNetCore.Components.Web.ProgressEventArgs> onabort { get; set; }
        [Parameter] public bool onabortStopPropagation { get; set; }
        [Parameter] public bool onabortPreventDefault { get; set; }
        [Parameter] public EventCallback<Microsoft.AspNetCore.Components.Web.ProgressEventArgs> onload { get; set; }
        [Parameter] public bool onloadStopPropagation { get; set; }
        [Parameter] public bool onloadPreventDefault { get; set; }
        [Parameter] public EventCallback<Microsoft.AspNetCore.Components.Web.ProgressEventArgs> onloadend { get; set; }
        [Parameter] public bool onloadendStopPropagation { get; set; }
        [Parameter] public bool onloadendPreventDefault { get; set; }
        [Parameter] public EventCallback<Microsoft.AspNetCore.Components.Web.ProgressEventArgs> onloadstart { get; set; }
        [Parameter] public bool onloadstartStopPropagation { get; set; }
        [Parameter] public bool onloadstartPreventDefault { get; set; }
        [Parameter] public EventCallback<Microsoft.AspNetCore.Components.Web.ProgressEventArgs> onprogress { get; set; }
        [Parameter] public bool onprogressStopPropagation { get; set; }
        [Parameter] public bool onprogressPreventDefault { get; set; }
        [Parameter] public EventCallback<Microsoft.AspNetCore.Components.Web.ProgressEventArgs> ontimeout { get; set; }
        [Parameter] public bool ontimeoutStopPropagation { get; set; }
        [Parameter] public bool ontimeoutPreventDefault { get; set; }
    }
}");
        }
    }
}
