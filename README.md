# CodeRules & Analyzer – Self-Defending Codebases

![Eine Codebase im Karate-Gi wehrt fehlerhafte Changes eines Entwicklers und eines AI Agents ab (AI-generiertes Bild)](self-defending-code-base.jpg)

> Für Menschen und AI Agents.
> Begleit-Repository zum Vortrag auf der **BASTA! Autumn 2026** von Rene Peuser.

Ein AI Agent kennt eure ungeschriebenen Regeln genauso wenig wie ein neuer Entwickler – er erzeugt Code nur wesentlich schneller.
Dieses Repository zeigt, wie aus einer einmal entdeckten Fehlerquelle eine **ausführbare Regel** wird, die jeden neuen Verstoß deterministisch erkennt.

## Was ist eine CodeRule?

Eine CodeRule überprüft eine projektspezifische Erwartung automatisch. Ein gutes Finding beantwortet:

| Frage | Beispiel |
|---|---|
| **Was** ist falsch? | `Example.Contracts` referenziert `Example.WebApi` |
| **Wo** liegt der Verstoß? | `src/Example.Contracts/Example.Contracts.csproj` |
| **Warum** existiert die Regel? | Contracts müssen unabhängig vom Host bleiben |
| **Wie** wird er behoben? | Referenz entfernen oder Typ nach Contracts verschieben |
| **Wie** wird der Fix validiert? | `dotnet test --filter CodeRule=ARCH-004` |

## Wo Regeln heute leben – und warum das nicht reicht

| Ort | Typisches Problem |
|---|---|
| Köpfe | Wissen verschwindet mit Personen |
| Dokumentation | Veraltet, schwer auffindbar, nicht ausführbar |
| Code Review | Spätes Feedback, wiederholte Kommentare |
| IDE-Einstellungen | Nicht verbindlich für Team, CI und Agents |
| Prompts & Skills | Kontext muss wiederholt und vollständig geliefert werden |

CodeRules liegen versioniert in der Codebase, laufen lokal und in der CI und liefern bei gleichem Input immer dasselbe Ergebnis.

## Eine Engine, zwei Ausführungsformen

| Kriterium | CodeRule als Test | CodeRule als Analyzer |
|---|---|---|
| Neue Regel | In Minuten | Höherer API- und Testaufwand |
| Zugriff | Solution, Dateien, MSBuild, Parser | C#-/VB-Semantikmodell |
| Feedback | Testlauf oder CI | Direkt in IDE und Build |
| Code Fix | – | Optional automatisch |

Eine Regel muss nicht als Analyzer geboren werden:

1. **Beobachtung** – eine Fehlerquelle zeigt den Bedarf
2. **Lokaler Test** – die Erwartung wird schnell und deterministisch abgesichert
3. **Gemeinsame Abstraktion** – wiederkehrender Boilerplate wandert in die Engine
4. **Rule Pack** – eine allgemeine Regel gilt unverändert in mehreren Projekten
5. **Analyzer** – sofortiges IDE-Feedback rechtfertigt den Mehraufwand

## Der einfachste zuverlässige Mechanismus

| Bedarf | Mechanismus |
|---|---|
| Typen und Attribute | Reflection-Test |
| Projekt- und Paketregeln | MSBuild- oder Solution-Test |
| C#-Semantik beim Schreiben | Roslyn Analyzer |
| Automatische Reparatur | Analyzer mit Code Fix |
| XAML, SQL, JSON, Markdown | Parser + CodeRule-Test |
| Runtime-Verhalten einer API | API- oder Integrationstest |

## Repository-Struktur

```
src/
├── CodeRules.HowTo/        Einstieg: MSBuild, Reflection, Roslyn und eine eigene Abstraktion
├── Basta.CodeRules/        CodeRules als Tests (erste Beispielregeln)
├── Basta.Analyzer/         Roslyn Analyzer (POC)
├── Basta.CodeFixes/        Code Fixes zu den Analyzern (POC)
├── Basta.WebApi/           Beispiel-API (Minimal API), gegen die die Regeln laufen
└── Basta.WebApi.Test/      Snapshot-basierte API-Tests
```

> **Status:** Veröffentlicht sind bisher zwei Beispielregeln: Produktivprojekte dürfen keine Testprojekte referenzieren (`PROJEDCTS_RULE_001`), und Record-Properties müssen immutable sein (`RECORD_RULE_001`).
> Die zugrunde liegenden Techniken zeigen die Beispiele in `CodeRules.HowTo`.

## Voraussetzungen

- [.NET SDK 10.0.401](https://dotnet.microsoft.com/download) (fixiert über `global.json`)
- Visual Studio 2026, JetBrains Rider oder VS Code mit C# Dev Kit
- [Git LFS](https://git-lfs.com) für Folien und Bilder (`git lfs install` vor dem Klonen)

## Loslegen

```bash
dotnet build code-rules.slnx

# alle Tests
dotnet test code-rules.slnx

# nur die CodeRules
dotnet test src/Basta.CodeRules --filter TestCategory=CodeRules

# nur die HowTo-Beispiele
dotnet test src/CodeRules.HowTo --filter TestCategory=HowTo
```

### Demo-Modus vs. CI-Modus

`Directory.Build.props` ist lokal auf Geschwindigkeit optimiert (Analyzer aus, Warnings nicht blockierend).
Mit `CI=true` greift der strikte Modus: alle .NET-Analyzer, Code-Style im Build, NuGet-Audit und `TreatWarningsAsErrors`.

```bash
dotnet build code-rules.slnx -p:CI=true
```

## CodeRules vs. AI

| AI / LLM | CodeRules |
|---|---|
| Findet neue, unbekannte Probleme | Überwacht bekannte Erwartungen |
| Benötigt ausreichend Kontext | Regel liegt versioniert in der Codebase |
| Ergebnisse können variieren | Gleicher Input, gleiches Ergebnis |
| Kosten pro Lauf (Tokens / Rechenleistung) | Nach der Implementierung günstig |
| Stark bei Analyse und Reparatur | Stark bei verbindlicher Absicherung |

Entwickler und AI Agents ändern den Code. CodeRules prüfen deterministisch. Beide lesen das Finding, korrigieren und führen dieselben Regeln erneut aus.

## Einführung in bestehende Codebases

1. **Baseline** – bestehende Verstöße erfassen
2. **Neue Verstöße** – neue Fehler zuerst blockieren
3. **Schweregrad** – Analyzer von Hinweis über Warning zu Error entwickeln
4. **Ausnahmen** – begründen und mit Ablaufdatum versehen
5. **Pflege** – Nutzen, Laufzeit und False Positives regelmäßig prüfen

## AI Agents

[`AGENTS.md`](AGENTS.md) beschreibt Befehle, Struktur und Regeln für AI Agents (Codex, Copilot, Cursor u. a.).
[`CLAUDE.md`](CLAUDE.md) importiert sie für Claude Code. Die Testprojekte von Analyzer und CodeFixes haben eigene `AGENTS.md` mit ihren Spezifikationsregeln.

## Folien

Die Präsentation liegt als [`Basta_Autumn_2026.pptx`](Basta_Autumn_2026.pptx) im Repository.

## Autor

**Rene Peuser** – Independent Software Developer, C# / .NET / ASP.NET Core

- GitHub: [RenePeuser](https://github.com/RenePeuser)
- LinkedIn: [rene-peuser](https://linkedin.com/in/rene-peuser)
- NuGet: [Rene.Peuser](https://www.nuget.org/profiles/Rene.Peuser)

## Lizenz

[MIT](LICENSE)
