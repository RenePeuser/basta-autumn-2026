# Test Project - User API Tests 📸

## 🎯 Snapshot Testing with AspNetCore.Simple.MsTest.Sdk

This test project uses **Snapshot Testing** for clean, maintainable API tests.

**Benefits**:
- ✅ **Visual**: Request/Response JSONs are easy to understand
- ✅ **Simple**: `[DynamicRequestLocator]` = One Test → Many Test Cases
- ✅ **Reviewable**: JSON Snapshots are version-controlled and readable
- ✅ **Fast**: New tests = New JSON files, no C# code needed

## 📁 Structure

```
Api/Users/V1/Create/
├── Status_200_Ok/
│   ├── Create_Status_200_OK_Test.cs          ← Test Code
│   ├── Requests/                              ← Request Snapshots
│   │   ├── CreateRegularUser.json
│   │   ├── CreateAdminUser.json
│   │   └── CreateAiAgentUser.json
│   └── Responses/                             ← Expected Response Snapshots
│       ├── CreateRegularUser.json
│       ├── CreateAdminUser.json
│       └── CreateAiAgentUser.json
│
├── Status_422_UnprocessableContent/
│   ├── Create_Status_422_UnprocessableContent_Test.cs
│   ├── Requests/
│   │   ├── EmptyFirstName.json
│   │   ├── FirstNameTooShort.json
│   │   ├── InvalidEmail.json
│   │   └── MultipleValidationErrors.json     ← Multiple field errors
│   └── Responses/
│       ├── EmptyFirstName.json
│       ├── FirstNameTooShort.json
│       ├── InvalidEmail.json
│       └── MultipleValidationErrors.json     ← Field-level errors
│
├── Status_409_Conflict/
│   ├── Create_Status_409_Conflict_Test.cs
│   ├── Requests/
│   │   └── DuplicateEmail.json
│   └── Responses/
│       └── DuplicateEmail.json
│
├── Status_400_BadRequest/
│   ├── Create_Status_400_BadRequest_Test.cs
│   ├── Requests/
│   │   ├── MissingComma.json                  ← Malformed JSON
│   │   └── MissingClosingBrace.json
│   └── Responses/
│       ├── MissingComma.json
│       └── MissingClosingBrace.json
│
└── BastaShowcase/
    ├── Basta_Show_Case_Test.cs                ← Special demo test
    ├── Requests/
    │   └── Basta.json                         ← Invalid enum value test
    └── Responses/
        └── Basta.json                         ← Expected BadRequest
```

## 🔥 Test Pattern (Simple!)

```csharp
[TestMethod]
[DynamicRequestLocator]  // ← Automatically discovers all Request/Response pairs
public Task What_Is_The_Problem(string useCase)
{
    return Client.AssertPostAsErrorAsync<ProblemDetails>(
        "api/v1/users",
        useCase,    // Request from /Requests/{useCase}.json
        useCase     // Response from /Responses/{useCase}.json
    );
}
```

**That's it!** 🎉 One test method automatically generates tests for:
- ✅ CreateRegularUser
- ✅ CreateAdminUser
- ✅ CreateAiAgentUser

## 📸 Snapshot Example

### Request: `MultipleValidationErrors.json`
```json
{
  "firstName": "J",        // Too short
  "lastName": "",          // Empty
  "email": "bad-email",    // Invalid format
  "userType": 0
}
```

### Expected Response: `MultipleValidationErrors.json`
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.23",
  "title": "One or more validation errors occurred.",
  "status": 422,
  "errors": {
    "FirstName": [
      "First name must be at least 2 characters"
    ],
    "LastName": [
      "Last name is required"
    ],
    "Email": [
      "Email must be a valid email address"
    ]
  }
}
```

This demonstrates field-level validation errors with clear, version-controlled snapshots.

## 🚀 Running Tests

```bash
# All User Tests
dotnet test --filter "TestCategory=Users"

# Success Tests Only
dotnet test --filter "TestCategory~Status 200 OK"

# Validation Tests Only
dotnet test --filter "TestCategory~422"
```

## 📊 Test Coverage

### ✅ Status 200 OK
- CreateRegularUser ✅
- CreateAdminUser ✅
- CreateAiAgentUser ✅

### ✅ Status 422 Unprocessable Content (Field-level Validation)
- EmptyFirstName ✅
- FirstNameTooShort ✅
- InvalidEmail ✅
- MultipleValidationErrors ✅

### ✅ Status 409 Conflict
- DuplicateEmail ✅

### ✅ Status 400 Bad Request (JSON Parsing)
- MissingComma ✅
- MissingClosingBrace ✅

### 🎉 BastaShowcase (Special Tests)
- Basta ✅ (Invalid enum value: "Conference")

## 🔧 Adding New Tests

1. **Create Request**: `Requests/MyTest.json`
2. **Create Response**: `Responses/MyTest.json`
3. **Done!** `[DynamicRequestLocator]` will automatically discover it

Or use `writeResponse: true` to automatically generate the snapshot.

## 📚 Detailed Documentation

See **[SNAPSHOT_TESTING.md](./SNAPSHOT_TESTING.md)** for:
- Placeholders (`{{guid}}`, `{{datetime}}`)
- Snapshot generation
- Best practices
- Troubleshooting

## 💡 Why Snapshot Testing?

### ❌ Traditional Tests
```csharp
Assert.AreEqual("First name is required", errors["FirstName"][0]);
Assert.AreEqual("Last name is required", errors["LastName"][0]);
Assert.AreEqual("Email must be valid", errors["Email"][0]);
// ... 50 lines of assertions
```

### ✅ Snapshot Tests
```json
{
  "status": 422,
  "errors": {
    "FirstName": ["First name is required"],
    "LastName": ["Last name is required"],
    "Email": ["Email must be valid"]
  }
}
```

**Benefits**:
- Immediately understandable
- Both request AND response are visible
- Version-controlled and reviewable
- Easy to add new test cases without code changes
