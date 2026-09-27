---
name: new-code-rule
description: Build a new CodeRule (MSTest) in src/Basta.CodeRules with the Solution.Parser API - solution, project and C# rules with findings that name what, where, why and how to fix
---

# New CodeRule - with Solution.Parser

**Purpose:** Turn a piece of project knowledge ("productive projects must not reference test projects",
"record properties must be immutable", ...) into an executable CodeRule that fails with a finding a
human or an agent can act on without guessing.

**Package:** `Solution.Parser` (version in `Directory.Packages.props`). API cheat sheet: [API.md](API.md).

**When to use:**
- The user describes a convention, architecture boundary or "we always/never do X" rule
- A review comment keeps coming back and should become a test
- An existing rule in `src/Basta.CodeRules` has to be extended

---

## Workflow

### 1. Pin down the rule

Before writing code, state in one sentence each:

- **What** is forbidden or required
- **Scope**: productive code, test code or both; solution, project file or C# file level
- **Why** it matters (goes into the finding)
- **Fix**: what the developer has to change (goes into the finding)

If the scope or the fix is unclear, ask the user. Do not guess a rule.

### 2. Pick the level and the data source

All data is loaded once in `CodeRuleTestBase` (`[AssemblyInitialize]`). Use it, do not parse again.

| Level | Rule is about | Start from |
|---|---|---|
| Solution | which projects exist, naming, test vs productive | `Solution.Projects`, `Solution.ProductiveProjects`, `Solution.UnitTestProjects` |
| Project | csproj content: references, packages, properties | `project.ProjectReferences`, `project.PackageReferences`, `project.Document` (XDocument) |
| C# file | types, members, attributes, usings, namespaces | `ProductiveSyntaxTrees`, `TestSyntaxTrees`, `AllSyntaxTrees`, `Records`, `Classes`, `Enums`, `Interfaces`, `Structs` |
| Test data | request/response snapshot JSON | `RequestsJsonFiles`, `ResponseJsonFiles` |

Only reach for raw Roslyn when the Solution.Parser model does not carry what you need
(see "Model gaps" in [API.md](API.md)). If you add something to `CodeRuleTestBase`, keep it lazy or cheap.

### 3. Create the file

CodeRules live only in `src/Basta.CodeRules/`. `src/CodeRules.HowTo` holds examples, not rules.

- Id: `<TOPIC>_RULE_<NNN>`, e.g. `RECORD_RULE_001`, `PROJECTS_RULE_001`. Reuse the topic of an existing
  rule when it fits and take the next free number (look at `src/Basta.CodeRules/**/*_RULE_*.cs`);
  otherwise start a new topic with `_001`.
- Folder: one per topic, named in plain words, e.g. `Records/`, `ProjectReferences/`. The folder becomes the
  category in the output.
- File name and class name = rule id. One rule per file.
- Namespace: `Basta.CodeRules`, no sub namespace per folder.
- Categories: `[TestCategory("CodeRules")]` plus one specific, e.g. `[TestCategory("CodeRules Records")]`.
- Method name reads as the rule: `Productive_Projects_Are_Not_Allowed_To_Reference_Any_Test_Project`.

### 4. Write the rule - template

Collect **all** findings as `CodeRuleFinding`, then assert once with `Assert.That.CodeRuleHasNoFindings`.
The rule only states *what* it checks - layout, sorting, documentation link and Human/Ai output come from
`src/Basta.CodeRules/Findings/`. Never build the failure message as a string.

Build every finding with an object initializer only - no constructor arguments, no mix of both.
`Subject` is `required`, the compiler rejects a finding without it. All other properties are optional.

#### 4a. C# declaration

For anything derived from `Solution.Parser.CSharp.DeclarationBase` (type, record, property, method, field, ...)
take `Subject`, `Location` and `Current` from the declaration, then add what carries information:
`Suggested`, `Fix`, `DocumentationUrl`, `Details`.

```csharp
using System.Collections.Immutable;
using Extensions.Pack;

namespace Basta.CodeRules
{
    [TestCategory("CodeRules")]
    [TestCategory("CodeRules Records")]
    [TestClass]
    public class RECORD_RULE_001 : CodeRuleTestBase
    {
        [TestMethod]
        public void Record_Properties_Have_To_Be_Be_Immutable()
        {
            var findings = from record in Records
                           from property in record.Properties
                           where property.IsReadOnly.IsFalse()
                           select new CodeRuleFinding
                           {
                               Subject = property.FullQualifiedName,
                               Location = property.Location,
                               Current = property.SyntaxTree,
                               Suggested = property.SyntaxTree.Replace("set;", "init;"),
                               Details = ImmutableDictionary<string, string>.Empty.Add("Property", $"{property.Type} {property.Name}")
                           };

            Assert.That.CodeRuleHasNoFindings(findings,
                                              rule: "RECORD_RULE_001",
                                              title: "Record properties must be immutable",
                                              because: "Records are value objects. Mutable properties break value equality and make instances unsafe to share.",
                                              fix: "Replace 'set;' with 'init;'.");
        }
    }
}
```

#### 4b. No declaration - project specific checks

For csproj, slnx, packages or snapshot JSON there is no declaration. Same initializer, `Location` accepts a
`CodeLocation` or a `FileInfo` (e.g. the csproj):

```csharp
select new CodeRuleFinding
{
    Subject = $"{project.AssemblyName} → {Path.GetFileNameWithoutExtension(include)}",   // required
    Location = projectFile,                                                             // FileInfo of the csproj
    Current = $"<ProjectReference Include=\"{reference.Include}\" />",
    Fix = $"Remove the <ProjectReference> from '{projectFile.Name}'."                    // optional: overrides the rule fix
};
```

Assert the same way with `Assert.That.CodeRuleHasNoFindings(...)` (see `ProjectReferences/PROJECTS_RULE_001.cs`).

If a project specific check yields no list of findings but just one fact (e.g. "the solution contains
exactly one Web API project", "`global.json` pins the SDK"), you may use a plain assert from
`AspNetCore.Simple.MsTest.Sdk` instead, always with `because:` and `fix:`:

```csharp
Assert.That.IsTrue(globalJson.Exists,
                   because: "The SDK version must be pinned so every machine and CI builds with the same SDK.",
                   fix: "Add a global.json with 'rollForward: disable' to the repository root.");
```

As soon as a check can hit more than one element, collect `CodeRuleFinding`s instead.

Finding checklist:

- [ ] **shape**: object initializer only, `Subject` always set (it is `required`)
- [ ] **what**: `Subject` names the offending element (type, member, reference, package)
- [ ] **where**: `Location` - pass `declaration.Location` or the csproj `FileInfo`; paths become repository relative
- [ ] **why**: `because:` - one or two sentences, once per rule
- [ ] **fix**: `fix:` for the rule, `Fix` on the finding when it depends on the finding; `Current` / `Suggested` for a before/after line
- [ ] **docs** (optional): `DocumentationUrl` on the finding for further reading about exactly this problem (API docs, pattern); the rule documentation link is added by the assert
- [ ] rule id, category (= folder), documentation link and rule source are added by the assert

Output mode: `CodeRuleSettings__OutputMode=ai` (or `TestSdkSettings__OutputMode=ai`) renders JSON for agents,
default is the framed Human output. Fixtures can catch `CodeRuleViolationException` and check `.Violation.Findings`.

### 5. Pitfalls

- **Paths:** never absolute. `ProjectReference.Include` is the raw relative path from the csproj with `\`.
  Resolve it with `Path.GetFullPath(Path.Combine(projectDir, include.Replace('\\', Path.DirectorySeparatorChar)))`
  and compare case-insensitively.
- **Test detection:** a project counts as test when its csproj contains `<IsTestProject>true` or references
  `MSTest.*`, `Microsoft.NET.Test.Sdk`, `NUnit`, `XUnit`. `IsTestProject` set only in `Directory.Build.props` is not seen.
- **Solution scope:** only projects listed in `code-rules.slnx` are parsed. Projects outside the slnx are invisible.
- **Nesting:** `tree.Types` / `tree.Records` are file-level only. Use `tree.AllTypes()` / `type.DescendantTypes()`
  for nested types, `AllMethods()` for all methods.
- **Generated code:** exclude `obj/`, `bin/`, `*.g.cs` if the query walks files itself.
- **Parse errors:** a file with `tree.HasParseErrors` yields an incomplete model; do not trust a green rule on it.
- **Determinism:** no time, no network, no ordering assumptions. Sort findings if the output order matters.

### 6. Verify - green AND red

1. Run only the new rule:
   `dotnet test src/Basta.CodeRules --filter "FullyQualifiedName~RECORD_RULE_001"`
2. If it is red on the current code: the code is wrong, not the rule. Report the findings to the user;
   do not weaken the rule or add exceptions without explicit approval (see AGENTS.md).
3. **Negative check:** temporarily introduce one violation (e.g. add a forbidden `<ProjectReference>`,
   a mutable property), run the rule, confirm the finding reads well, then **restore the file** and
   confirm with `git status` / `git diff` that nothing is left behind.
4. Run the whole category: `dotnet test src/Basta.CodeRules --filter "TestCategory=CodeRules"`.

### 7. Report

Tell the user: rule id and file, what it checks, green on the current code (or the findings), how the
negative check looked (paste the finding), and any known blind spots (e.g. projects outside the slnx).
