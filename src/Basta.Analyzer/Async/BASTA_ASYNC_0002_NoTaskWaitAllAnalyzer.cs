// ReSharper disable All
// We are using .NetStandard all optimizations are disabled to keep compatibility with projects using older .Net versions

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Basta.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class BASTA_ASYNC_0002_NoTaskWaitAllAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "BASTA_ASYNC_0002";

        private static readonly DiagnosticDescriptor Rule = new(id: DiagnosticId,
                                                                 title: "Do not call Task.WaitAll",
                                                                 messageFormat: "Do not use the blocking 'Task.WaitAll'; use 'await Task.WhenAll(...)' instead",
                                                                 description: "Task.WaitAll blocks the calling thread and can cause thread-pool starvation or deadlocks. Prefer awaiting Task.WhenAll instead.",
                                                                 category: "Async.Design",
                                                                 defaultSeverity: DiagnosticSeverity.Warning,
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

            context.RegisterOperationAction(operationContext => Analyze(operationContext, taskType), OperationKind.Invocation);
        }

        private static void Analyze(OperationAnalysisContext context,
                                    INamedTypeSymbol taskType)
        {
            var invocation = (IInvocationOperation)context.Operation;
            var method = invocation.TargetMethod;

            if (method.Name != "WaitAll" || !SymbolEqualityComparer.Default.Equals(method.ContainingType, taskType))
            {
                return;
            }

            var diagnostic = Diagnostic.Create(Rule, invocation.Syntax.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }
}
