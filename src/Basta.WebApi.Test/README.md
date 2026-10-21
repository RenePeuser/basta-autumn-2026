# Basta.WebApi.Test - API Snapshot Tests 📸

Snapshot-based API tests for `Basta.WebApi`, written with
[AspNetCore.Simple.MsTest.Sdk](https://www.nuget.org/packages/AspNetCore.Simple.MsTest.Sdk).
Every test case exists twice: once with the **classic API** and once with the **fluent API** (new in SDK 10.1).
Both styles run the same pipeline, read the same snapshots and produce the same failure output.

## 📁 Structure

```
Api/Users/V1/Create/BastaShowcase/
├── Basta_Show_Case_Test.cs           ← classic API
├── Basta_Show_Case_Fluent_Test.cs    ← fluent API, same test cases
├── Requests/
│   └── Basta_User.json               ← request body
└── Responses/
    └── Basta_User.json               ← expected response (status, headers, body)
ApiTestBase.cs                        ← starts the WebApi in memory, global SDK settings
```

## 🔥 The same test in both styles

The endpoint under test (`Basta.WebApi`):

```csharp
endpoints.MapPost("users", Handle)
         .Accepts<CreateUserRequest>(MediaTypeNames.Application.Json)
         .Produces<CreateUserResponse>();
```

**Classic API** (`Basta_Show_Case_Test.cs`):

```csharp
[TestMethod]
[DynamicRequestLocator]
public Task Should_Be_Able_To_Create_A_New_User(string useCase)
{
    return Client.AssertPostAsync<CreateUserResponse>("api/v1/users",
                                                      useCase,
                                                      useCase);
}
```

**Fluent API** (`Basta_Show_Case_Fluent_Test.cs`):

```csharp
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
```

The fluent chain reads like the endpoint contract it verifies:

| Endpoint                          | Fluent test                                |
|-----------------------------------|--------------------------------------------|
| `MapPost("users")`                | `AssertPost("api/v1/users")`               |
| `.Accepts<CreateUserRequest>(…)`  | `.AcceptsFromEmbeddedJson(useCase)`        |
| `.Produces<CreateUserResponse>()` | `.Produces<CreateUserResponse>(HttpStatusCode.OK)` |
| response body                     | `.ExpectedResponseFromEmbeddedJson(useCase)` |

What the fluent API adds on top:

- The expected status code is explicit in the test.
- The builder guides you: `Accepts…` only for POST/PUT/PATCH/QUERY and only once, comparison options only after an
  expected response.
- A forgotten `ExecuteAsync()` is a build error (analyzer `MSTESTSDK001`), not a silently green test.
- Precise ignores per test, e.g. `.IgnoreProperty(r => r.User.Id)`.

## ⚙️ How it works

- `[DynamicRequestLocator]` runs the test once per file in `Requests/`; the file name is the `useCase`.
- Request and response files are embedded resources (see `Basta.WebApi.Test.csproj`).
- The response snapshot holds the whole response: status code, content headers and body.
- `ApiTestBase` starts the WebApi in memory (`WebApplicationFactory`) and configures the SDK once for all tests:
  - `DifferenceFunc` ignores every `id`, because the API generates a new `Guid` per call.
  - `WriteResponse = true`: a failing comparison writes the actual response into the snapshot file.
    The test still fails, the next run is green. Review the change in git before you commit it.

## 🚀 Running the tests

```bash
dotnet test src/Basta.WebApi.Test

# AI-friendly output (JSON) instead of the framed human output
TestSdkSettings__OutputMode=ai dotnet test src/Basta.WebApi.Test
```

## ➕ Adding a test case

1. Add `Requests/<UseCase>.json` with the request body.
2. Run the tests. The new case fails once and writes `Responses/<UseCase>.json` (`WriteResponse = true`).
3. Review the written snapshot and commit both files. Both test classes pick the new case up automatically.

A test for an error response (e.g. 400 for an invalid enum value) needs its own test method with its own
`Requests/` folder, because the methods above expect a successful response. The classic API offers
`AssertPostAsErrorAsync<ProblemDetails>(…)`; the fluent error chain is still on the SDK roadmap.

More details in German: **[SNAPSHOT_TESTING.md](./SNAPSHOT_TESTING.md)**.
