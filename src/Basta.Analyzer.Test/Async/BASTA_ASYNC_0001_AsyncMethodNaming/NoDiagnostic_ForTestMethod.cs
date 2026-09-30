using Basta.Analyzer.Test.TestHarness;

namespace Basta.Analyzer.Test.Async.BASTA_ASYNC_0001_AsyncMethodNaming
{
    [TestClass]
    [TestCategory("RoslynAnalyzer")]
    [TestCategory("Async")]
    public sealed class NoDiagnostic_ForTestMethod
    {
        [TestMethod]
        public async Task NoDiagnostic_ForTestMethodAsync()
        {
            //language=csharp
            const string source = """
                                  using System.Threading.Tasks;
                                  using Microsoft.VisualStudio.TestTools.UnitTesting;

                                  [TestClass]
                                  public sealed class UserServiceTest
                                  {
                                      [TestMethod]
                                      public Task Save_User_Works()
                                      {
                                          return Task.CompletedTask;
                                      }
                                  }
                                  """;

            var diagnostics = await AnalyzerTestHarness.GetDiagnosticsAsync(source, new BASTA_ASYNC_0001_AsyncMethodNamingAnalyzer()).ConfigureAwait(false);

            Assert.That.IsEmpty(diagnostics,
                                because: $"Test methods are named after the scenario, not after the async pattern{Environment.NewLine}Actual: {AnalyzerTestHarness.Format(diagnostics)}",
                                fix: "Do not report a diagnostic for this case in BASTA_ASYNC_0001_AsyncMethodNamingAnalyzer");
        }
    }
}
