using System.Collections.Immutable;

namespace Basta.CodeRules
{
    [TestCategory("CodeRules")]
    [TestCategory("CodeRules Projects")]
    [TestClass]
    public class PROJECTS_RULE_001 : CodeRuleTestBase
    {
        [TestMethod]
        public void Productive_Projects_Are_Not_Allowed_To_Reference_Any_Test_Project()
        {
            // ProjectReference.Name is the referenced .csproj file name without extension
            var testProjectNames = Solution.UnitTestProjects
                                           .Select(testProject => Path.GetFileNameWithoutExtension(testProject.ProjectFileInfo.Value.Name))
                                           .ToImmutableHashSet(StringComparer.OrdinalIgnoreCase);

            var findings = from project in Solution.ProductiveProjects
                           from reference in project.ProjectReferences
                           where testProjectNames.Contains(reference.Name)
                           let projectFile = project.ProjectFileInfo.Value
                           select new CodeRuleFinding
                           {
                               Subject = $"{project.AssemblyName} → {reference.Name}",
                               Location = projectFile,
                               Current = $"<ProjectReference Include=\"{reference.Include}\" />",
                               Fix = $"Remove the <ProjectReference> from '{projectFile.Name}'. If the productive code needs something from the test project, move that code into a productive project instead."
                           };

            Assert.That.CodeRuleHasNoFindings(findings,
                                              rule: "PROJECTS_RULE_001",
                                              title: "Productive projects must not reference test projects",
                                              because: "Test projects are never deployed. A productive project that depends on them ships test code and test packages " +
                                                       "(MSTest, test SDKs, fakes) into production and creates a dependency cycle between production and test code.",
                                              fix: "Remove the <ProjectReference> to the test project.");
        }
    }
}
