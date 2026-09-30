using System.Collections.Immutable;

namespace Basta.CodeRules
{
    [TestCategory("CodeRules")]
    [TestCategory("CodeRules Services")]
    [TestClass]
    public class SERVICES_RULE_003 : ServiceCodeRuleBase
    {
        // Collection types Microsoft.Extensions.DependencyInjection cannot resolve as "all implementations of T"
        private static readonly ImmutableHashSet<string> UnsupportedCollectionTypes =
        [
            "List", "IList", "IReadOnlyList", "ICollection", "IReadOnlyCollection", "Collection", "ReadOnlyCollection", "ObservableCollection",
            "HashSet", "ISet", "IReadOnlySet", "SortedSet", "LinkedList", "Queue", "Stack",
            "ImmutableList", "IImmutableList", "ImmutableArray", "ImmutableHashSet", "IImmutableSet", "ImmutableSortedSet", "ImmutableQueue", "ImmutableStack",
            "FrozenSet"
        ];

        [TestMethod]
        public void Service_Constructors_Have_To_Use_IEnumerable_To_Inject_Multiple_Implementations()
        {
            // Parameters = primary constructor, Constructors = classic constructors
            var findings = from service in Services
                           from parameter in service.Class.Parameters.Concat(service.Class.Constructors.Where(constructor => !constructor.IsPrimary).SelectMany(constructor => constructor.Parameters))
                           let collectionType = CollectionType(parameter.Type)
                           where collectionType is not null
                           let elementType = ElementType(parameter.Type)
                           select new CodeRuleFinding
                           {
                               Subject = $"{service.Class.FullQualifiedName}({parameter.Type} {parameter.Name})",
                               Location = service.Class.Location,
                               Current = $"{parameter.Type} {parameter.Name}",
                               Suggested = $"IEnumerable<{elementType}> {parameter.Name}",
                               Fix = $"Inject 'IEnumerable<{elementType}>' and convert it inside '{service.Class.Name}' if you need a '{collectionType}'.",
                               DocumentationUrl = "https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection#service-registration-methods"
                           };

            Assert.That.CodeRuleHasNoFindings(findings,
                                              rule: "SERVICES_RULE_003",
                                              title: "Inject multiple implementations as IEnumerable<T>",
                                              because: "Microsoft.Extensions.DependencyInjection resolves all registered implementations of T only as IEnumerable<T>. " +
                                                       "Any other collection type in a constructor fails at runtime with 'Unable to resolve service'.",
                                              fix: "Replace the collection type in the constructor with IEnumerable<T>.");

            return;

            static string? CollectionType(string type)
            {
                if (type.EndsWith("[]", StringComparison.Ordinal))
                {
                    return "array";
                }

                var genericStart = type.IndexOf('<', StringComparison.Ordinal);
                var outerType = genericStart < 0 ? type : type[..genericStart];

                return UnsupportedCollectionTypes.Contains(outerType) ? outerType : null;
            }

            static string ElementType(string type)
            {
                if (type.EndsWith("[]", StringComparison.Ordinal))
                {
                    return type[..^2];
                }

                var genericStart = type.IndexOf('<', StringComparison.Ordinal);

                return genericStart < 0 ? "T" : type[(genericStart + 1)..^1];
            }
        }
    }
}
