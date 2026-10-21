using System.Collections.Immutable;

namespace Basta.CodeRules
{
    /// <summary>
    /// One violation of a CodeRule: what is wrong and where. Why and the default fix live on the rule,
    /// see <see cref="CodeRuleViolation"/>.
    /// </summary>
    public sealed record CodeRuleFinding
    {
        /// <summary>What is wrong: the offending type, member, reference or package.</summary>
        public required string Subject { get; init; }

        /// <summary>Where it is. Accepts a <see cref="Solution.Parser.CSharp.CodeLocation"/> or a <see cref="FileInfo"/>.</summary>
        public FindingLocation? Location { get; init; }

        /// <summary>The offending code as it is now.</summary>
        public string? Current { get; init; }

        /// <summary>The corrected code.</summary>
        public string? Suggested { get; init; }

        /// <summary>Overrides the fix of the rule when the fix depends on this finding.</summary>
        public string? Fix { get; init; }

        /// <summary>Further reading for this finding, e.g. the docs of the API or pattern it is about. The rule documentation is added by the assert.</summary>
        public string? DocumentationUrl { get; init; }

        /// <summary>Extra "Key : value" lines.</summary>
        public ImmutableDictionary<string, string>? Details { get; init; }
    }

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
                // Uri escapes '#' (e.g. "03_C#_Files"), which would otherwise start the fragment and cut the link
                var fileUri = new Uri(Path.GetFullPath(Path.Combine(RepositoryRoot.Directory.FullName, File))).AbsoluteUri;

                return Line is null ? fileUri : $"{fileUri}:{Line}";
            }
        }
    }
}
