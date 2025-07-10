using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Excubo.Generators.Blazor
{
    [Generator]
    public partial class EventParameterGenerator : IIncrementalGenerator
    {
        private static readonly DiagnosticDescriptor EventNotUsed = new DiagnosticDescriptor(
            id: "BB0002",
            title: "Event parameter not used",
            messageFormat: "Event {0} is a parameter, but is not used in markup",
            category: "Missing",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "TODO");
        private static readonly DiagnosticDescriptor EventStopPropagationNotUsed = new DiagnosticDescriptor(
            id: "BB0006",
            title: "EventStopPropagation parameter not used",
            messageFormat: "{0}StopPropagation is a parameter for an event, but is not used in markup",
            category: "Missing",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "TODO");
        private static readonly DiagnosticDescriptor EventPreventDefaultNotUsed = new DiagnosticDescriptor(
            id: "BB0007",
            title: "EventPreventDefault parameter not used",
            messageFormat: "{0}PreventDefault is a parameter for an event, but is not used in markup",
            category: "Missing",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "TODO");


        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Register a syntax receiver that will be created for each generation pass
            IncrementalValuesProvider<ClassDeclarationSyntax> classDeclarations = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: static (syntax_node, _) => syntax_node is ClassDeclarationSyntax classDeclarationSyntax && classDeclarationSyntax.AttributeLists.Count > 0,
                transform: static (context, _) => context.Node as ClassDeclarationSyntax)
                .Where(static m => m is not null)!;
            IncrementalValueProvider<(Compilation, ImmutableArray<ClassDeclarationSyntax>)> compilationAndClasses
    = context.CompilationProvider.Combine(classDeclarations.Collect());
            context.RegisterSourceOutput(compilationAndClasses, static (spc, source) => Execute(source.Item1, source.Item2, spc));
        }

        public static void Execute(Compilation compilation, ImmutableArray<ClassDeclarationSyntax> classes, SourceProductionContext context)
        {
            var candidate_classes = GetCandidateClasses(classes, compilation);

            foreach (var class_symbol in candidate_classes.Distinct(SymbolEqualityComparer.Default).Cast<INamedTypeSymbol>())
            {
                GenerateSetParametersAsyncMethod(context, class_symbol);
            }
        }

        private static string GetParameter(ITypeSymbol category, string event_name)
        {
            var args_type = category.Name switch
            {
                "Clipboard" => event_name switch
                {
                    "onbeforecut" => "System.EventArgs",
                    "onbeforecopy" => "System.EventArgs",
                    "onbeforepaste" => "System.EventArgs",
                    _ => "Microsoft.AspNetCore.Components.Web.ClipboardEventArgs",
                },
                "Drag" => "Microsoft.AspNetCore.Components.Web.DragEventArgs",
                "Error" => "Microsoft.AspNetCore.Components.Web.ErrorEventArgs",
                "Focus" => "Microsoft.AspNetCore.Components.Web.FocusEventArgs",
                "Input" => event_name switch
                {
                    "oninvalid" => "System.EventArgs",
                    "onreset" => "System.EventArgs",
                    "onselect" => "System.EventArgs",
                    "onselectionchange" => "System.EventArgs",
                    "onselectstart" => "System.EventArgs",
                    "onsubmit" => "System.EventArgs",
                    _ => "Microsoft.AspNetCore.Components.ChangeEventArgs",
                },
                "Keyboard" => "Microsoft.AspNetCore.Components.Web.KeyboardEventArgs",
                "Mouse" => "Microsoft.AspNetCore.Components.Web.MouseEventArgs",
                "Pointer" => "Microsoft.AspNetCore.Components.Web.PointerEventArgs",
                "Wheel" => "Microsoft.AspNetCore.Components.Web.WheelEventArgs",
                "Progress" => "Microsoft.AspNetCore.Components.Web.ProgressEventArgs",
                "Touch" => "Microsoft.AspNetCore.Components.Web.TouchEventArgs",
                _ => "System.EventArgs"
            };
            var callback = $"[Parameter] public EventCallback<{args_type}> {event_name} {{ get; set; }}";
            var stopPropagation = $"[Parameter] public bool {event_name}StopPropagation {{ get; set; }}";
            var preventDefault = $"[Parameter] public bool {event_name}PreventDefault {{ get; set; }}";
            return $"{callback} {stopPropagation} {preventDefault}";
        }
        private static IEnumerable<(string, string)> GetParameters(ITypeSymbol category, int values)
        {
            if (values == -1)
            {
                // return all
                foreach (var enum_value in category.GetMembers().OfType<IFieldSymbol>())
                {
                    if (enum_value.Name == "all")
                    {
                        continue;
                    }
                    yield return (GetParameter(category, enum_value.Name), enum_value.Name);
                }
            }
            else
            {
                // binary decompose
                foreach (var enum_value in category.GetMembers().OfType<IFieldSymbol>())
                {
                    var constant_value = (int)enum_value.ConstantValue;
                    var name = enum_value.Name;
                    if ((values & constant_value) == constant_value)
                    {
                        yield return (GetParameter(category, name), name);
                    }
                }
            }
        }
        private static void GenerateSetParametersAsyncMethod(SourceProductionContext context, INamedTypeSymbol class_symbol)
        {
            var namespaceName = class_symbol.ContainingNamespace.ToDisplayString();
            var type_kind = class_symbol.TypeKind switch { TypeKind.Class => "class", TypeKind.Interface => "interface", _ => "struct" };
            var type_parameters = string.Join(", ", class_symbol.TypeArguments.Select(t => t.Name));
            type_parameters = string.IsNullOrEmpty(type_parameters) ? type_parameters : "<" + type_parameters + ">";
            var attrs = class_symbol.GetAttributes().Where(a => a.AttributeClass!.Name == "GenerateEvents" || a.AttributeClass.Name == "GenerateEventsAttribute").ToList();
            List<string> all_parameters = new();
            List<string> event_names = new();
            foreach (var event_attribute in attrs)
            {
                var value = event_attribute.ConstructorArguments[0].Value as int?;
                var category = event_attribute.ConstructorArguments[0].Type;
                var results = GetParameters(category, value.Value);
                foreach (var (code, name) in results)
                {
                    all_parameters.Add(code);
                    event_names.Add(name);
                }
            }
            context.AddCode(class_symbol.ToDisplayString() + "_parameters.cs", $@"
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace {namespaceName}
{{
    public partial class {class_symbol.Name}{type_parameters}
    {{
        {string.Join("\n", all_parameters)}
    }}
}}");
            List<string> used_event_names = new();
            List<string> used_event_names_stop = new();
            List<string> used_event_names_prevent = new();
            var buildRenderTreeMethod = class_symbol.GetMembers().FirstOrDefault(m => m.Name == "BuildRenderTree");
            var buildRenderTreeMethodBody = ((buildRenderTreeMethod as IMethodSymbol)!.DeclaringSyntaxReferences[0].GetSyntax() as MethodDeclarationSyntax)!.Body;
            foreach (var statement in buildRenderTreeMethodBody!.Statements.OfType<ExpressionStatementSyntax>()) // TODO recursively!
            {
                if (statement.Expression is InvocationExpressionSyntax ies && ies.Expression is MemberAccessExpressionSyntax maes)
                {
                    if (maes.Name.ToString() == "AddAttribute")
                    {
                        if (ies.ArgumentList.Arguments[1].Expression is LiteralExpressionSyntax les)
                        {
                            used_event_names.Add(les.Token.ValueText);
                        }
                    }
                    else if (maes.Name.ToString() == "AddEventStopPropagationAttribute")
                    {
                        if (ies.ArgumentList.Arguments[2].Expression is IdentifierNameSyntax ins)
                        {
                            used_event_names_stop.Add(ins.Identifier.ValueText.Replace("StopPropagation", ""));
                        }
                    }
                    else if (maes.Name.ToString() == "AddEventPreventDefaultAttribute")
                    {
                        if (ies.ArgumentList.Arguments[2].Expression is IdentifierNameSyntax ins)
                        {
                            used_event_names_prevent.Add(ins.Identifier.ValueText.Replace("PreventDefault", ""));
                        }
                    }
                }
            }
            var unused_event_names = event_names.Except(used_event_names).ToList();
            var unused_event_names_stop = event_names.Except(used_event_names_stop).ToList();
            var unused_event_names_prevent = event_names.Except(used_event_names_prevent).ToList();
            foreach (var name in unused_event_names)
            {
                context.ReportDiagnostic(Diagnostic.Create(EventNotUsed, class_symbol.Locations[0], name));
            }
            foreach (var name in unused_event_names_stop)
            {
                context.ReportDiagnostic(Diagnostic.Create(EventStopPropagationNotUsed, class_symbol.Locations[0], name));
            }
            foreach (var name in unused_event_names_prevent)
            {
                context.ReportDiagnostic(Diagnostic.Create(EventPreventDefaultNotUsed, class_symbol.Locations[0], name));
            }
        }

        /// <summary>
        /// Enumerate methods with at least one Group attribute
        /// </summary>
        /// <param name="receiver"></param>
        /// <param name="compilation"></param>
        /// <returns></returns>
        private static IEnumerable<INamedTypeSymbol> GetCandidateClasses(ImmutableArray<ClassDeclarationSyntax> classes, Compilation compilation)
        {
            var positiveAttributeSymbol = compilation.GetTypeByMetadataName("Excubo.Generators.Blazor.ExperimentalDoNotUseYet.GenerateEventsAttribute");

            // loop over the candidate methods, and keep the ones that are actually annotated
            foreach (var class_declaration in classes)
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
                if (attributes.Any(ad => ad.AttributeClass != null && ad.AttributeClass.Equals(positiveAttributeSymbol, SymbolEqualityComparer.Default)))
                {
                    yield return class_symbol;
                }
            }
        }
    }
}