namespace Basta.WebApi.Api.User.V1.Create
{
    public enum UserType
    {
        Visitor,

        Developer,

        Speaker
    }

    public record User(Guid Id,
                       string FirstName,
                       string LastName,
                       string Email,
                       UserType UserType);
}
