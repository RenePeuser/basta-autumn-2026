# Assert.That.* Examples

Comprehensive examples for all assertion categories from [AspNetCore.Simple.MsTest.Sdk](https://www.nuget.org/packages/AspNetCore.Simple.MsTest.Sdk).

---

## Boolean Assertions

### IsTrue

**❌ Classic:**
```csharp
Assert.IsTrue(user.IsActive);
```

**✅ Modern:**
```csharp
Assert.That.IsTrue(user.IsActive,
    because: "Active users should have full system access",
    fix: "Activate the user account in the admin panel");
```

**✅ Real-World Example:**
```csharp
Assert.That.IsTrue(order.IsPaid && order.IsShipped,
    because: "Completed orders must be both paid and shipped for reporting",
    fix: "Update order status via OrderService.CompleteOrder(orderId)");
```

### IsFalse

**❌ Classic:**
```csharp
Assert.IsFalse(user.IsDeleted);
```

**✅ Modern:**
```csharp
Assert.That.IsFalse(user.IsDeleted,
    because: "Active users must not be marked as deleted",
    fix: "Restore the user via UserService.Restore(userId)");
```

---

## Null Assertions

### IsNull

**❌ Classic:**
```csharp
Assert.IsNull(result.Error);
```

**✅ Modern:**
```csharp
Assert.That.IsNull(result.Error,
    because: "Successful operations should not have error information",
    fix: "Check the service logic to ensure errors are only set on failures");
```

### IsNotNull

**❌ Classic:**
```csharp
Assert.IsNotNull(user.Email);
```

**✅ Modern:**
```csharp
Assert.That.IsNotNull(user.Email,
    because: "User accounts must have an email address for communication",
    fix: "Ensure email is provided during user creation via UserFactory.Create()");
```

---

## Equality Assertions

### AreEqual

**❌ Classic:**
```csharp
Assert.AreEqual(expected, actual);
```

**✅ Modern:**
```csharp
Assert.That.AreEqual(expectedId, product.Id,
    because: "Product ID must match the requested ID from the database",
    fix: "Verify ProductRepository.GetById() returns the correct record");
```

**✅ Real-World Example:**
```csharp
Assert.That.AreEqual(HttpStatusCode.OK, response.StatusCode,
    because: "API should return 200 OK for successful GET requests",
    fix: "Check the endpoint implementation and error handling logic");
```

### AreNotEqual

**❌ Classic:**
```csharp
Assert.AreNotEqual(userId1, userId2);
```

**✅ Modern:**
```csharp
Assert.That.AreNotEqual(userId1, userId2,
    because: "Test users must have distinct IDs to avoid data conflicts",
    fix: "Use UserFactory.CreateUnique() to generate separate test users");
```

### AreSame (Reference Equality)

**❌ Classic:**
```csharp
Assert.AreSame(expectedInstance, actualInstance);
```

**✅ Modern:**
```csharp
Assert.That.AreSame(singletonInstance, ServiceProvider.GetService<IMyService>(),
    because: "Service must be registered as a singleton to maintain state",
    fix: "Change service registration to services.AddSingleton<IMyService>()");
```

---

## Type Assertions

### IsInstanceOfType

**❌ Classic:**
```csharp
Assert.IsInstanceOfType(result, typeof(SuccessResult));
```

**✅ Modern:**
```csharp
Assert.That.IsInstanceOfType<SuccessResult>(result,
    because: "Successful operations must return SuccessResult type",
    fix: "Update service method to return SuccessResult instead of base Result");
```

**✅ Real-World Example:**
```csharp
Assert.That.IsInstanceOfType<JsonResult>(actionResult,
    because: "API endpoints must return JSON for client consumption",
    fix: "Wrap the response in a JsonResult or use return Json(data)");
```

---

## Numeric Assertions

### IsGreaterThan

**❌ Classic:**
```csharp
Assert.IsTrue(price > 0);
```

**✅ Modern:**
```csharp
Assert.That.IsGreaterThan(price, 0,
    because: "Product prices must be positive for valid transactions",
    fix: "Set a positive price in the product configuration");
```

### IsPositive

**❌ Classic:**
```csharp
Assert.IsTrue(balance > 0);
```

**✅ Modern:**
```csharp
Assert.That.IsPositive(balance,
    because: "Account balance must be positive to allow purchases",
    fix: "Add funds to the test account via PaymentService.AddFunds()");
```

### IsInRange

**❌ Classic:**
```csharp
Assert.IsTrue(age >= 18 && age <= 65);
```

**✅ Modern:**
```csharp
Assert.That.IsInRange(age, 18, 65,
    because: "Eligible users must be between 18 and 65 for this plan",
    fix: "Use a test user within the valid age range");
```

---

## String Assertions

### IsEmpty

**❌ Classic:**
```csharp
Assert.AreEqual("", result.Message);
```

**✅ Modern:**
```csharp
Assert.That.IsEmpty(result.Message,
    because: "Successful operations should not return error messages",
    fix: "Clear the message field in the Result constructor for success cases");
```

### IsNotEmpty

**❌ Classic:**
```csharp
Assert.IsTrue(user.Name.Length > 0);
```

**✅ Modern:**
```csharp
Assert.That.IsNotEmpty(user.Name,
    because: "User profiles must have a non-empty name for display",
    fix: "Provide a name during user creation via UserFactory.Create(name: 'Test User')");
```

### Contains

**❌ Classic:**
```csharp
Assert.IsTrue(email.Contains("@"));
```

**✅ Modern:**
```csharp
Assert.That.Contains(email, "@",
    because: "Email addresses must contain @ symbol for validation",
    fix: "Provide a valid email format like 'user@example.com'");
```

### StartsWith

**❌ Classic:**
```csharp
Assert.IsTrue(url.StartsWith("https://"));
```

**✅ Modern:**
```csharp
Assert.That.StartsWith(url, "https://",
    because: "API endpoints must use HTTPS for secure communication",
    fix: "Update the base URL configuration to use 'https://' prefix");
```

### Matches (Regex)

**❌ Classic:**
```csharp
Assert.IsTrue(Regex.IsMatch(phone, @"^\d{3}-\d{3}-\d{4}$"));
```

**✅ Modern:**
```csharp
Assert.That.Matches(phone, @"^\d{3}-\d{3}-\d{4}$",
    because: "Phone numbers must follow the format XXX-XXX-XXXX for validation",
    fix: "Format the phone number using PhoneFormatter.Format(rawPhone)");
```

---

## Collection Assertions

### IsEmpty

**❌ Classic:**
```csharp
Assert.AreEqual(0, collection.Count);
```

**✅ Modern:**
```csharp
Assert.That.IsEmpty(orders,
    because: "New customers should not have any existing orders",
    fix: "Use a fresh test customer without pre-existing order data");
```

### IsNotEmpty

**❌ Classic:**
```csharp
Assert.IsTrue(products.Count > 0);
```

**✅ Modern:**
```csharp
Assert.That.IsNotEmpty(products,
    because: "Product catalog must contain items for the search functionality to work",
    fix: "Seed test products via ProductFactory.CreateDefaultCatalog()");
```

### Contains

**❌ Classic:**
```csharp
Assert.IsTrue(roles.Contains("Admin"));
```

**✅ Modern:**
```csharp
Assert.That.Contains(roles, "Admin",
    because: "Admin users must have the 'Admin' role assigned",
    fix: "Add role via UserService.AssignRole(userId, 'Admin')");
```

### AllMatch

**❌ Classic:**
```csharp
Assert.IsTrue(products.All(p => p.Price > 0));
```

**✅ Modern:**
```csharp
Assert.That.AllMatch(products, p => p.Price > 0,
    because: "All products in the catalog must have positive prices",
    fix: "Update products with invalid prices via ProductService.UpdatePrice()");
```

---

## Exception Assertions

### Throws

**❌ Classic:**
```csharp
Assert.ThrowsException<ArgumentNullException>(() => service.Process(null));
```

**✅ Modern:**
```csharp
Assert.That.Throws<ArgumentNullException>(() => service.Process(null),
    because: "Service must validate input and fail fast on null arguments",
    fix: "Add null check at the start of Service.Process()");
```

**✅ Real-World Example:**
```csharp
Assert.That.Throws<InvalidOperationException>(() => order.Ship(),
    because: "Orders cannot be shipped before payment is confirmed",
    fix: "Call order.ConfirmPayment() before order.Ship()");
```

### DoesNotThrow

**❌ Classic:**
```csharp
try
{
    service.Execute();
}
catch
{
    Assert.Fail("Should not throw");
}
```

**✅ Modern:**
```csharp
Assert.That.DoesNotThrow(() => service.Execute(),
    because: "Service must handle edge cases gracefully without exceptions",
    fix: "Add null checks and validation in Service.Execute()");
```

---

## DateTime Assertions

### IsAfter

**❌ Classic:**
```csharp
Assert.IsTrue(createdAt > DateTime.UtcNow.AddMinutes(-5));
```

**✅ Modern:**
```csharp
Assert.That.IsAfter(createdAt, DateTime.UtcNow.AddMinutes(-5),
    because: "Entity must be created within the last 5 minutes for this test",
    fix: "Create a fresh test entity instead of using stale data");
```

### IsBefore

**❌ Classic:**
```csharp
Assert.IsTrue(expiresAt < DateTime.UtcNow.AddDays(30));
```

**✅ Modern:**
```csharp
Assert.That.IsBefore(expiresAt, DateTime.UtcNow.AddDays(30),
    because: "Token expiration must be within 30 days for security policy",
    fix: "Adjust token expiration via TokenService.Create(expiresInDays: 30)");
```

### IsUtc

**❌ Classic:**
```csharp
Assert.AreEqual(DateTimeKind.Utc, timestamp.Kind);
```

**✅ Modern:**
```csharp
Assert.That.IsUtc(timestamp,
    because: "Database timestamps must be in UTC for consistent timezone handling",
    fix: "Use DateTime.UtcNow instead of DateTime.Now when setting timestamps");
```

### IsCloseTo

**❌ Classic:**
```csharp
Assert.IsTrue(Math.Abs((createdAt - DateTime.UtcNow).TotalSeconds) < 5);
```

**✅ Modern:**
```csharp
Assert.That.IsCloseTo(createdAt, DateTime.UtcNow, TimeSpan.FromSeconds(5),
    because: "Created timestamp should be approximately now for new entities",
    fix: "Use DateTime.UtcNow when setting CreatedAt in the entity constructor");
```

---

## DateTimeOffset Assertions

### HasOffset

**❌ Classic:**
```csharp
Assert.AreEqual(TimeSpan.Zero, timestamp.Offset);
```

**✅ Modern:**
```csharp
Assert.That.HasOffset(timestamp, TimeSpan.Zero,
    because: "API timestamps must use UTC offset (00:00) for consistency",
    fix: "Use DateTimeOffset.UtcNow instead of DateTimeOffset.Now");
```

### IsUtc (DateTimeOffset)

**❌ Classic:**
```csharp
Assert.AreEqual(TimeSpan.Zero, timestamp.Offset);
```

**✅ Modern:**
```csharp
Assert.That.IsUtc(timestamp,
    because: "Event timestamps must be in UTC for global event processing",
    fix: "Convert to UTC via timestamp.ToUniversalTime() before storing");
```

---

## Complex Real-World Examples

### User Registration Flow

```csharp
[TestMethod]
public async Task Should_Create_Active_User_With_Verified_Email()
{
    // Arrange
    var email = "test@example.com";
    var password = "SecurePass123!";

    // Act
    var user = await _userService.RegisterAsync(email, password);

    // Assert
    Assert.That.IsNotNull(user,
        because: "User service must return a user object for successful registration",
        fix: "Ensure UserService.RegisterAsync() returns the created user");

    Assert.That.AreEqual(email, user.Email,
        because: "Registered user email must match the provided email for login",
        fix: "Check the mapping in UserService.RegisterAsync()");

    Assert.That.IsTrue(user.IsActive,
        because: "Newly registered users should be active by default",
        fix: "Set IsActive = true in User.Create() factory method");

    Assert.That.IsTrue(user.IsEmailVerified,
        because: "Email verification is required before user can access premium features",
        fix: "Call EmailVerificationService.VerifyEmail(user.Id) after registration");
}
```

### Order Processing with Multiple Assertions

```csharp
[TestMethod]
public async Task Should_Process_Order_And_Update_Inventory()
{
    // Arrange
    var product = await _productFactory.CreateAsync(stock: 10);
    var order = await _orderFactory.CreateAsync(productId: product.Id, quantity: 3);

    // Act
    var result = await _orderService.ProcessAsync(order.Id);

    // Assert - Order State
    Assert.That.IsNotNull(result,
        because: "Order service must return processing result",
        fix: "Ensure OrderService.ProcessAsync() returns a Result object");

    Assert.That.IsTrue(result.IsSuccess,
        because: "Order processing should succeed when inventory is available",
        fix: "Check OrderService.ProcessAsync() error handling and inventory validation");

    // Assert - Inventory Update
    var updatedProduct = await _productRepository.GetByIdAsync(product.Id);
    Assert.That.AreEqual(7, updatedProduct.Stock,
        because: "Product stock must decrease by order quantity after processing",
        fix: "Ensure InventoryService.DeductStock() is called during order processing");

    // Assert - Timestamps
    Assert.That.IsUtc(order.ProcessedAt!.Value,
        because: "Order timestamps must be in UTC for consistent reporting across timezones",
        fix: "Use DateTime.UtcNow when setting ProcessedAt in OrderService");

    Assert.That.IsAfter(order.ProcessedAt!.Value, order.CreatedAt,
        because: "Order must be processed after it was created",
        fix: "Ensure CreatedAt is set before ProcessedAt in the order lifecycle");
}
```

### API Response Validation

```csharp
[TestMethod]
public async Task Should_Return_Proper_API_Response_For_Valid_Request()
{
    // Arrange
    var request = new CreateCapabilityRequest<CreateProductProperties>
    {
        Type = "AWS_PRODUCT",
        Name = "Test Product",
        Properties = new CreateProductProperties { Price = 29.99m }
    };

    // Act
    var response = await _apiClient.PostAsync("/api/products", request);
    var content = await response.Content.ReadAsStringAsync();
    var product = JsonSerializer.Deserialize<Product>(content);

    // Assert - HTTP Status
    Assert.That.AreEqual(HttpStatusCode.Created, response.StatusCode,
        because: "API should return 201 Created for successful POST requests",
        fix: "Return CreatedAtAction() in the controller instead of Ok()");

    // Assert - Response Body
    Assert.That.IsNotNull(product,
        because: "API must return the created product in the response body",
        fix: "Ensure the controller returns the created entity");

    Assert.That.IsNotEmpty(product.Id,
        because: "Created products must have a generated ID",
        fix: "Set product.Id in ProductService.CreateAsync() before saving");

    Assert.That.AreEqual(request.Name, product.Name,
        because: "Product name in response must match the request",
        fix: "Check the mapping logic in ProductService.CreateAsync()");

    Assert.That.IsPositive(product.Price,
        because: "Product prices must be positive for valid transactions",
        fix: "Add price validation in CreateProductProperties or ProductService");
}
```

---

## Migration Checklist

When migrating existing tests:

1. **Find classic assertions:**
   ```bash
   /check-asserts
   ```

2. **Replace with Assert.That.***
   - Use the most specific assertion method
   - Add `because:` with business context
   - Add `fix:` with actionable resolution steps

3. **Review context quality:**
   - Avoid generic phrases
   - Explain *why* the assertion matters
   - Provide *how* to resolve failures

4. **Run tests to verify:**
   ```bash
   dotnet test
   ```

5. **Commit with clear message:**
   ```bash
   git commit -m "test: upgrade assertions to Assert.That.* with context"
   ```

---

## Reference

Package: [AspNetCore.Simple.MsTest.Sdk](https://www.nuget.org/packages/AspNetCore.Simple.MsTest.Sdk)

Full assertion categories:
- Boolean: `IsTrue`, `IsFalse`
- Null: `IsNull`, `IsNotNull`
- Equality: `AreEqual`, `AreNotEqual`, `AreSame`, `AreNotSame`
- Type: `IsInstanceOfType`, `IsNotInstanceOfType`
- Numeric: `IsGreaterThan`, `IsLessThan`, `IsInRange`, `IsPositive`, `IsNegative`
- String: `IsEmpty`, `IsNotEmpty`, `Contains`, `StartsWith`, `EndsWith`, `Matches`
- Collection: `IsEmpty`, `IsNotEmpty`, `Contains`, `DoesNotContain`, `AllMatch`
- Exception: `Throws`, `DoesNotThrow`
- DateTime: `IsAfter`, `IsBefore`, `IsInRange`, `IsCloseTo`, `IsUtc`, `IsLocal`, `IsUnspecified`
- DateTimeOffset: `IsAfter`, `IsBefore`, `IsInRange`, `IsCloseTo`, `HasOffset`, `IsUtc`, `IsLocal`
