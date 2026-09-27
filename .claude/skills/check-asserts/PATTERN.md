# Assert Quality Pattern

## Core Principle

**Every assertion must provide structured context for humans and AI debugging.**

Use `Assert.That.*` from `AspNetCore.Simple.MsTest.Sdk` with mandatory `because` and `fix` parameters.

---

## Rules

### 1. Never Use Classic MSTest Assertions

❌ **Avoid:**
```csharp
Assert.IsTrue(user.IsActive);
Assert.AreEqual(expected, actual);
Assert.IsNotNull(result);
Assert.ThrowsException<InvalidOperationException>(() => service.Execute());
```

✅ **Use:**
```csharp
Assert.That.IsTrue(user.IsActive,
    because: "Active users should have full system access",
    fix: "Activate the user account in the admin panel");

Assert.That.AreEqual(expected, actual,
    because: "API response must match the expected schema",
    fix: "Check the serialization logic in ResponseMapper");

Assert.That.IsNotNull(result,
    because: "Service must return a result for valid input",
    fix: "Ensure the service doesn't return null for this case");

Assert.That.Throws<InvalidOperationException>(() => service.Execute(),
    because: "Executing without initialization must fail fast",
    fix: "Call service.Initialize() before Execute()");
```

---

### 2. Provide Meaningful Context

The `because` parameter should answer **"Why does this assertion matter?"**

❌ **Poor Context:**
```csharp
because: "It should be true"           // Just repeats the assertion
because: "Value should not be null"    // Obvious from the method name
because: "Expected value"              // Too generic
```

✅ **Good Context:**
```csharp
because: "Active users require verified email for security compliance"
because: "Customer records must include billing address for invoice generation"
because: "Configuration file must exist before service initialization"
```

**Guidelines for `because`:**
- Explain the business or technical reason
- Reference domain concepts, not just code
- Help future developers understand the requirement

---

### 3. Provide Actionable Fixes

The `fix` parameter should answer **"How do I resolve this if it fails?"**

❌ **Poor Fix:**
```csharp
fix: "Fix it"                          // Not actionable
fix: "Check the value"                 // Too vague
fix: "Make it work"                    // Useless
```

✅ **Good Fix:**
```csharp
fix: "Run the migration script: dotnet ef database update"
fix: "Set the 'EMAIL_VERIFIED' flag in the user profile"
fix: "Ensure CustomerService.Create() includes BillingAddress"
```

**Guidelines for `fix`:**
- Provide specific debugging steps
- Reference methods, services, or configuration
- Include commands or actions to resolve the issue

---

### 4. Choose the Right Assertion Category

Use the most specific assertion method for your scenario.

#### Boolean Assertions
```csharp
Assert.That.IsTrue(condition, because:, fix:)
Assert.That.IsFalse(condition, because:, fix:)
```

#### Null Checks
```csharp
Assert.That.IsNull(value, because:, fix:)
Assert.That.IsNotNull(value, because:, fix:)
```

#### Equality
```csharp
Assert.That.AreEqual(expected, actual, because:, fix:)
Assert.That.AreNotEqual(expected, actual, because:, fix:)
Assert.That.AreSame(expected, actual, because:, fix:)     // Reference equality
Assert.That.AreNotSame(expected, actual, because:, fix:)
```

#### Type Checks
```csharp
Assert.That.IsInstanceOfType<TExpected>(obj, because:, fix:)
Assert.That.IsNotInstanceOfType<TUnexpected>(obj, because:, fix:)
```

#### Numeric Assertions
```csharp
Assert.That.IsGreaterThan(value, threshold, because:, fix:)
Assert.That.IsLessThan(value, threshold, because:, fix:)
Assert.That.IsInRange(value, min, max, because:, fix:)
Assert.That.IsPositive(value, because:, fix:)
Assert.That.IsNegative(value, because:, fix:)
```

#### String Assertions
```csharp
Assert.That.IsEmpty(str, because:, fix:)
Assert.That.IsNotEmpty(str, because:, fix:)
Assert.That.Contains(str, substring, because:, fix:)
Assert.That.StartsWith(str, prefix, because:, fix:)
Assert.That.EndsWith(str, suffix, because:, fix:)
Assert.That.Matches(str, regexPattern, because:, fix:)
```

#### Collection Assertions
```csharp
Assert.That.IsEmpty(collection, because:, fix:)
Assert.That.IsNotEmpty(collection, because:, fix:)
Assert.That.Contains(collection, item, because:, fix:)
Assert.That.DoesNotContain(collection, item, because:, fix:)
Assert.That.AllMatch(collection, predicate, because:, fix:)
```

#### Exception Assertions
```csharp
Assert.That.Throws<TException>(action, because:, fix:)
Assert.That.DoesNotThrow(action, because:, fix:)
```

#### DateTime Assertions
```csharp
Assert.That.IsAfter(dateTime, threshold, because:, fix:)
Assert.That.IsBefore(dateTime, threshold, because:, fix:)
Assert.That.IsInRange(dateTime, start, end, because:, fix:)
Assert.That.IsCloseTo(dateTime, expected, tolerance, because:, fix:)
Assert.That.IsUtc(dateTime, because:, fix:)
Assert.That.IsLocal(dateTime, because:, fix:)
Assert.That.IsUnspecified(dateTime, because:, fix:)
```

#### DateTimeOffset Assertions
```csharp
Assert.That.IsAfter(dateTimeOffset, threshold, because:, fix:)
Assert.That.IsBefore(dateTimeOffset, threshold, because:, fix:)
Assert.That.IsInRange(dateTimeOffset, start, end, because:, fix:)
Assert.That.IsCloseTo(dateTimeOffset, expected, tolerance, because:, fix:)
Assert.That.HasOffset(dateTimeOffset, expectedOffset, because:, fix:)
Assert.That.IsUtc(dateTimeOffset, because:, fix:)
Assert.That.IsLocal(dateTimeOffset, because:, fix:)
```

---

### 5. Prefer Specific Assertions Over Generic Ones

❌ **Too Generic:**
```csharp
Assert.That.IsTrue(count > 0,
    because: "Collection should have items",
    fix: "Add items to the collection");
```

✅ **More Specific:**
```csharp
Assert.That.IsNotEmpty(collection,
    because: "Collection should have items",
    fix: "Add items to the collection");
```

**Why?** Specific assertions provide better failure messages and context.

---

## Migration Guide

### Step 1: Identify Classic Assertions

Run the skill to find all classic assertions:
```bash
/check-asserts
```

### Step 2: Replace with Assert.That.*

For each classic assertion, determine the appropriate `Assert.That.*` method and add context.

### Step 3: Write Meaningful Context

Ask yourself:
- **Why** does this assertion matter? → `because` parameter
- **How** do I fix it if it fails? → `fix` parameter

### Step 4: Review Context Quality

Ensure context is:
- Specific (not generic)
- Actionable (not vague)
- Business/domain-focused (not just code)

---

## Code Review Checklist

When reviewing test code:

- [ ] All assertions use `Assert.That.*` (not classic `Assert.*`)
- [ ] Every assertion has a `because:` parameter
- [ ] Every assertion has a `fix:` parameter
- [ ] `because` explains the business/technical reason
- [ ] `fix` provides specific debugging steps
- [ ] The most specific assertion method is used
- [ ] Context is meaningful, not generic

---

## Examples by Category

### Boolean Assertions
```csharp
// ❌ Classic
Assert.IsTrue(user.IsActive);

// ✅ Modern
Assert.That.IsTrue(user.IsActive,
    because: "Active users should have full system access",
    fix: "Activate the user account via UserService.Activate(userId)");
```

### Null Checks
```csharp
// ❌ Classic
Assert.IsNotNull(result);

// ✅ Modern
Assert.That.IsNotNull(result,
    because: "Service must return a result for valid input",
    fix: "Ensure the service doesn't return null for this case");
```

### Equality
```csharp
// ❌ Classic
Assert.AreEqual(expectedId, product.Id);

// ✅ Modern
Assert.That.AreEqual(expectedId, product.Id,
    because: "Product ID must match the requested ID from the database",
    fix: "Verify the ProductRepository.GetById() query returns the correct record");
```

### Collections
```csharp
// ❌ Classic
Assert.IsTrue(orders.Count > 0);

// ✅ Modern
Assert.That.IsNotEmpty(orders,
    because: "Customer must have at least one order for this test scenario",
    fix: "Create test orders via OrderFactory.CreateDefaultOrders()");
```

### Exceptions
```csharp
// ❌ Classic
Assert.ThrowsException<ArgumentNullException>(() => service.Process(null));

// ✅ Modern
Assert.That.Throws<ArgumentNullException>(() => service.Process(null),
    because: "Service must validate input and fail fast on null arguments",
    fix: "Add null check at the start of Service.Process()");
```

### Numeric Assertions
```csharp
// ❌ Classic
Assert.IsTrue(price > 0);

// ✅ Modern
Assert.That.IsPositive(price,
    because: "Product prices must be positive for valid transactions",
    fix: "Set a positive price in the product configuration");
```

### String Assertions
```csharp
// ❌ Classic
Assert.IsTrue(email.Contains("@"));

// ✅ Modern
Assert.That.Contains(email, "@",
    because: "Email addresses must contain @ symbol for validation",
    fix: "Provide a valid email format like 'user@example.com'");
```

### DateTime Assertions
```csharp
// ❌ Classic
Assert.IsTrue(createdAt.Kind == DateTimeKind.Utc);

// ✅ Modern
Assert.That.IsUtc(createdAt,
    because: "Database timestamps must be in UTC for consistent timezone handling",
    fix: "Use DateTime.UtcNow when setting CreatedAt timestamps");
```

---

## Anti-Patterns to Avoid

### 1. Repeating the Assertion in Context
❌ **Bad:**
```csharp
Assert.That.IsTrue(condition,
    because: "Condition should be true",
    fix: "Make it true");
```

✅ **Good:**
```csharp
Assert.That.IsTrue(user.IsVerified,
    because: "Verified users can access premium features",
    fix: "Complete email verification via VerificationService.Verify(token)");
```

### 2. Vague or Generic Fixes
❌ **Bad:**
```csharp
fix: "Check the value"
fix: "Fix the bug"
fix: "Make sure it's correct"
```

✅ **Good:**
```csharp
fix: "Set the 'ApiKey' in appsettings.json"
fix: "Run migration: dotnet ef database update"
fix: "Initialize the service via DependencyInjection.AddMyService()"
```

### 3. Missing Business Context
❌ **Bad:**
```csharp
because: "The ID should match"
```

✅ **Good:**
```csharp
because: "Order ID must match the database record to ensure data consistency in the audit log"
```

---

## Benefits of Assert.That.*

### For Humans
- **Clear failure context**: Understand why a test failed without digging through code
- **Actionable guidance**: Know exactly how to fix the issue
- **Better test documentation**: Context explains business requirements

### For AI Tools
- **Structured data**: JSON serialization enables automated analysis
- **Debuggable failures**: AI can suggest fixes based on structured context
- **Test intent clarity**: `because` explains the requirement, not just the code

### For Teams
- **Faster debugging**: New team members understand failures immediately
- **Reduced cognitive load**: No need to reverse-engineer test intent
- **Better code reviews**: Context makes assertions self-documenting

---

## Reference

Package: [AspNetCore.Simple.MsTest.Sdk](https://www.nuget.org/packages/AspNetCore.Simple.MsTest.Sdk)

All `Assert.That.*` methods follow this signature:
```csharp
Assert.That.Method(args...,
    because: "Why this assertion matters",
    fix: "How to resolve if it fails");
```
