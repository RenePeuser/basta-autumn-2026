using System.Collections.Immutable;
using Solution.Parser.CSharp;

namespace Basta.CodeRules
{
    [TestCategory("CodeRules")]
    [TestCategory("CodeRules Services")]
    [TestClass]
    public class SERVICES_RULE_002 : ServiceCodeRuleBase
    {
        [TestMethod]
        public void Each_Service_Registration_Has_To_Be_Called()
        {
            // Registrations are called from other registrations or from the top-level statements in Program.cs
            var calls = SyntaxTreesToAnalyze.SelectMany(tree => tree.AllMethods())
                                            .SelectMany(method => method.Statements)
                                            .Concat(ProductiveSyntaxTrees.Where(tree => tree.FileName.EndsWith("Program.cs", StringComparison.Ordinal))
                                                                         .Select(tree => tree.SyntaxTree))
                                            .ToImmutableList();

            var findings = from service in Services
                           let registration = service.Registration
                           where registration is not null
                           from registrationMethod in registration.Methods
                           where registrationMethod.IsServiceRegistration()
                           where !calls.Any(call => call.Contains($".{registrationMethod.Name}(", StringComparison.Ordinal))
                           select new CodeRuleFinding
                           {
                               Subject = $"{registration.FullQualifiedName}.{registrationMethod.Name}",
                               Location = registrationMethod.Location,
                               Current = registrationMethod.SyntaxTree.FormatSyntaxTree(),
                               Suggested = $"services.{registrationMethod.Name}();",
                               Fix = $"Call 'services.{registrationMethod.Name}()' from the registration of the feature that uses '{service.Class.Name}', or from Program.cs.",
                               Details = ImmutableDictionary<string, string>.Empty.Add("Service", service.Class.Name)
                           };

            Assert.That.CodeRuleHasNoFindings(findings,
                                              rule: "SERVICES_RULE_002",
                                              title: "Each service registration has to be called",
                                              because: "A registration that is never called registers nothing. The application compiles, " +
                                                       "but fails at runtime as soon as the service is resolved.",
                                              fix: "Call the registration from the registration of the parent feature or from Program.cs.");
        }
    }
}
