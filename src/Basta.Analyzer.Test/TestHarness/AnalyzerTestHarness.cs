using System.Collections.Immutable;
using System.Globalization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Basta.Analyzer.Test.TestHarness
{
    internal static class AnalyzerTestHarness
    {
        // System.Private.CoreLib alone does not satisfy the System.Runtime type forwards the compiler expects,
        // without it generic BCL types do not resolve and operation based analyzers see nothing.
        private static readonly ImmutableArray<MetadataReference> References =
        [
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Assert).Assembly.Location),
            MetadataReference.CreateFromFile(System.Reflection.Assembly.Load("System.Runtime").Location)
        ];

        public static async Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync(string source,
                                                                                 DiagnosticAnalyzer analyzer)
        {
            var compilation = CSharpCompilation.Create(assemblyName: "TestProject",
                                                       syntaxTrees: [CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest), path: "Test0.cs")],
                                                       references: References,
                                                       options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            var diagnostics = await compilation.WithAnalyzers([analyzer])
                                               .GetAnalyzerDiagnosticsAsync()
                                               .ConfigureAwait(false);

            return diagnostics;
        }

        public static string Format(ImmutableArray<Diagnostic> diagnostics)
        {
            return diagnostics.IsEmpty
                       ? "<none>"
                       : string.Join(Environment.NewLine, diagnostics.Select(diagnostic => $"{diagnostic.Id} at {diagnostic.Location.GetLineSpan().StartLinePosition}: {diagnostic.GetMessage(CultureInfo.InvariantCulture)}"));
        }
    }
}
