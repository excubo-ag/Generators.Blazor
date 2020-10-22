using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace Excubo.Generators.Blazor
{
    [Generator]
    public partial class KeyAnalyzer : ISourceGenerator
    {
        private static readonly DiagnosticDescriptor KeylessForeach = new DiagnosticDescriptor("BB0003", "foreach without key", "A key must be used when rendering loops in Blazor", "Correctness", DiagnosticSeverity.Warning, isEnabledByDefault: true, description: "Not using a @key within a for-loop or foreach-loop in Blazor not only can have a negative performance impact, but also cause problems with disposable components.");

        public void Execute(GeneratorExecutionContext context)
        {
            if (!(context.SyntaxReceiver is SyntaxReceiver receiver))
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
                    if (for_body is BlockSyntax for_block)
                    {
                        int level = -1;
                        bool saw_key = false;
                        // TODO find top-level OpenElement/OpenComponent, ignore any non-top-level OpenElement/OpenComponent and make sure there's a key on all of them.
                        foreach (var invokation in for_block.Statements.OfType<ExpressionStatementSyntax>().Select(s => s.Expression).OfType<InvocationExpressionSyntax>())
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
                                    saw_key = true;
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