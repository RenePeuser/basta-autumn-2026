using System.Collections.Immutable;

namespace Basta.CodeRules
{
    /// <summary>
    /// One violation of a CodeRule: what is wrong and where. Why and the default fix live on the rule,
    /// see <see cref="CodeRuleViolation"/>.
    /// </summary>
    public sealed record CodeRuleFinding(string Subject,
                                         FindingLocation? Location = null,
                                         string? Current = null,
                                         string? Suggested = null,
                                         string? Fix = null,
                                         ImmutableDictionary<string, string>? Details = null);

    /// <summary>
    /// Where a finding sits, with the path relative to the repository root so the output is the same on every machine.
    /// </summary>
    public sealed record FindingLocation(string File,
                                         int? Line = null)
    {
        public static implicit operator FindingLocation(Solution.Parser.CSharp.CodeLocation location)
        {
            return new FindingLocation(RepositoryRoot.RelativePath(location.FilePath), location.StartLine);
        }

        public static implicit operator FindingLocation(FileInfo file)
        {
            return new FindingLocation(RepositoryRoot.RelativePath(file.FullName));
        }

        public string FileName => Path.GetFileName(File);

        public string ShortForm => Line is null ? FileName : $"{FileName}:{Line}";

        public string ClickableLink
        {
            get
            {
                var absolutePath = Path.GetFullPath(Path.Combine(RepositoryRoot.Directory.FullName, File)).Replace('\\', '/');

                return Line is null ? $"file:///{absolutePath}" : $"file:///{absolutePath}:{Line}";
            }
        }
    }
}
