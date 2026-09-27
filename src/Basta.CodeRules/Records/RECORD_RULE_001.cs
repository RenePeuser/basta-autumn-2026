using System.Collections.Immutable;
using Extensions.Pack;

namespace Basta.CodeRules
{
    [TestCategory("CodeRules")]
    [TestCategory("CodeRules Records")]
    [TestClass]
    public class RECORD_RULE_001 : CodeRuleTestBase
    {
        [TestMethod]
        public void Record_Properties_Have_To_Be_Be_Immutable()
        {
            var findings = from record in Records
                           from property in record.Properties
                           where property.IsReadOnly.IsFalse()
                           select new CodeRuleFinding
                           {
                               Subject = property.FullQualifiedName,
                               Location = property.Location,
                               Current = property.SyntaxTree,
                               Suggested = property.SyntaxTree.Replace("set;", "init;"),
                               Details = ImmutableDictionary<string, string>.Empty.Add("Property", $"{property.Type} {property.Name}"),
                               DocumentationUrl = "https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/record#init-only-properties"
                           };

            Assert.That.CodeRuleHasNoFindings(findings,
                                              rule: "RECORD_RULE_001",
                                              title: "Record properties must be immutable",
                                              because: "Mutable properties on records can change the values used for equality and hashing after an instance is created. This can lead to unexpected behavior, especially when the record is used as a key in a dictionary or stored in a hash set.",
                                              fix: "Replace 'set;' with 'init;'.");
        }
    }
}
