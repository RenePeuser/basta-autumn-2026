namespace Basta.CodeRules
{
    internal static class RepositoryRoot
    {
        private const string SolutionFileName = "code-rules.slnx";

        internal static readonly DirectoryInfo Directory = FindRoot();

        internal static string RelativePath(string path)
        {
            return Path.GetRelativePath(Directory.FullName, path).Replace('\\', '/');
        }

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
