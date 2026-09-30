using Basta.Analyzer.Test.TestHarness;

namespace Basta.Analyzer.Test.Async.BASTA_ASYNC_0002_NoTaskWaitAll
{
    [TestClass]
    [TestCategory("RoslynAnalyzer")]
    [TestCategory("Async")]
    public sealed class NoDiagnostic_ForTaskWhenAllCall
    {
        [TestMethod]
        public async Task NoDiagnostic_ForTaskWhenAllCallAsync()
        {
            //language=csharp
            const string source = """
                                  using System.Threading.Tasks;

                                  public sealed class Worker
                                  {
                                      public async Task RunAllAsync(Task first, Task second)
                                      {
                                          await Task.WhenAll(first, second);
                                      }
                                  }
                                  """;

            var diagnostics = await AnalyzerTestHarness.GetDiagnosticsAsync(source, new BASTA_ASYNC_0002_NoTaskWaitAllAnalyzer()).ConfigureAwait(false);

            Assert.That.IsEmpty(diagnostics,
                                because: $"Awaiting Task.WhenAll is the non-blocking alternative{Environment.NewLine}Actual: {AnalyzerTestHarness.Format(diagnostics)}",
                                fix: "Do not report a diagnostic for this case in BASTA_ASYNC_0002_NoTaskWaitAllAnalyzer");
        }
    }
}
