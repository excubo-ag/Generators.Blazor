using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
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
    public partial class KeyAnalyzer : ISourceGenerator
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
                    AnalyzeStatements(context, for_keyword, for_block.Statements, ref level, ref saw_key);
                }
            }
        }

        private static void AnalyzeStatements(GeneratorExecutionContext context, SyntaxToken? for_keyword, SyntaxList<StatementSyntax> statements, ref int level, ref bool saw_key)
        {
            foreach (var invokation in statements.SelectMany(s => s.RecurseExpressions()).Select(s => s.Expression).OfType<InvocationExpressionSyntax>())
            {
                AnalyzeInvokation(context, for_keyword, ref level, ref saw_key, invokation);
            }
        }

        private static void AnalyzeInvokation(GeneratorExecutionContext context, SyntaxToken? for_keyword, ref int level, ref bool saw_key, InvocationExpressionSyntax invokation)
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
                                    AnalyzeStatements(context, for_keyword, definition!.Body!.Statements, ref level, ref saw_key);
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