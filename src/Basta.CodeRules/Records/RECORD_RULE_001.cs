using System.Collections.Immutable;
using Extensions.Pack;

namespace Basta.CodeRules.Records
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
                           select new CodeRuleFinding(Subject: $"{record.FullQualifiedName}.{property.Name}",
                                                      Location: property.Location,
                                                      Current: property.SyntaxTree,
                                                      Suggested: property.SyntaxTree.Replace("set;", "init;"),
                                                      Details: ImmutableDictionary<string, string>.Empty.Add("Property", $"{property.Type} {property.Name}"));

            Assert.That.CodeRuleHasNoFindings(findings,
                                              rule: "RECORD_RULE_001",
                                              title: "Record properties must be immutable",
                                              because: "Records are value objects. Mutable properties break value equality and make instances unsafe to share.",
                                              fix: "Replace 'set;' with 'init;'.");
        }
    }
}
