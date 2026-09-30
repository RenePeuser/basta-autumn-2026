
using System.Collections.Immutable;

namespace Basta.CodeRules
{
    [TestCategory("CodeRules")]
    [TestCategory("CodeRules Records")]
    [TestClass]
    public class RECORDS_RULE_002 : CodeRuleTestBase
    {
        [TestMethod]
        public void Records_Are_Not_Allowed_To_Implement_Methods()
        {
            var findings = from record in Records
                           from method in record.Methods
                           select new CodeRuleFinding
                           {
                               Subject = method.FullQualifiedName,
                               Location = method.Location,
                               Current = method.SyntaxTree,
                               Fix = $"Move the method '{method.Name}' to a separate service class or static helper.",
                               Details = ImmutableDictionary.Create<string, string>().Add("Record", record.Name)
                                                                                     .Add("Method", $"{method.ReturnParameter} {method.Name}(...)")
                           };

            Assert.That.CodeRuleHasNoFindings(findings,
                                              rule: "RECORDS_RULE_002",
                                              title: "Records must not implement methods",
                                              because: "Records are designed for data transfer and value objects. " +
                                                       "Mixing data and logic violates the separation of concerns and " +
                                                       "makes the code harder to test and maintain.",
                                              fix: "Move methods to a separate service class or static helper class.");
        }
    }
}
