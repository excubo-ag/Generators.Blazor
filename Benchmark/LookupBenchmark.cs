using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Tests_Blazor
{
    public class Foo
    {
        public object Value { get; set; }
        public object ValueChanged { get; set; }
        public object ChildContent { get; set; }
        public object Class { get; set; }
        public object Style { get; set; }
        public object OnClick { get; set; }
        public object OnMouseOver { get; set; }
        public object OnMouseMove { get; set; }
        public object OnMouseOut { get; set; }
    }
    public class LookupBenchmark
    {
        static Dictionary<string, PropertyInfo> accessors;
        private void Reflection(string name, Foo foo, object value)
        {
            if (accessors == null)
            {
                accessors = typeof(Foo).GetProperties().ToDictionary(p => p.Name, p => p);
            }
            if (accessors.TryGetValue(name, out var accessor))
            {
                accessor.SetValue(foo, value);
            }
            else
            {
                var match = accessors.FirstOrDefault(a => a.Key.Equals(name, StringComparison.InvariantCultureIgnoreCase));
                if (match.Value != null)
                {
                    match.Value.SetValue(foo, value);
                }
                else
                {
                    throw new Exception();
                }
            }
        }
        private void SimpleSwitch(string name, Foo foo, object value)
        {
            switch (name)
            {
                case "Value":
                    foo.Value = value;
                    return;
                case "ValueChanged":
                    foo.ValueChanged = value;
                    return;
                case "ChildContent":
                    foo.ChildContent = value;
                    return;
                case "Class":
                    foo.Class = value;
                    return;
                case "Style":
                    foo.Style = value;
                    return;
                case "OnClick":
                    foo.OnClick = value;
                    return;
                case "OnMouseOver":
                    foo.OnMouseOver = value;
                    return;
                case "OnMouseMove":
                    foo.OnMouseMove = value;
                    return;
                case "OnMouseOut":
                    foo.OnMouseOut = value;
                    return;
                default:
                    {
                        switch (name.ToLowerInvariant())
                        {
                            case "value":
                                foo.Value = value;
                                return;
                            case "valuechanged":
                                foo.ValueChanged = value;
                                return;
                            case "childcontent":
                                foo.ChildContent = value;
                                return;
                            case "class":
                                foo.Class = value;
                                return;
                            case "style":
                                foo.Style = value;
                                return;
                            case "onclick":
                                foo.OnClick = value;
                                return;
                            case "onmouseover":
                                foo.OnMouseOver = value;
                                return;
                            case "onmousemove":
                                foo.OnMouseMove = value;
                                return;
                            case "onmouseout":
                                foo.OnMouseOut = value;
                                return;
                            default:
                                throw new Exception();
                        }
                        break;
                    }
            }
        }
        private void Optimized(string name, Foo foo, object value)
        {
            switch (name[0])
            {
                case 'O':
                case 'o':
                    {
                        if (name[2] == 'M' || name[2] == 'm')
                        {
                            switch (name[9])
                            {
                                case 'e':
                                    foo.OnMouseOver = value;
                                    return;
                                case 'v':
                                    foo.OnMouseMove = value;
                                    return;
                                case 't':
                                    foo.OnMouseOut = value;
                                    return;
                            }
                        }
                        else
                        {
                            foo.OnClick = value;
                            return;
                        }
                        break;
                    }
                case 'V':
                case 'v':
                    if (name.Length == 5)
                    {
                        foo.Value = value;
                    }
                    else
                    {
                        foo.ValueChanged = value;
                    }
                    return;
                case 'C':
                case 'c':
                    {
                        if (name.Length == 5)
                        {
                            foo.Class = value;
                        }
                        else
                        {
                            foo.ChildContent = value;
                        }
                    }
                    return;
                case 'S':
                case 's':
                    foo.Style = value;
                    return;
            }
            throw new Exception();
        }
        private const int N = 10000;
        private readonly List<List<string>> data;
        public LookupBenchmark()
        {
            var rnd = new Random();
            var all_names = new List<string> { "Value", "ValueChanged", "ChildContent", "Class", "Style", "OnClick", "OnMouseOver", "OnMouseMove", "OnMouseOut" };
            data = Enumerable.Range(0, N).Select(_ =>
            {
                var count = rnd.Next(2, 9);
                return all_names.OrderBy(_ => rnd.NextDouble()).Take(count).Select(n => rnd.NextDouble() < 0.002 ? n.ToLowerInvariant() : n).ToList();
            }).ToList();
        }
        [Benchmark]
        public int SimpleSwitch()
        {
            var value = new object();
            int max = 0;
            foreach (var row in data)
            {
                var foo = new Foo();
                foreach (var e in row)
                {
                    SimpleSwitch(e, foo, value);
                }
                int non_null =
                    (foo.ChildContent != null ? 1 : 0) +
                    (foo.Class != null ? 1 : 0) +
                    (foo.Style != null ? 1 : 0) +
                    (foo.Value != null ? 1 : 0) +
                    (foo.ValueChanged != null ? 1 : 0) +
                    (foo.OnClick != null ? 1 : 0) +
                    (foo.OnMouseMove != null ? 1 : 0) +
                    (foo.OnMouseOut != null ? 1 : 0) +
                    (foo.OnMouseOver != null ? 1 : 0);
                max = Math.Max(max, non_null);
            }
            return max;
        }
        [Benchmark]
        public int Optimized()
        {
            var value = new object();
            int max = 0;
            foreach (var row in data)
            {
                var foo = new Foo();
                foreach (var e in row)
                {
                    Optimized(e, foo, value);
                }
                int non_null =
                    (foo.ChildContent != null ? 1 : 0) +
                    (foo.Class != null ? 1 : 0) +
                    (foo.Style != null ? 1 : 0) +
                    (foo.Value != null ? 1 : 0) +
                    (foo.ValueChanged != null ? 1 : 0) +
                    (foo.OnClick != null ? 1 : 0) +
                    (foo.OnMouseMove != null ? 1 : 0) +
                    (foo.OnMouseOut != null ? 1 : 0) +
                    (foo.OnMouseOver != null ? 1 : 0);
                max = Math.Max(max, non_null);
            }
            return max;
        }
        [Benchmark]
        public int Reflection()
        {
            var value = new object();
            int max = 0;
            foreach (var row in data)
            {
                var foo = new Foo();
                foreach (var e in row)
                {
                    Reflection(e, foo, value);
                }
                int non_null =
                    (foo.ChildContent != null ? 1 : 0) +
                    (foo.Class != null ? 1 : 0) +
                    (foo.Style != null ? 1 : 0) +
                    (foo.Value != null ? 1 : 0) +
                    (foo.ValueChanged != null ? 1 : 0) +
                    (foo.OnClick != null ? 1 : 0) +
                    (foo.OnMouseMove != null ? 1 : 0) +
                    (foo.OnMouseOut != null ? 1 : 0) +
                    (foo.OnMouseOver != null ? 1 : 0);
                max = Math.Max(max, non_null);
            }
            return max;
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<LookupBenchmark>();
        }
    }
}