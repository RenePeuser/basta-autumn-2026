; Unshipped analyzer release
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID          | Category | Severity | Notes
-----------------|----------|----------|-------
BASTA_ASYNC_0001 | Async.Design | Error | Methods returning Task/ValueTask must be named with an 'Async' suffix
BASTA_ASYNC_0002 | Async.Design | Warning | Do not call Task.WaitAll
