using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Siemens.AspNet.ErrorHandling.Contracts;

namespace Basta.WebApi.Api.User.V1.Create
{
    internal static class AddCreateUserEndpointExtension
    {
        internal static void AddCreateUserEndpoint(this IServiceCollection services)
        {
            services.AddSingleton<IEndpointRegistration, CreateUserEndpoint>();
        }
    }

    internal sealed class CreateUserEndpoint : IEndpointRegistration
    {
        public void Map(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPost("users", HandleAsync)
                     .Accepts<CreateUserRequest>(MediaTypeNames.Application.Json)
                     .Produces<CreateUserResponse>()
                     .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
                     .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
                     .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
                     .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
                     .Produces<ProblemDetails>(StatusCodes.Status409Conflict)
                     .Produces<ValidationProblemDetailsExtended>(StatusCodes.Status422UnprocessableEntity)
                     .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
                     .Produces<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)
                     .WithTags("Users")
                     .WithName("CreateUserV1");

            static CreateUserResponse HandleAsync(CreateUserRequest createUserRequest,
                                                  HttpContext httpContext,
                                                  CancellationToken cancellationToken = default)
            {
                var user = new User(Guid.NewGuid(),
                                    createUserRequest.FirstName,
                                    createUserRequest.LastName,
                                    createUserRequest.Email,
                                    createUserRequest.UserType);

                var response = new CreateUserResponse(user);

                return response;
            }
        }
    }
}
