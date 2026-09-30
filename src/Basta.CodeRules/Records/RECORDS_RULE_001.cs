using System.Collections.Immutable;
using Extensions.Pack;

namespace Basta.CodeRules
{
    [TestCategory("CodeRules")]
    [TestCategory("CodeRules Records")]
    [TestClass]
    public class RECORDS_RULE_001 : CodeRuleTestBase
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
                                              because: "A record is a snapshot of a state, e.g. data loaded from the database. It must not change while it is being processed. A setter lets any code modify the same instance, so other parts that hold a reference suddenly see different data. A change must produce a new instance instead.",
                                              fix: "Replace 'set;' with 'init;' and create a modified copy with a 'with' expression, e.g. 'order with { Status = OrderStatus.Shipped }'.");
        }
    }
}
