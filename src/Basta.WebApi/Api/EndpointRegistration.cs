namespace Basta.WebApi.Api
{
    /// <summary>
    ///     Interface for endpoint registration.
    ///     Each endpoint implements this to define its own mapping behavior.
    /// </summary>
    internal interface IEndpointRegistration
    {
        void Map(IEndpointRouteBuilder versionBasePath);
    }

    /// <summary>
    ///     Registers all endpoints that implement IEndpointRegistration.
    /// </summary>
    internal sealed class RegisterEndpoints(IEnumerable<IEndpointRegistration> endpoints)
    {
        public void MapEndpoints(IEndpointRouteBuilder routeBuilder)
        {
            foreach (var endpoint in endpoints)
            {
                endpoint.Map(routeBuilder);
            }
        }
    }
}
