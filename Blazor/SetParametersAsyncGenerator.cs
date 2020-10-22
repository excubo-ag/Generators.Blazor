using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Excubo.Generators.Blazor
{
    [Generator]
    public partial class SetParametersAsyncGenerator : ISourceGenerator
    {
        private static readonly DiagnosticDescriptor ParameterNameConflict = new DiagnosticDescriptor("BB0001", "Parameter name conflict", "Parameter names are case insensitive. {0} conflicts with {1}.", "Conflict", DiagnosticSeverity.Error, isEnabledByDefault: true, description: "Parameter names must be case insensitive to be usable in routes. Rename the parameter to not be in conflict with other parameters.");

        private const string SetParametersAsyncAttributeText = @"
using System;
namespace Excubo.Generators.Blazor
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    sealed class GenerateSetParametersAsyncAttribute : Attribute
    {
        public bool RequireExactMatch { get; set; }
    }
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    sealed class DoNotGenerateSetParametersAsyncAttribute : Attribute
    {
    }
}
";

        public void Initialize(GeneratorInitializationContext context)
        {
            // Register a syntax receiver that will be created for each generation pass
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            // https://github.com/dotnet/AspNetCore.Docs/blob/1e199f340780f407a685695e6c4d953f173fa891/aspnetcore/blazor/webassembly-performance-best-practices.md#implement-setparametersasync-manually
            context.AddCode("GenerateSetParametersAsyncAttribute", SetParametersAsyncAttributeText);

            if (!(context.SyntaxReceiver is SyntaxReceiver receiver))
            {
                return;
            }

            var candidate_classes = GetCandidateClasses(receiver, GetCompilation(context));

            foreach (var class_symbol in candidate_classes.Distinct(SymbolEqualityComparer.Default).Cast<INamedTypeSymbol>())
            {
                GenerateSetParametersAsyncMethod(context, class_symbol);
            }
        }

        private static void GenerateSetParametersAsyncMethod(GeneratorExecutionContext context, INamedTypeSymbol class_symbol)
        {
            var force_exact_match = class_symbol.GetAttributes().Any(a => a.NamedArguments.Any(na => na.Key == "RequireExactMatch" && na.Value.Value is bool v && v));
            var namespaceName = class_symbol.ContainingNamespace.ToDisplayString();
            var type_kind = class_symbol.TypeKind switch { TypeKind.Class => "class", TypeKind.Interface => "interface", _ => "struct" };
            var type_parameters = string.Join(", ", class_symbol.TypeArguments.Select(t => t.Name));
            type_parameters = string.IsNullOrEmpty(type_parameters) ? type_parameters : "<" + type_parameters + ">";
            context.AddCode(class_symbol.ToDisplayString() + "_override.cs", $@"
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

#pragma warning disable CS0162
#pragma warning disable CS8632
namespace {namespaceName}
{{
    public partial class {class_symbol.Name}{type_parameters}
    {{
        public override Task SetParametersAsync(ParameterView parameters)
        {{
            foreach (var parameter in parameters)
            {{
                BlazorImplementation__WriteSingleParameter(parameter.Name, parameter.Value);
            }}

            // Run the normal lifecycle methods, but without assigning parameters again
            return base.SetParametersAsync(ParameterView.Empty);
        }}
    }}
}}
#pragma warning restore CS8632
#pragma warning restore CS0162
");
            var type_with_bases = class_symbol.GetTypeHierarchy();
            var members = type_with_bases.SelectMany(t => t.GetMembers());
            var property_symbols = members.OfType<IPropertySymbol>();
            var writable_property_symbols = property_symbols.Where(ps => !ps.IsReadOnly);
            var parameter_symbols = writable_property_symbols
                .Where(ps => ps.GetAttributes().Any(a => false
                || a.AttributeClass.Name == "Parameter"
                || a.AttributeClass.Name == "ParameterAttribute"
                || a.AttributeClass.Name == "CascadingParameter"
                || a.AttributeClass.Name == "CascadingParameterAttribute"
            ));
            var name_conflicts = parameter_symbols.GroupBy(ps => ps.Name.ToLowerInvariant()).Where(g => g.Count() > 1);
            foreach (var conflict in name_conflicts)
            {
                var key = conflict.Key;
                var conflicting_parameters = conflict.ToList();
                foreach (var parameter in conflicting_parameters)
                {
                    var this_name = parameter.Name;
                    var conflicting_name = conflicting_parameters.Select(p => p.Name).FirstOrDefault(n => n != this_name);
                    foreach (var location in parameter.Locations)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(ParameterNameConflict, location, this_name, conflicting_name));
                    }
                }
            }
            var all = parameter_symbols.ToList();
            var catch_all_parameter = parameter_symbols.FirstOrDefault(p =>
            {
                var parameter_attr = p.GetAttributes().FirstOrDefault(a => a.AttributeClass.Name.StartsWith("Parameter"));
                return parameter_attr != null && parameter_attr.NamedArguments.Any(n => n.Key == "CaptureUnmatchedValues" && n.Value.Value is bool v && v);
            });
            var lower_case_match_cases = parameter_symbols.Except(new[] { catch_all_parameter }).Select(p => $"case \"{p.Name.ToLowerInvariant()}\": this.{p.Name} = ({p.Type.ToDisplayString()}) value; break;");
            var lower_case_match_default = catch_all_parameter == null ? @"default: throw new ArgumentException($""Unknown parameter: {name}"");" : $@"
default:
{{
    this.{catch_all_parameter.Name} ??= new System.Collections.Generic.Dictionary<string, object>();
    if (!this.{catch_all_parameter.Name}.ContainsKey(name))
    {{
        this.{catch_all_parameter.Name}.Add(name, value);
    }}
    else
    {{
        this.{catch_all_parameter.Name}[name] = value;
    }}
    break;
}}";

            var exact_match_cases = parameter_symbols.Except(new[] { catch_all_parameter }).Select(p => $"case \"{p.Name}\": this.{p.Name} = ({p.Type.ToDisplayString()}) value; break;");
            string exact_match_default;
            if (force_exact_match)
            {
                if (catch_all_parameter == null) // exact matches are forced, and we do not have a catch-all parameter, therefore we need to throw on unmatched parameter
                {
                    exact_match_default = @"default: { throw new ArgumentException($""Unknown parameter: {name}""); }";
                }
                else // exact matches are forced, and we DO have a catch-all parameter, therefore we simply add that unmatched parameter to the dictionary
                {
                    exact_match_default = $@"
default:
{{
    this.{catch_all_parameter.Name} ??= new System.Collections.Generic.Dictionary<string, object>();
    if (!this.{catch_all_parameter.Name}.ContainsKey(name))
    {{
        this.{catch_all_parameter.Name}.Add(name, value);
    }}
    else
    {{
        this.{catch_all_parameter.Name}[name] = value;
    }}
    break;
}}";
                }
            }
            else
            {
                // exact matches are not forced, so if there is no exact match, we fall back to compare it in lower case
                exact_match_default = $@"
default:
{{
    switch (name.ToLowerInvariant())
    {{
        {string.Join("\n", lower_case_match_cases)}
        {lower_case_match_default}
    }}
    break;
}}
";
            }
            context.AddCode(class_symbol.ToDisplayString() + "_implementation.cs", $@"
using System;

#pragma warning disable CS0162
#pragma warning disable CS8632
namespace {namespaceName}
{{
    public partial class {class_symbol.Name}{type_parameters}
    {{
        private void BlazorImplementation__WriteSingleParameter(string name, object value)
        {{
            switch (name)
            {{
                {string.Join("\n", exact_match_cases)}
                {exact_match_default}
            }}
        }}
    }}
}}
#pragma warning restore CS8632
#pragma warning restore CS0162");
        }

        private static Compilation GetCompilation(GeneratorExecutionContext context)
        {
            var options = (context.Compilation as CSharpCompilation)!.SyntaxTrees[0].Options as CSharpParseOptions;
            var compilation = context.Compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(SourceText.From(SetParametersAsyncAttributeText, Encoding.UTF8), options));
            return compilation;
        }

        /// <summary>
        /// Enumerate methods with at least one Group attribute
        /// </summary>
        /// <param name="receiver"></param>
        /// <param name="compilation"></param>
        /// <returns></returns>
        private static IEnumerable<INamedTypeSymbol> GetCandidateClasses(SyntaxReceiver receiver, Compilation compilation)
        {
            var positiveAttributeSymbol = compilation.GetTypeByMetadataName("Excubo.Generators.Blazor.GenerateSetParametersAsyncAttribute");
            var negativeAttributeSymbol = compilation.GetTypeByMetadataName("Excubo.Generators.Blazor.DoNotGenerateSetParametersAsyncAttribute");

            // loop over the candidate methods, and keep the ones that are actually annotated
            foreach (var class_declaration in receiver.CandidateClasses)
            {
                var model = compilation.GetSemanticModel(class_declaration.SyntaxTree);
                var class_symbol = model.GetDeclaredSymbol(class_declaration);
                if (class_symbol is null)
                {
                    continue;
                }
                if (class_symbol.Name == "_Imports")
                {
                    continue;
                }
                var attributes = class_symbol.GetAttributes();
                if (attributes.Any(ad => ad.AttributeClass != null && ad.AttributeClass.Equals(positiveAttributeSymbol, SymbolEqualityComparer.Default))
                    && !attributes.Any(ad => ad.AttributeClass != null && ad.AttributeClass.Equals(negativeAttributeSymbol, SymbolEqualityComparer.Default)))
                {
                    yield return class_symbol;
                }
            }
        }

        /// <summary>
        /// Created on demand before each generation pass
        /// </summary>
        internal class SyntaxReceiver : ISyntaxReceiver
        {
            public List<ClassDeclarationSyntax> CandidateClasses { get; } = new List<ClassDeclarationSyntax>();

            /// <summary>
            /// Called for every syntax node in the compilation, we can inspect the nodes and save any information useful for generation
            /// </summary>
            public void OnVisitSyntaxNode(SyntaxNode syntax_node)
            {
                // any class with at least one attribute is a candidate for property generation
                if (syntax_node is ClassDeclarationSyntax classDeclarationSyntax && classDeclarationSyntax.AttributeLists.Count > 0)
                {
                    CandidateClasses.Add(classDeclarationSyntax);
                }
            }
        }
    }
}