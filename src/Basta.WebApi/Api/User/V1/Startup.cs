using Basta.WebApi.Api.User.V1.Create;

namespace Basta.WebApi.Api.User.V1
{
    internal static class Startup
    {
        internal static void AddUsersV1(this IServiceCollection serviceCollection,
                                        IConfiguration configuration)
        {
            serviceCollection.AddCreateUserEndpoint();
        }
    }
}
