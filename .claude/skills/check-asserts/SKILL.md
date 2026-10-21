---
name: check-asserts
description: Review and upgrade test assertions to use Assert.That.* with mandatory context
---

# Check Asserts - Test Assertion Quality Review

**Purpose:** Identify classic MSTest assertions and upgrade them to modern `Assert.That.*` assertions with mandatory `because` and `fix` parameters for better debuggability.

**Package:** [AspNetCore.Simple.MsTest.Sdk](https://www.nuget.org/packages/AspNetCore.Simple.MsTest.Sdk)

**When to use:**
- Reviewing existing test code
- Migrating legacy test projects to modern assertion style
- Ensuring test quality standards before PR merge
- Auditing test code for AI-debuggable assertions

**Output:** Analysis report + optional fixes applied

---

## What This Skill Does

1. **Scans test files** for classic MSTest assertions:
   - `Assert.IsTrue()`, `Assert.IsFalse()`
   - `Assert.AreEqual()`, `Assert.AreNotEqual()`
   - `Assert.IsNull()`, `Assert.IsNotNull()`
   - `Assert.IsInstanceOfType()`
   - `Assert.ThrowsException()`
   - And all other `Assert.*` methods except `Assert.That.*`

2. **Identifies missing context** in existing `Assert.That.*` calls:
   - Missing `because:` parameter
   - Missing `fix:` parameter
   - Generic or unhelpful messages

3. **Suggests modern alternatives** with:
   - Correct `Assert.That.*` method
   - Meaningful `because:` context
   - Actionable `fix:` guidance

4. **Optionally applies fixes** to upgrade assertions automatically

---

## Why Assert.That.* ?

Traditional assertions give you a line number and a brief message. `Assert.That.*` gives you structured failure context that makes debugging instant for humans and enables AI tools to understand test failures without guessing.

### Comparison

**Classic MSTest (Avoid):**
```csharp
Assert.IsTrue(user.IsActive);  // ❌ No context, unclear why this matters
```

**Modern Assert.That (Prefer):**
```csharp
Assert.That.IsTrue(user.IsActive,
    because: "Active users should have full system access",
    fix: "Activate the user account in the admin panel");  // ✅ Clear context and resolution
```

---

## Algorithm

### 1. Discovery Phase
```bash
# Find all test files
find . -path "*.Test/*.cs" -o -path "*.Tests/*.cs"

# Search for classic assertions
grep -E "Assert\.(IsTrue|IsFalse|AreEqual|IsNull|ThrowsException)" **/*.cs
```

### 2. Analysis Phase
For each assertion found:
- Classify as "Classic" or "Modern incomplete"
- Identify assertion type (boolean, equality, null, exception, etc.)
- Check for `because:` and `fix:` parameters
- Determine appropriate `Assert.That.*` replacement

### 3. Reporting Phase
Generate categorized report:
- **Critical:** Classic assertions without context
- **Warning:** `Assert.That.*` missing `because` or `fix`
- **Info:** Suggestions for more specific assertion methods

### 4. Fix Phase (Optional)
If user approves:
- Replace classic assertions with `Assert.That.*` equivalents
- Add placeholder `because` and `fix` comments for manual refinement
- Preserve original assertion logic

---

## Detection Patterns

### Classic Assertions to Replace

| Classic | Modern Replacement | Category |
|---------|-------------------|----------|
| `Assert.IsTrue(condition)` | `Assert.That.IsTrue(condition, because:, fix:)` | Boolean |
| `Assert.IsFalse(condition)` | `Assert.That.IsFalse(condition, because:, fix:)` | Boolean |
| `Assert.AreEqual(expected, actual)` | `Assert.That.AreEqual(expected, actual, because:, fix:)` | Equality |
| `Assert.AreNotEqual(expected, actual)` | `Assert.That.AreNotEqual(expected, actual, because:, fix:)` | Equality |
| `Assert.IsNull(value)` | `Assert.That.IsNull(value, because:, fix:)` | Null |
| `Assert.IsNotNull(value)` | `Assert.That.IsNotNull(value, because:, fix:)` | Null |
| `Assert.IsInstanceOfType(obj, type)` | `Assert.That.IsInstanceOfType<T>(obj, because:, fix:)` | Type |
| `Assert.ThrowsException<T>(action)` | `Assert.That.Throws<T>(action, because:, fix:)` | Exception |

### Context Quality Patterns

**❌ Poor context:**
```csharp
because: "It should be true"  // Just repeats the assertion
fix: "Fix it"                  // Not actionable
```

**✅ Good context:**
```csharp
because: "Active users require verified email addresses for security compliance"
fix: "Send verification email via EmailService.SendVerification(user.Email)"
```

---

## Example Output Report

```markdown
# Assert Quality Report
Generated: 2026-07-02
Scanned: 145 test files, 2,847 assertions

## Summary
- ❌ Classic assertions: 342 (needs upgrade)
- ⚠️  Missing context: 128 (incomplete)
- ✅ Proper Assert.That: 2,377

## Critical Issues (Classic Assertions)

### File: UserServiceTests.cs

**Line 42:**
```csharp
Assert.IsTrue(result.IsSuccess);
```

**Issue:** Classic assertion without context  
**Suggested Fix:**
```csharp
Assert.That.IsTrue(result.IsSuccess,
    because: "TODO: Why is success expected here?",
    fix: "TODO: How to resolve if this fails?");
```

---

### File: ProductRepositoryTests.cs

**Line 87:**
```csharp
Assert.AreEqual(expectedId, product.Id);
```

**Issue:** Classic equality assertion  
**Suggested Fix:**
```csharp
Assert.That.AreEqual(expectedId, product.Id,
    because: "TODO: Why must IDs match?",
    fix: "TODO: How to correct ID mismatch?");
```

---

## Warnings (Incomplete Assert.That)

### File: OrderProcessingTests.cs

**Line 156:**
```csharp
Assert.That.IsNotNull(order.Customer,
    because: "It should not be null",  // ⚠️  Not helpful
    fix: "Check the value");            // ⚠️  Not actionable
```

**Issue:** Generic context messages  
**Suggested Improvement:**
```csharp
Assert.That.IsNotNull(order.Customer,
    because: "Orders must have an associated customer for billing",
    fix: "Ensure Customer is loaded via Include(o => o.Customer) in the query");
```

---

## Recommendations

1. **Immediate Actions:**
   - Replace 342 classic assertions with `Assert.That.*`
   - Add meaningful `because` and `fix` to 128 incomplete assertions

2. **Quality Guidelines:**
   - `because:` should explain business/technical reason for the assertion
   - `fix:` should provide specific debugging steps or resolution actions
   - Avoid generic phrases like "should be true", "fix it", "check the value"

3. **Prevention:**
   - Add code analyzer rule to block classic `Assert.*` (except `Assert.That`)
   - Include assertion quality in code review checklist
```

---

## How to Run

### Review Mode (Default)
```bash
/check-asserts
```
Generates report without modifying code.

### Review + Fix Mode
```bash
/check-asserts --apply-fixes
```
Generates report and applies automatic fixes (with placeholder context).

### Specific File/Directory
```bash
/check-asserts --path src/Basta.WebApi.Test
```

---

## Safety Features

- ✅ Non-destructive by default (review-only mode)
- ✅ Preserves original assertion logic
- ✅ Adds TODO comments for manual context refinement
- ✅ Git-reversible (easy rollback with `git checkout`)
- ✅ Dry-run report before applying fixes

---

## Integration with Code Review

Add to `.github/pull_request_template.md`:

```markdown
## Test Quality Checklist
- [ ] All assertions use `Assert.That.*` (not classic `Assert.*`)
- [ ] Every assertion has meaningful `because:` parameter
- [ ] Every assertion has actionable `fix:` parameter
- [ ] Run `/check-asserts` before submitting PR
```

---

## Related Skills

- `/code-review` - Comprehensive code quality review

---

## Notes

- This skill enforces test quality standards from `AspNetCore.Simple.MsTest.Sdk`
- Classic assertions are not wrong, but lack structured debugging context
- AI tools and future debugging tooling benefit from structured assertion failures
- Meaningful context makes test failures actionable without deep code investigation
