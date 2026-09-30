using System.Collections.Immutable;
using Solution.Parser.CSharp;

namespace Basta.CodeRules
{
    [TestCategory("CodeRules")]
    [TestCategory("CodeRules Services")]
    [TestClass]
    public class SERVICES_RULE_004 : ServiceCodeRuleBase
    {
        [TestMethod]
        public void Each_Service_Registration_Has_To_Register_The_Dependencies_Of_The_Service()
        {
            // Only dependencies declared in this solution. Framework types (ILogger<T>, IOptions<T>, HttpClient, ...)
            // are registered by the host. IEnumerable<T> collects whatever the features register, the collector must not know them.
            var declaredTypes = Classes.Select(type => type.Name)
                                       .Concat(Interfaces.Select(type => type.Name))
                                       .Concat(Records.Select(type => type.Name))
                                       .ToImmutableHashSet(StringComparer.Ordinal);

            var registrationMethods = ProductiveSyntaxTrees.SelectMany(tree => tree.AllMethods())
                                                           .Where(method => method.IsServiceRegistration())
                                                           .ToImmutableList();

            var findings = from service in Services
                           let registration = service.Registration
                           where registration is not null
                           from registrationMethod in registration.Methods
                           where registrationMethod.IsServiceRegistration()
                           from parameter in service.Class.Parameters.Concat(service.Class.Constructors.Where(constructor => !constructor.IsPrimary).SelectMany(constructor => constructor.Parameters))
                           let dependency = parameter.Type.TrimEnd('?')
                           where declaredTypes.Contains(dependency)
                           where !Registers(registrationMethod, dependency, ImmutableHashSet<string>.Empty)
                           let providingRegistration = registrationMethods.FirstOrDefault(method => RegistersDirectly(method, dependency))
                           select new CodeRuleFinding
                           {
                               Subject = $"{service.Class.FullQualifiedName} → {dependency}",
                               Location = registrationMethod.Location,
                               Current = registrationMethod.SyntaxTree.FormatSyntaxTree(),
                               Suggested = providingRegistration is null ? null : $"services.{providingRegistration.Name}();",
                               Fix = providingRegistration is null
                                         ? $"'{dependency}' has no registration yet. Add one next to '{dependency}' (SERVICES_RULE_001) and call it from '{registrationMethod.Name}'."
                                         : $"Call 'services.{providingRegistration.Name}()' in '{registrationMethod.Name}'.",
                               Details = ImmutableDictionary<string, string>.Empty.Add("Service", service.Class.Name)
                                                                                  .Add("Dependency", $"{parameter.Type} {parameter.Name}")
                           };

            Assert.That.CodeRuleHasNoFindings(findings,
                                              rule: "SERVICES_RULE_004",
                                              title: "A service registration registers the dependencies of the service",
                                              because: "Whoever uses a service calls only its registration. If that registration does not bring the dependencies along, " +
                                                       "every caller has to know the internals of the service, and a forgotten dependency only fails at runtime with 'Unable to resolve service'.",
                                              fix: "Call the registration of each dependency from the registration of the service.");

            return;

            // Directly, or through a registration it calls (transitive)
            bool Registers(Method method,
                           string dependency,
                           ImmutableHashSet<string> visited)
            {
                if (RegistersDirectly(method, dependency))
                {
                    return true;
                }

                return registrationMethods.Where(called => !visited.Contains(called.FullQualifiedName) &&
                                                           method.Body.Contains($".{called.Name}(", StringComparison.Ordinal))
                                          .Any(called => Registers(called, dependency, visited.Add(method.FullQualifiedName)));
            }

            // services.AddSingleton<IFoo, Foo>() / services.AddSingleton<Foo>()
            static bool RegistersDirectly(Method method,
                                          string dependency)
            {
                return method.Body.Contains($"<{dependency}>", StringComparison.Ordinal) ||
                       method.Body.Contains($"<{dependency},", StringComparison.Ordinal) ||
                       method.Body.Contains($", {dependency}>", StringComparison.Ordinal);
            }
        }
    }
}
