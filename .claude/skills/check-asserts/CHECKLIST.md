# Assert Quality Review Checklist

Use this checklist when reviewing test code or migrating to [AspNetCore.Simple.MsTest.Sdk](https://www.nuget.org/packages/AspNetCore.Simple.MsTest.Sdk) assertions.

---

## Pre-Review Setup

- [ ] Run `/check-asserts` to generate initial analysis report
- [ ] Review the summary statistics (classic vs modern assertions)
- [ ] Prioritize files with the most classic assertions

---

## Per-Assertion Review

For each assertion in the test file:

### 1. Assertion Type

- [ ] Uses `Assert.That.*` (not classic `Assert.*` like `Assert.IsTrue()`)
- [ ] Uses the most specific assertion method available:
  - `IsPositive()` instead of `IsTrue(value > 0)`
  - `IsNotEmpty()` instead of `IsTrue(collection.Count > 0)`
  - `Contains()` instead of `IsTrue(string.Contains(...))`

### 2. Context Quality - `because` Parameter

- [ ] Has a `because:` parameter
- [ ] Explains the **business or technical reason** for the assertion
- [ ] Is **specific** (not generic like "should be true")
- [ ] References domain concepts (not just code structure)
- [ ] Helps future developers understand the requirement

**Bad `because` examples:**
- ❌ `"It should be true"`
- ❌ `"Value should not be null"`
- ❌ `"Expected value"`

**Good `because` examples:**
- ✅ `"Active users require verified email for security compliance"`
- ✅ `"Orders must have a customer for billing purposes"`
- ✅ `"Configuration file must exist before service initialization"`

### 3. Fix Quality - `fix` Parameter

- [ ] Has a `fix:` parameter
- [ ] Provides **specific debugging steps** or resolution actions
- [ ] References methods, services, or configuration when applicable
- [ ] Is **actionable** (not vague like "check the value")
- [ ] Helps developers quickly resolve the failure

**Bad `fix` examples:**
- ❌ `"Fix it"`
- ❌ `"Check the value"`
- ❌ `"Make it work"`

**Good `fix` examples:**
- ✅ `"Run migration: dotnet ef database update"`
- ✅ `"Set 'EMAIL_VERIFIED' flag via UserService.VerifyEmail()"`
- ✅ `"Add BillingAddress in CustomerService.Create()"`

---

## File-Level Review

After reviewing all assertions in a file:

### 4. Consistency

- [ ] All assertions in the file use `Assert.That.*`
- [ ] Context quality is consistent across all assertions
- [ ] Similar assertions have similar context patterns

### 5. Completeness

- [ ] No classic assertions remain (`Assert.IsTrue`, `Assert.AreEqual`, etc.)
- [ ] No TODO comments left for context parameters
- [ ] All assertions are properly formatted and readable

---

## Test Quality Standards

### 6. Test Structure

- [ ] Follows Arrange-Act-Assert pattern
- [ ] Has clear test method names (e.g., `Should_Create_User_When_Valid_Input`)
- [ ] Each test focuses on a single behavior

### 7. Assertion Count

- [ ] Tests have appropriate number of assertions
  - Single behavior → 1-3 assertions typical
  - Complex workflows → More assertions acceptable
- [ ] Multiple assertions have clear logical grouping
- [ ] Each assertion has distinct, meaningful context

---

## Common Patterns to Check

### 8. Boolean Assertions

- [ ] `Assert.IsTrue()` → `Assert.That.IsTrue()`
- [ ] `Assert.IsFalse()` → `Assert.That.IsFalse()`
- [ ] Complex conditions broken into multiple specific assertions when possible

### 9. Null Checks

- [ ] `Assert.IsNull()` → `Assert.That.IsNull()`
- [ ] `Assert.IsNotNull()` → `Assert.That.IsNotNull()`

### 10. Equality Checks

- [ ] `Assert.AreEqual()` → `Assert.That.AreEqual()`
- [ ] `Assert.AreNotEqual()` → `Assert.That.AreNotEqual()`
- [ ] Reference equality uses `AreSame()` / `AreNotSame()`

### 11. Collections

- [ ] `Assert.IsTrue(collection.Count > 0)` → `Assert.That.IsNotEmpty(collection)`
- [ ] `Assert.AreEqual(0, collection.Count)` → `Assert.That.IsEmpty(collection)`
- [ ] `Assert.IsTrue(collection.Contains(...))` → `Assert.That.Contains(collection, ...)`
- [ ] `Assert.IsTrue(collection.All(...))` → `Assert.That.AllMatch(collection, ...)`

### 12. Numeric Checks

- [ ] `Assert.IsTrue(value > threshold)` → `Assert.That.IsGreaterThan(value, threshold)`
- [ ] `Assert.IsTrue(value < threshold)` → `Assert.That.IsLessThan(value, threshold)`
- [ ] `Assert.IsTrue(value > 0)` → `Assert.That.IsPositive(value)`
- [ ] Range checks → `Assert.That.IsInRange(value, min, max)`

### 13. String Checks

- [ ] `Assert.IsTrue(str.Length == 0)` → `Assert.That.IsEmpty(str)`
- [ ] `Assert.IsTrue(str.Contains(...))` → `Assert.That.Contains(str, ...)`
- [ ] `Assert.IsTrue(str.StartsWith(...))` → `Assert.That.StartsWith(str, ...)`
- [ ] `Assert.IsTrue(Regex.IsMatch(...))` → `Assert.That.Matches(str, pattern)`

### 14. Exception Handling

- [ ] `Assert.ThrowsException<T>()` → `Assert.That.Throws<T>()`
- [ ] Try-catch with `Assert.Fail()` → `Assert.That.DoesNotThrow()`

### 15. DateTime/DateTimeOffset

- [ ] `Assert.IsTrue(dt.Kind == DateTimeKind.Utc)` → `Assert.That.IsUtc(dt)`
- [ ] Timestamp comparisons → `Assert.That.IsAfter()` / `IsBefore()`
- [ ] Approximate time checks → `Assert.That.IsCloseTo(dt, expected, tolerance)`

---

## Code Review Comments

When reviewing PRs, leave comments for:

### Issues to Address

- [ ] Classic assertions without migration plan
- [ ] Missing `because` or `fix` parameters
- [ ] Generic or unhelpful context messages
- [ ] Wrong assertion method (e.g., `IsTrue()` when `IsPositive()` is better)

### Example Comment Templates

**For classic assertions:**
```
Please upgrade to `Assert.That.*` with context:

Assert.That.IsTrue(user.IsActive,
    because: "Active users should have full system access",
    fix: "Activate the user via UserService.Activate(userId)");
```

**For missing context:**
```
Please add meaningful `because` and `fix` parameters:
- `because`: Why does this assertion matter? (business/technical reason)
- `fix`: How to resolve if this fails? (specific debugging steps)
```

**For wrong assertion method:**
```
Consider using a more specific assertion:
`Assert.That.IsPositive(price, because:, fix:)` 
instead of 
`Assert.That.IsTrue(price > 0, because:, fix:)`
```

---

## Post-Review Actions

### 16. Verification

- [ ] Run all tests: `dotnet test`
- [ ] Verify all tests pass
- [ ] Check test output for improved failure messages

### 17. Documentation

- [ ] Update test documentation if needed
- [ ] Add examples to team wiki or docs
- [ ] Share learnings with the team

### 18. Prevention

- [ ] Consider adding analyzer rules to block classic `Assert.*`
- [ ] Add assertion quality to code review checklist
- [ ] Include `/check-asserts` in CI pipeline (optional)

---

## Metrics to Track

Track these metrics over time to measure assertion quality improvement:

- [ ] **Classic assertion count**: Should trend toward 0
- [ ] **Assert.That.* adoption rate**: Should trend toward 100%
- [ ] **Context quality score**: Manual review of `because` and `fix` quality
- [ ] **Test failure resolution time**: Should decrease with better context

---

## Common Anti-Patterns

Watch for these during review:

### Anti-Pattern 1: Context Repeats Assertion
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

### Anti-Pattern 2: Vague Fix Instructions
❌ **Bad:**
```csharp
fix: "Check the database"
```

✅ **Good:**
```csharp
fix: "Verify the user exists in database via: SELECT * FROM Users WHERE Id = '{userId}'"
```

### Anti-Pattern 3: Using Generic Assertions
❌ **Bad:**
```csharp
Assert.That.IsTrue(count > 0,
    because: "Collection should have items",
    fix: "Add items to collection");
```

✅ **Good:**
```csharp
Assert.That.IsNotEmpty(collection,
    because: "Collection should have items",
    fix: "Add items to collection");
```

### Anti-Pattern 4: Missing Business Context
❌ **Bad:**
```csharp
because: "The value must match"
```

✅ **Good:**
```csharp
because: "Customer ID must match to ensure billing records are correctly linked"
```

---

## Tooling

- Run `/check-asserts` to analyze assertion quality
- Use IDE search/replace for bulk migrations (carefully!)
- Consider Roslyn analyzers for enforcement
- Add pre-commit hooks to prevent classic assertions

---

## Reference

- Package: [AspNetCore.Simple.MsTest.Sdk](https://www.nuget.org/packages/AspNetCore.Simple.MsTest.Sdk)
- Skill: `/check-asserts`
- Pattern Guide: [PATTERN.md](PATTERN.md)
- Examples: [EXAMPLES.md](EXAMPLES.md)

---

## Summary

A quality assertion should:

✅ Use `Assert.That.*` with the most specific method  
✅ Have meaningful `because` explaining business/technical reason  
✅ Have actionable `fix` providing specific resolution steps  
✅ Help humans and AI understand failures instantly  
✅ Make tests self-documenting and easier to maintain
