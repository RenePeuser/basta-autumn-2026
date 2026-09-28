using AspNetCore.Simple.MsTest.Sdk;

namespace CodeRules.HowTo
{
    [TestClass]
    [TestCategory("HowTo")]
    public class Parse_Solution_With_MSBuild_Test
    {
        private static readonly string SolutionPath = RepositoryPaths.Solution;

        [TestMethod]
        public void How_To_Parse_A_Solution_File()
        {
            var solutionFile = Microsoft.Build.Construction.SolutionFile.Parse(SolutionPath);

            Assert.That.IsNotNull(solutionFile,
                                  because: "SolutionFile.Parse must return a valid solution object",
                                  fix: "Check if the solution file exists and is valid");

            Assert.That.IsNotNull(solutionFile.ProjectsInOrder,
                                  because: "Solution must have a ProjectsInOrder collection",
                                  fix: "Ensure solution file is properly formatted");
        }
    }
}
