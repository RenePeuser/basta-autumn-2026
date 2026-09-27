using AspNetCore.Simple.MsTest.Sdk;
using System.Collections.Immutable;
using System.Text.Json;

namespace Basta.CodeRules
{
    /// <summary>
    /// Fixtures for the finding output itself. Not tagged "CodeRules" - these do not check the repository.
    /// </summary>
    [TestCategory("CodeRules Infrastructure")]
    [TestClass]
    public class CodeRuleOutput_Test
    {
        private static readonly ImmutableList<CodeRuleFinding> Findings =
            [
                new()
                {
                    Subject = "Basta.WebApi.UserDto.Email",
                    Location = new FindingLocation("src/Basta.WebApi/UserDto.cs", 12),
                    Current = "public required string Email { get; set; }",
                    Suggested = "public required string Email { get; init; }",
                    DocumentationUrl = "https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/init",
                    Details = ImmutableDictionary<string, string>.Empty.Add("Property", "string Email")
                },
                new()
                {
                    Subject = "CodeRules.HowTo.Person.City",
                    Location = new FindingLocation("src/CodeRules.HowTo/Person.cs", 9),
                    Current = "public required string City { get; set; }",
                    Suggested = "public required string City { get; init; }",
                    Details = ImmutableDictionary<string, string>.Empty.Add("Property", "string City")
                }
            ];

        [TestMethod]
        public void No_Findings_Passes()
        {
            Assert.That.DoesNotThrow(() => Assert.That.CodeRuleHasNoFindings([], rule: "TEST_RULE_001", title: "Title", because: "Because", fix: "Fix"),
                                     because: "A rule without findings is green",
                                     fix: "CodeRuleHasNoFindings must return when the findings are empty");
        }

        [TestMethod]
        public void Findings_Throw_A_Violation_With_Sorted_Findings()
        {
            var exception = Assert.That.ThrowsExactly<CodeRuleViolationException>(() => Assert.That.CodeRuleHasNoFindings(Findings.Reverse(), rule: "TEST_RULE_001", title: "Title", because: "Because", fix: "Fix"),
                                                                                  because: "A rule with findings has to fail with the structured violation",
                                                                                  fix: "CodeRuleHasNoFindings must throw CodeRuleViolationException");

            Assert.That.AreEqual(Findings.Select(finding => finding.Subject),
                                 exception.Violation.Findings.Select(finding => finding.Subject),
                                 because: "Findings are sorted by file and line, so the output does not depend on parse order",
                                 fix: "Sort the findings in CodeRuleHasNoFindings by location, then subject");

            Assert.That.AreEqual("Findings",
                                 exception.Violation.Category,
                                 because: "The category is the folder of the rule file",
                                 fix: "Derive the category from the caller file path");
        }

        [TestMethod]
        public void Human_Output_Has_An_Aligned_Table()
        {
            var output = HumanCodeRuleOutput.Render(Violation(Findings));

            var tableLineLengths = output.ReplaceLineEndings("\n")
                                         .Split('\n')
                                         .Where(line => line.StartsWith('┌') || line.StartsWith('│') || line.StartsWith('├') || line.StartsWith('└'))
                                         .Select(line => line.Length)
                                         .Distinct()
                                         .ToImmutableList();

            Assert.That.HasCount(1,
                                 tableLineLengths,
                                 because: "Every line of the findings table has the same width",
                                 fix: "Compute the column widths from the longest cell in HumanCodeRuleOutput.FindingsTable");
        }

        [TestMethod]
        public void Human_Output_Details_Only_The_First_Findings()
        {
            var manyFindings = Enumerable.Range(1, HumanCodeRuleOutput.MaxDetailedFindings + 3)
                                         .Select(number => new CodeRuleFinding { Subject = $"Subject {number:00}" })
                                         .ToImmutableList();

            var output = HumanCodeRuleOutput.Render(Violation(manyFindings));

            Assert.That.Contains(output,
                                 "… and 3 more (see table above)",
                                 because: "Long outputs keep the table complete but cut the detailed sections",
                                 fix: "Render at most MaxDetailedFindings finding sections");
        }

        [TestMethod]
        public void Ai_Output_Is_Json_With_Every_Finding()
        {
            var output = AiCodeRuleOutput.Render(Violation(Findings));

            using var json = JsonDocument.Parse(output);

            Assert.That.AreEqual(2,
                                 json.RootElement.GetProperty("findings").GetArrayLength(),
                                 because: "Agents need every finding, not a shortened list",
                                 fix: "Serialize all findings in AiCodeRuleOutput");

            Assert.That.AreEqual("src/Basta.WebApi/UserDto.cs",
                                 json.RootElement.GetProperty("findings")[0].GetProperty("file").GetString(),
                                 because: "Paths are relative to the repository root so they work on every machine",
                                 fix: "Write FindingLocation.File, not the clickable absolute link");
        }

        [TestMethod]
        public void Finding_Documentation_Url_Is_Rendered()
        {
            var violation = Violation(Findings);

            Assert.That.Contains(HumanCodeRuleOutput.Render(violation),
                                 "https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/init",
                                 because: "A finding can point to further reading for exactly this problem",
                                 fix: "Render CodeRuleFinding.DocumentationUrl as 'Docs' field in HumanCodeRuleOutput");

            using var json = JsonDocument.Parse(AiCodeRuleOutput.Render(violation));

            Assert.That.AreEqual("https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/init",
                                 json.RootElement.GetProperty("findings")[0].GetProperty("documentation").GetString(),
                                 because: "Agents need the finding documentation as well",
                                 fix: "Serialize CodeRuleFinding.DocumentationUrl as 'documentation' in AiCodeRuleOutput");
        }

        [TestMethod]
        public void Clickable_Link_Escapes_Special_Characters_In_The_Path()
        {
            var link = new FindingLocation("src/CodeRules.HowTo/03_C#_Files/Reflection_PropertyInfo_Test.cs", 9).ClickableLink;

            Assert.That.EndsWith(link,
                                 "/src/CodeRules.HowTo/03_C%23_Files/Reflection_PropertyInfo_Test.cs:9",
                                 because: "An unescaped '#' starts the URI fragment, so the link would end at '03_C' and open nothing",
                                 fix: "Build FindingLocation.ClickableLink with System.Uri instead of concatenating the raw path");
        }

        private static CodeRuleViolation Violation(ImmutableList<CodeRuleFinding> findings)
        {
            return new CodeRuleViolation(RuleId: "TEST_RULE_001",
                                         Title: "Record properties must be immutable",
                                         Category: "Records",
                                         Because: "Records are value objects.",
                                         Fix: "Replace 'set;' with 'init;'.",
                                         DocumentationUrl: $"{CodeRuleSettings.DocumentationDomain}#TEST_RULE_001",
                                         RuleSource: new FindingLocation("src/Basta.CodeRules/Records/TEST_RULE_001.cs", 1),
                                         Findings: findings);
        }
    }
}
