# Snapshot Testing mit AspNetCore.Simple.MsTest.Sdk 📸

## 🎯 Konzept

Wir nutzen [AspNetCore.Simple.MsTest.Sdk](https://www.nuget.org/packages/AspNetCore.Simple.MsTest.Sdk) für sauberes Request/Response Snapshot Testing.

**Vorteile**:
- ✅ Klare Trennung: Request in `/Requests`, Response in `/Responses`
- ✅ JSON-Snapshots sind versionierbar und reviewbar
- ✅ Automatische Snapshot-Vergleiche
- ✅ `DynamicRequestLocator` findet automatisch alle Test-Cases
- ✅ `writeResponse: true` generiert Snapshots automatisch

## 📁 Ordnerstruktur

```
Api/Users/V1/Create/
├── Status_200_Ok/
│   ├── Create_Status_200_OK_Test.cs
│   ├── Requests/
│   │   ├── CreateRegularUser.json
│   │   ├── CreateAdminUser.json
│   │   └── CreateAiAgentUser.json
│   └── Responses/
│       ├── CreateRegularUser.json
│       ├── CreateAdminUser.json
│       └── CreateAiAgentUser.json
├── Status_422_UnprocessableContent/
│   ├── Create_Status_422_UnprocessableContent_Test.cs
│   ├── Requests/
│   │   ├── EmptyFirstName.json
│   │   ├── FirstNameTooShort.json
│   │   ├── InvalidEmail.json
│   │   └── MultipleValidationErrors.json
│   └── Responses/
│       ├── EmptyFirstName.json
│       ├── FirstNameTooShort.json
│       ├── InvalidEmail.json
│       └── MultipleValidationErrors.json
└── Status_409_Conflict/
    ├── Create_Status_409_Conflict_Test.cs
    ├── Requests/
    │   └── DuplicateEmail.json
    └── Responses/
        └── DuplicateEmail.json
```

## 🔥 Pattern 1: DynamicRequestLocator (Empfohlen für Demo)

Der `[DynamicRequestLocator]` Attribut sucht automatisch alle Request/Response Paare:

```csharp
[TestMethod]
[DynamicRequestLocator]
public Task Should_Create_User_With(string useCase)
{
    return Client.AssertPostAsync<CreateUserResponse>("api/v1/users",
                                                      useCase,
                                                      useCase);
}
```

**Was passiert:**
1. SDK findet alle `.json` Files im `/Requests` Ordner
2. Für jeden Request (z.B. `CreateRegularUser.json`):
   - Sendet POST mit Request-Body
   - Vergleicht Response mit `/Responses/CreateRegularUser.json`
   - Test heißt automatisch: `Should_Create_User_With("CreateRegularUser")`

**Resultat**: Ein Test-Method → Mehrere Test-Cases automatisch! 🎉

## 📝 Pattern 2: Explizite Tests

Für spezielle Assertions oder dynamische Daten:

```csharp
[TestMethod]
public async Task Should_Create_User_And_Return_Generated_Id()
{
    // Arrange
    var request = new CreateUserRequest
    {
        FirstName = "John",
        LastName = "Doe",
        Email = $"test-{Guid.NewGuid()}@example.com",
        UserType = UserType.Regular
    };

    // Act
    var response = await Client.PostAsAsync<CreateUserResponse>("api/v1/users", request);

    // Assert
    Assert.IsNotNull(response);
    Assert.AreNotEqual(Guid.Empty, response.User.Id);
    Assert.AreEqual(request.FirstName, response.User.FirstName);
}
```

## 📸 Snapshots Erstellen

### Option 1: Manuell erstellen (Empfohlen für Demo)

1. **Request erstellen**: `Requests/MyTestCase.json`
```json
{
  "firstName": "Jane",
  "lastName": "Doe",
  "email": "jane@example.com",
  "userType": 0
}
```

2. **Response erstellen**: `Responses/MyTestCase.json`
```json
{
  "user": {
    "id": "{{guid}}",
    "firstName": "Jane",
    "lastName": "Doe",
    "email": "jane@example.com",
    "userType": 0
  }
}
```

**Platzhalter**:
- `{{guid}}` - Wird gegen beliebige GUID validiert
- `{{datetime}}` - Wird gegen beliebiges DateTime validiert
- `{{any}}` - Wird ignoriert (beliebiger Wert)

### Option 2: Automatisch generieren

```csharp
await Client.AssertPostAsync<CreateUserResponse>("api/v1/users",
                                                 "MyTestCase",
                                                 "MyTestCase",
                                                 writeResponse: true);  // <-- Generiert Snapshot!
```

## 🎬 Demo Use Cases

### Success Case (200 OK)

**Request**: `CreateRegularUser.json`
```json
{
  "firstName": "Jane",
  "lastName": "Doe",
  "email": "jane.doe@example.com",
  "userType": 0
}
```

**Response**: `CreateRegularUser.json`
```json
{
  "user": {
    "id": "{{guid}}",
    "firstName": "Jane",
    "lastName": "Doe",
    "email": "jane.doe@example.com",
    "userType": 0
  }
}
```

### Validation Error (422)

**Request**: `MultipleValidationErrors.json`
```json
{
  "firstName": "J",
  "lastName": "",
  "email": "bad-email",
  "userType": 0
}
```

**Response**: `MultipleValidationErrors.json`
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

### Conflict (409)

**Request**: `DuplicateEmail.json`
```json
{
  "firstName": "John",
  "lastName": "Doe",
  "email": "fixed-duplicate@example.com",
  "userType": 0
}
```

**Response**: `DuplicateEmail.json`
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.10",
  "title": "Conflict",
  "status": 409,
  "detail": "User with email 'fixed-duplicate@example.com' already exists"
}
```

## 🚀 Tests Ausführen

```bash
# Alle Tests
dotnet test

# Nur User Tests
dotnet test --filter "TestCategory=Users"

# Nur 422 Tests
dotnet test --filter "TestCategory=Users V1 Create Status 422 UnprocessableContent"

# Einzelner Test
dotnet test --filter "FullyQualifiedName~Should_Create_User_With"
```

## 🎯 Für Conference Demo

### Zeige im Präsentations:

1. **Ordnerstruktur** (5 Sekunden)
   ```
   Status_422_UnprocessableContent/
   ├── Requests/
   │   └── MultipleValidationErrors.json
   └── Responses/
       └── MultipleValidationErrors.json
   ```

2. **Request Snapshot** (10 Sekunden)
   ```json
   {
     "firstName": "J",
     "lastName": "",
     "email": "bad-email",
     "userType": 0
   }
   ```

3. **Expected Response** (15 Sekunden)
   ```json
   {
     "status": 422,
     "errors": {
       "FirstName": ["First name must be at least 2 characters"],
       "LastName": ["Last name is required"],
       "Email": ["Email must be a valid email address"]
     }
   }
   ```

4. **Simple Test Code** (10 Sekunden)
   ```csharp
   [TestMethod]
   [DynamicRequestLocator]
   public Task Should_Return_422_When_Request_Contains(string useCase)
   {
       return Client.AssertPostAsync<ValidationProblemDetails>(
           "api/v1/users",
           useCase,
           useCase
       );
   }
   ```

5. **Run Tests** → Grün! ✅

**Talk Track**:
> "Seht ihr? Ein Test-Method, aber mehrere Test-Cases automatisch. Request und Response sind klar getrennt, versioniert, und reviewbar. Das ist perfekt für API-Entwicklung!"

## 💡 Best Practices

### ✅ DO

- Nutze beschreibende Dateinamen: `MultipleValidationErrors.json` statt `Test1.json`
- Verwende `{{guid}}` für dynamische IDs
- Gruppiere ähnliche Tests in Ordner (Status_200_Ok, Status_422, etc.)
- Committe Snapshots ins Git

### ❌ DON'T

- Keine sensiblen Daten in Snapshots
- Keine großen Responses (> 100 lines) - splitten!
- Keine Timestamps ohne `{{datetime}}` Platzhalter

## 🔧 Troubleshooting

### Test schlägt fehl: "Response doesn't match snapshot"

1. Prüfe Response-Snapshot: Ist er aktuell?
2. Hat sich die API geändert?
3. Snapshot neu generieren: `writeResponse: true`

### "Could not find request file"

- Dateiname muss genau `useCase` Parameter entsprechen
- Case-sensitive!
- Pfad: `<TestFolder>/Requests/<useCase>.json`

### Tests hängen / timeout

- API läuft nicht?
- Port falsch?
- `dotnet run` im anderen Terminal vergessen?

## 📚 Weitere Infos

- [AspNetCore.Simple.MsTest.Sdk Docs](https://www.nuget.org/packages/AspNetCore.Simple.MsTest.Sdk#readme-body-tab)
- [RFC 7807 - Problem Details](https://tools.ietf.org/html/rfc7807)
- [RFC 9110 - HTTP Status Codes](https://www.rfc-editor.org/rfc/rfc9110)
