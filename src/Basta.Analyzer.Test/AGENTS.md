# AI Instructions – Analyzer Tests

This project defines the SPECIFICATION of analyzer behavior.
Tests are the single source of truth.

## Core Principles

- ONE SCENARIO = ONE TEST FILE
- ONE DIAGNOSTIC RULE = ONE TEST FOLDER

## Test Folder Structure (MANDATORY)

Analyzer tests MUST be organized by DiagnosticId.

Structure:
<Domain>/<DiagnosticId>_<RuleName>/<Scenario>.cs

Example:
MinimalApi/BASTA_MAPI_00020_ProducesResponseDto/MapGet_TaskOfDto_NoProduces.cs

Rules:
- One DiagnosticId per folder
- One scenario per test file
- Scenario file name describes behavior
- Test folder MUST map 1:1 to exactly one DiagnosticId

## Mandatory Rules

- One scenario per test file
- One TestClass per scenario
- One TestMethod per scenario
- File name MUST describe the scenario behavior

Example:
MapGet_TaskOfDto_NoProduces.cs

## Scenario Rules

Each scenario MUST clearly express:
- Endpoint type (MapGet / MapPost / etc.)
- Handler return type
- Relevant conditions
- Expected diagnostic behavior

Scenarios MUST cover:
- Valid cases
- Invalid cases
- Edge cases
- Real-world variants

## Test Design Rules

- Prefer explicit tests over parameterized tests
- Tests must be readable without jumping between files
- Source code should be minimal but realistic
- No shared helpers that hide intent

## Required Coverage Dimensions

Scenarios MUST vary across:
- Endpoint method (MapGet, MapPost, MapPut, ...)
- Handler shape (lambda, method group, local function)
- Return type (Task<T>, ValueTask<T>, IResult, Task)
- Fluent chain shape (inline, multiline, assigned variable)
- Presence / absence of required extensions
- Aliases and implicit typing where relevant

## Forbidden

- Monster test classes
- Grouping multiple scenarios in one file
- Multiple DiagnosticIds in one test folder
- Snapshot-style mega tests
- Implicit expectations

## Regression Rule

Every fixed bug MUST result in:
- A new dedicated scenario test file
- Located in the folder of the affected DiagnosticId
- With a descriptive file name explaining the bug
