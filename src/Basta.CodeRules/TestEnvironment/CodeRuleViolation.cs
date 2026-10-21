using System.Collections.Immutable;

namespace Basta.CodeRules
{
    /// <summary>
    /// The complete result of a failing CodeRule: the rule and every finding. Human and Ai output are both rendered from it.
    /// </summary>
    public sealed record CodeRuleViolation(string RuleId,
                                           string Title,
                                           string Category,
                                           string Because,
                                           string Fix,
                                           string DocumentationUrl,
                                           FindingLocation RuleSource,
                                           ImmutableList<CodeRuleFinding> Findings);

    /// <summary>
    /// Thrown when a CodeRule has findings. Carries the <see cref="CodeRuleViolation"/>, so fixtures can check
    /// the findings instead of the rendered text.
    /// </summary>
    public sealed class CodeRuleViolationException(CodeRuleViolation violation,
                                                   string message) : AssertFailedException(message)
    {
        public CodeRuleViolation Violation { get; } = violation;
    }
}
