---
name: csharp-style
description: C# coding conventions for this project, matching Rider's default code style (codified in the repo's /.editorconfig since no custom Rider style config exists). Use whenever writing or editing any .cs file in this repo — Assets/Scripts, Editor scripts, anywhere.
---

# C# style (Rider defaults)

This project has no custom ReSharper/Rider `.DotSettings` and no prior `.editorconfig`, so
"Rider's convention" means Rider's out-of-the-box C# style. That's now codified in
`/.editorconfig` at the repo root — Rider, `dotnet format`, and any CI will read it automatically.
Follow it when writing or editing C#:

## Braces & layout
- **Allman style**: opening brace always on its own new line — types, methods, properties,
  `if`/`for`/`while`/`using`/`try`, everything.
- Always use braces, even for single-statement bodies.
- 4-space indent, no tabs.
- `using` directives outside the namespace, `System.*` first, alphabetical within each group.
- Namespace blocks (`namespace X { ... }`), not file-scoped `namespace X;` — matches the rest
  of the codebase.

## Naming
- Types (`class`/`struct`/`enum`/`delegate`): `PascalCase`.
- Interfaces: `IPascalCase`.
- Type parameters: `TPascalCase`.
- Methods, properties, events: `PascalCase`.
- **Fields**: every field is `camelCase`, public or private, on any type — a plain runtime class,
  a `MonoBehaviour`, a `ScriptableObject`, a struct, all the same rule.
  - Public/internal/protected fields: bare `camelCase`, no prefix (`public string characterId;`,
    `public readonly CharacterState state;`).
  - Private fields marked `[SerializeField]` (Inspector-visible): also bare `camelCase`, no
    underscore (`[SerializeField] private string animationName;`).
  - Private fields that are NOT serialized (ordinary internal state): `_camelCase`, leading
    underscore (`private readonly ComboTracker _combo;`, `private bool _isBusy;`). This is the
    one case that keeps the underscore — the deciding factor is "is this Inspector-visible /
    externally-set data," not "is this a MonoBehaviour."
- Constants: `PascalCase`.
- Parameters and locals: `camelCase`.

This was revised mid-project — it supersedes any earlier PascalCase-public-field guidance you
might recall. Canonical examples: `CharacterDefinition.characterId`/`.basicAttacks` (public,
no underscore), `SpineAnimationState`'s `[SerializeField] private string animationName;` (no
underscore because serialized), `CombatParticipant`'s `private readonly Dictionary<...>
_liveStateByAssetReference;` (underscore because not serialized) — while `GetStatsAtLevel(...)`,
`SceneRoot` (a property) stay PascalCase regardless.

Acronym casing: treat a multi-letter acronym as one word when it appears in an identifier —
`SO` → `So`, `ID` → `Id` (e.g. `CharacterDatabaseSo`, not `CharacterDatabaseSO`). This can't be
expressed in `.editorconfig`'s naming rules, so it's not machine-enforced — just match it by hand.

## Member style
- Use `var` when the right-hand side makes the type obvious (`var stat = new AttackStat(5);`);
  spell out the type otherwise (e.g. an interface-typed result, a numeric literal without a
  clarifying cast).
- Prefer expression-bodied members for one-line properties/accessors
  (`public string DisplayName => "HP";`); use a block body once there's more than one statement.
- One type per file, file named after the type — small tightly-coupled types (e.g. a family of
  enum-like subclasses) can share a file when a single file is clearly more readable, but default
  to splitting.

## Comments
- Keep comments short: one line (at most two) saying what something is for or a non-obvious
  constraint. No multi-paragraph headers, no restating what the code does, no design essays.
- Skip comments on self-explanatory members (simple fields, events, one-line methods).
- Wrap comment lines at ~80 columns.
- This applies to new code even when nearby older code has long comments — don't copy their length.

## What NOT to do
- Don't add a custom brace style, tabs, or non-Rider naming (e.g. `m_field`, `s_field`) —
  those are conventions from *other* projects/packages in this workspace
  (`Packages/com.unity.pipeline` uses `m_`/`s_` for its own code); they don't apply here.
- Don't hand-format against these rules "because it reads better" — consistency with
  `.editorconfig` beats individual preference, since Rider will otherwise re-flag or
  auto-reformat on save.
