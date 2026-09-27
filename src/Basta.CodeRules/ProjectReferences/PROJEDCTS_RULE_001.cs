using System.Collections.Immutable;

namespace Basta.CodeRules.ProjectReferences
{
    [TestCategory("CodeRules")]
    [TestCategory("CodeRules Projects")]
    [TestClass]
    public class PROJEDCTS_RULE_001 : CodeRuleTestBase
    {
        [TestMethod]
        public void Productive_Projects_Are_Not_Allowed_To_Reference_Any_Test_Project()
        {
            var testProjectPaths = Solution.UnitTestProjects
                                           .Select(testProject => testProject.ProjectFileInfo.Value.FullName)
                                           .ToImmutableHashSet(StringComparer.OrdinalIgnoreCase);

            var findings = from project in Solution.ProductiveProjects
                           let projectFile = project.ProjectFileInfo.Value
                           from reference in project.ProjectReferences

                           // Include is the raw relative path from the .csproj, usually with '\' separators
                           let include = reference.Include.Replace('\\', Path.DirectorySeparatorChar)
                           let referencedPath = Path.GetFullPath(Path.Combine(projectFile.DirectoryName!, include))
                           where testProjectPaths.Contains(referencedPath)
                           select new CodeRuleFinding(Subject: $"{project.AssemblyName} → {Path.GetFileNameWithoutExtension(include)}",
                                                      Location: projectFile,
                                                      Current: $"<ProjectReference Include=\"{reference.Include}\" />",
                                                      Fix: $"Remove the <ProjectReference> from '{projectFile.Name}'. If the productive code needs something from the test project, move that code into a productive project instead.");

            Assert.That.CodeRuleHasNoFindings(findings,
                                              rule: "PROJEDCTS_RULE_001",
                                              title: "Productive projects must not reference test projects",
                                              because: "Test projects are never deployed. A productive project that depends on them ships test code and test packages " +
                                                       "(MSTest, test SDKs, fakes) into production and creates a dependency cycle between production and test code.",
                                              fix: "Remove the <ProjectReference> to the test project.");
        }
    }
}
