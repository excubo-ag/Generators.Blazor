using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Excubo.Generators.Blazor
{
    [Generator]
    public partial class RequiredParameterAnalyzer : ISourceGenerator
    {
        private static readonly DiagnosticDescriptor MissingRequiredParameter = new DiagnosticDescriptor(
            id: "BB0004",
            title: "Missing parameter assignment",
            messageFormat: "Component {0} requires assignment of parameter {1}",
            category: "Correctness",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "A parameter marked as required may not be omitted when using the component.");
        private static readonly DiagnosticDescriptor ParameterTooMuch = new DiagnosticDescriptor(
            id: "BB0008",
            title: "Unmatched parameter assignment",
            messageFormat: "Component {0} does not have a parameter called {1}",
            category: "Correctness",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "When a component does not define a parameter and also no CaptureUnmatched parameter, then using anything but the parameters will result in failure.");
        private static readonly DiagnosticDescriptor FatalError = new DiagnosticDescriptor(
            id: "BB9999",
            title: "Fatal error",
            messageFormat: "{0} encountered an error while processing {1}. We know from context it has something to do with {2}. Please consider reporting this as an issue.",
            category: "Build",
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: "No software is perfect...");
        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver is not SyntaxReceiver receiver)
            {
                return;
            }

            var compilation = context.Compilation;
            try
            {
                foreach (var method in receiver.CandidateMethods.Where(m => m.Body != null))
                {
                    AnalyzeRenderTreeMethod(context, method.Body!.Statements);
                }
            }
            catch (Exception e)
            {
                context.ReportDiagnostic(Diagnostic.Create(FatalError, null, nameof(RequiredParameterAnalyzer), "a method", e.StackTrace));
            }
            try
            {
                foreach (var lambda in receiver.CandidateLambdas)
                {
                    AnalyzeRenderTreeMethod(context, (lambda.Body as BlockSyntax)!.Statements);
                }
            }
            catch (Exception e)
            {
                context.ReportDiagnostic(Diagnostic.Create(FatalError, null, nameof(RequiredParameterAnalyzer), "a lambda", e.StackTrace));
            }
        }

        private static void AnalyzeRenderTreeMethod(GeneratorExecutionContext context, SyntaxList<StatementSyntax> statements)
        {
            var current_component = new Stack<(SyntaxNode? SyntaxNode, INamedTypeSymbol? Symbol, Dictionary<string, InvocationExpressionSyntax>? AssignedParameters)>();
            AnalyzeStatements(context, statements, ref current_component);
        }

        private static void AnalyzeStatements(GeneratorExecutionContext context, SyntaxList<StatementSyntax> statements, ref Stack<(SyntaxNode? SyntaxNode, INamedTypeSymbol? Symbol, Dictionary<string, InvocationExpressionSyntax>? AssignedParameters)> current_components)
        {
            foreach (var invokation in statements.SelectMany(s => s.RecurseExpressions()).Select(s => s.Expression).OfType<InvocationExpressionSyntax>())
            {
                AnalyzeInvokation(context, invokation, ref current_components);
            }
        }

        private static void AnalyzeInvokation(GeneratorExecutionContext context, InvocationExpressionSyntax invokation, ref Stack<(SyntaxNode? SyntaxNode, INamedTypeSymbol? Symbol, Dictionary<string, InvocationExpressionSyntax>? AssignedParameters)> current_components)
        {
            if (invokation.Expression is MemberAccessExpressionSyntax maes)
            {
                if (maes.Name.ToString().StartsWith("OpenComponent"))
                {
                    if (maes.Name is GenericNameSyntax gns)
                    {
                        var type_arg = gns.TypeArgumentList.Arguments[0];
                        var comp = context.Compilation.GetSemanticModel(type_arg.SyntaxTree).GetSymbolInfo(type_arg);
                        if (comp.Symbol is INamedTypeSymbol comp_symbol)
                        {
                            current_components.Push((invokation, comp_symbol, new Dictionary<string, InvocationExpressionSyntax>()));
                        }
                        else
                        {
                            current_components.Push((null, null, null));
                        }
                    }
                    else
                    {
                        current_components.Push((null, null, null));
                    }
                }
                else if (maes.Name.ToString() == "CloseComponent")
                {
                    try
                    {
                        var (node, component, assigned_parameters) = current_components.Peek();
                        if (component != null)
                        {
                            Func<IPropertySymbol, bool> parameter_condition = component.GetAttributes().Any(a => a.AttributeClass!.Name == "ParametersAreRequiredByDefault" || a.AttributeClass.Name == "ParametersAreRequiredByDefaultAttribute")
                                ? (ps) => true
                                : (ps) => ps.GetAttributes().Any(a => a.AttributeClass!.Name == "Required" || a.AttributeClass.Name == "RequiredAttribute");
                            var bases = component.GetTypeHierarchy().Where(t => !SymbolEqualityComparer.Default.Equals(t, component));
                            var members = component.GetMembers() // members of the type itself
                                .Concat(bases.SelectMany(t => t.GetMembers().Where(m => m.DeclaredAccessibility != Accessibility.Private))) // plus accessible members of any base
                                .Distinct(SymbolEqualityComparer.Default);
                            var component_params = members
                                .OfType<IPropertySymbol>()
                                .Where(ps => ps.GetAttributes().Any(a => a.AttributeClass!.Name == "Parameter" || a.AttributeClass.Name == "ParameterAttribute"))
                                .ToList();
                            var catch_all_parameter = component_params.FirstOrDefault(p =>
                            {
                                var parameter_attr = p.GetAttributes().FirstOrDefault(a => a.AttributeClass!.Name.StartsWith("Parameter"));
                                return parameter_attr != null && parameter_attr.NamedArguments.Any(n => n.Key == "CaptureUnmatchedValues" && n.Value.Value is bool v && v);
                            });
                            var missing_parameters = component_params.Where(p => p != catch_all_parameter).Where(parameter_condition).Select(ps => ps.Name).Except(assigned_parameters.Keys);
                            foreach (var missing_parameter in missing_parameters)
                            {
                                context.ReportDiagnostic(Diagnostic.Create(MissingRequiredParameter, node!.GetLocation(), component.Name, missing_parameter));
                            }
                            if (catch_all_parameter == null)
                            {
                                foreach (var kv in assigned_parameters)
                                {
                                    var name = kv.Key;
                                    var p = kv.Value;
                                    if (!component_params.Select(ps => ps.Name).Contains(name))
                                    {
                                        context.ReportDiagnostic(Diagnostic.Create(ParameterTooMuch, p!.GetLocation(), component.Name, name));
                                    }
                                }
                            }
                        }
                        try
                        {
                            current_components.Pop();
                        }
                        catch (Exception e)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(FatalError, invokation.GetLocation(), nameof(RequiredParameterAnalyzer), invokation.ToString(), "stack peek for CloseComponent"));
                        }
                    }
                    catch (Exception e)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(FatalError, invokation.GetLocation(), nameof(RequiredParameterAnalyzer), invokation.ToString(), "stack pop for CloseComponent"));
                    }
                }
                else if (maes.Name.ToString().StartsWith("OpenElement"))
                {
                    current_components.Push((null, null, null));
                }
                else if (maes.Name.ToString() == "CloseElement")
                {
                    try
                    {
                        current_components.Pop();
                    }
                    catch (Exception e)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(FatalError, invokation.GetLocation(), nameof(RequiredParameterAnalyzer), invokation.ToString(), "stack pop for CloseElement"));
                    }
                }
                else if (maes.Name.ToString() == "AddAttribute")
                {
                    try
                    {
                        var (node, component, assigned_parameters) = current_components.Peek();
                        if (component != null && invokation.ArgumentList.Arguments.Count >= 2)
                        {
                            var name_argument = invokation.ArgumentList.Arguments[1];
                            if (name_argument.Expression is LiteralExpressionSyntax les)
                            {
                                assigned_parameters!.Add(les.Token.ValueText, invokation);
                            }
                            else if (name_argument.Expression is InvocationExpressionSyntax nameof_ies)
                            {
                                var nameof_op = context.Compilation.GetSemanticModel(nameof_ies.SyntaxTree).GetOperation(nameof_ies);
                                var nameof_result = nameof_op!.ConstantValue.Value as string;
                                assigned_parameters!.Add(nameof_result!, invokation);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(FatalError, invokation.GetLocation(), nameof(RequiredParameterAnalyzer), invokation.ToString(), "stack peek for AddAttribute"));
                    }
                }
                else if (invokation.ArgumentList.Arguments.Any(a => a.Expression is IdentifierNameSyntax ins && ins.Identifier.ToString().Contains("builder")))
                {
                    var model = context.Compilation.GetSemanticModel(invokation.SyntaxTree);
                    var called_method = model.GetSymbolInfo(invokation);
                    if (called_method.Symbol != null && called_method.Symbol.Kind is SymbolKind.Method)
                    {
                        if (called_method.Symbol is IMethodSymbol ims)
                        {
                            var syntax_reference = ims.DeclaringSyntaxReferences.FirstOrDefault();
                            if (syntax_reference != null)
                            {
                                if (syntax_reference.GetSyntax() is MethodDeclarationSyntax definition)
                                {
                                    AnalyzeStatements(context, definition!.Body!.Statements, ref current_components);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            // Register a syntax receiver that will be created for each generation pass
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        /// <summary>
        /// Created on demand before each generation pass
        /// </summary>
        internal class SyntaxReceiver : ISyntaxReceiver
        {
            public List<MethodDeclarationSyntax> CandidateMethods { get; } = new List<MethodDeclarationSyntax>();
            public List<ParenthesizedLambdaExpressionSyntax> CandidateLambdas { get; } = new List<ParenthesizedLambdaExpressionSyntax>();

            /// <summary>
            /// Called for every syntax node in the compilation, we can inspect the nodes and save any information useful for generation
            /// </summary>
            public void OnVisitSyntaxNode(SyntaxNode syntax_node)
            {
                // any class with at least one attribute is a candidate for property generation
                if (syntax_node is MethodDeclarationSyntax method && method.Identifier.ToString() == "BuildRenderTree")
                {
                    CandidateMethods.Add(method);
                }
                if (syntax_node is ParenthesizedLambdaExpressionSyntax lambda
                    && lambda.ParameterList.Parameters.Count == 1
                    && lambda.ParameterList.Parameters[0].Identifier.ToString().Contains("builder")
                    && lambda.Body is BlockSyntax)
                {
                    CandidateLambdas.Add(lambda);
                }
            }
        }
    }
}