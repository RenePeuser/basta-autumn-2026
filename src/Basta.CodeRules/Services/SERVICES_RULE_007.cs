using System.Collections.Immutable;
using Solution.Parser.CSharp;

namespace Basta.CodeRules
{
    [TestCategory("CodeRules")]
    [TestCategory("CodeRules Services")]
    [TestClass]
    public class SERVICES_RULE_007 : ServiceCodeRuleBase
    {
        [TestMethod]
        public void Each_Service_Registration_Has_To_Register_The_Service_Itself()
        {
            var findings = from service in Services
                           let registration = service.Registration
                           where registration is not null
                           let registrationMethods = registration.Methods.Where(method => method.IsServiceRegistration()).ToImmutableList()
                           where registrationMethods.Any()
                           where !registrationMethods.Any(method => RegistersService(method, service.Class.Name))
                           from registrationMethod in registrationMethods
                           select new CodeRuleFinding
                           {
                               Subject = $"{registration.FullQualifiedName}.{registrationMethod.Name}",
                               Location = registrationMethod.Location,
                               Current = registrationMethod.SyntaxTree.FormatSyntaxTree(),
                               Suggested = SampleRegistration(service.Class),
                               Fix = $"Register '{service.Class.Name}' in '{registrationMethod.Name}', e.g. '{SampleRegistration(service.Class)}'. Commented out registrations do not count.",
                               Details = ImmutableDictionary<string, string>.Empty.Add("Service", service.Class.Name)
                           };

            Assert.That.CodeRuleHasNoFindings(findings,
                                              rule: "SERVICES_RULE_007",
                                              title: "Each service registration has to register the service itself",
                                              because: "Whoever uses a service calls only its registration. A registration that exists and is called, " +
                                                       "but does not register the service (empty or commented out), compiles and passes every other check, " +
                                                       "yet fails at runtime with 'Unable to resolve service' or silently drops the feature (e.g. a missing endpoint).",
                                              fix: "Register the service in its 'Add<Service>' method, e.g. 'services.AddSingleton<IService, Service>();'.");

            return;

            // services.AddSingleton<IFoo, Foo>() / services.AddSingleton<Foo>() / services.AddHostedService<Foo>() - comments are ignored
            static bool RegistersService(Method method,
                                         string service)
            {
                var code = string.Join(Environment.NewLine,
                                       method.Statements
                                             .SelectMany(statement => statement.Split('\n'))
                                             .Select(line => line.Trim())
                                             .Where(line => !line.StartsWith("//", StringComparison.Ordinal)));

                return code.Contains($"<{service}>", StringComparison.Ordinal) ||
                       code.Contains($"<{service},", StringComparison.Ordinal) ||
                       code.Contains($", {service}>", StringComparison.Ordinal);
            }

            static string SampleRegistration(Class service)
            {
                var serviceInterface = service.BaseTypes.Select(baseType => baseType.TypeName).FirstOrDefault();

                return serviceInterface is null ? $"services.AddSingleton<{service.Name}>();" : $"services.AddSingleton<{serviceInterface}, {service.Name}>();";
            }
        }
    }
}
