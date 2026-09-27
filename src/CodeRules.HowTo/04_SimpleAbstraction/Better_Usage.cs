using System.Collections.Immutable;
using AspNetCore.Simple.MsTest.Sdk;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeRules.HowTo
{
    [TestClass]
    [TestCategory("HowTo")]
    public class Simple_Abstraction
    {
        private const string Code = """
                                    public record Person
                                    {
                                        public required int Age { get; init; }
                                        public required string City { get; set; }
                                    }
                                    """;

        private static BastaRecord _record = null!;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _record = BastaSyntaxTreeParser.Parse(Code);
        }

        [TestMethod]
        public void Age_Should_Be_Readonly()
        {
            var age = _record.Properties.First(p => p.Name == "Age");

            Assert.That.IsTrue(age.IsReadonly,
                               because: "Properties with 'init' accessor should be detected as readonly to enforce immutability patterns",
                               fix: "Check the BastaSyntaxTreeParser.ParseProperty logic - it should identify 'init' accessors as readonly");
        }

        [TestMethod]
        public void City_Should_Not_Be_Readonly()
        {
            var city = _record.Properties.First(p => p.Name == "City");

            Assert.That.IsFalse(city.IsReadonly,
                                because: "Properties with 'set' accessor should be detected as mutable to allow runtime modifications",
                                fix: "Check the BastaSyntaxTreeParser.ParseProperty logic - it should identify 'set' accessors as not readonly");
        }
    }

    // Parser - isoliert die Syntax-Tree-Logik
    public static class BastaSyntaxTreeParser
    {
        public static BastaRecord Parse(string syntax)
        {
            var tree = CSharpSyntaxTree.ParseText(syntax);
            var root = tree.GetRoot();

            var props = root.DescendantNodes()
                            .OfType<PropertyDeclarationSyntax>()
                            .Select(ParseProperty)
                            .ToImmutableList();

            return new BastaRecord
            {
                Properties = props
            };
        }

        private static BastaProperty ParseProperty(PropertyDeclarationSyntax syntax)
        {
            var name = syntax.Identifier.Text;
            var isReadonly = syntax.AccessorList?.Accessors.All(a => a.Keyword.Text != "set") ?? false;

            return new BastaProperty
            {
                Name = name,
                IsReadonly = isReadonly
            };
        }
    }

    // Daten-Typ - reine Daten, keine Parsing-Logik
    public record BastaRecord
    {
        public required ImmutableList<BastaProperty> Properties { get; init; }
    }

    // Daten-Typ - reine Daten
    public record BastaProperty
    {
        public required string Name { get; init; }

        public required bool IsReadonly { get; init; }
    }
}
