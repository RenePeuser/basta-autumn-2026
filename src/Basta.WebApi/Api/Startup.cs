using Basta.WebApi.Api.User;

namespace Basta.WebApi.Api
{
    internal static class Startup
    {
        internal static void AddApi(this IServiceCollection services,
                                    IConfiguration configuration)
        {
            // Register endpoint mapper
            services.AddRegisterEndpoints();

            // Register domain endpoints
            services.AddUsers(configuration);
        }

        internal static void MapApi(this WebApplication app)
        {
            var apiBasePath = app.MapGroup("api/v1");
            var registerEndpoints = app.Services.GetRequiredService<RegisterEndpoints>();
            registerEndpoints.MapEndpoints(apiBasePath);
        }
    }
}
