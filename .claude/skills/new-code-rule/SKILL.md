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

- Id: next free `BASTA_RULE_<NNN>` (look at `src/Basta.CodeRules/**/BASTA_RULE_*.cs`).
- Folder by level: `01_Solution`, `02_Projects`, `03_C#_Files`, or `04_Custom` for rules spanning levels.
- File name and class name = rule id. One rule per file.
- Categories: `[TestCategory("CodeRules")]` plus one specific, e.g. `[TestCategory("CodeRules Projects")]`.
- Method name reads as the rule: `Productive_Projects_Are_Not_Allowed_To_Reference_Any_Test_Project`.

### 4. Write the rule - template

Collect **all** findings as `CodeRuleFinding`, then assert once with `Assert.That.CodeRuleHasNoFindings`.
The rule only states *what* it checks - layout, sorting, documentation link and Human/Ai output come from
`src/Basta.CodeRules/Findings/`. Never build the failure message as a string.

```csharp
using System.Collections.Immutable;
using Extensions.Pack;

namespace Basta.CodeRules.<Category>
{
    [TestCategory("CodeRules")]
    [TestCategory("CodeRules <Category>")]
    [TestClass]
    public class BASTA_RULE_NNN : CodeRuleTestBase
    {
        [TestMethod]
        public void Rule_Reads_Like_A_Sentence()
        {
            var findings = from tree in ProductiveSyntaxTrees
                           from type in tree.AllTypes()
                           where /* violation */
                           select new CodeRuleFinding(Subject: type.FullQualifiedName,
                                                      Location: type.Location,        // CodeLocation or FileInfo (csproj)
                                                      Current: type.SyntaxTree,       // optional: offending code
                                                      Suggested: "...",               // optional: corrected code
                                                      Fix: "...",                     // optional: overrides the rule fix
                                                      Details: null);                 // optional: extra "Key : value" lines

            Assert.That.CodeRuleHasNoFindings(findings,
                                              rule: "BASTA_RULE_NNN",
                                              title: "<Rule in one sentence>",
                                              because: "<why this rule exists>",
                                              fix: "<concrete change>");
        }
    }
}
```

Finding checklist:

- [ ] **what**: `Subject` names the offending element (type, member, reference, package)
- [ ] **where**: `Location` - pass `declaration.Location` or the csproj `FileInfo`; paths become repository relative
- [ ] **why**: `because:` - one or two sentences, once per rule
- [ ] **fix**: `fix:` for the rule, `Fix:` on the finding when it depends on the finding; `Current` / `Suggested` for a before/after line
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
   `dotnet test src/Basta.CodeRules --filter "FullyQualifiedName~BASTA_RULE_NNN"`
2. If it is red on the current code: the code is wrong, not the rule. Report the findings to the user;
   do not weaken the rule or add exceptions without explicit approval (see AGENTS.md).
3. **Negative check:** temporarily introduce one violation (e.g. add a forbidden `<ProjectReference>`,
   a mutable property), run the rule, confirm the finding reads well, then **restore the file** and
   confirm with `git status` / `git diff` that nothing is left behind.
4. Run the whole category: `dotnet test src/Basta.CodeRules --filter "TestCategory=CodeRules"`.

### 7. Report

Tell the user: rule id and file, what it checks, green on the current code (or the findings), how the
negative check looked (paste the finding), and any known blind spots (e.g. projects outside the slnx).
