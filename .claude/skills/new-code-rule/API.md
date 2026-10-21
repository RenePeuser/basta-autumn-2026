# Solution.Parser - API cheat sheet for CodeRules

Namespaces: `Solution.Parser.Sln`, `Solution.Parser.Project`, `Solution.Parser.CSharp`.
Helpers like `IsEmpty()`, `IsFalse()`, `Flatten(separator)`, `DoesNotContain(...)` come from `Extensions.Pack`.

In `src/Basta.CodeRules` watch out for name clashes: `CSharpSyntaxTree` and `Enum` exist in Roslyn / System
too - `CodeRuleTestBase.cs` aliases them.

## Loading (already done in `CodeRuleTestBase`)

```csharp
SolutionFile solution = SolutionFile.Find("code-rules.slnx");   // searched upwards from the test output

CSharpSyntaxTree tree = CSharpSyntaxTree.Parse(project.SourceFiles.GetCSharpFiles().First());
```

## Solution level - `SolutionFile`

| Member | Type |
|---|---|
| `FileInfo`, `Name`, `Path` | the slnx/sln found |
| `Projects` | `ImmutableList<ProjectFile>` all projects of the solution |
| `ProductionProjects` | projects not classified as test |
| `Project("Name")` | a project by name |
| `UnitTestProjects` | projects with `<IsTestProject>true` or a test package (`MSTest.*`, `Microsoft.NET.Test.Sdk`, `NUnit`, `XUnit`) in the csproj |

## Project level - `ProjectFile`

| Member | Type / meaning |
|---|---|
| `FileInfo`, `Name`, `Path` | the .csproj (`FileInfo` is a plain `System.IO.FileInfo`) |
| `AssemblyName` | `string` |
| `Document` | `XDocument` - raw csproj, for anything not modelled (`Document.Root.Descendants("PropertyGroup")` ...) |
| `ProjectReferences` | `ImmutableList<ProjectReference>`: `Include` (raw relative path, `\`), `Name` (file name without extension if no `<Name>`), `CopyLocal` |
| `PackageReferences` | `ImmutableList<PackageReference>`: `Include`, `Version` |
| `AssemblyReferences` | `ImmutableList<AssemblyReference>`: `Include`, `Name`, `HintPath`, `SpecificVersion` |
| `ProjectTypes` | `ImmutableList<ProjectType>` (`ProjectType.Test`, `ProjectType.C_Sharp`, ...) |
| `TargetFrameworkVersion` | `ImmutableList<string>` |
| `SourceFiles` | `ImmutableList<FileInfo>` - plain files; `.GetCSharpFiles()` for C# |
| `ContentItems`, `Imports`, `Packages`, `BuildDependencies`, `DocumentationFile`, `Guid` | as named |

Resolve a project reference to its csproj:

```csharp
var include = reference.Include.Replace('\\', Path.DirectorySeparatorChar);
var referencedPath = Path.GetFullPath(Path.Combine(project.FileInfo.DirectoryName!, include));
var referencedProject = Solution.Projects.FirstOrDefault(p => string.Equals(p.Path, referencedPath, StringComparison.OrdinalIgnoreCase));
```

## C# file level - `CSharpSyntaxTree`

| Member | Type / meaning |
|---|---|
| `FileName`, `Namespace.Name`, `Namespaces` | file and namespace(s) |
| `Usings` | `Using`: `Name`, `Alias`, `IsGlobal`, `IsStatic`, `Location` |
| `Types` | file-level types only; `Classes`, `Records`, `Interfaces`, `Enums`, `Structs` filter it |
| `AllTypes()` / `AllMethods()` | include nested types; `IEnumerable<CSharpSyntaxTree>.AllTypes()` for many trees |
| `AssemblyAttributes`, `Statements` (top-level), `Delegates` | as named |
| `Diagnostics`, `HasParseErrors` | Roslyn parse diagnostics |
| `SourceCode` | full source text |

### Every declaration (`DeclarationBase`)

`Name`, `FullyQualifiedName`, `SourceCode` (source of the declaration), `FilePath`,
`Location` (`CodeLocation`, `ToString()` = `path(line,column)`), `LineCount`.

### With modifiers (`DeclarationWithModifiers`: types, methods, properties, fields, ctors ...)

`Modifiers` (`Modifier.Public|Internal|Protected|Private|Static|ReadOnly|Const|Abstract|Partial|Required|Sealed|Virtual|Override|New|Async|...`),
`Accessibility` (effective, incl. the default when nothing is written: `Private`, `Internal`, `Public`, `Protected`, `ProtectedOrInternal`, `ProtectedAndInternal`),
`Attributes`, `Documentation`.

Predicates: `IsPublic()`, `IsInternal()`, `IsPrivate()`, `IsStatic()`, `IsSealed()`, `IsAbstract()`,
`IsPartial()`, `IsVirtual()`, `IsOverride()`, `HasAttribute("Obsolete")` (matches `Obsolete`, `ObsoleteAttribute`, fully qualified).

### Types (`TypeDeclaration` -> `Class`, `Record`, `Struct`, `Interface`, `Enum`)

`Kind` (`TypeKind`), `Namespace`, `BaseTypes` (`BaseType.TypeName`), `TypeParameters`, `IsGeneric`,
`Constructors`, `PrimaryConstructor`, `Parameters` (primary ctor), `Properties`, `Methods`, `Fields`,
`Events`, `EventFields`, `Indexers`, `Operators`, `Delegates`, `Finalizers`,
`NestedTypes` (+ `NestedClasses`, `NestedRecords`, ...), `DescendantTypes()`, `AllMethods()`,
`Implements("IDisposable")`, `InheritsFrom("ControllerBase")`.

- `Record.IsRecordStruct`
- `Enum.EnumFields`, `UnderlyingType`, `IsFlags`

### Members

| Model | Useful members |
|---|---|
| `Property` | `Type`, `IsReadOnly` (no `set`), `IsInitOnly`, `HasGetter`, `HasSetter`, `IsRequired`, `IsNullable`, `IsAutoProperty`, `IsExpressionBodied`, `Initializer`, `Accessors` |
| `Method` | `ReturnParameter`, `Parameters`, `TypeParameters`, `Body`, `Statements`, `LineStatements`, `LocalFunctions`, `IsAsync`, `HasBody`, `IsExpressionBodied`, `IsIterator`, `UseCorrectAsyncNaming()` |
| `Constructor` | as method + `IsPrimary`, `IsStatic`, `InitializerKind`, `Arguments` |
| `Field` | `Type`, `IsReadOnly`, `IsConst`, `IsStatic`, `IsNullable`, `Initializer` |
| `Parameter` | `Type`, `Modifiers`, `Attributes`, `IsOptional`, `DefaultValue`, `IsParams`, `IsNullable`, `Ordinal` |
| `Attribute` | `Name`, `Arguments`, `PositionalArguments`, `NamedArguments` (`Name`, `Value`), `Target`, `IsNamed(name)`; list: `HasAttribute(name)`, `Named(name)` (all matches); `declaration.Attribute(name)` for one |

### Formatting for findings

`declaration.FormatSyntaxTree()` / `"code".FormatSyntaxTree()` - normalized source, good for a "Sample:" block:

```csharp
{record.SourceCode.Replace("set;", "init;").FormatSyntaxTree()}
```

## Model gaps

The model is syntactic, not semantic: type names are strings as written (`"List<string>"`, not a symbol),
there is no call graph and no resolved type information. For "who calls X" or "which type does this
resolve to" fall back to Roslyn directly (see `src/CodeRules.HowTo/03_C#_Files/Roslyn_Syntax_Tree_Test.cs`)
or match on text in `Body` / `SourceCode` and accept the false-positive risk consciously.

## Existing rules as reference

- `src/Basta.CodeRules/Records/RECORD_RULE_001.cs` - C# level: mutable record properties, with before/after sample
- `src/Basta.CodeRules/ProjectReferences/PROJECTS_RULE_001.cs` - project level: productive projects must not reference test projects
