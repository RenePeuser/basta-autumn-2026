using System.Net;
using AspNetCore.Simple.MsTest.Sdk;
using Basta.WebApi.Api.User.V1.Create;

namespace Basta.WebApi.Test.Api.Users.V1.Create.BastaShowcase
{
    /// <summary>
    ///     Same test cases as <see cref="Basta_Show_Case_Test" />, written with the fluent API.
    ///     Both share the Requests/ and Responses/ snapshots of this folder.
    /// </summary>
    [TestClass]
    [TestCategory("Api")]
    public class Basta_Show_Case_Fluent_Test : ApiTestBase
    {
        [TestMethod]
        [DynamicRequestLocator]
        public Task Should_Be_Able_To_Create_A_New_User(string useCase)
        {
            return Client.AssertPost("api/v1/users")
                         .AcceptsFromEmbeddedJson(useCase)
                         .Produces<CreateUserResponse>(HttpStatusCode.OK)
                         .ExpectedResponseFromEmbeddedJson(useCase)
                         .ExecuteAsync();
        }
    }
}
