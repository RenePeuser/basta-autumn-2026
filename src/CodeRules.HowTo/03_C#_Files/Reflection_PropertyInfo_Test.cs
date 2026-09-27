using AspNetCore.Simple.MsTest.Sdk;

namespace CodeRules.HowTo
{
    public record Person
    {
        public required int Age { get; init; }

        public required string City { get; set; }
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
            var isReadOnly = ageProperty.SetMethod?.IsPublic != true ||
                             ageProperty.SetMethod.ReturnParameter
                                        .GetRequiredCustomModifiers()
                                        .Any(m => m.Name == "IsExternalInit");

            Assert.That.IsTrue(isReadOnly,
                               because: "Age property has init-only accessor, which is readonly",
                               fix: "Change Age to { get; init; } if it needs to be immutable");
        }

        [TestMethod]
        public void How_To_Check_That_City_Should_Not_Be_Readonly()
        {
            var cityProperty = typeof(Person).GetProperty("City");

            Assert.That.IsNotNull(cityProperty,
                                  because: "Person record must have a City property",
                                  fix: "Add City property to Person record");

            // Prüfe: Hat die Property einen public set accessor?
            var isReadOnly = cityProperty.SetMethod?.IsPublic != true ||
                             cityProperty.SetMethod.ReturnParameter
                                         .GetRequiredCustomModifiers()
                                         .Any(m => m.Name == "IsExternalInit");

            Assert.That.IsFalse(isReadOnly,
                                because: "City property has get; set; accessor, which is mutable",
                                fix: "Change City to { get; init; } to make it readonly");
        }
    }
}
