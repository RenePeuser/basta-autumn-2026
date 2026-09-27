# Solution.Parser - API cheat sheet for CodeRules

Namespaces: `Solution.Parser.Sln`, `Solution.Parser.Project`, `Solution.Parser.CSharp`.
Helpers like `IsEmpty()`, `IsFalse()`, `Flatten(separator)`, `DoesNotContain(...)` come from `Extensions.Pack`.

In `src/Basta.CodeRules` watch out for name clashes: `CSharpSyntaxTree` and `Enum` exist in Roslyn / System
too - `CodeRuleTestBase.cs` aliases them.

## Loading (already done in `CodeRuleTestBase`)

```csharp
var solutionFile = new SolutionFileName("code-rules.slnx")
    .FindSolutionFileReverseFrom(new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory));
SolutionFile solution = solutionFile.Parse();

CSharpSyntaxTree tree = project.SourceFiles.GetCSharpFiles().First().Parse();
```

## Solution level - `SolutionFile`

| Member | Type |
|---|---|
| `SolutionFileInfo` | the slnx/sln found |
| `Projects` | `ImmutableList<ProjectFile>` all projects of the solution |
| `ProductiveProjects` | projects not classified as test |
| `UnitTestProjects` | projects with `<IsTestProject>true` or a test package (`MSTest.*`, `Microsoft.NET.Test.Sdk`, `NUnit`, `XUnit`) in the csproj |

## Project level - `ProjectFile`

| Member | Type / meaning |
|---|---|
| `ProjectFileInfo.Value` | `FileInfo` of the .csproj (`.FullName`, `.DirectoryName`, `.Name`) |
| `AssemblyName` | `string` |
| `Document` | `XDocument` - raw csproj, for anything not modelled (`Document.Root.Descendants("PropertyGroup")` ...) |
| `ProjectReferences` | `ImmutableList<ProjectReference>`: `Include` (raw relative path, `\`), `Name` (file name without extension if no `<Name>`), `CopyLocal` |
| `PackageReferences` | `ImmutableList<PackageReference>`: `Include`, `PackageVersion` |
| `AssemblyReferences` | `ImmutableList<AssemblyReference>`: `Include`, `Name`, `HintPath`, `SpecificVersion` |
| `ProjectTypes` | `ImmutableList<ProjectType>` (`ProjectType.Test`, `ProjectType.C_Sharp`, ...) |
| `TargetFrameworkVersion` | `ImmutableList<string>` |
| `SourceFiles` | `ImmutableList<FileInfo>` - plain files; `.GetCSharpFiles()` for C# |
| `ContentItems`, `Imports`, `Packages`, `BuildDependencies`, `DocumentationFile`, `Guid` | as named |

Resolve a project reference to its csproj:

```csharp
var include = reference.Include.Replace('\\', Path.DirectorySeparatorChar);
var referencedPath = Path.GetFullPath(Path.Combine(project.ProjectFileInfo.Value.DirectoryName!, include));
var referencedProject = Solution.Projects.FirstOrDefault(p => string.Equals(p.ProjectFileInfo.Value.FullName, referencedPath, StringComparison.OrdinalIgnoreCase));
```

## C# file level - `CSharpSyntaxTree`

| Member | Type / meaning |
|---|---|
| `FileName`, `NameSpace.Name`, `NameSpaces` | file and namespace(s) |
| `Usings` | `Using`: `Value`, `Alias`, `IsGlobal`, `IsStatic`, `Location` |
| `Types` | file-level types only; `Classes`, `Records`, `Interfaces`, `Enums`, `Structs` filter it |
| `AllTypes()` / `AllMethods()` | include nested types; `IEnumerable<CSharpSyntaxTree>.AllTypes()` for many trees |
| `AssemblyAttributes`, `Statements` (top-level), `Delegates` | as named |
| `Diagnostics`, `HasParseErrors` | Roslyn parse diagnostics |
| `SyntaxTree` | full source text |

### Every declaration (`DeclarationBase`)

`Name`, `FullQualifiedName`, `SyntaxTree` (source of the declaration), `FilePath`,
`Location` (`CodeLocation`, `ToString()` = `path(line,column)`), `LineCount`.

### With modifiers (`DeclarationWithModifiers`: types, methods, properties, fields, ctors ...)

`Modifiers` (`Modifier.Public|Internal|Protected|Private|Static|ReadOnly|Const|Abstract|Partial|Required|Sealed|Virtual|Override|New|Async|...`),
`Accessibility` (effective, incl. the default when nothing is written: `Private`, `Internal`, `Public`, `Protected`, `ProtectedOrInternal`, `ProtectedAndInternal`),
`Attributes`, `Documentation`.

Predicates: `IsPublic()`, `IsInternal()`, `IsPrivate()`, `IsStatic()`, `IsSealed()`, `IsAbstract()`,
`IsPartial()`, `IsVirtual()`, `IsOverride()`, `HasAttribute("Obsolete")` (matches `Obsolete`, `ObsoleteAttribute`, fully qualified).

### Types (`TypeDeclaration` -> `Class`, `Record`, `Struct`, `Interface`, `Enum`)

`Kind` (`TypeKind`), `NameSpace`, `BaseTypes` (`BaseType.TypeName`), `TypeParameters`, `IsGeneric`,
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
| `Attribute` | `Name`, `Arguments`, `PositionalArguments`, `NamedArguments` (`Name`, `Value`), `Target`, `IsNamed(name)`; list: `HasAttribute(name)`, `Named(name)` |

### Formatting for findings

`declaration.FormatSyntaxTree()` / `"code".FormatSyntaxTree()` - normalized source, good for a "Sample:" block:

```csharp
{record.SyntaxTree.Replace("set;", "init;").FormatSyntaxTree()}
```

## Model gaps

The model is syntactic, not semantic: type names are strings as written (`"List<string>"`, not a symbol),
there is no call graph and no resolved type information. For "who calls X" or "which type does this
resolve to" fall back to Roslyn directly (see `src/CodeRules.HowTo/03_C#_Files/Roslyn_Syntax_Tree_Test.cs`)
or match on text in `Body` / `SyntaxTree` and accept the false-positive risk consciously.

## Existing rules as reference

- `src/Basta.CodeRules/04_Custom/BASTA_RULE_001.cs` - C# level: mutable record properties, with before/after sample
- `src/Basta.CodeRules/04_Custom/BASTA_RULE_002.cs` - project level: productive projects must not reference test projects
