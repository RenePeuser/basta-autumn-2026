using AspNetCore.Simple.MsTest.Sdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeRules.HowTo
{
    [TestClass]
    [TestCategory("HowTo")]
    public class Better_Usage
    {
        private const string Code = """
                                    public record Person
                                    {
                                        public required int Age { get; init; }
                                        public required string City { get; set; }
                                    }
                                    """;

        private static SyntaxNode _root = null!;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            var tree = CSharpSyntaxTree.ParseText(Code);
            _root = tree.GetRoot();
        }

        [TestMethod]
        public void How_To_Check_That_Age_Should_Be_Readonly()
        {
            var ageProp = _root.DescendantNodes()
                               .OfType<PropertyDeclarationSyntax>()
                               .FirstOrDefault(p => p.Identifier.Text == "Age");

            Assert.That.IsNotNull(ageProp,
                                  because: "Source code must contain Age property",
                                  fix: "Add Age property to the Person record in the source code");

            Assert.That.IsNotNull(ageProp.AccessorList,
                                  because: "Age property must have accessor list ({ get; init; })",
                                  fix: "Add accessor list to Age property");

            var hasSet = ageProp.AccessorList.Accessors.Any(a => a.Keyword.Text == "set");

            Assert.That.IsFalse(hasSet,
                                because: "Age property is defined with { get; init; } accessor, making it readonly",
                                fix: "Change Age to { get; set; } in the source code if it needs to be mutable");
        }

        [TestMethod]
        public void How_To_Check_That_City_Should_Not_Be_Readonly()
        {
            var cityProp = _root.DescendantNodes()
                                .OfType<PropertyDeclarationSyntax>()
                                .FirstOrDefault(p => p.Identifier.Text == "City");

            Assert.That.IsNotNull(cityProp,
                                  because: "Source code must contain City property",
                                  fix: "Add City property to the Person record in the source code");

            Assert.That.IsNotNull(cityProp.AccessorList,
                                  because: "City property must have accessor list ({ get; set; })",
                                  fix: "Add accessor list to City property");

            // IsReadOnly = hat KEINEN 'set' accessor (init zählt nicht als set)
            var hasSet = cityProp.AccessorList.Accessors
                                 .Any(a => a.Keyword.Text == "set");

            Assert.That.IsTrue(hasSet,
                               because: "City property is defined with { get; set; } accessor, making it mutable",
                               fix: "Change City to { get; init; } in the source code to make it readonly");
        }
    }
}
