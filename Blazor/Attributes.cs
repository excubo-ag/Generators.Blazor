using System;

namespace Excubo.Generators.Blazor.ExperimentalDoNotUseYet
{
    /* These events don't bubble
  'abort',
  'blur',
  'change',
  'error',
  'focus',
  'load',
  'loadend',
  'loadstart',
  'mouseenter',
  'mouseleave',
  'progress',
  'reset',
  'scroll',
  'submit',
  'unload',
  'toggle',
     */
    // should this have [Flag] and what about underlying type
    public enum Clipboard
    {
        oncut = 1 << 0,
        oncopy = 1 << 1,
        onpaste = 1 << 2,
        onbeforecut = 1 << 3,
        onbeforecopy = 1 << 4,
        onbeforepaste = 1 << 5,
        all = -1
    }
    public enum Drag
    {
        ondrag = 1 << 0,
        ondragstart = 1 << 1,
        ondragenter = 1 << 2,
        ondragleave = 1 << 3,
        ondragover = 1 << 4,
        ondrop = 1 << 5,
        ondragend = 1 << 6,
        all = -1
    }
    public enum Error
    {
        onerror = 1 << 0,
    }
    public enum General
    {
        onactivate = 1 << 0,
        onbeforeactivate = 1 << 1,
        onbeforedeactivate = 1 << 2,
        ondeactivate = 1 << 3,
        onfullscreenchange = 1 << 4,
        onfullscreenerror = 1 << 5,
        onloadeddata = 1 << 6,
        onloadedmetadata = 1 << 7,
        onpointerlockchange = 1 << 8,
        onpointerlockerror = 1 << 9,
        onreadystatechange = 1 << 10,
        onscroll = 1 << 11,
        all = -1
    }
    public enum Input
    {
        oninvalid = 1 << 0,
        onreset = 1 << 1,
        onselect = 1 << 2,
        onselectionchange = 1 << 3,
        onselectstart = 1 << 4,
        onsubmit = 1 << 5,
        onchange = 1 << 6,
        oninput = 1 << 7,
        all = -1
    }
    public enum Media
    {
        oncanplay = 1 << 0,
        oncanplaythrough = 1 << 1,
        oncuechange = 1 << 2,
        ondurationchange = 1 << 3,
        onemptied = 1 << 4,
        onended = 1 << 5,
        onpause = 1 << 6,
        onplay = 1 << 7,
        onplaying = 1 << 8,
        onratechange = 1 << 9,
        onseeked = 1 << 10,
        onseeking = 1 << 11,
        onstalled = 1 << 12,
        onstop = 1 << 13,
        onsuspend = 1 << 14,
        ontimeupdate = 1 << 15,
        onvolumechange = 1 << 16,
        onwaiting = 1 << 17,
        all = -1
    }
    public enum Focus
    {
        onfocus = 1 << 0,
        onblur = 1 << 1,
        onfocusin = 1 << 2,
        onfocusout = 1 << 3,
        all = -1
    }
    public enum Keyboard
    {
        onkeydown = 1 << 0,
        onkeypress = 1 << 1,
        onkeyup = 1 << 2,
        all = -1
    }
    public enum Mouse
    {
        onclick = 1 << 0,
        oncontextmenu = 1 << 1,
        ondblclick = 1 << 2,
        onmousedown = 1 << 3,
        onmouseup = 1 << 4,
        onmouseover = 1 << 5,
        onmousemove = 1 << 6,
        onmouseout = 1 << 7,
        all = -1
    }
    public enum Pointer
    {
        onpointerdown = 1 << 0,
        onpointerup = 1 << 1,
        onpointercancel = 1 << 2,
        onpointermove = 1 << 3,
        onpointerover = 1 << 4,
        onpointerout = 1 << 5,
        onpointerenter = 1 << 6,
        onpointerleave = 1 << 7,
        ongotpointercapture = 1 << 8,
        onlostpointercapture = 1 << 9,
        all = -1
    }
    public enum Wheel
    {
        onwheel = 1 << 0,
        onmousewheel = 1 << 1,
        all = -1
    }
    public enum Progress
    {
        onabort = 1 << 0,
        onload = 1 << 1,
        onloadend = 1 << 2,
        onloadstart = 1 << 3,
        onprogress = 1 << 4,
        ontimeout = 1 << 5,
        all = -1
    }
    public enum Touch
    {
        ontouchstart = 1 << 0,
        ontouchend = 1 << 1,
        ontouchmove = 1 << 2,
        ontouchenter = 1 << 3,
        ontouchleave = 1 << 4,
        ontouchcancel = 1 << 5,
        all = -1
    }
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class GenerateEventsAttribute : Attribute
    {
        public GenerateEventsAttribute(Clipboard events) { }
        public GenerateEventsAttribute(Drag events) { }
        public GenerateEventsAttribute(Error events) { }
        public GenerateEventsAttribute(General events) { }
        public GenerateEventsAttribute(Input events) { }
        public GenerateEventsAttribute(Media events) { }
        public GenerateEventsAttribute(Focus events) { }
        public GenerateEventsAttribute(Keyboard events) { }
        public GenerateEventsAttribute(Mouse events) { }
        public GenerateEventsAttribute(Pointer events) { }
        public GenerateEventsAttribute(Wheel events) { }
        public GenerateEventsAttribute(Progress events) { }
        public GenerateEventsAttribute(Touch events) { }
    }
}
namespace Excubo.Generators.Blazor
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
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