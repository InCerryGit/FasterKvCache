# Agent Guide (FasterKv.Cache)

This repo is a multi-targeted .NET library (C#), built with `dotnet` and tested with xUnit.

## Quick Commands

- Build solution (Release):
  - `dotnet build -c Release FasterKv.Cache.sln`
- Build a single project:
  - `dotnet build -c Release src/FasterKv.Cache.Core/FasterKv.Cache.Core.csproj`
- Run all tests (default frameworks in test project):
  - `dotnet test tests/FasterKv.Cache.Core.Tests/FasterKv.Cache.Core.Tests.csproj`
- Run tests for a specific framework (CI uses net6.0..net10.0):
  - `dotnet test --framework net8.0 tests/FasterKv.Cache.Core.Tests/FasterKv.Cache.Core.Tests.csproj`

## Run A Single Test / Subset

xUnit runs via `dotnet test` using the VSTest filter syntax.

- Run a single test by fully-qualified name (recommended):
  - `dotnet test tests/FasterKv.Cache.Core.Tests/FasterKv.Cache.Core.Tests.csproj --filter "FullyQualifiedName=FasterKv.Cache.Core.Tests.Serializers.FasterKvSerializerSerializeTests.Expired_Value_Should_Only_Serialize_ExpiryTime"`
- Run all tests in a class:
  - `dotnet test tests/FasterKv.Cache.Core.Tests/FasterKv.Cache.Core.Tests.csproj --filter "FullyQualifiedName~FasterKv.Cache.Core.Tests.Serializers.FasterKvSerializerSerializeTests"`
- Run by method name substring:
  - `dotnet test tests/FasterKv.Cache.Core.Tests/FasterKv.Cache.Core.Tests.csproj --filter "Name~Serialize"`
- Run by trait (only if tests define traits):
  - `dotnet test ... --filter "Trait=Category=Slow"`

Tip: add `--framework net8.0` to any command when you want faster local iteration.

## Lint / Formatting

This repo does not currently define a dedicated linter/formatter command (no `.editorconfig`, `dotnet format` config, or `stylecop` config checked in).

If you want to format locally, use standard tooling (do not enforce via PR unless the repo adds it):
- `dotnet format FasterKv.Cache.sln` (only if your environment has `dotnet-format` installed)

## CI Expectations (from GitHub Actions)

- Build: `dotnet build --configuration Release FasterKv.Cache.sln`
- Tests (Linux matrix):
  - `dotnet test --framework=<net6.0|net7.0|net8.0|net9.0|net10.0> tests/FasterKv.Cache.Core.Tests/FasterKv.Cache.Core.Tests.csproj`

## NuGet Packaging Policy

This repo uses a "deny by default" pack strategy.

- Default: `IsPackable=false` via `Directory.Build.props`
- Libraries: explicitly opt-in with `IsPackable=true` in the `src/` project `.csproj`
- Non-shipping projects (tests/benchmark/sample/tools): keep `IsPackable=false`
- Release workflows should push all produced `*.nupkg` and rely on `IsPackable` (avoid filename-based skip lists)

## Project Defaults (from `Directory.Build.props`)

- C# language version: 11 (`<LangVersion>11</LangVersion>`)
- Nullable reference types: enabled (`<Nullable>enable</Nullable>`)
- Warnings as errors: enabled (`<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`)
- Test assemblies get `InternalsVisibleTo $(AssemblyName).Tests`

## Code Style Guidelines (C#)

### File Layout

- Prefer file-scoped namespaces:
  - `namespace FasterKv.Cache.Core.Tests.Serializers;`
- One public type per file (exceptions: small internal helper types).
- Keep files small and focused; avoid "god" classes.

### Using Directives / Imports

- Keep `using` directives at the top; avoid mid-file `using`.
- Sort usings: `System.*` first, then third-party, then project namespaces.
- Prefer `global using` for ubiquitous test-only imports (already used: `global using Xunit;` in `tests/FasterKv.Cache.Core.Tests/Usings.cs`).
- Avoid unused usings; treat warnings as errors will break the build.

### Naming

- Namespaces: `FasterKv.Cache.<Area>`.
- Types/methods/properties: `PascalCase`.
- Local variables/parameters: `camelCase`.
- Private fields: `_camelCase`.
- Interfaces: `IThing`.
- Async methods: suffix `Async`.
- Tests:
  - Keep descriptive names; existing tests often use underscores for readability.

### Types, Nullability, and `var`

- Keep nullability correct; prefer non-nullable by default.
- Use `ArgumentNullException.ThrowIfNull(x)` for required parameters.
- Use `var` when the RHS makes the type obvious; otherwise use explicit types.
- Prefer `readonly` for fields and locals when possible.

### Formatting

- Follow standard C# conventions:
  - 4-space indentation.
  - Braces on the next line for types/members.
  - Keep line length reasonable; wrap fluent chains.
- Prefer expression-bodied members only when it improves readability.

### Error Handling

- Do not swallow exceptions silently.
- Throw framework exceptions for argument validation:
  - `ArgumentException`, `ArgumentOutOfRangeException`, `InvalidOperationException`.
- When surfacing errors from the underlying FASTER engine, include enough context
  (cache name, key info, operation) but avoid logging/throwing sensitive payloads.
- Prefer "Try" APIs (`TryGet...`, `TryRecover...`) when failure is a normal outcome.

### Performance & Allocation Awareness

This library targets high performance.

- Avoid unnecessary allocations on hot paths:
  - Prefer spans/`ReadOnlySpan<T>` / pooled buffers where appropriate.
  - Minimize boxing (especially for the non-generic cache API).
- Dispose resources deterministically:
  - Use `using var ...;` for `Stream`, `IDisposable` objects.
- Be careful with async/await overhead in tight loops.

### Tests (xUnit + Moq)

- Use `[Fact]` for single-case tests; `[Theory]` + `[InlineData]` for parameterized tests.
- Prefer behavior-focused tests with clear Arrange/Act/Assert separation.
- When mocking:
  - Keep setups minimal.
  - Verify important interactions; avoid over-specification.

## Repository-Specific Rules (Cursor/Copilot)

- Cursor rules: none found (no `.cursor/rules/` and no `.cursorrules`).
- Copilot instructions: none found (no `.github/copilot-instructions.md`).

## Common Gotchas

- Multi-targeting: ensure changes compile across `net6.0`, `net7.0`, `net8.0`, `net9.0`, `net10.0` (and `netstandard2.0` for core library).
- Warnings are errors: even minor warnings will fail CI.
- When adding packages, consider framework-specific references (see existing per-TFM package references).
