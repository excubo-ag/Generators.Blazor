using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Excubo.Generators.Blazor
{
    internal static class StatementSyntaxExtension
    {
        internal static IEnumerable<ExpressionStatementSyntax> RecurseExpressions(this StatementSyntax syntax)
        {
            if (syntax is ExpressionStatementSyntax ess)
            {
                yield return ess;
            }
            else
            {
                foreach (var child in syntax.ChildNodes().OfType<StatementSyntax>())
                {
                    foreach (var result in RecurseExpressions(child))
                    {
                        yield return result;
                    }
                }
            }
        }
    }
    [Generator]
    public partial class KeyAnalyzer : IIncrementalGenerator
    {
        private static readonly DiagnosticDescriptor KeylessForeach = new DiagnosticDescriptor(
            id: "BB0003",
            title: "foreach without key",
            messageFormat: "A key must be used when rendering loops in Blazor",
            category: "Correctness",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Not using a @key within a for-loop or foreach-loop in Blazor not only can have a negative performance impact, but also cause problems with disposable components.");
        private static readonly DiagnosticDescriptor ConstantKey = new DiagnosticDescriptor(
            id: "BB0005",
            title: "key should not be a constant",
            messageFormat: "A key must be unique per loop iteration. Did you mean @{0} instead of {0}?",
            category: "Correctness",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "A key must be unique per loop iteration. Therefore, it cannot be a constant expression.");

        public static void Execute(Compilation compilation, ImmutableArray<MethodDeclarationSyntax> methods, SourceProductionContext context)
        {
            foreach (var method in methods.Where(m => m.Body != null))
            {
                AnalyzeRenderTreeMethod(compilation, context, method.Body!.Statements);
            }
        }
        public static void Execute(Compilation compilation, ImmutableArray<ParenthesizedLambdaExpressionSyntax> lambdas, SourceProductionContext context)
        {
            foreach (var lambda in lambdas)
            {
                AnalyzeRenderTreeMethod(compilation, context, (lambda.Body as BlockSyntax)!.Statements);
            }
        }

        private static void AnalyzeRenderTreeMethod(Compilation compilation, SourceProductionContext context, SyntaxList<StatementSyntax> statements)
        {
            foreach (var statement in statements)
            {
                if (statement.IsKind(SyntaxKind.ForEachStatement) || statement.IsKind(SyntaxKind.ForStatement))
                {
                    var for_keyword = (statement as ForEachStatementSyntax)?.ForEachKeyword ?? (statement as ForStatementSyntax)?.ForKeyword;
                    // TODO analyze for-body and see if there are any builder*.OpenComponent / builder*.OpenElement and no builder*.SetKey()
                    var for_body = (statement as ForEachStatementSyntax)?.Statement ?? (statement as ForStatementSyntax)?.Statement;
                    if (for_body is not BlockSyntax for_block)
                    {
                        continue;
                    }
                    var level = -1;
                    var saw_key = false;
                    AnalyzeStatements(compilation, context, for_keyword, for_block.Statements, ref level, ref saw_key);
                }
            }
        }

        private static void AnalyzeStatements(Compilation compilation, SourceProductionContext context, SyntaxToken? for_keyword, SyntaxList<StatementSyntax> statements, ref int level, ref bool saw_key)
        {
            foreach (var invokation in statements.SelectMany(s => s.RecurseExpressions()).Select(s => s.Expression).OfType<InvocationExpressionSyntax>())
            {
                AnalyzeInvokation(compilation, context, for_keyword, ref level, ref saw_key, invokation);
            }
        }

        private static void AnalyzeInvokation(Compilation compilation, SourceProductionContext context, SyntaxToken? for_keyword, ref int level, ref bool saw_key, InvocationExpressionSyntax invokation)
        {
            if (invokation.Expression is MemberAccessExpressionSyntax maes)
            {
                if (maes.Name.ToString() == "OpenElement" || maes.Name.ToString().StartsWith("OpenComponent"))
                {
                    ++level;
                    if (level == 0)
                    {
                        // this is a top level element, we therefore have to reset whether we saw a key yet. All top-level elements/components within a loop need a key
                        saw_key = false;
                    }
                }
                else if (maes.Name.ToString() == "CloseElement" || maes.Name.ToString() == "CloseComponent")
                {
                    if (level == 0 && !saw_key)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(KeylessForeach, for_keyword!.Value.GetLocation()));
                    }
                    --level;
                }
                else if (level == 0 && maes.Name.ToString() == "SetKey")
                {
                    if (invokation.ArgumentList.Arguments.Any() && invokation.ArgumentList.Arguments[0].Expression is LiteralExpressionSyntax les)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(ConstantKey, invokation.ArgumentList.Arguments[0].GetLocation(), les.Token.ValueText));
                    }
                    saw_key = true;
                }
                else if (invokation.ArgumentList.Arguments.Any(a => a.Expression is IdentifierNameSyntax ins && ins.Identifier.ToString().Contains("builder")))
                {
                    var model = compilation.GetSemanticModel(invokation.SyntaxTree);
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
                                    AnalyzeStatements(compilation, context, for_keyword, definition!.Body!.Statements, ref level, ref saw_key);
                                }
                            }
                        }
                    }
                }
            }
        }
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Register a syntax receiver that will be created for each generation pass
            IncrementalValuesProvider<MethodDeclarationSyntax> methods = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: static (syntax_node, _) => syntax_node is MethodDeclarationSyntax method && method.Identifier.ToString() == "BuildRenderTree",
                transform: static (context, _) => context.Node as MethodDeclarationSyntax)
                .Where(static m => m is not null)!;
            // Register a syntax receiver that will be created for each generation pass
            IncrementalValuesProvider<ParenthesizedLambdaExpressionSyntax> lambdas = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: static (syntax_node, _) => syntax_node is ParenthesizedLambdaExpressionSyntax lambda
                    && lambda.ParameterList.Parameters.Count == 1
                    && lambda.ParameterList.Parameters[0].Identifier.ToString().Contains("builder")
                    && lambda.Body is BlockSyntax,
                transform: static (context, _) => context.Node as ParenthesizedLambdaExpressionSyntax)
                .Where(static m => m is not null)!;


            IncrementalValueProvider<(Compilation, ImmutableArray<MethodDeclarationSyntax>)> compilationAndMethods = context.CompilationProvider.Combine(methods.Collect());
            context.RegisterSourceOutput(compilationAndMethods, static (spc, source) => Execute(source.Item1, source.Item2, spc));
            IncrementalValueProvider<(Compilation, ImmutableArray<ParenthesizedLambdaExpressionSyntax>)> compilationAndLambdas = context.CompilationProvider.Combine(lambdas.Collect());
            context.RegisterSourceOutput(compilationAndLambdas, static (spc, source) => Execute(source.Item1, source.Item2, spc));
        }
    }
}