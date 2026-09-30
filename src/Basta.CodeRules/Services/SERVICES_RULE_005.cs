using System.Collections.Immutable;
using Solution.Parser.CSharp;

namespace Basta.CodeRules
{
    [TestCategory("CodeRules")]
    [TestCategory("CodeRules Services")]
    [TestClass]
    public class SERVICES_RULE_005 : CodeRuleTestBase
    {
        [TestMethod]
        public void Service_Registrations_Have_To_Return_Void()
        {
            var findings = from @class in Classes
                           where @class.IsStatic()
                           from method in @class.Methods
                           where method.IsStatic() &&
                                 method.Name.StartsWith("Add", StringComparison.Ordinal) &&
                                 method.IsServiceRegistration() &&
                                 method.ReturnParameter != "void"
                           select new CodeRuleFinding
                           {
                               Subject = $"{@class.FullQualifiedName}.{method.Name}",
                               Location = method.Location,
                               Current = method.SyntaxTree.FormatSyntaxTree(),
                               Fix = $"Change the return type of '{method.Name}' from '{method.ReturnParameter}' to 'void' and remove the 'return'.",
                               Details = ImmutableDictionary<string, string>.Empty.Add("Returns", method.ReturnParameter)
                           };

            Assert.That.CodeRuleHasNoFindings(findings,
                                              rule: "SERVICES_RULE_005",
                                              title: "Service registrations return void",
                                              because: "One style for all registrations: a registration is a statement, not a builder. " +
                                                       "Returning IServiceCollection invites long fluent chains in which a missing or duplicate registration is hard to spot in a review.",
                                              fix: "Return 'void' from the registration and call each registration as its own statement.");
        }
    }
}
