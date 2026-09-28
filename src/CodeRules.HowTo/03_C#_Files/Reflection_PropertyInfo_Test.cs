using AspNetCore.Simple.MsTest.Sdk;
using Extensions.Pack;

namespace CodeRules.HowTo
{
    public record Person
    {
        public required int Age { get; init; }

        public required string City { get; init; }
    }

    [TestClass]
    [TestCategory("HowTo")]
    public class Reflection_PropertyInfo_Test
    {
        [TestMethod]
        public void How_To_Check_That_Age_Should_Be_Readonly()
        {
            var ageProperty = typeof(Person).GetProperty("Age");

            Assert.That.IsNotNull(ageProperty,
                                  because: "Person record must have an Age property",
                                  fix: "Add Age property to Person record");

            // Prüfe: Hat die Property einen public set accessor?
            // (init zählt NICHT als set accessor)
            var isReadOnly = ageProperty.SetMethod.IsNull() ||
                             ageProperty.SetMethod.ReturnParameter
                                        .GetRequiredCustomModifiers()
                                        .Any(m => m.Name == "IsExternalInit");

            Assert.That.IsTrue(isReadOnly,
                               because: "Age property has init-only accessor, which is readonly",
                               fix: "Change Age to { get; init; } if it needs to be immutable");
        }
    }
}
