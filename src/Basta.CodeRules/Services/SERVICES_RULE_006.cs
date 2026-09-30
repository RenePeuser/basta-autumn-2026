using System.Collections.Immutable;

namespace Basta.CodeRules
{
    [TestCategory("CodeRules")]
    [TestCategory("CodeRules Services")]
    [TestClass]
    public class SERVICES_RULE_006 : ServiceCodeRuleBase
    {
        private const int MaxDependencies = 5;

        [TestMethod]
        public void Services_Are_Not_Allowed_To_Inject_Too_Many_Dependencies()
        {
            // Primary constructor or the classic constructor with the most parameters
            var findings = from service in Services
                           let parameters = service.Class.Constructors
                                                   .Select(constructor => constructor.Parameters)
                                                   .Append(service.Class.Parameters)
                                                   .MaxBy(parameters => parameters.Count)
                           where parameters is not null && parameters.Count > MaxDependencies
                           select new CodeRuleFinding
                           {
                               Subject = service.Class.FullQualifiedName,
                               Location = service.Class.Location,
                               Current = $"{service.Class.Name}({string.Join(", ", parameters.Select(parameter => $"{parameter.Type} {parameter.Name}"))})",
                               Fix = $"'{service.Class.Name}' injects {parameters.Count} dependencies. Split it into smaller services with one responsibility each, or group dependencies that are always used together behind one service.",
                               Details = ImmutableDictionary<string, string>.Empty.Add("Dependencies", $"{parameters.Count} (max {MaxDependencies})")
                           };

            Assert.That.CodeRuleHasNoFindings(findings,
                                              rule: "SERVICES_RULE_006",
                                              title: $"Services inject at most {MaxDependencies} dependencies",
                                              because: "Many constructor dependencies are a strong hint that a service does too much (Single Responsibility Principle). " +
                                                       "Such services are hard to test, because every test has to set up all dependencies.",
                                              fix: "Split the service or group dependencies that belong together.");
        }
    }
}
