using AspNetCore.Simple.MsTest.Sdk;
using Basta.WebApi.Api.User.V1.Create;
// using Basta.WebApi.Api.User.V1.Create;

namespace Basta.WebApi.Test.Api.Users.V1.Create.BastaShowcase
{
    [TestClass]
    [TestCategory("Api")]
    public class Basta_Show_Case_Test : ApiTestBase
    {
        [TestMethod]
        [DynamicRequestLocator]
        public Task Should_Be_Able_To_Create_A_New_User(string useCase)
        {
            return Client.AssertPostAsync<CreateUserResponse>("api/v1/users",
                                                              useCase,
                                                              useCase);
        }
    }

}
