using System.Text.Json.Serialization;
using Basta.WebApi.Api;
var builder = WebApplication.CreateBuilder(args);

// Register API endpoints and services
builder.Services.AddApi(builder.Configuration);

// Error handling - RFC 7807 ProblemDetails for exceptions and error status codes
builder.Services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
{
    // An unreadable request (invalid JSON, unknown enum value, ...) is the client's fault, tell it what is wrong
    if (context.Exception is BadHttpRequestException badRequest)
    {
        context.ProblemDetails.Detail = badRequest.InnerException?.Message ?? badRequest.Message;
    }
});

// Enums as strings in requests and responses
builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();

// Configure pipeline
// Minimal APIs throw BadHttpRequestException in Development, keep its 4xx status instead of a 500
app.UseExceptionHandler(new ExceptionHandlerOptions
{
    StatusCodeSelector = exception => exception is BadHttpRequestException badRequest ? badRequest.StatusCode : StatusCodes.Status500InternalServerError
});
app.UseStatusCodePages();
app.UseHttpsRedirection();

// Map API endpoints
app.MapApi();

app.Run();

// Needed for integration tests via WebApplicationFactory<Program>
namespace Basta.WebApi
{
#pragma warning disable CA1515
    public class Program;
#pragma warning restore CA1515
}
