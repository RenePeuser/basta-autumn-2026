using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Basta.CodeRules
{
    public static class AssertThatCodeRuleExtensions
    {
        /// <summary>
        /// Fails with a <see cref="CodeRuleViolationException"/> when <paramref name="findings"/> is not empty.
        /// The output is Human or Ai, see <see cref="CodeRuleSettings.OutputMode"/>. The category is the folder of the rule file.
        /// </summary>
        public static void CodeRuleHasNoFindings(this Assert _,
                                                 IEnumerable<CodeRuleFinding> findings,
                                                 string rule,
                                                 string title,
                                                 string because,
                                                 string fix,
                                                 [CallerFilePath] string callerFilePath = "",
                                                 [CallerLineNumber] int callerLineNumber = 0)
        {
            // Sorted, so the output does not depend on parse order
            var sortedFindings = findings.OrderBy(finding => finding.Location?.File, StringComparer.Ordinal)
                                         .ThenBy(finding => finding.Location?.Line)
                                         .ThenBy(finding => finding.Subject, StringComparer.Ordinal)
                                         .ToImmutableList();

            if (sortedFindings.IsEmpty)
            {
                return;
            }

            var violation = new CodeRuleViolation(RuleId: rule,
                                                  Title: title,
                                                  Category: Path.GetFileName(Path.GetDirectoryName(callerFilePath)) ?? "-",
                                                  Because: because,
                                                  Fix: fix,
                                                  DocumentationUrl: $"{CodeRuleSettings.DocumentationDomain}#{rule}",
                                                  RuleSource: new FindingLocation(RepositoryRoot.RelativePath(callerFilePath), callerLineNumber),
                                                  Findings: sortedFindings);

            var message = CodeRuleSettings.OutputMode == CodeRuleOutputMode.Ai ? AiCodeRuleOutput.Render(violation) : HumanCodeRuleOutput.Render(violation);

            throw new CodeRuleViolationException(violation, message);
        }
    }
}
