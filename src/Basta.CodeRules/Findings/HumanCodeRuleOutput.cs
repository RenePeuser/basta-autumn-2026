using System.Collections.Immutable;
using System.Text;

namespace Basta.CodeRules
{
    /// <summary>
    /// Renders a violation in the same look as the AspNetCore.Simple.MsTest.Sdk failures: framed sections,
    /// an aligned overview table and one section per finding.
    /// </summary>
    public static class HumanCodeRuleOutput
    {
        public const int MaxDetailedFindings = 10;

        private const int MaxCellWidth = 60;

        private const int LabelWidth = 10;

        private static readonly string Frame = new('═', 62);

        private static readonly string Rule = new('─', 62);

        public static string Render(CodeRuleViolation violation)
        {
            var output = new StringBuilder();
            var findings = violation.Findings;

            output.AppendLine();
            output.AppendLine(Frame);
            output.AppendLine($"🛡️ CODE RULE VIOLATED · {violation.RuleId}");
            output.AppendLine(Frame);

            Section(output, "📦 Rule Information");
            Field(output, "Id", violation.RuleId);
            Field(output, "Title", violation.Title);
            Field(output, "Category", violation.Category);
            Field(output, "Findings", findings.Count.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Field(output, "Docs", violation.DocumentationUrl);
            Field(output, "Rule", violation.RuleSource.ClickableLink);

            Section(output, "⚠️ Failure Details");
            output.AppendLine(violation.Because);

            Section(output, $"🔍 Findings (Count {findings.Count})");
            output.Append(FindingsTable(findings));

            foreach (var (finding, index) in findings.Take(MaxDetailedFindings).Select((finding, index) => (finding, index)))
            {
                Section(output, $"📄 Finding {index + 1} of {findings.Count}");
                Field(output, "Subject", finding.Subject);
                Field(output, "File", finding.Location?.ClickableLink);

                foreach (var detail in finding.Details ?? ImmutableDictionary<string, string>.Empty)
                {
                    Field(output, detail.Key, detail.Value);
                }

                Field(output, "Current", finding.Current);
                Field(output, "Suggested", finding.Suggested);
                Field(output, "Fix", finding.Fix ?? violation.Fix);
            }

            if (findings.Count > MaxDetailedFindings)
            {
                output.AppendLine();
                output.AppendLine($"… and {findings.Count - MaxDetailedFindings} more (see table above)");
            }

            output.AppendLine();
            output.AppendLine(Frame);

            return output.ToString();
        }

        private static void Section(StringBuilder output,
                                    string title)
        {
            output.AppendLine();
            output.AppendLine(title);
            output.AppendLine(Rule);
            output.AppendLine();
        }

        private static void Field(StringBuilder output,
                                  string label,
                                  string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            // Continuation lines of multi line values start under the value column
            var indent = new string(' ', LabelWidth + 3);
            var lines = value.Trim().ReplaceLineEndings("\n").Split('\n');

            output.AppendLine($"{label.PadRight(LabelWidth)} : {lines[0]}");

            foreach (var line in lines.Skip(1))
            {
                output.AppendLine($"{indent}{line}");
            }
        }

        private static string FindingsTable(ImmutableList<CodeRuleFinding> findings)
        {
            var withDetails = findings.Any(finding => finding.Details is { Count: > 0 });

            var header = withDetails ? new[] { "#", "Subject", "Location", "Detail" } : new[] { "#", "Subject", "Location" };

            var rows = findings.Select((finding, index) =>
                                       {
                                           var cells = new List<string>
                                           {
                                               (index + 1).ToString(System.Globalization.CultureInfo.InvariantCulture),
                                               finding.Subject,
                                               finding.Location?.ShortForm ?? "-"
                                           };

                                           if (withDetails)
                                           {
                                               cells.Add(string.Join(", ", (finding.Details ?? ImmutableDictionary<string, string>.Empty).Values));
                                           }

                                           return cells.Select(Truncate).ToArray();
                                       })
                               .ToImmutableList();

            var widths = header.Select((title, column) => rows.Select(row => row[column].Length).Append(title.Length).Max()).ToArray();

            var table = new StringBuilder();
            table.AppendLine(Border('┌', '┬', '┐', widths));
            table.AppendLine(Row(header, widths));
            table.AppendLine(Border('├', '┼', '┤', widths));

            foreach (var row in rows)
            {
                table.AppendLine(Row(row, widths));
            }

            table.AppendLine(Border('└', '┴', '┘', widths));

            return table.ToString();
        }

        private static string Truncate(string cell)
        {
            var singleLine = cell.ReplaceLineEndings(" ");

            return singleLine.Length <= MaxCellWidth ? singleLine : string.Concat(singleLine.AsSpan(0, MaxCellWidth - 1), "…");
        }

        private static string Border(char left,
                                     char middle,
                                     char right,
                                     int[] widths)
        {
            return left + string.Join(middle, widths.Select(width => new string('─', width + 2))) + right;
        }

        private static string Row(string[] cells,
                                  int[] widths)
        {
            return "│" + string.Join("│", cells.Select((cell, column) => $" {cell.PadRight(widths[column])} ")) + "│";
        }
    }
}
