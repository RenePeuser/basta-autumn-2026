using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Basta.CodeRules
{
    /// <summary>
    /// Renders a violation as JSON for AI agents: every finding, repository relative paths, no decoration.
    /// Field names follow the AiOutputTransformer of AspNetCore.Simple.MsTest.Sdk.
    /// </summary>
    public static class AiCodeRuleOutput
    {
        private static readonly JsonSerializerOptions Options = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,

            // Keep <, > and ' readable - findings quote csproj XML and C# code
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        public static string Render(CodeRuleViolation violation)
        {
            var output = new
            {
                ErrorCode = "CODE_RULE_VIOLATED",
                Severity = "error",
                Rule = new
                {
                    Id = violation.RuleId,
                    violation.Title,
                    violation.Category,
                    Why = violation.Because,
                    Documentation = violation.DocumentationUrl,
                    Source = new { violation.RuleSource.File, violation.RuleSource.Line }
                },
                FindingCount = violation.Findings.Count,
                Findings = violation.Findings.Select(finding => new
                {
                    finding.Subject,
                    finding.Location?.File,
                    finding.Location?.Line,
                    finding.Current,
                    finding.Suggested,
                    SuggestedFix = finding.Fix ?? violation.Fix,
                    finding.Details
                }),
                Instructions = "Fix the code, not the rule. Do not weaken the rule, add exceptions or [Ignore] without human approval. " +
                               $"Re-run: dotnet test src/Basta.CodeRules --filter \"FullyQualifiedName~{violation.RuleId}\""
            };

            return Environment.NewLine + JsonSerializer.Serialize(output, Options);
        }
    }
}
