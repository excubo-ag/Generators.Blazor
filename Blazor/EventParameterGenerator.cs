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
    public partial class EventParameterGenerator : ISourceGenerator
    {
        private static readonly DiagnosticDescriptor EventNotUsed = new DiagnosticDescriptor("BB0002", "Event parameter not used", "Parameter names are case insensitive. {0} conflicts with {1}.", "Conflict", DiagnosticSeverity.Error, isEnabledByDefault: true, description: "Parameter names must be case insensitive to be usable in routes. Rename the parameter to not be in conflict with other parameters.");

        private const string SetParametersAsyncAttributeText = @"
using System;
namespace Excubo.Generators.Blazor
{
    public enum HtmlEvent
    {
        Click = 1
    }
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class GenerateEventsAttribute : Attribute
    {
        public GenerateEventsAttribute(HtmlEvent events)
        {
        }
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
            context.AddCode("GenerateEventsAttribute", SetParametersAsyncAttributeText);

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
            var namespaceName = class_symbol.ContainingNamespace.ToDisplayString();
            var type_kind = class_symbol.TypeKind switch { TypeKind.Class => "class", TypeKind.Interface => "interface", _ => "struct" };
            var type_parameters = string.Join(", ", class_symbol.TypeArguments.Select(t => t.Name));
            type_parameters = string.IsNullOrEmpty(type_parameters) ? type_parameters : "<" + type_parameters + ">";
            var attrs = class_symbol.GetAttributes().Where(a => a.AttributeClass.Name == "GenerateEvents" || a.AttributeClass.Name == "GenerateEventsAttribute").ToList();
            var value = attrs[0].ConstructorArguments[0].Value;
            var event_name = "Click";
            context.AddCode(class_symbol.ToDisplayString() + "_parameters.cs", $@"
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace {namespaceName}
{{
    public partial class {class_symbol.Name}{type_parameters}
    {{
        [Parameter] public EventCallback<Microsoft.AspNetCore.Components.Web.MouseEventArgs> On{event_name} {{ get; set; }}
    }}
}}");
            var buildRenderTreeMethod = class_symbol.GetMembers().FirstOrDefault(m => m.Name == "BuildRenderTree");
            var buildRenderTreeMethodBody = ((buildRenderTreeMethod as IMethodSymbol).DeclaringSyntaxReferences[0].GetSyntax() as MethodDeclarationSyntax).Body;
            foreach (var statement in buildRenderTreeMethodBody.Statements.OfType<ExpressionStatementSyntax>())
            {
                if (statement.Expression is InvocationExpressionSyntax ies && ies.Expression is MemberAccessExpressionSyntax maes && maes.Name.ToString() == "AddAttribute")
                {
                    if (ies.ArgumentList.Arguments[1].Expression is LiteralExpressionSyntax les && les.Token.ValueText == "onclick")
                    {

                    }
                }
            }
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
            var positiveAttributeSymbol = compilation.GetTypeByMetadataName("Excubo.Generators.Blazor.GenerateEventsAttribute");

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
                if (attributes.Any(ad => ad.AttributeClass != null && ad.AttributeClass.Equals(positiveAttributeSymbol, SymbolEqualityComparer.Default)))
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