using Basta.WebApi.Api;
using Siemens.AspNet.ErrorHandling;
using Siemens.AspNet.MinimalApi.Sdk;
var builder = WebApplication.CreateBuilder(args);

// Register API endpoints and services
builder.Services.AddApi(builder.Configuration);

// Error handling - Enterprise ready
builder.Services.AddErrorHandling(builder.Configuration);

// Common services
builder.Services.AddValidation();
builder.Services.AddJsonSerializeOptions();
builder.Services.AddAllowedQueryParameter();

var app = builder.Build();

// Configure pipeline
app.UseErrorHandling();
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
