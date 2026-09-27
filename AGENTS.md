# AGENTS.md

Companion repository for the talk "CodeRules & Analyzer – Self-Defending Codebases" (BASTA! Autumn 2026).
The goal: project knowledge lives in executable CodeRules. When a rule fails, the finding tells you what, where, why and how to fix it.

Status: only a few example CodeRules are published (`src/Basta.CodeRules`); `src/CodeRules.HowTo` shows the techniques. Use the `/new-code-rule` skill to add a rule.

## Commands

```bash
dotnet build code-rules.slnx
dotnet test code-rules.slnx

# CodeRules only
dotnet test src/Basta.CodeRules --filter TestCategory=CodeRules

# HowTo examples only
dotnet test src/CodeRules.HowTo --filter TestCategory=HowTo

# strict CI mode (all analyzers, warnings as errors, NuGet audit)
dotnet build code-rules.slnx -p:CI=true
```

The SDK version is pinned in `global.json` (`rollForward: disable`).

## Layout

| Path | Purpose |
|---|---|
| `src/Basta.CodeRules` | CodeRules as MSTest tests, one rule per file, shared setup in `CodeRuleTestBase` |
| `src/CodeRules.HowTo` | Minimal examples: MSBuild, Reflection, Roslyn, custom abstraction |
| `src/Basta.WebApi` | Sample Minimal API the rules run against |
| `src/Basta.WebApi.Test` | Snapshot-based API tests (`Requests/` + `Responses/` JSON). |
| `src/Basta.Analyzer`, `src/Basta.CodeFixes` | Roslyn analyzer and code fixes (POC) |
| `src/Basta.Analyzer.Test`, `src/Basta.CodeFixes.Test` | Specs for analyzer and code fixes, see their `AGENTS.md`. Not part of `code-rules.slnx` yet. |

## Working with CodeRules

- A failing CodeRule means the code is wrong, not the rule. Fix the code.
- Never weaken a rule, add whitelist or exception entries, or add `[Ignore]` without explicit approval from a human. Every exception needs a reason in a comment.
- Re-run the failing rule to verify the fix before running the full suite.
- Set `CodeRuleSettings__OutputMode=ai` to get findings as JSON instead of the framed Human output (`TestSdkSettings__OutputMode=ai` works too and also switches the API tests).

## Writing a new CodeRule

- One rule per file in `src/Basta.CodeRules/<Category>/`, named after its rule id.
- Add `[TestCategory("CodeRules")]` plus a category, e.g. `[TestCategory("CodeRules Projects")]`.
- The failure message must name the rule id, the location, the reason and the fix.
- Rules must be deterministic and fast, with few false positives, and have positive and negative fixtures.
- Never use absolute paths. Resolve files relative to the repository root (see `src/CodeRules.HowTo/RepositoryPaths.cs`).

## Conventions

- NuGet versions live only in `Directory.Packages.props`. No `Version` attribute in project files.
- Shared build settings live in `Directory.Build.props`. Do not repeat them in project files.
- Test assertions use `Assert.That.*` with `because:` and `fix:` (package `AspNetCore.Simple.MsTest.Sdk`). The `/check-asserts` skill reviews this.
