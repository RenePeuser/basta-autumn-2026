namespace Basta.WebApi.Api.User.V1.Create
{
    public record CreateUserRequest
    {
        public required string FirstName { get; init; }

        public required string LastName { get; init; }

        public required string Email { get; init; }

        public required UserType UserType { get; init; }
    }
}
