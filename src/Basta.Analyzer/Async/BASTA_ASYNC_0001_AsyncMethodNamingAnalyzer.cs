// ReSharper disable All
// We are using .NetStandard all optimizations are disabled to keep compatibility with projects using older .Net versions

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Basta.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class BASTA_ASYNC_0001_AsyncMethodNamingAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "BASTA_ASYNC_0001";

        private static readonly DiagnosticDescriptor Rule = new(id: DiagnosticId,
                                                                 title: "Methods returning Task/ValueTask must be named with an 'Async' suffix",
                                                                 messageFormat: "Rename method '{0}' to '{0}Async'; methods returning Task, Task<T>, ValueTask or ValueTask<T> must use the 'Async' suffix",
                                                                 description: "Asynchronous methods (returning Task, Task<T>, ValueTask or ValueTask<T>) must be named with the 'Async' suffix, except for well-known framework entry points ('Handle', 'Main') and test methods.",
                                                                 category: "Async.Design",
                                                                 defaultSeverity: DiagnosticSeverity.Error,
                                                                 isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            Throw.IfNull(context, nameof(context));

            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterCompilationStartAction(OnCompilationStart);
        }

        private static void OnCompilationStart(CompilationStartAnalysisContext context)
        {
            var taskType = context.Compilation.GetTypeByMetadataName("System.Threading.Tasks.Task");

            if (taskType is null)
            {
                // No Task infrastructure referenced by this compilation; nothing for this rule to check.
                return;
            }

            var taskOfTType = context.Compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1");
            var valueTaskType = context.Compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask");
            var valueTaskOfTType = context.Compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask`1");

            context.RegisterSyntaxNodeAction(nodeContext => Analyze(nodeContext, taskType, taskOfTType, valueTaskType, valueTaskOfTType),
                                             SyntaxKind.MethodDeclaration);
        }

        private static void Analyze(SyntaxNodeAnalysisContext context,
                                    INamedTypeSymbol taskType,
                                    INamedTypeSymbol? taskOfTType,
                                    INamedTypeSymbol? valueTaskType,
                                    INamedTypeSymbol? valueTaskOfTType)
        {
            var methodDeclaration = (MethodDeclarationSyntax)context.Node;
            var name = methodDeclaration.Identifier.ValueText;

            if (name.EndsWith("Async", StringComparison.Ordinal) ||
                name is "Handle" or "Main")
            {
                return;
            }

            var methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodDeclaration, context.CancellationToken);

            if (methodSymbol is null)
            {
                return;
            }

            var returnType = methodSymbol.ReturnType.OriginalDefinition;

            var isTaskLike = SymbolEqualityComparer.Default.Equals(returnType, taskType) ||
                             SymbolEqualityComparer.Default.Equals(returnType, taskOfTType) ||
                             SymbolEqualityComparer.Default.Equals(returnType, valueTaskType) ||
                             SymbolEqualityComparer.Default.Equals(returnType, valueTaskOfTType);

            if (!isTaskLike)
            {
                return;
            }

            if (HasTestMethodAttribute(methodDeclaration))
            {
                return;
            }

            var diagnostic = Diagnostic.Create(Rule, methodDeclaration.Identifier.GetLocation(), name);
            context.ReportDiagnostic(diagnostic);
        }

        private static bool HasTestMethodAttribute(MethodDeclarationSyntax methodDeclaration)
        {
            return methodDeclaration.AttributeLists
                                    .SelectMany(list => list.Attributes)
                                    .Any(attribute => attribute.Name.ToString().Contains("TestMethod"));
        }
    }
}
