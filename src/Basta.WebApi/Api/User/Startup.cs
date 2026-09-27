using Basta.WebApi.Api.User.V1;

namespace Basta.WebApi.Api.User
{
    internal static class Startup
    {
        internal static void AddUsers(this IServiceCollection services,
                                      IConfiguration configuration)
        {
            services.AddUsersV1(configuration);
        }
    }
}
