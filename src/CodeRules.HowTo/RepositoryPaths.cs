namespace CodeRules.HowTo
{
    internal static class RepositoryPaths
    {
        private const string SolutionFileName = "code-rules.slnx";

        internal static readonly DirectoryInfo Root = FindRoot();

        internal static string Solution => Path.Combine(Root.FullName, SolutionFileName);

        internal static string HowToProject =>
            Path.Combine(Root.FullName, "src", "CodeRules.HowTo",
                         "CodeRules.HowTo.csproj");

        private static DirectoryInfo FindRoot()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);

            while (directory is not null && !File.Exists(Path.Combine(directory.FullName, SolutionFileName)))
            {
                directory = directory.Parent;
            }

            return directory ?? throw new InvalidOperationException($"'{SolutionFileName}' not found above '{AppContext.BaseDirectory}'.");
        }
    }
}
