using Basta.Analyzer.Test.TestHarness;

namespace Basta.Analyzer.Test.Async.BASTA_ASYNC_0001_AsyncMethodNaming
{
    [TestClass]
    [TestCategory("RoslynAnalyzer")]
    [TestCategory("Async")]
    public sealed class NoDiagnostic_ForTaskMethodWithAsyncSuffix
    {
        [TestMethod]
        public async Task NoDiagnostic_ForTaskMethodWithAsyncSuffixAsync()
        {
            //language=csharp
            const string source = """
                                  using System.Threading.Tasks;

                                  public sealed class UserService
                                  {
                                      public async Task<int> CountUsersAsync()
                                      {
                                          await Task.Yield();

                                          return 42;
                                      }
                                  }
                                  """;

            var diagnostics = await AnalyzerTestHarness.GetDiagnosticsAsync(source, new BASTA_ASYNC_0001_AsyncMethodNamingAnalyzer()).ConfigureAwait(false);

            Assert.That.IsEmpty(diagnostics,
                                because: $"A method returning Task with the Async suffix follows the naming rule{Environment.NewLine}Actual: {AnalyzerTestHarness.Format(diagnostics)}",
                                fix: "Do not report a diagnostic for this case in BASTA_ASYNC_0001_AsyncMethodNamingAnalyzer");
        }
    }
}
