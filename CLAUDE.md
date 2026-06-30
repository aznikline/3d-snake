# NEON SERPENT — Claude Code Game Studios

3D First-Person Low-Poly Snake Game. Built with Unity 6000.4.8f1 (Unity 6), Universal Render Pipeline.

## Technology Stack

- **Engine**: Unity 6000.4.8f1 (Unity 6, ARM64 macOS)
- **Language**: C# (.NET Standard 2.1)
- **Render Pipeline**: URP (Universal Render Pipeline)
- **UI**: Unity UI (uGUI) + TextMesh Pro
- **Input**: Unity Input System
- **Version Control**: Git with trunk-based development
- **Build System**: Unity Build Pipeline (custom `BuildPipeline.cs`)
- **Asset Pipeline**: Procedural — zero external art/audio assets

## Project Architecture

```
Assets/
├── Scripts/
│   ├── Core/           # GameState, GameConstants, GameBootstrap
│   ├── Player/         # SnakeHeadController, VerletSnakeBody, Abilities
│   ├── Gameplay/       # ComboSystem, DeathManager, Food, FoodSpawner
│   ├── Level/          # LevelManager, LevelData, Scoring, Mode Managers
│   ├── Procedural/     # City generation, synth music, SFX, particles
│   ├── UI/             # HUD, Menus, Tutorial, Settings controllers
│   ├── Progression/    # SaveSystem, UnlockSystem
│   ├── Steam/          # Steamworks integration (stubbed)
│   ├── Audio/          # MusicManager
│   ├── Platform/       # BuildConfig
│   └── Editor/         # LevelEditorWindow, BuildPipeline
├── Settings/           # URP settings, Input Actions
├── Scenes/             # Boot.unity, Gameplay.unity
├── LevelData/          # Campaign level ScriptableObjects
├── Materials/          # Snake, Environment, Procedural, UI materials
├── Shaders/            # NeonEmission.shadergraph
├── Resources/          # Runtime-loadable assets
└── Tests/
    ├── EditMode/       # Editor tests (LevelScoring, ProceduralGenerator)
    └── PlayMode/       # Runtime tests (GameplayRule, VerletSnakeBody)
```

## Engine Version Reference

@.claude/docs/engine-reference (configure with `/setup-engine unity 2022.3`)

## Namespaces

- `NeonSerpent.Core` — GameState, GameConstants, GameBootstrap
- `NeonSerpent.Player` — SnakeHeadController, VerletSnakeBody, CameraController, Abilities
- `NeonSerpent.Gameplay` — ComboSystem, DeathManager, Food, FoodSpawner
- `NeonSerpent.Level` — LevelManager, LevelData, LevelScoring, Mode Managers
- `NeonSerpent.UI` — HUDController, MainMenuController, PauseMenuController, etc.
- `NeonSerpent.Progression` — SaveSystem, UnlockSystem
- `NeonSerpent.Steam` — SteamManager, AchievementManager, LeaderboardManager
- `NeonSerpent.Procedural.Art.Environment` — ProceduralCityGenerator, BuildingGenerator, NeonSignGenerator
- `NeonSerpent.Procedural.Art.Effects` — NeonFlickerEffect, ProceduralParticleEffects
- `NeonSerpent.Procedural.Art.UI` — ProceduralUIRenderer
- `NeonSerpent.Procedural.Audio` — ProceduralMusicSystem, ProceduralSFXSystem
- `NeonSerpent.Audio` — MusicManager
- `NeonSerpent.Platform` — BuildConfig

## Key Design Decisions

- **Zero external assets** — all art, audio, and UI generated procedurally at runtime
- **Runtime bootstrap** — `GameBootstrap` creates all game systems via code (no prefab dependencies)
- **Event-driven architecture** — systems communicate via C# events, not polling
- **Verlet physics** — custom snake body simulation (not Unity Physics) for 200+ node support
- **Immutability** — prefer creating new objects over mutating existing ones

## Coding Standards

@.claude/docs/coding-standards.md

Additional Unity/C# conventions:
- Prefer composition over MonoBehaviour inheritance
- Cache component references in `Awake()`, never call `GetComponent<>()` in `Update()`
- Use `ScriptableObject` for data-driven content (LevelData, ChallengeRule)
- Separate data from behavior
- Assembly definitions (`.asmdef`) for all code folders

## Studio Hierarchy

@.claude/docs/agent-roster.md
@.claude/docs/agent-coordination-map.md
@.claude/docs/coordination-rules.md

## Collaboration Protocol

**User-driven collaboration, not autonomous execution.**
Every task follows: **Question -> Options -> Decision -> Draft -> Approval**

- Propose architecture before implementing — show thinking, explain trade-offs
- Get approval before writing files
- Flag deviations from design docs explicitly
- No commits without user instruction

## Context Management

@.claude/docs/context-management.md

## Skills Reference

@.claude/docs/skills-reference.md
@.claude/docs/workflow-catalog.yaml
