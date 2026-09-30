using Basta.Analyzer.Test.TestHarness;

namespace Basta.Analyzer.Test.Async.BASTA_ASYNC_0001_AsyncMethodNaming
{
    [TestClass]
    [TestCategory("RoslynAnalyzer")]
    [TestCategory("Async")]
    public sealed class NoDiagnostic_ForSynchronousMethod
    {
        [TestMethod]
        public async Task NoDiagnostic_ForSynchronousMethodAsync()
        {
            //language=csharp
            const string source = """
                                  public sealed class UserService
                                  {
                                      public int CountUsers()
                                      {
                                          return 42;
                                      }
                                  }
                                  """;

            var diagnostics = await AnalyzerTestHarness.GetDiagnosticsAsync(source, new BASTA_ASYNC_0001_AsyncMethodNamingAnalyzer()).ConfigureAwait(false);

            Assert.That.IsEmpty(diagnostics,
                                because: $"A method that does not return a Task-like type is not async{Environment.NewLine}Actual: {AnalyzerTestHarness.Format(diagnostics)}",
                                fix: "Do not report a diagnostic for this case in BASTA_ASYNC_0001_AsyncMethodNamingAnalyzer");
        }
    }
}
