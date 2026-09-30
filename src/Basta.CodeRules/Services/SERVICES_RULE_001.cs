using System.Collections.Immutable;
using Solution.Parser.CSharp;

namespace Basta.CodeRules
{
    [TestCategory("CodeRules")]
    [TestCategory("CodeRules Services")]
    [TestClass]
    public class SERVICES_RULE_001 : ServiceCodeRuleBase
    {
        [TestMethod]
        public void Each_Service_Has_To_Have_A_Registration_Extension_In_The_Same_File()
        {
            var findings = from service in Services
                           where service.Registration is null
                           select new CodeRuleFinding
                           {
                               Subject = service.Class.FullQualifiedName,
                               Location = service.Class.Location,
                               Current = service.WrongNameRegistration?.SyntaxTree.FormatSyntaxTree(),
                               Suggested = SampleRegistration(service.Class),
                               Fix = service.WrongNameRegistration is null
                                         ? $"Add a static class 'Add{service.Class.Name}Extension' with an 'Add{service.Class.Name}(this IServiceCollection services)' method to '{Path.GetFileName(service.Class.FilePath)}'."
                                         : $"Rename '{service.WrongNameRegistration.Name}' to 'Add{service.Class.Name}Extension', the registration has to be named after the service.",
                               Details = ImmutableDictionary<string, string>.Empty.Add("Service", service.Class.Name)
                           };

            Assert.That.CodeRuleHasNoFindings(findings,
                                              rule: "SERVICES_RULE_001",
                                              title: "Each service needs a registration extension in the same file",
                                              because: "Whoever uses a service calls exactly one registration method and gets everything the service needs. " +
                                                       "The registration next to the service keeps both in sync; a service registered somewhere else " +
                                                       "is easily forgotten and only fails at runtime with 'Unable to resolve service'.",
                                              fix: "Add a static 'Add<Service>Extension' class with an 'Add<Service>(this IServiceCollection services)' method to the file of the service.");

            return;

            static string SampleRegistration(Class service)
            {
                var serviceInterface = service.BaseTypes.Select(baseType => baseType.TypeName).FirstOrDefault();
                var registration = serviceInterface is null ? $"services.AddSingleton<{service.Name}>();" : $"services.AddSingleton<{serviceInterface}, {service.Name}>();";

                return $$"""
                         internal static class Add{{service.Name}}Extension
                         {
                             internal static void Add{{service.Name}}(this IServiceCollection services)
                             {
                                 {{registration}}
                             }
                         }
                         """;
            }
        }
    }
}
