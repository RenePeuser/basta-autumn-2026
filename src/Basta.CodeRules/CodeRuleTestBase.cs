using System.Collections.Immutable;
using Extensions.Pack;
using Solution.Parser.CSharp;
using Solution.Parser.Sln;
using CSharpSyntaxTree = Solution.Parser.CSharp.CSharpSyntaxTree;
using Enum = Solution.Parser.CSharp.Enum;

// [assembly: Parallelize(Scope = ExecutionScope.ClassLevel)]
[assembly: DoNotParallelize]

namespace Basta.CodeRules
{
    public record SqlScript(FileInfo FileInfo,
                            string Content);

    public record JsonFile(FileInfo FileInfo,
                           string Content);

    [TestClass]
    public abstract class CodeRuleTestBase
    {
        // Blacklist not to check for code analysis
        // Remove version path which you lile to analyze with the ne code rules :)
        private static readonly string[] IgnoreForCodeRuleAnalyse = [];

        public static ImmutableList<CSharpSyntaxTree> SyntaxTreesToAnalyze { get; set; } = ImmutableList<CSharpSyntaxTree>.Empty;

        public static ImmutableList<CSharpSyntaxTree> ProductiveSyntaxTrees { get; set; } = ImmutableList<CSharpSyntaxTree>.Empty;

        public static ImmutableList<CSharpSyntaxTree> TestSyntaxTrees { get; set; } = ImmutableList<CSharpSyntaxTree>.Empty;

        protected static ImmutableList<CSharpSyntaxTree> SyntaxTreesToAnalyzeFromTests { get; private set; } = ImmutableList<CSharpSyntaxTree>.Empty;

        public static SolutionFile Solution { get; private set; } = null!;

        public static ImmutableList<CSharpSyntaxTree> AllSyntaxTrees { get; set; } = ImmutableList<CSharpSyntaxTree>.Empty;

        public static ImmutableList<JsonFile> RequestsJsonFiles { get; private set; } = ImmutableList<JsonFile>.Empty;

        public static ImmutableList<JsonFile> ResponseJsonFiles { get; private set; } = ImmutableList<JsonFile>.Empty;

        public static ImmutableList<Record> Records { get; private set; } = ImmutableList<Record>.Empty;

        public static ImmutableList<Class> Classes { get; private set; } = ImmutableList<Class>.Empty;

        protected static ImmutableList<Interface> Interfaces { get; private set; } = ImmutableList<Interface>.Empty;

        public static ImmutableList<Enum> Enums { get; private set; } = ImmutableList<Enum>.Empty;

        protected static ImmutableList<Struct> Structs { get; private set; } = ImmutableList<Struct>.Empty;

        [AssemblyInitialize]
        public static void Init(TestContext testContext)
        {
            var solutionFile = new SolutionFileName("code-rules.slnx").FindSolutionFileReverseFrom(new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory));
            Solution = solutionFile.Parse();

            ProductiveSyntaxTrees = Solution.ProductiveProjects
                                            .SelectMany(p => p.SourceFiles.GetCSharpFiles())
                                            .Select(c => c.Parse())
                                            .Where(tree => !IgnoreForCodeRuleAnalyse.Any(namespaceToIgnore => tree.NameSpace.Name.Contains(namespaceToIgnore)))
                                            .ToImmutableList();

            TestSyntaxTrees = Solution.UnitTestProjects
                                      .SelectMany(p => p.SourceFiles.GetCSharpFiles())
                                      .Select(c => c.Parse())
                                      .Where(tree => !IgnoreForCodeRuleAnalyse.Any(namespaceToIgnore => tree.NameSpace.Name.Contains(namespaceToIgnore)))
                                      .ToImmutableList();

            AllSyntaxTrees = ProductiveSyntaxTrees.Concat(TestSyntaxTrees).ToImmutableList();

            SyntaxTreesToAnalyze = AllSyntaxTrees.Where(tree => !IgnoreForCodeRuleAnalyse.Any(namespaceToIgnore => tree.NameSpace.Name.Contains(namespaceToIgnore))).ToImmutableList();

            Records = AllSyntaxTrees.SelectMany(tree => tree.Records).ToImmutableList();
            Classes = AllSyntaxTrees.SelectMany(tree => tree.Classes).ToImmutableList();
            Enums = AllSyntaxTrees.SelectMany(tree => tree.Enums).ToImmutableList();
            Interfaces = AllSyntaxTrees.SelectMany(tree => tree.Interfaces).ToImmutableList();
            Structs = AllSyntaxTrees.SelectMany(tree => tree.Structs).ToImmutableList();

            RequestsJsonFiles = Solution.UnitTestProjects.SelectMany(p => p.ProjectFileInfo.Value.Directory!.EnumerateFiles("*.json", SearchOption.AllDirectories))
                                        .Where(f => f.FullName.Contains($"{Path.DirectorySeparatorChar}Requests{Path.DirectorySeparatorChar}") &&
                                                    f.FullName.DoesNotContain($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}") &&
                                                    f.FullName.DoesNotContain($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"))
                                        .Select(f => new JsonFile(f, File.ReadAllText(f.FullName))).ToImmutableList();

            ResponseJsonFiles = Solution.UnitTestProjects.SelectMany(p => p.ProjectFileInfo.Value.Directory!.EnumerateFiles("*.json", SearchOption.AllDirectories))
                                        .Where(f => f.FullName.Contains($"{Path.DirectorySeparatorChar}Responses{Path.DirectorySeparatorChar}") &&
                                                    f.FullName.DoesNotContain($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}") &&
                                                    f.FullName.DoesNotContain($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"))
                                        .Select(f => new JsonFile(f, File.ReadAllText(f.FullName))).ToImmutableList();
        }
    }

    public record Variable(Class Class,
                           string SqlStatement);
}
