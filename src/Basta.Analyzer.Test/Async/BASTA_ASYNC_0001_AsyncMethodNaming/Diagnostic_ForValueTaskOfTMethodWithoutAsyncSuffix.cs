using Basta.Analyzer.Test.TestHarness;

namespace Basta.Analyzer.Test.Async.BASTA_ASYNC_0001_AsyncMethodNaming
{
    [TestClass]
    [TestCategory("RoslynAnalyzer")]
    [TestCategory("Async")]
    public sealed class Diagnostic_ForValueTaskOfTMethodWithoutAsyncSuffix
    {
        [TestMethod]
        public async Task Diagnostic_ForValueTaskOfTMethodWithoutAsyncSuffixAsync()
        {
            //language=csharp
            const string source = """
                                  using System.Threading.Tasks;

                                  public sealed class UserService
                                  {
                                      public ValueTask<int> CountUsers()
                                      {
                                          return ValueTask.FromResult(42);
                                      }
                                  }
                                  """;

            var diagnostics = await AnalyzerTestHarness.GetDiagnosticsAsync(source, new BASTA_ASYNC_0001_AsyncMethodNamingAnalyzer()).ConfigureAwait(false);

            Assert.That.HasCount(1,
                                 diagnostics,
                                 because: $"A method returning ValueTask<T> without the Async suffix violates the naming rule{Environment.NewLine}Actual: {AnalyzerTestHarness.Format(diagnostics)}",
                                 fix: "Report exactly one diagnostic in BASTA_ASYNC_0001_AsyncMethodNamingAnalyzer");

            Assert.That.AreEqual(BASTA_ASYNC_0001_AsyncMethodNamingAnalyzer.DiagnosticId,
                                 diagnostics[0].Id,
                                 because: "The diagnostic has to carry the id of the rule",
                                 fix: "Create the diagnostic from the descriptor of BASTA_ASYNC_0001_AsyncMethodNamingAnalyzer");
        }
    }
}
