using Basta.Analyzer.Test.TestHarness;

namespace Basta.Analyzer.Test.Async.BASTA_ASYNC_0002_NoTaskWaitAll
{
    [TestClass]
    [TestCategory("RoslynAnalyzer")]
    [TestCategory("Async")]
    public sealed class Diagnostic_ForTaskWaitAllCall
    {
        [TestMethod]
        public async Task Diagnostic_ForTaskWaitAllCallAsync()
        {
            //language=csharp
            const string source = """
                                  using System.Threading.Tasks;

                                  public sealed class Worker
                                  {
                                      public void RunAll(Task first, Task second)
                                      {
                                          Task.WaitAll(first, second);
                                      }
                                  }
                                  """;

            var diagnostics = await AnalyzerTestHarness.GetDiagnosticsAsync(source, new BASTA_ASYNC_0002_NoTaskWaitAllAnalyzer()).ConfigureAwait(false);

            Assert.That.HasCount(1,
                                 diagnostics,
                                 because: $"Task.WaitAll blocks the calling thread{Environment.NewLine}Actual: {AnalyzerTestHarness.Format(diagnostics)}",
                                 fix: "Report exactly one diagnostic in BASTA_ASYNC_0002_NoTaskWaitAllAnalyzer");

            Assert.That.AreEqual(BASTA_ASYNC_0002_NoTaskWaitAllAnalyzer.DiagnosticId,
                                 diagnostics[0].Id,
                                 because: "The diagnostic has to carry the id of the rule",
                                 fix: "Create the diagnostic from the descriptor of BASTA_ASYNC_0002_NoTaskWaitAllAnalyzer");
        }
    }
}
