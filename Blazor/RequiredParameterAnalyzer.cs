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
        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver is not SyntaxReceiver receiver)
            {
                return;
            }

            var compilation = context.Compilation;
            foreach (var method in receiver.CandidateMethods.Where(m => m.Body != null))
            {
                AnalyzeRenderTreeMethod(context, method.Body!.Statements);
            }
            foreach (var lambda in receiver.CandidateLambdas)
            {
                AnalyzeRenderTreeMethod(context, (lambda.Body as BlockSyntax)!.Statements);
            }
        }

        private static void AnalyzeRenderTreeMethod(GeneratorExecutionContext context, SyntaxList<StatementSyntax> statements)
        {
            var current_component = new Stack<(SyntaxNode? SyntaxNode, INamedTypeSymbol? Symbol, HashSet<string>? AssignedParameters)>();
            AnalyzeStatements(context, statements, ref current_component);
        }

        private static void AnalyzeStatements(GeneratorExecutionContext context, SyntaxList<StatementSyntax> statements, ref Stack<(SyntaxNode? SyntaxNode, INamedTypeSymbol? Symbol, HashSet<string>? AssignedParameters)> current_components)
        {
            foreach (var invokation in statements.OfType<ExpressionStatementSyntax>().Select(s => s.Expression).OfType<InvocationExpressionSyntax>())
            {
                AnalyzeInvokation(context, invokation, ref current_components);
            }
        }

        private static void AnalyzeInvokation(GeneratorExecutionContext context, InvocationExpressionSyntax invokation, ref Stack<(SyntaxNode? SyntaxNode, INamedTypeSymbol? Symbol, HashSet<string>? AssignedParameters)> current_components)
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
                            current_components.Push((invokation, comp_symbol, new HashSet<string>()));
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
                    var (node, component, assigned_parameters) = current_components.Peek();
                    if (component != null)
                    {
                        Func<IPropertySymbol, bool> parameter_condition = component.GetAttributes().Any(a => a.AttributeClass!.Name == "ParametersAreRequiredByDefault" || a.AttributeClass.Name == "ParametersAreRequiredByDefaultAttribute")
                            ? (ps) => true
                            : (ps) => ps.GetAttributes().Any(a => a.AttributeClass!.Name == "Required" || a.AttributeClass.Name == "RequiredAttribute");
                        var missing_parameters = component
                            .GetMembers()
                            .OfType<IPropertySymbol>()
                            .Where(ps => ps.GetAttributes().Any(a => a.AttributeClass!.Name == "Parameter" || a.AttributeClass.Name == "ParameterAttribute"))
                            .Where(parameter_condition)
                            .Select(ps => ps.Name)
                            .Except(assigned_parameters)
                            .ToList();

                        foreach (var missing_parameter in missing_parameters)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(MissingRequiredParameter, node!.GetLocation(), component.Name, missing_parameter));
                        }
                    }
                    current_components.Pop();
                }
                else if (maes.Name.ToString().StartsWith("OpenElement"))
                {
                    current_components.Push((null, null, null));
                }
                else if (maes.Name.ToString() == "CloseElement")
                {
                    current_components.Pop();
                }
                else if (maes.Name.ToString() == "AddAttribute")
                {
                    var (node, component, assigned_parameters) = current_components.Peek();
                    if (component != null)
                    {
                        var name_argument = invokation.ArgumentList.Arguments[1];
                        if (name_argument.Expression is LiteralExpressionSyntax les)
                        {
                            assigned_parameters!.Add(les.Token.ValueText);
                        }
                        else if (name_argument.Expression is InvocationExpressionSyntax nameof_ies)
                        {
                            var nameof_op = context.Compilation.GetSemanticModel(nameof_ies.SyntaxTree).GetOperation(nameof_ies);
                            var nameof_result = nameof_op!.ConstantValue.Value as string;
                            assigned_parameters!.Add(nameof_result!);
                        }
                    }
                }
                else if (invokation.ArgumentList.Arguments.Any(a => a.Expression is IdentifierNameSyntax ins && ins.Identifier.ToString().Contains("builder")))
                {
                    var model = context.Compilation.GetSemanticModel(invokation.SyntaxTree);
                    var called_method = model.GetSymbolInfo(invokation);
                    if (called_method.Symbol != null && called_method.Symbol.Kind is SymbolKind.Method)
                    {
                        var definition = (called_method.Symbol as IMethodSymbol)!.DeclaringSyntaxReferences[0].GetSyntax() as MethodDeclarationSyntax;
                        AnalyzeStatements(context, definition!.Body!.Statements, ref current_components);
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