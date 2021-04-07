using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Excubo.Generators.Blazor
{
    [Generator]
    public partial class LambdaEventCallbackAnalyzer : ISourceGenerator
    {
        private static readonly DiagnosticDescriptor LambdaUsedParameter = new DiagnosticDescriptor(
            id: "BB0009",
            title: "Use of lambda within attribute",
            messageFormat: "Consider replacing the lambda callback in {0} by a method",
            category: "Correctness",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Lambdas may cause unnecessary re-rendering (see https://github.com/dotnet/aspnetcore/issues/18919).");
        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver is not SyntaxReceiver receiver)
            {
                return;
            }
            try
            {
                foreach (var invocation in receiver.CandidateInvocations)
                {
                    AnalyzeInvokation(context, invocation);
                }
            }
            catch (Exception e)
            {
                context.ReportDiagnostic(Diagnostic.Create(RequiredParameterAnalyzer.FatalError, null, nameof(LambdaEventCallbackAnalyzer), "an AddAttribute invocation", e.StackTrace));
            }
        }

        private static void AnalyzeInvokation(GeneratorExecutionContext context, InvocationExpressionSyntax invokation)
        {
            if (invokation.Expression is MemberAccessExpressionSyntax maes)
            {
                // We know a few things so far:
                // 1. this is a call to AddAttribute
                // 2. there are three arguments
                // 
                // So this might be an AddAttribute(int, string, EventCallback)
                // We're only interested in the last argument, because lambdas in there will be problematic
                var value_argument = invokation.ArgumentList.Arguments[2];
                var lambda = value_argument.DescendantNodes().FirstOrDefault(n => n.IsKind(SyntaxKind.ParenthesizedLambdaExpression) || n.IsKind(SyntaxKind.SimpleLambdaExpression));
                if (lambda != null)
                {
                    var symbol = context.Compilation.GetSemanticModel(lambda.SyntaxTree).GetSymbolInfo(lambda);
                    var rf = context.Compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Components.RenderFragment");
                    var rtb = context.Compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder");
                    if (symbol.Symbol is IMethodSymbol ms && ms.Parameters.Length == 1 && (SymbolEqualityComparer.Default.Equals(ms.ReturnType, rf) || SymbolEqualityComparer.Default.Equals(ms.Parameters[0].Type, rtb)))
                    {
                        // this is to exclude lambdas that are just RenderFragments (e.g. ChildContent) or RenderFragment<T> (i.e. templates)
                        return;
                    }
                    context.ReportDiagnostic(Diagnostic.Create(LambdaUsedParameter, lambda.GetLocation(), invokation.ArgumentList.Arguments[1].ToString()));
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
            public List<InvocationExpressionSyntax> CandidateInvocations { get; } = new List<InvocationExpressionSyntax>();

            /// <summary>
            /// Called for every syntax node in the compilation, we can inspect the nodes and save any information useful for generation
            /// </summary>
            public void OnVisitSyntaxNode(SyntaxNode syntax_node)
            {
                if (syntax_node is InvocationExpressionSyntax invocation &&
                    invocation.ArgumentList.Arguments.Count == 3 &&
                    invocation.Expression is MemberAccessExpressionSyntax maes &&
                    maes.Name.ToString() == "AddAttribute")
                {
                    CandidateInvocations.Add(invocation);
                }
            }
        }
    }
}