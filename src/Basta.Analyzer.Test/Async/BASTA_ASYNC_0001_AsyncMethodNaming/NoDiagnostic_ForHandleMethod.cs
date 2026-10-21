using Basta.Analyzer.Test.TestHarness;

namespace Basta.Analyzer.Test.Async.BASTA_ASYNC_0001_AsyncMethodNaming
{
    [TestClass]
    [TestCategory("RoslynAnalyzer")]
    [TestCategory("Async")]
    public sealed class NoDiagnostic_ForHandleMethod
    {
        [TestMethod]
        public async Task NoDiagnostic_ForHandleMethodAsync()
        {
            //language=csharp
            const string source = """
                                  using System.Threading.Tasks;

                                  public sealed class CreateUserHandler
                                  {
                                      public Task Handle()
                                      {
                                          return Task.CompletedTask;
                                      }
                                  }
                                  """;

            var diagnostics = await AnalyzerTestHarness.GetDiagnosticsAsync(source, new BASTA_ASYNC_0001_AsyncMethodNamingAnalyzer()).ConfigureAwait(false);

            Assert.That.IsEmpty(diagnostics,
                                because: $"'Handle' is a well-known framework entry point and keeps its name{Environment.NewLine}Actual: {AnalyzerTestHarness.Format(diagnostics)}",
                                fix: "Do not report a diagnostic for this case in BASTA_ASYNC_0001_AsyncMethodNamingAnalyzer");
        }
    }
}
