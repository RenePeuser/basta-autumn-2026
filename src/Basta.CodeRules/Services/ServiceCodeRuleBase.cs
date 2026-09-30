using System.Collections.Immutable;
using Extensions.Pack;
using Solution.Parser.CSharp;
using CSharpSyntaxTree = Solution.Parser.CSharp.CSharpSyntaxTree;

namespace Basta.CodeRules
{
    /// <summary>
    /// A productive class that is resolved via dependency injection, together with the registration extension
    /// found in the same file.
    /// </summary>
    public sealed record Service(Class Class,
                                 CSharpSyntaxTree SyntaxTree,
                                 Class? Registration,
                                 Class? WrongNameRegistration);

    public abstract class ServiceCodeRuleBase : CodeRuleTestBase
    {
        private static readonly Lazy<ImmutableList<Service>> LazyServices = new(() => FindServices().ToImmutableList());

        /// <summary>
        /// Heuristic, the model is syntactic: a service is a non static, non abstract productive class with methods,
        /// that implements interfaces only (or nothing) and is never created with 'new'.
        /// Classes in a 'Startup' folder are the composition root, not services.
        /// </summary>
        protected static ImmutableList<Service> Services => LazyServices.Value;

        private static IEnumerable<Service> FindServices()
        {
            // Split every class once up front instead of once per (service, class) pair
            var classesWithLines = Classes.Select(@class => (Class: @class, Lines: @class.SyntaxTree.Split(Environment.NewLine))).ToImmutableArray();

            foreach (var syntaxTree in ProductiveSyntaxTrees)
            {
                var services = syntaxTree.Classes.Where(@class => @class.Modifiers.Any(modifier => modifier is Modifier.Static or Modifier.Abstract).IsFalse() &&
                                                                  @class.Methods.Any() &&
                                                                  (@class.BaseTypes.IsEmpty() || @class.BaseTypes.All(baseType => baseType.TypeName.StartsWith('I'))));

                foreach (var service in services)
                {
                    if (new FileInfo(service.FilePath).Directory!.Name.Contains("startup", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    // new Service() or Service service = new(); -> not resolved via dependency injection
                    var isDirectlyInstantiated = classesWithLines.Any(c => c.Class.SyntaxTree.Contains($"new {service.Name}(", StringComparison.Ordinal) ||
                                                                           c.Lines.Any(line => line.Contains(service.Name, StringComparison.Ordinal) && line.Contains(" = new(", StringComparison.Ordinal)));

                    if (isDirectlyInstantiated)
                    {
                        continue;
                    }

                    var registration = syntaxTree.Classes.FirstOrDefault(@class => @class.Name.Contains(service.Name, StringComparison.Ordinal) &&
                                                                                   @class.Name.Contains("Extension", StringComparison.Ordinal));

                    var wrongNameRegistration = (from @class in syntaxTree.Classes
                                                 from method in @class.Methods
                                                 where method.IsServiceRegistration() &&
                                                       (method.Body.Contains($"<{service.Name}", StringComparison.Ordinal) || method.Body.Contains($"{service.Name}>", StringComparison.Ordinal))
                                                 select @class).FirstOrDefault();

                    yield return new Service(service, syntaxTree, registration, wrongNameRegistration);
                }
            }
        }
    }

    internal static class ServiceRegistrationExtensions
    {
        /// <summary>An extension method on IServiceCollection, e.g. 'AddUserService(this IServiceCollection services)'.</summary>
        internal static bool IsServiceRegistration(this Method method)
        {
            return method.Parameters.Any(parameter => parameter.Type == "IServiceCollection");
        }
    }
}
