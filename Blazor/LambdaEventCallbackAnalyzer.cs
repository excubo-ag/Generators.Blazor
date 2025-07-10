﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Excubo.Generators.Blazor
{
    // This analyzer is waay too chatty, as every @bind will be reported. This isn't a good idea as of right now, so disabled until this is better understood.
    //[Generator]
    public partial class LambdaEventCallbackAnalyzer : IIncrementalGenerator
    {
        private static readonly DiagnosticDescriptor LambdaUsedParameter = new DiagnosticDescriptor(
            id: "BB0009",
            title: "Use of lambda within attribute",
            messageFormat: "Consider replacing the lambda callback in {0} by a method",
            category: "Correctness",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Lambdas may cause unnecessary re-rendering (see https://github.com/dotnet/aspnetcore/issues/18919).");
        public static void Execute(Compilation compilation, ImmutableArray<InvocationExpressionSyntax> invocations, SourceProductionContext context)
        {
            var rf = compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Components.RenderFragment")!;
            var ec = compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Components.EventCallback")!;
            var ect = compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Components.EventCallback`1")!;
            var rtb = compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder")!;
            try
            {
                foreach (var invocation in invocations)
                {
                    AnalyzeInvokation(compilation, context, invocation, rf, rtb, ec, ect);
                }
            }
            catch (Exception e)
            {
                context.ReportDiagnostic(Diagnostic.Create(RequiredParameterAnalyzer.FatalError, null, nameof(LambdaEventCallbackAnalyzer), "an AddAttribute invocation", e.StackTrace));
            }
        }

        private static void AnalyzeInvokation(Compilation compilation, SourceProductionContext context, InvocationExpressionSyntax invokation, INamedTypeSymbol rf, INamedTypeSymbol rtb, INamedTypeSymbol ec, INamedTypeSymbol ect)
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
                var sm = compilation.GetSemanticModel(invokation.SyntaxTree);
                if (!(sm.GetSymbolInfo(invokation).Symbol is IMethodSymbol invocation_ms))
                {
                    return;
                }
                var value_type = invocation_ms.Parameters[2].Type;
                if (SymbolEqualityComparer.Default.Equals(value_type, ec)
                 || SymbolEqualityComparer.Default.Equals(value_type.OriginalDefinition, ect))
                {
                    var lambda = value_argument.DescendantNodes().FirstOrDefault(n => n.IsKind(SyntaxKind.ParenthesizedLambdaExpression) || n.IsKind(SyntaxKind.SimpleLambdaExpression));
                    if (lambda != null)
                    {
                        var dataFlow = sm.AnalyzeDataFlow(lambda);
                        var capturedVariables = dataFlow.Captured;
                        if (!capturedVariables.Any())
                        {
                            return;
                        }
                        var symbol = sm.GetSymbolInfo(lambda);
                        if (symbol.Symbol is IMethodSymbol ms && ms.Parameters.Length == 1 && (SymbolEqualityComparer.Default.Equals(ms.ReturnType, rf) || SymbolEqualityComparer.Default.Equals(ms.Parameters[0].Type, rtb)))
                        {
                            // this is to exclude lambdas that are just RenderFragments (e.g. ChildContent) or RenderFragment<T> (i.e. templates)
                            return;
                        }
                        context.ReportDiagnostic(Diagnostic.Create(LambdaUsedParameter, lambda.GetLocation(), invokation.ArgumentList.Arguments[1].ToString()));
                    }
                }
            }
        }
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Register a syntax receiver that will be created for each generation pass
            IncrementalValuesProvider<InvocationExpressionSyntax> invocations = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: static (syntax_node, _) => syntax_node is InvocationExpressionSyntax invocation &&
                    invocation.ArgumentList.Arguments.Count == 3 &&
                    invocation.Expression is MemberAccessExpressionSyntax maes &&
                    maes.Name.ToString() == "AddAttribute",
                transform: static (context, _) => context.Node as InvocationExpressionSyntax)
                .Where(static m => m is not null)!;
            IncrementalValueProvider<(Compilation, ImmutableArray<InvocationExpressionSyntax>)> compilationAndInvocations = context.CompilationProvider.Combine(invocations.Collect());
            context.RegisterSourceOutput(compilationAndInvocations, static (spc, source) => Execute(source.Item1, source.Item2, spc));
        }
    }
}